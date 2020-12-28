using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Data;
using System;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data.Config;

namespace Com4Love.Qmax.Helper
{
    public class NoCDThrowSkillHelper
    {
        BoardBehaviour boardbeh;
        public NoCDThrowSkillHelper(BoardBehaviour boardbeh)
        {
            this.boardbeh = boardbeh;
        }

        public bool Play(ColorType colorType)
        {
            BattleModel battleModel = GameController.Instance.Model.BattleModel;

            int befCD = battleModel.SkillCDDict[colorType];
            ///設置滿技能CD///

            if (!battleModel.SkillConfDict.ContainsKey(colorType))
                return false;

            SkillConfig skillConf = battleModel.SkillConfDict[colorType];
            battleModel.SkillCDDict[colorType] = skillConf.SkillCD;

            List<TileObject> throwTileList = battleModel.CheckSkillCD();

            ///面板上不能再投擲技能了////
            if (throwTileList.Count == 0)
            {
                battleModel.SkillCDDict[colorType] = befCD;
                return false;
            }

            ///鎖定面板///
            boardbeh.PlusInteractLock();

            SkillLoadingBehaviour skillLoading = boardbeh.SkillLoadings[colorType];
            skillLoading.gameObject.SetActive(true);
            skillLoading.SetPercentage(1f, true);

            Action<TileObject> AddSkillTileEffect = delegate(TileObject tObj)
            {
                ///關閉所有技能面板//
                foreach (var skill in boardbeh.SkillLoadings)
                {
                    skill.Value.gameObject.SetActive(false);
                }
                ///當前技能記得清零//
                skillLoading.SetPercentage(0f);
                boardbeh.MinusInteractLock();
                //加入技能殺光效果///
                //Prefab名字不能改，美術以後會改動///
                UnityEngine.Object obj = Resources.Load("Prefabs/Effects/EffectJinengtubiao");
                if (obj != null)
                {
                    GameObject gameObject = GameObject.Instantiate(obj) as GameObject;
                    gameObject.name = "EffectJinengtubiao";
                    gameObject.transform.SetParent(boardbeh.GetTypeGameObjects(TileType.Element)[tObj.Row, tObj.Col].transform);
                    gameObject.transform.localPosition = Vector3.zero;
                }
                bool isCanNext = GameController.Instance.PlayingRuleCtr.CheckElimatablePath(null, battleModel.LinkableStack);

                if (!isCanNext)
                {
                    GameController.Instance.PlayingRuleCtr.Rearrange();
                }
            };

            TileObject tile = throwTileList[0];
            Animator anim = boardbeh.UnitAnims[tile.Config.ColorType];
            ThrowTileHelper.Play(boardbeh, tile, anim, AddSkillTileEffect);

            return true;
        }
    }
}

