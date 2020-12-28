using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.User
{
    public class UserLoginRequest:Protocol
    {
        /// <summary>
        ///  平台id
        /// </summary>
        public String platformId;
        /// <summary>
        ///  令牌字符串 分隔符
        /// </summary>
        public String token;
        /// <summary>
        ///  版本号
        /// </summary>
        public String version;
		/// <summary>
        ///  版本号
        /// </summary>
        public String sdkVersion;
        public UserLoginRequest()
        {
        }
        public UserLoginRequest(String platformId,String token,String version, String sdkVersion)
        {
            this.platformId = platformId;
            this.token = token;
            this.version = version;
			this.sdkVersion = sdkVersion;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            platformId = Serializer.ReadString(stream, isLittleEndian);
            token = Serializer.ReadString(stream, isLittleEndian);
            version = Serializer.ReadString(stream, isLittleEndian);
			sdkVersion = Serializer.ReadString(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, platformId, isLittleEndian);
            Serializer.ToBytes(stream, token, isLittleEndian);
            Serializer.ToBytes(stream, version, isLittleEndian);
			Serializer.ToBytes(stream, sdkVersion, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UserLoginRequest]:platformId={0},token={1},version={2},sdkVersion={3}",platformId,token,version,sdkVersion);
        }
    }
}
