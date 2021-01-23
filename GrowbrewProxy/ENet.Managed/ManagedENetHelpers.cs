using System;
using System.Linq;
using System.Net;

namespace ENet.Managed
{
    /// <summary>
    /// Set of helper methods for ENet library.
    /// </summary>
    public static class ManagedENetHelpers
    {
        /// <summary>
        /// Attempts to turn [IP/HOST]:[PORT] style string into <see cref="IPEndPoint"/>
        /// </summary>
        /// <returns>Returns true if succeed otherwise false</returns>
        [Obsolete("This method is obselete; consider using IPEndPoint.Parse().")]
        public static bool TryGetIPEndPoint(string hostPort, out IPEndPoint endPoint)
        {
            if (hostPort == null)
                throw new ArgumentNullException(nameof(hostPort));

            var split = hostPort.Trim().Split(':');

            if (split.Length != 2)
                goto return_false;

            var hostString = split[0];
            var portString = split[1];

            if (!IPAddress.TryParse(hostString, out var address))
            {
                try
                {
                    address = Dns.GetHostEntry(hostString).AddressList.First();
                }
                catch
                {
                    goto return_false;
                }
            }

            if (!ushort.TryParse(portString, out var port))
                goto return_false;

            endPoint = new IPEndPoint(address, port);
            return true;

        return_false:
            endPoint = null!;
            return false;
        }
    }
}
