using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Net.Protocols.goods;
using System;
using System.Collections.Generic;

namespace Com4Love.Qmax.Ctr
{
    public class GoodsCtr : IDisposable
    {
        protected Dictionary<int, GoodsItem> goodsItemMap;

        private GameController gameCtr;

        public GoodsCtr()
        {
            goodsItemMap = new Dictionary<int, GoodsItem>();

            gameCtr = GameController.Instance;
            gameCtr.Client.AddResponseCallback(Module.Goods, OnNetResponse);

            gameCtr.ModelEventSystem.OnGoodsRefreshList += RefreshAllGoods;
        }

        private void OnNetResponse(byte module, byte cmd, short status, object value)
        {
            Action OnMainThread = delegate ()
            {
                Dictionary<Module, BaseClient.OnResponse> dict = new Dictionary<Module, BaseClient.OnResponse>();
                dict.Add(Module.Goods, OnGoodsCmd);

                dict[(Module)module](module, cmd, status, value);
            };

            gameCtr.InvokeOnMainThread(OnMainThread);
        }

        public void Dispose()
        {
            gameCtr.Client.RemoveResponseCallback(Module.Goods, OnNetResponse);
            gameCtr.ModelEventSystem.OnGoodsRefreshList -= RefreshAllGoods;
        }

        public void Clear()
        {
            //重新登陸後 不同賬號的背包信息不一樣 重新初始化
            goodsItemMap = new Dictionary<int, GoodsItem>();
        }

        private void OnGoodsCmd(byte module, byte cmd, short status, object value)
        {
            if (status != 0)
            {
                GameController.Instance.AlertRespLogicException(module, cmd, (ResponseCode)status);
                return;
            }
            
            GoodsCmd _cmd = (GoodsCmd)cmd;
            switch (_cmd)
            {
                case GoodsCmd.GET_ALL_GOODS:
                    {
                        RefreshAllGoods(value);
                    }
                    break;
                case GoodsCmd.USE_GOODS:
                    {
                        UseGoodsResponse res = (UseGoodsResponse)value;
                        GameController.Instance.PlayerCtr.UpdateByResponse(res.valueResultListResponse);

                        List<GoodsItem> goodsItems = res.goodsItems;
                        foreach (var item in goodsItems)
                        {
                            //if (goodsItemMap.ContainsKey(item.id))
                            //    Q.Warning("GoodsList not foud!!!");

                            if (item.num > 0)
                            {
                                goodsItemMap[item.id] = item;
                            }
                            else
                            {
                                goodsItemMap.Remove(item.id);
                            }
                        }
                        if (gameCtr.ModelEventSystem.OnUseGoodsItem != null)
                        {
                            gameCtr.ModelEventSystem.OnUseGoodsItem(res);
                        }
                    }
                    break;
                case GoodsCmd.BUY_GOODS:
                    {
                        BuyGoodsResponse res = (BuyGoodsResponse)value;
                        //刷新玩家數據
                        gameCtr.PlayerCtr.UpdateByResponse(res.valueResultListResponse);
                        List<GoodsItem> goodsItems = res.goodsItems;
                        //購買的數量 直接傳出去
                        foreach (var item in goodsItems)
                        {
                            int buyNum;
                            GoodsItem goodsItem = item;
                            if (goodsItem.num > 0 && goodsItem.id > 0)
                            {
                                int oldNum = goodsItemMap.ContainsKey(goodsItem.id) ? goodsItemMap[goodsItem.id].num : 0;
                                goodsItemMap[item.id] = goodsItem;
                                buyNum = goodsItem.num - oldNum;
                            }
                            else
                            {
                                //沒有數量 也沒有ID 可能是金幣等物品 直接用 buyShopIdCache 去取配置裡面的購買數量
                                ShopConfig shopConfig = GameController.Instance.Model.ShopConfigs[buyShopIdCache];
                                goodsItem = new GoodsItem(shopConfig.GoodsId, 0);
                                buyNum = GameController.Instance.Model.ShopConfigs[buyShopIdCache].ByNum;
                            }
                            if (gameCtr.ModelEventSystem.OnBuyGoods != null)
                            {
                                gameCtr.ModelEventSystem.OnBuyGoods(buyNum, goodsItem);
                            }
                        }
                        if(gameCtr.ModelEventSystem.OnBuyGoodsList!=null)
                        {
                            gameCtr.ModelEventSystem.OnBuyGoodsList(res);
                        }
                    }
                    break;
                default:
                    break;
            }
        }//OnGoodsCmd

        private void RefreshAllGoods(object value)
        {
            //刷新背表列表
            GoodsListResponse res = (GoodsListResponse)value;
            this.ToItemDic(res.goodsList);
            if (gameCtr.ModelEventSystem.OnGetGoodsList != null)
            {
                gameCtr.ModelEventSystem.OnGetGoodsList(res);
            }
        }

        public void GetAllGoodsList()
        {
            gameCtr.Client.GetAllGoodsList();
        }

        /// <summary>
        /// 使用背包物品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="num"></param>
        public void UseGoods(int id, int num)
        {
            gameCtr.Client.UseGoods(id, num);
        }

