using Com4Love.Qmax.Net.Protocols.goods;
using System;
using System.Collections.Generic;
using Com4Love.Qmax.Data.Config;
using UnityEngine;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.VO;

namespace Com4Love.Qmax.Ctr
{
    public class PropCtr
    {

        public enum BuyMode
        {
            None = 0,
            Coin =1,
            Gem = 2
        };

        Dictionary<int, GoodsConfig.SType> AllPropDic;
        Dictionary<int, bool> PropSelectDic;
        Dictionary<PropType,int> PropUseNumDic;
        Dictionary<PropType, Action<bool,float>> PropNumEffActionDic;

        /// <summary>
        /// 道具選擇事件，如選擇道具顯示道具信息，按鈕排他事件....///
        /// </summary>
        public Dictionary<PropType, Action<PropType>> PropSelectActionDic;

        /// <summary>
        /// 購買道具消費統計//
        /// 類型，bool表示是購買還是取消購買//
        /// </summary>
        public Action<PropType,bool> PropBuyCountAction;

        /// <summary>
        /// 當前選擇的道具//
        /// </summary>
        public PropType TemSelectedProp;


        List<int> AllPassivePropID;
        List<int> AllActivePropID;
        public void Clear()
        {
            PropSelectDic.Clear();
            PropUseNumDic.Clear();
            PropNumEffActionDic.Clear();
            PropSelectActionDic.Clear();

            PropBuyCountAction = null;
            TemSelectedProp = PropType.None;
        }

        public void ClearBuyCountAction(Action<PropType, bool> buyac =null)
        {
            if (buyac != null)
                PropBuyCountAction -= buyac;
            else
                PropBuyCountAction = null;
        }

        /// <summary>
        /// 清除所有道具的使用數量和選擇狀態//
        /// </summary>
        public void ClearSelectAndUse()
        {
            ClearSelect();
            PropUseNumDic.Clear();
        }

        /// <summary>
        /// 清除所有道具選擇狀態//
        /// </summary>
        public void ClearSelect()
        {
            PropSelectDic.Clear();
            TemSelectedProp = PropType.None;
        }

        public PropCtr()
        {
            AllPropDic = new Dictionary<int, GoodsConfig.SType>();
            PropSelectDic = new Dictionary<int, bool>();
            PropUseNumDic = new Dictionary<PropType, int>();
            PropNumEffActionDic = new Dictionary<PropType, Action<bool, float>>();

            PropSelectActionDic = new Dictionary<PropType, Action<PropType>>();

            AllPassivePropID = new List<int>();
            AllActivePropID = new List<int>();

            TemSelectedProp = PropType.None;
        }

        public Dictionary<int, GoodsConfig.SType> GetAllPropDic()
        {
            if (AllPropDic.Count != 0)
                return AllPropDic;

            foreach (var config in GameController.Instance.Model.GoodsConfigs)
            {
                if (config.Value.SubType == GoodsConfig.SType.ActiveProp
                    || config.Value.SubType == GoodsConfig.SType.PassiveProp)
                {
                    AllPropDic.Add(config.Value.UID, config.Value.SubType);
                }
            }

            return AllPropDic;
        }

        public GoodsConfig.SType GetPropSType(int propID)
        {
            if (AllPropDic.Count == 0)
                GetAllPropDic();

            if (AllPropDic.ContainsKey(propID))
                return AllPropDic[propID];

            return GoodsConfig.SType.None;
        }

        /// <summary>
        /// 獲取道具數量//
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public int GetPropNum(int ID)
        {
            if (GameController.Instance.GoodsCtr.GoodsItemMap.ContainsKey(ID))
            {
                return GameController.Instance.GoodsCtr.GoodsItemMap[ID].num;
            }

            return 0;

        }

        public int GetPropNum(PropType propType)
        {
            return GetPropNum((int)propType);
        }

        /// <summary>
        /// 獲取道具選擇狀態//
        /// </summary>
        /// <param name="propType"></param>
        /// <returns></returns>
        public bool GetPropSelect(PropType propType)
        {
            int propid = (int)propType;

            return GetPropSelect(propid);
        }

        public bool GetPropSelect(int propid)
        {
            if (PropSelectDic.ContainsKey(propid))
            {
                return PropSelectDic[propid];
            }
            else
            {
                PropSelectDic.Add(propid, false);
            }
            return false;
        }

