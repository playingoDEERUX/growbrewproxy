// thanks to iProgramInCpp#0489, most things are made by him in the GrowtopiaCustomClient, I have just rewritten it into c# and maybe also improved. -playingo
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet.Managed;

namespace GrowbrewProxy
{
    public class PacketSending
    {
        private static Random rand = new Random();
        public static void SendData(byte[] data, ENetPeer peer, ENetPacketFlags flag = ENetPacketFlags.Reliable)
        {
            if (peer == null) return;
            if (peer.State != ENetPeerState.Connected) return;
            int loadb = rand.Next(0, 1);
            if (loadb == 0) peer.Send(data, 0, flag);
            else peer.Send(data, 0, flag);
        }

        public static void SendPacketRaw(int type, byte[] data, ENetPeer peer, ENetPacketFlags flag = ENetPacketFlags.Reliable)
        {           
            byte[] packetData = new byte[data.Length + 5];
            Array.Copy(BitConverter.GetBytes(type), packetData, 4);
            Array.Copy(data, 0, packetData, 4, data.Length);
            SendData(packetData, peer);
        }

        public static void SendPacket(int type, string str, ENetPeer peer, ENetPacketFlags flag = ENetPacketFlags.Reliable)
        {            
            SendPacketRaw(type, Encoding.ASCII.GetBytes(str.ToCharArray()), peer);            
        }

        public static void SecondaryLogonAccepted(ENetPeer peer)
        {
            SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, string.Empty, peer);
        }

        public static void InitialLogonAccepted(ENetPeer peer)
        {
            SendPacket((int)NetTypes.NetMessages.SERVER_HELLO, string.Empty, peer);
        }
    }
}
