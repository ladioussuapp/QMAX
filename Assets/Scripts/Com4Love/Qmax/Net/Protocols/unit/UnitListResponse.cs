using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Unit
{
    public class UnitListResponse:Protocol
    {
        /// <summary>
        ///  伙伴列表
        /// </summary>
        public List<Unit> list;
        public UnitListResponse()
        {
        }
        public UnitListResponse(List<Unit> list)
        {
            this.list = list;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            list = Serializer.ReadList<Unit>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, list, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UnitListResponse]:list={0}",list);
        }
    }
}
