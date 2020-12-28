using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.Stage
{
    public class Stage:Protocol
    {
        /// <summary>
        ///  據點id
        /// </summary>
        public int stageId;
        /// <summary>
        ///  通關星數
        /// </summary>
        public byte star;
        /// <summary>
        ///  當前關卡勝利次數
        /// </summary>
        public short win;
        /// <summary>
        ///  當前關卡失敗次數
        /// </summary>
        public short lose;
        /// <summary>
        ///  首次通關步數
        /// </summary>
        public short firstStep;
        /// <summary>
        ///  過關分數
        /// </summary>
        public int score;
        /// <summary>
        ///  過關目標
        /// </summary>
        public Dictionary<byte,string> targets;
        /// <summary>
        ///  步數限制
        /// </summary>
        public int stageLimit;
        public Stage()
        {
        }
        public Stage(int stageId,byte star,short win,short lose,short firstStep,int score,Dictionary<byte,string> targets,int stageLimit)
        {
            this.stageId = stageId;
            this.star = star;
            this.win = win;
            this.lose = lose;
            this.firstStep = firstStep;
            this.score = score;
            this.targets = targets;
            this.stageLimit = stageLimit;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stageId = Serializer.ReadInt32(stream, isLittleEndian);
            star = (byte)stream.ReadByte();
            win = Serializer.ReadInt16(stream, isLittleEndian);
            lose = Serializer.ReadInt16(stream, isLittleEndian);
            firstStep = Serializer.ReadInt16(stream, isLittleEndian);
            score = Serializer.ReadInt32(stream, isLittleEndian);
            targets = Serializer.ReadDict<byte,string>(stream, isLittleEndian);
            stageLimit = Serializer.ReadInt32(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            Serializer.ToBytes(stream, stageId, isLittleEndian);
            stream.WriteByte(star);
            Serializer.ToBytes(stream, win, isLittleEndian);
            Serializer.ToBytes(stream, lose, isLittleEndian);
            Serializer.ToBytes(stream, firstStep, isLittleEndian);
            Serializer.ToBytes(stream, score, isLittleEndian);
            Serializer.ToBytes(stream, targets, isLittleEndian);
            Serializer.ToBytes(stream, stageLimit, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[Stage]:stageId={0},star={1},win={2},lose={3},firstStep={4},score={5},targets={6},stageLimit={7}",stageId,star,win,lose,firstStep,score,targets,stageLimit);
        }
    }
}