        /// <summary>
        /// 設置道具被選中狀態/// 
        ///                ？？？在點擊道具並成功使用後,代碼執行了 (SetPropSelect2_1 處)， 將 TemSelectedProp 賦了值???        
        /// </summary>
        /// <param name="propType"></param>
        /// <param name="select"></param>
        public void SetPropSelect(PropType propType,bool select)
        {
            //Debug.LogWarning("SetPropSelect1");

            int propid = (int)propType;

            if (PropSelectDic.ContainsKey(propid))
            {
                PropSelectDic[propid] = select;
            }
            else
            {
                PropSelectDic.Add(propid, select);
            }

            /////選中則臨時選中道具變量改變//
            ////if(select)
            if (GetPropNum(propType) == 0)
            {
                //Debug.LogWarning("SetPropSelect2_1");

                TemSelectedProp = propType;
            }
            else
            {
                //Debug.LogWarning("SetPropSelect2_2");

                TemSelectedProp = PropType.None;
            }
        }

        //所有ctr 相關的初始化動作放在Reset和構造方法裡 不應該gameCtr中單獨調用
        public void TemSelectPropReset()
        {
            TemSelectedProp = PropType.None;
        }

        public void SetPropSelect(int id, bool select)
        {
            SetPropSelect((PropType)id, select);
        }

        public void SetPropSelect(List<int> propList, bool select = true)
        {
            if (propList == null)
                return;

            foreach (var proptype in propList)
            {
                SetPropSelect(proptype, select);
            }
        }

        public List<PropType> GetAllPropSelectedList()
        {
            List<PropType> list = new List<PropType>();

            foreach (var sele in PropSelectDic)
            {
                if (sele.Value)
                    list.Add((PropType)sele.Key);
            }

            return list;
        }

        /// <summary>
        /// 設置使用被動道具數量///
        /// 記錄的數據在上傳給服務器///
        /// </summary>
        /// <param name="propType"></param>
        /// <param name="num"></param>
        public void SetPropUseNum(PropType propType, int num)
        {
            if (PropUseNumDic.ContainsKey(propType))
            {
                PropUseNumDic[propType] = num;
            }
            else
            {
                PropUseNumDic.Add(propType,num);
            }
        }

        public int GetPropUseNum(PropType propType)
        {
            int num = 0;
            if (PropUseNumDic.ContainsKey(propType))
            {
                num = PropUseNumDic[propType];
            }

            return num;
        }

        public int GetPropUseNum(int id)
        {
            int num = 0;
            PropType propType = (PropType)id;
            if (PropUseNumDic.ContainsKey(propType))
            {
                num = PropUseNumDic[propType];
            }

            return num;
        }

        /// <summary>
        ///獲取所有選中了但是卻沒有的被動道具，相當於在購物車中的被動道具//
        /// </summary>
        /// <returns></returns>
        public List<int> GetNoneAndSelectList()
        {
            List<int> allselect = new List<int>();

            GetAllPassivePropIDList();
            GetAllActivePropIDList();

            ///選中但是擁有數量為0的被動道具///
            foreach (var pasid in AllPassivePropID)
            {
                if (GetPropSelect(pasid) && GetPropNum(pasid)<=0 )
                    allselect.Add(pasid);
            }

            ///選中但是擁有數量為0的主動道具///
            foreach (var pasid in AllActivePropID)
            {
                if (GetPropSelect(pasid) && GetPropNum(pasid) <= 0)
                    allselect.Add(pasid);
            }

            return allselect;
        }

        /// <summary>
        /// 獲取所有選中的被動道具//
        /// </summary>
        /// <returns></returns>
        public List<int> GetPassiveSelectList()
        {
            List<int> allselect = new List<int>();

            GetAllPassivePropIDList();

            foreach (var pasid in AllPassivePropID)
            {
                if (GetPropSelect(pasid))
                    allselect.Add(pasid);
            }

            return allselect;
        }

        /// <summary>
        /// 獲取所有被動道具使用情況，返回所有使用數量不為0的Dictionary//
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> GetAllPassivePropUseDic()
        {
            Dictionary<int, int> passiveUseDic = new Dictionary<int, int>();

            GetAllPassivePropIDList();

            foreach (var pasid in AllPassivePropID)
            {
                int num = GetPropUseNum(pasid);

                if(num>0)
                    passiveUseDic.Add(pasid, num);
            }

            return passiveUseDic;


        }
        /// <summary>
        /// 獲取所有被動道具ID//
        /// </summary>
        /// <returns></returns>
        public List<int> GetAllPassivePropIDList()
        {
            if (AllPassivePropID.Count == 0)
            {
                Dictionary<int, GoodsConfig.SType> alldic = GetAllPropDic();

                foreach (var prop in alldic)
                {
                    if (prop.Value == GoodsConfig.SType.PassiveProp)
                    {
                        AllPassivePropID.Add(prop.Key);
                    }
                }
            }

            return AllPassivePropID;
        }
        /// <summary>
        /// 獲取所有主動道具ID//
        /// </summary>
        /// <returns></returns>
        public List<int> GetAllActivePropIDList()
        {
            if (AllActivePropID.Count == 0)
            {
                Dictionary<int, GoodsConfig.SType> alldic = GetAllPropDic();

                foreach (var prop in alldic)
                {
                    if (prop.Value == GoodsConfig.SType.ActiveProp)
                    {
                        AllActivePropID.Add(prop.Key);
                    }
                }
            }

            return AllActivePropID;
        }

