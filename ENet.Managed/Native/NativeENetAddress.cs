using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using ENet.Managed.Internal;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NativeENetAddress
    {
        [FieldOffset(0)]
        public ENetAddressType Type;

        [FieldOffset(4)]
        public ushort Port;

        [FieldOffset(6)]
        public fixed byte V4[4];

        [FieldOffset(6)]
        public fixed byte V6[16];

        public IPEndPoint ToIPEndPoint()
        {
            if (Type == ENetAddressType.Any)
            {
                return new IPEndPoint(IPAddress.Any, Port);
            }

            if (Type == ENetAddressType.IPv4)
            {
                fixed (byte* v4 = V4)
                {
                    var ipBytes = new ReadOnlySpan<byte>(v4, 4).ToArray();
                    var ip = new IPAddress(ipBytes);
                    return new IPEndPoint(ip, Port);
                }
            }

            if (Type == ENetAddressType.IPv6)
            {
                fixed (byte* v6 = V6)
                {
                    var ipBytes = new ReadOnlySpan<byte>(v6, 16).ToArray();
                    var ip = new IPAddress(ipBytes);
                    return new IPEndPoint(ip, Port);
                }
            }

            throw new NotSupportedException($"Address of type {Type} is not supported.");
        }

        public static NativeENetAddress FromIPEndPoint(IPEndPoint endPoint)
        {
            ThrowHelper.ThrowIfArgumentNull(endPoint, nameof(endPoint));

            var result = default(NativeENetAddress);

            switch (endPoint.Address.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    result.Type = ENetAddressType.IPv4;
                    result.Port = (ushort)endPoint.Port;

                    var resultIp4 = new Span<byte>(result.V4, 4);
                    endPoint.Address.GetAddressBytes().AsSpan().CopyTo(resultIp4);
                    break;

                case AddressFamily.InterNetworkV6:
                    result.Type = ENetAddressType.IPv6;
                    result.Port = (ushort)endPoint.Port;

                    var resultIp6 = new Span<byte>(result.V6, 16);
                    endPoint.Address.GetAddressBytes().AsSpan().CopyTo(resultIp6);
                    break;

                default:
                    throw new NotSupportedException($"Address Family {endPoint.AddressFamily} not supported.");
            }

            return result;
        }
    }
}
