using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class CreateActorResponse:Protocol
    {
        /// <summary>
        ///  角色Id
        /// </summary>
        public long actorId;
        public CreateActorResponse()
        {
        }
        public CreateActorResponse(long actorId)
        {
            this.actorId = actorId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            actorId = Serializer.ReadInt64(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, actorId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[CreateActorResponse]:actorId={0}",actorId);
        }
    }
}
