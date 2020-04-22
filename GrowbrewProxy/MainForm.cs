// (C) Made, programmed and designed by PlayIngoHD/PlayIngoHD Gaming/playingo/DEERUX and iProgramInCpp/iProgramMC only.
// Reselling this is illegal, because this is free-opensource-ware and credits are appreciated :)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ENet.Managed;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using GrowbrewProxy;
using System.Diagnostics;
using System.Net.Sockets;
using Kernys.Bson;

namespace GrowbrewProxy
{
    public partial class MainForm : Form
    {

        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 1024;
            // Receive buffer.  
            public byte[] buffer = new byte[BufferSize];
            // Received data string.  
            public StringBuilder sb = new StringBuilder();
        }

        public struct UserData
        {
            public string username;
            public int paddingInt;
        }

        
        public static byte GrowbrewHNetVersion = 1;
        static bool isHTTPRunning = false;
        public static PlayerForm pForm = new PlayerForm();

        public static UserData userInfo = new UserData();

        public static bool skipCache = false;

        public static TcpClient tClient = new TcpClient();
        public static StateObject stateObj = new StateObject();


        public static string LogText = string.Empty;

        private delegate void SafeCallDelegate(string text);

        private static ENetHost client;
        private ENetHost m_Host;

        public static ENetPeer realPeer;
        public static ENetPeer proxyPeer;

        public bool mayContinue = false;
        public bool srvRunning = false;
        public bool clientRunning = false;
        public static int Growtopia_Port = 17126; // todo auto get port
        public static string Growtopia_IP = "209.59.191.76";
        public static bool isSwitchingServer = false;
        public static bool blockEnterGame = false;


        // internal variables =>
        public static string tankIDName = "";
        public static string tankIDPass = "";
        public static string game_version = "3.32";
        public static string country = "de";
        public static string requestedName = "";
        public static int token = 0;
        public static bool resetStuffNextLogon = false;
        public static int userID = 0;
        public static int lmode = 0;
        public static byte[] skinColor = new byte[4];
        public static bool hasLogonAlready = false;
        public static bool hasUpdatedItemsAlready = false;
        public static bool bypassAAP = false;
        public static bool ghostSkin = false;
        // CHEAT VARS/DEFS
        public static bool cheat_magplant = false;
        public static bool cheat_rgbSkin = false;
        public static bool cheat_autoworldban_mod = false;
        public static bool cheat_speedy = false;
        // CHEAT VARS/DEFS
        public static string macc = "02:15:01:20:30:05";
        public static string doorid = "";
        ItemDatabase itemDB = new ItemDatabase();

        public static HandleMessages messageHandler = new HandleMessages();

        public MainForm()
        {
            InitializeComponent();
        }

