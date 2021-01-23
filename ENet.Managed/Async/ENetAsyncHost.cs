using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using ENet.Managed.Checksums;
using ENet.Managed.Compressors;
using ENet.Managed.Internal;
using ENet.Managed.Internal.Threading;
using ENet.Managed.Native;

namespace ENet.Managed.Async
{
    /// <summary>
    /// An asynchronous version of <see cref="ENetHost"/>.
    /// </summary>
    public sealed class ENetAsyncHost : IDisposable
    {
        private const int SERVICE_TIMEOUT_MS = 5;

        private static readonly UnboundedChannelOptions s_AcceptChannelOptions = new()
        {
            AllowSynchronousContinuations = false, // Important!
            SingleWriter = true,
            SingleReader = false,
        };

        internal readonly SharedStateLock<ENetAsyncSharedState> SharedStateLock;

        private readonly Thread m_ServiceThread;
        private readonly TaskCompletionSource<bool> m_StopCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Channel<ENetAsyncPeer> m_AcceptChannel;
        private volatile bool m_Stop = false;

        /// <summary>
        /// Constructs a <see cref="ENetAsyncHost"/> with given parameters.
        /// </summary>
        /// <param name="address">The address at which other peers may connect to this host. If null, then no peers may connect to the host.</param>
        /// <param name="peers">Maximum number of peers</param>
        /// <param name="channels">Maximum number of channels (0 = <see cref="ENetHost.MaximumPeers"/>)</param>
        /// <param name="incomingBandwidth">Maximum incoming bandwidth (0 = unlimited)</param>
        /// <param name="outgoingBandwidth">Maximum outgoing bandwidth (0 = unlimited)</param>
        public ENetAsyncHost(IPEndPoint? address, int peers, byte channels, long incomingBandwidth = 0, long outgoingBandwidth = 0)
        {
            var host = new ENetHost(address, peers, channels, incomingBandwidth, outgoingBandwidth);

            var sharedState = new ENetAsyncSharedState(host);
            SharedStateLock = new SharedStateLock<ENetAsyncSharedState>(sharedState);
            m_ServiceThread = new Thread(ServiceThreadEntry);
            m_ServiceThread.IsBackground = true;
            m_ServiceThread.Priority = ThreadPriority.BelowNormal;
            m_AcceptChannel = Channel.CreateUnbounded<ENetAsyncPeer>(s_AcceptChannelOptions);
        }

        /// <summary>
        /// Starts the (async) host underlaying thread.
        /// </summary>
        /// <returns></returns>
        public async ValueTask StartAsync()
        {
            using (var lockgaurd = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                if (lockgaurd.SharedState.Started)
                    return;

                lockgaurd.SharedState.Started = true;
                m_ServiceThread.Start();
            }
        }

        /// <summary>
        /// Signals the stop of the (async) host to the underlaying thread.
        /// </summary>
        public void BeginStop()
        {
            m_Stop = true;
        }