        /// <summary>
        /// 獲取所有被選擇的主動道具//
        /// </summary>
        /// <returns></returns>
        public List<int> GetAllSelectActivePropIDlist()
        {
            List<int> allaclist = GetAllActivePropIDList();
            List<int> selectlist = new List<int>();

            foreach (var id in allaclist)
            {
                if (GetPropSelect(id))
                    selectlist.Add(id);
            }

            return selectlist;
        }
        /// <summary>
        /// 獲取使用效果值///
        /// </summary>
        /// <param name="propType"></param>
        /// <returns></returns>
        public float GetPropValue(PropType propType)
        {
            int id = (int)propType;
            float propvalue = 0;

            if (GameController.Instance.Model.GoodsConfigs.ContainsKey(id))
            {
                //Arg0  改為數組，當此道具為套裝時，效果值為多種
                float.TryParse(GameController.Instance.Model.GoodsConfigs[id].Arg0[0], out propvalue);
            }

            return propvalue;
        }

        public Dictionary<PropType, int> GetPropUseDic()
        {
            return PropUseNumDic;
        }

        public void AddPropNumEffAction(PropType propType, Action<bool,float> ac)
        {
            if (PropNumEffActionDic.ContainsKey(propType))
            {
                PropNumEffActionDic[propType] += ac;
            }
            else
            {
                PropNumEffActionDic.Add(propType, ac);
            }
        }

        public Action<bool, float> GetPropNumEffAction(PropType propType)
        {
            if (PropNumEffActionDic.ContainsKey(propType))
                return PropNumEffActionDic[propType];

            return null;
        }

        /// <summary>
        /// 清除數字動態效果事件//
        /// </summary>
        /// <param name="propType"></param>
        public void ClearPropNumEffAction(PropType propType = PropType.None)
        {
            if (propType == PropType.None)
            {
                PropNumEffActionDic.Clear();
            }
            else
            {
                if (PropNumEffActionDic.ContainsKey(propType))
                    PropNumEffActionDic[propType] = null;
            }
              
        }

        /// <summary>
        /// 清除道具選擇事件///
        /// </summary>
        /// <param name="propType"></param>
        public void ClearPropSelectAction(PropType propType = PropType.None)
        {
            if (propType == PropType.None)
            {
                PropSelectActionDic.Clear();
            }
            else
            {
                if (PropSelectActionDic.ContainsKey(propType))
                    PropSelectActionDic[propType] = null;
            }
        }

        public bool UseProp(PropType propType,int num,Action useEvent)
        {
            int id = (int)propType;

            if (GetPropNum(id) <= 0)
                return false;

            GameController.Instance.Client.UseGoods(id, num);


            Action<UseGoodsResponse> uevent = null;

            uevent = delegate (UseGoodsResponse res)
            {
                GameController.Instance.ModelEventSystem.OnUseGoodsItem -= uevent;

                ///設置該道具為未選中狀態//
                SetPropSelect(propType,false);

                //新增 物品使用之後，清理相關的選中信息 主要是將TemSelectedProp清理
                //2016 /2/1
                ClearSelect();

                ///道具效果//
                if (useEvent != null)
                    useEvent();
            };
            GameController.Instance.ModelEventSystem.OnUseGoodsItem += uevent;



            return true;
        }

        public bool UseProp(PropType propType, Action useEvent)
        {
            return UseProp(propType, 1, useEvent);
        }
        /// <summary>
        /// 教學作弊專用,沒有經過服務器//
        /// </summary>
        public bool UsePropByGuide(PropType propType, Action useEvent)
        {
            ///設置該道具為未選中狀態//
            SetPropSelect(propType, false);
            ///道具效果//
            if (useEvent != null)
                useEvent();

            return true;
        }

