using System;
using System.Net;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax.Net.Protocols;

namespace Com4Love.Qmax
{
    public class Serializer
    {
        #region byte[] ToBytes(T)
        static public byte[] ToBytes(byte value)
        {
            byte[] ret = { value };
            return ret;
        }
        static public byte[] ToBytes(short value, bool isLittleEndian = true)
        {
            if (!isLittleEndian)
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            return BitConverter.GetBytes(value);
        }
        static public byte[] ToBytes(int value, bool isLittleEndian = true)
        {
            if (!isLittleEndian)
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            return BitConverter.GetBytes(value);
        }
        static public byte[] ToBytes(long value, bool isLittleEndian = true)
        {
            if (!isLittleEndian)
                return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value));
            return BitConverter.GetBytes(value);
        }

        static public byte[] ToBytes(float value, bool isLittleEndian = true)
        {
            byte[] ret = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian != isLittleEndian)
                Array.Reverse(ret);
            return ret;
        }

        static public byte[] ToBytes(double value, bool isLittleEndian = true)
        {
            byte[] ret = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian != isLittleEndian)
                Array.Reverse(ret);
            return ret;
        }

        static public byte[] ToBytes(string value, bool isLittleEndian = true)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            byte[] lenBytes = ToBytes((short)valueBytes.Length, isLittleEndian);
            byte[] ret = new byte[valueBytes.Length + lenBytes.Length];
            valueBytes.CopyTo(ret, 0);
            lenBytes.CopyTo(ret, valueBytes.Length);
            return ret;
        }

        static public byte[] ToBytes(IList value, bool isLittleEndian = true)
        {
            MemoryStream stream = new MemoryStream();
            ToBytes(stream, value, isLittleEndian);
            return stream.ToArray();
        }

        static public byte[] ToBytes(IDictionary value, bool isLittleEndian = true)
        {
            MemoryStream stream = new MemoryStream();
            ToBytes(stream, value, isLittleEndian);
            return stream.ToArray();
        }

        static public byte[] ToBytes(IProtocol value, bool isLittleEndian = true)
        {
            return value.Serialize(isLittleEndian);
        }
        #endregion

        #region int ToBytes(Stream, T)
        static public int ToBytes(Stream stream, byte value)
        {
            stream.WriteByte(value);
            return 1;
        }
        static public int ToBytes(Stream stream, short value, bool isLittleEndian = true)
        {
            byte[] bytes = ToBytes(value, isLittleEndian);
            stream.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }
        static public int ToBytes(Stream stream, int value, bool isLittleEndian = true)
        {
            byte[] bytes = ToBytes(value, isLittleEndian);
            stream.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }
        static public int ToBytes(Stream stream, long value, bool isLittleEndian = true)
        {
            byte[] bytes = ToBytes(value, isLittleEndian);
            stream.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }
        static public int ToBytes(Stream stream, float value, bool isLittleEndian = true)
        {
            byte[] bytes = ToBytes(value, isLittleEndian);
            stream.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }
        static public int ToBytes(Stream stream, double value, bool isLittleEndian = true)
        {
            byte[] bytes = ToBytes(value, isLittleEndian);
            stream.Write(bytes, 0, bytes.Length);
            return bytes.Length;
        }
        static public int ToBytes(Stream stream, string value, bool isLittleEndian = true)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            ToBytes(stream, (short)valueBytes.Length, isLittleEndian);
            stream.Write(valueBytes, 0, valueBytes.Length);
            return (int)stream.Length;
        }

        static public int ToBytes(Stream stream, IList value, bool isLittleEndian = true)
        {
            ToBytes(stream, (short)value.Count, isLittleEndian);
            int len = 2;
            if (value.Count == 0)
                return len;
            object obj = value[0];
            Type type = obj.GetType();
            //这里如果写成一个for，在for里做类型判断，那么就每个类型都需要做一个类型判断
            //而改成类型判断完，再做for循环，就只要做一次类型判断
            if (type == typeof(byte))
                for (int i = 0, n = value.Count; i < n; i++, len++)
                    stream.WriteByte((byte)value[i]);
            else if (type == typeof(short))
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (short)value[i], isLittleEndian);
            else if (type == typeof(int))
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (int)value[i], isLittleEndian);
            else if (type == typeof(long))
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (long)value[i], isLittleEndian);
            else if (type == typeof(float))
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (float)value[i], isLittleEndian);
            else if (type == typeof(double))
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (double)value[i], isLittleEndian);
            else if (type == typeof(string))
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (string)value[i], isLittleEndian);
            else if (obj is IList)
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (IList)value[i], isLittleEndian);
            else if (obj is IDictionary)
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (IDictionary)value[i], isLittleEndian);
            else if (obj is IProtocol)
                for (int i = 0, n = value.Count; i < n; i++)
                    len += ToBytes(stream, (IProtocol)value[i], isLittleEndian);
            else
                Q.Assert(false);

            return len;
        }

        static public int ToBytes(Stream stream, IDictionary value, bool isLittleEndian = true)
        {
            int len = 0;
            len += ToBytes(stream, (short)value.Count, isLittleEndian);
            foreach (object key in value.Keys)
            {                
                len += ToBytes(stream, key, isLittleEndian);
                len += ToBytes(stream, value[key], isLittleEndian);
            }
            return len;
        }

        static public int ToBytes(Stream stream, IProtocol value, bool isLittleEndian = true)
        {
            return value.Serialize(stream, isLittleEndian);
        }

        static public int ToBytes(Stream stream, object value, bool isLittleEndian = true)
        {
            if (value is byte)
                stream.WriteByte((byte)value);
            else if (value is short)
                ToBytes(stream, (short)value, isLittleEndian);
            else if (value is int)
                ToBytes(stream, (int)value, isLittleEndian);
            else if (value is long)
                ToBytes(stream, (long)value, isLittleEndian);
            else if (value is float)
                ToBytes(stream, (float)value, isLittleEndian);
            else if (value is double)
                ToBytes(stream, (double)value, isLittleEndian);
            else if (value is string)
                ToBytes(stream, (string)value, isLittleEndian);
            return 0;
        }
        #endregion

        #region T ReadType(stream)
        static public byte ReadByte(Stream stream)
        {
            return Convert.ToByte(stream.ReadByte());
        }
        static public short ReadInt16(Stream stream, bool isLittleEndian = true)
        {
            byte[] bytes = new byte[2];
            stream.Read(bytes, 0, 2);
            if (BitConverter.IsLittleEndian != isLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }
        static public int ReadInt32(Stream stream, bool isLittleEndian = true)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            if (BitConverter.IsLittleEndian != isLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
        static public long ReadInt64(Stream stream, bool isLittleEndian = true)
        {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian != isLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }
        static public float ReadSingle(Stream stream, bool isLittleEndian = true)
        {
            byte[] bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            if (BitConverter.IsLittleEndian != isLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }
        static public double ReadDouble(Stream stream, bool isLittleEndian = true)
        {
            byte[] bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            if (BitConverter.IsLittleEndian != isLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        static public String ReadString(Stream stream, bool isLittleEndian = true)
        {
            short strLen = ReadInt16(stream, isLittleEndian);
            byte[] bytes = new byte[strLen];
            stream.Read(bytes, 0, strLen);
            return Encoding.UTF8.GetString(bytes);
        }


        static public Dictionary<TKey, TValue> ReadDict<TKey, TValue>(Stream stream, bool isLittleEndian = true)
        {
            short dictLen = ReadInt16(stream, isLittleEndian);
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(dictLen);
            for (int i = 0; i < dictLen; i++)
            {
                TKey key = Read<TKey>(stream, isLittleEndian);
                TValue value = Read<TValue>(stream, isLittleEndian);
                ret.Add(key, value);
            }
            return ret;
        }

        static public T ReadProtocol<T>(Stream stream, bool isLittleEndian = true)
        {
            IProtocol item = (IProtocol)Activator.CreateInstance(typeof(T));
            try
            {
                item.Deserialize(stream, isLittleEndian);
            }
            catch (Exception e)
            {
                Q.Log("ReadProtocol<T> Error: " + e.Message);
                return default(T);
            }

            return (T)item;
        }

        static public List<T> ReadList<T>(Stream stream, bool isLittleEndian = true)
        {
            short listLen = ReadInt16(stream, isLittleEndian);
            List<T> ret = new List<T>(listLen);
            for (int i = 0; i < listLen; i++)
            {
                T item = Read<T>(stream, isLittleEndian);
                ret.Add(item);
            }
            return ret;
        }


        static public byte[] ReadByteArray(Stream stream, bool isLittleEndian = true)
        {
            int listLen = ReadInt32(stream, isLittleEndian);
            byte[] ret = new byte[listLen];
            stream.Read(ret, 0, listLen);
            return ret;
        }

        #endregion


        #region Type ReadType(byte[], int);
        static public short ReadByte(byte[] bytes, int startIndex)
        {
            return bytes[startIndex];
        }

        static public short ReadInt16(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            byte[] t = bytes;
            if (BitConverter.IsLittleEndian != isLittleEndian)
            {
                t = new byte[2];
                Array.Copy(bytes, startIndex, t, 0, 2);
                Array.Reverse(t);
            }
            return BitConverter.ToInt16(t, 0);
        }

        static public int ReadInt32(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            byte[] t = bytes;
            if (BitConverter.IsLittleEndian != isLittleEndian)
            {
                t = new byte[4];
                Array.Copy(bytes, startIndex, t, 0, 4);
                Array.Reverse(t);
            }
            return BitConverter.ToInt32(t, 0);
        }

        static public long ReadInt64(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            byte[] t = bytes;
            if (BitConverter.IsLittleEndian != isLittleEndian)
            {
                t = new byte[8];
                Array.Copy(bytes, startIndex, t, 0, 8);
                Array.Reverse(t);
            }
            return BitConverter.ToInt64(t, 0);
        }

        static public float ReadSingle(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            byte[] t = bytes;
            if (BitConverter.IsLittleEndian != isLittleEndian)
            {
                t = new byte[4];
                Array.Copy(bytes, startIndex, t, 0, 4);
                Array.Reverse(t);
            }
            return BitConverter.ToSingle(t, 0);
        }

        static public double ReadDouble(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            byte[] t = bytes;
            if (BitConverter.IsLittleEndian != isLittleEndian)
            {
                t = new byte[8];
                Array.Copy(bytes, startIndex, t, 0, 8);
                Array.Reverse(t);
            }
            return BitConverter.ToDouble(t, 0);
        }

        static public String ReadString(byte[] bytes, int startIndex, out int len, bool isLittleEndian = true)
        {
            short strLen = ReadInt16(bytes, startIndex, isLittleEndian);
            len = 2 + strLen;
            return Encoding.UTF8.GetString(bytes, startIndex + 2, strLen);
        }

        static public String ReadString(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            int len = 0;
            return ReadString(bytes, startIndex, out len, isLittleEndian);
        }

        static public List<T> ReadList<T>(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            short len = ReadInt16(bytes, startIndex, isLittleEndian);
            if (len == 0)
                return null;

            List<T> list = new List<T>(len);
            int offset = startIndex + 2;
            for (short i = 0; i < len; i++)
            {
                int l = 0;
                T item = Read<T>(bytes, offset, out l, isLittleEndian);
                offset += l;
                list.Add(item);
            }
            return list;
        }

        static public List<T> ReadList<T>(byte[] bytes, int startIndex, out int len, bool isLittleEndian = true)
        {
            short listCount = ReadInt16(bytes, startIndex, isLittleEndian);
            len = 2;
            if (listCount == 0)
                return null;

            List<T> list = new List<T>(listCount);
            int offset = startIndex + 2;
            for (short i = 0; i < listCount; i++)
            {
                int l = 0;
                T item = Read<T>(bytes, offset, out l, isLittleEndian);
                offset += l;
                list.Add(item);
            }
            len = offset - startIndex;
            return list;
        }

        static public Dictionary<TKey, TValue> ReadDict<TKey, TValue>(byte[] bytes, int startIndex, out int len, bool isLittleEndian = true)
        {
            short dictCount = ReadInt16(bytes, startIndex, isLittleEndian);
            len = 2;
            if (dictCount == 0)
                return null;
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>();
            int offset = startIndex + 2;
            for (short i = 0; i < dictCount; i++)
            {
                int l = 0;
                TKey k = Read<TKey>(bytes, offset, out l, isLittleEndian);
                offset += l;
                TValue v = Read<TValue>(bytes, offset, out l, isLittleEndian);
                offset += l;
                ret.Add(k, v);
            }
            len = offset - startIndex;
            return ret;
        }
        static public Dictionary<TKey, TValue> ReadDict<TKey, TValue>(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            int l = 0;
            return ReadDict<TKey, TValue>(bytes, startIndex, out l, isLittleEndian);
        }


        static public T ReadProtocol<T>(byte[] bytes, int startIndex, out int len, bool isLittleEndian = true)
        {
            IProtocol item = (IProtocol)Activator.CreateInstance(typeof(T));
            try
            {
                len = item.Deserialize(bytes, startIndex, isLittleEndian);
            }
            catch (Exception e)
            {
                Q.Log("ReadProtocol<T> Error: " + e.Message);
                len = 0;
                return default(T);
            }

            return (T)item;
        }
        static public T ReadProtocol<T>(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            int l;
            return ReadProtocol<T>(bytes, startIndex, out l, isLittleEndian);
        }
        #endregion

        static public T Read<T>(byte[] bytes, int startIndex, out int len, bool isLittleEndian = true)
        {
            T ret;
            if (typeof(T) == typeof(byte))
            {
                ret = (T)(object)bytes[startIndex];
                len = 1;
            }
            else if (typeof(T) == typeof(short))
            {
                ret = (T)(object)ReadInt16(bytes, startIndex, isLittleEndian);
                len = 2;
            }
            else if (typeof(T) == typeof(int))
            {
                ret = (T)(object)ReadInt32(bytes, startIndex, isLittleEndian);
                len = 4;
            }
            else if (typeof(T) == typeof(long))
            {
                ret = (T)(object)ReadInt64(bytes, startIndex, isLittleEndian);
                len = 8;
            }
            else if (typeof(T) == typeof(float))
            {
                ret = (T)(object)ReadSingle(bytes, startIndex, isLittleEndian);
                len = 4;
            }
            else if (typeof(T) == typeof(double))
            {
                ret = (T)(object)ReadDouble(bytes, startIndex, isLittleEndian);
                len = 8;
            }
            else if (typeof(T) == typeof(string))
            {
                ret = (T)(object)ReadString(bytes, startIndex, out len, isLittleEndian);
            }
            else if (typeof(T).GetInterface("IProtocol") != null)
            {
                ret = ReadProtocol<T>(bytes, startIndex, out len, isLittleEndian);
            }
            else
            {
                throw new Exception("無法解析的類型.");
            }

            return ret;
        }


        static public T Read<T>(Stream stream, bool isLittleEndian = true)
        {
            T ret;
            if (typeof(T) == typeof(byte))
                ret = (T)(object)Convert.ToByte(stream.ReadByte());
            else if (typeof(T) == typeof(short))
                ret = (T)(object)ReadInt16(stream, isLittleEndian);
            else if (typeof(T) == typeof(int))
                ret = (T)(object)ReadInt32(stream, isLittleEndian);
            else if (typeof(T) == typeof(long))
                ret = (T)(object)ReadInt64(stream, isLittleEndian);
            else if (typeof(T) == typeof(float))
                ret = (T)(object)ReadSingle(stream, isLittleEndian);
            else if (typeof(T) == typeof(double))
                ret = (T)(object)ReadDouble(stream, isLittleEndian);
            else if (typeof(T) == typeof(string))
                ret = (T)(object)ReadString(stream, isLittleEndian);
            else if(typeof(T) == typeof(byte[]))
                ret = (T)(object)ReadByteArray(stream, isLittleEndian);
            else if (typeof(T).GetInterface("IProtocol") != null)
                ret = ReadProtocol<T>(stream, isLittleEndian);
            else
                throw new Exception("無法解析的類型.");

            return ret;
        }



        public Serializer()
        {
        }
    }
}

