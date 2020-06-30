using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ENet.Managed;
using System.Windows.Forms;


namespace GrowbrewProxy
{
    class GamePacketProton
    {
        // thanks to iProgramInCpp#0489, most things are made by him in the GrowtopiaCustomClient, I have just rewritten it into c# and maybe also improved. -playingo
        // NOTE: does not have the ability to use multiple args as appending values, did not want to include this in here at least!!
        public List<object> objects = new List<object>();
        public int NetID = -1;
        public int delay = 0;

        public struct Vector2
        {
            public float x, y;
            public Vector2(float x_, float y_)
            {
                x = x_; y = y_;
            }
            public Vector2 Zero => new Vector2(0, 0);
        }
        public struct Vector3
        {
            public float x, y, z;
            public Vector3(float x_, float y_, float z_)
            {
                x = x_; y = y_; z = z_;
            }
            public Vector3 Zero => new Vector3(0, 0, 0);
        }
        public struct Rectf
        {
            public float x, y, w, h;
            public Rectf(float x_, float y_, float w_, float h_)
            {
                x = x_; y = y_; w = w_; h = h_;
            }
            public Rectf Zero => new Rectf(0, 0, 0, 0);
        }

        public void AppendInt(int i)
        {
            objects.Add(i);
        }
        public void AppendUInt(uint ui)
        {
            objects.Add(ui);
        }
        public void AppendString(string s)
        {
            objects.Add(s);
        }
        public void AppendFloat(float v)
        {
            objects.Add(v);
        }
        public void AppendVector(Vector2 v)
        {
            objects.Add(v);
        }
        public void AppendVector(Vector3 v)
        {
            objects.Add(v);
        }
        public void AppendRect(Rectf v)
        {
            objects.Add(v);
        }
        public GamePacketProton() { }

        public byte[] GetBytes()
        {
            byte[] bytesArray = new byte[61];
            bytesArray[0] = 4; // message type
            bytesArray[4] = 1; // TankPacket type
            Array.Copy(BitConverter.GetBytes(NetID), 0, bytesArray, 8, 4);
            Array.Copy(BitConverter.GetBytes(8), 0, bytesArray, 16, 4);
            Array.Copy(BitConverter.GetBytes(delay), 0, bytesArray, 24, 4);
            Array.Copy(BitConverter.GetBytes(objects.Count), 0, bytesArray, 56, 4);

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 61; i++) bytes.Add(bytesArray[i]);

            // Now we need to loop through all the objects.
            byte ind = 0;
            foreach (object obj in objects)
            {
                if (obj is int)
                {
                    // appendInt
                    byte[] data2 = new byte[6];
                    byte[] data = BitConverter.GetBytes((int)obj);
                    Array.Copy(data, 0, data2, 2, 4);
                    data2[0] = ind;
                    data2[1] = 0x9;
                    bytes.AddRange(data2);
                }
                else
                if (obj is uint)
                {
                    // appendUint32
                    byte[] data2 = new byte[6];
                    byte[] data = BitConverter.GetBytes((uint)obj);
                    Array.Copy(data, 0, data2, 2, 4);
                    data2[0] = ind;
                    data2[1] = 0x5;
                    bytes.AddRange(data2);
                }
                else
                if (obj is string)
                {
                    // appendString
                    string str = (string)obj;
                    byte[] data = Encoding.ASCII.GetBytes(str);
                    byte[] data2 = new byte[2 + data.Length + 4];
                    int length = data.Length;
                    Array.Copy(BitConverter.GetBytes(length), 0, data2, 2, 4);
                    Array.Copy(data, 0, data2, 6, data.Length);
                    data2[0] = ind;
                    data2[1] = 0x2;
                    bytes.AddRange(data2);
                }
                else
                if (obj is float)
                {
                    // appendFloat
                    byte[] data2 = new byte[6];
                    byte[] data = BitConverter.GetBytes((float)obj);
                    Array.Copy(data, 0, data2, 2, 4);
                    data2[0] = ind;
                    data2[1] = 0x1;
                    bytes.AddRange(data2);
                }
                else
                if (obj is Vector2)
                {
                    // appendVec
                    Vector2 vec = (Vector2)obj;
                    byte[] data2 = new byte[2 + 4 * 2];
                    byte[] data_1 = BitConverter.GetBytes(vec.x);
                    byte[] data_2 = BitConverter.GetBytes(vec.y);
                    Array.Copy(data_1, 0, data2, 2, 4);
                    Array.Copy(data_2, 0, data2, 6, 4);
                    data2[0] = ind;
                    data2[1] = 0x3;
                    bytes.AddRange(data2);
                }
                else
                if (obj is Vector3)
                {
                    // appendVec3
                    Vector3 vec = (Vector3)obj;
                    byte[] data2 = new byte[2 + 4 * 3];
                    byte[] data_1 = BitConverter.GetBytes(vec.x);
                    byte[] data_2 = BitConverter.GetBytes(vec.y);
                    byte[] data_3 = BitConverter.GetBytes(vec.z);
                    Array.Copy(data_1, 0, data2, 2, 4);
                    Array.Copy(data_2, 0, data2, 6, 4);
                    Array.Copy(data_3, 0, data2, 10, 4);
                    data2[0] = ind;
                    data2[1] = 0x4;
                    bytes.AddRange(data2);
                }
                // idk about rectangle but it's not really used anyway so meh
                else
                {
                    throw new InvalidOperationException($"Failed to write {obj.GetType().ToString()} object to packet data.");
                }
                ind++;
            }

