using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.actorgame
{
    public class ActorSaveGuideRequest:Protocol
    {
        /// <summary>
        ///  当前引导步骤
        /// </summary>
        public int currentGuideStep;
        /// <summary>
        ///  引导教学id
        /// </summary>
        public int guideId;
        public ActorSaveGuideRequest()
        {
        }
        public ActorSaveGuideRequest(int currentGuideStep,int guideId)
        {
            this.currentGuideStep = currentGuideStep;
            this.guideId = guideId;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            currentGuideStep = Serializer.ReadInt32(stream, isLittleEndian);
            guideId = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, currentGuideStep, isLittleEndian);
            Serializer.ToBytes(stream, guideId, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[ActorSaveGuideRequest]:currentGuideStep={0},guideId={1}",currentGuideStep,guideId);
        }
    }
}
