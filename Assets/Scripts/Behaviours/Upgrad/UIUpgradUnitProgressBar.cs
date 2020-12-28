using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgradUnitProgressBar : MonoBehaviour
{
    public Image[] UnitBodys;
    public Image[] UnitLvlImgs;
    public UIButtonBehaviour[] UnitLvlButtons;
    public Image ProgressBarImg;
    public Transform ProgressBarEffect;

    int[] LVL_PROGRESS = new int[5] { 1, 5, 10, 15, 20 };
    float[] BAR_WIDTH_PROGRESS = new float[] { 100f, 240f, 380f, 540f, 700f };  //670

    Color[] PROGRESS_BG_COLORS = new Color[] { Color.white, new Color(197f, 152f, 169f), new Color(199f, 136f, 66f), new Color(108f, 173f, 9f), new Color(101f, 210f, 186f), new Color(211f, 187f, 92f) };

    /// <summary>
    /// 不同顏色界面的描邊顏色
    /// </summary>
    public Color[] OutLineColor;
    bool dataReady;
    UnitConfig Data;

    //float barWidth;

    void Awake()
    {
        barWidth = ProgressBarImg.rectTransform.sizeDelta.x;

        for (int i = 0; i < UnitLvlButtons.Length; i++)
        {
            UIButtonBehaviour lvlButton = UnitLvlButtons[i];
            lvlButton.onClick += LvlButton_onClick;
        }
    }

    public void OnDestroy()
    {
        for (int i = 0; i < UnitLvlButtons.Length; i++)
        {
            UIButtonBehaviour lvlButton = UnitLvlButtons[i];
            lvlButton.onClick -= LvlButton_onClick;
        }
    }

    private void LvlButton_onClick(UIButtonBehaviour button)
    {
        int index = Array.IndexOf(UnitLvlButtons, button);
        int lvl = index * 5;
        lvl = lvl == 0 ? 1 : lvl;
        OpenSkillTip(lvl);
    }

    public void OnEnable()
    {
        dataReady = false;
    }

    void OpenSkillTip(int unitLvl)
    {
        UnitConfig unitConfig = GameController.Instance.UnitCtr.GetUnit(Data.UnitTypeId, unitLvl);
        int nextLvl = unitLvl + 5;
        nextLvl = nextLvl > 20 ? 20 : nextLvl;
        UnitConfig nextUnitConfig = GameController.Instance.UnitCtr.GetUnit(Data.UnitTypeId, nextLvl);

        GameController.Instance.EffectProxy.SnapshotCache(Camera.main, 2);
        UIUpgradeSkillTip skillTip = GameController.Instance.Popup.Open(PopupID.UIUpgradeSkillTip, null, false, true, -1, "Prefabs/Ui/UIScreenSnapshotPopUp")
            .GetComponent<UIUpgradeSkillTip>();
        skillTip.LvlUnitData = unitConfig;
        skillTip.NextLvlUnitData = nextUnitConfig;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isUpgrade">是否是升級動作造成的數據改變</param>
    public void SetData(UnitConfig data, bool isUpgrade)
    {
        Data = data;
        int unityType = data.UnitTypeId;

        UnitConfig typeUnit;
        Image body;
        Image lvlImg;
        Image checkImg;
        Text lvlText;
        UnitConfig ownTypeUnit;
        Color tmpColor;

        ownTypeUnit = GameController.Instance.UnitCtr.GetOwnTypeUnit(unityType);
        int lvlProgress = 1;
        int progress = LVL_PROGRESS.Length;

        for (int i = 0; i < LVL_PROGRESS.Length; i++)
        {
            lvlProgress = LVL_PROGRESS[i];
            typeUnit = GameController.Instance.UnitCtr.GetUnit(unityType, lvlProgress);
            body = UnitBodys[i];
            lvlImg = UnitLvlImgs[i];

            lvlText = lvlImg.transform.Find("ColorText").GetComponent<Text>();
            checkImg = lvlImg.transform.Find("CheckImg").GetComponent<Image>();
            SetA(body, typeUnit);
            lvlImg.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, string.Format("yangcheng_colorbutton{0}", (int)data.UnitColor));
            lvlImg.SetNativeSize();
            checkImg.GetComponent<Outline>().effectColor = OutLineColor[(int)data.UnitColor - 1];
            lvlText.GetComponent<Outline>().effectColor = OutLineColor[(int)data.UnitColor - 1];

            //進度達到
            if (lvlProgress <= ownTypeUnit.Level)
            {
                body.color = Color.white;
                checkImg.gameObject.SetActive(true);
                lvlText.gameObject.SetActive(false);

                progress = i;
            }
            else
            {
                body.color = Color.black;
                checkImg.gameObject.SetActive(false);
                lvlText.gameObject.SetActive(true);
            }
        }

        tmpColor = PROGRESS_BG_COLORS[(int)data.UnitColor];
        ProgressBarImg.color = new Color(tmpColor.r / 255f, tmpColor.g / 255f, tmpColor.b / 255f);

        float fWidth = ProgressBarImg.rectTransform.sizeDelta.x;
        float tmpLvlProgress = data.Level - LVL_PROGRESS[progress];

        //之前的邏輯 不規律排列
        int nextProgress = progress == BAR_WIDTH_PROGRESS.Length - 1 ? progress : progress + 1;
        float tWidth = BAR_WIDTH_PROGRESS[progress];
        tWidth = (tmpLvlProgress / 5) * (BAR_WIDTH_PROGRESS[nextProgress] - BAR_WIDTH_PROGRESS[progress]) + tWidth;

        if (fWidth == tWidth)
        {
            return;
        }

        if (!dataReady)
        {
            dataReady = true;   //被打開時刷新
            ProgressBarImg.rectTransform.sizeDelta = new Vector2(tWidth, ProgressBarImg.rectTransform.rect.height);
            return;
        }

        ProgressBarEffect.gameObject.SetActive(isUpgrade);

        LeanTween.cancel(ProgressBarImg.gameObject);
        //更新進度條
        LeanTween.value(ProgressBarImg.gameObject, fWidth, tWidth, .6f).setEase(LeanTweenType.easeOutQuad).setOnUpdate(delegate (float val)
        {
            ProgressBarImg.rectTransform.sizeDelta = new Vector2(val, ProgressBarImg.rectTransform.rect.height);
        }).setOnComplete(delegate ()
        {
            ProgressBarEffect.gameObject.SetActive(false);
        });
    }


    private void SetA(Image img, UnitConfig cfg)
    {

        GameController.Instance.QMaxAssetsFactory.CreateUnitSprite(
            cfg, new Vector2(.5F, 0),
            delegate (Sprite s)
            {
                img.sprite = s;
                img.SetNativeSize();
            });
    }


    public float barWidth { get; set; }
}
