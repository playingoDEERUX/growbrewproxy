using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GrowbrewProxy.Kernys.Bson
{
    public class BSONValue
    {
        protected bool Equals(BSONValue other)
        {
            return Equals(_binary, other._binary) && _bool == other._bool && _dateTime.Equals(other._dateTime) && _double.Equals(other._double) && _int32 == other._int32 && _int64 == other._int64 && _string == other._string && valueType == other.valueType;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BSONValue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _binary != null ? _binary.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ _bool.GetHashCode();
                hashCode = (hashCode * 397) ^ _dateTime.GetHashCode();
                hashCode = (hashCode * 397) ^ _double.GetHashCode();
                hashCode = (hashCode * 397) ^ _int32;
                hashCode = (hashCode * 397) ^ _int64.GetHashCode();
                hashCode = (hashCode * 397) ^ (_string != null ? _string.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) valueType;
                return hashCode;
            }
        }

        public enum ValueType
        {
            Double,
            String,
            Array,
            Binary,
            Boolean,
            UTCDateTime,
            None,
            Int32,
            Int64,
            Object
        }

        private readonly byte[] _binary;
        private readonly bool _bool;
        private readonly DateTime _dateTime;
        private readonly double _double;
        private readonly int _int32;
        private readonly long _int64;
        private readonly string _string;

        ///
        protected BSONValue(ValueType valueType)
        {
            this.valueType = valueType;
        }

        public BSONValue()
        {
            valueType = ValueType.None;
        }

        public BSONValue(double v)
        {
            valueType = ValueType.Double;
            _double = v;
        }

        public BSONValue(string v)
        {
            valueType = ValueType.String;
            _string = v;
        }

        public BSONValue(byte[] v)
        {
            valueType = ValueType.Binary;
            _binary = v;
        }

        public BSONValue(bool v)
        {
            valueType = ValueType.Boolean;
            _bool = v;
        }

        public BSONValue(DateTime dt)
        {
            valueType = ValueType.UTCDateTime;
            _dateTime = dt;
        }

        public BSONValue(int v)
        {
            valueType = ValueType.Int32;
            _int32 = v;
        }

        public BSONValue(long v)
        {
            valueType = ValueType.Int64;
            _int64 = v;
        }

        /*
		protected static BSONValue convert(object obj) {
			if (obj as BSONValue != null)
				return obj as BSONValue;

			if (obj is Int32)
				return new BSONValue (obj as Int32);
			if (obj is Int64)
				return new BSONValue (obj as Int64);
			if (obj is byte[])
				return new BSONValue (obj as byte[]);
			if (obj is DateTime)
				return new BSONValue (obj as DateTime);
			if (obj is string)
				return new BSONValue (obj as string);
			if (obj is bool)
				return new BSONValue (obj as bool);
			if (obj is double)
				return new BSONValue (obj as double);

			throw new InvalidCastException();
		}
		*/

        /// Properties
        public ValueType valueType { get; }

        public double doubleValue
        {
            get
            {
                return valueType switch
                {
                    ValueType.Int32 => _int32,
                    ValueType.Int64 => _int64,
                    ValueType.Double => _double,
                    ValueType.None => float.NaN,
                    _ => throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to double",
                        valueType))
                };
            }
        }

        public int int32Value
        {
            get
            {
                return valueType switch
                {
                    ValueType.Int32 => _int32,
                    ValueType.Int64 => (int) _int64,
                    ValueType.Double => (int) _double,
                    _ => throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to Int32",
                        valueType))
                };
            }
        }

        public long int64Value
        {
            get
            {
                return valueType switch
                {
                    ValueType.Int32 => _int32,
                    ValueType.Int64 => _int64,
                    ValueType.Double => (long) _double,
                    _ => throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to Int64",
                        valueType))
                };
            }
        }

        public byte[] binaryValue
        {
            get
            {
                return valueType switch
                {
                    ValueType.Binary => _binary,
                    _ => throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to binary",
                        valueType))
                };
            }
        }

        public DateTime dateTimeValue
        {
            get
            {
                return valueType switch
                {
                    ValueType.UTCDateTime => _dateTime,
                    _ => throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to DateTime",
                        valueType))
                };
            }
        }

        public string stringValue
        {
            get
            {
                return valueType switch
                {
                    ValueType.Int32 => Convert.ToString(_int32),
                    ValueType.Int64 => Convert.ToString(_int64),
                    ValueType.Double => Convert.ToString(_double, CultureInfo.InvariantCulture),
                    ValueType.String => _string?.TrimEnd((char) 0),
                    ValueType.Boolean => _bool ? "true" : "false",
                    ValueType.Binary => Encoding.UTF8.GetString(_binary).TrimEnd((char) 0),
                    _ => throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to string",
                        valueType))
                };
            }
        }

        public bool boolValue
        {
            get
            {
                return valueType switch
                {
                    ValueType.Boolean => _bool,
                    _ => throw new Exception(string.Format("Original type is {0}. Cannot convert from {0} to bool",
                        valueType))
                };
            }
        }

        public virtual BSONValue this[string key]
        {
            get => null;
            set { }
        }

        public virtual BSONValue this[int index]
        {
            get => null;
            set { }
        }

        public virtual void Clear()
        {
        }

        public virtual void Add(string key, BSONValue value)
        {
        }

        public virtual void Add(BSONValue value)
        {
        }

        public virtual bool Contains(BSONValue v)
        {
            return false;
        }

        public virtual bool ContainsKey(string key)
        {
            return false;
        }

        public static implicit operator BSONValue(double v)
        {
            return new BSONValue(v);
        }

        public static implicit operator BSONValue(int v)
        {
            return new BSONValue(v);
        }

        public static implicit operator BSONValue(long v)
        {
            return new BSONValue(v);
        }

        public static implicit operator BSONValue(byte[] v)
        {
            return new BSONValue(v);
        }

        public static implicit operator BSONValue(DateTime v)
        {
            return new BSONValue(v);
        }

        public static implicit operator BSONValue(string v)
        {
            return new BSONValue(v);
        }

        public static implicit operator BSONValue(bool v)
        {
            return new BSONValue(v);
        }

        public static implicit operator double(BSONValue v)
        {
            return v.doubleValue;
        }

        public static implicit operator int(BSONValue v)
        {
            return v.int32Value;
        }

        public static implicit operator long(BSONValue v)
        {
            return v.int64Value;
        }

        public static implicit operator byte[](BSONValue v)
        {
            return v.binaryValue;
        }

        public static implicit operator DateTime(BSONValue v)
        {
            return v.dateTimeValue;
        }

        public static implicit operator string(BSONValue v)
        {
            return v.stringValue;
        }

        public static implicit operator bool(BSONValue v)
        {
            return v.boolValue;
        }

        public static bool operator ==(BSONValue a, object b)
        {
            return ReferenceEquals(a, b);
        }

        public static bool operator !=(BSONValue a, object b)
        {
            return !(a == b);
        }
    }

    public class BSONObject : BSONValue, IEnumerable
    {
        private readonly Dictionary<string, BSONValue> mMap = new Dictionary<string, BSONValue>();

        public BSONObject()
            : base(ValueType.Object)
        {
        }

        //
        // Properties
        //
        public ICollection<string> Keys => mMap.Keys;

        //
        // Indexer
        //
        public override BSONValue this[string key]
        {
            get => mMap[key];
            set => mMap[key] = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mMap.GetEnumerator();
        }

        //
        // Methods
        //
        public override void Clear()
        {
            mMap.Clear();
        }

        public override void Add(string key, BSONValue value)
        {
            mMap.Add(key, value);
        }

        public override bool Contains(BSONValue v)
        {
            return mMap.ContainsValue(v);
        }

        public override bool ContainsKey(string key)
        {
            return mMap.ContainsKey(key);
        }
    }

    public class BSONArray : BSONValue, IEnumerable
    {
        private readonly List<BSONValue> mList = new List<BSONValue>();

        public BSONArray()
            : base(ValueType.Array)
        {
        }

        //
        // Indexer
        //
        public override BSONValue this[int index]
        {
            get => mList[index];
            set => mList[index] = value;
        }

        public int Count => mList.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        //
        // Methods
        //
        public override void Add(BSONValue v)
        {
            mList.Add(v);
        }

        public override void Clear()
        {
            mList.Clear();
        }
    }

    public class SimpleBSON
    {
        private readonly BinaryReader mBinaryReader;

        private SimpleBSON(byte[] buf = null)
        {
            if (buf == null) return;
            MemoryStream mMemoryStream = new MemoryStream(buf);
            mBinaryReader = new BinaryReader(mMemoryStream);
        }

        public static BSONObject Load(byte[] buf)
        {
            SimpleBSON bson = new SimpleBSON(buf);

            return bson.decodeDocument();
        }

        public static byte[] Dump(BSONObject obj)
        {
            SimpleBSON bson = new SimpleBSON();
            MemoryStream ms = new MemoryStream();

            bson.encodeDocument(ms, obj);

            byte[] buf = new byte[ms.Position];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(buf, 0, buf.Length);

            return buf;
        }

        private BSONValue decodeElement(out string name)
        {
            byte elementType = mBinaryReader.ReadByte();

            if (elementType == 0x01)
            {
                // Double
                name = decodeCString();
                return new BSONValue(mBinaryReader.ReadDouble());
            }

            if (elementType == 0x02)
            {
                // String
                name = decodeCString();
                return new BSONValue(decodeString());
            }

            if (elementType == 0x03)
            {
                // Document
                name = decodeCString();
                return decodeDocument();
            }

            if (elementType == 0x04)
            {
                // Array
                name = decodeCString();
                return decodeArray();
            }

            if (elementType == 0x05)
            {
                // Binary
                name = decodeCString();
                int length = mBinaryReader.ReadInt32();

                return new BSONValue(mBinaryReader.ReadBytes(length));
            }

            if (elementType == 0x08)
            {
                // Boolean
                name = decodeCString();
                return new BSONValue(mBinaryReader.ReadBoolean());
            }

            if (elementType == 0x09)
            {
                // DateTime
                name = decodeCString();
                long time = mBinaryReader.ReadInt64();
                return new BSONValue(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + new TimeSpan(time * 10000));
            }

            if (elementType == 0x0A)
            {
                // None
                name = decodeCString();
                return new BSONValue();
            }

            if (elementType == 0x10)
            {
                // Int32
                name = decodeCString();
                return new BSONValue(mBinaryReader.ReadInt32());
            }

            if (elementType != 0x12) throw new Exception($"Don't know elementType={elementType}");
            // Int64
            name = decodeCString();
            return new BSONValue(mBinaryReader.ReadInt64());

        }

        private BSONObject decodeDocument()
        {
            int length = mBinaryReader.ReadInt32() - 4;

            BSONObject obj = new BSONObject();

            int i = (int) mBinaryReader.BaseStream.Position;
            while (mBinaryReader.BaseStream.Position < i + length - 1)
            {
                BSONValue value = decodeElement(out string name);
                obj.Add(name, value);
            }

            mBinaryReader.ReadByte(); // zero
            return obj;
        }

        private BSONArray decodeArray()
        {
            BSONObject obj = decodeDocument();

            int i = 0;
            BSONArray array = new BSONArray();
            while (obj.ContainsKey(Convert.ToString(i)))
            {
                array.Add(obj[Convert.ToString(i)]);

                i += 1;
            }

            return array;
        }

        private string decodeString()
        {
            int length = mBinaryReader.ReadInt32();
            byte[] buf = mBinaryReader.ReadBytes(length);

            return Encoding.UTF8.GetString(buf);
        }

        private string decodeCString()
        {
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                byte buf = mBinaryReader.ReadByte();
                if (buf == 0)
                    break;
                ms.WriteByte(buf);
            }

            return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int) ms.Position);
        }

        private void encodeElement(MemoryStream ms, string name, BSONValue v)
        {
            switch (v.valueType)
            {
                case BSONValue.ValueType.Double:
                    ms.WriteByte(0x01);
                    encodeCString(ms, name);
                    encodeDouble(ms, v.doubleValue);
                    return;

                case BSONValue.ValueType.String:
                    ms.WriteByte(0x02);
                    encodeCString(ms, name);
                    encodeString(ms, v.stringValue);
                    return;

                case BSONValue.ValueType.Object:
                    ms.WriteByte(0x03);
                    encodeCString(ms, name);
                    encodeDocument(ms, v as BSONObject);
                    return;

                case BSONValue.ValueType.Array:
                    ms.WriteByte(0x04);
                    encodeCString(ms, name);
                    encodeArray(ms, v as BSONArray);
                    return;

                case BSONValue.ValueType.Binary:
                    ms.WriteByte(0x05);
                    encodeCString(ms, name);
                    encodeBinary(ms, v.binaryValue);
                    return;

                case BSONValue.ValueType.Boolean:
                    ms.WriteByte(0x08);
                    encodeCString(ms, name);
                    encodeBool(ms, v.boolValue);
                    return;

                case BSONValue.ValueType.UTCDateTime:
                    ms.WriteByte(0x09);
                    encodeCString(ms, name);
                    encodeUTCDateTime(ms, v.dateTimeValue);
                    return;

                case BSONValue.ValueType.None:
                    ms.WriteByte(0x0A);
                    encodeCString(ms, name);
                    return;

                case BSONValue.ValueType.Int32:
                    ms.WriteByte(0x10);
                    encodeCString(ms, name);
                    encodeInt32(ms, v.int32Value);
                    return;

                case BSONValue.ValueType.Int64:
                    ms.WriteByte(0x12);
                    encodeCString(ms, name);
                    encodeInt64(ms, v.int64Value);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void encodeDocument(Stream ms, BSONObject obj)
        {
            MemoryStream dms = new MemoryStream();
            foreach (string str in obj.Keys) encodeElement(dms, str, obj[str]);

            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write((int) (dms.Position + 4 + 1));
            bw.Write(dms.GetBuffer(), 0, (int) dms.Position);
            bw.Write((byte) 0);
        }

        private void encodeArray(Stream ms, BSONArray lst)
        {
            BSONObject obj = new BSONObject();
            for (int i = 0; i < lst.Count; ++i) obj.Add(Convert.ToString(i), lst[i]);

            encodeDocument(ms, obj);
        }

        private static void encodeBinary(Stream ms, byte[] buf)
        {
            byte[] aBuf = BitConverter.GetBytes(buf.Length);
            ms.Write(aBuf, 0, aBuf.Length);
            ms.WriteByte(0);
            ms.Write(buf, 0, buf.Length);
        }

        private void encodeCString(MemoryStream ms, string v)
        {
            byte[] buf = new UTF8Encoding().GetBytes(v);
            ms.Write(buf, 0, buf.Length);
            ms.WriteByte(0);
        }

        private void encodeString(MemoryStream ms, string v)
        {
            byte[] strBuf = new UTF8Encoding().GetBytes(v);
            byte[] buf = BitConverter.GetBytes(strBuf.Length + 1);

            ms.Write(buf, 0, buf.Length);
            ms.Write(strBuf, 0, strBuf.Length);
            ms.WriteByte(0);
        }

        private void encodeDouble(MemoryStream ms, double v)
        {
            byte[] buf = BitConverter.GetBytes(v);
            ms.Write(buf, 0, buf.Length);
        }

        private void encodeBool(MemoryStream ms, bool v)
        {
            byte[] buf = BitConverter.GetBytes(v);
            ms.Write(buf, 0, buf.Length);
        }

        private void encodeInt32(MemoryStream ms, int v)
        {
            byte[] buf = BitConverter.GetBytes(v);
            ms.Write(buf, 0, buf.Length);
        }

        private void encodeInt64(MemoryStream ms, long v)
        {
            byte[] buf = BitConverter.GetBytes(v);
            ms.Write(buf, 0, buf.Length);
        }

        private void encodeUTCDateTime(MemoryStream ms, DateTime dt)
        {
            TimeSpan span;
            if (dt.Kind == DateTimeKind.Local)
                span = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            else
                span = dt - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            byte[] buf = BitConverter.GetBytes((long) (span.TotalSeconds * 1000));
            ms.Write(buf, 0, buf.Length);
        }
    }
}