using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class ActorLoginRequest:Protocol
    {
        /// <summary>
        ///  选择的游戏服Id
        /// </summary>
        public int serverId;
        /// <summary>
        ///  角色Id
        /// </summary>
        public long actorId;
        /// <summary>
        ///  sim信息
        /// </summary>
        public String sim;
        /// <summary>
        ///  mac地址
        /// </summary>
        public String mac;
        /// <summary>
        ///  imei信息
        /// </summary>
        public String imei;
        public ActorLoginRequest()
        {
        }
        public ActorLoginRequest(int serverId,long actorId,String sim,String mac,String imei)
        {
            this.serverId = serverId;
            this.actorId = actorId;
            this.sim = sim;
            this.mac = mac;
            this.imei = imei;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            serverId = Serializer.ReadInt32(stream, isLittleEndian);
            actorId = Serializer.ReadInt64(stream, isLittleEndian);
            sim = Serializer.ReadString(stream, isLittleEndian);
            mac = Serializer.ReadString(stream, isLittleEndian);
            imei = Serializer.ReadString(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, serverId, isLittleEndian);
            Serializer.ToBytes(stream, actorId, isLittleEndian);
            Serializer.ToBytes(stream, sim, isLittleEndian);
            Serializer.ToBytes(stream, mac, isLittleEndian);
            Serializer.ToBytes(stream, imei, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ActorLoginRequest]:serverId={0},actorId={1},sim={2},mac={3},imei={4}",serverId,actorId,sim,mac,imei);
        }
    }
}
