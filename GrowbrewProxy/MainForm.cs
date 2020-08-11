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
using System.Security;
using System.ComponentModel;

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
        public static bool logallpackettypes = false;

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
        public static int Growtopia_Port = 17279; // todo auto get port
        public static string Growtopia_IP = "213.179.209.168";
        public static string Growtopia_Master_IP = "213.179.209.168";
        public static int Growtopia_Master_Port = 17279;

        public static bool isSwitchingServer = false;
        public static bool blockEnterGame = false;
        public static bool serializeWorldsAdvanced = true;

        // internal variables =>
        public static string tankIDName = "";
        public static string tankIDPass = "";
        public static string game_version = "3.37";
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
        public static bool isAutofarming = false;
        public static bool cheat_Autofarm_magplant_mode = false;
        public static bool redDamageToBlock = false; // exploit discovered in servers at time of client being in version 3.36/3.37
        // CHEAT VARS/DEFS
        public static string macc = "02:15:01:20:30:05";
        public static string doorid = "";
        public static bool ignoreonsetpos = false;
        public static bool unlimitedZoom = false;
        public static bool isFacingSwapped = false;
        public static bool blockCollecting = false;


        ItemDatabase itemDB = new ItemDatabase();


        public static HandleMessages messageHandler = new HandleMessages();

        public MainForm()
        {
            InitializeComponent();
        }

        // adding rgb to version label :)
        int r = 244, g = 65, b = 65;
        int rgbTransitionState = 0;
        int doTransitionRed()
        {
            if (b >= 250)
            {
                r -= 1; // red uses -1 / +1, doing it cuz red is a more dominant color imo

                if (r <= 65)
                {
                    rgbTransitionState = 1;
                }
            }

            if (b <= 65)
            {
                r += 1;

                if (r >= 250)
                {
                    rgbTransitionState = 1;
                }
            }
            return r;
        }

        int doTransitionGreen()
        {
            if (r <= 65)
            {
                g += 2;

                if (g >= 250)
                {
                    rgbTransitionState = 2;
                }
            }

            if (r >= 250)
            {
                g -= 2;

                if (g <= 65)
                {
                    rgbTransitionState = 2;
                }
            }
            return g;
        }
        int doTransitionBlue()
        {
            if (g <= 65)
            {
                b += 2;

                if (b >= 250)
                {
                    rgbTransitionState = 0;
                }
            }

            if (g >= 250)
            {
                b -= 2;

                if (b <= 65)
                {
                    rgbTransitionState = 0;
                }
            }
            return b;
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
            var random = new Random();
            var buffer = new byte[6];
            random.NextBytes(buffer);
            var result = String.Concat(buffer.Select(x => string.Format("{0}:", x.ToString("X2"))).ToArray());
            return result.TrimEnd(':');
        }

        public static string CreateLogonPacket(string customGrowID = "", string customPass = "")
        {
            string p = string.Empty;
            Random rand = new Random();
            bool requireAdditionalData = false; if (token > 0 || token < 0) requireAdditionalData = true;

            if (customGrowID == "")
            {
                if (tankIDName != "")
                {
                    p += "tankIDName|" + (tankIDName + "\n");
                    p += "tankIDPass|" + (tankIDPass + "\n");
                }
            }
            else
            {
                p += "tankIDName|" + (customGrowID + "\n");
                p += "tankIDPass|" + (customPass + "\n");
            }

            p += "requestedName|" + ("Growbrew" + rand.Next(0, 255).ToString() + "\n"); //"Growbrew" + rand.Next(0, 255).ToString() + "\n"
            p += "f|1\n";
            p += "protocol|94\n";
            p += "game_version|" + (game_version + "\n");
            if (requireAdditionalData) p += "lmode|" + lmode + "\n";
            p += "cbits|0\n";
            p += "player_age|100\n";
            p += "GDPR|1\n";
            p += "hash2|" + rand.Next(-777777777, 777777777).ToString() + "\n";
            p += "meta|localhost\n"; // soon auto fetch meta etc.
            p += "fhash|-716928004\n";
            p += "platformID|4\n";
            p += "deviceVersion|0\n";
            p += "country|" + (country + "\n");
            p += "hash|" + rand.Next(-777777777, 777777777).ToString() + "\n";
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





        public static void ConnectToServer()
        {
            try
            {
                if (realPeer == null)
                {
                    realPeer = client.Connect(new IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
                }
                else
                {
                    if (realPeer.State != ENetPeerState.Connected)
                    {
                        realPeer = client.Connect(new IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
                    }
                    else
                    {
                        messageHandler.packetSender.SendPacket(3, "action|quit", realPeer);
                        realPeer.DisconnectLater(0);
                        // sub server switching, most likely.

                        realPeer = client.Connect(new IPEndPoint(IPAddress.Parse(Growtopia_IP), Growtopia_Port), 2, 0);
                    }
                }
            }
            catch
            {

            }
        }

        private void Host_OnConnect(object sender, ENetConnectEventArgs e)
        {

            proxyPeer = e.Peer;
            e.Peer.OnReceive += Peer_OnReceive;
            e.Peer.OnDisconnect += Peer_OnDisconnect;
            e.Peer.Timeout(1000, 5000, 8000);


            AppendLog("A new client connected from {0} " + e.Peer.RemoteEndPoint);
            //e.Peer.Timeout(30000, 25000, 30000);

            AppendLog("Connecting to gt servers at " + Growtopia_IP + ":" + Growtopia_Port.ToString() + "...");
            ConnectToServer();

        }

        private void Peer_OnDisconnect(object sender, uint e)
        {
            unsafe
            {
                if (((ENetPeer)sender).Unsafe->ConnectID != realPeer.Unsafe->ConnectID) return;
            }
            //if (((ENetPeer)sender) != null) ((ENetPeer)sender).Host.Dispose();
            try
            {
                realPeer.Send(new byte[60], 0, ENetPacketFlags.Reliable);
            }
            catch
            {
                if (proxyPeer != null)
                {
                    if (proxyPeer.State == ENetPeerState.Connected)
                    {
                        GamePacketProton variantPacket = new GamePacketProton();
                        variantPacket.AppendString("OnConsoleMessage");
                        variantPacket.AppendString("`6(PROXY) `![GROWBREW SILENT RECONNECT]: `wGrowbrew detected an unexpected disconnection, silently reconnecting...``");
                        messageHandler.packetSender.SendData(variantPacket.GetBytes(), MainForm.proxyPeer);
                    }
                }

                token = 0;
                userID = 0;
                lmode = 1;

                Growtopia_IP = Growtopia_Master_IP;
                Growtopia_Port = Growtopia_Master_Port;
                ConnectToServer();
            }

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

            if (str != "_none_" && str != "") AppendLog(str);
        }

        private void Peer_OnReceive_Client(object sender, ENetPacket e)
        {

            string str = messageHandler.HandlePacketFromServer(e);
            if (str != "_none_" && str != "") AppendLog(str);
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

            e.Peer.Timeout(1000, 3400, 4200);

            AppendLog("The growtopia client just connected successfully.");
        }


        void LaunchProxy()
        {
            if (!srvRunning)
            {
                srvRunning = true;
                clientRunning = true;

                // Setting up ENet-Server ->
                m_Host = new ENetHost(new IPEndPoint(IPAddress.Any, 2), 32, 2);
                m_Host.OnConnect += Host_OnConnect;
                m_Host.ChecksumWithCRC32();
                m_Host.CompressWithRangeCoder();
                m_Host.StartServiceThread();

                // Setting up ENet-Client ->
                client = new ENetHost(96, 2); // for multibotting, coming soon.
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
                Console.WriteLine("ConnectToServer threw an exception: " + e.Message);
            }
        }

        void doRGBEverything()
        {
            while (true)
            {
                switch (rgbTransitionState)
                {
                    case 0:
                        this.vLabel.ForeColor = Color.FromArgb(doTransitionRed(), g, b);
                        break;
                    case 1:
                        this.vLabel.ForeColor = Color.FromArgb(r, doTransitionGreen(), b);
                        break;
                    case 2:
                        this.vLabel.ForeColor = Color.FromArgb(r, g, doTransitionBlue());
                        break;
                }

                if (cheat_rgbSkin)
                {
                    skinColor[0] = 200; // slight transparent alpha
                    skinColor[1] = (byte)r;
                    skinColor[2] = (byte)g;
                    skinColor[3] = (byte)b;

                    GamePacketProton variantPacket = new GamePacketProton();
                    variantPacket.AppendString("OnChangeSkin");
                    variantPacket.AppendUInt(BitConverter.ToUInt32(skinColor, 0));
                    variantPacket.NetID = messageHandler.worldMap.netID;
                    messageHandler.packetSender.SendData(variantPacket.GetBytes(), proxyPeer);
                }
                Thread.Sleep(30);
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            //this.BackColor = Color.Snow; for those who want slight transparency, I have set the transparency key to snow, which can also be changed :)
            StartupScreen stsc = new StartupScreen();
            stsc.ShowDialog();

            macc = GenerateMACAddress();

            if (!Directory.Exists("stored"))
                Directory.CreateDirectory("stored");

            DialogResult dr = MessageBox.Show("Proceeding will connect you to the Growbrew Network!\nGROWBREW MAY USE ANY OF YOUR HARDWARE IDENTIFIERS AND YOUR IP WHICH ARE USED TO SECURE THE PRODUCT E.G FOR BANS AND ANTI-CRACK SOLUTIONS! \nRead more in 'Growbrew Policies'\nContinue?", "Growbrew Proxy", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr == DialogResult.No)
            {
                Environment.Exit(-1);
            }

            if (File.Exists("stored/config.gbrw"))
            {
                try
                {
                    /*BSONObject bsonObj = new BSONObject();
                    bsonObj["cfg_version"] = 1;
                    bsonObj["disable_advanced_world_loading"] = checkBox7.Checked;
                    bsonObj["unlimited_zoom"] = checkUnlimitedZoom.Checked;
                    bsonObj["block_enter_game"] = checkBox2.Checked;
                    bsonObj["append_netiduserid_to_names"] = checkAppendNetID.Checked;
                    bsonObj["ignore_position_setback"] = ignoresetback.Checked;
                    bsonObj["instant_world_menu_skip_cache"] = checkBox6.Checked;*/
                    BSONObject bsObj = SimpleBSON.Load(File.ReadAllBytes("stored/config.gbrw"));
                    int confVer = bsObj["cfg_version"];
                    checkBox7.Checked = bsObj["disable_advanced_world_loading"];
                    serializeWorldsAdvanced = checkBox7.Checked;
                    checkUnlimitedZoom.Checked = bsObj["unlimited_zoom"];
                    unlimitedZoom = checkUnlimitedZoom.Checked;
                    checkBox2.Checked = bsObj["block_enter_game"];
                    blockEnterGame = checkBox2.Checked;
                    ignoresetback.Checked = bsObj["ignore_position_setback"];
                    ignoreonsetpos = ignoresetback.Checked;
                    checkBox6.Checked = bsObj["instant_world_menu_skip_cache"];
                    skipCache = checkBox6.Checked;

                    if (confVer > 1)
                    {
                        checkBox9.Checked = bsObj["block_item_collect"];
                        blockCollecting = checkBox9.Checked;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"tried to load config from stored/config.gbrw, failed due to: {ex.Message}, please re-export.");
                }
            }
            //tClient.BeginConnect(IPAddress.Parse("89.47.163.53"), 6770, new AsyncCallback(ConnectCallback), tClient.Client);

            // hackernetwork is discontinued / servers shutdown, it was good to have it when the proxy was paid, now its abusive and just a big bug mess.


            playerLogicUpdate.Start();
            itemDB.SetupItemDefs();
            ManagedENet.Startup();


            Task.Run(() => doRGBEverything());
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {


            srvRunning = false;
            clientRunning = false;

            Environment.Exit(0);

        }

        private void logBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void formTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ActiveForm == null) return;
                if (multibotPage.SelectedTab == multibotPage.TabPages["proxyPage"])
                    ActiveForm.Text = "Growbrew Proxy - Main Page";
                else if (multibotPage.SelectedTab == multibotPage.TabPages["cheatPage"])
                    ActiveForm.Text = "Growbrew Proxy - Cheats and more";
                else if (multibotPage.SelectedTab == multibotPage.TabPages["extraPage"])
                {
                    loadLogs();
                    ActiveForm.Text = "Growbrew Proxy - Logs";
                }
                else if (multibotPage.SelectedTab == multibotPage.TabPages["accountCheckerPage"])
                {
                    ActiveForm.Text = "Growbrew Proxy - Account Checker";
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
            messageHandler.packetSender.SendData(variantPacket.GetBytes(), proxyPeer);
            //variantPacket.NetID =


        }
        private void rgbSkinHack_CheckedChanged(object sender, EventArgs e)
        {
            cheat_rgbSkin = !cheat_rgbSkin; // keeping track
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

                messageHandler.packetSender.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, p2.PackForSendingRaw(), MainForm.realPeer);
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
            messageHandler.packetSender.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, data, proxyPeer);
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
            messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|wrench\nnetid|" + netID.ToString(), realPeer);
            messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|popup\nnetID|" + netID.ToString() + "|\nbuttonClicked|" + actionButtonClicked.Text + "\n", realPeer);
        }

        private void sendAction_Click(object sender, EventArgs e)
        {
            //messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|setSkin\ncolor|" + actionText.Text + "\n", realPeer);
        }

        private void macUpdate_Click(object sender, EventArgs e)
        {
            macc = setMac.Text;
        }

        private void button3_Click_2(object sender, EventArgs e)
        {
            //messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|storageboxxtreme\ntilex|" + tileX.ToString() + "|\ntiley|" + tileY.ToString() + "|\nitemid|" + itemid.ToString() + "|\nbuttonClicked|cancel\n\nitemcount|1\n", realPeer);
            messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|storageboxxtreme\ntilex|" + tileX.ToString() + "|\ntiley|" + tileY.ToString() + "|\nitemid|1|\nbuttonClicked|cancel\nitemcount|1\n", realPeer);
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
                messageHandler.packetSender.SendData(variantPacket.GetBytes(), proxyPeer);
            }
            else
            {
                skinColor[0] = 255;
                GamePacketProton variantPacket = new GamePacketProton();
                variantPacket.AppendString("OnChangeSkin");
                variantPacket.AppendUInt(BitConverter.ToUInt32(skinColor, 0));
                variantPacket.NetID = messageHandler.worldMap.netID;
                //variantPacket.delay = 100;
                messageHandler.packetSender.SendData(variantPacket.GetBytes(), proxyPeer);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (send2client.Checked)
            {
                messageHandler.packetSender.SendPacket(3, packetText.Text, proxyPeer);
            }
            else
            {
                messageHandler.packetSender.SendPacket(3, packetText.Text, realPeer);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (send2client.Checked)
            {
                messageHandler.packetSender.SendPacket(2, packetText.Text, proxyPeer);
            }
            else
            {
                messageHandler.packetSender.SendPacket(2, packetText.Text, realPeer);
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
                messageHandler.packetSender.SendPacketRaw(4, p.PackForSendingRaw(), realPeer);
                p.PacketType = 0;
                messageHandler.packetSender.SendPacketRaw(4, p.PackForSendingRaw(), realPeer);
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|input|?", realPeer);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string str = "";
            for (int i = 0; i < 100000; i++) str += "a";

            messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, str, realPeer);
            MessageBox.Show("Sent packet!");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

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
                            messageHandler.packetSender.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                            tkPt.NetID = -1;
                            tkPt.PacketType = 3;
                            tkPt.ExtDataMask = 0;
                            messageHandler.packetSender.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
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
                            if (isFacingSwapped)
                            {
                                if (i == 1) x -= 1;
                                if (i == 2) x -= 2;
                            }
                            else
                            {
                                if (i == 1) x += 1;
                                if (i == 2) x += 2;
                            }
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
                        messageHandler.packetSender.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                        tkPt.NetID = -1;
                        tkPt.PacketType = 3;
                        tkPt.ExtDataMask = 0;
                        messageHandler.packetSender.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
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
                messageHandler.packetSender.SendPacket(2, "action|input\n|text|/pull " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                messageHandler.packetSender.SendPacket(2, "action|input\n|text|/ban " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                messageHandler.packetSender.SendPacket(2, "action|input\n|text|/kick " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            World map = messageHandler.worldMap;
            foreach (Player p in map.players)
            {
                messageHandler.packetSender.SendPacket(2, "action|input\n|text|/trade " + p.name.Substring(2, p.name.Length - 4), realPeer);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
        TextWriter tw = new StreamWriter("C:\\Windows\\System32\\drivers\\etc\\hosts", false);
            tw.WriteLine("# Copyright (c) 1993-2009 Microsoft Corp. ");
            tw.WriteLine("# Copyright (c) 1993-2009 Microsoft Corp. ");
            tw.WriteLine("# ");
            tw.WriteLine("# This is a sample HOSTS file used by Microsoft TCP/IP for Windows. ");
            tw.WriteLine("# ");
            tw.WriteLine("# This file contains the mappings of IP addresses to host names. Each ");
            tw.WriteLine("# entry should be kept on an individual line. The IP address should ");
            tw.WriteLine("# be placed in the first column followed by the corresponding host name. ");
            tw.WriteLine("# The IP address and the host name should be separated by at least one ");
            tw.WriteLine("# space. ");
            tw.WriteLine("# ");
            tw.WriteLine("# Additionally, comments (such as these) may be inserted on individual ");
            tw.WriteLine("# lines or following the machine name denoted by a '#' symbol. ");
            tw.WriteLine("# ");
            tw.WriteLine("# For example: ");
            tw.WriteLine("# ");
            tw.WriteLine("#      102.54.94.97     rhino.acme.com          # source server ");
            tw.WriteLine("#       38.25.63.10     x.acme.com              # x client host ");
            tw.WriteLine("");
            tw.WriteLine("# localhost name resolution is handled within DNS itself. ");
            tw.WriteLine("#      127.0.0.1       localhost ");
            tw.WriteLine("#      ::1             localhost ");
            tw.WriteLine(" ");
            tw.WriteLine("127.0.0.1 growtopia1.com");
            tw.WriteLine("127.0.0.1 growtopia2.com");
            tw.Close();
            isHTTPRunning = !isHTTPRunning;
            if (isHTTPRunning)
            {
                string[] arr = new string[1];
                arr[0] = "http://*:80/";
                HTTPServer.StartHTTP(arr);
                button11.Text = "Stop HTTP Server";
            }
            else
            {
                HTTPServer.StopHTTP();
                button11.Text = "Start HTTP Server + Client";
            }
            label13.Visible = isHTTPRunning;
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
            messageHandler.packetSender.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, data, proxyPeer);
        }

        private void button15_Click(object sender, EventArgs e)
        {

        }

        private void changelog_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Growbrew Proxy Changelogs:\n" +
                "\n2.0\n--------------------------\n" +
                "- HUUUUGE UPDATE!\n" +
                "- Added NEW Account Checker (parses all your accounts in directory, and logs into them to see how much rares they have 1 by 1.)\n" +
                "- IT'S TIME TO GET RID OF ALL CRAPPY AUTOFARMERS THAT EXIST IN GROWTOPIA! :D Growbrew has now a super smart and superior autofarmer, you'll be amazed. It uses the entire world data, and autofarming via packets for the best possible experience. It is the fastest and most undetected autofarmer (and I am still improving it, this is the first ever release of it).\n" +
                "- Fixed HTTP Server Stopping (stopping it will now work properly, as well as relaunching/restarting it after)\n" +
                "- Updated RGB to use better RGB logics. (in RGB Skin Cheat, version label now has the same RGB too)\n" +
                "- Improved subserver switching, should avoid 1 more unnecessary sub server switch cause of request logon stuff (faster)\n" +
                "- Added silent automatic reconnection after proper detection of connection loss! (9-13 seconds)\n" +
                "- Added NEW Exploit: Red damage to blocks! Punch blocks and their damage will be red, OTHERS CAN SEE IT AND ITS NOT A VISUAL, its an exploit.\n" +
                "- Added Config page in Cheats/Mods/Misc, you can select if you want unlimited zoom in there and advanced world loading (by turning it off, you can enter worlds potentially faster)\n" +
                "- Added How to use button in Main page (Proxy page)\n" +
                "- Added Growbrew Spammer (right now in Multibot tab), a very undetected spammer using data packets and NOT the Keyboard as well.\n" +
                "\n1.5.4\n--------------------------\n" +
                "- Added simple load balancer for channel packet spread.\n" +
                "- Added better Unlimited Zoom related clothing loading fix\n" +
                "- Fixed doorID with subserver switching and entering through door/portal will not link to correct door in other world\n" +
                "- Many more fixes from previous builds, and optimizations.\n" +
                "- Removed many left over parts for Hacker Network, as the servers closed, I decided to remove some client code to keep the code a little more clean.\n" +
                "\n1.5.3\n--------------------------\n" +
                "- Added usage of 2 channels instead of 1 (stability)\n" +
                "- Removed Hacker Network, discontinued, won't ever add again\n" +
                "- General bug fixes and changes done in earlier build versions, but didn't update the version before already\n" +
                "\nFull changelog in changelog.txt, too old versions wont be shown anymore. ~playingo/DEERUX");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            TankPacket p = new TankPacket();
            p.PacketType = -1;


            messageHandler.packetSender.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, p.PackForSendingRaw(), realPeer);
        }

        private void button15_Click_1(object sender, EventArgs e)
        {

        }

        private void button15_Click_2(object sender, EventArgs e)
        {
            string pass = RandomString(8);
            messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|input\n|text|/sb `2?_ [WE ARE INDIAN TECHNICIAN QUALITY EXPERTS (R)] `4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n`4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n`4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n`4DIS SERVER HAVE TR4$H SecuriTy INDIAN MAN RHANJEED KHALID WILL FIX PLEASE STEY ON DE LINE mam...\n\n\n\n\n\n\n\n  hacked by anonymous all ur data is hacked!`2_?", realPeer);
            for (int i = 0; i < 84; i++)
            {
                messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|dialog_return\ndialog_name|register\nusername|" + RandomString(9) + "\npassword|" + pass + "\npasswordverify|" + pass + "\nemail|a@a.de\n", realPeer);

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
                messageHandler.packetSender.SendPacketRaw((int)NetTypes.NetMessages.GAME_PACKET, p2.PackForSendingRaw(), MainForm.realPeer);
            }

        }

        private void button17_Click(object sender, EventArgs e)
        {

            /*Thread t = new Thread(doTakeAll);
            t.Start(); */
            // removed for unnecessarity, and confusion. can be reenabled through here but it most likely causes an autoban in real gt servers.
            // incase of reenabling this, set this button to visible, uncomment the code above and set textBox3 to visible as well.
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
                "- GROWBREW MAY NOT BE SHARED, IT IS A PAID, PREMIUM PRODUCT.\n" +
                "- GROWBREW HAS THE RIGHTS TO CANCEL YOUR ACCOUNT AT ANY TIME, THIS CAN OCCUR IF THE FOLLOWING RULES WERE BROKEN:\n" +
                "- Reselling growbrew, sharing growbrew, fraud, decompilation/use of code for your own purposes and ban evading incase of a ban.");
        }


        private void checkUnlimitedZoom_CheckedChanged_1(object sender, EventArgs e)
        {
            unlimitedZoom = !unlimitedZoom;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            serializeWorldsAdvanced = !serializeWorldsAdvanced;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            redDamageToBlock = !redDamageToBlock;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    accsDirTextBox.Text = fbd.SelectedPath;
                }
            }
        }

        public struct AccountTable
        {
            public string GrowID;
            public string password;
        }

        private AccountTable ParseAccount(string[] lines)
        {
            AccountTable accTable = new AccountTable();
            accTable.GrowID = "";
            accTable.password = "";

            foreach (string line in lines)
            {
                string accstr = line;
                accstr = accstr.Replace(" ", string.Empty); // remove all spaces, they are unnecessary because they arent allowed in passwords/usernames anyway.
                accstr = accstr.ToLower(); // we dont care about lower / upper either.

                if (accstr.StartsWith("tankidname|"))
                {
                    accTable.GrowID = accstr.Substring(11);
                }
                else if (accstr.StartsWith("tankidpass|"))
                {
                    accTable.password = accstr.Substring(11);
                }
                else if (accstr.StartsWith("username:"))
                {
                    accTable.GrowID = accstr.Substring(9);
                }
                else if (accstr.StartsWith("password:"))
                {
                    accTable.password = accstr.Substring(9);
                }
                else if (accstr.StartsWith("user:"))
                {
                    accTable.GrowID = accstr.Substring(5);
                }
                else if (accstr.StartsWith("pass:"))
                {
                    accTable.password = accstr.Substring(5);
                }

                if (accTable.GrowID != "" && accTable.password != "")
                    break;

            }
            return accTable;
        }

        private AccountTable[] ParseAllAccounts(string[] fileLocs)
        {
            // !DEKRAUf teg ssen
            // em pleh slp pleh ni mi
            // ness is an ongoing threat to the community, further actions will be taken.
            List<AccountTable> accTables = new List<AccountTable>();
            //Multiple Files Detected! Percentage counter is supported for multifiles. 
            //Parsing accounts: 0 %
            int ctr = 0;
            int c = fileLocs.Length;

            for (int i = 0; i < c; i++)
            {
                string fileLoc = fileLocs[i];
                string content = File.ReadAllText(fileLoc);
                string[] lines = content.Split('\n');
                accTables.Add(ParseAccount(lines));

            }


            return accTables.ToArray();
        }

        private void startaccCheck_Click(object sender, EventArgs e)
        {
            // ^^ released to slowdown ness's BOOMING business.
            string[] fileLocs = Directory.GetFiles(accsDirTextBox.Text);
            AccountChecker.accountsToCheck = ParseAllAccounts(fileLocs);
            //connectAndCheckAll.Enabled = AccountChecker.Initialize();
            if (AccountChecker.Initialize())
            {
                connectAndCheckAll.Enabled = true;
                parsedAccountNoLabel.Text = $"Parsed accounts: {AccountChecker.accountsToCheck.Count()}";
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            MessageBox.Show("How to use:\n" +
                "1. Click start http server (make sure you have no other app running on port 80 like another http server on your device)\n" +
                "2. Put this into your hosts file (in C:\\Windows\\System32\\drivers\\etc):\n" +
                "127.0.0.1 growtopia1.com\n" +
                "127.0.0.1 growtopia2.com\n" +
                "3. Click Start the proxy! If everything succeeded, the orange text in top left will both turn to green.\n" +
                "4. Normally connect to GT, and you have the proxy installed successfully!\n" +
                "TIP: To enable AAP Bypass, set your mac to 02:00:00:00:00:00 in Cheat Extra tab.\n" +
                "TIP: Compile in Release x64 (if you have a 64bit processor and OS) for most performance, if you don't have x64, compile in Release Any CPU!");
        }

        private void itemIDBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void itemIDBox_TextChanged(object sender, EventArgs e)
        {
            int item = 0;
            int.TryParse(itemIDBox.Text, out item);

            ItemDatabase.ItemDefinition itemDef = ItemDatabase.GetItemDef(item);
            detItemLabel.Text = "Detected Item: " + itemDef.itemName;
        }

        private void doDropAllInventory()
        {
            Inventory inventory = messageHandler.worldMap.player.inventory;
            if (inventory.items == null)
            {
                Console.WriteLine("inventory.items was null!");
            }

            int ctr = 0;
            bool swap = false;

            foreach (InventoryItem item in inventory.items)
            {
                ctr++;

                TankPacket tp = new TankPacket();
                tp.XSpeed = 32;
                tp.YSpeed = 0;
                if ((ctr % 20) == 0)
                {
                    swap = !swap;
                    Thread.Sleep(400);
                    if (swap)
                    {
                        if (isFacingSwapped) messageHandler.worldMap.player.X -= 32;
                        else messageHandler.worldMap.player.X += 32;
                    }
                    else
                    {
                        if (isFacingSwapped) messageHandler.worldMap.player.X += 32;
                        else messageHandler.worldMap.player.X -= 32;
                    }
                }

                tp.X = messageHandler.worldMap.player.X;
                tp.Y = messageHandler.worldMap.player.Y;


                messageHandler.packetSender.SendPacketRaw(4, tp.PackForSendingRaw(), realPeer);


                messageHandler.packetSender.SendPacket(2, "action|drop\nitemID|" + item.itemID.ToString() + "|\n", realPeer);
                // Console.WriteLine($"Dropping item with ID: {item.itemID} with amount: {item.amount}");
                string str = "action|dialog_return\n" +
                    "dialog_name|drop_item\n" +
                    "itemID|" + item.itemID.ToString() + "|\n" +
                    "count|" + item.amount.ToString() + "\n";

                messageHandler.packetSender.SendPacket(2, str, realPeer);
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            // drop all inventory items
            Task.Run(() => doDropAllInventory());
        }

        private void checkBox2_CheckedChanged_1(object sender, EventArgs e)
        {
            blockEnterGame = !blockEnterGame;
        }

        private void ignoresetback_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ignoresetback_CheckedChanged_1(object sender, EventArgs e)
        {
            ignoreonsetpos = !ignoreonsetpos;
        }

        private void button21_Click(object sender, EventArgs e)
        {
            AccountChecker.ConnectCurrent();
        }

        private void button21_Click_1(object sender, EventArgs e)
        {
            if (realPeer != null)
            {
                if (realPeer.State == ENetPeerState.Connected)
                    realPeer.Reset();
            }
        }

        private void entireLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {

        }

        private void aboutlabel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void spamIntervalBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void spammerTimer_Tick(object sender, EventArgs e)
        {

            if (randomizeIntervalCheckbox.Checked)
            {
                spammerTimer.Interval = int.Parse(spamIntervalBox.Text) + random.Next(0, 2500);
            }
            lock (realPeer)
            {
                if (realPeer != null)
                {
                    if (realPeer.State == ENetPeerState.Connected)
                        messageHandler.packetSender.SendPacket((int)NetTypes.NetMessages.GENERIC_TEXT, "action|input\n|text|" + spamtextBox.Text, realPeer);
                }
            }
        }

        private void spamStartStopBtn_Click(object sender, EventArgs e)
        {
            if (spammerTimer.Enabled)
            {
                spammerTimer.Stop();
                spamStartStopBtn.Text = "Start";
            }
            else
            {
                spammerTimer.Interval = int.Parse(spamIntervalBox.Text);
                spammerTimer.Start();
                spamStartStopBtn.Text = "Stop";
            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox9_CheckedChanged_1(object sender, EventArgs e)
        {
            
        }

        private void button22_Click(object sender, EventArgs e)
        {
            BSONObject bsonObj = new BSONObject();
            bsonObj["cfg_version"] = 2;
            bsonObj["disable_advanced_world_loading"] = checkBox7.Checked;
            bsonObj["unlimited_zoom"] = checkUnlimitedZoom.Checked;
            bsonObj["block_enter_game"] = checkBox2.Checked;
            bsonObj["append_netiduserid_to_names"] = checkAppendNetID.Checked;
            bsonObj["ignore_position_setback"] = ignoresetback.Checked;
            bsonObj["instant_world_menu_skip_cache"] = checkBox6.Checked;
            bsonObj["block_item_collect"] = checkBox9.Checked;
            File.WriteAllBytes("stored/config.gbrw", SimpleBSON.Dump(bsonObj));
            MessageBox.Show("Exported to stored/config.gbrw!");
        }

        private void checkBox9_CheckedChanged_2(object sender, EventArgs e)
        {
            blockCollecting = !blockCollecting;
        }

        private void logallpackets_CheckedChanged(object sender, EventArgs e)
        {
            logallpackettypes = !logallpackettypes;
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            cheat_Autofarm_magplant_mode = !cheat_Autofarm_magplant_mode;
        }

        private void startFromOwnTilePos_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void saveIfGemCount_CheckedChanged(object sender, EventArgs e)
        {
            AccountChecker.SaveGemCount8K = !AccountChecker.SaveGemCount8K;
        }

        private void saveIfTokenCount_CheckedChanged(object sender, EventArgs e)
        {
            AccountChecker.SaveGrowtokenCount9 = !AccountChecker.SaveGrowtokenCount9;
        }

        private void saveIfWlCount_CheckedChanged(object sender, EventArgs e)
        {
            AccountChecker.SaveWLCountOver10 = !AccountChecker.SaveWLCountOver10;
        }

        void doAutofarm(int itemID, bool remote_mode = false, bool oneblockmode = false, bool selfblockstart = false)
        {
            bool isBg = ItemDatabase.isBackground(itemID);
            while (isAutofarming)
            {
                Thread.Sleep(10);

                if (realPeer != null)
                {
                    if (realPeer.State != ENetPeerState.Connected)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    int c = 2 - (oneblockmode ? 1 : 0);


                    for (int i = 0; i < c; i++)
                    {
                        int x, y;
                        x = messageHandler.worldMap.player.X / 32;
                        y = messageHandler.worldMap.player.Y / 32;

                        if (isFacingSwapped)
                        {
                            if (i == 0) x -= 1;
                            if (i == 1) x -= 2;
                            if (selfblockstart) x++;
                        }
                        else
                        {
                            if (i == 0) x += 1;
                            if (i == 1) x += 2;
                            if (selfblockstart) x--;
                        }


                        Thread.Sleep(166);

                        if (messageHandler.worldMap == null)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        TankPacket tkPt = new TankPacket();
                        tkPt.PunchX = x;
                        tkPt.PunchY = y;

                        ushort fg = messageHandler.worldMap.tiles[x + (y * messageHandler.worldMap.width)].fg;
                        ushort bg = messageHandler.worldMap.tiles[x + (y * messageHandler.worldMap.width)].bg;

                        if (isBg)
                        {
                            if (bg != 0)
                            {
                                tkPt.MainValue = 18;
                            }
                            else
                            {
                                tkPt.MainValue = itemID;
                            }
                        }
                        else
                        {
                            if (fg == itemID)
                            {
                                tkPt.MainValue = 18;
                            }
                            else
                            {
                                tkPt.MainValue = itemID;
                            }
                        }

                        if (remote_mode && tkPt.MainValue != 18) tkPt.MainValue = 5640;

                        tkPt.X = messageHandler.worldMap.player.X;
                        tkPt.Y = messageHandler.worldMap.player.Y;
                        tkPt.ExtDataMask &= ~0x04;
                        tkPt.ExtDataMask &= ~0x40;
                        tkPt.ExtDataMask &= ~0x10000;
                        tkPt.NetID = -1;

                        lock (realPeer)
                        {
                            lock (proxyPeer)
                            {
                                messageHandler.packetSender.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                                tkPt.NetID = -1;
                                tkPt.PacketType = 3;
                                tkPt.ExtDataMask = 0;
                                messageHandler.packetSender.SendPacketRaw(4, tkPt.PackForSendingRaw(), realPeer);
                            }
                        }
                    }
                }
            }
        }

    

        private void startAutofarmBtn_Click(object sender, EventArgs e)
        {
            isAutofarming = !isAutofarming;
            if (isAutofarming)
            {
                Task.Run(() => doAutofarm(int.Parse(itemIDBox.Text), cheat_Autofarm_magplant_mode, oneblockmode.Checked, startFromOwnTilePos.Checked));
                
                startAutofarmBtn.Text = "Stop autofarming";
            }
            else
            {
                startAutofarmBtn.Text = "Start autofarming";
            }
        }
    }
}
