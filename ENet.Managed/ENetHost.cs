using System;
using System.Net;
using System.Runtime.InteropServices;

using ENet.Managed.Checksums;
using ENet.Managed.Common;
using ENet.Managed.Compressors;
using ENet.Managed.Internal;
using ENet.Managed.Native;

namespace ENet.Managed
{
    public sealed unsafe class ENetHost : DisposableBase
    {
        public const int MaximumPeers = LibENet.ProtocolMaximumPeerId;

        // We use this reference to indicate that host is using ENet's builtin CRC32 checksum method
        private static readonly ENetSentinelChecksum s_Crc32Checksum = new ENetSentinelChecksum();

        // We use this reference to indicate that host is using ENet's builtin ranger-coder as compressor 
        private static readonly ENetSentinelCompressor s_RangeCoderCompressor = new ENetSentinelCompressor();

        // Native pointer to ENetHost struct
        private readonly IntPtr m_Pointer;

        private ENetChecksum? m_Checksum;
        private ENetCompressor? m_Compressor;

        // we have to hold this callbacks reference to prevent it from being collected by GC
        private ENetChecksumCallback? m_ChecksumCallback;
        private ENetCompressCallback? m_CompressorCompressCallback;
        private ENetDecompressCallback? m_CompressorDecompressCallback;
        private ENetInterceptCallback? m_InterceptCallback;

        // Unsafe native pointers to fields of actual ENetHost struct
        private readonly IntPtr* m_pInterceptCallback;
        private readonly IntPtr* m_pChecksumCallback;
        private readonly uint* m_pTotalSentData;
        private readonly uint* m_pTotalSentPackets;
        private readonly uint* m_pTotalReceivedData;
        private readonly uint* m_pTotalReceivedPackets;
        private readonly UIntPtr* m_pConnectedPeers;
        private readonly UIntPtr* m_pDuplicatePeers;
        private readonly UIntPtr* m_pPeerCount;
        internal readonly NativeENetPeer* PeersStartPtr;

        /// <summary>
        /// Gets current in-use (custom) compressor
        /// </summary>
        public ENetCompressor Compressor
        {
            get
            {
                if (m_Compressor == null)
                    ThrowHelper.ThrowENetHostNoCompresserInUse();

                if (m_Compressor == s_RangeCoderCompressor)
                    ThrowHelper.ThrowENetHostIsUsingRangeCoder();

                return m_Compressor!;
            }
        }

        /// <summary>
        /// Gets current in-use (custom) checksum
        /// </summary>
        public ENetChecksum Checksum
        {
            get
            {
                if (m_Checksum == null)
                    ThrowHelper.ThrowENetHostNoChecksumInUse();

                if (m_Checksum == s_Crc32Checksum)
                    ThrowHelper.ThrowENetHostIsUsingCRC32();

                return m_Checksum!;
            }
        }

        /// <summary>
        /// Gets current in-use (custom) intercept callback
        /// </summary>
        public ENetInterceptCallback InterceptCallback
        {
            get
            {
                if (m_InterceptCallback == null)
                    ThrowHelper.ThrowENetHostIsNotUsingInterceptor();

                return m_InterceptCallback!;
            }
        }
        /// <summary>
        /// Indicates whether checksum is enabled or not
        /// </summary>
        public bool IsUsingChecksum => m_Checksum != null;

        /// <summary>
        /// Indicates whether ENet's builtin CRC32 checksum is enabled or not
        /// </summary>
        public bool IsUsingCRC32Checksum => m_Checksum == s_Crc32Checksum;

        /// <summary>
        /// Indicates whether compression is enabled or not
        /// </summary>
        public bool IsUsingCompressor => m_Compressor != null;

        /// <summary>
        /// Indicates whether ENet's builtin range-coder compressor is enabled or not
        /// </summary>
        public bool IsUsingRangerCoderCompressor => m_Compressor == s_RangeCoderCompressor;

        /// <summary>
        /// Indicates whether an interceptor is being used or not
        /// </summary>
        public bool IsUsingInterceptor => m_InterceptCallback != null;

        /// <summary>
        /// Total number of bytes sent
        /// </summary>
        public long TotalSentData
        {
            get
            {
                CheckDispose();
                return *m_pTotalSentData;
            }
        }

        /// <summary>
        /// Total number of packets sent
        /// </summary>
        public long TotalSentPackets
        {
            get
            {
                CheckDispose();
                return *m_pTotalSentPackets;
            }
        }

        /// <summary>
        /// Total number of received bytes
        /// </summary>
        public long TotalReceivedData
        {
            get
            {
                CheckDispose();
                return *m_pTotalReceivedData;
            }
        }

