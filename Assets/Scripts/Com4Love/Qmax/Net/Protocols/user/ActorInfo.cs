using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class ActorInfo:Protocol
    {
        /// <summary>
        ///  角色id
        /// </summary>
        public long actorId;
        /// <summary>
        ///  角色名
        /// </summary>
        public String actorName;
        /// <summary>
        ///  上次登出时间
        /// </summary>
        public int lastLogoutTime;
        public ActorInfo()
        {
        }
        public ActorInfo(long actorId,String actorName,int lastLogoutTime)
        {
            this.actorId = actorId;
            this.actorName = actorName;
            this.lastLogoutTime = lastLogoutTime;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            actorId = Serializer.ReadInt64(stream, isLittleEndian);
            actorName = Serializer.ReadString(stream, isLittleEndian);
            lastLogoutTime = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, actorId, isLittleEndian);
            Serializer.ToBytes(stream, actorName, isLittleEndian);
            Serializer.ToBytes(stream, lastLogoutTime, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ActorInfo]:actorId={0},actorName={1},lastLogoutTime={2}",actorId,actorName,lastLogoutTime);
        }
    }
}
