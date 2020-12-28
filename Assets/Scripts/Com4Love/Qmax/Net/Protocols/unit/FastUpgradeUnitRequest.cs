using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Unit
{
    public class FastUpgradeUnitRequest:Protocol
    {
        /// <summary>
        ///  伙伴id
        /// </summary>
        public int unitId;
        public FastUpgradeUnitRequest()
        {
        }
        public FastUpgradeUnitRequest(int unitId)
        {
            this.unitId = unitId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            unitId = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, unitId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[FastUpgradeUnitRequest]:unitId={0}",unitId);
        }
    }
}