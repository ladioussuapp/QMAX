using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class CreateActorRequest:Protocol
    {
        /// <summary>
        ///  选择的游戏服Id
        /// </summary>
        public int serverId;
        /// <summary>
        ///  输入角色名
        /// </summary>
        public String actorName;
        /// <summary>
        ///  创建角色时的渠道id
        /// </summary>
        public String channelId;
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
		/// <summary>
        ///   系统版本
        /// </summary>
        public String osversion;
		 /// <summary>
        ///  设备型号
        /// </summary>
        public String phonetype;
		/// <summary>
        ///   系统类型
        /// </summary>
        public String phoneos;
		 /// <summary>
        ///  语言
        /// </summary>
        public String language;
        public CreateActorRequest()
        {
        }
        public CreateActorRequest(int serverId,String actorName,String channelId,String sim,String mac,String imei,String osversion, String phonetype, String phoneos, String language)
        {
            this.serverId = serverId;
            this.actorName = actorName;
            this.channelId = channelId;
            this.sim = sim;
            this.mac = mac;
            this.imei = imei;
			this.osversion = osversion;
			this.phonetype = phonetype;
			this.phoneos = phoneos;
			this.language = language;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            serverId = Serializer.ReadInt32(stream, isLittleEndian);
            actorName = Serializer.ReadString(stream, isLittleEndian);
            channelId = Serializer.ReadString(stream, isLittleEndian);
            sim = Serializer.ReadString(stream, isLittleEndian);
            mac = Serializer.ReadString(stream, isLittleEndian);
            imei = Serializer.ReadString(stream, isLittleEndian);
			osversion = Serializer.ReadString(stream, isLittleEndian);
			phonetype = Serializer.ReadString(stream, isLittleEndian);
            phoneos = Serializer.ReadString(stream, isLittleEndian);
            language = Serializer.ReadString(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, serverId, isLittleEndian);
            Serializer.ToBytes(stream, actorName, isLittleEndian);
            Serializer.ToBytes(stream, channelId, isLittleEndian);
            Serializer.ToBytes(stream, sim, isLittleEndian);
            Serializer.ToBytes(stream, mac, isLittleEndian);
            Serializer.ToBytes(stream, imei, isLittleEndian);
			Serializer.ToBytes(stream, osversion, isLittleEndian);
			Serializer.ToBytes(stream, phonetype, isLittleEndian);
            Serializer.ToBytes(stream, phoneos, isLittleEndian);
            Serializer.ToBytes(stream, language, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[CreateActorRequest]:serverId={0},actorName={1},channelId={2},sim={3},mac={4},imei={5},osversion={6},phonetype={7},phoneos={8},language={9}",serverId,actorName,channelId,sim,mac,imei,osversion,phonetype, phoneos,language);
        }
    }
}
