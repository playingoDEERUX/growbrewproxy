using System;
using System.Runtime.InteropServices;

namespace ENet.Managed.Allocators
{
    /// <summary>
    /// Implements <see cref="ENetAllocator"/> which uses <see cref="Marshal.AllocHGlobal(int)"/> and <see cref="Marshal.FreeHGlobal(IntPtr)"/> to allocate and deallocate memory.
    /// This class is singleton.
    /// </summary>
    public sealed class ENetHGlobalAllocator : ENetAllocator
    {
        public static readonly ENetHGlobalAllocator Instance = new ENetHGlobalAllocator();

        private ENetHGlobalAllocator()
        {
            // Singleton
        }

        public override IntPtr Allocate(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        public override void Free(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
