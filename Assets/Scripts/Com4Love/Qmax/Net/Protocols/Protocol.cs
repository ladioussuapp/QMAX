using System;
using System.IO;

namespace Com4Love.Qmax.Net.Protocols
{
    public class Protocol: IProtocol
    {
        public virtual byte[] Serialize(bool isLittleEndian = true)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(stream, isLittleEndian);
            return stream.ToArray();
        }


        public virtual int Deserialize(byte[] bytes, int startIndex, bool isLittleEndian = true)
        {
            MemoryStream stream = new MemoryStream(bytes, startIndex, bytes.Length - startIndex);
            return Deserialize(stream, isLittleEndian);
        }


        public virtual int Serialize(Stream stream, bool isLittleEndian = true)
        {
            throw new NotImplementedException();
        }

        public virtual int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            throw new NotImplementedException();
        }
    }
}
