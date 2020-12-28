using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Energy
{
    public class RefreshEnergyResponse:Protocol
    {
        /// <summary>
        ///  當前體力值
        /// </summary>
        public short energy;
        /// <summary>
        ///  當前體力值
        /// </summary>
        public short maxEnergy;
        /// <summary>
        ///  上次恢復體力的時間(秒)
        /// </summary>
        public int fixEnergyTime;
        public RefreshEnergyResponse()
        {
        }
        public RefreshEnergyResponse(short energy,short maxEnergy,int fixEnergyTime)
        {
            this.energy = energy;
            this.maxEnergy = maxEnergy;
            this.fixEnergyTime = fixEnergyTime;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            energy = Serializer.ReadInt16(stream, isLittleEndian);
            maxEnergy = Serializer.ReadInt16(stream, isLittleEndian);
            fixEnergyTime = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, energy, isLittleEndian);
            Serializer.ToBytes(stream, maxEnergy, isLittleEndian);
            Serializer.ToBytes(stream, fixEnergyTime, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[RefreshEnergyResponse]:energy={0},maxEnergy={1},fixEnergyTime={2}",energy,maxEnergy,fixEnergyTime);
        }
    }
}