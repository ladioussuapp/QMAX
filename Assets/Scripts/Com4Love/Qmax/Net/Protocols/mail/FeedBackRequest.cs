using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.mail
{
    public class FeedBackRequest:Protocol
    {
        /// <summary>
        ///  反馈类型

		///游戏出错 ERROR(1),
 		///举报 REPORT(2),
 		///充值 RECHARGE(3),
  		///好友 FRIEND(4),
  		///其他 OTHER(5);
        /// </summary>
        public byte feedbackType;
        /// <summary>
        ///  反馈的角色id
        /// </summary>
        public long actorId;
        /// <summary>
        ///  联系方式 qq
        /// </summary>
        public String qq;
        /// <summary>
        ///  联系方式邮箱
        /// </summary>
        public String mail;
        /// <summary>
        ///  反馈内容
        /// </summary>
        public String content;
        public FeedBackRequest()
        {
        }
        public FeedBackRequest(byte feedbackType,long actorId,String qq,String mail,String content)
        {
            this.feedbackType = feedbackType;
            this.actorId = actorId;
            this.qq = qq;
            this.mail = mail;
            this.content = content;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            feedbackType = (byte)stream.ReadByte();
            actorId = Serializer.ReadInt64(stream, isLittleEndian);
            qq = Serializer.Read<String>(stream, isLittleEndian);
            mail = Serializer.Read<String>(stream, isLittleEndian);
            content = Serializer.Read<String>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stream.WriteByte(feedbackType);
            Serializer.ToBytes(stream, actorId, isLittleEndian);
            Serializer.ToBytes(stream, qq, isLittleEndian);
            Serializer.ToBytes(stream, mail, isLittleEndian);
            Serializer.ToBytes(stream, content, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[FeedBackRequest]:feedbackType={0},actorId={1},qq={2},mail={3},content={4}",feedbackType,actorId,qq,mail,content);
        }
    }
}
