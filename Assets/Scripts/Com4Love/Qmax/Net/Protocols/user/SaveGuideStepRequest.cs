using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class SaveGuideStepRequest:Protocol
    {
        /// <summary>
        ///  引导类型id
        /// </summary>
        public int key;
        /// <summary>
        ///  引导值
        /// </summary>
        public int value;
        public SaveGuideStepRequest()
        {
        }
        public SaveGuideStepRequest(int key,int value)
        {
            this.key = key;
            this.value = value;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            key = Serializer.ReadInt32(stream, isLittleEndian);
            value = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, key, isLittleEndian);
            Serializer.ToBytes(stream, value, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SaveGuideStepRequest]:key={0},value={1}",key,value);
        }
    }
}
