using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Platforms
{
    internal static class LinuxApi
    {
        public const string LIBDL = "libdl.so";
        public const string LIBC = "libc";
        public const int RTLD_NOW = 2;

        [DllImport(LIBDL)]
        public static extern IntPtr dlopen(string lpFileName, int flags);

        [DllImport(LIBDL)]
        public static extern int dlclose(IntPtr hModule);

        [DllImport(LIBDL)]
        public static extern IntPtr dlsym(IntPtr hModule, string name);
    }
}
