using System.Collections.Generic;
using System.IO;

namespace Com4Love.Qmax.Net.Protocols
{
    public class ValueResultListResponse:Protocol
    {
        /// <summary>
        ///  1 鑰 2 黃毛球 3 藍毛球 4 鑽石 5 體力上限 6 體力 7 夥伴
        ///  物品類型
        /// </summary>
        public List<ValueResult> list;
        public ValueResultListResponse()
        {
        }
        public ValueResultListResponse(List<ValueResult> list)
        {
            this.list = list;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            list = Serializer.ReadList<ValueResult>(stream, isLittleEndian);
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
            return string.Format("[ValueResultListResponse]:list={0}",list);
        }
    }
}
