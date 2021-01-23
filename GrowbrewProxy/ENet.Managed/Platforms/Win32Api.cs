using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Platforms
{
    internal static class Win32Api
    {
        public const string KERNEL32 = "kernel32";

        [DllImport(KERNEL32)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(KERNEL32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport(KERNEL32)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    }
}
