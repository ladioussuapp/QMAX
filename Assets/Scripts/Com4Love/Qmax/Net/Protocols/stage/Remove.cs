using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class Remove:Protocol
    {
        /// <summary>
        ///  物品id
        /// </summary>
        public int objectId;
        /// <summary>
        ///  数量
        /// </summary>
        public int num;
        public Remove()
        {
        }
        public Remove(int objectId,int num)
        {
            this.objectId = objectId;
            this.num = num;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            objectId = Serializer.ReadInt32(stream, isLittleEndian);
            num = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, objectId, isLittleEndian);
            Serializer.ToBytes(stream, num, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[Remove]:objectId={0},num={1}",objectId,num);
        }
    }
}
