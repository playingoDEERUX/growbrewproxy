using System;
using System.Runtime.InteropServices;

using ENet.Managed.Internal;

namespace ENet.Managed.Platforms
{
    public sealed class Win32Platform : Platform
    {
        public override byte[] GetENetBinaryBytes()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                case Architecture.X64:
                    return Environment.Is64BitProcess ? ENetBinariesResource.enet_win32_x86_64 :
                                                        ENetBinariesResource.enet_win32_x86;

                case Architecture.Arm:
                    return ENetBinariesResource.enet_win32_arm64;

                default:
                    ThrowHelper.ThrowCurrentArchitectureNotSupportedYet();
                    return null!;
            }
        }

        public override string GetENetBinaryName()
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                case Architecture.X64:
                    return Environment.Is64BitProcess ? "enet-win32-x86-64.dll" : "enet-win32-x86.dll";

                case Architecture.Arm:
                    return "enet-win32-arm.dll";

                default:
                    ThrowHelper.ThrowCurrentArchitectureNotSupportedYet();
                    return null!;
            }
        }

        public override void FreeDynamicLibrary(IntPtr hModule) => Win32Api.FreeLibrary(hModule);

        public override IntPtr LoadDynamicLibrary(string dllPath)
        {
            var handle = Win32Api.LoadLibrary(dllPath);
            var lastError = Marshal.GetLastWin32Error();

            if (handle == IntPtr.Zero)
                ThrowHelper.ThrowENetLibraryLoadFailed(lastError);

            return handle;
        }

        public override IntPtr GetDynamicLibraryProcedureAddress(IntPtr handle, string procName)
        {
            var addr = Win32Api.GetProcAddress(handle, procName);
            var lastError = Marshal.GetLastWin32Error();

            if (addr == IntPtr.Zero)
                ThrowHelper.ThrowENetLibraryProcNotFound(procName, lastError);

            return addr;
        }
    }
}