        /// <summary>
        /// 檢查遊戲貨幣是否夠支付購買//
        /// </summary>
        /// <param name="proptype">需要加入價格計算的道具</param>
        /// withOther 是否和其他的選中的道具一起加入價格判定計算///
        /// <returns></returns>
        public bool CheckBuyProp(PropType proptype = PropType.None,bool withOther = true)
        {
            int curid = (int)proptype;
            //return GameController.Instance.GoodsCtr.CheckBuyGoodsAbleByGoodsID(id);
            
            if (withOther)
            {
                ///和其他選中道具一起加入計算//
                List<int> noneAndSelect = GetNoneAndSelectList();

                if (proptype != PropType.None)
                    noneAndSelect.Add(curid);

                return CheckBuyProp(noneAndSelect);
            }
            else
            {
                ///單個道具不與其他道具一起加入計算//
                if (proptype != PropType.None)
                    return GameController.Instance.GoodsCtr.CheckBuyGoodsAbleByGoodsID(curid);
            }

            ///道具類型為none並且不與其他選中道具加入計算//
            return false;
        }

        public bool CheckBuyProp(List<int> idlist)
        {
            int gem = 0;
            int coin = 0;

            foreach (int id in idlist)
            {
                ShopConfig config = GetShopConfig(id);

                if (config != null)
                {
                    gem += config.Gem;
                    coin += config.Coin;
                }
                else
                {
                    Q.Log("沒有該物品的商店配置 ", id);
                    return false;
                }

            }

            if (GameController.Instance.Model.PlayerData.gem >= gem
                && GameController.Instance.Model.PlayerData.coin >= coin)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 遊戲幣購買單個道具//
        /// </summary>
        /// <param name="proptype"></param>
        /// <returns></returns>
        public bool BuySingleGoods( PropType proptype)
        {
            int id = (int)proptype;
            return GameController.Instance.GoodsCtr.BuySingleGoods(id);
        }

        public bool BuyPropList(List<int> idlist)
        {
            ///如果遊戲幣不夠支付//
            if (!CheckBuyProp())
                return false;

            List<int> shopid = new List<int>();

            foreach (int id in idlist)
            {
                ShopConfig shopConfig = GameController.Instance.Model.SingleGoodsShopConfig[id];

                if (shopConfig != null)
                    shopid.Add(shopConfig.UID);
                else
                {
                    UnityEngine.Debug.LogError(string.Format("道具 {0} 沒有商店購買配置 ", id));

                    return false;
                }
                    
            }

            ///購買所選中的道具//
            GameController.Instance.GoodsCtr.BuyGoodsList(shopid);

            return true;
        }

        /// <summary>
        /// 獲取道具的商店配置
        /// </summary>
        /// <param name="proptype"></param>
        /// <returns></returns>
        public ShopConfig GetShopConfig(PropType proptype)
        {
            return GetShopConfig((int)proptype);
        }
        public ShopConfig GetShopConfig(int id)
        {
            return GameController.Instance.GoodsCtr.GetShopConfigByGoodsId(id);
        }

        /// <summary>
        /// 購買方式//
        /// </summary>
        /// <param name="proptype"></param>
        /// <returns></returns>
        public BuyMode GetBuyMode(PropType proptype)
        {
            return GetBuyMode((int)proptype);
        }

        public BuyMode GetBuyMode(int id)
        {
            ShopConfig config = GetShopConfig(id);

            if (config == null)
                return BuyMode.None;

            if (config.Coin != 0)
                return BuyMode.Coin;
            else if (config.Gem != 0)
                return BuyMode.Gem;

            return BuyMode.None;
        }

        public string GetPropPaymentID(int goodsId, bool isCombot)
        {
            string paymentID = null;
            int localP = isCombot ? 2: 1;
            foreach (PaymentSystemConfig item in GameController.Instance.Model.PaymentSystemConfigs.Values)
            {
                if (item.GoodsID == goodsId && item.InCombat == localP)
                {
                    paymentID = item.PaymentId;
                }
            }

            return paymentID;
        }

        /// <summary>
        /// 當前擁有的鑽石是否足夠購買當前道具所需花費的金幣//
        /// </summary>
        /// <param name="proptype"></param>
        /// <returns></returns>
        public bool IsGemToCoin(PropType proptype)
        {
            bool iscan = false;

            ShopConfig config = GetShopConfig(proptype);

            if (config != null)
            {
                ///tap為3是鑽石購買金幣的配置//
                List<ShopConfig> shoplist = GameController.Instance.Model.ShopConfigByTap[3];

                foreach (var lis in shoplist)
                {
                    if (lis.ByNum > config.Coin && GameController.Instance.Model.PlayerData.gem >= lis.Gem)
                    {
                        iscan = true;
                        break;
                    }
                }
            }

            return iscan;
        }

        /// <summary>
        /// 剩餘步數
        /// </summary>
        /// <param name="num"></param>
        public void SetRemainSteps(BattleModel battleModel)
        {
            ////增加步數///
            int addsteps = 0;
            if (GetPropUseNum(PropType.AddState) > 0)
            {
                addsteps = System.Convert.ToInt32(GetPropValue(PropType.AddState));
            }
            battleModel.RemainSteps = battleModel.StageLimit + addsteps;

        }

        //設置怪物血量//
        public void SetEnemiesData(BattleModel battleModel,UnitConfig cfg)
        {
            if (battleModel.EnemiesData == null)
            {
                Debug.LogFormat("EnemiesData is null ");
            }
            Unit monster = new Unit(cfg);
            ///被動減少怪物血量///
            if (GetPropUseNum(PropType.ReduceMonsterHP) > 0)
            {
                float point = GetPropValue(PropType.ReduceMonsterHP) / 100;
                monster.HpMax = System.Convert.ToInt32(cfg.UnitHp * (1.0f - point));
                monster.Hp = monster.HpMax;
            }

            battleModel.EnemiesData.Add(monster);
        }

        //返回配置過關目標怪物的數量
        public int SetCurrentGoal(BattleModel battleModel,List<StageConfig.Goal> goals)
        {
            PropCtr propCtr = GameController.Instance.PropCtr;
            battleModel.CurrentGoal = new List<StageConfig.Goal>();

            StageConfig.Goal enemyGoal = new StageConfig.Goal();

            ///要斷引用//
            foreach (var goal in goals)
            {
                StageConfig.Goal go = goal.CopyTo();
                battleModel.CurrentGoal.Add(go);

                ///記錄配置的怪物目標//
                if (go.Type == BattleGoal.Unit)
                    enemyGoal = go;
            }

            /////減少目標物體////
            if (propCtr.GetPropUseNum(PropType.ReduceGoal) > 0)
            {
                float point = propCtr.GetPropValue(PropType.ReduceGoal) / 100;

                foreach (var goal in battleModel.CurrentGoal)
                {
                    goal.Num = System.Convert.ToInt32(goal.Num * (1 - point));
                }
            }

            return enemyGoal.Num;
        }


        /// <summary>
        /// 增加分數
        /// 返回增加分數
        /// isAddTrue表示是否加到數據層
        /// </summary>
        /// <param name="svalue"></param>
        /// <returns></returns>
        public int AddScore(BattleModel battleModel,int svalue,bool isAddTrue = true)
        {
            float point = 0;

            if (GetPropUseNum(PropType.AddScore) > 0)
            {
                point = GetPropValue(PropType.AddScore) / 100;
            }
            
            int addscore = System.Convert.ToInt32(svalue * (1 + point));

            if (isAddTrue)
                battleModel.Score += addscore;

            return addscore;
        }

        public int AddHurt( int hurtVa)
        {
            if (GetPropUseNum(PropType.AddHurt) > 0)
            {
                float hurtpoint = GetPropValue(PropType.AddHurt) / 100;

                hurtVa = System.Convert.ToInt32(hurtVa * (1 + hurtpoint));
            }

            return hurtVa;
        }

        public List<StageConfig.Goal> GetGoalByStar(BattleModel battleModel, byte star)
        {
            List<StageConfig.Goal> goals = BattleTools.ParseGoal(battleModel.CurStage.targets[star]);

            /////減少目標物體////
            if (GetPropUseNum(PropType.ReduceGoal) > 0)
            {
                float point = GetPropValue(PropType.ReduceGoal) / 100;

                foreach (var goal in goals)
                {
                    goal.Num = System.Convert.ToInt32(goal.Num * (1 - point));
                }
            }

            return goals;
        }

        int GetGoalScore(List<StageConfig.Goal> goals)
        {
            int num = 0;
            foreach (var goal in goals)
            {
                if (goal.Type == BattleGoal.Score)
                {
                    num = goal.Num;
                    break;
                }
            }

            return num;
        }

        public int[] SetStarScore(BattleModel battleModel)
        {
            battleModel.StarScore = new int[3]{ GetGoalScore(GetGoalByStar(battleModel, 1)), GetGoalScore(GetGoalByStar(battleModel, 2)), GetGoalScore(GetGoalByStar(battleModel, 3)) };

            return battleModel.StarScore;
        }

    }
}