            bytes.Add(0);
            byte[] basdf = bytes.ToArray();
            basdf[60] = ind;
            //Array.Copy(BitConverter.GetBytes(basdf.Length - 56), 0, basdf, 60, 4);
            return basdf;
        }
    };

    class VariantList
    {
        // this class has been entirely made by me, based on the code available on the gt bot of anybody :)
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        public struct VarList
        {
            public string FunctionName;
            public int netID;           
            public uint delay;           
            public object[] functionArgs;
        };

        public enum OnSendToServerArgs
        {
            port = 1,
            token,
            userId,
            IPWithExtraData = 4
        };

        public static byte[] get_extended_data(byte[] pktData)
        {        
            return pktData.Skip(56).ToArray();
        }

        public static byte[] get_struct_data(byte[] package)
        {
            int packetLen = package.Length;
            if (packetLen >= 0x3c)
            {
                byte[] structPackage = new byte[packetLen - 4];
                Array.Copy(package, 4, structPackage, 0, packetLen - 4);
                int p2Len = BitConverter.ToInt32(package, 56);
                if (((byte)(package[16]) & 8) != 0)
                {
                    if (packetLen < p2Len + 60)
                    {
                        MainForm.LogText += ("[" + DateTime.UtcNow + "] (PROXY): (ERROR) Too small extended packet to be valid!\n");
                    }
                }
                else
                {
                    Array.Copy(BitConverter.GetBytes(0), 0, package, 56, 4);
                }
                return structPackage;
            }
            return null;
        }

        public static VarList GetCall(byte[] package)
        {
           
            VarList varList = new VarList();
            //if (package.Length < 60) return varList;
            int pos = 0;
            //varList.netID = BitConverter.ToInt32(package, 8);
            //varList.delay = BitConverter.ToUInt32(package, 24);
            byte argsTotal = package[pos];
            pos++;
                if (argsTotal > 7) return varList;
                varList.functionArgs = new object[argsTotal];

                for (int i = 0; i < argsTotal; i++)
                {
                    varList.functionArgs[i] = 0; // just to be sure...
                    byte index = package[pos]; pos++; // pls dont bully sm
                    byte type = package[pos]; pos++;


                    switch (type)
                    {
                        case 1:
                            {
                                float vFloat = BitConverter.ToUInt32(package, pos); pos += 4;
                                varList.functionArgs[index] = vFloat;
                                break;
                            }
                        case 2: // string
                            int strLen = BitConverter.ToInt32(package, pos); pos += 4;
                            string v = string.Empty;
                            v = Encoding.ASCII.GetString(package, pos, strLen); pos += strLen;

                            if (index == 0)
                                 varList.FunctionName = v;

                            if (index > 0)
                            {
                                if (varList.FunctionName == "OnSendToServer") // exceptionary function, having it easier like this :)
                                {
                                    MainForm.doorid = v.Substring(v.IndexOf("|") + 1); // doorid
                                    if (v.Length >= 8)
                                        v = v.Substring(0, v.IndexOf("|"));
                                }

                                varList.functionArgs[index] = v;
                            }
                            break;
                        case 5: // uint
                            uint vUInt = BitConverter.ToUInt32(package, pos); pos += 4;
                            varList.functionArgs[index] = vUInt;
                            break;
                        case 9: // int (can hold negative values, of course they are always casted but its just a sign from the server that the value was intended to hold negative values as well)
                            int vInt = BitConverter.ToInt32(package, pos); pos += 4;
                            varList.functionArgs[index] = vInt;
                            break;
                        default:
                            break;
                    }
                }
            return varList;
        }
    }
}
