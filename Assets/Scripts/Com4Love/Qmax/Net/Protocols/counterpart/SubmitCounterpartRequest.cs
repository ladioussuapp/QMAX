using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.counterpart
{
    public class SubmitCounterpartRequest:Protocol
    {
        /// <summary>
        ///  Ãö¥did
        /// </summary>
        public int stageId;
        /// <summary>
        ///  Ïû³ıÏêÏ¸²½Öè¼ÇÂ¼
        /// </summary>
        public List<Step> steps;
        /// <summary>
        ///  ÀÛ»ıÉËº¦
        /// </summary>
        public int damage;
        /// <summary>
        ///  Í¶µİ½±Àø key:½±ÀøÀàĞÍ value:½±ÀøÊıÁ¿
        /// </summary>
        public Dictionary<byte,int> rewards;
        public SubmitCounterpartRequest()
        {
        }
        public SubmitCounterpartRequest(int stageId,List<Step> steps,int damage,Dictionary<byte,int> rewards)
        {
            this.stageId = stageId;
            this.steps = steps;
            this.damage = damage;
            this.rewards = rewards;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stageId = Serializer.ReadInt32(stream, isLittleEndian);
            steps = Serializer.ReadList<Step>(stream, isLittleEndian);
            damage = Serializer.ReadInt32(stream, isLittleEndian);
            rewards = Serializer.ReadDict<byte,int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stageId, isLittleEndian);
            Serializer.ToBytes(stream, steps, isLittleEndian);
            Serializer.ToBytes(stream, damage, isLittleEndian);
            Serializer.ToBytes(stream, rewards, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SubmitCounterpartRequest]:stageId={0},steps={1},damage={2},rewards={3}",stageId,steps,damage,rewards);
        }
    }
}