using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class ActorLoginResponse:Protocol
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
        ///  创建角色时的渠道id
        /// </summary>
        public String channelId;
        /// <summary>
        ///  平台id
        /// </summary>
        public String platformId;
        /// <summary>
        ///  游戏服id
        /// </summary>
        public int serverId;
        public ActorLoginResponse()
        {
        }
        public ActorLoginResponse(long actorId,String actorName,String channelId,String platformId,int serverId)
        {
            this.actorId = actorId;
            this.actorName = actorName;
            this.channelId = channelId;
            this.platformId = platformId;
            this.serverId = serverId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            actorId = Serializer.ReadInt64(stream, isLittleEndian);
            actorName = Serializer.ReadString(stream, isLittleEndian);
            channelId = Serializer.ReadString(stream, isLittleEndian);
            platformId = Serializer.ReadString(stream, isLittleEndian);
            serverId = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, actorId, isLittleEndian);
            Serializer.ToBytes(stream, actorName, isLittleEndian);
            Serializer.ToBytes(stream, channelId, isLittleEndian);
            Serializer.ToBytes(stream, platformId, isLittleEndian);
            Serializer.ToBytes(stream, serverId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ActorLoginResponse]:actorId={0},actorName={1},channelId={2},platformId={3},serverId={4}",actorId,actorName,channelId,platformId,serverId);
        }
    }
}