        /// <summary>
        /// Total number of received packets
        /// </summary>
        public long TotalReceivedPackets
        {
            get
            {
                CheckDispose();
                return *m_pTotalReceivedPackets;
            }
        }

        /// <summary>
        /// Number of connected peers
        /// </summary>
        public int ConnectedPeers
        {
            get
            {
                CheckDispose();
                return unchecked((int)(*m_pConnectedPeers));
            }
        }

        /// <summary>
        /// Maximum number of Peers that can connect with same IP.
        /// </summary>
        public int DuplicatePeers
        {
            get
            {
                CheckDispose();
                return unchecked((int)(*m_pDuplicatePeers));
            }
            set
            {
                CheckDispose();
                *m_pDuplicatePeers = new UIntPtr(unchecked((uint)value));
            }
        }

        /// <summary>
        /// Number of peers allocated.
        /// </summary>
        public int PeersCount
        {
            get
            {
                CheckDispose();
                return unchecked((int)(*m_pPeerCount));
            }
        }

        /// <summary>
        /// List of all allocated and pre-allocated peers by this host.
        /// </summary>
        public ENetHostPeerList PeerList { get; }

        /// <summary>
        /// Instantiates <see cref="ENetHost"/> by creating native ENet host 
        /// </summary>
        /// <param name="address">The address at which other peers may connect to this host. If null, then no peers may connect to the host.</param>
        /// <param name="peers">Maximum number of peers</param>
        /// <param name="channels">Maximum number of channels (0 = <see cref="MaximumPeers"/>)</param>
        /// <param name="incomingBandwidth">Maximum incoming bandwidth (0 = unlimited)</param>
        /// <param name="outgoingBandwidth">Maximum outgoing bandwidth (0 = unlimited)</param>
        public ENetHost(IPEndPoint? address, int peers, byte channels, long incomingBandwidth = 0, long outgoingBandwidth = 0)
        {
            if (incomingBandwidth < uint.MinValue || incomingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(incomingBandwidth));

            if (outgoingBandwidth < uint.MinValue || outgoingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(outgoingBandwidth));

            if (peers < 0 || peers > MaximumPeers)
                throw new ArgumentOutOfRangeException(nameof(peers));

            var nativeAddress = NativeENetAddress.FromIPEndPoint(address ?? new IPEndPoint(IPAddress.Any, 0));
            m_Pointer = LibENet.HostCreate(nativeAddress.Type, &nativeAddress, (UIntPtr)peers, (UIntPtr)channels, (uint)incomingBandwidth, (uint)outgoingBandwidth);

            if (m_Pointer == IntPtr.Zero)
                ThrowHelper.ThrowENetCreateHostFailed();

            // id must be in sync with interop_helpers.c
            static int getOffset(uint id)
            {
                var offset = LibENet.InteropHelperSizeOrOffset(id);
                if (offset == new IntPtr(-1))
                    throw new ENetException("Size-or-Offset identifier mismatch.");
                return offset.ToInt32();
            }

            m_pInterceptCallback = (IntPtr*)IntPtr.Add(m_Pointer, getOffset(3));
            m_pChecksumCallback = (IntPtr*)IntPtr.Add(m_Pointer, getOffset(2));
            m_pTotalSentData = (uint*)IntPtr.Add(m_Pointer, getOffset(4));
            m_pTotalSentPackets = (uint*)IntPtr.Add(m_Pointer, getOffset(5));
            m_pTotalReceivedData = (uint*)IntPtr.Add(m_Pointer, getOffset(12));
            m_pTotalReceivedPackets = (uint*)IntPtr.Add(m_Pointer, getOffset(6));
            m_pConnectedPeers = (UIntPtr*)IntPtr.Add(m_Pointer, getOffset(7));
            m_pDuplicatePeers = (UIntPtr*)IntPtr.Add(m_Pointer, getOffset(8));
            m_pPeerCount = (UIntPtr*)IntPtr.Add(m_Pointer, getOffset(9));

            PeersStartPtr = (NativeENetPeer*)Marshal.ReadIntPtr(IntPtr.Add(m_Pointer, getOffset(10)));
            PeerList = new ENetHostPeerList(this);
        }

        /// <summary>
        /// Gets native pointer to ENetHost that could be passed to ENet library functions
        /// </summary>
        public IntPtr GetNativePointer()
        {
            CheckDispose();

            return m_Pointer;
        }

