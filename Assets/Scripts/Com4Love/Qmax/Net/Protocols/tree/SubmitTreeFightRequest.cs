using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.tree
{
    public class SubmitTreeFightRequest:Protocol
    {
        /// <summary>
        ///  当前活动的日期
        /// </summary>
        public int activityTime;
        /// <summary>
        ///  关卡编号。

        ///  活动每天会开多次，用来区分活动
        /// </summary>
        public short activityId;
        /// <summary>
        ///  消除详细步骤记录
        /// </summary>
        public List<Step> steps;
        /// <summary>
        ///  累积伤害
        /// </summary>
        public int damage;
        /// <summary>
        ///  投递奖励 key:奖励类型 value:奖励数量
        /// </summary>
        public Dictionary<byte,int> rewards;
        public SubmitTreeFightRequest()
        {
        }
        public SubmitTreeFightRequest(int activityTime,short activityId,List<Step> steps,int damage,Dictionary<byte,int> rewards)
        {
            this.activityTime = activityTime;
            this.activityId = activityId;
            this.steps = steps;
            this.damage = damage;
            this.rewards = rewards;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            activityTime = Serializer.ReadInt32(stream, isLittleEndian);
            activityId = Serializer.ReadInt16(stream, isLittleEndian);
            steps = Serializer.ReadList<Step>(stream, isLittleEndian);
            damage = Serializer.ReadInt32(stream, isLittleEndian);
            rewards = Serializer.ReadDict<byte,int>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, activityTime, isLittleEndian);
            Serializer.ToBytes(stream, activityId, isLittleEndian);
            Serializer.ToBytes(stream, steps, isLittleEndian);
            Serializer.ToBytes(stream, damage, isLittleEndian);
            Serializer.ToBytes(stream, rewards, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SubmitTreeFightRequest]:activityTime={0},activityId={1},steps={2},damage={3},rewards={4}",activityTime,activityId,steps,damage,rewards);
        }
    }
}