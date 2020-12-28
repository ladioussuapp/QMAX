using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Ctr;

public class UIUpgradBehaviour : MonoBehaviour
{
    public Image ColorBg;
    [HideInInspector]
    public QMaxScrollRect StageList;
    [HideInInspector]
    public QMaxScrollRect FGList;
    public int selectIndexInCutView = -1;    //當前界面靜止後當前介面的索引
    public UIButtonBehaviour MapButton;
    public RectTransform TopInfo;

    public UIUpgradFootButton[] FootButtons;        //按照顏色順序
    //public RectTransform Cloud;       //雲層去掉
    protected Color[] BG_COLORS = new Color[] { new Color(149, 87, 222), new Color(254, 129, 57), new Color(154, 198, 75), new Color(81, 221, 199), new Color(247, 217, 50) };

    protected ColorType cutColor = ColorType.None;
    public static UIUpgradBehaviour Instance;

    void Awake()
    {
        Instance = this;
        QMaxScrollRect[] scrollRects = GetComponents<QMaxScrollRect>();

        foreach (QMaxScrollRect item in scrollRects)
        {
            if (item.autoSelect)
            {
                FGList = item;
            }
            else
            {
                StageList = item;
            }
        }

        FGList.OnSelectChange += FGList_OnSelectChange;
        StageList.OnSelectChange += StageList_OnSelectChange;
        //StageList.onValueChanged.AddListener(OnListValueChange);

        foreach (UIUpgradFootButton footButton in FootButtons)
        {
            footButton.OnFootButtonClick += footButton_OnFootButtonClick;
        }

        MapButton.onClick += MapButton_onClick;

        GameController.Instance.AtlasManager.AddAtlas(Atlas.UIUpgradeBg, ColorBg.sprite.texture);
    }
 
    void Start()
    {
        //除開戰鬥場景 所有的ui攝像機的size都是5 如果是被戰鬥場景打開，可能需要將場景的主攝像機的size調到5
        Camera.main.orthographicSize = 5;

        GameController.Instance.AtlasManager.AddAtlas(Atlas.UIUpgradeBg, ColorBg.sprite.texture);

        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                addFliter_v1();
                break;
            default:
                break;
        }
    }
 
    public void OnDestroy()
    {
        MapButton.onClick -= MapButton_onClick;
        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                removeFliter_v1();
                break;
            default:
                break;
        }

        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIUpgradeBg);
    }

    void MapButton_onClick(UIButtonBehaviour button)
    {
        if (GameController.Instance.SceneCtr.TargetScene == Scenes.MapScene)
        {
            //在地圖場景直接被打開
            GameController.Instance.Popup.Close(PopupID.UIUpgrad , false);
        }
        else
        {
            GameController.Instance.SceneCtr.LoadLevel(Scenes.MapScene, null, true, true);
        }
    }

    void FGList_OnSelectChange(ScrollRectItem arg1, ScrollRectItem arg2)
    {
        StageList.SelectIndex = arg2.index;
    }
 
    //void OnListValueChange(Vector2 value)
    //{
    //    //Cloud.anchoredPosition = new Vector2(-Cloud.sizeDelta.x * value.x * .2f, Cloud.anchoredPosition.y);
    //}

    void footButton_OnFootButtonClick(UIUpgradFootButton obj)
    {
        //切換按鈕音效
        SelectChange(obj.color);
    }

    private bool _isInited;
    void Update()
    {
        if (_isInited)
            return;
        _isInited = true;
    }

    //選中改變
    void StageList_OnSelectChange(ScrollRectItem arg1, ScrollRectItem arg2)
    {
        if (_isInited)
        {
            // 剛打開介面初始化數據的時候會回調此方法，但並不需要比方音效，這裡這樣處理
            GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.UPGRAD_SCENE_PAGEBUTTON_CLICK);
        }

        UIUpgradStageItem item = arg2 as UIUpgradStageItem;
        UIUpgradStageItemData itemData = item.Data as UIUpgradStageItemData;

        if (itemData != null)
        {
            SelectChange(itemData.ColorType);
        }
    }
   

    protected void SelectChange(ColorType color)
    {
        if (color == cutColor)
        {
            return;
        }

        foreach (UIUpgradFootButton button in FootButtons)
        {
            button.SetSelect(button.color == color);
        }

        UIUpgradStageItemData selectItemData = StageList.SelectItem.Data as UIUpgradStageItemData;

        if (selectItemData.ColorType != color)
        {
            //當前list的選中項不是此顏色， 滑動項
            ScrollRectItem item = StageList.items[(int)color - 1];
            StageList.SelectItem = item;

            ScrollRectItem fgItem = FGList.items[item.index];
            FGList.SelectItem = fgItem;
        }

        ChangeBgColor(color);
    }

    /// <summary>
    /// 5種顏色 5個數據
    /// </summary>
    /// <param name="datas"></param>
    public void SetDatas(UIUpgradStageItemData[] datas, int selectIndex)
    {
        ScrollRectItem item;
        UIUpgradFootButton button;
        UIUpgradStageItemData itemData;

        for (int i = 0; i < 5; i++)
        {
            itemData = datas[i];
            button = FootButtons[i];
            button.setData(itemData);

            if (i < StageList.items.Count)
            {
                itemData.IsDefaultStage = (i == selectIndex);
                item = StageList.items[i];
                item.Data = itemData;
            }
        }

        if (selectIndex != -1)
        {
            //指定選中的索引項
            FGList.SelectIndex = selectIndex;
            StageList.SelectIndex = selectIndex;
            FGList.ContentToPosition();
            StageList.ContentToPosition();
        }

        if (StageList.SelectItem)
        {
            itemData = StageList.SelectItem.Data as UIUpgradStageItemData;
            SelectChange(itemData.ColorType);
        }
    }

    protected void ChangeBgColor(ColorType color)
    {
        //ColorBg
        //測試直接改變顏色
        Color tmpColor = BG_COLORS[(int)color - 1];     //color中的r g b是0到1.。。。。 
        ColorBg.color = new Color(tmpColor.r / 255f, tmpColor.g / 255f, tmpColor.b / 255f);
    }

    private void addFliter_v1()
    {
        GuideNode node = new GuideNode();
        node.TargetNode = MapButton;
        node.TarCamera = Camera.main;
        node.index = 3;
        node.ShowMask = false;
        node.CallBack = delegate()
        {
            MapButton_onClick(MapButton);
        };
        GuideManager.getInstance().addGuideNode("UnitBackToMapView", node);
    }


    private void removeFliter_v1()
    {
        GuideManager.getInstance().removeGuideNode("UnitBackToMapView");
    }
}
