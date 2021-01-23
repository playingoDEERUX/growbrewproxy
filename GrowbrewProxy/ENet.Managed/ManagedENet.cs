using System;
using System.Runtime.InteropServices;

using ENet.Managed.Allocators;
using ENet.Managed.Internal;
using ENet.Managed.Native;

#pragma warning disable CS0618 // For suppressing LibENet.Load(..) warnings

namespace ENet.Managed
{
    /// <summary>
    /// Manages initialization and deinitialization of ENet library
    /// </summary>
    /// <remarks>
    /// The methods of this should be manually called at beginning and end 
    /// of your application. <br/>
    /// This class is not thread-safe.
    /// </remarks>
    public unsafe static class ManagedENet
    {
        private static ENetAllocator? s_Allocator;
        private static Version? s_LinkedVersion;

        // We hold this delegates references in a static variable
        // in order to prevent garbage collector from collecting them
        private static readonly ENetMemoryAllocCallback s_MemAllocDelegate;
        private static readonly ENetMemoryFreeCallback s_MemFreeDelegate;
        private static readonly ENetNoMemoryCallback s_NoMemoryDelegate;

        /// <summary>
        /// Indicates whether ENet has initialized or not
        /// </summary>
        public static bool Started { get; private set; }

        /// <summary>
        /// The memory allocator currently in-use by ENet
        /// </summary>
        public static ENetAllocator Allocator
        {
            get
            {
                if (!Started)
                    ThrowHelper.ThrowENetIsNotInitialized();

                return s_Allocator!;
            }
        }

        /// <summary>
        /// The ENet static-linked version 
        /// </summary>
        public static Version LinkedVersion
        {
            get
            {
                if (!Started)
                    ThrowHelper.ThrowENetIsNotInitialized();

                return s_LinkedVersion!;
            }
        }

        static ManagedENet()
        {
            Started = false;
            s_LinkedVersion = null;
            s_Allocator = null;

            s_MemAllocDelegate = MemAllocCallback;
            s_MemFreeDelegate = MemFreeCallback;
            s_NoMemoryDelegate = NoMemoryCallback;
        }

        /// <summary>
        /// Initializes ENet with specified memory allocator
        /// </summary>
        /// <param name="allocator">If this parameter receives null ENet will use its own heap allocator.</param>
        public static void Startup(ENetAllocator? allocator = null)
        {
            var startupOptions = new ENetStartupOptions
            {
                Allocator = allocator
            };

            Startup(startupOptions);
        }

        /// <summary>
        /// Initializes ENet with given startup options.
        /// </summary>
        /// <param name="startupOptions">The startup options.</param>
        public static void Startup(ENetStartupOptions startupOptions)
        {
            ThrowHelper.ThrowIfArgumentNull(startupOptions, nameof(startupOptions));
            startupOptions.CheckValues();

            var allocator = startupOptions.Allocator;

            if (Started) return;
            Started = true;

            if (startupOptions.ModulePath != null)
            {
                LibENet.Load(startupOptions.ModulePath);
            }
            else if (startupOptions.ModuleHandle != IntPtr.Zero)
            {
                LibENet.Load(startupOptions.ModuleHandle);
            }
            else
            {
                // Load from resource
                LibENet.Load();
            }

            var linkedVer = LibENet.LinkedVersion();
            s_LinkedVersion = new Version((int)(((linkedVer) >> 16) & 0xFF),
                                          (int)(((linkedVer) >> 8) & 0xFF),
                                          (int)((linkedVer) & 0xFF));

            if (allocator == null)
            {
                if (LibENet.Initialize() != 0)
                    ThrowHelper.ThrowENetInitializationFailed();
            }
            else
            {
                s_Allocator = allocator;

                NativeENetCallbacks callbacks = new NativeENetCallbacks
                {
                    Malloc = Marshal.GetFunctionPointerForDelegate(s_MemAllocDelegate),
                    Free = Marshal.GetFunctionPointerForDelegate(s_MemFreeDelegate),
                    NoMemory = Marshal.GetFunctionPointerForDelegate(s_NoMemoryDelegate)
                };

                if (LibENet.InitializeWithCallbacks(linkedVer, &callbacks) != 0)
                    ThrowHelper.ThrowENetInitializationFailed();
            }
        }

        /// <summary>
        /// Shutdowns and unloads ENet's library
        /// </summary>
        /// <param name="delete">Specifies whether the ENet dynamic library should be removed or not from disk.</param>
        /// <remarks>
        /// Any interaction with ENet managed wrapper instances like <see cref="ENetHost"/> should be avoided
        /// after calling this method. <br/>
        /// Parameter <paramref name="delete"/> is only considered when the library is loaded from the resources.
        /// </remarks>
        public static void Shutdown(bool delete = true)
        {
            if (!Started) return;
            Started = false;

            LibENet.Unload();
            if (delete) LibENet.TryDelete();

            s_Allocator?.Dispose();
            s_Allocator = null;
        }

        private static void NoMemoryCallback() => throw new OutOfMemoryException("ENet ran out of memory.");
        private static IntPtr MemAllocCallback(UIntPtr size)
        {
            var allocator = s_Allocator!;

            if (allocator != null)
            {
                return allocator.Allocate((int)size.ToUInt32());
            }
            else
            {
                ThrowHelper.ThrowENetAllocatorRefIsNull();
                return IntPtr.Zero;
            }
        }

        private static void MemFreeCallback(IntPtr memory)
        {
            var allocator = s_Allocator!;

            if (allocator != null)
            {
                allocator.Free(memory);
            }
            else
            {
                ThrowHelper.ThrowENetAllocatorRefIsNull();
            }
        }
    }
}