        /// <summary>
        /// Connects to the specified <see cref="IPEndPoint"/>
        /// </summary>
        /// <param name="endPoint">Remote endpoint</param>
        /// <param name="channels">Maximum number of channels (can't be zero)</param>
        /// <param name="data">User data supplied to the receiving host</param>
        /// <returns>The connecting peer</returns>
        public ENetPeer Connect(IPEndPoint endPoint, byte channels, uint data)
        {
            CheckDispose();

            if (channels < 1)
                throw new ArgumentOutOfRangeException(nameof(channels));

            NativeENetAddress address = NativeENetAddress.FromIPEndPoint(endPoint);
            var native = LibENet.HostConnect(m_Pointer, &address, unchecked((UIntPtr)channels), data);

            if (((IntPtr)native) == IntPtr.Zero)
                ThrowHelper.ThrowENetConnectFailure();

            return new ENetPeer(native);
        }

        /// <summary>
        /// Queues a packet to be sent to all peers associated with the host.
        /// </summary>
        /// <remarks>
        /// <see cref="ENetPacketFlags.NoAllocate"/> will be ignored.
        /// </remarks>
        public void Broadcast(byte channel, ReadOnlySpan<byte> buffer, ENetPacketFlags flags)
        {
            CheckDispose();

            NativeENetPacket* packet;
            fixed (byte* p = buffer)
            {
                packet = LibENet.PacketCreate((IntPtr)p, unchecked((UIntPtr)buffer.Length), flags & ~ENetPacketFlags.NoAllocate);
            }

            LibENet.HostBroadcast(m_Pointer, channel, packet);
        }

        /// <summary>
        /// Queues a packet to be sent to all peers associated with the host.
        /// </summary>
        /// <remarks>
        /// This method will destroy the packet if its <see cref="ENetPacket.ReferenceCount"/> is zero
        /// </remarks>
        public void Broadcast(byte channel, ENetPacket packet)
        {
            CheckDispose();

            if (packet.IsNull)
                throw new ArgumentNullException(nameof(packet));

            LibENet.HostBroadcast(m_Pointer, channel, packet.GetNativePointer());
        }

        /// <summary>
        /// Adjusts the bandwidth limits of the host.
        /// </summary>
        public void BandwidthLimit(long incomingBandwidth, long outgoingBandwidth)
        {
            CheckDispose();

            if (incomingBandwidth < uint.MinValue || incomingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(incomingBandwidth));

            if (outgoingBandwidth < uint.MinValue || outgoingBandwidth > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(outgoingBandwidth));

            LibENet.HostBandwidthLimit(m_Pointer, unchecked((uint)incomingBandwidth), unchecked((uint)outgoingBandwidth));
        }

        /// <summary>
        /// Limits the maximum allowed channels of future incoming connections.
        /// </summary>
        public void ChannelLimit(byte channels)
        {
            CheckDispose();

            LibENet.HostChannelLimit(m_Pointer, unchecked((UIntPtr)channels));
        }

        /// <summary>
        /// Checks for any queued events on the host and dispatches one if available.
        /// </summary>
        public ENetEvent CheckEvents()
        {
            CheckDispose();

            NativeENetEvent native;
            if (LibENet.HostCheckEvents(m_Pointer, &native) < 0)
                ThrowHelper.ThrowENetFailure();

            return new ENetEvent(native);
        }

        /// <summary>
        /// Waits for events on the host specified and shuttles packets between the host and its peers.
        /// </summary>
        public ENetEvent Service(TimeSpan timeout)
        {
            CheckDispose();

            NativeENetEvent native;

            if (LibENet.HostService(m_Pointer, &native, unchecked((uint)timeout.TotalMilliseconds)) < 0)
                ThrowHelper.ThrowENetFailure();

            return new ENetEvent(native);
        }

        /// <summary>
        /// Sends any queued packets on the host specified to its designated peers.
        /// </summary>
        public void Flush()
        {
            CheckDispose();

            LibENet.HostFlush(m_Pointer);
        }

