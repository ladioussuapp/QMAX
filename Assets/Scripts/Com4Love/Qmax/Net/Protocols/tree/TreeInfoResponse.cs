using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.tree
{
    public class TreeInfoResponse:Protocol
    {
        /// <summary>
        ///  活动开关。 0关闭 不显示活动入口图标 1开启 显示活动入口图标
        /// </summary>
        public byte status;
        /// <summary>
        ///  大树等级
        /// </summary>
        public byte level;
        /// <summary>
        ///  当前活动的日期
        /// </summary>
        public int activityTime;
        /// <summary>
        ///  进入过的活动ID。
        /// </summary>
        public short activityId;
        /// <summary>
        ///  是否在活动时间内 0 不在活动开启时间段内 1 在活动开启时间段内
        /// </summary>
        public byte isStart;
        /// <summary>
        ///  是否已经进入当前活动 0 未进入过 1 已经进入过
        /// </summary>
        public byte hasEnteredAct;
        /// <summary>
        ///  到下次活动开始的倒计时
        /// </summary>
        public int secToNextAct;
        /// <summary>
        ///  如果活动周期中，到当前活动结束的秒数；否则为0
        /// </summary>
        public int secToCrtActEnd;
        /// <summary>
        ///  配置的活动时间
        /// </summary>
        public List<string> timeList;
        /// <summary>
        ///  可参与的次数
        /// </summary>
        public int playCount;
        /// <summary>
        ///  剩余次数
        /// </summary>
        public int leftCount;
        public TreeInfoResponse()
        {
        }
        public TreeInfoResponse(byte status,byte level,int activityTime,short activityId,byte isStart,byte hasEnteredAct,int secToNextAct,int secToCrtActEnd,List<string> timeList,int playCount,int leftCount)
        {
            this.status = status;
            this.level = level;
            this.activityTime = activityTime;
            this.activityId = activityId;
            this.isStart = isStart;
            this.hasEnteredAct = hasEnteredAct;
            this.secToNextAct = secToNextAct;
            this.secToCrtActEnd = secToCrtActEnd;
            this.timeList = timeList;
            this.playCount = playCount;
            this.leftCount = leftCount;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            status = (byte)stream.ReadByte();
            level = (byte)stream.ReadByte();
            activityTime = Serializer.ReadInt32(stream, isLittleEndian);
            activityId = Serializer.ReadInt16(stream, isLittleEndian);
            isStart = (byte)stream.ReadByte();
            hasEnteredAct = (byte)stream.ReadByte();
            secToNextAct = Serializer.ReadInt32(stream, isLittleEndian);
            secToCrtActEnd = Serializer.ReadInt32(stream, isLittleEndian);
            timeList = Serializer.ReadList<string>(stream, isLittleEndian);
            playCount = Serializer.ReadInt32(stream, isLittleEndian);
            leftCount = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stream.WriteByte(status);
            stream.WriteByte(level);
            Serializer.ToBytes(stream, activityTime, isLittleEndian);
            Serializer.ToBytes(stream, activityId, isLittleEndian);
            stream.WriteByte(isStart);
            stream.WriteByte(hasEnteredAct);
            Serializer.ToBytes(stream, secToNextAct, isLittleEndian);
            Serializer.ToBytes(stream, secToCrtActEnd, isLittleEndian);
            Serializer.ToBytes(stream, timeList, isLittleEndian);
            Serializer.ToBytes(stream, playCount, isLittleEndian);
            Serializer.ToBytes(stream, leftCount, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[TreeInfoResponse]:status={0},level={1},activityTime={2},activityId={3},isStart={4},hasEnteredAct={5},secToNextAct={6},secToCrtActEnd={7},timeList={8},playCount={9},leftCount={10}",status,level,activityTime,activityId,isStart,hasEnteredAct,secToNextAct,secToCrtActEnd,timeList,playCount,leftCount);
        }
    }
}