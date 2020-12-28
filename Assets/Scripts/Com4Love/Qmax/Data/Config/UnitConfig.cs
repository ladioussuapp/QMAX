using Com4Love.Qmax.Data.VO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Data.Config
{
    public class UnitConfig
    {
        public int ID;
        public string UnitName;
        /// <summary>
        /// 类型
        /// </summary>
        public int UnitTypeId;
        public int Level;

        /// <summary>
        /// 显示的名称，暂时直接使用
        /// </summary>
        public string NameStringId;

        /// <summary>
        ///描述  同上
        /// </summary>
        public string StroyStringId;
        /// <summary>
        /// 可升级到的伙伴  -1表示是最终
        /// </summary>
        public int UnitUpgrade;
        public int UnitHp;
        public int UnitAtk;
        /// <summary>
        /// 升级时费用   黄毛球
        /// </summary>
        public int UnitUpgradeA;
        public int UnitUpgradeB;
        public int UnitUnlock;
        public List<int> UnitSkillIdArr;
        /// <summary>
        /// 伙伴技能，技能取的是一个用逗号分开的字符数组，伙伴只有一个技能，取数组的第一个
        /// </summary>
        public int UnitSkillId;
        /// <summary>
        /// 对应颜色
        /// </summary>
        public ColorType UnitColor;
        /// <summary>
        /// "消除敌人给予奖励(a,b)
        /// a= 1鑰  2 黃毛球
        /// 3 藍毛球 4 鑽石
        /// b = 數量"
        /// </summary>
        public ItemQtt[] UnitGift;
        /// <summary>
        /// 图片文件名
        /// </summary>
        public string ResourceIcon;

        /// <summary>
        /// 伙伴界面显示在伙伴下方的提示图片
        /// </summary>
        public string TipsIcon;

        /// <summary>
        /// 是否是敌人
        /// </summary>
        public bool isEnemy = false;

        public string DialogIcon;

        /// <summary>
        /// 蓄力音效
        /// </summary>
        public string AudioCharge;

        /// <summary>
        /// 攻击音效
        /// </summary>
        public string AudioAttack;

        /// <summary>
        /// 受击音效
        /// </summary>
        public string AudioHit;

        /// <summary>
        /// 死亡音效
        /// </summary>
        public string AudioDie;

        /// <summary>
        /// 升级音效1
        /// </summary>
        public string AudioUpgrade1;

        /// <summary>
        /// 升级音效2
        /// </summary>
        public string AudioUpgrade2;

        /// <summary>
        /// 升级音效3
        /// </summary>
        public string AudioUpgrade3;

        public string PrefabPath;

        /// <summary>
        /// 血条位置////
        /// </summary>
        public int HpPos;

        /// <summary>
        /// 攻击倍数
        /// </summary>
        public float AttackMp;


        public float UnitScale;

        public Vector3 OffsetCoordinate;

        /// <summary>
        /// 使用的皮肤
        /// </summary>
        public string Skin;


        public UnitConfig(XMLInStream inStream)
        {
            UnitSkillIdArr = new List<int>();
            int color;
            string UnitSkillIds;
            string unitGift;
            int isEnemy = 0;
            string OffsetCoor;

            inStream.Attribute("id", out ID).
            Attribute("unitName", out UnitName).
            Attribute("unitTypeId", out UnitTypeId).
            Attribute("level", out Level).
            Attribute("nameStringId", out NameStringId).
            Attribute("stroyStringId", out StroyStringId).
            Attribute("unitUpgrade", out UnitUpgrade).
            Attribute("unitHp", out UnitHp).
            Attribute("unitAtk", out UnitAtk).
            Attribute("attackMp", out AttackMp).
            Attribute("unitUpgradeA", out UnitUpgradeA).
            Attribute("unitUpgradeB", out UnitUpgradeB).
            Attribute("unitUnlock", out UnitUnlock).
            Attribute("unitSkillId", out UnitSkillIds).
            Attribute("unitColor", out color).
            Attribute("unitGift", out unitGift).
            Attribute("resourceIcon", out ResourceIcon).
            Attribute("hpPos", out HpPos).
            Attribute("tipsIcon", out TipsIcon).
            Attribute("isEnemy", out isEnemy).
            Attribute("audioCharge", out AudioCharge).
            Attribute("audioAttack", out AudioAttack).
            Attribute("audioHit", out AudioHit).
            Attribute("DialogIcon", out DialogIcon).
            Attribute("scale", out UnitScale).
            Attribute("coordinate", out OffsetCoor).
            Attribute("audioDie", out AudioDie).
            Attribute("audioUpgrade1", out AudioUpgrade1).
            Attribute("audioUpgrade2", out AudioUpgrade2).
            Attribute("audioUpgrade3", out AudioUpgrade3).
            Attribute("skin", out Skin);

            UnitColor = (ColorType)color;
            UnitGift = ItemQtt.ParseMulti(unitGift);
            this.isEnemy = isEnemy != 0;

            ///伙伴技能
            string[] skillArr = UnitSkillIds.Split(',');
            for (int i = 0; i < skillArr.Length; i++)
            {
                UnitSkillIdArr.Add(Convert.ToInt32(skillArr[i]));
            }
            if (UnitSkillIdArr.Count == 0)
            {
                UnitSkillId = -1;
            }
            else
            {
                UnitSkillId = UnitSkillIdArr[0];
            }

            string[] coors = OffsetCoor.Split(',');
            OffsetCoordinate = new Vector3(0, 0, 0);
            if (coors.Length >=2 )
            {
                OffsetCoordinate.x = Convert.ToSingle(coors[0]);
                OffsetCoordinate.y = Convert.ToSingle(coors[1]);
            }

            PrefabPath = string.Format("Assets/ExternalRes/Unit/{0}/{1}.prefab", ResourceIcon, ResourceIcon);
        }
    }
}
