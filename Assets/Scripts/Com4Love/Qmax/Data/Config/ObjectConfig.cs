using Com4Love.Qmax.Data.VO;
using System;
using System.Collections.Generic;
namespace Com4Love.Qmax.Data.Config
{
    public class TileObjectConfig
    {
        public int ID;
        public string ObjectName;

        /// <summary>
        /// 颜色值。当有多个颜色时，保存第一个颜色
        /// </summary>
        public ColorType ColorType;

        /// <summary>
        /// 所有颜色值
        /// </summary>
        public List<ColorType> AllColors = new List<ColorType>();

        public TileType ObjectType;
        public bool Dropable;
        public int Level;
        public int ObstacleRule;
        public int ChangeObjectId;
        public string EliminateAdded;
        public ElimRangeMode RangeMode;
        public string Arg1;
        public string Arg2;
        public string Arg3;
        public string Arg4;
        public ElementType ElementType;
        public string ResourceIcon;
        public string EliminateAnim;
        public int DeliveryPriority;
        public ItemQtt deliverAward;
        public TileObjectConfig(XMLInStream inStream)
        {
            int objectType, dropable, elementType;
            string colorType;
            string deliveryAdded;
            string arg0;

            inStream.Attribute("id", out ID)
                .Attribute("objectName", out ObjectName)
                .Attribute("colorType", out colorType)
                .Attribute("objectType", out objectType)
                .Attribute("dropable", out dropable)
                .Attribute("level", out Level)
                .Attribute("obstacleRule", out ObstacleRule)
                .Attribute("changeObjectId", out ChangeObjectId)
                .Attribute("eliminateAdded", out EliminateAdded)
                .Attribute("deliveryPriority", out DeliveryPriority)
                .Attribute("deliveryAdded", out deliveryAdded)
                .Attribute("arg0", out arg0)
                .Attribute("arg1", out Arg1)
                .Attribute("arg2", out Arg2)
                .Attribute("arg3", out Arg3)
                .Attribute("arg4", out Arg4)
                .Attribute("elementType", out elementType)
                .Attribute("resourceIcon", out ResourceIcon)
                .Attribute("eliminateAnim", out EliminateAnim);

            string[] sl = colorType.Split(',');
            for (int i = 0; i < sl.Length; i++)
            {
                string str = sl[i];
                if (i == 0)
                    ColorType = (ColorType)int.Parse(str);
                AllColors.Add((ColorType)int.Parse(str));
            }
            this.ObjectType = (TileType)objectType;
            this.Dropable = dropable > 0;

            RangeMode = (ElimRangeMode)Convert.ToInt32(arg0);
            deliverAward = ItemQtt.ParseOne(deliveryAdded);
            ElementType = (ElementType)elementType;

            if(ElementType == ElementType.MultiColor)
            {
                Q.Assert(AllColors.Count > 1, "配置错误 ObjectConfig id={0}", ID);
            }
        }
    }
}
