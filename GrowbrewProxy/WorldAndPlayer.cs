// thanks to iProgramInCpp#0489, most things are made by him in the GrowtopiaCustomClient, 
// I have just rewritten it into c# and maybe also improved. -playingo

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GrowbrewProxy
{
    public class Player //NetAvatar
    {
        

        public string name = "";
        public string country = "";
        public int netID = 0;
        public int userID = 0;
        public int invis = 0;
        public int mstate = 0;
        public int smstate = 0;
        public int X, Y = 0;
        public bool didClothingLoad = false; // unused now
        public bool didCharacterStateLoad = false; // unused now
        public Inventory inventory; // should only not be null if player is local.
        public void SerializePlayerInventory(byte[] inventoryData)
        {
            int invPacketSize = inventoryData.Length;
            inventory.version = inventoryData[0];
            inventory.backpackSpace = BitConverter.ToInt16(inventoryData, 1);
            int inventoryitemCount = BitConverter.ToInt16(inventoryData, 5); // trade exceeding
            inventory.items = new InventoryItem[inventoryitemCount];

            for (int i = 0; i < inventoryitemCount; i++)
            {
                int pos = 7 + i * 4;
                inventory.items[i].itemID = BitConverter.ToUInt16(inventoryData, pos);
                inventory.items[i].amount = BitConverter.ToInt16(inventoryData, pos + 2);
            }
        }
    };
    
    
    public struct Inventory
    {
        public byte version;
        public short backpackSpace;
        public InventoryItem[] items;
    }
    public struct InventoryItem
    {
        public short amount;
        public ushort itemID;
        public byte flags; // 8-bits reserved.
    }

    public struct Tile
    {
        public int x, y;
        public ushort fg, bg;
        public int tileState;
        public byte[] tileExtra; // might be unused.
        public string str_1; // might be unused.
        public byte type;        
    };
        

    public struct DroppedObject
    {
        public int uid;
        public float x, y;
        public byte itemCount;
        public int id;
    };

    public class World
    {
        // from docs.microsoft.com, used for implementing a disposable class.
       
        
        

        /* (object map) */
        public List<Player> players = new List<Player>();
        public Player player = new Player();
        public Tile[] tiles;
        /* (object map) */ public List<DroppedObject> droppedItems = new List<DroppedObject>();
        DroppedObject dropped = new DroppedObject();
        public int dropped_ITEMUID = -1;
        int dropped_TOTALUIDS = 0;
        internal int tilesProperlySerialized = 0;
        public string currentWorld = "EXIT";
        public int width, height;
        public int playerCount = 0;
        public int netID = 0;
        public int userID = 0; // better keep track of userid too :D
        private int readPos = 0; // good to keep track of world serialization pos :)
        int tileCount = 0;
        internal ulong total_bytes_serialized = 0;

        void clearPlayers()
        {
            players.Clear();
            netID = 0;
            playerCount = 0;
        }

        public int GetNetIDByName(string name)
        {
            foreach (Player pl in players)
            {
                if (pl.name == name) return pl.netID;
            }
            return -1;
        }

        public int GetUserIDByName(string name)
        {
            foreach (Player pl in players)
            {
                if (pl.name == name) return pl.userID;
            }
            return -1;
        }

        public Point GetPositionByNetID(int netID)
        {
            foreach (Player pl in players)
            {
                if (pl.netID == netID) return new Point(pl.X, pl.Y);
            }
            return new Point(-1, -1);
        }

        private Point GetPlayerBtnLocation()
        {
            int x = 32, y = 32;

            if (MainForm.pForm == null) return new Point(x, y);
            var playerControls = MainForm.pForm.playerBox.Controls;


            for (int i = 0; i < playerControls.Count; i++)
            {
                y += 32 * 2;
                if (y >= PlayerForm.updatedHeight - 100) // ghetto, no rescaling after control has been set. too lazy rn
                {
                    y = 32;
                    x += 86 * 2;
                }
            }
        
            return new Point(x, y);
        }

        public void AddPlayerControlToBox(Player pl) // todo - universally add player...
        {
            string username = pl.name.Substring(0, pl.name.Length - 2);

            MainForm.pForm.AddPlayerBtnToForm(username, pl.netID, GetPlayerBtnLocation());
            //MainForm.pControls.Add()
        }

        public void RemovePlayerControl(int netID)
        {
            var playerForm = MainForm.pForm;
            
            for (int i = 0; i < playerForm.Controls.Count; i++)
            {
                var control = playerForm.Controls[i];
                if (control.Name == netID.ToString())
                {
                    MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): Removed player from player control box with netID: " + netID.ToString() + "\n");
                    playerForm.Controls.RemoveAt(i);
                    break;
                }
            }
        }

        private void TileExtra_Serialize(byte[] dataPassed, int loc)
        {
            
            byte a = dataPassed[readPos++];
            tiles[loc].type = a;

            short len = 0;
            //MainForm.LogText += ("[" + DateTime.UtcNow + "] Serializing with type: " + a.ToString() + "\n");
            switch (a)
            {
                case 0:
                    {

                    }
                    break;
                case 1:
                    {
                        // this is door
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        //string f = "";
                        //for (int i = 0; i < x; i++) f += dataPassed[readPos + i]; just skip for now, not needed in here.
                        readPos += len + 1;
                    }
                    break;
                case 2:
                    {
                        // this is sign
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        readPos += len + 4;
                    }
                    break;
                case 3:
                    {
                        // wl shit, owner userID etc...
                        readPos++;
                        byte adminCount = dataPassed[readPos + 4];
                        readPos += (16 + (adminCount * 4));
                    }
                    break;
                case 4:
                    {
                        // trees n timing shit, skipping...
                        readPos += 5;
                    }
                    break;
                case 0x8:
                    {
                        readPos++;
                    }
                    break;
                case 0x9: // provider
                    {
                        // skipping
                        readPos += 4;
                    }
                    break;
                case 0xa:
                    {
                        readPos += 5;
                    }
                    break;
                case 0xb: // hmon. fuaurk off ->_>
                    {
                        readPos += 4;
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        readPos += len;
                    }
                    break;
                case 0xe: // mannequins
                    {
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        readPos += len;
                        // clothing serialization skipping (23 bytes of data in total) =>
                        readPos += 23;
                    }
                    break;
                case 0x0f:
                    {
                        // bunny egg
                        readPos++; 
                    }
                    break;
                case 0x10:
                    {
                        // gameblocks
                        readPos++; 
                    }
                    break;
                case 0x12:
                    {
                        // Xenonite
                        readPos += 5;
                    }
                    break;
                case 0x13:
                    {
                        // Phone Booth
                        readPos += 18;
                    }
                    break;
                case 0x14:
                    {
                        // crystal
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        readPos += len;
                    }
                    break;
                case 0x15:
                    {
                        // Crime in progr....
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        for (int i = 0; i < len; i++) tiles[loc].str_1 += (char)dataPassed[readPos++];
                        readPos += 5;
                    }
                    break;
                case 0x17:
                    {
                        // display blocks
                        readPos += 4;
                    }
                    break;
                case 0x18:
                    {
                        // vends...
                        readPos += 8;
                    }
                    break;
                case 0x19:
                    {
                        readPos++;
                        int c = BitConverter.ToInt32(dataPassed, readPos); readPos += 4;
                        readPos += 4 * c;
                    }
                    break;
                case 0x1B:
                    {
                        readPos += 4;
                    }
                    break;
                case 0x1C: // deco
                    {
                        readPos += 6;
                    }
                    break;
                case 0x20: // sewing machine
                    {
                        readPos += 4;
                    }
                    break;
                case 0x21:
                    {
                        
                        if (tiles[loc].fg == 3394)
                        {
                            len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                            readPos += len;
                        }
                    }
                    break;
                case 0x23:
                    {
                        readPos += 4;
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        for (int i = 0; i < len; i++) tiles[loc].str_1 += (char)dataPassed[readPos++];
                    }
                    break;
                case 0x25:
                    {
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        for (int i = 0; i < len; i++) tiles[loc].str_1 += (char)dataPassed[readPos++];
                        readPos += 32;
                    }
                    break;
                case 0x27:
                    {
                        // lock-bot
                        readPos += 4;
                    }
                    break;
                case 0x28:
                    {
                        // bg weather
                        readPos += 4;
                    }
                    break;
                case 0x2a:
                    {
                        // data bedrock (used for npc data storing, not properly handled tho, todo)
                        readPos++;
                    }
                    break;
                case 0x2b:
                    {
                        readPos += 16;
                    }
                    break;
                case 0x2c:
                    {
                        readPos++; // skipping owner uid
                        readPos += 4;
                        byte adminCount = dataPassed[readPos];
                        readPos += 4; // guild shit
                        readPos += (adminCount * 4);

                    }
                    break;
                case 0x2f:
                    {
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        for (int i = 0; i < len; i++) tiles[loc].str_1 += (char)dataPassed[readPos++];
                        readPos += 5;
                    }
                    break;
                case 0x30:
                    {
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        for (int i = 0; i < len; i++) tiles[loc].str_1 += (char)dataPassed[readPos++];
                        readPos += 26;
                    }
                    break;
                case 0x31:
                    {
                        // stuff weather
                        readPos += 9;
                    }
                    break;
                case 0x32:
                    {
                        // activity indicator in there, keep skipping as usual...
                        readPos += 4;
                    }
                    break;
                case 0x34:
                    {
                        // Howler => do not serialize or increase bytes read?
                    }
                    break;
                case 0x36:
                    {
                        // storage box xtreme
                        short itemsSize = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        readPos += itemsSize;
                    }
                    break;
                case 0x38:
                    {
                        // lucky token
                        len = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                        for (int i = 0; i < len; i++) tiles[loc].str_1 += (char)dataPassed[readPos++];
                        readPos += 4;
                    }
                    break;
                case 0x39:
                    {
                        // geiger charger
                        readPos += 4;
                        // yeah also wondering why 1 byte wasnt enough to determine the existence of a geiger iprogram :)
                    }
                    break;
                case 0x3a:
                    {
                        // adventure begins = nothing lol:)
                    }
                    break;
                case 0x3e:
                    {
                        readPos += 14;
                    }
                    break;
                case 0x3f:
                    {
                        // cybots
                        int r = BitConverter.ToInt32(dataPassed, readPos); readPos += 4;
                        readPos += (r * 15);
                        readPos += 8;
                    }
                    break;
                case 0x41:
                    {
                        // guild item
                        readPos += 17;
                    }
                    break;
                case 0x42:
                    {
                        // growscan 9000
                        readPos++;
                    }
                    break;
                case 0x49:
                    {
                        // temporary platforms
                        readPos += 4;
                    }
                    break;
                case 0x4a:
                    {
                         // safe vault, nothing inside.
                    }
                    break;
                default:
                    len = 0;
                    break; // unknown tile visual type...
            }

        }

        private int Tile_Serialize(byte[] dataPassed, int loc)
        {
            try
            {
                if (readPos >= dataPassed.Length) return -1;
                ushort fg = BitConverter.ToUInt16(dataPassed, readPos); readPos += 2; // sizeof short
                tiles[loc].fg = fg;
                ushort bg = BitConverter.ToUInt16(dataPassed, readPos); readPos += 2;
                tiles[loc].bg = bg;
                int add = BitConverter.ToInt32(dataPassed, readPos);
                tiles[loc].tileState = add;
                readPos += 4;
                if ((short)add > 0)
                {
                    readPos += 2;
                }
                if (ItemDatabase.RequiresTileExtra(fg)) TileExtra_Serialize(dataPassed, loc);
            }
            catch (ArgumentException)
            {
                return -1;
                //MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): Failed to load tile number " + loc.ToString() + ", total tile count is: " + tileCount.ToString() + "\n");
                //MessageBox.Show(loc.ToString());
            }
            tilesProperlySerialized++;
            return 0;
        }

        private int WorldObjects_Serialize(byte[] dataPassed)
        {
            try
            {
                dropped.id = BitConverter.ToInt16(dataPassed, readPos); readPos += 2;
                dropped.x = BitConverter.ToInt32(dataPassed, readPos); readPos += 4;
                dropped.y = BitConverter.ToInt32(dataPassed, readPos); readPos += 4;
                dropped.itemCount = dataPassed[readPos++];
                dropped.uid = BitConverter.ToInt32(dataPassed, readPos); readPos += 4;
                MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): Serialized +1 dropped item object. ID: " + dropped.id.ToString() + " X:" + dropped.x.ToString() + " Y: " + dropped.y.ToString() + " UID: " + dropped.uid.ToString() + "\n");
            }
            catch (ArgumentException)
            {
                return -1;
            }
            catch (IndexOutOfRangeException)//yeah idk will be looking into it soon
            {
                return -2;
            }
           
            droppedItems.Add(dropped);
            dropped_TOTALUIDS++;
            return 0;
        }

        private void ResetAndInit()
        {

            // free mem
           
            droppedItems.Clear();
            players.Clear();
            tiles = null;

            // now other stuff
            tilesProperlySerialized = 0;
            currentWorld = "";
            dropped_ITEMUID = -1;
            dropped_TOTALUIDS = 0;
            tileCount = 0;
            playerCount = 0;
            readPos = 0;
            netID = -1;
            width = 0;
            height = 0;
        }

        public World LoadMap(byte[] packet)
        {
            try
            {
                ResetAndInit(); // just incase, may be removed when disposing or in here, ill keep it like that tho.
                byte[] data = VariantList.get_extended_data(packet); // agh, maybe puublically declare and use then like that but too late, too lazy  to refactor now.
                if (data.Length < 8192) return this;
                if (data.Length > 200000) return this; // 200kb too big not gonna do that...

                readPos += 6;
                short pLen = BitConverter.ToInt16(data, readPos); readPos += sizeof(short); // 2

                for (int i = 0; i < pLen; i++)
                    currentWorld += (char)data[readPos++];


                width = BitConverter.ToInt32(data, readPos); readPos += sizeof(int);
                height = BitConverter.ToInt32(data, readPos); readPos += sizeof(int);
                tileCount = BitConverter.ToInt32(data, readPos); readPos += sizeof(int);
                tiles = new Tile[tileCount]; // allocate exact tile data.

                MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): Loading world: " + currentWorld + ", total tile count is: " + tileCount.ToString() + "\n");
                if (!MainForm.serializeWorldsAdvanced)
                {
                    MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): NOTE: Advanced world serialization was disabled, not continuing to load tiles.\n");
                    return this;
                }

                for (int i = 0; i < tileCount; i++)
                {
                    if (Tile_Serialize(data, i) != 0) break;
                }
                MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): [" + currentWorld + "]" + " Tiles properly serialized (without any errors): " + tilesProperlySerialized.ToString() + "\n");
                if (readPos >= data.Length) return this;
                //weather n dropped items:
                droppedItems.Clear();
                dropped_ITEMUID = BitConverter.ToInt32(data, readPos); readPos += sizeof(int);
                int count = BitConverter.ToInt32(data, readPos); readPos += sizeof(int);

                /*for (int i = 0; i < count; i++)
                {
                    if (WorldObjects_Serialize(data) != 0)
                    {
                        MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): ERROR! Dropped objects in world '" + currentWorld + "' could not be loaded properly! This might have made the magplant hack/pickup range cheat malfunctioning, its recommended to turn it off now!\n");
                        break;
                    }
                }*/
                //tiles properly serialized: " + tilesProperlySerialized.ToString() + ",
                dropped_ITEMUID = count; // yeah at least if serializing them fails we can atleast get uid count....
                total_bytes_serialized = (ulong)readPos;
                MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): Succeeded loading world '" + currentWorld + "'! Total bytes serialized: " + total_bytes_serialized.ToString() + ",  dropped item uid count: " + count + "\n");
            }
            catch
            {
                return this;
            }
            return this;
        }        
    }
}
