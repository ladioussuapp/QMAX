using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Tools;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UIUpgradWin : PopupEventCor
{
    public Transform ContentRenderPrefab;
    public Text HpText;
    public Text AtkText;
    public Image SkillImg;
    public Image AtkImg;        //攻擊圖片顯示基本消除圖片
    public Text UpgradAText;
    public Text UpgradBText;
    public UnitNamesBar namesBar;
    public RectTransform AtkGo;
    public RectTransform HpGo;
    public RectTransform LvlGo;
    public RectTransform SkillGo;
    public RectTransform UpgradeA;
    public RectTransform UpgradeB;

    public Text InfoMsg;    //夥伴的描述信息
    public Text SkillMsg;   //夥伴的技能描述

    //顯示升級時
    //拆分成UpgradeA與UpgradeB
    public RectTransform UpgradesContent;
    //顯示信息時
    public RectTransform InfoContent;

    public UIUpgradUnitProgressBar progressBar;
    public Transform ButtonBar;
    public RectTransform MaxLvlInfoBar;
    public GameObject UpgradeEffect;
    public GameObject AtkEffect;
    public GameObject HpEffect;
    public GameObject SkillEffect;
    public GameObject LvlEffect;
    public Image ColorBg;
    public Image ButtonColorBg;

    //舊效果 暫時屏蔽，後期刪除
    //public Image FootColorImg;
    public Image MoneyTipImg;

    public UIButtonBehaviour InfoSwitchButton;
    public UIButtonBehaviour UpgradeSwitchButton;

    [HideInInspector]
    public MonoBehaviour Caller;
    public UpgradTextEffect TextEffect;
    public UIButtonBehaviour CloseButton;
    public UIButtonBehaviour UpgradeButton;

    /// <summary>
    /// 快速升級按鈕
    /// </summary>
    public UIButtonBehaviour FastButton;
    public UIButtonBehaviour LeftButton;
    public UIButtonBehaviour RightButton;

    public Color[] Body_BG_COLORS;
    Color[] BG_COLORS = new Color[] { Color.white, new Color(141, 76, 219), new Color(223, 97, 24), new Color(127, 241, 82), new Color(59, 216, 214), new Color(214, 184, 13) };
    public Text[] CNTexts;
    public Animator animator;

    public QMaxScrollRect ContentScrollRect;
    UIUpgradWinContentItem cutContentItem;
    string ContentRenderPrefabKey = "UIUpgradWin::ContentRenderPrefab";
    /// <summary>
    /// 每次選中切換時，都會將此data置為當前選中項的真實data。所以外部可以通過此data數據來判斷當前選中的是哪個夥伴
    /// </summary>
    Data data;
    Data lastData;

    /// <summary>
    /// 是否需要改變夥伴模型
    /// </summary>
    bool modelChange = false;
    /// <summary>
    /// 彈出時將人物信息那些什麼的移到本窗口中，關閉窗口時再移動回去。
    /// </summary>
    Transform topInfoParent;

    public void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            animator.SetInteger("Lvl", 0);
        }

        ContentRenderPrefab.gameObject.SetActive(false);

        GameController.Instance.PoolManager.PrePrefabSpawn(ContentRenderPrefab, ContentRenderPrefabKey, 3);
    }

    void Start()
    {
        GetTopInfoView();
        ChangeLanguage();
        GameController.Instance.ModelEventSystem.OnPlayerInfoRef += ModelEventSystem_OnPlayerInfoRef;
        UpgradeButton.onClick += UpgradeButton_onClick;
        FastButton.onClick += FastButton_onClick;
        CloseButton.onClick += CloseButton_onClick;
        InfoSwitchButton.onClick += SwitchButton_onClick;
        UpgradeSwitchButton.onClick += SwitchButton_onClick;
        GameController.Instance.Popup.OnOpenComplete += onOpenWindow;

        //默認顯示升級部分
        InfoContent.gameObject.SetActive(false);
        UpgradesContent.gameObject.SetActive(true);

        // 材料足夠可以自動升級
        if (GameController.Instance.UnitCtr.CheckUpgradAble(data.uConfig, true))
        {
            FastButton.interactable = true;
        }

        LeftButton.onClick += LeftButton_onClick;
        RightButton.onClick += RightButton_onClick;
        //ContentScrollRect.DragAble = false;
        ContentScrollRect.OnSelectChange += ContentScrollRect_OnSelectChange;
        //ContentScrollRect.OnDragEvent += ContentScrollRect_OnDragEvent;
        ContentScrollRect.OnDragBeginEvent += ContentScrollRect_OnDragBeginEvent;
    }

    public void OnDestroy()
    {
        GameController.Instance.PoolManager.Despool(ContentRenderPrefabKey);

        if (GameController.Instance.Popup.IsPopup(PopupID.UIUpgradGoleTipWin))
        {
            GameController.Instance.Popup.Close(PopupID.UIUpgradGoleTipWin, false);
        }

        StopAllCoroutines();

        GameController.Instance.ModelEventSystem.OnPlayerInfoRef -= ModelEventSystem_OnPlayerInfoRef;
        UpgradeButton.onClick -= UpgradeButton_onClick;
        FastButton.onClick -= FastButton_onClick;
        CloseButton.onClick -= CloseButton_onClick;

        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                removeFliter_v1();
                break;
            default:
                break;
        }
    }

    void RightButton_onClick(UIButtonBehaviour button)
    {
        //滑動到右邊
        if (ContentScrollRect.SelectIndex != ContentScrollRect.items.Count - 1)
        {
            ContentScrollRect.SelectIndex = ContentScrollRect.SelectIndex + 1;
        }
    }

    void LeftButton_onClick(UIButtonBehaviour button)
    {
        //滑動到左邊
        if (ContentScrollRect.SelectIndex != 0)
        {
            ContentScrollRect.SelectIndex = ContentScrollRect.SelectIndex - 1;
        }
    }

    /// <summary>
    /// 緩存3項 但是只動態加載一項
    /// </summary>
    UIUpgradWinContentItem[] Items = new UIUpgradWinContentItem[3];

    void ContentScrollRect_OnSelectChange(ScrollRectItem arg1, ScrollRectItem arg2)
    {
        cutContentItem = arg2 as UIUpgradWinContentItem;
        data.uConfig = cutContentItem.ItemData.uConfig;
        SetData(data, false);
        //讓父物件ScrollRectItem的z軸歸零 夥伴升級UI才能正常顯示----------------------------12/23
        cutContentItem.transform.localPosition = new Vector3(cutContentItem.transform.localPosition.x, cutContentItem.transform.localPosition.y,0);

        DrawContentItems(1);
    }

    void ContentScrollRect_OnDragBeginEvent(UnityEngine.EventSystems.PointerEventData obj)
    {
        DrawContentItems(obj.delta.x < 0 ? 2 : 0);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dir">0 左邊 2 右邊</param>
    void DrawContentItems(int dir)
    {
        UIUpgradWinContentItem itemLNew = ContentScrollRect.SelectIndex == 0 ? null : ContentScrollRect.items[ContentScrollRect.SelectIndex - 1] as UIUpgradWinContentItem;
        UIUpgradWinContentItem itemSNew = cutContentItem;
        UIUpgradWinContentItem itemRNew = ContentScrollRect.SelectIndex == ContentScrollRect.items.Count - 1 ? null : ContentScrollRect.items[ContentScrollRect.SelectIndex + 1] as UIUpgradWinContentItem;
        UIUpgradWinContentItem itemHelp;

        LeftButton.gameObject.SetActive(itemLNew != null);
        RightButton.gameObject.SetActive(itemRNew != null);

        for (int i = 0; i < 3; i++)
        {
            itemHelp = Items[i];

            if (itemHelp && itemHelp != itemLNew && itemHelp != itemSNew && itemHelp != itemRNew)
            {
                itemHelp.Clear();
            }
        }

        Items[0] = itemLNew;
        Items[1] = itemSNew;
        Items[2] = itemRNew;

        if (Items[dir] != null)
        {
            Items[dir].Draw();
            Items[dir].BodyChange();
        }
    }

    void SwitchButton_onClick(UIButtonBehaviour button)
    {
        if (button == InfoSwitchButton)
        {
            //顯示信息部分
            InfoContent.gameObject.SetActive(true);
            UpgradesContent.gameObject.SetActive(false);
        }
        else
        {
            //顯示升級部分
            InfoContent.gameObject.SetActive(false);
            UpgradesContent.gameObject.SetActive(true);
        }
    }

    private void onOpenWindow(PopupID obj)
    {
        if (obj == PopupID.UIUpgradWin)
        {
            GameController.Instance.Popup.OnOpenComplete -= onOpenWindow;
            switch (GuideManager.getInstance().version)
            {
                case GuideVersion.Version_1:
                    addFliter_v1();
                    break;
                default:
                    break;
            }
        }
    }

    void CloseButton_onClick(UIButtonBehaviour button)
    {
        Close();
    }

    override public void Close()
    {
        Caller = null;
        ReturnTopInfoView();
        GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.BACK_TYPE2);
        GameController.Instance.Popup.Close(PopupID.UIUpgradWin);
    }

    void UpgradeButton_onClick(UIButtonBehaviour button)
    {
        if (GameController.Instance.UnitCtr.CheckUpgradAble(data.uConfig, true))
        {
            //材料夠，直接升級
            UpgradeUnit();
        }
        else
        {
            //材料不夠 去購買
            UIUpgradGoleTipWin tipWin = GameController.Instance.Popup.Open(PopupID.UIUpgradGoleTipWin, null, true, true).GetComponent<UIUpgradGoleTipWin>();
            UIUpgradGoleTipWin.Data tipData = new UIUpgradGoleTipWin.Data();
            tipWin.OnBuyUpgrade += tipWin_OnBuyUpgrade;

            int lessUpgradeA = data.uConfig.UnitUpgradeA - GameController.Instance.PlayerCtr.PlayerData.upgradeA;
            int leffUpgradeB = data.uConfig.UnitUpgradeB - GameController.Instance.PlayerCtr.PlayerData.upgradeB;
            tipData.UpgradeA = lessUpgradeA > 0 ? lessUpgradeA : 0;
            tipData.UpgradeB = leffUpgradeB > 0 ? leffUpgradeB : 0;
            tipData.Gold = GameController.Instance.UnitCtr.GetGemCost(tipData.UpgradeA, tipData.UpgradeB);
            tipWin.SetData(tipData);
        }
    }

    UIAlertBehaviour upgradeAlertTmp;

    void FastButton_onClick(UIButtonBehaviour button)
    {
        if (GameController.Instance.UnitCtr.CheckUpgradAble(data.uConfig, true))
        {
            //不能跟升級夥伴的的新手引導相衝突
            //if (GuideManager.getInstance().guideIndex != GuideManager.getInstance().UPGRADE_FIRST_UNIT)
            if (GuideManager.getInstance().CurrentGuideID() != GuideManager.getInstance().UPGRADE_FIRST_UNIT)
            {
                if (!GameController.Instance.Popup.IsPopup(PopupID.UIAlert) && PlayerPrefsTools.GetIntValue(OnOff.TipAutoUpgradeMark, true) == 0)
                {
                    //是否需要彈出提示
                    upgradeAlertTmp = UIAlertBehaviour.Alert(Utils.GetTextByID(1729), "", "", 2, 0, 0
                    , (byte)UIAlertBehaviour.ButtonStates.ButtonOk | (byte)UIAlertBehaviour.ButtonStates.ButtonCancel | (byte)UIAlertBehaviour.ButtonStates.DisplayNoMoreToggle);

                    //alert在關閉後會把所有按鈕事件清除
                    upgradeAlertTmp.OnClickOKButton += upgradeAlert_OnClick;
                    upgradeAlertTmp.OnClickCacelButton += upgradeAlert_OnClick;
                    GameController.Instance.Popup.OnCloseComplete += Popup_OnCloseComplete;
                }
                else
                {
                    UpgradeUnit(true);
                }
            }
            else
            {
                UpgradeUnit(true);
            }
        }
    }

    /// <summary>
    /// for 自動升級 提示框
    /// </summary>
    /// <param name="obj"></param>
    void Popup_OnCloseComplete(PopupID obj)
    {
        upgradeAlertTmp.OnClickOKButton -= upgradeAlert_OnClick;
        upgradeAlertTmp.OnClickCacelButton -= upgradeAlert_OnClick;
        GameController.Instance.Popup.OnCloseComplete -= Popup_OnCloseComplete;

        if (obj == PopupID.UIAlert)
        {
            //快速升級的提示框 點了確定按鈕
            //快速自動升級
            if (upgradeAlertTmp != null)
            {
                if (upgradeAlertTmp.DisplayNoMoreToggle.isOn)
                {
                    //不再提示 以後都不需要再提示
                    PlayerPrefsTools.SetIntValue(OnOff.TipAutoUpgradeMark, 1, true);
                }
            }
        }
    }

    void upgradeAlert_OnClick(UIButtonBehaviour obj)
    {
        upgradeAlertTmp.OnClickOKButton -= upgradeAlert_OnClick;
        upgradeAlertTmp.OnClickCacelButton -= upgradeAlert_OnClick;

        if (upgradeAlertTmp != null)
        {
            if (obj == upgradeAlertTmp.ButtonOK)
            {
                UpgradeUnit(true);
            }
        }
    }

    void ModelEventSystem_OnPlayerInfoRef(System.Collections.Generic.List<RewardType> arg1, Com4Love.Qmax.Net.Protocols.ActorGame.ActorGameResponse arg2)
    {
        //材料不夠現實提示圖片，不管鑽石夠不夠。鑽石不夠的話會提示充值
        MoneyTipImg.gameObject.SetActive(!GameController.Instance.UnitCtr.CheckUpgradAble(data.uConfig, true, false));
    }

    void ChangeLanguage()
    {
        //CNTexts
        foreach (Text text in CNTexts)
        {
            text.text = Utils.GetText(text.text);
        }
    }

    void GetTopInfoView()
    {
        Vector3 pos = UIUpgradBehaviour.Instance.TopInfo.anchoredPosition3D;
        topInfoParent = UIUpgradBehaviour.Instance.TopInfo.parent;
        RectTransform canvasRoot = GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        UIUpgradBehaviour.Instance.TopInfo.SetParent(canvasRoot);
        UIUpgradBehaviour.Instance.TopInfo.localScale = new Vector3(1, 1, 1);
        UIUpgradBehaviour.Instance.TopInfo.anchoredPosition3D = pos;


        //TopInfoView tiv = UIUpgradBehaviour.Instance.TopInfo.GetComponent<TopInfoView>();
        //if (tiv != null)
        //{
        //    tiv.AutoUpdate = false;
        //    tiv.isTweenText = true;
        //}
    }

    void ReturnTopInfoView()
    {
        //TopInfoView tiv = UIUpgradBehaviour.Instance.TopInfo.GetComponent<TopInfoView>();
        //if (tiv != null)
        //{
        //    tiv.AutoUpdate = true;
        //    tiv.isTweenText = false;
        //}
        Vector3 pos = UIUpgradBehaviour.Instance.TopInfo.anchoredPosition3D;
        UIUpgradBehaviour.Instance.TopInfo.SetParent(topInfoParent);
        UIUpgradBehaviour.Instance.TopInfo.localScale = new Vector3(1, 1, 1);
        UIUpgradBehaviour.Instance.TopInfo.anchoredPosition3D = pos;

    }

    /// <summary>
    /// 此處在完成所有的升級動作之後調用，刷新介面的數據。也在初始化的時候調用一次
    /// </summary>
    void UpdateDataInfo()
    {
        UpgradeSwitchButton.interactable = InfoSwitchButton.interactable = CloseButton.interactable = true;
        ContentScrollRect.DragAble = true;
        LeftButton.interactable = RightButton.interactable = true;

        if (cutContentItem != null)
        {
            cutContentItem.TouchAble = true;
        }

        lastData = data;
        ButtonBar.gameObject.SetActive(!IsMaxLvl());
        UpgradeA.gameObject.SetActive(!IsMaxLvl());
        UpgradeB.gameObject.SetActive(!IsMaxLvl());
        MaxLvlInfoBar.gameObject.SetActive(IsMaxLvl());
        AtkText.text = data.uConfig.UnitAtk.ToString();
        HpText.text = data.uConfig.UnitHp.ToString();
        UpgradAText.text = data.uConfig.UnitUpgradeA.ToString();
        UpgradBText.text = data.uConfig.UnitUpgradeB.ToString();

        MoneyTipImg.gameObject.SetActive(!GameController.Instance.UnitCtr.CheckUpgradAble(data.uConfig, true, false));

        //夥伴信息顯示更新
        InfoMsg.text = GameController.Instance.UnitCtr.GetStroyStr(data.uConfig);
        SkillConfig skillConfig = GameController.Instance.Model.SkillConfigs[data.uConfig.UnitSkillId];
        SkillMsg.text = string.Format("{0}：{1}", GameController.Instance.UnitCtr.GetSkillNameStr(skillConfig), GameController.Instance.UnitCtr.GetSkillEffectStr(skillConfig));
    }

    public Data GetData()
    {
        return data;
    }

    public void SetData(Data data_, bool isUpgrade)
    {
        data = data_;

        AtlasManager atlasMgr = GameController.Instance.AtlasManager;
        SkillConfig skillConfig = GameController.Instance.Model.SkillConfigs[data.uConfig.UnitSkillId];
        SkillImg.sprite = atlasMgr.GetSprite(Atlas.Tile, skillConfig.ResourceIcon);
        SkillImg.SetNativeSize();

        string[] strs = {
            "ElementPurple",
            "ElementRed",
            "ElementGreen",
            "ElementBlue",
            "ElementYellow"};
        AtkImg.sprite = atlasMgr.GetSprite(Atlas.Tile, strs[(int)data.uConfig.UnitColor - 1]);
        AtkImg.SetNativeSize();
        progressBar.SetData(data_.uConfig, isUpgrade);

        if (isUpgrade)
        {
            if (lastData.uConfig != null && lastData.uConfig.PrefabPath != data.uConfig.PrefabPath)
            {
                modelChange = true;     //改變了模型
            }
            else
            {
                modelChange = false;
            }

            cutContentItem.ItemData.uConfig = data.uConfig;
            //cutContentItem.ItemData.nextUConfig = data.uConfig.UnitUpgrade == -1 ? null : GameController.Instance.Model.UnitConfigs[data.uConfig.UnitUpgrade];

            //升級刷新
            StartCoroutine(PlayUpgradeEffect());
        }
        else
        {
            ChangeBg();
            UpdateDataInfo();
            namesBar.SetDatas(data.uConfig);

            if (ContentScrollRect.GetDatas() == null)
            {
                SetListData();
            }
        }

        // 材料是否足夠自動升級
        FastButton.interactable = GameController.Instance.UnitCtr.CheckUpgradAble(data.uConfig, true);
    }

    void SetListData()
    {
        //test 測試滾動的數據
        List<ScrollRectItemData> itemDatas = new List<ScrollRectItemData>();
        UIUpgradWinContentItemData itemData;
        List<UnitConfig> allUnits = GameController.Instance.UnitCtr.GetOwnUnits();
        UnitConfig uConfig;
        int selectIndex = 0;

        for (int i = 0; i < allUnits.Count; i++)
        {
            uConfig = allUnits[i];
            itemData = new UIUpgradWinContentItemData();
            int colorIndex = (int)uConfig.UnitColor - 1;
            itemData.BodyBgColor = Body_BG_COLORS[colorIndex];
            itemData.uConfig = uConfig;
            //itemData.nextUConfig = uConfig.UnitUpgrade == -1 ? null : GameController.Instance.Model.UnitConfigs[uConfig.UnitUpgrade];
            itemDatas.Add(itemData);

            if (uConfig.ID == data.uConfig.ID)
            {
                selectIndex = i;
            }
        }

        ContentScrollRect.SetDatas(itemDatas);
        ContentScrollRect.SelectIndex = selectIndex;
    }

    //設置數據的時候改變背景
    void ChangeBg()
    {
        Color tmpColor = BG_COLORS[(int)data.uConfig.UnitColor];
        ButtonColorBg.color = ColorBg.color = new Color(tmpColor.r / 255f, tmpColor.g / 255f, tmpColor.b / 255f);
        //FootColorImg.color = new Color(tmpColor.r / 255f, tmpColor.g / 255f, tmpColor.b / 255f);

    }

    bool IsMaxLvl()
    {
        return data.uConfig.UnitUpgrade == -1;
    }

    IEnumerator PlayUpgradeEffect()
    {
        if (modelChange)
        {
            //緩存夥伴現在已經不卡 改到最開始
            cutContentItem.PreloadBody();
        }

        yield return StartCoroutine(PropertyAnimation());
        yield return StartCoroutine(PlayPropertyEffect());
    }

    IEnumerator PropertyAnimation()
    {
        animator.SetInteger("Lvl", data.uConfig.Level);

        int upgradeA = lastData.uConfig.UnitUpgradeA;
        int upgradeB = lastData.uConfig.UnitUpgradeB;

        if (upgradeA > 0)
            animator.SetTrigger("TriggerL");

        if (upgradeB > 0)
            animator.SetTrigger("TriggerR");
        yield return new WaitForSeconds(1f);
    }

    private bool guideFlag = true;
    IEnumerator PlayPropertyEffect()
    {
        int numUp;

        if (!modelChange)
        {
            //升級不升階時在播放夥伴動畫時事先播放音效 升階時在切換夥伴模型後再播放音效
            cutContentItem.PlayLvlUpAudio();
        }
 
        cutContentItem.UnitAnimation();
        LvlEffect.SetActive(true);
        TextEffect.Play(Utils.GetText("升 级 成 功 ！"));
        ScaleEffect(LvlGo);
        namesBar.SetDatas(data.uConfig);
        yield return new WaitForSeconds(.8f);

        //攻擊力提升
        numUp = data.uConfig.UnitAtk - lastData.uConfig.UnitAtk;
        TextEffect.Play(Utils.GetText("攻 擊 力 + {0}", numUp));
        TextScroll(AtkText, lastData.uConfig.UnitAtk, data.uConfig.UnitAtk);
        AtkEffect.SetActive(true);
        ScaleEffect(AtkGo);
        yield return new WaitForSeconds(.8f);

        //刷新數據，並且可以關閉
        UpdateDataInfo();

        //血量提升
        //暫時屏蔽血量提升的提示
        //numUp = data.uConfig.UnitHp - lastData.uConfig.UnitHp;
        //TextEffect.Play(Utils.GetText("血 量 + {0}", numUp));
        //TextScroll(HpText, lastData.uConfig.UnitHp, data.uConfig.UnitHp);
        //HpEffect.SetActive(true);
        //ScaleEffect(HpGo);
        //yield return new WaitForSeconds(.8f);

        if (modelChange)
        {
            //進階
            SkillEffect.SetActive(true);
            TextEffect.Play(Utils.GetText("技 能 提 升 ！"));
            ScaleEffect(SkillGo);

            yield return new WaitForSeconds(1f);
            UpgradeEffect.gameObject.SetActive(true);
            yield return new WaitForSeconds(.3f);
            cutContentItem.BodyChange();
            cutContentItem.PlayLvlUpAudio();

            //引級成功後引導說的話
            if (GuideManager.getInstance().version == GuideVersion.Version_1)
            {
                if (guideFlag)
                {
                    GuideNode node1 = new GuideNode();
                    node1.TargetNode = null;
                    node1.TarCamera = Camera.main;
                    node1.index = 3;
                    node1.ShowMask = false;
                    node1.ShowTips = true;
                    node1.CallBack = delegate ()
                    {
                    };
                    GuideManager.getInstance().addGuideNode("SaySomething", node1);

                    UIButtonBehaviour topbut = UIUpgradBehaviour.Instance.TopInfo.GetComponent<TopInfoView>().GemButton;

                    GuideNode node3 = new GuideNode();
                    node3.TargetNode = topbut;
                    node3.TarCamera = Camera.main;
                    node3.index = 3;
                    node3.ShowMask = true;
                    node3.ShowTips = false;
                    node3.CallBack = delegate ()
                    {
                    };
                    GuideManager.getInstance().addGuideNode("UpgradeButtonTip", node3);

                    GuideNode node4 = new GuideNode();
                    node4.TargetNode = null;
                    node4.TarCamera = Camera.main;
                    node4.index = 3;
                    node4.ShowMask = false;
                    node4.ShowTips = true;
                    node4.CallBack = delegate ()
                    {
                    };
                    GuideManager.getInstance().addGuideNode("SaySomething2", node4);


                    GuideNode node2 = new GuideNode();
                    node2.TargetNode = CloseButton;
                    node2.TarCamera = Camera.main;
                    node2.index = 3;
                    node2.ShowMask = false;
                    node2.CallBack = delegate ()
                    {
                        Close();
                    };
                    GuideManager.getInstance().addGuideNode("UnitCloseUpgradeBtn", node2);

                    guideFlag = false;
                    //防止中迷,通知服務器這個引導完成
                    int guideid = GuideManager.getInstance().CurrentGuideID();
                    GameController.Instance.Client.SaveGuideIndex(GuideManager.getInstance().guideIndex, guideid);
                    GuideManager.getInstance().GuideOverIDList.Add(guideid);
                }
            }
            //
        }

        yield return new WaitForSeconds(1f);

        AtkEffect.SetActive(false);
        HpEffect.SetActive(false);
        SkillEffect.SetActive(false);
        UpgradeEffect.gameObject.SetActive(false);
        LvlEffect.SetActive(false);
    }

    void ScaleEffect(RectTransform target)
    {
        LTDescr ltDescr;

        ltDescr = LeanTween.value(target.gameObject, 1f, 1.2f, .3f);

        ltDescr = ltDescr.setEase(LeanTweenType.easeOutQuad);

        ltDescr = ltDescr.setOnUpdate(delegate (float valF)
        {
            target.localScale = new Vector3(valF, valF, valF);
        });

        ltDescr.setOnComplete(delegate ()
        {
            LeanTween.value(target.gameObject, 1.2f, 1f, .3f).setOnUpdate(delegate (float val2)
            {
                target.localScale = new Vector3(val2, val2, val2);
            });
        });
    }

    void TextScroll(Text text, int from, int to)
    {
        GameController.Instance.EffectProxy.ScrollText(text, from, to, .8f);
    }

    //升級接口 調用後，禁止點擊按鈕，禁止滑動
    public void UpgradeUnit(bool isAutoUpgrad = false)
    {
        ButtonBar.gameObject.SetActive(false);
        UpgradeSwitchButton.interactable = InfoSwitchButton.interactable = CloseButton.interactable = false;
        cutContentItem.TouchAble = ContentScrollRect.DragAble = false;
        LeftButton.interactable = RightButton.interactable = false;

        GameController.Instance.UnitCtr.UpgradUnit(cutContentItem.ItemData.uConfig.ID, isAutoUpgrad);
    }

    void tipWin_OnBuyUpgrade()
    {
        //UIUpgradGoleTipWin銷毀時移除所有監聽
        UpgradeUnit();
    }

    public struct Data
    {
        public UnitConfig uConfig;

    }

    private void addFliter_v1()
    {
        Text toptext = UIUpgradBehaviour.Instance.TopInfo.GetComponent<TopInfoView>().UpgradBText;

        GuideNode node1 = new GuideNode();
        node1.TargetNode = toptext;
        node1.TarCamera = Camera.main;
        node1.index = 3;
        node1.ShowMask = true;
        node1.ShowTips = false;

        GuideManager.getInstance().addGuideNode("UpgradeIconImage", node1);

        GuideNode node2 = new GuideNode();
        node2.TargetNode = null;
        node2.TarCamera = Camera.main;
        node2.index = 3;
        node2.ShowMask = false;
        node2.ShowTips = true;

        GuideManager.getInstance().addGuideNode("UpgradeIconImageSay", node2);

        GuideNode node = new GuideNode();
        node.TargetNode = FastButton;
        node.TarCamera = Camera.main;
        node.index = 3;
        node.ShowMask = false;
        node.CallBack = delegate ()
        {
            FastButton_onClick(null);
        };
        GuideManager.getInstance().addGuideNode("UnitUpgradeBtn", node);

        //         GuideNode node1 = new GuideNode();
        //         node1.TargetNode = null;
        //         node1.TarCamera = Camera.main;
        //         node1.index = 3;
        //         node1.ShowMask = false;
        //         node1.CallBack = delegate()
        //         {
        //         };
        //         GuideManager.getInstance().addGuideNode("SaySomething", node1);
    }


    private void removeFliter_v1()
    {
        GuideManager.getInstance().removeGuideNode("UnitUpgradeBtn");

        GuideManager.getInstance().removeGuideNode("SaySomething");
        GuideManager.getInstance().removeGuideNode("UnitCloseUpgradeBtn");

        GuideManager.getInstance().removeGuideNode("UpgradeIconImage");

        GuideManager.getInstance().removeGuideNode("UpgradeIconImageSay");

        GuideManager.getInstance().removeGuideNode("UpgradeButtonTip");

        GuideManager.getInstance().removeGuideNode("SaySomething2");
    }

}

