using System;
using System.Runtime.InteropServices;

namespace ENet.Managed
{
    /// <summary>
    /// Set of extension methods for ENet.
    /// </summary>
    public static unsafe class ENetExtensionMethods
    {
        /// <summary>
        /// Sets or overwrites the user-data of the given packet.
        /// </summary>
        /// <typeparam name="TData">The user-data type.</typeparam>
        /// <param name="peer">The peer to store user-data into it's <see cref="ENetPeer.UserData"/> field.</param>
        /// <param name="data">The user-data.</param>
        public static void SetUserData<TData>(this ENetPeer peer, TData data)
        {
            var native = peer.GetNativePointer();
            SetUserData(ref native->Data, data);
        }

        /// <summary>
        /// Sets or overwrites the user-data of the given packet.
        /// </summary>
        /// <typeparam name="TData">The user-data type.</typeparam>
        /// <param name="packet">The packet to store user-data into it's <see cref="ENetPacket.UserData"/> field.</param>
        /// <param name="data">The user-data.</param>
        public static void SetUserData<TData>(this ENetPacket packet, TData data)
        {
            var native = packet.GetNativePointer();
            SetUserData(ref native->UserData, data);
        }

        /// <summary>
        /// Tries to retrieve user-data from <see cref="ENetPeer"/> of given type. 
        /// </summary>
        /// <typeparam name="TData">The data type.</typeparam>
        /// <param name="peer">The peer that holds the user-data.</param>
        /// <param name="data">The result user-data.</param>
        /// <returns>Returns true if the user-data found; otherwise false.</returns>
        /// <remarks>
        /// If the data type differs from what was supplied to <see cref="SetUserData{TData}(ENetPeer, TData)"/> then return value becomes false.
        /// </remarks>
        public static bool TryGetUserData<TData>(this ENetPeer peer, out TData data)
        {
            var native = peer.GetNativePointer();
            return TryGetUserData(native->Data, out data);
        }

        /// <summary>
        /// Tries to retrieve user-data from <see cref="ENetPeer"/> of given type. 
        /// </summary>
        /// <typeparam name="TData">The data type.</typeparam>
        /// <param name="packet">The packet that holds the user-data.</param>
        /// <param name="data">The result user-data.</param>
        /// <returns>Returns true if the user-data found; otherwise false.</returns>
        /// <remarks>
        /// If the data type differs from what was supplied to <see cref="SetUserData{TData}(ENetPacket, TData)"/> then return value becomes false.
        /// </remarks>
        public static bool TryGetUserData<TData>(this ENetPacket packet, out TData data)
        {
            var native = packet.GetNativePointer();
            return TryGetUserData(native->UserData, out data);
        }

        /// <summary>
        /// An extension method for clearing the user-data.
        /// </summary>
        /// <param name="peer">The peer that holds the user-data.</param>
        public static void UnsetUserData(this ENetPeer peer)
        {
            var native = peer.GetNativePointer();
            UnsetUserData(ref native->Data);
        }

        /// <summary>
        /// An extension method for clearing the user-data.
        /// </summary>
        /// <param name="packet">The packet that holds the user-data.</param>
        public static void UnsetUserData(this ENetPacket packet)
        {
            var native = packet.GetNativePointer();
            UnsetUserData(ref native->UserData);
        }

        private static void SetUserData<TData>(ref IntPtr dataField, TData data)
        {
            GCHandle gcHandle;
            ENetUserDataContainer<TData> container;
            if (dataField == IntPtr.Zero)
                goto allocNew;

            gcHandle = GCHandle.FromIntPtr(dataField);
            if (!gcHandle.IsAllocated)
                goto allocNew;

            if (gcHandle.Target is ENetUserDataContainer<TData> casted)
            {
                casted.Data = data;
                return;
            }
            else
            {
                gcHandle.Free();
                goto allocNew;
            }

        allocNew:
            container = new ENetUserDataContainer<TData>(data);
            gcHandle = GCHandle.Alloc(container, GCHandleType.Normal);
            dataField = GCHandle.ToIntPtr(gcHandle);
            return;
        }

        private static void UnsetUserData(ref IntPtr dataField)
        {
            if (dataField == IntPtr.Zero)
                return;

            var gcHandle = GCHandle.FromIntPtr(dataField);
            if (gcHandle.IsAllocated)
                gcHandle.Free();

            dataField = IntPtr.Zero;
        }

        private static bool TryGetUserData<TData>(IntPtr dataFieldValue, out TData data)
        {
            var container = TryGetUserDataContainer(dataFieldValue);
            if (container == null)
            {
                data = default!;
                return false;
            }

            if (container is ENetUserDataContainer<TData> castedContainer)
            {
                data = castedContainer.Data;
                return true;
            }

            if (container.GetData() is TData casted)
            {
                data = casted;
                return true;
            }

            data = default!;
            return false;
        }

        private static IENetUserDataContainer? TryGetUserDataContainer(IntPtr dataFieldValue)
        {
            if (dataFieldValue == IntPtr.Zero)
                return null;

            var gcHandle = GCHandle.FromIntPtr(dataFieldValue);
            if (!gcHandle.IsAllocated)
                return null;

            if (gcHandle.Target is IENetUserDataContainer casted)
                return casted;
            else
                return null;
        }
    }
}
