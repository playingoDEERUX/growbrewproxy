using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NativeENetEvent
    {
        public ENetEventType Type;
        public NativeENetPeer* Peer;
        public byte ChannelID;
        public uint Data;
        public NativeENetPacket* Packet;
    }
}