        /// <summary>
        /// Waits for <see cref="ENetEventType.Connect"/> to occur and returns the peer.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to watch.</param>
        /// <returns>A peer that remotely connected.</returns>
        public async ValueTask<ENetAsyncPeer> AcceptAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await m_AcceptChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ChannelClosedException ex)
            {
                throw new ENetAsyncHostStoppedException("Async Host has stopped.", ex);
            }
        }

        /// <summary>
        /// Connects to the specified <see cref="IPEndPoint"/>
        /// </summary>
        /// <param name="endPoint">Remote endpoint</param>
        /// <param name="channels">Maximum number of channels (can't be zero)</param>
        /// <param name="data">User data supplied to the receiving host</param>
        /// <returns>The connecting peer</returns>
        public async ValueTask<ENetAsyncPeer> ConnectAsync(IPEndPoint endPoint, byte channels, uint data)
        {
            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                var peer = sharedState.Host.Connect(endPoint, channels, data);
                var asyncPeer = new ENetAsyncPeer(this, peer, endPoint, data);
                asyncPeer.ConnectCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                sharedState.AsyncPeers.Add(asyncPeer);
                return asyncPeer;
            }
        }

        /// <summary>
        /// Queues a packet to be sent to all peers associated with the host.
        /// </summary>
        /// <remarks>
        /// This method will destroy the packet if its <see cref="ENetPacket.ReferenceCount"/> is zero
        /// </remarks>
        public async ValueTask BroadcastAsync(byte channel, ENetPacket packet)
        {
            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.Broadcast(channel, packet);
            }
        }

        /// <summary>
        /// Adjusts the bandwidth limits of the host.
        /// </summary>
        public void BandwidthLimit(long incomingBandwidth, long outgoingBandwidth)
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.BandwidthLimit(incomingBandwidth, outgoingBandwidth);
            }
        }

        /// <summary>
        /// Limits the maximum allowed channels of future incoming connections.
        /// </summary>
        public void ChannelLimit(byte channels)
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.ChannelLimit(channels);
            }
        }

        /// <summary>
        /// Sends any queued packets on the host specified to its designated peers.
        /// </summary>
        public async ValueTask FlushAsync()
        {
            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.Flush();
            }
        }

        /// <summary>
        /// Enables compression with given custom compressor
        /// </summary>
        public void CompressWith(ENetCompressor compressor)
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.CompressWith(compressor);
            }
        }

        /// <summary>
        /// Enables compression using ENet's builtin range-coder compression
        /// </summary>
        public void CompressWithRangeCoder()
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.CompressWithRangeCoder();
            }
        }

        /// <summary>
        /// Disables compression
        /// </summary>
        public void DisableCompression()
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.DisableCompression();
            }
        }

        /// <summary>
        /// Enables checksum with given custom checksum method
        /// </summary>
        public void ChecksumWith(ENetChecksum checksum)
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.ChecksumWith(checksum);
            }
        }

        /// <summary>
        /// Enables ENet's builtin CRC32 checksum
        /// </summary>
        public void ChecksumWithCRC32()
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.ChecksumWithCRC32();
            }
        }

        /// <summary>
        /// Disables checksum
        /// </summary>
        public void DisableChecksum()
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.DisableChecksum();
            }
        }

        /// <summary>
        /// Intercepts host events with given callback
        /// </summary>
        /// <param name="callback">Callback which intercepts the events</param>
        public void InterceptWith(ENetInterceptCallback callback)
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.InterceptWith(callback);
            }
        }

        /// <summary>
        /// Disables interception
        /// </summary>
        public void DisableInterception()
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                sharedState.Host.DisableInterception();
            }
        }

        /// <summary>
        /// [Not Thread Safe] Get underlying host.
        /// </summary>
        /// <remarks>
        /// Becarful when using this method; the Async Host should not be running.
        /// </remarks>
        /// <returns>The underlaying host.</returns>
        public ENetHost DangerousGetUnderlayingHost()
        {
            using (var lockguard = SharedStateLock.Acquire())
            {
                return lockguard.SharedState.Host;
            }
        }

        /// <summary>
        /// Gets number of (async) peers. (whether connected or connecting)
        /// </summary>
        /// <returns>The peers count</returns>
        public async ValueTask<int> GetPeersCountAsync()
        {
            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                return sharedState.AsyncPeers.Count;
            }
        }

        /// <summary>
        /// Gets a copy array of (async) peers. (whether connected or connecting)
        /// </summary>
        /// <returns>An array of Async Peers.</returns>
        public async ValueTask<ENetAsyncPeer[]> GetPeersAsync()
        {
            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                return sharedState.AsyncPeers.ToArray();
            }
        }

        /// <summary>
        /// Gets (async) peers by storing the them into a buffer.
        /// </summary>
        /// <param name="peersBuffer">The buffer to store (async) peers to.</param>
        /// <returns></returns>
        public async ValueTask GetPeersAsync(ICollection<ENetAsyncPeer> peersBuffer)
        {
            ThrowHelper.ThrowIfArgumentNull(peersBuffer, nameof(peersBuffer));

            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                foreach (var asyncPeer in sharedState.AsyncPeers)
                {
                    peersBuffer.Add(asyncPeer);
                }
            }
        }

        /// <summary>
        /// Gets a snapshot of the current (async) host statistics.
        /// </summary>
        /// <returns>The statistics.</returns>
        public async ValueTask<ENetAsyncStatistics> GetStatisticsAsync()
        {
            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;

                sharedState.ThrowIfStopped();

                return new ENetAsyncStatistics()
                {
                    ConnectedPeers = sharedState.Host.ConnectedPeers,
                    DuplicatePeers = sharedState.Host.DuplicatePeers,
                    PeersCount = sharedState.Host.PeersCount,
                    TotalReceivedData = sharedState.Host.TotalReceivedData,
                    TotalReceivedPackets = sharedState.Host.TotalReceivedPackets,
                    TotalSentData = sharedState.Host.TotalSentData,
                    TotalSentPackets = sharedState.Host.TotalSentPackets,
                };
            }
        }

        public async Task WaitStopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (cancellationToken == default)
            {
                await m_StopCompletionSource.Task.ConfigureAwait(false);
                return;
            }

            var cancelCompletionSource = new TaskCompletionSource<bool>();
            cancellationToken.Register(() =>
            {
                cancelCompletionSource.SetResult(true);
            });

            var cancelTask = cancelCompletionSource.Task;
            if (await Task.WhenAny(m_StopCompletionSource.Task, cancelTask).ConfigureAwait(false) == cancelTask)
                throw new OperationCanceledException();
        }

        /// <summary>
        /// Signals stop of the host and waits for the stop procedure to complete.
        /// </summary>
        /// <returns></returns>
        public Task StopAsync()
        {
            BeginStop();
            return WaitStopAsync();
        }

        /// <summary>
        /// Gets state of the host.
        /// </summary>
        /// <returns>The state of the host.</returns>
        public async ValueTask<ENetAsyncHostState> GetStateAsync()
        {
            using (var lockguard = await SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                var sharedState = lockguard.SharedState;
                if (sharedState.Stopped)
                    return ENetAsyncHostState.Stopped;

                if (sharedState.Started)
                    return ENetAsyncHostState.Started;

                return ENetAsyncHostState.NotStarted;
            }
        }

        /// <summary>
        /// Stops the host, if it have not, disposes the host and other resources.
        /// </summary>
        public void Dispose()
        {
            BeginStop();
            WaitStopAsync().GetAwaiter().GetResult();

            using (var lockguard = SharedStateLock.Acquire())
            {
                lockguard.SharedState.Host.Dispose();
            }
        }

        private void ServiceThreadEntry()
        {
            try
            {
                RunServiceLoop();
                m_StopCompletionSource.TrySetResult(true);
            }
            catch (Exception ex)
            {
                m_StopCompletionSource.TrySetException(ex);
            }

            using (var lockguard = SharedStateLock.Acquire())
            {
                var sharedState = lockguard.SharedState;

                foreach (var asyncPeer in sharedState.AsyncPeers)
                {
                    asyncPeer.OnDisconnect(0);
                    asyncPeer.ConnectCompletionSource?.TrySetResult(true);
                }

                sharedState.AsyncPeers.Clear();
                sharedState.Stopped = true;
            }
        }

        private void RunServiceLoop()
        {
            var serviceTimeout = TimeSpan.FromMilliseconds(SERVICE_TIMEOUT_MS);

            while (m_Stop != false)
            {
                Thread.Yield();

                using (var lockguard = SharedStateLock.Acquire())
                {
                    var host = lockguard.SharedState.Host;

                    var @event = host.Service(serviceTimeout);
                    if (@event.Type == ENetEventType.None)
                        continue;

                    HandleEvent(lockguard.SharedState, @event);

                    while (true)
                    {
                        @event = host.CheckEvents();
                        if (@event.Type != ENetEventType.None)
                            HandleEvent(lockguard.SharedState, @event);
                        else
                            break;
                    }
                }
            }
        }

        private void HandleEvent(ENetAsyncSharedState sharedState, ENetEvent @event)
        {
            ENetAsyncPeer? asyncPeer;

            if (@event.Type == ENetEventType.None)
                return;

            ENetAsyncPeer? tryGetAsyncPeer() =>
                 sharedState.AsyncPeers.Find(x => x.Peer == @event.Peer);

            ENetAsyncPeer getAsyncPeer() =>
                 tryGetAsyncPeer() ??
                 throw new NullReferenceException("Event's associated peer is unexpectedly not registered in the async peers list.");

            switch (@event.Type)
            {
                case ENetEventType.Connect:
                    var remoteEndPoint = @event.Peer.GetRemoteEndPoint();
                    asyncPeer = tryGetAsyncPeer();
                    if (asyncPeer == null)
                    {
                        asyncPeer = new ENetAsyncPeer(this, @event.Peer, remoteEndPoint, connectData: @event.Data);
                        sharedState.AsyncPeers.Add(asyncPeer);
                        m_AcceptChannel.Writer.TryWrite(asyncPeer);
                    }
                    else
                    {
                        asyncPeer.ConnectCompletionSource!.TrySetResult(true);
                    }
                    break;

                case ENetEventType.Disconnect:
                    asyncPeer = getAsyncPeer();
                    asyncPeer.OnDisconnect(@event.Data);
                    sharedState.AsyncPeers.OrderlessRemove(asyncPeer);
                    break;

                case ENetEventType.Receive:
                    asyncPeer = getAsyncPeer();
                    asyncPeer.OnReceive(@event.Packet, @event.ChannelId);
                    @event.Packet.RemoveRef();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
