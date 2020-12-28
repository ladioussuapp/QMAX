using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.achievement;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.goods;

public class AchievementSheetData : SheetData
{
    public int Type;
    public string TabIcon;
    public int TabName;

    public List<AchieveItemData> AchieveDataList = new List<AchieveItemData>();
}

public class UIAchievementBehaviour : MonoBehaviour
{

    public Transform Canvas;

    public Image AchievementImg;

    public UISheetGroupBehaviour SheetGroup;

    public UIButtonBehaviour CloseButton;

    public Sprite RefreshImageHL;

    public Sprite RefreshImageDK;

    /// <summary>
    /// 所有分頁集合
    /// </summary>
    private Dictionary<int, UISheetBehaviour> sheets;

    /// <summary>
    /// 所有成就項數據集合
    /// </summary>
    private Dictionary<int, AchieveItemData> achieveItems;

    /// <summary>
    /// 列表內膽面板
    /// </summary>
    private Transform contont;
    private int contontType;

    /// <summary>
    /// 刷新按鈕
    /// </summary>
    private UIButtonBehaviour refresh;

    /// <summary>
    /// 提示(每日任務)
    /// </summary>
    private Text tips;

    /// <summary>
    /// 列表項靜態數量
    /// 指列表項靜態創建的數量，數據量超過該值則由循環列表重複使用列表項
    /// </summary>
    private int listItemStaticCount = 8;

    private bool isInitialShow;

    void Awake()
    {
        isInitialShow = true;
        sheets = new Dictionary<int, UISheetBehaviour>();
        achieveItems = new Dictionary<int, AchieveItemData>();

        // 註冊成就界面用到的圖集
        AtlasManager atlasMgr = GameController.Instance.AtlasManager;
        atlasMgr.AddAtlas(Atlas.UIAchievement, AchievementImg.sprite.texture);
        SheetGroup.OnChange += OnSheetChange;
        CloseButton.onClick += OnClose;
        // 接收數據回調
        GameController.Instance.ModelEventSystem.OnAchieveOpen += OnOpenResponse;
        GameController.Instance.ModelEventSystem.OnAchieveReward += OnRewardResponse;
        GameController.Instance.ModelEventSystem.OnAchieveRefresh += OnRefreshResponse;
    }

