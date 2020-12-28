using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.sign
{
    public class SignInfoResponse:Protocol
    {
        /// <summary>
        ///  当前的签到天数
        /// </summary>
        public int signDay;
        /// <summary>
        ///  是否已经领取
        ///  1 已经领取
        ///  0 未领取
        /// </summary>
        public byte isReceived;
        public SignInfoResponse()
        {
        }
        public SignInfoResponse(int signDay,byte isReceived)
        {
            this.signDay = signDay;
            this.isReceived = isReceived;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            signDay = Serializer.ReadInt32(stream, isLittleEndian);
            isReceived = (byte)stream.ReadByte();
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, signDay, isLittleEndian);
            stream.WriteByte(isReceived);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[SignInfoResponse]:signDay={0},isReceived={1}",signDay,isReceived);
        }
    }
}