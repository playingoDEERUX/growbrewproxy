using System;
using System.Collections.Generic;
using System.Text;

using ENet.Managed.Allocators;
using ENet.Managed.Internal;

namespace ENet.Managed
{
    /// <summary>
    /// Set of options describing ENet's initialization and startup.
    /// </summary>
    public sealed class ENetStartupOptions
    {
        /// <summary>
        /// Allocator to be given to ENet. 
        /// Defaults to null, meaning ENet's default allocator will be used.
        /// </summary>
        public ENetAllocator? Allocator { get; set; }

        /// <summary>
        /// Path to ENet's shared library (.dll, .so, .dylib).
        /// Defaults to null, meaning load appropriate one from resources.
        /// </summary>
        public string? ModulePath { get; set; } = null;

        /// <summary>
        /// Native ENet's shared library module's handle.
        /// It is retrieved with GetModuleHandle or LoadLibrary on Windows.
        /// Defaults to <see cref="IntPtr.Zero"/>, meaning load appropriate one from resources. 
        /// </summary>
        public IntPtr ModuleHandle { get; set; } = IntPtr.Zero;

        internal void CheckValues()
        {
            if (ModulePath != null && ModuleHandle != IntPtr.Zero)
                throw new ArgumentException($"Both {nameof(ModulePath)} and {nameof(ModuleHandle)} properties are supplied, it's ambiguous to use which one.");
        }
    }
}
