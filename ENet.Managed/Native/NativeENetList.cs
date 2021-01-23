using System.Runtime.InteropServices;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeENetList
    {
        public NativeENetListNode Sentinel;
    }
}
