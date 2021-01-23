using System;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using ENet.Managed.Internal;

namespace ENet.Managed.Async
{
    /// <summary>
    /// An asynchronous version of <see cref="ENetPeer"/>.
    /// </summary>
    public sealed class ENetAsyncPeer
    {
        private static readonly UnboundedChannelOptions s_ReceiveChannelOptions = new()
        {
            AllowSynchronousContinuations = false, // Important!
            SingleWriter = true,
            SingleReader = false,
        };

        private readonly TaskCompletionSource<uint> m_DisconnectCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Channel<ENetAsyncPacket> m_ReceivePacketChannel;
        internal ENetPeer Peer { get; }
        internal TaskCompletionSource<bool>? ConnectCompletionSource { get; set; }

        /// <summary>
        /// The assosiated (async) host.
        /// </summary>
        public ENetAsyncHost AsyncHost { get; }

        /// <summary>
        /// The (async) peer's remote endpoint.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// The connection data.
        /// </summary>
        /// <remarks>
        /// This is the data that is passed to <see cref="ENetHost.Connect(IPEndPoint, byte, uint)"/> or it's async counterpart locally or remotely.
        /// </remarks>
        public uint ConnectData { get; }

        /// <summary>
        /// User supplied data.
        /// </summary>
        public object? UserData { get; set; }


        /// <summary>
        /// Gets a task that completes when peer gets disconnected; The task returns the disconnection data.
        /// </summary>
        /// <remarks>
        /// The disconnection data is the parameter passed to <see cref="ENetPeer.Disconnect(uint)"/> or <see cref="DisconnectAsync(uint)"/>.
        /// </remarks>
        public Task<uint> Disconnection => m_DisconnectCompletionSource.Task;

        /// <summary>
        /// Gets a task that completes when peer gets connected.
        /// </summary>
        public Task Connection
        {
            get
            {
                if (ConnectCompletionSource == null)
                    return Task.CompletedTask;
                else
                    return ConnectCompletionSource.Task;
            }
        }

        /// <summary>
        /// Indicates whether the (async) peer is connected or not.
        /// </summary>
        public bool IsConnected => Connection.IsCompleted && !Disconnection.IsCompleted;

        internal ENetAsyncPeer(ENetAsyncHost asyncHost, ENetPeer peer, IPEndPoint remoteEndPoint, uint connectData)
        {
            AsyncHost = asyncHost;
            Peer = peer;
            RemoteEndPoint = remoteEndPoint;
            ConnectData = connectData;
            m_ReceivePacketChannel = Channel.CreateUnbounded<ENetAsyncPacket>(s_ReceiveChannelOptions);
        }

        /// <summary>
        /// Waits for an incoming packet and returns it.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ENetAsyncPeerDisconnectedException">Thrown if peer gets disconnected during this call.</exception>
        /// <exception cref="ENetAsyncPeerResetException">Thrown if peers gets reset during this call..</exception>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token gets cancelled.</exception>
        /// <returns>Received (async) packet.</returns>
        public async ValueTask<ENetAsyncPacket> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfNotConnected();
            try
            {
                return await m_ReceivePacketChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ChannelClosedException ex)
            {
                throw new ENetAsyncPeerDisconnectedException("Async Peer has disconnected.", ex);
            }
        }

        public ValueTask SendAsync(ENetAsyncPacket asyncPacket)
        {
            ThrowHelper.ThrowIfArgumentNull(asyncPacket, nameof(asyncPacket));

            return SendAsync(asyncPacket.ChannelId, asyncPacket.Data, asyncPacket.Flags);
        }

