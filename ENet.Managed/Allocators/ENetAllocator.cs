using System;

using ENet.Managed.Common;

namespace ENet.Managed.Allocators
{
    /// <summary>
    /// An abstract class which is responsible for allocating and deallocating memory for ENet library
    /// </summary>
    public abstract class ENetAllocator : DisposableBase
    {
        public abstract IntPtr Allocate(int size);
        public abstract void Free(IntPtr ptr);
    }
}
