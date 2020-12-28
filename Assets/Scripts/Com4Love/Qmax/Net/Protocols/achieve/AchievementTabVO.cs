using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
namespace Com4Love.Qmax.Net.Protocols.achievement
{
    public class AchievementTabVO:Protocol
    {
        /// <summary>
        ///  成就類型
        /// </summary>
        public byte achieveType;
        /// <summary>
        ///  icon path
        /// </summary>
        public String icon;
        /// <summary>
        ///  類型名稱
        /// </summary>
        public int name;
        /// <summary>
        ///  成就列表
        /// </summary>
        public List<AchievementVO> achieveList;
        public AchievementTabVO()
        {
        }
        public AchievementTabVO(byte achieveType, String icon, int name, List<AchievementVO> achieveList)
        {
            this.achieveType = achieveType;
            this.icon = icon;
            this.name = name;
            this.achieveList = achieveList;
        }
        public override int Deserialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            achieveType = (byte)stream.ReadByte();
            icon = Serializer.Read<String>(stream, isLittleEndian);
            name = Serializer.ReadInt32(stream, isLittleEndian);
            achieveList = Serializer.ReadList<AchievementVO>(stream, isLittleEndian);
            return (int)(len - stream.Length);
        }
        public override int Serialize(Stream stream, bool isLittleEndian = true)
        {
            long len = stream.Length;
            stream.WriteByte(achieveType);
            Serializer.ToBytes(stream, icon, isLittleEndian);
            Serializer.ToBytes(stream, name, isLittleEndian);
            Serializer.ToBytes(stream, achieveList, isLittleEndian);
            return (int)(stream.Length - len);
        }
        public override string ToString ()
        {
            return string.Format("[AchievementTabVO]:achieveType={0},icon={1},name={2},achieveList={3}", achieveType, icon, name, achieveList);
        }
    }
}