using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Net.Protocols.getchance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com4Love.Qmax.Ctr;
using System;
 
using Com4Love.Qmax.Net.Protocols.goods;

public class UIGetChanceBehaviour : MonoBehaviour
{

    public const string BOMB_EFFECT_SPAWN_KEY = "Prefabs/Effects/EffectFudaibaozha";
    public const string GIFT_UI_SPAWN_KEY = "Prefabs/Ui/UIGetchance/Gift";
    public const string UNITGIFT_UI_SPAWN_KEY = "Prefabs/Ui/UIGetchance/Gift_Unit";


    public RectTransform CutBoard;
    public RectTransform NextBoard;
    public Image CutImg;


    public Transform EffectRoot;
    public RectTransform CutButtonBg;
    public Button KeyButton;
    public Text KeyNumText;
    public Transform BackButton;
    public TopInfoView TopInfoView;
    public Image AtlasImg;

    public Animator animator;
    //專門控制抽獎後數據刷新的行為類
    public UIGetChanceAnimationBehaviour animationBehaviour;

    bool effectWaitting = false;
    bool boardWaitting = false;

    Action closeCommonWindowCallback;
    string cutImgSpawnKey;
    private GameController gc;
    //UIGetChanceBoxItem[] items;
    UIGetChanceBoxItem[] cutItems;
    UIGetChanceBoxItem[] nextItems;
    //當前面闆對應福袋的狀態 1表示可打開
    int[] BoardBoxStates = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    ActionData cutAction;
 
    public struct ActionData
    {
        public UIGetChanceBoxItem BoxItem;
        public GetChanceConfig GConfig;

        /// <summary>
        /// 物品以外的材料等 有可能是空 盡量使用GConfig裡面的東西判斷
        /// </summary>
        public ValueResult VR;
        /// <summary>
        /// 物品信息
        /// </summary>
        public GoodsItem GI;
        public bool NeedTip;
    }

    void Start()
    {
        //除開戰斗場景 所有的ui攝像機的size都是5 如果是被戰斗場景打開，可能需要將場景的主攝像機的size調到5
        Camera.main.orthographicSize = 5;
        NextBoard.gameObject.SetActive(false);
        CutButtonBg.gameObject.SetActive(false);
        KeyNumText.transform.parent.gameObject.SetActive(false);        //整個圈圈圖片隱藏 keyNum的層級不要更改
        //BackButton.gameObject.SetActive(false);

        gc = GameController.Instance;
        BaseStateMachineBehaviour stateMachines = animator.GetBehaviour<BaseStateMachineBehaviour>();
        stateMachines.StateExitEvent += stateMachines_StateExitEvent;
        gc.AtlasManager.AddAtlas(Atlas.UIGetChance, AtlasImg.sprite.texture);

        //緩存後面會用到的特效
        cutImgSpawnKey = string.Concat("UiGetChanceBehaviour cutimg", Time.realtimeSinceStartup);
        gc.PoolManager.PrePrefabSpawn(BOMB_EFFECT_SPAWN_KEY, 5);
        gc.PoolManager.PrePrefabSpawn(GIFT_UI_SPAWN_KEY,  5);
        gc.PoolManager.PrePrefabSpawn(UNITGIFT_UI_SPAWN_KEY,  5);
        gc.PoolManager.PrePrefabSpawn(CutImg.transform, cutImgSpawnKey, 2);

#if AUTO_FIGHT
        //test  延遲調用方法跳轉場景
        Invoke("OnBackClick", 2f);
#endif
    }

    private void Popup_OnOpen(PopupID obj)
    {
        if ((obj == PopupID.UICommonDialog || obj == PopupID.UIGetChanceUnitWin))
        {
            effectWaitting = true;                       
            UpdateItemEffectWaitting(true);
        }
    }

    private void Popup_OnCloseComplete(PopupID obj)
    {
        if ((obj == PopupID.UICommonDialog || obj == PopupID.UIGetChanceUnitWin) 
            && !GameController.Instance.Popup.IsPopup(PopupID.UICommonDialog, PopupID.UIGetChanceUnitWin))
        {
            effectWaitting = false;             //在item點擊後就被賦值為true
            UpdateItemEffectWaitting(false);

            if (obj == PopupID.UICommonDialog && closeCommonWindowCallback != null)
            {
                closeCommonWindowCallback();
                closeCommonWindowCallback = null;
            }
        }
    }

