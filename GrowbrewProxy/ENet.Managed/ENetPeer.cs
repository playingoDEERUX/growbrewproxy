using System;
using System.Net;
using System.Runtime.CompilerServices;

using ENet.Managed.Internal;
using ENet.Managed.Native;

namespace ENet.Managed
{
    /// <summary>
    /// Wraps <see cref="NativeENetPeer"/> pointer
    /// </summary>
    public unsafe readonly struct ENetPeer : IEquatable<ENetPeer>
    {
        private readonly NativeENetPeer* m_Native;

        /// <summary>
        /// Indicates whether underlaying pointer is null or not
        /// </summary>
        public bool IsNull => m_Native == null;

        /// <summary>
        /// Peer state.
        /// </summary>
        public ENetPeerState State
        {
            get
            {
                ThrowIfNull();

                return m_Native->State;
            }
        }

        /// <summary>
        /// Native user supplied data.
        /// </summary>
        public IntPtr UserData
        {
            get
            {
                ThrowIfNull();

                return m_Native->Data;
            }
            set
            {
                ThrowIfNull();

                m_Native->Data = value;
            }
        }

        public ENetPeer(NativeENetPeer* peer)
        {
            if (peer == null)
                throw new ArgumentNullException(nameof(peer));

            m_Native = peer;
        }

        /// <summary>
        /// Gets underlaying <see cref="NativeENetPeer"/> pointer
        /// </summary>
        /// <returns>Pointer to <see cref="NativeENetPeer"/></returns>
        public NativeENetPeer* GetNativePointer()
        {
            ThrowIfNull();

            return m_Native;
        }

        /// <summary>
        /// Get remote IP and Port of peer as <see cref="IPEndPoint"/>
        /// </summary>
        /// <returns>Peer's remote endpoint</returns>
        public IPEndPoint GetRemoteEndPoint()
        {
            ThrowIfNull();

            return m_Native->Address.ToIPEndPoint();
        }

        /// <summary>
        /// Queues a packet to be sent.
        /// </summary>
        /// <param name="channelId">Destination channel Id</param>
        /// <param name="packet">Packet to be queued</param>
        /// <remarks>
        /// This method will destroy the packet if its <see cref="ENetPacket.ReferenceCount"/> is zero
        /// </remarks>
        public void Send(byte channelId, ENetPacket packet)
        {
            ThrowIfNull();

            if (packet.IsNull)
                throw new ArgumentNullException(nameof(packet));

            if (LibENet.PeerSend(m_Native, channelId, packet.GetNativePointer()) < 0)
                ThrowHelper.ThrowENetPeerSendFailed();
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
        public void Send(byte channelId, ReadOnlySpan<byte> buffer, ENetPacketFlags flags)
        {
            ThrowIfNull();

            NativeENetPacket* packet;
            fixed (byte* p = buffer)
            {
                packet = LibENet.PacketCreate((IntPtr)p, unchecked((UIntPtr)buffer.Length), flags & ~ENetPacketFlags.NoAllocate);
            }

            if (LibENet.PeerSend(m_Native, channelId, packet) < 0)
                ThrowHelper.ThrowENetPeerSendFailed();
        }

        /// <summary>
        /// Attempts to dequeue any incoming queued packet.
        /// </summary>
        /// <param name="packet">Received packet if return value is true</param>
        /// <param name="channelId">Receiver channel if return value is true</param>
        /// <returns>Return true if packet received otherwise false</returns>
        public bool TryReceive(out ENetPacket packet, out byte channelId)
        {
            ThrowIfNull();

            byte chnl = 0;
            var resultPacket = LibENet.PeerReceive(m_Native, &chnl);
            if (resultPacket == null)
            {
                channelId = 0;
                packet = default;
                return false;
            }
            else
            {
                channelId = chnl;
                packet = new ENetPacket(resultPacket);
                return true;
            }
        }

        /// <summary>
        /// Forcefully disconnects a peer.
        /// </summary>
        /// <remarks>
        /// The foreign host represented by the peer is not notified of the disconnection and will timeout on its connection to the local host.
        /// </remarks>
        public void Reset()
        {
            ThrowIfNull();

            LibENet.PeerReset(m_Native);
        }

        /// <summary>
        /// Request disconnection from the peer.
        /// </summary>
        /// <param name="data">Data describing the disconnection</param>
        /// <remarks>
        /// An <see cref="ENetEventType.Disconnect"/> event will be generated by service method once disconnection is complete
        /// </remarks>
        public void Disconnect(uint data)
        {
            ThrowIfNull();

            LibENet.PeerDisconnect(m_Native, data);
        }

        /// <summary>
        /// Request disconnection from the peer.
        /// </summary>
        /// <param name="data">Data describing the disconnection</param>
        /// <remarks>
        /// An <see cref="ENetEventType.Disconnect"/> event will be generated by service method once disconnection is complete
        /// </remarks>
        public void DisconnectLater(uint data)
        {
            ThrowIfNull();

            LibENet.PeerDisconnectLater(m_Native, data);
        }

        /// <summary>
        /// Force an immediate disconnection from the peer.
        /// </summary>
        /// <param name="data">Data describing the disconnection</param>
        /// <remarks>
        /// No <see cref="ENetEventType.Disconnect"/> event will be generated by service method once disconnection is complete
        /// </remarks>
        public void DisconnectNow(uint data)
        {
            ThrowIfNull();

            LibENet.PeerDisconnectNow(m_Native, data);
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
        public void Ping()
        {
            ThrowIfNull();

            LibENet.PeerPing(m_Native);
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
        public void PingInterval(TimeSpan timeout)
        {
            ThrowIfNull();

            LibENet.PeerPingInterval(m_Native, (uint)timeout.TotalMilliseconds);
        }

        /// <summary>
        /// Configures throttle parameter for a peer.
        /// </summary>
        /// <remarks>
        /// See enet_peer_throttle_configure function documentation at http://enet.bespin.org/
        /// </remarks>
        public void ThrottleConfigure(TimeSpan interval, uint acceleration, uint deceleration)
        {
            ThrowIfNull();

            LibENet.PeerThrottleConfigure(m_Native, (uint)interval.TotalMilliseconds, acceleration, deceleration);
        }

        /// <summary>
        /// Sets the timeout parameters for a peer.
        /// </summary>
        /// <remarks>
        /// See enet_peer_timeout function documentation at http://enet.bespin.org/
        /// </remarks>
        public void Timeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum)
        {
            ThrowIfNull();

            LibENet.PeerTimeout(m_Native, timeoutLimit, timeoutMinimum, timeoutMaximum);
        }

        private void ThrowIfNull()
        {
            if (IsNull)
                ThrowHelper.ThrowENetNullPeerPointer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ENetPeer other)
        {
            return other.m_Native == m_Native;
        }

        public override bool Equals(object obj)
        {
            if (obj is ENetPeer peer)
            {
                return Equals(peer);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return new IntPtr(m_Native).GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ENetPeer a, ENetPeer b) => a.Equals(b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ENetPeer a, ENetPeer b) => !a.Equals(b);
    }
}