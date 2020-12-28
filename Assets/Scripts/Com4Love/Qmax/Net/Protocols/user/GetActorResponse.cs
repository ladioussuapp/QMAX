using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class GetActorResponse:Protocol
    {
        /// <summary>
        ///  角色基本信息列表
        /// </summary>
        public List<ActorInfo> list;
        public GetActorResponse()
        {
        }
        public GetActorResponse(List<ActorInfo> list)
        {
            this.list = list;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            list = Serializer.ReadList<ActorInfo>(stream, isLittleEndian);
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
            return string.Format("[GetActorResponse]:list={0}",list);
        }
    }
}
