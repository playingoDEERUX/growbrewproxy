using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeENetPeer
    {
        public NativeENetListNode DispatchList;
        public IntPtr Host;
        public ushort OutgoingPeerID;
        public ushort IncomingPeerID;
        public uint ConnectID;
        public byte OutgoingSessionID;
        public byte IncomingSessionID;
        public NativeENetAddress Address;
        public IntPtr Data;
        public ENetPeerState State;
        public IntPtr Channels;
        public UIntPtr ChannelCount;
        public uint IncomingBandwidth;
        public uint OutgoingBandwidth;
        public uint IncomingBandwidthThrottleEpoch;
        public uint OutgoingBandwidthThrottleEpoch;
        public uint IncomingDataTotal;
        public uint OutgoingDataTotal;
        public uint LastSendTime;
        public uint LastReceiveTime;
        public uint NextTimeout;
        public uint EarliestTimeout;
        public uint PacketLossEpoch;
        public uint PacketsSent;
        public uint PacketsLost;
        public uint PacketLoss;
        public uint PacketLossVariance;
        public uint PacketThrottle;
        public uint PacketThrottleLimit;
        public uint PacketThrottleCounter;
        public uint PacketThrottleEpoch;
        public uint PacketThrottleAcceleration;
        public uint PacketThrottleDeceleration;
        public uint PacketThrottleInterval;
        public uint PingInterval;
        public uint TimeoutLimit;
        public uint TimeoutMinimum;
        public uint TimeoutMaximum;
        public uint LastRoundTripTime;
        public uint LowestRoundTripTime;
        public uint LastRoundTripTimeVariance;
        public uint HighestRoundTripTimeVariance;
        public uint RoundTripTime;
        public uint RoundTripTimeVariance;
        public uint MTU;
        public uint WindowSize;
        public uint ReliableDataInTransit;
        public ushort OutgoingReliableSequenceNumber;
        public NativeENetList Acknowledgements;
        public NativeENetList SentReliableCommands;
        public NativeENetList SentUnreliableCommands;
        public NativeENetList OutgoingCommands;
        public NativeENetList DispatchedCommands;
        public ushort Flags;
        public byte RoundTripTimeRemainder;
        public byte RoundTripTimeVarianceRemainder;
        public ushort IncomingUnsequencedGroup;
        public ushort OutgoingUnsequencedGroup;
        public fixed uint UnsequencedWindow[LibENet.PeerUnsqeuencedWindowSize / 32];
        public uint EventData;
        public UIntPtr TotalWaitingData;
    }
}