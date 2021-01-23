using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeENetBuffer
    {
        public UIntPtr DataLength;
        public IntPtr Data;

        public NativeENetBuffer(IntPtr data, UIntPtr dataLength)
        {
            Data = data;
            DataLength = dataLength;
        }

        public unsafe Span<byte> AsSpan()
        {
            return new Span<byte>(Data.ToPointer(), unchecked((int)DataLength));
        }
    }
}
