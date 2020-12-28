using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.ActorGame
{
    public class ActorGameResponse:Protocol
    {
        /// <summary>
        ///  体力
        /// </summary>
        public short energy;
        /// <summary>
        ///  体力上限
        /// </summary>
        public short energyMax;
        /// <summary>
        ///  钻石
        /// </summary>
        public int gem;
        /// <summary>
        ///  钥匙
        /// </summary>
        public short key;
        /// <summary>
        ///  黄毛球
        /// </summary>
        public int upgradeA;
        /// <summary>
        ///  蓝毛球
        /// </summary>
        public int upgradeB;
        /// <summary>
        ///  恢复体力的时间
        /// </summary>
        public int fixEnergyTime;
        /// <summary>
        ///  伙伴列表
        /// </summary>
        public List<Unit.Unit> list;
        /// <summary>
        ///  上次出战的伙伴
        /// </summary>
        public List<int> lastFightUnits;
        /// <summary>
        ///  最高通关关卡id
        /// </summary>
        public int passStageId;
        /// <summary>
        ///  钻石已解锁关卡
        /// </summary>
        public List<int> gemUnlockStageId;
        /// <summary>
        ///  当前引导走到第几步
        /// </summary>
        public int guideIndex;
        /// <summary>
        ///  当前引导id集合
        /// </summary>
        public List<int> guideIds;
        /// <summary>
        ///  金币
        /// </summary>
        public int coin;
        /// <summary>
        ///  免费体力倒计时
        /// </summary>
        public int freeTime;
        public ActorGameResponse()
        {
        }
        public ActorGameResponse(short energy,short energyMax,int gem,short key,int upgradeA,int upgradeB,int fixEnergyTime,List<Unit.Unit> list,List<int> lastFightUnits,int passStageId,List<int> gemUnlockStageId,int guideIndex,List<int> guideIds,int coin,int freeTime)
        {
            this.energy = energy;
            this.energyMax = energyMax;
            this.gem = gem;
            this.key = key;
            this.upgradeA = upgradeA;
            this.upgradeB = upgradeB;
            this.fixEnergyTime = fixEnergyTime;
            this.list = list;
            this.lastFightUnits = lastFightUnits;
            this.passStageId = passStageId;
            this.gemUnlockStageId = gemUnlockStageId;
            this.guideIndex = guideIndex;
            this.guideIds = guideIds;
            this.coin = coin;
            this.freeTime = freeTime;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            energy = Serializer.ReadInt16(stream, isLittleEndian);
            energyMax = Serializer.ReadInt16(stream, isLittleEndian);
            gem = Serializer.ReadInt32(stream, isLittleEndian);
            key = Serializer.ReadInt16(stream, isLittleEndian);
            upgradeA = Serializer.ReadInt32(stream, isLittleEndian);
            upgradeB = Serializer.ReadInt32(stream, isLittleEndian);
            fixEnergyTime = Serializer.ReadInt32(stream, isLittleEndian);
            list = Serializer.ReadList<Unit.Unit>(stream, isLittleEndian);
            lastFightUnits = Serializer.ReadList<int>(stream, isLittleEndian);
            passStageId = Serializer.ReadInt32(stream, isLittleEndian);
            gemUnlockStageId = Serializer.ReadList<int>(stream, isLittleEndian);
            guideIndex = Serializer.ReadInt32(stream, isLittleEndian);
            guideIds = Serializer.ReadList<int>(stream, isLittleEndian);
            coin = Serializer.ReadInt32(stream, isLittleEndian);
            freeTime = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, energy, isLittleEndian);
            Serializer.ToBytes(stream, energyMax, isLittleEndian);
            Serializer.ToBytes(stream, gem, isLittleEndian);
            Serializer.ToBytes(stream, key, isLittleEndian);
            Serializer.ToBytes(stream, upgradeA, isLittleEndian);
            Serializer.ToBytes(stream, upgradeB, isLittleEndian);
            Serializer.ToBytes(stream, fixEnergyTime, isLittleEndian);
            Serializer.ToBytes(stream, list, isLittleEndian);
            Serializer.ToBytes(stream, lastFightUnits, isLittleEndian);
            Serializer.ToBytes(stream, passStageId, isLittleEndian);
            Serializer.ToBytes(stream, gemUnlockStageId, isLittleEndian);
            Serializer.ToBytes(stream, guideIndex, isLittleEndian);
            Serializer.ToBytes(stream, guideIds, isLittleEndian);
            Serializer.ToBytes(stream, coin, isLittleEndian);
            Serializer.ToBytes(stream, freeTime, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ActorGameResponse]:energy={0},energyMax={1},gem={2},key={3},upgradeA={4},upgradeB={5},fixEnergyTime={6},list={7},lastFightUnits={8},passStageId={9},gemUnlockStageId={10},guideIndex={11},guideIds={12},coin={13},freeTime={14}",energy,energyMax,gem,key,upgradeA,upgradeB,fixEnergyTime,list,lastFightUnits,passStageId,gemUnlockStageId,guideIndex,guideIds,coin,freeTime);
        }
    }
}
