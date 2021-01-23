using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeENetCompressor
    {
        public IntPtr Context;
        public IntPtr Compress;
        public IntPtr Decompress;
        public IntPtr Destroy;
    }
}
