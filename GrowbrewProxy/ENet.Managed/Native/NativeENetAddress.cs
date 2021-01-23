using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using ENet.Managed.Internal;

namespace ENet.Managed.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeENetAddress
    {
        public uint Host;
        public ushort Port;
        public ushort Padding;

        public IPEndPoint ToIPEndPoint()
        {
            var address = new IPAddress(Host);
            return new IPEndPoint(address, Port);
        }

        public static NativeENetAddress FromIPEndPoint(IPEndPoint endPoint)
        {
            if (endPoint.AddressFamily != AddressFamily.InterNetwork)
                throw new NotSupportedException(string.Format("Address Family {0} not supported", endPoint.AddressFamily));

            NativeENetAddress address = new NativeENetAddress();
#pragma warning disable CS0618 // Type or member is obsolete
            address.Host = (uint)endPoint.Address.Address;
#pragma warning restore CS0618 // Type or member is obsolete
            address.Port = (ushort)endPoint.Port;
            return address;
        }
    }
}
