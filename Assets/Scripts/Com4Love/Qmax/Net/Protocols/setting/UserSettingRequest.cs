using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.setting
{
    public class UserSettingRequest:Protocol
    {
        /// <summary>
        ///  设置key-value
        /// </summary>
        public Dictionary<string,string> settingValueMap;
        public UserSettingRequest()
        {
        }
        public UserSettingRequest(Dictionary<string,string> settingValueMap)
        {
            this.settingValueMap = settingValueMap;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            settingValueMap = Serializer.ReadDict<string,string>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, settingValueMap, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[UserSettingRequest]:settingValueMap={0}",settingValueMap);
        }
    }
}