    public void AddSheet(AchievementSheetData data, bool isSelect = false)
    {
        if (sheets.ContainsKey(data.Type))
            return;

        GameObject tab_prefab = Resources.Load<GameObject>("Prefabs/Ui/UIAchieve/UIAchieveTab");
        GameObject ga = Instantiate<GameObject>(tab_prefab);

        UISheetBehaviour sheet = ga.GetComponent<UISheetBehaviour>();
        if (sheet == null)
            return;

        Transform tf = ga.transform.Find("ButtonOK");
        Transform icon = tf.Find("Icon");
        Text name = tf.Find("CNText_Purple").GetComponent<Text>();

        AtlasManager atlasMgr = GameController.Instance.AtlasManager;
        Sprite icon_sprite = atlasMgr.GetSprite(Atlas.UIAchievement, data.TabIcon);
        if (icon_sprite != null)
        {
            Image img = icon.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = icon_sprite;
                img.SetNativeSize();
            }
        }
        if (name != null)
        {
            name.text = Utils.GetTextByID(data.TabName);
        }
        SheetGroup.Add(sheet);
        sheet.Data = data;
        sheet.IsSelect = isSelect;
        sheets.Add(data.Type, sheet);
        //-----------------------------------修正ui位置12/25
        sheet.transform.localPosition = new Vector3(sheet.transform.localPosition.x, sheet.transform.localPosition.y,0);
        // 更新提示數字
        UpdateTipNum(sheet);
    }

    public void AddItems(Transform grid, List<AchieveItemData> items)
    {
        int unReceiveCount = 0;
        TegraMask tMask = grid.GetComponentInParent<TegraMask>();
        for (int i = 0; i < items.Count && i < listItemStaticCount; i++)
        {
            AchieveItemData achieve = items[i];
            if (achieve == null)
                continue;

            GameObject item_prefab = Resources.Load<GameObject>("Prefabs/Ui/UIAchieve/UIAchieveItem");
            GameObject item = Instantiate<GameObject>(item_prefab);

            AchieveItemBehaviour itemBeh = item.GetComponent<AchieveItemBehaviour>();
            if (itemBeh != null)
            {
                itemBeh.Data = achieve;

                if (itemBeh.Data.Status == 1)
                {
                    unReceiveCount++;
                }
                //兼容方案的遮罩
                if (tMask != null)
                {
                    tMask.RegisteredGraphics(itemBeh.transform);
                }
            }

            item.transform.SetParent(grid);
            item.transform.localPosition = new Vector3(0, 0, 0);
            item.transform.localScale = new Vector3(1, 1, 1);
        }
        if (refresh != null)
        {
            Transform btntf = refresh.transform.Find("ButtonOK");
            Transform imgtf = btntf.Find("Image");
            Image img = imgtf.GetComponent<Image>();
            if (img == null)
                return;

            if (unReceiveCount == 0)
            {
                refresh.interactable = false;
                img.sprite = RefreshImageDK;
                img.SetNativeSize();
            }
            else
            {
                refresh.interactable = true;
                img.sprite = RefreshImageHL;
                img.SetNativeSize();
            }
        }
    }

    private void UpdateTipNum(UISheetBehaviour sheet)
    {
        AchievementSheetData data = sheet.Data as AchievementSheetData;
        if (data == null)
            return;

        // 數字提示已達成的數量
        int reachCount = 0;
        foreach (AchieveItemData aid in data.AchieveDataList)
        {
            if (aid.Status == 2)
                reachCount++;
        }
        Transform nt = sheet.transform.Find("NumTip");
        if (reachCount > 0)
        {
            nt.gameObject.SetActive(true);
            Text txt = nt.Find("EText").GetComponent<Text>();
            if (reachCount > 99)
                txt.text = "N";
            else
                txt.text = reachCount.ToString();
        }
        else
        {
            nt.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 成就類型切換
    /// </summary>
    /// <param name="sheet"></param>
    private void OnSheetChange(UISheetBehaviour sheet)
    {
        AchievementSheetData adata = sheet.Data as AchievementSheetData;
        if (adata == null)
            return;

        if (isInitialShow)
        {
            isInitialShow = false;
            SetListData(adata);
        }
        else if (contontType != adata.Type)
        {
            GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.UPGRAD_SCENE_PAGEBUTTON_CLICK);
            GameController.Instance.Client.OpenAchieveData((byte)adata.Type);
        }
    }


    private void SetListData(AchievementSheetData adata)
    {
        if (adata == null)
            return;

        if (contont == null || contontType != adata.Type)
        {
            if (contont != null)
            {
                GameObject tmp = contont.gameObject;
                Transform tmp_scrol = tmp.transform.Find("ScrollRect").transform;
                Transform tmp_grid = tmp_scrol.Find("Grid").transform;
                LeanTween.moveLocal(tmp_grid.gameObject, new Vector3(-1000, tmp_grid.localPosition.y, 0), 0.2f).setOnComplete(delegate()
                {
                    if (tmp != null)
                    {
                        CycleListBehaviour cycleList = tmp.GetComponentInChildren<CycleListBehaviour>();
                        if (cycleList != null)
                            cycleList.OnItemReset -= OnListItemReset;

                        Destroy(tmp);
                    }
                });
            }

            if (adata.Type == (byte)AchievementType.DALIY)
            {
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Ui/UIAchieve/ContontA");
                contont = Instantiate<GameObject>(prefab).transform;

                Transform tipTransform = contont.Find("Tips");
                Transform refTransform = contont.Find("Refresh");
                if (tipTransform != null)
                {
                    tipTransform.gameObject.SetActive(false);
                    tips = tipTransform.GetComponent<Text>();
                    if (tips != null)
                    {
                        tips.text = Utils.GetTextByID(14421);
                    }
                }
                if (refTransform != null)
                {
                    refTransform.gameObject.SetActive(false);
                    refresh = refTransform.GetComponent<UIButtonBehaviour>();
                    refresh.onClick += OnRefresh;
                }
            }
            else
            {
                if (tips != null)
                {
                    tips.gameObject.SetActive(true);
                    tips = null;
                }
                if (refresh != null)
                {
                    refresh.gameObject.SetActive(true);
                    refresh = null;
                }
                GameObject prefab = Resources.Load<GameObject>("Prefabs/Ui/UIAchieve/ContontB");
                contont = Instantiate<GameObject>(prefab).transform;
            }
            CycleListBehaviour cycle = contont.GetComponentInChildren<CycleListBehaviour>();
            if (cycle != null)
            {
                int dataCount = adata.AchieveDataList.Count;
                if (dataCount > listItemStaticCount)
                {
                    cycle.setItemCount(dataCount);
                }
                cycle.OnItemReset += OnListItemReset;
            }

            contontType = adata.Type;
            contont.SetParent(Canvas);
            contont.localPosition = new Vector3(-2, -103, 0);
            contont.localScale = new Vector3(1, 1, 1);

            Transform scrol = contont.Find("ScrollRect").transform;
            Transform grid = scrol.Find("Grid").transform;
            grid.gameObject.SetActive(false);
            // 因為初始位置不對，延遲一幀
            StartCoroutine(Utils.DelayNextFrameCall(delegate()
            {
                grid.gameObject.SetActive(true);

                Vector3 gridPos = grid.localPosition;
                grid.localPosition = new Vector3(gridPos.x + 1000, gridPos.y, 0);
                LeanTween.moveLocal(grid.gameObject, new Vector3(gridPos.x, gridPos.y, 0), 0.2f)
                    .setDelay(0.1f)
                    .setOnComplete(delegate()
                    {
                        if (tips != null)
                            tips.gameObject.SetActive(true);

                        if (refresh != null)
                            refresh.gameObject.SetActive(true);
                    });
            }));
            adata.AchieveDataList.Sort();
            AddItems(grid, adata.AchieveDataList);
        }
    }

    private void OnClose(UIButtonBehaviour button)
    {
        GameController.Instance.Client.GetReachAchieveCount();
        GameController.Instance.Popup.Close(PopupID.UIAchievement);
    }

    /// <summary>
    /// 打開介面響應數據
    /// </summary>
    /// <param name="res"></param>
    private void OnOpenResponse(AchievementListResponse res)
    {
        Action OnMainThread = delegate()
        {
            int count = res.achievements.Count;
            if (count > 1)
            {
                for (int i = 0; i < count; i++)
                {
                    AchievementTabVO tabVO = res.achievements[i];
                    AchievementSheetData sheet = new AchievementSheetData();
                    sheet.Type = tabVO.achieveType;
                    sheet.TabIcon = tabVO.icon;
                    sheet.TabName = tabVO.name;
                    ReadAchieveItems(tabVO, sheet.AchieveDataList);
                    // 添加到Sheet
                    AddSheet(sheet, i == 0);
                }
            }
            else if (count == 1)
            {
                AchievementTabVO tabVO = res.achievements[0];
                AchievementSheetData sheet = new AchievementSheetData();
                sheet.Type = tabVO.achieveType;
                sheet.TabIcon = tabVO.icon;
                sheet.TabName = tabVO.name;
                ReadAchieveItems(tabVO, sheet.AchieveDataList);
                SetListData(sheet);
                // 更新UISheetBehaviour數據
                UISheetBehaviour sheetBeh = SheetGroup.GetSelected();
                sheetBeh.Data = sheet;
            }

            TegraMask tMask = SheetGroup.GetComponentInParent<TegraMask>();
            if (tMask != null)
            {
                tMask.SetMaterialDirty();
            }
        };
        GameController.Instance.InvokeOnMainThread(OnMainThread);
    }

    /// <summary>
    /// 領取獎勵響應數據
    /// </summary>
    /// <param name="res"></param>
    private void OnRewardResponse(AchievementRewardResponse res)
    {
        Action OnMainThread = delegate()
        {
            int achieveId = res.achievementId;
            if (!achieveItems.ContainsKey(achieveId))
                return;

            AchieveItemBehaviour achieveItemBeh = achieveItems[achieveId].Beh;
            if (achieveItemBeh == null)
                return;

            // 已經領取的狀態
            achieveItemBeh.Data.Status = 3;
            achieveItemBeh.SetStatus(achieveItemBeh.Data.Status);

            Transform tf = achieveItemBeh.gameObject.transform.Find("IconImage");
            if (tf == null)
                return;

            res.list.Sort(GameController.Instance.PlayerCtr.ValueResultSort);
            GameController.Instance.PlayerCtr.UpdateByResponse(res.list);
            if (res.goodsItems.Count > 0)
            {
                GameController.Instance.GoodsCtr.AddGoodsItem(res.goodsItems);
            }

            // 獎勵彈窗
            // res.list過濾掉道具
            List<ValueResult> tmpList = new List<ValueResult>();
            foreach (ValueResult vr in res.list)
            {
                if (vr.valuesType == 0)
                    continue;

                tmpList.Add(vr);
            }
            // 獎勵多少種東西
            int count = tmpList.Count + res.goodsItems.Count;
            List<Goods> goods = new List<Goods>();
            string[] reward = new string[count];
            for (int i = 0; i < tmpList.Count; i++)
            {
                ValueResult v = tmpList[i];
                reward[i] = v.changeValue.ToString();

                Goods g = new Goods();
                g.Num = reward[i];
                g.GoodsSprite = GameController.Instance.AtlasManager.GetSpriteInUIComponent((RewardType)v.valuesType);
                g.GoodsSpriteSize = new Vector2(120f,120f);
                goods.Add(g);
            }

            for (int i = tmpList.Count; i < count; i++)
            {
                GoodsItem v = res.goodsItems[i - tmpList.Count];
                reward[i] = v.num.ToString();

                Goods g = new Goods();
                g.Num = reward[i];
                g.GoodsSpriteSize = new Vector2(120f, 120f);

                GoodsConfig goodsConfig = GameController.Instance.Model.GoodsConfigs[v.id];
                if (goodsConfig != null)
                    g.GoodsSprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, goodsConfig.GoodsIcon);

                goods.Add(g);
            }

            UICommonDialogBehaviour uiCommon = GameController.Instance.Popup.Open(PopupID.UICommonDialog, null, true, true).GetComponent<UICommonDialogBehaviour>();
            uiCommon.CloseEvent = delegate ()
            {
                GameController.Instance.AudioManager.PlayAudio("SD_ui_7day_close");
            };
            string title = Utils.GetTextByID(achieveItemBeh.Data.NameId);
            string info = Utils.GetTextByID(achieveItemBeh.Data.ContentId, reward);
            uiCommon.SetInfo(title, info, goods);

            ///播放领取成功音效
            GameController.Instance.AudioManager.PlayAudio("Vo_achieve_receive");
            // 更新提示數字
            UpdateTipNum(SheetGroup.GetSelected());
        };
        GameController.Instance.InvokeOnMainThread(OnMainThread);
    }

    /// <summary>
    /// 刷新成就列表響應數據
    /// </summary>
    /// <param name="res"></param>
    private void OnRefreshResponse(AchievementListResponse res)
    {
        Action OnMainThread = delegate()
        {
            if (res.achievements.Count <= 0)
                return;

            // 不是每日任務的面板
            if (contontType != (byte)AchievementType.DALIY)
                return;

            Transform scrol = contont.Find("ScrollRect").transform;
            Transform grid = scrol.Find("Grid").transform;
            for (int i = 0; i < grid.childCount; i++)
            {
                GameObject go = grid.GetChild(i).gameObject;
                Destroy(go);
            }

            //刷新後的成就數據
            AchievementTabVO tabVO = res.achievements[0];
            List<AchieveItemData> items = new List<AchieveItemData>();
            ReadAchieveItems(tabVO, items);
            AddItems(grid, items);

            GameController.Instance.PlayerCtr.UpdateByResponse(res.valueResultListResponse);
        };
        GameController.Instance.InvokeOnMainThread(OnMainThread);
    }

    /// <summary>
    /// 讀取成就項
    /// </summary>
    /// <param name="tabVO"></param>
    /// <param name="results"></param>
    private void ReadAchieveItems(AchievementTabVO tabVO, List<AchieveItemData> results)
    {
        QmaxModel model = GameController.Instance.Model;
        for (int i = 0; i < tabVO.achieveList.Count; i++)
        {
            AchievementVO achievementVO = tabVO.achieveList[i];
            if (!model.AchieveConfigs.ContainsKey(achievementVO.achievementId))
                continue;

            AchieveItemData item = new AchieveItemData();
            item.AchieveId = achievementVO.achievementId;
            item.Progress = achievementVO.accumulateValue;
            item.Status = achievementVO.state;
            // 取配置數據
            AchieveConfig config = model.AchieveConfigs[item.AchieveId];
            item.AchieveDesc = Utils.GetTextByID(config.AchieveStringId);
            item.NameId = config.NameId;
            item.ContentId = config.ContentId;
            item.RewardIcon = config.RewardIcon;
            item.RewardCount = config.RewardCount;
            item.IconScale = config.IconScale;
            item.Target = config.Target;

            // 把數據加入字典中
            if (achieveItems.ContainsKey(item.AchieveId))
                achieveItems.Remove(item.AchieveId);
            achieveItems.Add(item.AchieveId, item);

            results.Add(item);
        }
        results.Sort();
    }

    private void OnRefresh(UIButtonBehaviour button)
    {
        string title = Utils.GetTextByID(1520, GameController.Instance.Model.GameSystemConfig.AchieveRefreshGem);
        UIAlertBehaviour alert = UIAlertBehaviour.Alert(title, "AchievementAlertContent", "", 2, 2, 0, (byte)UIAlertBehaviour.ButtonStates.ButtonOk | (byte)UIAlertBehaviour.ButtonStates.ButtonCancel);
        alert.OnClickOKButton += delegate(UIButtonBehaviour obj)
        {
            // 請求刷新列表
            GameController.Instance.Client.RefreshAchieve();
        };
    }

    private void OnListItemReset(Transform transform, int index)
    {
        AchieveItemBehaviour item = transform.GetComponent<AchieveItemBehaviour>();
        if (item == null)
            return;

        UISheetBehaviour sheetBeh = SheetGroup.GetSelected();
        AchievementSheetData adata = sheetBeh.Data as AchievementSheetData;
        if (adata == null)
            return;

        if (index < 0 || index >= adata.AchieveDataList.Count)
        {
            return;
        }
        // 設置新數據
        AchieveItemData itemData = adata.AchieveDataList[index];
        item.Data = itemData;
    }

    public void OnDestroy()
    {
        // 移除消息回調
        GameController.Instance.ModelEventSystem.OnAchieveOpen -= OnOpenResponse;
        GameController.Instance.ModelEventSystem.OnAchieveReward -= OnRewardResponse;
        GameController.Instance.ModelEventSystem.OnAchieveRefresh -= OnRefreshResponse;

        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIAchievement);
        SheetGroup.OnChange -= OnSheetChange;
        CloseButton.onClick -= OnClose;

        if (refresh != null)
        {
            refresh.onClick -= OnRefresh;
        }

        if (contont != null)
        {
            CycleListBehaviour cycle = contont.GetComponentInChildren<CycleListBehaviour>();
            if (cycle != null)
            {
                cycle.OnItemReset -= OnListItemReset;
            }
        }
    }

}
