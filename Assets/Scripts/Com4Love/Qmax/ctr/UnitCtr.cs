using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.Unit;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    public class UnitCtr : IDisposable
    {
        public UnitCtr()
        {
            GameController.Instance.Client.AddResponseCallback(Module.Unit, OnUnitResponse);
        }

        public void Clear() { }


        public void Dispose()
        {
            GameController.Instance.Client.RemoveResponseCallback(Module.Unit, OnUnitResponse);
        }

        private void OnUnitResponse(byte module, byte cmd, short status, object value)
        {
            Action OnMainThread = delegate()
            {
                GameController gameCtr = GameController.Instance;
                if (cmd == (byte)UnitCmd.UPGRAD_UNIT)
                {
                    if (status != 0)
                    {
                        Q.Warning("升級夥伴失敗");
                        gameCtr.AlertRespLogicException(module, cmd, status, true,
                            delegate(byte m, byte c, int s)
                            {
                                //即使失敗，也要調用這個事件，否則界面會被鎖住
                                //其實因為請求有Loading，所以點擊升級時，並沒有必要鎖界面
                                if (gameCtr.ModelEventSystem.OnUnitUpgrad != null)
                                    gameCtr.ModelEventSystem.OnUnitUpgrad();
                            }
                        );

                        return;
                    }

                    UpgradeUnitResponse res = value as UpgradeUnitResponse;

                    //更新玩家的伙伴
                    foreach (Unit unit in gameCtr.PlayerCtr.PlayerData.list)
                    {
                        if (unit.unitId == res.beforeUnitId)
                        {
                            unit.unitId = res.afterUnitId;
                            break;
                        }
                    }

                    //更新玩家的資源
                    gameCtr.PlayerCtr.UpdateByResponse(res.valueResultList);
                    //玩傢伙伴更新事件
                    if (gameCtr.ModelEventSystem.OnUnitUpgrad != null)
                        gameCtr.ModelEventSystem.OnUnitUpgrad();
                }
                else if (cmd == (byte)UnitCmd.BUY_UPGRADE)
                {
                    if (status != 0)
                    {
                        Q.Warning("夥伴購買升級失敗");
                        gameCtr.AlertRespLogicException(module, cmd, status);
                        return;
                    }

                    ValueResultListResponse res = value as ValueResultListResponse;
                    gameCtr.PlayerCtr.UpdateByResponse(res);
                    if (gameCtr.ModelEventSystem.OnBuyUpgrade != null)
                        gameCtr.ModelEventSystem.OnBuyUpgrade();
                }
                else if (cmd == (byte)UnitCmd.FAST_UPGRAD_UNIT)
                {
                    if (status != 0)
                    {
                        Q.Warning("快速升級夥伴失敗");
                        gameCtr.AlertRespLogicException(module, cmd, status, true,
                            delegate(byte m, byte c, int s)
                            {
                                //即使失敗，也要調用這個事件，否則界面會被鎖住
                                //其實因為請求有Loading，所以點擊升級時，並沒有必要鎖界面
                                if (gameCtr.ModelEventSystem.OnUnitUpgrad != null)
                                    gameCtr.ModelEventSystem.OnUnitUpgrad();
                            }
                        );
                        return;
                    }

                    FastUpgradeUnitResponse res = value as FastUpgradeUnitResponse;

                    //更新玩家的伙伴
                    foreach (Unit unit in gameCtr.PlayerCtr.PlayerData.list)
                    {
                        if (unit.unitId == res.beforeUnitId)
                        {
                            unit.unitId = res.afterUnitId;
                            break;
                        }
                    }
                    //更新玩家的資源
                    gameCtr.PlayerCtr.UpdateByResponse(res.valueResultList);

                    //玩家伙伴更新事件
                    if (gameCtr.ModelEventSystem.OnUnitUpgrad != null)
                        gameCtr.ModelEventSystem.OnUnitUpgrad();

                }
            };

            GameController.Instance.InvokeOnMainThread(OnMainThread);
        }

        /// <summary>
        /// 升级伙伴
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="isAutoUpgrad"></param>
        public void UpgradUnit(int unitId, bool isAutoUpgrad)
        {
            if (!isAutoUpgrad)
                GameController.Instance.Client.UpgradUnit(unitId);
            else
                GameController.Instance.Client.AutoUpgradUnit(unitId);
        }

        protected Dictionary<ColorType, List<UnitConfig>> lockUnits = new Dictionary<ColorType, List<UnitConfig>>();
        /// <summary>
        /// 獲取被鎖定的伙伴
        /// </summary>
        /// <param name="passStage"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public List<UnitConfig> GetLockUnits(int passStage, ColorType color)
        {
            List<UnitConfig> colorUnitList = new List<UnitConfig>();
            List<UnitConfig> Units;

            if (!lockUnits.ContainsKey(color))
            {
                Units = new List<UnitConfig>();

                foreach (KeyValuePair<int, UnitConfig> kv in GameController.Instance.Model.UnitConfigs)
                {
                    if (kv.Value.UnitUnlock > 0 && kv.Value.UnitColor == color)
                    {
                        Units.Add(kv.Value);
                    }
                }

                lockUnits.Add(color, Units);
            }

            Units = lockUnits[color];

            foreach (UnitConfig uConfig in Units)
            {
                //已解鎖到特定關卡，並且沒有相同類型的伙伴
                if (uConfig.UnitUnlock <= passStage && !HasTypeUnit(uConfig.ID))
                {
                    colorUnitList.Add(uConfig);
                }
            }

            return colorUnitList;
        }

        /// <summary>
        /// 通過某一隻夥伴的ID獲取此1級時的伙伴 如 3320 是由 3301 最終升級到的
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public UnitConfig GetLvlUnit(int id)
        {
            UnitConfig config = GameController.Instance.Model.UnitConfigs[id];

            // UnitTypeId 加上 Level 等於 id
            int lvl1Id = config.UnitTypeId * 100 + 1;
            config = GameController.Instance.Model.UnitConfigs[lvl1Id];
            Q.Assert(config != null, "等級1夥伴：" + lvl1Id + " 不存在");

            return config;
        }

        /// <summary>
        ///玩家是否擁有此類夥伴 根據 unitTypeId 判斷
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasTypeUnit(int id)
        {
            int unitTypeId = GameController.Instance.Model.UnitConfigs[id].UnitTypeId;
            UnitConfig ownUnit;

            foreach (Unit unit in GameController.Instance.PlayerCtr.PlayerData.list)
            {
                ownUnit = GameController.Instance.Model.UnitConfigs[unit.unitId];

                if (ownUnit.UnitTypeId == unitTypeId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasIdUnit(int id)
        {
            UnitConfig ownUnit;

            foreach (Unit unit in GameController.Instance.PlayerCtr.PlayerData.list)
            {
                ownUnit = GameController.Instance.Model.UnitConfigs[unit.unitId];

                if (ownUnit.ID == id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 獲取玩家擁有的某類型夥伴 玩家只能擁有1只某類型的伙伴
        /// </summary>
        /// <param name="unitType"></param>
        /// <returns></returns>
        public UnitConfig GetOwnTypeUnit(int unitType)
        {
            foreach (Unit item in GameController.Instance.PlayerCtr.PlayerData.list)
            {
                UnitConfig config = GameController.Instance.Model.UnitConfigs[item.unitId];

                if (config.UnitTypeId == unitType)
                {
                    return config;
                }
            }

            return null;
        }

        /// <summary>
        /// 根據顏色獲取擁有的伙伴
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public List<UnitConfig> GetOwnUnits(ColorType color)
        {
            if (GameController.Instance.PlayerCtr.PlayerData == null)
                return null;

            List<UnitConfig> units = new List<UnitConfig>();

            foreach (Unit item in GameController.Instance.PlayerCtr.PlayerData.list)
            {
                UnitConfig config = GameController.Instance.Model.UnitConfigs[item.unitId];

                if (config.UnitColor == color)
                {
                    units.Add(config);
                }
            }

            return units;
        }

        /// <summary>
        /// 獲取所有擁有的小伙伴
        /// </summary>
        /// <returns></returns>
        public List<UnitConfig> GetOwnUnits()
        {
            if (GameController.Instance.PlayerCtr.PlayerData == null)
                return null;

            List<UnitConfig> units = new List<UnitConfig>();

            foreach (Unit item in GameController.Instance.PlayerCtr.PlayerData.list)
            {
                UnitConfig config = GameController.Instance.Model.UnitConfigs[item.unitId];
                units.Add(config);
            }

            return units;
        }

        /// <summary>
        /// 通過類型與等級獲取夥伴 
        /// </summary>
        /// <param name="unitType"></param>
        /// <param name="lvl"></param>
        /// <returns></returns>
        public UnitConfig GetUnit(int unitType, int lvl)
        {
            int id = unitType * 100 + lvl;
            UnitConfig config = GameController.Instance.Model.UnitConfigs[id];

            // UnitTypeId 加上 Level 等于 id

            config = GameController.Instance.Model.UnitConfigs[id];
            Q.Assert(config != null, id + "夥伴不存在");

            return config;
        }

        /// <summary>
        /// 判斷某個夥伴是否可以升級
        /// </summary>
        /// <param name="config">你懂得</param>
        /// <param name="CheckMaterial">是否判断材料足够，如果不判断，则此伙伴只要没有升级到最高，就表示可以继续升级</param>
        /// <param name="CheckGem">是否判断钻石足够，当材料不足时，如果购买所缺材料的钻石足够则表示可以升级</param>
        /// <returns></returns>
        public bool CheckUpgradAble(UnitConfig config, bool CheckMaterial = false, bool CheckGem = false)
        {
            if (CheckGem && !CheckMaterial)
            {
                //不允許只檢查鑽石不檢查材料
                Q.Assert(false, "不允許只檢查鑽石不檢查材料");
                return false;
            }

            ActorGameResponse playerData = GameController.Instance.PlayerCtr.PlayerData;

            if (config.UnitUpgrade == -1 || playerData == null)
                return false;

            int[] upgradeANeeds;

            if (CheckMaterial)
            {
                upgradeANeeds = GetUpgradeMaterialLeft(config);

                if (upgradeANeeds[0] > 0 || upgradeANeeds[1] > 0)
                {
                    //材料不足
                    if (CheckGem)
                    {
                        //繼續檢查鑽石是否足夠
                        int gemNeed = GetGemCost(upgradeANeeds[0], upgradeANeeds[1]);

                        if (playerData.gem < gemNeed)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 獲取升級所缺少的材料數量
        /// </summary>
        /// <param name="unitConfig"></param>
        /// <returns></returns>
        public int[] GetUpgradeMaterialLeft(UnitConfig unitConfig)
        {
            int[] res = { 0, 0 };

            res[0] = unitConfig.UnitUpgradeA - GameController.Instance.PlayerCtr.PlayerData.upgradeA;
            res[1] = unitConfig.UnitUpgradeB - GameController.Instance.PlayerCtr.PlayerData.upgradeB;

            res[0] = res[0] > 0 ? res[0] : 0;
            res[1] = res[1] > 0 ? res[1] : 0;

            return res;
        }

        /// <summary>
        /// 一共能升級的伙伴數量
        /// </summary>
        /// <returns></returns>
        public int GetUpgradeAbleCount()
        {
            Unit unit;
            UnitConfig unitConfig;
            int count = 0;

            for (int i = 0; i < GameController.Instance.PlayerCtr.PlayerData.list.Count; i++)
            {
                unit = GameController.Instance.PlayerCtr.PlayerData.list[i];
                unitConfig = GameController.Instance.Model.UnitConfigs[unit.unitId];

                if (CheckUpgradAble(unitConfig, true, true))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 獲取夥伴名字   暫時使用 UnitName 需改為通過NameStringId從表中取
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public string GetUnitNameStr(UnitConfig config)
        {
            string nameStringId = config.NameStringId;

            return Utils.GetTextByStringID(nameStringId);
        }

        /// <summary>
        /// 同上   暫時直接取StroyStringId
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public string GetStroyStr(UnitConfig config)
        {
            string StroyStringId = config.StroyStringId;

            return Utils.GetTextByStringID(StroyStringId);
        }

        public string GetSkillNameStr(SkillConfig config)
        {
            string nameStringId = config.SkillStringId;

            return Utils.GetTextByStringID(nameStringId);
        }

        public string GetSkillEffectStr(SkillConfig config)
        {
            string stringId = config.EffectStringId;

            return Utils.GetTextByStringID(stringId);
        }

        /// <summary>
        /// 獲取購買升級材料所需要的花費
        /// </summary>
        /// <param name="upgradeA"></param>
        /// <param name="upgradeB"></param>
        /// <returns></returns>
        public int GetGemCost(float upgradeA, float upgradeB)
        {
            float gemCost = Mathf.Ceil(upgradeA / GameController.Instance.Model.GameSystemConfig.BuyUpgradeAForGem)
                + Mathf.Ceil(upgradeB / GameController.Instance.Model.GameSystemConfig.BuyUpgradeBForGem);

            return (int)gemCost;
        }

        /// <summary>
        /// 購買升級材料
        /// </summary>
        /// <param name="upgradeA">需要購買的upgradeA</param>
        /// <param name="upgradeB">需要購買的upgradeB</param>
        public bool BuyUpgrade(float upgradeA, float upgradeB)
        {
            Dictionary<byte, int> buyArgs = new Dictionary<byte, int>();

            int gemUpgradeACost = 0;
            int gemUpgradeBCost = 0;
            int gemCost = 0;

            if (upgradeA != 0)
            {
                gemUpgradeACost = (int)Mathf.Ceil(upgradeA / GameController.Instance.Model.GameSystemConfig.BuyUpgradeAForGem);

                buyArgs.Add(1, gemUpgradeACost);
            }

            if (upgradeB != 0)
            {
                gemUpgradeBCost = (int)Mathf.Ceil(upgradeB / GameController.Instance.Model.GameSystemConfig.BuyUpgradeBForGem);

                buyArgs.Add(0, gemUpgradeBCost);
            }

            gemCost = gemUpgradeACost + gemUpgradeBCost;

            if (GameController.Instance.PlayerCtr.PlayerData.gem < gemCost)
            {
                //鑽石不夠  
                return false;
            }
            else
            {
                GameController.Instance.Client.BuyUpgrade(buyArgs);
                return true;
            }
        }

    }
}
