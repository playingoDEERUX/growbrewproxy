using System;
using System.Runtime.InteropServices;

using ENet.Managed.Internal;

namespace ENet.Managed.Platforms
{
    public abstract class Platform
    {
        private static Platform? s_CurrentPlatform;

        public static Platform Current
        {
            // According to https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/constructor
            // We should not throw exception from static ctor
            // Thats why we are using property

            get
            {
                if (s_CurrentPlatform != null)
                    return s_CurrentPlatform;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    s_CurrentPlatform = new Win32Platform();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    s_CurrentPlatform = new LinuxPlatform();
                }
                else
                {
                    ThrowHelper.ThrowCurrentPlatfromIsNotSupportedYet();
                }

                return s_CurrentPlatform!;
            }
            set
            {
                if (s_CurrentPlatform == null)
                    throw new ArgumentNullException(nameof(value));

                s_CurrentPlatform = value;
            }
        }

        public abstract string GetENetBinaryName();
        public abstract byte[] GetENetBinaryBytes();
        public abstract IntPtr LoadDynamicLibrary(string dllPath);
        public abstract void FreeDynamicLibrary(IntPtr hModule);
        public abstract IntPtr GetDynamicLibraryProcedureAddress(IntPtr handle, string procName);
    }
}

