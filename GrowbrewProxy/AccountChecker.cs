using ENet.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GrowbrewProxy
{
    public class AccountChecker
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        public static bool SaveGemCount8K = false;
        public static bool SaveGrowtokenCount9 = false;
        public static bool SaveWLCountOver10 = false;

        public static int Growtopia_Port = MainForm.Growtopia_Master_Port; // todo auto get port
        public static string Growtopia_IP = MainForm.Growtopia_Master_IP;
        public static MainForm.AccountTable[] accountsToCheck;
        private static ENetHost g_Client;
        private static ENetPeer g_Peer;

        internal static int leftToCheckIndex;
        internal static int checkCurrentIndex = 0;
        private static PacketSending packetSender = MainForm.messageHandler.packetSender;
        private static void Peer_OnReceive_Client(object sender, ENetPacket e)
        {
            try
            {
                // this is a specific, external client made only for the purpose of using the TRACK packet for our gains/advantage in order to check all accounts quick and efficiently.
                byte[] packet = e.GetPayloadFinal();
                Console.WriteLine("RECEIVE TYPE: " + packet[0].ToString());
                switch (packet[0])
                {
                    case 1: // HELLO server packet.
                        {
                            string username = accountsToCheck[checkCurrentIndex].GrowID;
                            string pass = accountsToCheck[checkCurrentIndex].password;
                            // todo add mac optionally, will do that incases aap bypass gets fixed.
                            Console.WriteLine("[ACCOUNT-CHECKER] Logging on " + username + "...");

                            packetSender.SendPacket(2, MainForm.CreateLogonPacket(username, pass), g_Peer);
                            break;
                        }
                    case 2:
                    case 3:
                        {
                            Console.WriteLine("[ACCOUNT-CHECKER] TEXT PACKET CONTENT:\n" + Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                            break;
                        }
                    case 4:
                        {
                            byte[] tankPacket = VariantList.get_struct_data(packet);
                            if (tankPacket[0] == 1)
                            {
                                VariantList.VarList vList = VariantList.GetCall(VariantList.get_extended_data(tankPacket));
                                vList.netID = BitConverter.ToInt32(tankPacket, 4); // add netid
                                vList.delay = BitConverter.ToUInt32(tankPacket, 20); // add keep track of delay modifier

                                // Console.WriteLine(VarListFetched.FunctionName);
                                if (vList.FunctionName == "OnSendToServer")
                                {
                                    string ip = (string)vList.functionArgs[4];

                                    if (ip.Contains("|"))
                                        ip = ip.Substring(0, ip.IndexOf("|"));

                                    int port = (int)vList.functionArgs[1];
                                    int userID = (int)vList.functionArgs[3];
                                    int token = (int)vList.functionArgs[2];
                                    int lmode = (int)vList.functionArgs[5];
                                    Growtopia_IP = ip;
                                    Growtopia_Port = port;
                                    ConnectCurrent();
                                }
                                // variant call, just rn used for subserver switching
                            }
                            break;
                        }
                    case (byte)NetTypes.NetMessages.TRACK: // TRACK packet.
                        {
                            Console.WriteLine("[ACCOUNT-CHECKER] TRACK PACKET CONTENT:\n" + Encoding.ASCII.GetString(packet.Skip(4).ToArray()));
                            checkCurrentIndex++;
                            Console.WriteLine("[ACCOUNT-CHECKER] +1 account checked, disconnecting and moving onto the next one.");
                            Growtopia_Port = MainForm.Growtopia_Master_Port; // todo auto get port
                            Growtopia_IP = MainForm.Growtopia_Master_IP;
                            ConnectCurrent();
                            break;
                        }
                    default:
                        break;
                }
            } 
            catch
            {
                
            }
        }
        private static void Peer_OnDisconnect_Client(object sender, uint e)
        {
            
            // dc
            // MainForm.hasLogonAlready = false;
            Console.WriteLine("[ACCOUNT-CHECKER] Disconnected from GT Server(s)!");
        }
        private static void Client_OnConnect(object sender, ENetConnectEventArgs e)
        {
            e.Peer.OnReceive += Peer_OnReceive_Client;
            e.Peer.OnDisconnect += Peer_OnDisconnect_Client;
            e.Peer.PingInterval(1000);
            e.Peer.Timeout(1000, 9000, 13000);

            Console.WriteLine("[ACCOUNT-CHECKER] Successfully connected to GT Server(s)!");
        }
        public static bool Initialize()
        {
            // Setting up ENet-Client ->
            if (g_Client == null)
            {
                AllocConsole();
                Console.WriteLine("[ACCOUNT-CHECKER] Account Checker Bot/Client (C) 2020 playingo (aka DEERUX), github.com/playingoDEERUX/growbrewproxy\n" +
                    "DO NOT CLOSE THIS WINDOW, OTHERWISE THE ENTIRE PROXY WILL CLOSE (except do it when you wanna exit from it)! \n" +
                    "Although, you can still click on stop checking accounts to gain performance for proxy-only again.");

                if (accountsToCheck == null)
                {
                    Console.WriteLine("[ACCOUNT-CHECKER] ERROR: Could not start Account Checking, accountsToCheck list was null.");
                    return false;
                }

                if (accountsToCheck.Count() <= 0)
                {
                    Console.WriteLine("[ACCOUNT-CHECKER] ERROR: Could not start Account Checking, there were no accounts loaded.");
                    return false;
                }

                leftToCheckIndex = accountsToCheck.Count() - 1;
                g_Client = new ENetHost(1, 2);
                g_Client.OnConnect += Client_OnConnect;
                g_Client.ChecksumWithCRC32();
                g_Client.CompressWithRangeCoder();
                g_Client.StartServiceThread();
                Console.WriteLine("[ACCOUNT-CHECKER] Initialized Global Client Host and started service thread!\n" +
                    "\nClick 'Connect and check all accounts' to start checking!");
            }
            return true;
        }
        public static void ConnectCurrent()
        {
            if (g_Client == null) return;

            if (g_Client.ServiceThreadStarted)
            {

                if (g_Peer == null)
                {
                    g_Peer = g_Client.Connect(new System.Net.IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
                }
                else if (g_Peer.State == ENetPeerState.Connected)
                {
                    g_Peer.Reset();

                    g_Peer = g_Client.Connect(new System.Net.IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
                }
            }
        }
    }
}
