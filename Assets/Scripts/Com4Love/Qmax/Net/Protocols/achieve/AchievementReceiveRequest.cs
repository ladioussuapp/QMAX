using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementReceiveRequest:Protocol
    {
        /// <summary>
        ///  要領取獎勵的成就id
        /// </summary>
        public int achievementId;
        public AchievementReceiveRequest()
        {
        }
        public AchievementReceiveRequest(int achievementId)
        {
            this.achievementId = achievementId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            achievementId = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, achievementId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[AchievementReceiveRequest]:achievementId={0}",achievementId);
        }
    }
}