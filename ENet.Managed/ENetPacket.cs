using System;
using System.Runtime.CompilerServices;

using ENet.Managed.Internal;
using ENet.Managed.Native;

namespace ENet.Managed
{
    /// <summary>
    /// Wraps <see cref="NativeENetPacket"/> pointer
    /// </summary>
    public unsafe readonly struct ENetPacket : IEquatable<ENetPacket>
    {
        private readonly NativeENetPacket* m_Native;

        /// <summary>
        /// Indicates whether underlaying pointer is null or not
        /// </summary>
        public bool IsNull => m_Native == null;

        /// <summary>
        /// Packet data
        /// </summary>
        public Span<byte> Data => m_Native->GetDataAsSpan();

        /// <summary>
        /// Packet flags
        /// </summary>
        public ENetPacketFlags Flags
        {
            get
            {
                ThrowIfNull();

                return m_Native->Flags;
            }
            set
            {
                ThrowIfNull();

                m_Native->Flags = value;
            }
        }

        /// <summary>
        /// Pointer to user supplied data or tag to this packet.
        /// </summary>
        /// <remarks>
        /// <see cref="UserData"/> differs from <see cref="Data"/>.
        /// It's purpose is to allow you to tag the packet if needed.
        /// </remarks>
        public IntPtr UserData
        {
            get
            {
                ThrowIfNull();

                return m_Native->UserData;
            }
            set
            {
                ThrowIfNull();

                m_Native->UserData = value;
            }
        }

        /// <summary>
        /// Callback to be called after packet gets destoryed.
        /// </summary>
        public IntPtr FreeCallback
        {
            get
            {
                ThrowIfNull();

                return m_Native->FreeCallback;
            }

            set
            {
                ThrowIfNull();

                m_Native->FreeCallback = value;
            }
        }

        /// <summary>
        /// Number of references to this packet, initial value is 0
        /// </summary>
        public int ReferenceCount
        {
            get
            {
                ThrowIfNull();

                return unchecked((int)m_Native->ReferenceCount);
            }
        }

        /// <summary>
        /// Get underlaying pointer to <see cref="NativeENetPacket"/>.
        /// </summary>
        /// <returns>Pointer to <see cref="NativeENetPacket"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeENetPacket* GetNativePointer() => m_Native;

        public ENetPacket(NativeENetPacket* native)
        {
            if (native == null)
                throw new ArgumentNullException(nameof(native));

            m_Native = native;
        }

        public ENetPacket(int dataLength, ENetPacketFlags flags)
        {
            if (dataLength < 0)
                throw new ArgumentOutOfRangeException(nameof(dataLength));

            m_Native = LibENet.PacketCreate(IntPtr.Zero, unchecked((UIntPtr)dataLength), flags);
        }

        public ENetPacket(Span<byte> data, ENetPacketFlags flags)
        {
            fixed (byte* p = data)
            {
                m_Native = LibENet.PacketCreate(new IntPtr(p), unchecked((UIntPtr)data.Length), flags);
            }
        }

        /// <summary>
        /// Increments packet references count by one
        /// </summary>
        public void AddRef()
        {
            ThrowIfNull();

            m_Native->ReferenceCount = UIntPtr.Add(m_Native->ReferenceCount, 1);
        }

        /// <summary>
        /// Decerements packet references count by one and destroys the packet if count reaches zero
        /// </summary>
        public void RemoveRef()
        {
            ThrowIfNull();

            var newRefCount = UIntPtr.Subtract(m_Native->ReferenceCount, 1);

            if (newRefCount.ToUInt32() == 0)
            {
                Destroy();
            }
            else
            {
                m_Native->ReferenceCount = newRefCount;
            }
        }

        /// <summary>
        /// Resizes packet data length
        /// </summary>
        /// <param name="dataLength">New data length</param>
        /// <remarks>
        /// <see cref="Data"/> becomes invalid after calling this method.
        /// </remarks>
        public void Resize(int dataLength)
        {
            if (dataLength < 0)
                throw new ArgumentOutOfRangeException(nameof(dataLength));

            ThrowIfNull();

            if (LibENet.PacketResize(m_Native, unchecked((UIntPtr)dataLength)) < 0)
                ThrowHelper.ThrowENetPacketResizeFailed();
        }

        /// <summary>
        /// Destroys packet without considering references count
        /// </summary>
        public void Destroy()
        {
            ThrowIfNull();

            LibENet.PacketDestroy(m_Native);
        }

        private void ThrowIfNull()
        {
            if (IsNull)
                ThrowHelper.ThrowENetPacketPointerNull();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ENetPacket other)
        {
            return other.m_Native == m_Native;
        }

        public override bool Equals(object obj)
        {
            if (obj is ENetPacket packet)
            {
                return Equals(packet);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return new IntPtr(m_Native).GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ENetPacket left, ENetPacket right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ENetPacket left, ENetPacket right) => !left.Equals(right);
    }
}