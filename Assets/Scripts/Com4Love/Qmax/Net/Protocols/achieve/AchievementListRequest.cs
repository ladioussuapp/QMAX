using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementListRequest : Protocol
    {
        /// <summary>
        ///  渠道標識
        /// </summary>
        public String channel;
        /// <summary>
        ///  成就分類(主分類)
        /// </summary>
        public byte achievementType;
        public AchievementListRequest()
        {
        }
        public AchievementListRequest(String channel, byte achievementType)
        {
            this.channel = channel;
            this.achievementType = achievementType;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            channel = Serializer.Read<String>(stream, isLittleEndian);
            achievementType = (byte)stream.ReadByte();
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, channel, isLittleEndian);
            stream.WriteByte(achievementType);
            return (int)(stream.Length - len);
        }
        public override string ToString()
        {
            return string.Format("[AchievementListRequest]:channel={0},achievementType={1}", channel, achievementType);
        }
    }
}