using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class UserLoginResponse:Protocol
    {
        /// <summary>
        ///  平台用户的uid;
        /// </summary>
        public String uid;
        /// <summary>
        ///  登陆后的一些扩展参数
        /// </summary>
        public Dictionary<string,string> parameters;
        /// <summary>
        ///  重连id
        /// </summary>
        public String reconnectId;
        /// <summary>
        ///  密钥
        /// </summary>
        public byte[] cryptKey;
        public UserLoginResponse()
        {
        }
        public UserLoginResponse(String uid,Dictionary<string,string> parameters,String reconnectId,byte[] cryptKey)
        {
            this.uid = uid;
            this.parameters = parameters;
            this.reconnectId = reconnectId;
            this.cryptKey = cryptKey;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            uid = Serializer.Read<String>(stream, isLittleEndian);
            parameters = Serializer.ReadDict<string,string>(stream, isLittleEndian);
            reconnectId = Serializer.Read<String>(stream, isLittleEndian);
            cryptKey = Serializer.Read<byte[]>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, uid, isLittleEndian);
            Serializer.ToBytes(stream, parameters, isLittleEndian);
            Serializer.ToBytes(stream, reconnectId, isLittleEndian);
            Serializer.ToBytes(stream, cryptKey, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UserLoginResponse]:uid={0},parameters={1},reconnectId={2},cryptKey={3}",uid,parameters,reconnectId,cryptKey);
        }
    }
}
