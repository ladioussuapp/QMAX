using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementRefreshRequest:Protocol
    {
        /// <summary>
        ///  渠道標識
        /// </summary>
        public String channel;
        public AchievementRefreshRequest()
        {
        }
        public AchievementRefreshRequest(String channel)
        {
            this.channel = channel;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            channel = Serializer.Read<String>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, channel, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[AchievementRefreshRequest]:channel={0}",channel);
        }
    }
}