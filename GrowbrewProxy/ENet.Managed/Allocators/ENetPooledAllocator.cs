using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ENet.Managed.Allocators
{
    /// <summary>
    /// Implements a pooling allocator with fixed length buffers for ENet
    /// </summary>
    public sealed class ENetPooledAllocator : ENetAllocator, IDisposable
    {
        private const int MAX_POOL_BUFFER_LENGTH = 1024 * 16;
        private const int MAX_POOL_BUFFER_COUNT = 512;

        private readonly ConcurrentBag<IntPtr> m_Buffers;
        private readonly int m_MaxBufferLen, m_MaxBufferCount;

        /// <summary>
        /// Constructs the allocator with a general size.
        /// </summary>
        public ENetPooledAllocator() : this(MAX_POOL_BUFFER_LENGTH, MAX_POOL_BUFFER_COUNT) { }

        /// <summary>
        /// Constructs the allocator with custom size.
        /// </summary>
        /// <param name="maxBufferLen">Maximum length of buffers this allocator is allowed to pool.</param>
        /// <param name="maxBufferCount">Maximum number of buffers this allocator is allowed to pool.</param>
        public ENetPooledAllocator(int maxBufferLen, int maxBufferCount)
        {
            if (maxBufferLen < 0)
                throw new ArgumentOutOfRangeException(nameof(m_MaxBufferLen));

            if (maxBufferCount < 0)
                throw new ArgumentOutOfRangeException(nameof(maxBufferCount));

            m_MaxBufferLen = maxBufferLen;
            m_MaxBufferCount = maxBufferCount;
            m_Buffers = new ConcurrentBag<IntPtr>();
        }

        public override IntPtr Allocate(int size)
        {
            IntPtr buf;

            var isPoolable = size <= m_MaxBufferLen;

            // If requested buffer length meets the maximum
            // If yes then check if pool has any buffer
            // If yes then take the buffer, skip its header and return it
            if (isPoolable && m_Buffers.TryTake(out buf))
                return IntPtr.Add(buf, sizeof(int));


            // If poolable then set the size to the fixed length
            if (isPoolable)
                size = m_MaxBufferLen;

            // Allocate buffer plus its header
            buf = Marshal.AllocHGlobal(size + sizeof(int));

            // Mark the buffer pooled if it is
            Marshal.WriteInt32(buf, isPoolable ? 1 : 0);

            // skip the header and return
            return IntPtr.Add(buf, sizeof(int));
        }

        public override void Free(IntPtr ptr)
        {
            // Get header by substracting header len from ptr
            ptr = IntPtr.Subtract(ptr, sizeof(int));

            // Check if our buffer is pooled by reading the buffer header
            var isPoolable = (Marshal.ReadInt32(ptr) != 0);

            // if the buffer is poolable and pool isn't full then return it to pool;
            // otherwise release the buffer memory.
            if (isPoolable && m_Buffers.Count > m_MaxBufferCount)
            {
                // Return it to pool
                m_Buffers.Add(ptr); 
            }
            else
            {
                // Release it away
                Marshal.FreeHGlobal(ptr); 
            }
        }

        // It is safe to call this function multiple times
        protected override void Dispose(bool disposing)
        {
            while (m_Buffers.TryTake(out var buf))
            {
                Marshal.FreeHGlobal(buf);
            }
        }
    }
}
