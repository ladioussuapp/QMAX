using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using UnityEngine;
using UnityEngine.UI;

//包括圖片與namebar
public class UpGradUnitItem : MonoBehaviour
{
    const string SELECT_EFFECT_PATH = "Prefabs/Effects/UiUpgradSelectEffect";
    const string UNIT_ITEM_PATH = "Prefabs/Ui/UIUpgrad/UnitListItem";
    UpGradUnitItemRendder rendder;

    ItemData data;
    UIUpgradWin upgradWin;
    RectTransform selectEffect;

    UpGradUnitItemRendder LoadItemRendder()
    {
        RectTransform rendderT = GameController.Instance.QMaxAssetsFactory.CreatePrefab(UNIT_ITEM_PATH) as RectTransform;
        rendderT.SetParent(transform);
        rendderT.localScale = new Vector3(1, 1, 1);
        rendderT.anchoredPosition3D = Vector3.zero;
        rendder = rendderT.GetComponent<UpGradUnitItemRendder>();
        rendder.HitAreaButton.onClick.AddListener(OnUpgradButtonClick);
        rendder.UpgradeButton.onClick += TipButton_onClick;
        rendder.TipImg.gameObject.SetActive(false);

        return rendder;
    }

    void TipButton_onClick(UIButtonBehaviour button)
    {
        OnUpgradButtonClick();
    }

    public void OnDestroy()
    {
        if (rendder == null)
        {
            return;
        }

        if (rendder.UpgradeButton != null)
        {
            rendder.UpgradeButton.onClick -= TipButton_onClick;
        }

        if (rendder.HitAreaButton != null)
        {
            rendder.HitAreaButton.onClick.RemoveAllListeners();
        }
    }

    public void SetData(ItemData value , bool delayLoad = false)
    {
        if (data == value)
        {
            return;
        }

        if (rendder == null)
        {
            rendder = LoadItemRendder();
        }

        data = value;

        if (data == null)
        {
            //不顯示
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (delayLoad)
        {
            Invoke("LoadRenderBody", 0.3f);
        }
        else
        {
            LoadRenderBody();
        }
 
        if (data.isLock)
        {
            //黑色不顯示
            rendder.NamesBar.gameObject.SetActive(false);

            if (value.config.TipsIcon != "")
            {
                rendder.StageGetTipImg.gameObject.SetActive(true);
                rendder.StageGetTipImg.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIUpgradeBg, value.config.TipsIcon);
                rendder.StageGetTipImg.SetNativeSize();
                rendder.StageGetTipImg.transform.localPosition = new Vector3(0, 64, 0);
                rendder.StageGetTipImg.transform.localRotation = Quaternion.AngleAxis(20, Vector3.forward);
            }
            else
            {
                rendder.StageGetTipImg.gameObject.SetActive(false);
            }
        }
        else
        {
            rendder.NamesBar.gameObject.SetActive(true);
            rendder.NamesBar.SetDatas(data.config);
            rendder.StageGetTipImg.gameObject.SetActive(false);
        }

        Transform upgradWinT = GameController.Instance.Popup.GetPopup(PopupID.UIUpgradWin);

        if (upgradWinT != null)
        {
            upgradWin = upgradWinT.GetComponent<UIUpgradWin>();

            if (upgradWin != null)
            {
                UnitConfig uWinTargetUnit = upgradWin.GetData().uConfig;
                //判斷是否是當前升級窗口中選中的伙伴新數據
                //升級後，ID發生改變,unitTypeId保持不變。

                if (uWinTargetUnit.UnitTypeId == data.config.UnitTypeId)
                {
                    UIUpgradWin.Data wData = new UIUpgradWin.Data();
                    wData.uConfig = data.config;
                    upgradWin.SetData(wData, true);
                }
            }
        }

        if (data.upgradAble)
        {
            if (!rendder.TipImg.gameObject.activeSelf)
            {
                rendder.TipImg.gameObject.SetActive(true);
                LTDescr ltDescr = LeanTween.value(rendder.TipImg.gameObject, 1f, 1.1f, .4f);

                ltDescr.setOnUpdate(delegate (float val)
                {
                    rendder.TipImg.transform.localScale = new Vector3(val, val, val);
                });

                ltDescr.setLoopPingPong();
            }
        }
        else
        {
            rendder.TipImg.gameObject.SetActive(false);
            LeanTween.cancel(rendder.TipImg.gameObject);
        }

        SelectChange();
    }

    void LoadRenderBody()
    {
        GameController.Instance.QMaxAssetsFactory.CreateUnitSprite(
        data.config, new Vector2(.5f, 0),
        delegate (Sprite s)
        {
            rendder.Body.sprite = s;
            rendder.Body.SetNativeSize();
            rendder.Body.color = data.isLock ? Color.black : Color.white;
        });
    }

    void SelectChange()
    {
        if (data.isSelect && selectEffect == null)
        {
            //添加
            //不移除。此場景不會改變出場的伙伴，所以選中狀態會一直保持
            selectEffect = GameController.Instance.QMaxAssetsFactory.CreatePrefab(SELECT_EFFECT_PATH) as RectTransform;
            selectEffect.SetParent(transform);
            selectEffect.anchoredPosition3D = Vector3.zero;
            selectEffect.localScale = new Vector3(1, 1, 1);
            selectEffect.SetSiblingIndex(0);
            //UiUpgradSelectEffect selectEffectB = selectEffect.GetComponent<UiUpgradSelectEffect>();
            //selectEffectB.ChangeBgColor(UIUpgradeColorConfig.Instance.Body_BG_COLORS[(int)data.config.UnitColor - 1]);
        }
    }

    /// <summary>
    /// 是否在等待夥伴緩存成功
    /// </summary>
    bool wait4LoadUnit = false;

    public void OnUpgradButtonClick()
    {
        if (data.isLock || wait4LoadUnit)
        {
            return;
        }
 
        wait4LoadUnit = true;
        //第一次彈出窗口時，需要事先緩存當前的這只夥伴，不然會看到打開的窗口中出現空白的現象。。
        GameController.Instance.PoolManager.GetUnitInstance(data.config, GetUnitCallBack);
    }

    //第一隻夥伴先緩存成功再打開窗口， 防止在窗口被打開後因為初始化夥伴造成的另一次卡頓
    void GetUnitCallBack(string key, Transform trans)
    {
        if (trans != null)
        {
            GameController.Instance.PoolManager.PushToInstancePool(key, trans);
        }

        OpenUpgradeWin();
        wait4LoadUnit = false;
    }

    void OpenUpgradeWin()
    {
        //打開窗口，升級
        Transform t = GameController.Instance.Popup.Open(PopupID.UIUpgradWin, null, true, false);
        upgradWin = t.GetComponent<UIUpgradWin>();

        UIUpgradWin.Data wData = new UIUpgradWin.Data();
        wData.uConfig = data.config;
        upgradWin.Caller = this;
        upgradWin.SetData(wData, false);
    }
 
    public class ItemData
    {
        public UnitConfig config;
        public bool upgradAble;
        public bool isLock;     //鎖定時是黑色
        public bool isSelect;   //是否選中
    }

}