        public bool CheckBuyGoodsAble(int shopId)
        {
            ShopConfig config = GameController.Instance.Model.ShopConfigs[shopId];

            if (gameCtr.PlayerCtr.PlayerData.gem >= config.Gem && gameCtr.PlayerCtr.PlayerData.coin >= config.Coin)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 用物品id檢查是否足夠遊戲幣購買物品,用來判定單個物品的購買//
        /// </summary>
        /// <param name="goodsid"></param>
        /// <returns></returns>
        public bool CheckBuyGoodsAbleByGoodsID( int goodsid)
        {
            ShopConfig config = GameController.Instance.Model.SingleGoodsShopConfig[goodsid];

            if (config == null)
                return false;

            if (gameCtr.PlayerCtr.PlayerData.gem >= config.Gem && gameCtr.PlayerCtr.PlayerData.coin >= config.Coin)
            {
                return true;
            }

            return false;
        }

        //購買金幣等物品後，後台返回的ID是0 記錄當時購買的id
        int buyShopIdCache = 0;

        public bool BuyGoods(int shopId)
        {
            if (CheckBuyGoodsAble(shopId))
            {
                gameCtr.Client.BuyGoods(shopId);
                buyShopIdCache = shopId;
                return true;
            }

            return false;
        }

        public void BuyGoodsList(List<int> cfgIds)
        {
            gameCtr.Client.BuyGoodsList(cfgIds);
        }

        /// <summary>
        /// 購買單個物品項
        /// </summary>
        /// <param name="goodsId">物品id</param>
        /// <returns></returns>
        public bool BuySingleGoods(int goodsId)
        {
            Q.Assert(GameController.Instance.Model.SingleGoodsShopConfig.ContainsKey(goodsId) , "购买单个物品错误  无此单个物品项 {0}" , goodsId);

            ShopConfig shopConfig = GameController.Instance.Model.SingleGoodsShopConfig[goodsId];
            return BuyGoods(shopConfig.UID);
        }

        /// <summary>
        /// 通過物品ID獲取在商城裡面的購買項配置
        /// </summary>
        /// <param name="goodsId">物品ID</param>
        /// <returns></returns>
        public ShopConfig GetShopConfigByGoodsId(int goodsId)
        {
            if (!GameController.Instance.Model.SingleGoodsShopConfig.ContainsKey(goodsId))
            {
                return null;
            }

            return GameController.Instance.Model.SingleGoodsShopConfig[goodsId];
        }

        public List<GoodsItem> GoodsItems
        {
            set
            {
                this.ToItemDic(value);
            }
            get
            {
                List<GoodsItem> goodsItemList = new List<GoodsItem>(goodsItemMap.Values);
                return goodsItemList;
            }
        }

        public Dictionary<int, GoodsItem> GoodsItemMap
        {
            get
            {
                return goodsItemMap;
            }
        }

        public int ItemNum
        {
            get
            {
                return goodsItemMap.Count;
            }
        }

        public bool CheckGoodsItemLegal(int uid)
        {
            return goodsItemMap.ContainsKey(uid);
        }

        /// <summary>
        /// 獲取物品的名字 語言表解析
        /// </summary>
        /// <param name="goodsConfig"></param>
        /// <returns></returns>
        public string GetGoodsNameStr(GoodsConfig goodsConfig)
        {
            return Utils.GetTextByID(goodsConfig.GoodsStringId);
        }

        /// <summary>
        /// 獲取物品的描述 通過arg0進行語言表描述
        /// </summary>
        /// <param name="goodsConfig"></param>
        /// <returns></returns>
        public string GetGoodsContentStr(GoodsConfig goodsConfig)
        {
            return Utils.GetTextByID(goodsConfig.GoodsContentId, goodsConfig.Arg0);
        }

        public string GetGoodsContentStr(int id)
        {
            GoodsConfig config = null;

            if (GameController.Instance.Model.GoodsConfigs.ContainsKey(id))
                    config = GameController.Instance.Model.GoodsConfigs[id];

            if (config != null)
                return GetGoodsContentStr(config);

            return null;

        }

        public void AddGoodsItem(List<GoodsItem> items)
        {
            foreach (var item in items)
            {
                this.AddGoodsItem(item);
            }
        }

        public void AddGoodsItem(GoodsItem item)
        {
            if(goodsItemMap.ContainsKey(item.id))
            {
                goodsItemMap[item.id].num += item.num;
            }
            else
            {
                goodsItemMap[item.id] = item;
            }
        }

        /// <summary>
        /// 刷新道具信息
        /// </summary>
        /// <param name="item">刷新的道具</param>
        public void RefreshItem(GoodsItem item)
        {
            if (goodsItemMap.ContainsKey(item.id))
            {
                goodsItemMap[item.id].num = item.num;
            }
            else
            {
                goodsItemMap[item.id] = item;
            }
        }

        private void ToItemDic(List<GoodsItem> itemList)
        {
            goodsItemMap.Clear();
            foreach (var item in itemList)
            {
                goodsItemMap[item.id] = item;
            }
        }
    }
}
