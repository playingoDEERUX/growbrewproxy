using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using ENet.Managed.Internal;
using ENet.Managed.Platforms;

#nullable disable // Notice!

namespace ENet.Managed.Native
{
    /// <summary>
    /// Provides methods and delegates to load/unload/call ENet library 
    /// </summary>
    public static unsafe class LibENet
    {
        public const CallingConvention ENetCallingConvention = CallingConvention.Cdecl;

        public const int MaximumPacketCommands = 32;
        public const int PeerUnsqeuencedWindowSize = 1024;
        public const int ProtocolMaximumPeerId = 0xFFF;

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int InitializeDelegate();
        public static InitializeDelegate Initialize { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int InitializeWithCallbacksDelegate(uint version, NativeENetCallbacks* callbacks);
        public static InitializeWithCallbacksDelegate InitializeWithCallbacks { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void DeinitializeDelegate();
        public static DeinitializeDelegate Deinitialize { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate uint LinkedVersionDelegate();
        public static LinkedVersionDelegate LinkedVersion { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int AddressGetHostDelegate(NativeENetAddress* address, IntPtr hostName, UIntPtr nameLength);
        public static AddressGetHostDelegate AddressGetHost { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int AddressGetHostIPDelegate(NativeENetAddress* address, IntPtr hostName, UIntPtr nameLength);
        public static AddressGetHostIPDelegate AddressGetHostIP { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int AddressSetHostDelegate(NativeENetAddress* address, [MarshalAs(UnmanagedType.LPStr)] string hostName);
        public static AddressSetHostDelegate AddressSetHost { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int AddressSetHostIPDelegate(NativeENetAddress* address, [MarshalAs(UnmanagedType.LPStr)] string hostName);
        public static AddressSetHostIPDelegate AddressSetHostIP { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void HostBandwidthLimitDelegate(IntPtr host, uint incomingBandwidth, uint outgoingBandwidth);
        public static HostBandwidthLimitDelegate HostBandwidthLimit { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void HostBroadcastDelegate(IntPtr host, byte channel, NativeENetPacket* packet);
        public static HostBroadcastDelegate HostBroadcast { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void HostChannelLimitDelegate(IntPtr host, UIntPtr channelLimit);
        public static HostChannelLimitDelegate HostChannelLimit { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public unsafe delegate int HostCheckEventsDelegate(IntPtr host, NativeENetEvent* e);
        public static HostCheckEventsDelegate HostCheckEvents { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void HostCompressDelegate(IntPtr host, NativeENetCompressor* compressor);
        public static HostCompressDelegate HostCompress { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int HostCompressWithRangeCoderDelegate(IntPtr host);
        public static HostCompressWithRangeCoderDelegate HostCompressWithRangeCoder { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate NativeENetPeer* HostConnectDelegate(IntPtr host, NativeENetAddress* address, UIntPtr channelCount, uint data);
        public static HostConnectDelegate HostConnect { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate IntPtr HostCreateDelegate(ENetAddressType addressType, NativeENetAddress* address, UIntPtr peerCount, UIntPtr channelLimit, uint incomingBandwidth, uint outgoingBandwidth);
        public static HostCreateDelegate HostCreate { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void HostDestroyDelegate(IntPtr host);
        public static HostDestroyDelegate HostDestroy { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void HostFlushDelegate(IntPtr host);
        public static HostFlushDelegate HostFlush { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int HostServiceDelegate(IntPtr host, NativeENetEvent* e, uint timeout);
        public static HostServiceDelegate HostService { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate uint Crc32Delegate(NativeENetBuffer* buffers, UIntPtr buffersCount);
        public static Crc32Delegate Crc32 { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate NativeENetPacket* PacketCreateDelegate(IntPtr data, UIntPtr dataLength, ENetPacketFlags flags);
        public static PacketCreateDelegate PacketCreate { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void PacketDestroyDelegate(NativeENetPacket* packet);
        public static PacketDestroyDelegate PacketDestroy;

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int PacketResizeDelegate(NativeENetPacket* packet, UIntPtr dataLength);
        public static PacketResizeDelegate PacketResize { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void PeerDisconnectDelegate(NativeENetPeer* peer, uint data);
        public static PeerDisconnectDelegate PeerDisconnect { get; private set; }
        public static PeerDisconnectDelegate PeerDisconnectNow { get; private set; }
        public static PeerDisconnectDelegate PeerDisconnectLater { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void PeerPingDelegate(NativeENetPeer* peer);
        public static PeerPingDelegate PeerPing { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void PeerPingIntervalDelegate(NativeENetPeer* peer, uint pingInterval);
        public static PeerPingIntervalDelegate PeerPingInterval { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate NativeENetPacket* PeerReceiveDelegate(NativeENetPeer* peer, byte* channelID);
        public static PeerReceiveDelegate PeerReceive { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void PeerResetDelegate(NativeENetPeer* peer);
        public static PeerResetDelegate PeerReset { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate int PeerSendDelegate(NativeENetPeer* peer, byte channelID, NativeENetPacket* packet);
        public static PeerSendDelegate PeerSend { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void PeerThrottleConfigureDelegate(NativeENetPeer* peer, uint interval, uint acceleration, uint deceleration);
        public static PeerThrottleConfigureDelegate PeerThrottleConfigure { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate void PeerTimeoutDelegate(NativeENetPeer* peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum);
        public static PeerTimeoutDelegate PeerTimeout { get; private set; }

        [UnmanagedFunctionPointer(ENetCallingConvention)]
        public delegate IntPtr InteropHelperSizeOrOffsetDelegate(uint id);
        public static InteropHelperSizeOrOffsetDelegate InteropHelperSizeOrOffset { get; private set; }

        enum LoadMode
        {
            NotLoaded,
            FromResource,
            CustomFile,
            CustomHandle,
        }

        private static LoadMode s_LoadMode = LoadMode.NotLoaded;

        /// <summary>
        /// Current loaded ENet dynamic library path
        /// </summary>
        public static string Path { get; set; } = GetTemporaryModulePath();

        /// <summary>
        /// Current loaded ENet dynamic library handle
        /// </summary>
        public static IntPtr Handle { get; private set; } = IntPtr.Zero;

        /// <summary>
        /// Indicates whether ENet dynamic libray is loaded or not
        /// </summary>
        public static bool IsLoaded { get; } = s_LoadMode != LoadMode.NotLoaded;

        /// <summary>
        /// Tries to delete ENet module.
        /// </summary>
        /// <remarks>
        /// This method only tries to delete the module if it was loaded from resources.
        /// </remarks>
        /// <returns>Return true if the delete operation was successful; otherwise false.</returns>
        public static bool TryDelete()
        {
            if (s_LoadMode != LoadMode.FromResource) 
                return false;

            try
            {
                File.Delete(Path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Unloads ENet module if loaded.
        /// </summary>
        public static void Unload()
        {
            if (IsLoaded == false)
                return;

            try
            {
                Deinitialize?.Invoke();

                Platform.Current.FreeDynamicLibrary(Handle);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Got exception {ex.GetType().Name} when deinitializing and freeing ENet library.");
            }

            Handle = IntPtr.Zero;
            Path = string.Empty;
            s_LoadMode = LoadMode.NotLoaded;
        }

        /// <summary>
        /// Loads appropriate ENet module for the current OS from resources.
        /// </summary>
        [Obsolete("Consider using " + nameof(ENetStartupOptions))]
        public static void Load()
        {
            if (IsLoaded) return;

            try
            {
                LoadModuleFromResource(overwrite: false);
            }
            catch
            {
                LoadModuleFromResource(overwrite: true);
            }
        }

        /// <summary>
        /// Loads ENet from the given library handle.
        /// </summary>
        /// <param name="dllHandle">The DLL handle.</param>
        [Obsolete("Consider using " + nameof(ENetStartupOptions))]
        public static void Load(IntPtr dllHandle)
        {
            if (IsLoaded) return;

            LoadModuleFromHandle(dllHandle);
        }

        /// <summary>
        /// Load ENet from the given shared library path.
        /// </summary>
        /// <param name="dllFile">The DLL file path.</param>
        [Obsolete("Consider using " + nameof(ENetStartupOptions))]
        public static void Load(string dllFile)
        {
            if (IsLoaded) return;

            LoadModuleFromFile(dllFile);
        }

        public static T GetProc<T>(string procName)
        {
            return (T)(object)Marshal.GetDelegateForFunctionPointer(GetProc(procName), typeof(T));
        }

        public static IntPtr GetProc(string procName)
        {
            if (Handle == IntPtr.Zero)
                ThrowHelper.ThrowENetLibraryNotLoaded();

            return Platform.Current.GetDynamicLibraryProcedureAddress(Handle, procName);
        }

        static void LoadModuleFromResource(bool overwrite)
        {
            var path = GetTemporaryModulePath();

            if (!File.Exists(path) || overwrite)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.WriteAllBytes(path, Platform.Current.GetENetBinaryBytes());
            }

            Handle = LoadDllFromFile(path);
            Path = path;

            try
            {
                LoadModuleProcs();
                s_LoadMode = LoadMode.FromResource;
            }
            catch
            {
                Handle = IntPtr.Zero;
                Path = null;
                throw;
            }
        }

        static void LoadModuleFromHandle(IntPtr handle)
        {
            Handle = handle;

            try
            {
                LoadModuleProcs();
                s_LoadMode = LoadMode.CustomHandle;
            }
            catch
            {
                Handle = IntPtr.Zero;
                Path = null;
                throw;
            }
        }

        static void LoadModuleFromFile(string path)
        {
            Handle = LoadDllFromFile(path);
            Path = string.Empty;

            try
            {
                LoadModuleProcs();
                s_LoadMode = LoadMode.CustomFile;
            }
            catch
            {
                Handle = IntPtr.Zero;
                Path = null;
                throw;
            }
        }

        static void LoadModuleProcs()
        {
            Initialize = GetProc<InitializeDelegate>("enet_initialize");
            InitializeWithCallbacks = GetProc<InitializeWithCallbacksDelegate>("enet_initialize_with_callbacks");
            Deinitialize = GetProc<DeinitializeDelegate>("enet_deinitialize");
            LinkedVersion = GetProc<LinkedVersionDelegate>("enet_linked_version");

            AddressGetHost = GetProc<AddressGetHostDelegate>("enet_address_get_host");
            AddressGetHostIP = GetProc<AddressGetHostIPDelegate>("enet_address_get_host_ip");
            AddressSetHost = GetProc<AddressSetHostDelegate>("enet_address_set_host");
            AddressSetHostIP = GetProc<AddressSetHostIPDelegate>("enet_address_set_host_ip");

            HostBandwidthLimit = GetProc<HostBandwidthLimitDelegate>("enet_host_bandwidth_limit");
            //HostBandwidthThrottle = GetProc<ENetHostBandwidthThrottleDelegate>("enet_host_bandwidth_throttle");
            HostBroadcast = GetProc<HostBroadcastDelegate>("enet_host_broadcast");
            HostChannelLimit = GetProc<HostChannelLimitDelegate>("enet_host_channel_limit");
            HostCheckEvents = GetProc<HostCheckEventsDelegate>("enet_host_check_events");
            HostCompress = GetProc<HostCompressDelegate>("enet_host_compress");
            HostCompressWithRangeCoder = GetProc<HostCompressWithRangeCoderDelegate>("enet_host_compress_with_range_coder");
            HostConnect = GetProc<HostConnectDelegate>("enet_host_connect");
            HostCreate = GetProc<HostCreateDelegate>("enet_host_create");
            HostDestroy = GetProc<HostDestroyDelegate>("enet_host_destroy");
            HostFlush = GetProc<HostFlushDelegate>("enet_host_flush");
            HostService = GetProc<HostServiceDelegate>("enet_host_service");

            Crc32 = GetProc<Crc32Delegate>("enet_crc32");
            PacketCreate = GetProc<PacketCreateDelegate>("enet_packet_create");
            PacketDestroy = GetProc<PacketDestroyDelegate>("enet_packet_destroy");
            PacketResize = GetProc<PacketResizeDelegate>("enet_packet_resize");

            PeerDisconnect = GetProc<PeerDisconnectDelegate>("enet_peer_disconnect");
            PeerDisconnectLater = GetProc<PeerDisconnectDelegate>("enet_peer_disconnect_later");
            PeerDisconnectNow = GetProc<PeerDisconnectDelegate>("enet_peer_disconnect_now");
            PeerPing = GetProc<PeerPingDelegate>("enet_peer_ping");
            PeerPingInterval = GetProc<PeerPingIntervalDelegate>("enet_peer_ping_interval");
            PeerReceive = GetProc<PeerReceiveDelegate>("enet_peer_receive");
            PeerReset = GetProc<PeerResetDelegate>("enet_peer_reset");
            PeerSend = GetProc<PeerSendDelegate>("enet_peer_send");
            PeerThrottleConfigure = GetProc<PeerThrottleConfigureDelegate>("enet_peer_throttle_configure");
            PeerTimeout = GetProc<PeerTimeoutDelegate>("enet_peer_timeout");

            InteropHelperSizeOrOffset = GetProc<InteropHelperSizeOrOffsetDelegate>("enet_interophelper_sizeoroffset");
        }

        static IntPtr LoadDllFromFile(string path)
        {
            return Platform.Current.LoadDynamicLibrary(path);
        }

        static string GetTemporaryModulePath()
        {
            var dllName = Platform.Current.GetENetBinaryName();
            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), "enet_managed_resource", dllName);
        }
    }
}
