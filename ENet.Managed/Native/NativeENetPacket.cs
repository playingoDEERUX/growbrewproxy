using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeENetPacket
    {
        public UIntPtr ReferenceCount;
        public ENetPacketFlags Flags;
        public IntPtr Data;
        public UIntPtr DataLength;
        public IntPtr FreeCallback;
        public IntPtr UserData;

        public unsafe Span<byte> GetDataAsSpan()
        {
            return new Span<byte>(Data.ToPointer(), unchecked((int)DataLength));
        }
    }
}