        /// <summary>
        /// Queues a packet to be sent.
        /// </summary>
        /// <param name="channelId">Destination channel Id</param>
        /// <param name="buffer">Buffer containing packet data</param>
        /// <param name="flags">Packet flags</param>
        /// <remarks>
        /// <see cref="ENetPacketFlags.NoAllocate"/> will be ignored.
        /// </remarks>
        public async ValueTask SendAsync(byte channelId, ReadOnlyMemory<byte> buffer, ENetPacketFlags flags)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.Send(channelId, buffer.Span, flags);
            }
        }

        /// <summary>
        /// Queues a packet to be sent.
        /// </summary>
        /// <param name="channelId">Destination channel Id</param>
        /// <param name="packet">Packet to be queued</param>
        /// <remarks>
        /// This method will destroy the packet if its <see cref="ENetPacket.ReferenceCount"/> is zero
        /// </remarks>
        public async ValueTask SendAsync(byte channelId, ENetPacket packet)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.Send(channelId, packet);
            }
        }

        /// <summary>
        /// Forcefully disconnects a peer.
        /// </summary>
        /// <remarks>
        /// ENet: <br/>
        /// The foreign host represented by the peer is not notified of the disconnection and will timeout on its connection to the local host. <br/>
        /// Async: <br/>
        /// <see cref="Connection"/>, <see cref="Disconnection"/> and <see cref="ReceiveAsync(CancellationToken)"/> will throw <see cref="ENetAsyncPeerResetException"/>.
        /// </remarks>
        public async ValueTask ResetAsync()
        {
            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.Reset();
                OnReset();
                lockguard.SharedState.AsyncPeers.OrderlessRemove(this);
            }
        }

        /// <summary>
        /// Request disconnection from the peer.
        /// </summary>
        /// <param name="data">Data describing the disconnection</param>
        /// <remarks>
        /// Makes <see cref="ReceiveAsync(CancellationToken)"/> throw <see cref="ENetAsyncPeerDisconnectedException"/> upon disconnection. <br/>
        /// Makes <see cref="Disconnection"/> complete upon disconnection.
        /// </remarks>
        public async ValueTask DisconnectAsync(uint data)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.Disconnect(data);
            }
        }

        /// <summary>
        /// Request disconnection from the peer.
        /// </summary>
        /// <param name="data">Data describing the disconnection</param>
        /// <remarks>
        /// Makes <see cref="ReceiveAsync(CancellationToken)"/> throw <see cref="ENetAsyncPeerDisconnectedException"/> upon disconnection. <br/>
        /// Makes <see cref="Disconnection"/> complete upon disconnection.
        /// </remarks>
        public async ValueTask DisconnectLaterAsync(uint data)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.DisconnectLater(data);
            }
        }

        /// <summary>
        /// Force an immediate disconnection from the peer.
        /// </summary>
        /// <param name="data">Data describing the disconnection</param>
        /// <remarks>
        /// Unlike it's counterpart <see cref="ENetPeer.DisconnectNow(uint)"/> it haves the following side effects: <br/>
        /// <see cref="Disconnection"/> completes immediately after this call. <br/>
        /// Subsequent <see cref="ReceiveAsync(CancellationToken)"/> calls throw <see cref="ENetAsyncPeerDisconnectedException"/>.
        /// </remarks>
        public async ValueTask DisconnectNowAsync(uint data)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.DisconnectNow(data);
                OnDisconnect(data);
                lockguard.SharedState.AsyncPeers.OrderlessRemove(this);
            }
        }

        /// <summary>
        /// Sends a ping request to the peer.
        /// </summary>
        /// <remarks>
        /// From offical doc:
        /// Ping requests factor into the mean round trip time as designated by the roundTripTime field in the ENetPeer structure.
        /// ENet automatically pings all connected peers at regular intervals,
        /// however, this function may be called to ensure more frequent ping requests.
        /// </remarks>
        public async ValueTask PingAsync()
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.Ping();
            }
        }

        /// <summary>
        /// Sets the interval at which pings will be sent to the peer.
        /// </summary>
        /// <param name="timeout">The interval at which to send pings; defaults to ENET_PEER_PING_INTERVAL if <see cref="TimeSpan.Zero"/></param>
        /// <remarks>
        /// From offical doc:
        /// Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during
        /// periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
        /// </remarks>
        public async ValueTask PingIntervalAsync(TimeSpan timeout)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.PingInterval(timeout);
            }
        }

        /// <summary>
        /// Configures throttle parameter for a peer.
        /// </summary>
        /// <remarks>
        /// See enet_peer_throttle_configure function documentation at http://enet.bespin.org/
        /// </remarks>
        public async ValueTask ThrottleConfigureAsync(TimeSpan interval, uint acceleration, uint deceleration)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.ThrottleConfigure(interval, acceleration, deceleration);
            }
        }

        /// <summary>
        /// Sets the timeout parameters for a peer.
        /// </summary>
        /// <remarks>
        /// See enet_peer_timeout function documentation at http://enet.bespin.org/
        /// </remarks>
        public async ValueTask Timeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum)
        {
            ThrowIfNotConnected();

            using (var lockguard = await AsyncHost.SharedStateLock.AcquireAsync().ConfigureAwait(false))
            {
                Peer.Timeout(timeoutLimit, timeoutMinimum, timeoutMaximum);
            }
        }

        public ENetPeer DangerousGetUnderlayingPeer()
        {
            return Peer;
        }

        internal void OnDisconnect(uint data)
        {
            m_DisconnectCompletionSource.TrySetResult(data);
            m_ReceivePacketChannel.Writer.TryComplete();
        }

        internal void OnReset()
        {
            var ex = new ENetAsyncPeerResetException("Peer is reset.");
            m_DisconnectCompletionSource.TrySetException(ex);
            m_ReceivePacketChannel.Writer.TryComplete(ex);
            ConnectCompletionSource?.TrySetException(ex);
        }

        internal void OnReceive(ENetPacket packet, byte channel)
        {
            var asyncPacket = new ENetAsyncPacket(packet, channel);
            m_ReceivePacketChannel.Writer.TryWrite(asyncPacket);
        }

        internal void ThrowIfNotConnected()
        {
            if (IsConnected == false)
                throw new InvalidOperationException("Async Peer is not connected.");
        }
    }
}
