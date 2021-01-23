using System;
using System.Collections;
using System.Collections.Generic;

namespace ENet.Managed
{
    /// <summary>
    /// Represents ENet host's allocated peers.
    /// </summary>
    public sealed class ENetHostPeerList : IReadOnlyList<ENetPeer>
    {
        public ENetHost Host { get; }
        public int Count => Host.PeersCount;
        public ENetPeer this[int index]
        {
            get
            {
                unsafe
                {
                    if (index < 0 || Host.PeersCount <= index)
                        throw new ArgumentOutOfRangeException("Peer index is out of range.");

                    return UnsafeGetPeerByIndex(index);
                }
            }
        }

        internal ENetHostPeerList(ENetHost host)
        {
            Host = host;
        }

        public IEnumerator<ENetPeer> GetEnumerator() => EnumPeers();
        IEnumerator IEnumerable.GetEnumerator() => EnumPeers();

        private IEnumerator<ENetPeer> EnumPeers()
        {
            var count = Count;
            for (int i = 0; i < count; i++)
            {
                Host.CheckDispose();
                yield return UnsafeGetPeerByIndex(i);
            }
        }

        private unsafe ENetPeer UnsafeGetPeerByIndex(int index)
        {
            return new ENetPeer(&Host.PeersStartPtr[index]);
        }
    }
}
