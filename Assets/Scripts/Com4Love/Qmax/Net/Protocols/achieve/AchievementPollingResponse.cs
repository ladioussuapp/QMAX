using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementPollingResponse:Protocol
    {
        /// <summary>
        ///  可領取的成就數量
        /// </summary>
        public int achieveNum;
        public AchievementPollingResponse()
        {
        }
        public AchievementPollingResponse(int achieveNum)
        {
            this.achieveNum = achieveNum;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            achieveNum = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, achieveNum, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[AchievementPollingResponse]:achieveNum={0}",achieveNum);
        }
    }
}