    void Init()
    {
        animationBehaviour.init(GameController.Instance.PlayerCtr.PlayerData); 
        cutAction = new ActionData();
        cutAction.BoxItem = null;

        cutItems = CutBoard.GetComponentsInChildren<UIGetChanceBoxItem>(true); 
        nextItems = NextBoard.GetComponentsInChildren<UIGetChanceBoxItem>(true);

        AddItemEvents();

        KeyButton.onClick.AddListener(OnKeyButtonClick);
        gc.Popup.OnCloseComplete += Popup_OnCloseComplete;
        gc.Popup.OnOpen += Popup_OnOpen;
        gc.ModelEventSystem.OnOpenBox += ModelEventSystem_OnOpenBox;
        gc.ModelEventSystem.OnBuyKeys += ModelEventSystem_OnBuyValue;

        //要求進場之後不抖動
        //RefreashItems();
        DisplayButton();

        addFliter();

        if (GuideManager.getInstance().CurrentGuideID() == 2 && 
            !GuideManager.getInstance().IsGuideRunning &&
            gc.PlayerCtr.PlayerData.key > 0)
        {
            List<Com4Love.Qmax.Net.Protocols.Unit.Unit> unitlist = GameController.Instance.PlayerCtr.PlayerData.list;

            bool isfind = false;

            foreach (var unit in unitlist)
            {
                ///已經擁有猴子
                if (unit.unitId == 5101)
                {
                    isfind = true;
                    break;
                }
            }

            ///有猴子跳過這步教學
            if (isfind)
                GuideManager.getInstance().SaveAndGotoNext();
            else
                GuideManager.getInstance().StartGuide(1);
        }

        //switch (GuideManager.getInstance().version)
        //{
        //    case GuideVersion.Version_1:
        //        if (GuideManager.getInstance().guideIndex == 3 && !GuideManager.getInstance().IsGuideRunning)
        //        {
        //            GuideManager.getInstance().StartGuide(1);
        //        }
        //        break;
        //    default:
        //        break;
        //}
    }

    void OnDestroy()
    {
        RemoveItemEvents();
        removeFliter();
        gc.Popup.OnCloseComplete -= Popup_OnCloseComplete;
        //gc.Popup.OnOpenComplete -= Popup_OnOpenComplete;
        gc.ModelEventSystem.OnOpenBox -= ModelEventSystem_OnOpenBox;
        gc.ModelEventSystem.OnPlayerInfoInit -= ModelEventSystem_OnPlayerInfoInit;
        gc.ModelEventSystem.OnPlayerInfoRef -= ModelEventSystem_OnPlayerInfoRef;
        gc.ModelEventSystem.OnBuyKeys -= ModelEventSystem_OnBuyValue;

        gc.AtlasManager.UnloadAtlas(Atlas.UIGetChance);

        gc.PoolManager.Despool(UNITGIFT_UI_SPAWN_KEY);
        gc.PoolManager.Despool(GIFT_UI_SPAWN_KEY);
        gc.PoolManager.Despool(BOMB_EFFECT_SPAWN_KEY);
        gc.PoolManager.Despool(cutImgSpawnKey);
    }
     
