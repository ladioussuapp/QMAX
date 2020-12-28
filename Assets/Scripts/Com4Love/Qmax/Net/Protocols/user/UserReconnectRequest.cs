using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class UserReconnectRequest:Protocol
    {
        /// <summary>
        ///  平台id
        /// </summary>
        public String platformId;
        /// <summary>
        ///  连接id
        /// </summary>
        public String connectionId;
        /// <summary>
        ///  版本号
        /// </summary>
        public String version;
        /// <summary>
        ///  角色id
        /// </summary>
        public long actorId;
        public UserReconnectRequest()
        {
        }
        public UserReconnectRequest(String platformId,String connectionId,String version, long actorId)
        {
            this.platformId = platformId;
            this.connectionId = connectionId;
            this.version = version;
            this.actorId = actorId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            platformId = Serializer.ReadString(stream, isLittleEndian);
            connectionId = Serializer.ReadString(stream, isLittleEndian);
            version = Serializer.ReadString(stream, isLittleEndian);
            actorId = Serializer.ReadInt64(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, platformId, isLittleEndian);
            Serializer.ToBytes(stream, connectionId, isLittleEndian);
            Serializer.ToBytes(stream, version, isLittleEndian);
            Serializer.ToBytes(stream, actorId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UserReconnectRequest]:platformId={0},connectionId={1},version={2},actorId={3}",platformId,connectionId,version,actorId);
        }
    }
}
