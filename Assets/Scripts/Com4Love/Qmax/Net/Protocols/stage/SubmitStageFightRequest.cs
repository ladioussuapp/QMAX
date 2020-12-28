using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using Com4Love.Qmax.Net.Protocols.Stage;
namespace Com4Love.Qmax.Net.Protocols.stage
{
    public class SubmitStageFightRequest:Protocol
    {
        /// <summary>
        ///  關卡id
        /// </summary>
        public int stageId;
        /// <summary>
        ///  消除詳細步驟記錄
        /// </summary>
        public List<Step> steps;
        /// <summary>
        ///  獲得星級
        /// </summary>
        public int star;
        /// <summary>
        ///  獲得分數
        /// </summary>
        public int score;
        /// <summary>
        ///  投遞獎勵 key:獎勵類型 value:獎勵數量
        /// </summary>
        public Dictionary<byte,int> rewards;
        public SubmitStageFightRequest()
        {
        }
        public SubmitStageFightRequest(int stageId,List<Step> steps,int star,int score,Dictionary<byte,int> rewards)
        {
            this.stageId = stageId;
            this.steps = steps;
            this.star = star;
            this.score = score;
            this.rewards = rewards;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stageId = Serializer.ReadInt32(stream, isLittleEndian);
            steps = Serializer.ReadList<Step>(stream, isLittleEndian);
            star = Serializer.ReadInt32(stream, isLittleEndian);
            score = Serializer.ReadInt32(stream, isLittleEndian);
            rewards = Serializer.ReadDict<byte,int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stageId, isLittleEndian);
            Serializer.ToBytes(stream, steps, isLittleEndian);
            Serializer.ToBytes(stream, star, isLittleEndian);
            Serializer.ToBytes(stream, score, isLittleEndian);
            Serializer.ToBytes(stream, rewards, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SubmitStageFightRequest]:stageId={0},steps={1},star={2},score={3},rewards={4}",stageId,steps,star,score,rewards);
        }
    }
}