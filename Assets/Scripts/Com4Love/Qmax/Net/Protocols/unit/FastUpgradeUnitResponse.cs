using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Unit
{
    public class FastUpgradeUnitResponse:Protocol
    {
        /// <summary>
        ///  升级前的id
        /// </summary>
        public int beforeUnitId;
        /// <summary>
        ///  升级后的id
        /// </summary>
        public int afterUnitId;
        /// <summary>
        ///  数值变化
        /// </summary>
        public ValueResultListResponse valueResultList;
        public FastUpgradeUnitResponse()
        {
        }
        public FastUpgradeUnitResponse(int beforeUnitId,int afterUnitId,ValueResultListResponse valueResultList)
        {
            this.beforeUnitId = beforeUnitId;
            this.afterUnitId = afterUnitId;
            this.valueResultList = valueResultList;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            beforeUnitId = Serializer.ReadInt32(stream, isLittleEndian);
            afterUnitId = Serializer.ReadInt32(stream, isLittleEndian);
            valueResultList = Serializer.Read<ValueResultListResponse>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, beforeUnitId, isLittleEndian);
            Serializer.ToBytes(stream, afterUnitId, isLittleEndian);
            Serializer.ToBytes(stream, valueResultList, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[FastUpgradeUnitResponse]:beforeUnitId={0},afterUnitId={1},valueResultList={2}",beforeUnitId,afterUnitId,valueResultList);
        }
    }
}