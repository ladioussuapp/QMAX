using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementListResponse : Protocol
    {
        /// <summary>
        ///  成就列表
        /// </summary>
        public List<AchievementTabVO> achievements;
        /// <summary>
        ///  數據結果
        /// </summary>
        public ValueResultListResponse valueResultListResponse;
        public AchievementListResponse()
        {
        }
        public AchievementListResponse(List<AchievementTabVO> achievements, ValueResultListResponse valueResultListResponse)
        {
            this.achievements = achievements;
            this.valueResultListResponse = valueResultListResponse;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            achievements = Serializer.ReadList<AchievementTabVO>(stream, isLittleEndian);
            valueResultListResponse = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, achievements, isLittleEndian);
            Serializer.ToBytes(stream, valueResultListResponse, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString()
        {
            return string.Format("[AchievementListResponse]:achievements={0},valueResultListResponse={1}", achievements, valueResultListResponse);
        }
    }
}