    void stateMachines_StateExitEvent(Animator arg1, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("Start"))
        {
            //進場動畫完成
            if (gc.PlayerCtr == null)
            {
                return;
            }

            if (gc.PlayerCtr.IsDataReady)
            {
                Init();
            }
            else
            {
                gc.ModelEventSystem.OnPlayerInfoInit += ModelEventSystem_OnPlayerInfoInit;
            }

            gc.ModelEventSystem.OnPlayerInfoRef += ModelEventSystem_OnPlayerInfoRef;
        }
    }

    void ModelEventSystem_OnPlayerInfoRef(List<RewardType> arg1, Com4Love.Qmax.Net.Protocols.ActorGame.ActorGameResponse arg2)
    {
        DisplayButton();
    }

    void ModelEventSystem_OnPlayerInfoInit(Com4Love.Qmax.Net.Protocols.ActorGame.ActorGameResponse obj)
    {
        Init();
    }

    void ModelEventSystem_OnBuyValue(ValueResultListResponse obj)
    {
        foreach (ValueResult vr in obj.list)
        {
            if (vr.changeType == 1 && vr.valuesType == 1)
            {
                //購買鑰匙 鑰匙有變化 鑽石有變化 需要手動刷新infoview
                animator.SetTrigger("ButtonFlash2Trigger");
            }
        }
    }

    void OnKeyButtonClick()
    {
        if (GameController.Instance.PlayerCtr.PlayerData.gem < GameController.Instance.Model.GameSystemConfig.BuyKey.Price)
        {
            //鑽石不夠
            UIShop.Open(UIShop.TapIndex.GEM_INDEX);
        }
        else
        {
            gc.PlayerCtr.BuyKey();
        }
    }

    void RefreashItems()
    {
        foreach (UIGetChanceBoxItem item in cutItems)
        {
            item.PlayRefresh();
        }
    }

    void RemoveItemEvents()
    {
        if (cutItems == null)
        {
            return;
        }

        UIGetChanceBoxItem[] items = new UIGetChanceBoxItem[cutItems.Length + nextItems.Length];
        cutItems.CopyTo(items, 0);
        nextItems.CopyTo(items, cutItems.Length);
 
        foreach (UIGetChanceBoxItem item in items)
        {
            if (item != null)
            {
                item.OnOpenBox -= item_OnOpenBox;
                item.OnBoxTip -= item_OnBoxTip;
                item.OnFlyEffectComplete -= item_OnFlyEffectComplete;
            }
        }
    }
    
    void AddItemEvents()
    {
        UIGetChanceBoxItem[] items = new UIGetChanceBoxItem[cutItems.Length + nextItems.Length];
        cutItems.CopyTo(items, 0);
        nextItems.CopyTo(items, cutItems.Length);

        foreach (UIGetChanceBoxItem item in items)
        {
            item.OnOpenBox += item_OnOpenBox;
            item.OnBoxTip += item_OnBoxTip;
            item.OnFlyEffectComplete += item_OnFlyEffectComplete;
        }
    }

    void UpdateItemEffectWaitting(bool waitting)
    {
        foreach (UIGetChanceBoxItem item in cutItems)
        {
            item.effectWaitting = waitting;
        }

        foreach (UIGetChanceBoxItem item in nextItems)
        {
            item.effectWaitting = waitting;
        }
    }

    void item_OnFlyEffectComplete(UIGetChanceBoxItem.Data obj)
    {
        //禮品飛行效果完成
        RewardType vt = (RewardType)obj.GConfig.GiftType;
        int changeVal = obj.vr == null ? obj.GI.num : obj.vr.changeValue;

        switch (vt)
        {
            case RewardType.UpgradeA:
                animationBehaviour.Upgrow(changeVal, 0, 0);
                animator.SetTrigger("IconFlash1Trigger");
                break;
            case RewardType.UpgradeB:
                animationBehaviour.Upgrow(0, changeVal, 0);
                animator.SetTrigger("IconFlash2Trigger");
                break;
            case RewardType.Gem:
                animationBehaviour.Upgrow(0, 0, changeVal);
                animator.SetTrigger("IconFlash3Trigger");
                break;
            case RewardType.Energy:
            case RewardType.MaxEnergy:
                animator.SetTrigger("IconFlash4Trigger");
                break;
            case RewardType.Good:
                //物品  暫時不處理
                break;
            case RewardType.Coin:
                animationBehaviour.Upgrow(0, 0, 0, changeVal);
                animator.SetTrigger("IconFlash5Trigger");
                break;
            default:
                break;
        }
    }

    void item_OnBoxTip(bool isUnit, UIGetChanceBoxItem item)
    {
        if (isUnit)
        {
            if (GameController.Instance.Popup.IsPopup(PopupID.UICommonDialog))
            {
                //有提示窗口在， 等待提示窗口關閉後再彈出夥伴
                closeCommonWindowCallback = UnitTip;
            }
            else
            {
                //夥伴彈出提示
                UnitTip();
            }
        }
        else
        {
            //得到普通獎勵
            //有可能需要彈出提示窗口
            CommonTip(item.GetData());
        }
    }

    void UnitTip()
    {
        UIGetChanceUnitWin win = gc.Popup.Open(PopupID.UIGetChanceUnitWin, null, true, true).GetComponent<UIGetChanceUnitWin>();
        UIGetChanceUnitWin.Data data = new UIGetChanceUnitWin.Data();

        if (cutAction.VR.valuesType != (int)RewardType.Unit)
        {
            return;
        }

        UnitConfig uConfig = gc.Model.UnitConfigs[cutAction.VR.changeValue];
        SkillConfig sConfig = gc.Model.SkillConfigs[uConfig.UnitSkillId];
        data.sConfig = sConfig;
        data.uConfig = uConfig;

        win.SetData(data);
        win.OnCloseCallback += delegate (bool isPlay)
        {
            StartCoroutine(playKeyRunOut(isPlay));
        };
    }

    void CommonTip(UIGetChanceBoxItem.Data getData)
    {
        GetChanceConfig gConfig = getData.GConfig;

        ///如果彈窗信息不為空///
        ///每種物品只提醒一次//
        ///現在是根據GiftType判定沒個id都算一個物品//
        if (gConfig.GiftType != (int)RewardType.Unit)
        {
            string key = GetTipDontShowKey(gConfig.GiftType, getData.GI);

            //邏輯待整理
            //if (gConfig.GiftType == (int)RewardType.UpgradeA)
            //{
            //    //如果是第一次抽到橘子則需要播放特定聲音
            //    PlayFirstUpgradeAAudio();
            //}

            if (!PlayerPrefsTools.HasKey(key, true))
            {
                string title = Utils.GetTextByID(gConfig.title);
                string info = Utils.GetTextByID(gConfig.info);
                List<Goods> datas = new List<Goods>();
                Goods data = new Goods();

                data.GoodsSprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIGetChance, gConfig.Icon);
                data.GoodsSpriteSize = new Vector2(120f, 120f);
                ///因為獎勵只有一個，所以直接去獲獎列表中的值//
                data.Num = getData.vr == null ? getData.GI.num.ToString() : getData.vr.changeValue.ToString();
                datas.Add(data);

                info = string.Format(info, data.Num);
                UICommonDialogBehaviour uiCommon = GameController.Instance.Popup.Open(PopupID.UICommonDialog, null, true, true).GetComponent<UICommonDialogBehaviour>();
                uiCommon.SetInfo(title, info, datas);
                PlayerPrefsTools.SetIntValue(key, 1, true);
            }
        }
    }

    UIGetChanceBoxItem GetItem(int index)
    {
        return cutItems[index];
    }

    void item_OnOpenBox(int index)
    {
        if (!CheckBoxExist(index))
        {
            return;
        }
 
        //正在等待
        if (effectWaitting)
        {
            return;
        }
    
        if (gc.PlayerCtr.PlayerData.key == 0)
        {
            //Q.Log("box index:" + index);
            //item.PlayClick();
            animator.SetTrigger("ButtonFlash1Trigger");
            return;
        }

        effectWaitting = true;

        BoardBoxStates[index] = 0;
        cutAction = new ActionData();
        cutAction.BoxItem = GetItem(index);

        gc.PlayerCtr.OpenBox();
    }

    //某位置是否有箱子
    bool CheckBoxExist(int index)
    {
        return BoardBoxStates[index] == 1;
    }

    string GetTipDontShowKey(int giftType, GoodsItem item)
    {
        string key;

        if (giftType == (int)RewardType.Good)
        {
            //物品   需要每種ID都提示一次  key 为 TipDontShowOnGetChance(0)(getData.GI.id)
            key = string.Concat(OnOff.TipDontShowOnGetChance, giftType, item.id);
        }
        else
        {
            //普通材料 
            key = string.Concat(OnOff.TipDontShowOnGetChance, giftType);
        }

        //#if UNITY_EDITOR
        //        Debug.Log("抽獎測試 換KEY");
        //        key += "aaaaaaaaa";
        //#endif

        return key;
    }

    void ModelEventSystem_OnOpenBox(GetChanceResponse res)
    {
        Q.Assert(gc.Model.GetChanceConfigs.ContainsKey(res.id), "GetChanceResponse 配置无此ID:{0}", res.id);

        DisplayButton();
        GetChanceConfig gConfig = gc.Model.GetChanceConfigs[res.id];
        cutAction.GConfig = gConfig;
        cutAction.NeedTip = false;      //默認不要提示

        if (res.valueGoodsItems.Count > 0)
        {
            //抽到物品   只能抽到一個物品
            cutAction.GI = res.valueGoodsItems[0];
        }
        else if(res.valueResultListResponse.list.Count > 1)
        {
            List<ValueResult> list = res.valueResultListResponse.list;

            //抽獎只能抽到一個材料   
            for (int i = 0; i < list.Count; i ++)
            {
                if (list[i].changeType == 1)
                {
                    //抽獎默認只能抽到一種東西
                    cutAction.VR = list[i];
                    break;
                }
            }
        }

        Q.Assert(res.valueGoodsItems.Count > 0 || res.valueResultListResponse.list.Count > 1, "抽獎沒有抽到任何東西 getchanceId:" + res.id);

        if (cutAction.GConfig.GiftType != (int)RewardType.Unit)
        {
            //如果抽到夥伴 保持 effectWaitting 不變， 否則將 effectWaitting 設置為 false
            effectWaitting = false;

            string key = GetTipDontShowKey(gConfig.GiftType, cutAction.GI);

            if (!PlayerPrefsTools.HasKey(key, true))
            {
                //需要提示 （非強制性）
                cutAction.NeedTip = true;
            }
        }
        else
        {
            effectWaitting = true;
            cutAction.NeedTip = true;           //抽到夥伴的邏輯是強制tip 此變量其實無用
        }

        StartCoroutine(BoxAndKeyEffect(cutAction));
    }

    /// <summary>
    /// 第一次抽到橘子時 播放指定的台詞音效
    /// </summary>
    void PlayFirstUpgradeAAudio()
    {
        if (!PlayerPrefsTools.HasKey(OnOff.FirstTimeGitOrange, true))
        {
            PlayerPrefsTools.SetIntValue(OnOff.FirstTimeGitOrange, 1, true);
            GameController.Instance.AudioManager.PlayAudio("Vo_accompany_5");
        }
    }

    void DisplayButton()
    {
        KeyNumText.text = gc.PlayerCtr.PlayerData.key.ToString();

        if (gc.PlayerCtr.PlayerData.key == 0)
        {
            CutButtonBg.gameObject.SetActive(true);
            KeyNumText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            CutButtonBg.gameObject.SetActive(false);
            KeyNumText.transform.parent.gameObject.SetActive(true);
        }
    }

    IEnumerator NextBoardEffect()
    {
        boardWaitting = true;
        NextBoard.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);    
 
        while (effectWaitting)
        {
            yield return 0;
        }
 
        LeanTween.value(CutBoard.gameObject, CutBoard.anchoredPosition.x, 1080f, .8f).setEase(LeanTweenType.easeInOutQuart).setOnUpdate(delegate (float val)
        {
            CutBoard.anchoredPosition = new Vector2(val, CutBoard.anchoredPosition.y);
        });

        LeanTween.value(NextBoard.gameObject, NextBoard.anchoredPosition.x, 0f, .8f).setEase(LeanTweenType.easeInOutQuart).setOnUpdate(delegate (float val)
        {
            NextBoard.anchoredPosition = new Vector2(val, NextBoard.anchoredPosition.y);
        });

        while (LeanTween.isTweening(NextBoard.gameObject) || LeanTween.isTweening(CutBoard.gameObject))
        {
            yield return 0;
        }

        foreach (UIGetChanceBoxItem item in cutItems)
        {
            item.Reset();
        }

        RectTransform tmp = CutBoard;
        CutBoard = NextBoard;
        NextBoard = tmp;

        UIGetChanceBoxItem[] tmpItems = cutItems;
        cutItems = nextItems;
        nextItems = tmpItems;

        NextBoard.anchoredPosition = new Vector2(-1080f, NextBoard.anchoredPosition.y);
        CutBoard.anchoredPosition = new Vector2(0f, CutBoard.anchoredPosition.y);
        BoardBoxStates = new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        RefreashItems();

        NextBoard.gameObject.SetActive(false);
        effectWaitting = false;
        boardWaitting = false;
    }
    IEnumerator BoxAndKeyEffect(ActionData action)
    {
        yield return StartCoroutine(CutEffect(action));

        StartCoroutine(BoxEffect(action));
    }

    IEnumerator BoxEffect(ActionData action)
    {
        UIGetChanceBoxItem.Data itemData = new UIGetChanceBoxItem.Data();
        itemData.GConfig = action.GConfig;
        itemData.vr = action.VR;
        itemData.GI = action.GI;
        itemData.EffectRoot = EffectRoot;
        itemData.NeedTip = action.NeedTip;

        switch (itemData.GConfig.GiftType)
        {
            case (int)RewardType.Energy:
            case (int)RewardType.MaxEnergy:
                itemData.FlyPoint = TopInfoView.EnergyIcon.rectTransform;
                break;
            case (int)RewardType.Gem:
                itemData.FlyPoint = TopInfoView.GemIcon.rectTransform;
                itemData.FlyCompleteClipName = "SD_ui_flotage4";
                break;
            case (int)RewardType.UpgradeA:
                itemData.FlyPoint = TopInfoView.UpgradeAIcon.rectTransform;
                itemData.FlyCompleteClipName = "SD_ui_flotage3";
                break;
            case (int)RewardType.UpgradeB:
                itemData.FlyPoint = TopInfoView.UpgradeBIcon.rectTransform;
                itemData.FlyCompleteClipName = "SD_ui_flotage2";
                break;
            case (int)RewardType.Coin:
                itemData.FlyPoint = TopInfoView.CoinIcon.rectTransform;
                itemData.FlyCompleteClipName = "SD_ui_flotage1";
                break;
            case (int)RewardType.Good:
                //抽到了物品
                itemData.FlyPoint = TopInfoView.CoinIcon.rectTransform;
                itemData.FlyCompleteClipName = "";
                break;
            default:
                itemData.FlyPoint = TopInfoView.CoinIcon.rectTransform;
                itemData.FlyCompleteClipName = "";
                break;
        }
 
        action.BoxItem.SetData(itemData);
        //item會被動態隱藏，直接在此處調用協程
        StartCoroutine(action.BoxItem.BoxEffect());

        if (!((IList<int>)BoardBoxStates).Contains(1) && !boardWaitting)
        {
            //全部開完，換一批
            StartCoroutine(NextBoardEffect());
        }

        //？延遲1.5秒播放 為什麼?
        yield return new WaitForSeconds(1.5f);

        if (itemData.GConfig.GiftType != (int)RewardType.Unit)
        {
            StartCoroutine(playKeyRunOut(true));
        }
    }

    //剪刀飛到箱子動畫
    IEnumerator CutEffect(ActionData action)
    {
        Image CutImgFly = gc.PoolManager.PrefabSpawn(cutImgSpawnKey).GetComponent<Image>();
        CutImgFly.gameObject.SetActive(true);
        CutImgFly.rectTransform.SetParent(CutImg.transform.parent);
        CutImgFly.rectTransform.anchoredPosition3D = CutImg.rectTransform.anchoredPosition3D;
        CutImgFly.rectTransform.localScale = new Vector3(1, 1, 1);

        LeanTween.value(CutImgFly.gameObject, CutImgFly.rectTransform.anchoredPosition, action.BoxItem.RectTransform.anchoredPosition, .2f).setOnUpdate(delegate (Vector2 val)
        {
            CutImgFly.rectTransform.anchoredPosition = val;
        });

        while (LeanTween.isTweening(CutImgFly.gameObject))
        {
            yield return 0;
        }

        gc.PoolManager.Despawn(CutImgFly.transform);
    }

    public void OnBackClick()
    {
        GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.BACK_TYPE2);

        if (gc.SceneCtr.TargetScene == Scenes.MapScene)
        {
            gc.Popup.Close(PopupID.UIGetChance, false);
        }
        else
        {
            gc.SceneCtr.LoadLevel(Scenes.MapScene, null, true, true);
        }
    }

    private void addFliter()
    {
        GuideNode node = new GuideNode();
        node.TargetNode = GetItem(4);
        node.TarCamera = Camera.main;
        node.index = 2;
        node.ShowMask = false;
        node.CallBack = delegate ()
        {
            item_OnOpenBox(4);
        };
        GuideManager.getInstance().addGuideNode("AwardPageSelectBtn", node);


        GuideNode node1 = new GuideNode();
        node1.TargetNode = BackButton;
        node1.TarCamera = Camera.main;
        node1.index = 4;
        node1.ShowMask = false;
        node1.CallBack = delegate ()
        {
            OnBackClick();
        };
        GuideManager.getInstance().addGuideNode("AwardPageBackBtn", node1);
    }

    private void removeFliter()
    {
        GuideManager.getInstance().removeGuideNode("AwardPageSelectBtn");
        GuideManager.getInstance().removeGuideNode("AwardPageBackBtn");
    }

    //在夥伴窗口關閉時播放？在抽獎動畫末尾播放？
    bool isPlaying = false;
    IEnumerator playKeyRunOut(bool isPlay)
    {
        if (isPlaying)
            yield break;

        if (isPlay && gc.PlayerCtr.PlayerData.passStageId > 5 && gc.PlayerCtr.PlayerData.key <= 0)
        {
            isPlaying = true;
            GameController.Instance.AudioManager.PlayAudio("Vo_accompany_15");
        }

        yield return new WaitForSeconds(2.5f);
        isPlaying = false;
    }
}
