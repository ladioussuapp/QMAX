using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class GetActorRequest:Protocol
    {
        /// <summary>
        ///  选择的游戏服Id
        /// </summary>
        public int serverId;
        /// <summary>
        ///  渠道id(用于记录新进用户)
        /// </summary>
        public string channelId;
        public GetActorRequest()
        {
        }
        public GetActorRequest(int serverId,string channelId)
        {
            this.serverId = serverId;
            this.channelId = channelId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            serverId = Serializer.ReadInt32(stream, isLittleEndian);
            channelId = Serializer.ReadString(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, serverId, isLittleEndian);
            Serializer.ToBytes(stream, channelId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[GetActorRequest]:serverId={0},channelId={1}",serverId,channelId);
        }
    }
}
