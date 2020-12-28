using System;
using System.Collections.Generic;
using System.IO;

namespace Com4Love.Qmax.Net.Protocols
{
	public class TestProtocol: Protocol
	{
		public byte byteData;
		public short shortData;
		public int intData;
		public long longData;
		public float floatData;
		public double doubleData;
		public string strData;
		
		public List<int> list;		
		public Dictionary<Byte, String> dict;
		
		public TestProtocol(byte byteData, short shortData, int intData, long longData, float floatData, 
		                    double doubleData, string strData,
		                    List<int> list, Dictionary<Byte, String> dict)
		{
			this.byteData = byteData;
			this.shortData = shortData;
			this.intData = intData;
			this.longData = longData;
			this.floatData = floatData;
			this.doubleData = doubleData;
			this.strData = strData;
			this.list = list;
			this.dict = dict;
		}


        public override int Deserialize(Stream stream, bool isLittleEndian = true)
		{
            long len = stream.Length;
            byteData = (byte)stream.ReadByte();
            shortData = Serializer.ReadInt16(stream, isLittleEndian);
            intData = Serializer.ReadInt32(stream, isLittleEndian);
            longData = Serializer.ReadInt64(stream, isLittleEndian);
            floatData = Serializer.ReadSingle(stream, isLittleEndian);
            doubleData = Serializer.ReadDouble(stream, isLittleEndian);
            strData = Serializer.ReadString(stream, isLittleEndian);
            list = Serializer.ReadList<int>(stream, isLittleEndian);
            dict = Serializer.ReadDict<byte, string>(stream, isLittleEndian);
            return (int)(len - stream.Length);
		}

        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stream.WriteByte(byteData);
            Serializer.ToBytes(stream, shortData, false);
            Serializer.ToBytes(stream, intData, false);
            Serializer.ToBytes(stream, longData, false);
            Serializer.ToBytes(stream, floatData, false);
            Serializer.ToBytes(stream, doubleData, false);
            Serializer.ToBytes(stream, strData, false);
            Serializer.ToBytes(stream, list, false);
            Serializer.ToBytes(stream, dict, false);
            return (int)(stream.Length - len);
        }


		public override string ToString ()
		{
			return string.Format ("[TestProtocol]: short={0}, " +
			                      "intData={1}, " +
			                      "longData={2}, " +
			                      "floatData={3}, " +
			                      "doubleData={4}, " +
			                      "strData={5}, " +
			                      "list.Count={6}, " +
			                      "dict.Count={7}",
			                      shortData, intData, longData, floatData, doubleData, strData,
			                      list, dict);
		}
	};  
}

