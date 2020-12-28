using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.activity
{
    public class ExchangeCodeRequest:Protocol
    {
        /// <summary>
        ///  兌換碼
        /// </summary>
        public String code;
        public ExchangeCodeRequest()
        {
        }
        public ExchangeCodeRequest(String code)
        {
            this.code = code;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            code = Serializer.Read<String>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, code, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ExchangeCodeRequest]:code={0}",code);
        }
    }
}
