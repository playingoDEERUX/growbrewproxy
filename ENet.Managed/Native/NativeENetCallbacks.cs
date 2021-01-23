using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeENetCallbacks
    {
        public IntPtr Malloc;
        public IntPtr Free;
        public IntPtr NoMemory;
    }
}
