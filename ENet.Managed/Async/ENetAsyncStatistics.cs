using System;
using System.Text;

using ENet.Managed.Internal;

namespace ENet.Managed.Async
{
    /// <summary>
    /// Represents a snapshot of general statistics of <see cref="ENetAsyncHost"/>.
    /// </summary>
    public sealed class ENetAsyncStatistics : IEquatable<ENetAsyncStatistics>
    {
        public int PeersCount { get; set; }
        public int DuplicatePeers { get; set; }
        public int ConnectedPeers { get; set; }
        public long TotalReceivedPackets { get; set; }
        public long TotalReceivedData { get; set; }
        public long TotalSentPackets { get; set; }
        public long TotalSentData { get; set; }


        /// <summary>
        /// Formats the statistics into a fancy string representation.
        /// </summary>
        /// <returns>A string of statistics.</returns>
        public string Format()
        {
            var sb = new StringBuilder(256);

            sb.AppendLine($"{nameof(PeersCount)}:           {PeersCount}");
            sb.AppendLine($"{nameof(DuplicatePeers)}:       {DuplicatePeers}");
            sb.AppendLine($"{nameof(ConnectedPeers)}:       {ConnectedPeers}");
            sb.AppendLine($"{nameof(TotalReceivedPackets)}: {TotalReceivedPackets}");
            sb.AppendLine($"{nameof(TotalSentPackets)}:     {TotalSentPackets}");
            sb.AppendLine($"{nameof(TotalReceivedData)}:    {TotalReceivedData}");
            sb.AppendLine($"{nameof(TotalSentData)}:        {TotalSentData}");

            return sb.ToString();
        }

        /// <summary>
        /// Compares the statistics with other instant.
        /// </summary>
        /// <param name="other">The other instant.</param>
        /// <returns>True if all statistics are equal; otherwise false.</returns>
        public bool Equals(ENetAsyncStatistics other)
        {
            ThrowHelper.ThrowIfArgumentNull(other, nameof(other));

            return
                PeersCount == other.PeersCount &&
                DuplicatePeers == other.DuplicatePeers &&
                ConnectedPeers == other.ConnectedPeers &&
                TotalReceivedPackets == other.TotalReceivedPackets &&
                TotalReceivedData == other.TotalReceivedData &&
                TotalSentPackets == other.TotalSentPackets &&
                TotalSentData == other.TotalSentData;
        }

        public override bool Equals(object obj)
        {
            if (obj is ENetAsyncStatistics)
            {
                return Equals(obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a hashcode of all statistics combined.
        /// </summary>
        /// <returns>A hashcode.</returns>
        public override int GetHashCode()
        {
            // HashCode.Combine() is not available on all frameworks.
            return
                PeersCount.GetHashCode() ^
                DuplicatePeers.GetHashCode() ^
                ConnectedPeers.GetHashCode() ^
                TotalReceivedPackets.GetHashCode() ^
                TotalReceivedData.GetHashCode() ^
                TotalSentPackets.GetHashCode() ^
                TotalSentData.GetHashCode();
        }
    }
}
