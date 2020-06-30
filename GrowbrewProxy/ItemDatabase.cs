// thanks to iProgramInCpp#0489, most things are made by him in the GrowtopiaCustomClient, 
// I have just rewritten it into c# and maybe also improved. -playingo
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrowbrewProxy
{
    public class ItemDatabase
    {
        public struct ItemDefinition
        {
            public short id;
            public byte editType;
            public byte editCategory;
            public byte actionType;
            public byte hitSound;
            public string itemName;
            public string fileName;
            public int texHash;
            public byte itemKind;
            public byte texX;
            public byte texY;
            public byte sprType;
            public byte isStripey;
            public byte collType;
            public byte hitsTaken;
            public byte dropChance;
            public int clothingType;
            public short rarity;
            public short toolKind;
            public string audioFile;
            public int audioHash;
            public short audioVol;
            public byte seedBase;
            public byte seedOver;
            public byte treeBase;
            public byte treeOver;
            // Colors are stored in ARGB.
            public byte color1R, color1G, color1B, color1A;
            public byte color2R, color2G, color2B, color2A;
            public short ing1, ing2;
            public int growTime;
            public string extraUnk01;
            public string extraUnk02;
            public string extraUnk03;
            public string extraUnk04;
            public string extraUnk05;
            public string extraUnk11;
            public string extraUnk12;
            public string extraUnk13;
            public string extraUnk14;
            public string extraUnk15;
            public short extraUnkShort1;
            public short extraUnkShort2;
            public int extraUnkInt1;
        };

        public static List<ItemDefinition> itemDefs = new List<ItemDefinition>();

        public static bool isBackground(int itemID) // thanks for the dev iProgramInCpp for telling me a reliable method on how to determine between foreground and background in GT.
        {
            ItemDefinition def = GetItemDef(itemID);
            byte actType = def.actionType;
            return (actType == 18 || actType == 23 || actType == 28);
        }
        public static ItemDefinition GetItemDef(int itemID)
        {
            if (itemID < 0 || itemID > (int)itemDefs.Count()) return itemDefs[0];
            ItemDefinition def = itemDefs[itemID];
            if (def.id != itemID)
            {
                // For some reason, something is off.
                foreach (var d in itemDefs)
                {
                    if (d.id == itemID) return d;
                }
            }
            return def;
        }

        public static bool RequiresTileExtra(int id)
        {
            ItemDefinition def = GetItemDef(id);
            return
                def.actionType == 2 || // Door
                def.actionType == 3 || // Lock
                def.actionType == 10 || // Sign
                def.actionType == 13 || // Main Door
                def.actionType == 19 || // Seed
                def.actionType == 26 || // Portal
                def.actionType == 33 || // Mailbox
                def.actionType == 34 || // Bulletin Board
                def.actionType == 36 || // Dice Block
                def.actionType == 36 || // Roshambo Block
                def.actionType == 38 || // Chemical Source
                def.actionType == 40 || // Achievement Block
                def.actionType == 43 || // Sungate
                def.actionType == 46 ||
                def.actionType == 47 ||
                def.actionType == 49 ||
                def.actionType == 50 ||
                def.actionType == 51 || // Bunny Egg
                def.actionType == 52 ||
                def.actionType == 53 ||
                def.actionType == 54 || // Xenonite
                def.actionType == 55 || // Phone Booth
                def.actionType == 56 || // Crystal
                def.id == 2246 || // Crystal
                def.actionType == 57 || // Crime In Progress
                def.actionType == 59 || // Spotlight
                def.actionType == 61 ||
                def.actionType == 62 ||
                def.actionType == 63 || // Fish Wall Port
                def.id == 3760 || // Data Bedrock
                def.actionType == 66 || // Forge
                def.actionType == 67 || // Giving Tree
                def.actionType == 73 || // Sewing Machine
                def.actionType == 74 ||
                def.actionType == 76 || // Painting Easel
                def.actionType == 78 || // Pet Trainer (WHY?!)
                def.actionType == 80 || // Lock-Bot (Why?!)
                def.actionType == 81 ||
                def.actionType == 83 || // Display Shelf
                def.actionType == 84 ||
                def.actionType == 85 || // Challenge Timer
                def.actionType == 86 || // Challenge Start/End Flags
                def.actionType == 87 || // Fish Wall Mount
                def.actionType == 88 || // Portrait
                def.actionType == 89 ||
                def.actionType == 91 || // Fossil Prep Station
                def.actionType == 93 || // Howler
                def.actionType == 97 || // Storage Box Xtreme / Untrade-a-box
                def.actionType == 100 || // Geiger Charger
                def.actionType == 101 ||
                def.actionType == 111 || // Magplant
                def.actionType == 113 || // CyBot
                def.actionType == 115 || // Lucky Token
                def.actionType == 116 || // GrowScan 9000 ???
                def.actionType == 127 || // Temp. Platform
                def.actionType == 130 ||
                (def.id % 2 == 0 && def.id >= 5818 && def.id <= 5932) ||
                // ...
                false;
        }

        public void SetupItemDefs()
        {
            string a = File.ReadAllText("include/base.txt");
            List<string> aaa = a.Split('|').ToList();
            if (aaa.Count < 3) return;
            int itemCount = -1;
            int.TryParse(aaa[2], out itemCount);
            if (itemCount == -1) return;
            short id = 0;
            itemDefs.Clear();
            ItemDefinition def = new ItemDefinition();
            using (StreamReader sr = File.OpenText("include/item_defs.txt"))
            {
                string s = String.Empty;
                while ((s = sr.ReadLine()) != null)
                {
                    if (s.Length < 2) continue;
                    if (s.Contains("//")) continue;
                    List<string> infos = s.Split('\\').ToList();
                    if (infos[0] != "add_item") continue;
                   
                    def.id = short.Parse(infos[1]);
                    def.actionType = byte.Parse(infos[4]);
                    def.itemName = infos[6];

                    if (def.id != id)
                    {
                        // unordered db item, can cause problems!!

                    }
                    itemDefs.Add(def);
                    id++;
                }
            }
        }
    }
}
