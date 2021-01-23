using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeENetListNode
    {
        public IntPtr Next;
        public IntPtr Previous;
    }
}
