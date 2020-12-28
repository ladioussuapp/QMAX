using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementVO:Protocol
    {
        /// <summary>
        ///  成就id
        /// </summary>
        public int achievementId;
        /// <summary>
        ///  獎勵狀態
        /// </summary>
        public byte state;
        /// <summary>
        ///  累計進度
        /// </summary>
        public int accumulateValue;
        public AchievementVO()
        {
        }
        public AchievementVO(int achievementId,byte state,int accumulateValue)
        {
            this.achievementId = achievementId;
            this.state = state;
            this.accumulateValue = accumulateValue;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            achievementId = Serializer.ReadInt32(stream, isLittleEndian);
            state = (byte)stream.ReadByte();
            accumulateValue = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, achievementId, isLittleEndian);
            stream.WriteByte(state);
            Serializer.ToBytes(stream, accumulateValue, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[AchievementVO]:achievementId={0},state={1},accumulateValue={2}",achievementId,state,accumulateValue);
        }
    }
}