        /// <summary>
        /// Enables compression with given custom compressor
        /// </summary>
        public void CompressWith(ENetCompressor compressor)
        {
            CheckDispose();

            if (compressor == null)
                throw new ArgumentNullException(nameof(compressor));

            if (m_Compressor != null)
            {
                DisableCompression();
            }

            m_CompressorCompressCallback = (_, inBuffers, inBufferCount, inLimit, outData, outLimit) =>
            {
                var buffersCount32 = (int)inBufferCount;

                compressor.BeginCompress((int)inLimit);

                for (int i = 0; i < buffersCount32; i++)
                {
                    compressor.CompressChunk(inBuffers[i].AsSpan());
                }

                return (UIntPtr)compressor.EndCompress(new Span<byte>(outData.ToPointer(), (int)outLimit));
            };

            m_CompressorDecompressCallback = (_, inData, inLimit, outData, outLimit) =>
            {
                return (UIntPtr)compressor.Decompress(new ReadOnlySpan<byte>(inData.ToPointer(), (int)inLimit),
                                                      new Span<byte>(outData.ToPointer(), (int)outLimit));
            };

            NativeENetCompressor nativeCompressor = default;
            nativeCompressor.Context = new IntPtr(1);
            nativeCompressor.Destroy = IntPtr.Zero;
            nativeCompressor.Compress = Marshal.GetFunctionPointerForDelegate(m_CompressorCompressCallback);
            nativeCompressor.Decompress = Marshal.GetFunctionPointerForDelegate(m_CompressorDecompressCallback);

            LibENet.HostCompress(m_Pointer, &nativeCompressor);
        }

        /// <summary>
        /// Enables compression using ENet's builtin range-coder compression
        /// </summary>
        public void CompressWithRangeCoder()
        {
            CheckDispose();

            if (m_CompressorCompressCallback != null)
            {
                DisableCompression();
            }

            if (LibENet.HostCompressWithRangeCoder(m_Pointer) < 0)
                ThrowHelper.ThrowENetHostSetCompressWithRangeCoderFailed();

            m_Compressor = s_RangeCoderCompressor;
        }

        /// <summary>
        /// Disables compression
        /// </summary>
        public void DisableCompression()
        {
            CheckDispose();

            if (m_Compressor != null)
            {
                m_CompressorCompressCallback = null;
                m_CompressorDecompressCallback = null;
                m_Compressor.Dispose();
                m_Compressor = null;
            }

            LibENet.HostCompress(m_Pointer, null);
        }

        /// <summary>
        /// Enables checksum with given custom checksum method
        /// </summary>
        public void ChecksumWith(ENetChecksum checksum)
        {
            CheckDispose();

            if (m_Checksum != null)
            {
                DisableChecksum();
            }

            m_Checksum = checksum ?? throw new ArgumentNullException(nameof(checksum));

            m_ChecksumCallback = (buffers, buffersCount) =>
            {
                var buffersCount32 = checked((int)buffersCount);
                checksum.Begin();
                for (int i = 0; i < buffersCount32; i++)
                {
                    checksum.Sum(buffers[i].AsSpan());
                }
                return checksum.End();
            };

            *m_pChecksumCallback = Marshal.GetFunctionPointerForDelegate(m_ChecksumCallback);
        }

        /// <summary>
        /// Enables ENet's builtin CRC32 checksum
        /// </summary>
        public void ChecksumWithCRC32()
        {
            CheckDispose();

            if (m_Checksum != null)
            {
                DisableChecksum();
            }

            m_Checksum = s_Crc32Checksum;
            *m_pChecksumCallback = LibENet.GetProc("enet_crc32");
        }

        /// <summary>
        /// Disables checksum
        /// </summary>
        public void DisableChecksum()
        {
            CheckDispose();

            if (m_Checksum != null)
            {
                m_ChecksumCallback = null;
                m_Checksum.Dispose();
                m_Checksum = null;
            }

            *m_pChecksumCallback = IntPtr.Zero;
        }

        /// <summary>
        /// Intercepts host events with given callback
        /// </summary>
        /// <param name="callback">Callback which intercepts the events</param>
        public void InterceptWith(ENetInterceptCallback callback)
        {
            CheckDispose();

            if (m_InterceptCallback != null)
                DisableInterception();

            m_InterceptCallback = callback ?? throw new ArgumentNullException(nameof(callback));
            *m_pInterceptCallback = Marshal.GetFunctionPointerForDelegate(callback);
        }

        /// <summary>
        /// Disables interception
        /// </summary>
        public void DisableInterception()
        {
            CheckDispose();

            m_InterceptCallback = null;
            *m_pInterceptCallback = IntPtr.Zero;
        }

        protected override void Dispose(bool disposing)
        {
            LibENet.HostDestroy(m_Pointer);

            if (disposing)
                return;

            m_ChecksumCallback = null;
            m_InterceptCallback = null;
            m_CompressorCompressCallback = null;
            m_CompressorDecompressCallback = null;

            if (m_Compressor != null)
            {
                m_Compressor.Dispose();
                m_Compressor = null;
            }

            if (m_Checksum != null)
            {
                m_Checksum.Dispose();
                m_Checksum = null;
            }
        }

        internal void CheckDispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(ENetHost));
        }
    }
}