        public static string GenerateRID()
        {
            string str = "0";
            Random random = new Random();
            const string chars = "ABCDEF0123456789";
            str += new string(Enumerable.Repeat(chars, 31)
               .Select(s => s[random.Next(s.Length)]).ToArray());
            return str;
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string GenerateUniqueWinKey()
        {
            string str = "7";
            Random random = new Random();
            const string chars = "ABCDEF0123456789";
            str += new string(Enumerable.Repeat(chars, 31)
               .Select(s => s[random.Next(s.Length)]).ToArray());
            return str;
        }

        public static string GenerateMACAddress()
        {
            Random rand = new Random();
            byte[] macAddr = new byte[6];
            rand.NextBytes(macAddr);
            macAddr[0] = (byte)(macAddr[0] & (byte)254);  //zeroing last 2 bytes to make it unicast and locally adminstrated
            StringBuilder sb = new StringBuilder(18);
            foreach (byte b in macAddr)
            {

                if (sb.Length > 0)
                    sb.Append(":");

                sb.Append(String.Format("%02x", b));
            }
            string str = sb.ToString();
            str = str.ToLower();
            return str;
        }
        
        public static string CreateLogonPacket(bool hasGrowId = false)
        {
            string p = string.Empty;
            Random rand = new Random();
            bool requireAdditionalData = false; if (token > 0 || token < 0) requireAdditionalData = true;

            if (hasGrowId)
            {
                p += "tankIDName|" + (tankIDName + "\n");
                p += "tankIDPass|" + (tankIDPass + "\n");
            }
            p += "requestedName|" + ("Growbrew" + rand.Next(0, 255).ToString() + "\n"); //"Growbrew" + rand.Next(0, 255).ToString() + "\n"
            p += "f|1\n";
            p += "protocol|94\n";
            p += "game_version|" + (game_version + "\n");
            if (requireAdditionalData) p += "lmode|" + lmode + "\n";
            p += "cbits|0\n";
            p += "player_age|100\n";
            p += "GDPR|1\n";
            p += "hash2|231337357\n";
            p += "meta|localhost\n"; // soon auto fetch meta etc.
            p += "fhash|-716928004\n";
            p += "platformID|4\n";
            p += "deviceVersion|0\n";
            p += "country|" + (country + "\n");
            p += "hash|-481288825\n";
            p += "mac|" + macc + "\n";
            if (requireAdditionalData) p += "user|" + (userID.ToString() + "\n");
            if (requireAdditionalData) p += "token|" + (token.ToString() + "\n");
            if (doorid != "") p += "doorID|" + doorid.ToString() + "\n";
            p += "wk|" + ("NONE0\n");
            //p += "zf|-1576181843";
            return p;
        }

        void AppendLog(string text)
        {
            if (text == string.Empty) return;
            if (logBox.InvokeRequired)
                logBox.Invoke(new SafeCallDelegate(AppendLog), new object[] { text });
            else logBox.Text += (text + "\n");
        }

        void UpdateUserCount(int count)
        {
            if (count == 0) return;

            if (userscountlabel.InvokeRequired)
                userscountlabel.Invoke((MethodInvoker)(() => userscountlabel.Text = count.ToString()));
            else userscountlabel.Text = count.ToString();
        }

        void AppendChat(string text)
        {
            if (text == string.Empty) return;
            if (chatcontent.InvokeRequired)
                chatcontent.Invoke(new SafeCallDelegate(AppendChat), new object[] { text });
            else chatcontent.Text += (text + "\n");
        }

        public static void ConnectToServer()
        {
            
            if (realPeer == null)
            {
                realPeer = client.Connect(new IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 1, 0);
            }
            else
            {
                if (realPeer.State != ENetPeerState.Connected)
                {
                    realPeer = client.Connect(new IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 1, 0);
                }
                else
                {
                    PacketSending.SendPacket(3, "action|quit", realPeer);
                    // sub server switching, most likely.
                    realPeer = client.Connect(new IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 1, 0);
                }
            }            
        }

        private void Host_OnConnect(object sender, ENetConnectEventArgs e)
        {

            proxyPeer = e.Peer;
            e.Peer.OnReceive += Peer_OnReceive;
            e.Peer.OnDisconnect += Peer_OnDisconnect;
           
            

            AppendLog("A new client connected from {0} " + e.Peer.RemoteEndPoint);
            e.Peer.Timeout(30000, 25000, 30000);
          
            AppendLog("Connecting to gt servers...");
            ConnectToServer();

        }

        private void Peer_OnDisconnect(object sender, uint e)
        {

            //if (((ENetPeer)sender) != null) ((ENetPeer)sender).Host.Dispose();
            //token = 0;
           // userID = 0;
            //lmode = 0;
            messageHandler.enteredGame = false;            
            AppendLog("An internal disconnection was triggered in the proxy, you may want to reconnect your GT Client if you are not being disconnected by default (maybe because of sub-server switching?)");
        }

        private void Peer_OnReceive(object sender, ENetPacket e)
        {
            /*if (proxyPeer == null || realPeer == null) return;
            if ((proxyPeer.State != ENetPeerState.Connected || proxyPeer.Data == null) ||
               (realPeer.State != ENetPeerState.Connected || proxyPeer.Data == null)) return;*/
            //use checkPeerUsability if you wanna check for peer, since this is an open source freeware, I will not have to for now.

            //messageHandler.HandlePacketFromClient(e);
            
            string str = messageHandler.HandlePacketFromClient(e);
            if (str != "_none_") AppendLog(str);
        }

        private void Peer_OnReceive_Client(object sender, ENetPacket e)
        {
            string str = messageHandler.HandlePacketFromServer(e);
            if (str != "_none_") AppendLog(str);
        }

        private void Peer_OnDisconnect_Client(object sender, uint e)
        {
            var peer = sender as ENetPeer;
            var data = e;

           // MainForm.hasLogonAlready = false;
            
        }


        void loadLogs(bool requireReloadFromFile = false)
        {            
            if (requireReloadFromFile)
            {
                LogText = File.ReadAllText("debuglog.txt");
                entireLog.Text = LogText;
                return;
            }
            entireLog.Text = LogText;
        }
        

        private void Client_OnConnect(object sender, ENetConnectEventArgs e)
        {                     
            e.Peer.OnReceive += Peer_OnReceive_Client;
            e.Peer.OnDisconnect += Peer_OnDisconnect_Client;
            

            AppendLog("The growtopia client just connected successfully.");
        }


        void LaunchProxy()
        {
            if (!srvRunning)
            {
                srvRunning = true;
                clientRunning = true;

                // Setting up ENet-Server ->
                m_Host = new ENetHost(new IPEndPoint(IPAddress.Any, 2), 512, 10); // allow only 1 peer to be connected at the same time
                m_Host.OnConnect += Host_OnConnect;
                m_Host.ChecksumWithCRC32();
                m_Host.CompressWithRangeCoder();
                m_Host.StartServiceThread();
                
                // Setting up ENet-Client ->
                client = new ENetHost(512, 10);
                client.OnConnect += Client_OnConnect;
                client.ChecksumWithCRC32();
                client.CompressWithRangeCoder();
                client.StartServiceThread();


                // Setting up controls
                runproxy.Enabled = false; // too lazy to make it so u can disable it via button
                labelsrvrunning.Text = "Server is running!";
                labelsrvrunning.ForeColor = Color.Green;
                labelclientrunning.Text = "Client is running!";
                labelclientrunning.ForeColor = Color.Green;
            }
        }

        private void runproxy_Click(object sender, EventArgs e)
        {
            if (ipaddrBox.Text != "" && portBox.Text != "")
            {
                Growtopia_IP = ipaddrBox.Text;
                Growtopia_Port = Convert.ToInt32(portBox.Text);
            }
            LaunchProxy();
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;
                if (client == null) return;
                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                

                // Signal that all bytes have been sent.  
                
            }
            catch (Exception e)
            {
                
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket tClient = (Socket)ar.AsyncState;

                stateObj.workSocket = tClient;

                BSONObject request = new BSONObject();
                request["msg"] = "auth";
                request["note"] = "(none)";
                request["mID"] = HardwareID.GetHwid();


                byte[] requestData = SimpleBSON.Dump(request);


                tClient.BeginSend(requestData, 0, requestData.Length, SocketFlags.None, new AsyncCallback(SendCallback), stateObj);

                tClient.BeginReceive(stateObj.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), stateObj);
            }
            catch
            {

            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                if (bytesRead > 0)
                {
                    if (bytesRead < 1024)
                    {
                        byte[] data = new byte[bytesRead];
                        Array.Copy(state.buffer, data, bytesRead);

                        BSONObject bObj = SimpleBSON.Load(data);
                        string message = bObj["msg"].stringValue;
                        string note = bObj["note"].stringValue;


                        switch (message)
                        {
                            case "auth":
                                {
                                    int authState = bObj["auth_state"].int32Value;
                                    userInfo.username = bObj["username"].stringValue;



                                    if (authState == 0)
                                    {
                                        
                                        MessageBox.Show(note, "Growbrew Server");

                                    }
                                    else
                                    {
                                        //Environment.Exit(authState);
                                    }
                                    break;
                                }
                            case "get_online":
                                {
                                    int count = bObj["c"];
                                    UpdateUserCount(count);
                                    break;
                                }
                            case "chat_res":
                                {
                                    AppendChat(note);
                                    break;
                                }

                            default:
                                break;
                        }
                    }
                    tClient.Client.BeginReceive(stateObj.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), stateObj);
                }
            }
            catch
            {

            }
        }

      
        private void MainForm_Load(object sender, EventArgs e)
        {


            DialogResult dr = MessageBox.Show("Proceeding will connect you to the Growbrew Network!\nGROWBREW MAY USE ANY OF YOUR HARDWARE IDENTIFIERS AND YOUR IP WHICH ARE USED TO SECURE THE PRODUCT E.G FOR BANS AND ANTI-CRACK SOLUTIONS! \nRead more in 'Growbrew Policies'\nContinue?", "Growbrew Proxy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.No)
            {
                Environment.Exit(-1);
            }

            
            tClient.BeginConnect(IPAddress.Parse("89.47.163.53"), 6770, new AsyncCallback(ConnectCallback), tClient.Client);
            

            playerLogicUpdate.Start();
            itemDB.SetupItemDefs();
            LibENet.Load();
            LibENet.Initialize();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
                // deinitialization might be required here, decide for your self, I get weird service failure errors due to that, too lazy to fix.
                srvRunning = false;
                clientRunning = false;

            try
            {
                BSONObject bObj = new BSONObject();
                bObj["msg"] = "quit";
                bObj["note"] = "Goodbye.";
                byte[] data = SimpleBSON.Dump(bObj);

                tClient.Client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), stateObj.workSocket);
            }
            catch
            { 

            }
            Environment.Exit(0);
            
        }

        private void logBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private void formTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ActiveForm == null) return;
                if (formTabs.SelectedTab == formTabs.TabPages["proxyPage"])
                    ActiveForm.Text = "Growbrew Proxy - Main Page";
                else if (formTabs.SelectedTab == formTabs.TabPages["cheatPage"])
                    ActiveForm.Text = "Growbrew Proxy - Cheats and more";
                else if (formTabs.SelectedTab == formTabs.TabPages["extraPage"])
                {
                    loadLogs();
                    ActiveForm.Text = "Growbrew Proxy - Logs";
                }
                else if (formTabs.SelectedTab == formTabs.TabPages["hackernet"])
                {
                    ActiveForm.Text = "Growbrew Proxy - HNetwork";
                    logniasuserlabel.Text = "Logged in as: " + userInfo.username;
                    if (!getUsersOnline.Enabled) getUsersOnline.Start();

                    BSONObject request = new BSONObject();
                    request["msg"] = "get_online";
                    request["note"] = "(none)";
                    byte[] requestData = SimpleBSON.Dump(request);

                    stateObj.workSocket.BeginSend(requestData, 0, requestData.Length, SocketFlags.None, new AsyncCallback(SendCallback), stateObj);
                }
            }
            catch
            {

            }
        }

        private void reloadLogs_Click(object sender, EventArgs e)
        {            
            loadLogs();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (ipaddrBox.Text != "" && portBox.Value != 0)
            {
                token = 0;
                Growtopia_IP = ipaddrBox.Text;
                Growtopia_Port = (int)portBox.Value;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (realPeer != null)
            {
                if (realPeer.State == ENetPeerState.Connected)
                realPeer.DisconnectNow(0);
            }
            if (proxyPeer != null)
            {
                if (proxyPeer.State == ENetPeerState.Connected)
                proxyPeer.DisconnectNow(0);
            }
        }

        private void changeNameBox_TextChanged(object sender, EventArgs e)
        {
            GamePacketProton variantPacket = new GamePacketProton();
            variantPacket.AppendString("OnNameChanged");
            variantPacket.AppendString("`w" + changeNameBox.Text + "``");
            variantPacket.NetID = messageHandler.worldMap.netID;
            PacketSending.SendData(variantPacket.GetBytes(), proxyPeer);
            //variantPacket.NetID =
                
                
        }

        void doRGBHack()
        {
            bool k1 = false;
            bool k2 = false;
            bool k3 = false;
            bool kAll = false;
            while (rgbSkinHack.Checked)
            {
                Thread.Sleep(32);
                skinColor[0] = 255;

                if (kAll == false)
                {
                    if (skinColor[1] < 255) skinColor[1]++;
                    else k1 = true;
                    if (k1 == true) if (skinColor[2] < 255) skinColor[2]++;
                        else k2 = true;
                    if (k2 == true) if (skinColor[3] < 255) skinColor[3]++;
                        else k3 = true;

                    if (k3 == true) kAll = true;
                }
                else
                {
                    if (skinColor[3] > 0) skinColor[3]--;
                    else k1 = false;
                    if (k1 == false) if (skinColor[2] > 0) skinColor[2]--;
                        else k2 = false;
                    if (k2 == false) if (skinColor[1] > 0) skinColor[1]--;
                        else k3 = false;

                    if (k3 == false) kAll = false;
                }

            
                
                //else Array.Copy(BitConverter.GetBytes(0), 0, skinColor, 1, 3);
                
                GamePacketProton variantPacket = new GamePacketProton();
                variantPacket.AppendString("OnChangeSkin");
                variantPacket.AppendUInt(BitConverter.ToUInt32(skinColor, 0));
                variantPacket.NetID = messageHandler.worldMap.netID;
                //variantPacket.delay = 100;
                PacketSending.SendData(variantPacket.GetBytes(), proxyPeer);
                
            }
        }

        private void rgbSkinHack_CheckedChanged(object sender, EventArgs e)
        {
            cheat_rgbSkin = !cheat_rgbSkin; // keeping track
            if (rgbSkinHack.Checked)
            {
                Thread t = new Thread(doRGBHack);
                t.Start();
            }
        }

        private void hack_magplant_CheckedChanged(object sender, EventArgs e)
        {
            cheat_magplant = !cheat_magplant;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pForm.Text = "All players in " + messageHandler.worldMap.currentWorld;
            pForm.ShowDialog();
        }

        private void aboutlabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {           
            Process.Start("http://github.com/iProgramMC");  // iprogramincpp
            Process.Start("http://github.com/playingoDEERUX");
        }

        private void hack_autoworldbanmod_CheckedChanged(object sender, EventArgs e)
        {
            cheat_autoworldban_mod = !cheat_autoworldban_mod;
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            try
            {
                TankPacket p2 = new TankPacket();
                p2.PacketType = (int)NetTypes.PacketTypes.ITEM_ACTIVATE_OBJ;
                if (messageHandler.worldMap == null) return;
                custom_collect_x.Text = messageHandler.worldMap.player.X.ToString(); // crahp
                custom_collect_y.Text = messageHandler.worldMap.player.Y.ToString(); // crahp
                p2.X = int.Parse(custom_collect_x.Text);
                p2.Y = int.Parse(custom_collect_y.Text);
                p2.MainValue = int.Parse(custom_collect_uid.Text);

                PacketSending.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, p2.PackForSendingRaw(), MainForm.realPeer);
            }
            catch // ignore exception
            {

            }
        }

        private void playerLogicUpdate_Tick(object sender, EventArgs e)
        {
            if (messageHandler.worldMap != null) // checking if we have it setup
            {
                Player playerObject = messageHandler.worldMap.player;
                posXYLabel.Text = "X: " + playerObject.X.ToString() + " Y: " + playerObject.Y.ToString();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            cheat_speedy = !cheat_speedy;
           
            TankPacket p = new TankPacket();
            p.PacketType = (int)NetTypes.PacketTypes.SET_CHARACTER_STATE;
            p.X = 1000;
            p.Y = 300;
            p.YSpeed = 1000;
            p.NetID = messageHandler.worldMap.netID;
            if (cheat_speed.Checked)
            {              
                p.XSpeed = 100000;
            }
            else
            {
                p.XSpeed = 300;
            }
            byte[] data = p.PackForSendingRaw();
            Buffer.BlockCopy(BitConverter.GetBytes(8487168), 0, data, 1, 3);
            PacketSending.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, data, proxyPeer);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            int netID = 0;
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                if (p.name.Contains(nameBoxOn.Text))
                {
                    netID = p.netID;
                    break;
                }
            }
            PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|wrench\nnetid|" + netID.ToString(), realPeer);
            PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|popup\nnetID|" + netID.ToString() + "|\nbuttonClicked|" + actionButtonClicked.Text + "\n", realPeer);
        }

        private void sendAction_Click(object sender, EventArgs e)
        {
            //PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|setSkin\ncolor|" + actionText.Text + "\n", realPeer);
        }

        private void macUpdate_Click(object sender, EventArgs e)
        {
            macc = setMac.Text;
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            //PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|storageboxxtreme\ntilex|" + tileX.ToString() + "|\ntiley|" + tileY.ToString() + "|\nitemid|" + itemid.ToString() + "|\nbuttonClicked|cancel\n\nitemcount|1\n", realPeer);
            PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|storageboxxtreme\ntilex|" + tileX.ToString() + "|\ntiley|" + tileY.ToString() + "|\nitemid|1|\nbuttonClicked|cancel\nitemcount|1\n", realPeer);
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            bypassAAP = !bypassAAP;
        }

        private void ghostmodskin_CheckedChanged(object sender, EventArgs e)
        {
            if (ghostmodskin.Checked)
            {
                skinColor[0] = 110; // A - transparency
                skinColor[1] = 255;
                skinColor[2] = 255;
                skinColor[3] = 255;

                GamePacketProton variantPacket = new GamePacketProton();
                variantPacket.AppendString("OnChangeSkin");
                variantPacket.AppendUInt(BitConverter.ToUInt32(skinColor, 0));
                variantPacket.NetID = messageHandler.worldMap.netID;
                //variantPacket.delay = 100;
                PacketSending.SendData(variantPacket.GetBytes(), proxyPeer);
            }
            else
            {
                skinColor[0] = 255;
                GamePacketProton variantPacket = new GamePacketProton();
                variantPacket.AppendString("OnChangeSkin");
                variantPacket.AppendUInt(BitConverter.ToUInt32(skinColor, 0));
                variantPacket.NetID = messageHandler.worldMap.netID;
                //variantPacket.delay = 100;
                PacketSending.SendData(variantPacket.GetBytes(), proxyPeer);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (send2client.Checked)
            {
                PacketSending.SendPacket(3, packetText.Text, proxyPeer);
            }
            else
            {
                PacketSending.SendPacket(3, packetText.Text, realPeer);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (send2client.Checked)
            {
                PacketSending.SendPacket(2, packetText.Text, proxyPeer);
            }
            else
            {
                PacketSending.SendPacket(2, packetText.Text, realPeer);
            }
        }

        private void actionButtonClicked_TextChanged(object sender, EventArgs e)
        {

        }

        private void nameBoxOn_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            TankPacket p = new TankPacket();
            p.PacketType = 3;

            for (int i = 0; i < 100; i++)
            {
                p.PunchX = i;
                p.PunchY = i;
                p.ExtDataMask = 838338258;
                PacketSending.SendPacketRaw(4, p.PackForSendingRaw(), realPeer);
                p.PacketType = 0;
                PacketSending.SendPacketRaw(4, p.PackForSendingRaw(), realPeer);
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|input|?", realPeer);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string str = "";
            for (int i = 0; i < 100000; i++) str += "a";
            
            PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, str, realPeer);
            MessageBox.Show("Sent packet!");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            blockEnterGame = !blockEnterGame;
        }

        void doTheFastNukaz()
        {
            while (checkBox1.Checked)
            {
                Thread.Sleep(10);
                if (realPeer != null)
                {
                    if (realPeer.State != ENetPeerState.Connected) return;
                    
                    for (int c = 0; c < 3; c++)
                    {
                        Thread.Sleep(1000);
                        for (int i = 0; i < 40; i++)
                        {
                            int x, y;
                            x = messageHandler.worldMap.player.X / 32;
                            y = messageHandler.worldMap.player.Y / 32;




                            TankPacket tkPt = new TankPacket();
                            tkPt.PunchX = x;
                            tkPt.PunchY = y + i;
                            tkPt.MainValue = 18;
                            tkPt.X = messageHandler.worldMap.player.X;
                            tkPt.Y = messageHandler.worldMap.player.Y;
                            tkPt.ExtDataMask &= ~0x04;
                            tkPt.ExtDataMask &= ~0x40;
                            tkPt.ExtDataMask &= ~0x10000;
                            tkPt.NetID = -1;
                            PacketSending.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                            tkPt.NetID = -1;
                            tkPt.PacketType = 3;
                            tkPt.ExtDataMask = 0;
                            PacketSending.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                        }
                    }
                }
            }
        }

        void doTheNukaz()
        {
            while (checkBox3.Checked)
            {
                Thread.Sleep(10);
                if (realPeer != null)
                {
                    if (realPeer.State != ENetPeerState.Connected) return;

                    int c = 3;
                    if (checkBox4.Checked) c = 4;
                    for (int i = 0; i < c; i++)
                    {
                        int x, y;
                        x = messageHandler.worldMap.player.X / 32;
                        y = messageHandler.worldMap.player.Y / 32;

                        if (!checkBox5.Checked)
                        {
                            if (i == 0) x = x + 1;
                            else if (i == 1) x = x - 1;
                            else if (i == 2) y = y - 1;
                            if (checkBox4.Checked) if (i == 3) y = y + 1;
                        }
                        else
                        {
                            
                            if (i == 1) x -= 1;
                            if (i == 2) x -= 2;
                        }

                        Thread.Sleep(166);
                        TankPacket tkPt = new TankPacket();
                        tkPt.PunchX = x;
                        tkPt.PunchY = y;
                        tkPt.MainValue = 18;
                        tkPt.X = messageHandler.worldMap.player.X;
                        tkPt.Y = messageHandler.worldMap.player.Y;
                        tkPt.ExtDataMask &= ~0x04;
                        tkPt.ExtDataMask &= ~0x40;
                        tkPt.ExtDataMask &= ~0x10000;
                        tkPt.NetID = -1;
                        PacketSending.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                        tkPt.NetID = -1;
                        tkPt.PacketType = 3;
                        tkPt.ExtDataMask = 0;
                        PacketSending.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                    }
                }
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                Thread thread = new Thread(new ThreadStart(doTheNukaz));
                thread.Start();
            }
        }

        string filterOutAllBadChars(string str)
        {
            return Regex.Replace(str, @"[^a-zA-Z0-9\-]", "");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                PacketSending.SendPacket(2, "action|input\n|text|/pull " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                PacketSending.SendPacket(2, "action|input\n|text|/ban " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                PacketSending.SendPacket(2, "action|input\n|text|/kick " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                PacketSending.SendPacket(2, "action|input\n|text|/trade " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            isHTTPRunning = !isHTTPRunning;
            if (isHTTPRunning)
            {
                string[] arr = new string[1];
                arr[0] = "http://*:80/";
                HTTPServer.StartHTTP(arr);
                button11.Text = "Stop HTTP Server";
                label13.Visible = true;
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            skipCache = !skipCache;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            game_version = textBox2.Text;
        }

        private void proxyPage_Click(object sender, EventArgs e)
        {

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {

        }

       

        private void button14_Click(object sender, EventArgs e)
        {
            TankPacket p = new TankPacket();
            p.PacketType = (int)NetTypes.PacketTypes.SET_CHARACTER_STATE;
            p.X = 1000;
            p.Y = 300;
            p.YSpeed = 1000;
            p.NetID = messageHandler.worldMap.netID;
            if (cheat_speed.Checked)
            {
                p.XSpeed = 100000;
            }
            else
            {
                p.XSpeed = 300;
            }
            p.MainValue = 1;
            byte[] data = p.PackForSendingRaw();
            Buffer.BlockCopy(BitConverter.GetBytes(8487168), 0, data, 1, 3);
            PacketSending.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, data, proxyPeer);
        }

        private void button15_Click(object sender, EventArgs e)
        {

        }

        private void changelog_Click(object sender, EventArgs e)
        {
            this.BeginInvoke((Action)(() => MessageBox.Show("Growbrew Proxy Changelogs:\n" +
                "\n1.5.1\n--------------------------\n" +
                "- [Hacker Network] Added Enter to conversate\n" +
                "- [Hacker Network] Added clear all messages\n" +
                "- [Hacker Network] Fixed some server sided issues\n" +
                "- [Hacker Network] Added automatic version check & update.\n" +
                "\n1.5\n--------------------------\n" +
                "- Added Mod Noclip (can ban)\n" +
                "- Added Ignore Setback (can ban)\n" +
                "- Added HWID Lock\n" +
                "- Added Hacker Network (growbrew users can talk there)\n" +
                "- Captcha should never show up anymore to client and be instantly solved by proxy!\n" +
                "- Some bug fixes, fixed aap bypass not working and worlds crashing rarely.\n" +
                "- Added Changelogs (all changes since 1.5 will be logged here)\n" +
                "\n~playingo/DEERUX")));
        }

        private void button13_Click(object sender, EventArgs e)
        {
            TankPacket p = new TankPacket();
            p.PacketType = -1;
            
            
            PacketSending.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, p.PackForSendingRaw(), realPeer);
        }

        private void button15_Click_1(object sender, EventArgs e)
        {
            
        }

        private void button15_Click_2(object sender, EventArgs e)
        {
            string pass = RandomString(8);
            PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|input\n|text|/sb `2?_ [WE ARE INDIAN TECHNICIAN QUALITY EXPERTS (R)] `4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n`4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n`4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n`4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n  hacked by anonymous all ur data is hacked!`2_?", realPeer);
            for (int i = 0; i < 84; i++)
            {
                PacketSending.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|register\nusername|" + RandomString(9) + "\npassword|" + pass + "\npasswordverify|" + pass + "\nemail|a@a.de\n", realPeer);

            }
        }

        void doTakeAll()
        {
            int startingfrom = 0;
            int.TryParse(textBox3.Text, out startingfrom);

            for (int i = 0; i < 10000; i++)
            {
                Thread.Sleep(12);
                TankPacket p2 = new TankPacket();
                p2.PacketType = (int)NetTypes.PacketTypes.ITEM_ACTIVATE_OBJ;
                p2.MainValue = i;
                PacketSending.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, p2.PackForSendingRaw(), MainForm.realPeer);
            }
           
        }

        private void button17_Click(object sender, EventArgs e)
        {
           
            Thread t = new Thread(doTakeAll);
            t.Start(); 
        }

        private void checkBox1_CheckedChanged_2(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                Thread thread = new Thread(new ThreadStart(doTheFastNukaz));
                thread.Start();
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Growbrew Policies (last updated 16.04.2020):\n" +
                "- GROWBREW IS PERMITTED TO USE ANY HARDWARE IDENTIFIER AND YOUR IP ADDRESS.\n" +
                "- GROWBREW IS NOT RESPONSIBLE FOR BANNED GROWTOPIA ACCOUNTS.\n" +
                "- GROWBREW DOES AND WILL NOT PROVIDE ANY KIND OF PROGRAM RELIABILITY, THERE ARE UPDATES BUT BUGS, MISTAKES AND SUCH MAY OCCUR.\n" +
                "- GROWBREW MAY NOT BE SHARED, IT IS A PAID, PREMIUM PRODUCT." +
                "- GROWBREW HAS THE RIGHTS TO CANCEL YOUR ACCOUNT AT ANY TIME, THIS CAN OCCUR IF THE FOLLOWING RULES WERE BROKEN:\n" +
                "- Reselling growbrew, sharing growbrew, fraud, decompilation/use of code for your own purposes and ban evading incase of a ban.");
        }

        private void sendchatmsg_Click(object sender, EventArgs e)
        {
            try
            {
                BSONObject request = new BSONObject();
                request["msg"] = "chat_req";
                request["note"] = chatbox.Text;


                byte[] requestData = SimpleBSON.Dump(request);
                
                stateObj.workSocket.BeginSend(requestData, 0, requestData.Length, SocketFlags.None, new AsyncCallback(SendCallback), stateObj);
                chatbox.Text = "";
            }
            catch
            {

            }
        }

        private void getUsersOnline_Tick(object sender, EventArgs e)
        {
            try
            {
                if (chatbox.Text.Length <= 0)
                {
                    
                    return;
                }
                BSONObject request = new BSONObject();
                request["msg"] = "get_online";
                request["note"] = "(none)";
                byte[] requestData = SimpleBSON.Dump(request);

                tClient.Client.BeginSend(requestData, 0, requestData.Length, SocketFlags.None, new AsyncCallback(SendCallback), stateObj);
            }
            catch
            {

            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            chatcontent.Clear();
        }

        private void chatbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendchatmsg.PerformClick();
                // these last two lines will stop the beep sound
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}
