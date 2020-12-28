using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax;
using UnityEngine.UI;

public class AchieveItemBehaviour : MonoBehaviour {

    private AchieveItemData _data;

    public Image Receive;
    public Image ButtonImg;
    public UIButtonBehaviour Button;

    public Sprite BsComm;
    public Sprite BsDisble;

    public AchieveItemData Data
    {
        get { return _data; }
        set
        {
            _data = value;
            _data.Beh = this;
            SetRewardIcon(_data.RewardIcon, _data.IconScale);
            SetRewardCount(_data.RewardCount);
            SetAchieveDesc(_data.AchieveDesc);
            SetAchieveTarget(_data.Target);
            SetAchieveProgress(_data.Progress);
            SetStatus(_data.Status);
        }
    }

    void Awake()
    {
        if (Button != null)
            Button.onClick += OnClick;
    }

    /// <summary>
    /// 設置獎勵圖標
    /// </summary>
    /// <param name="iconName"></param>
    public void SetRewardIcon(string iconName, float iconScale)
    {
        Transform tf = transform.Find("IconImage");
        if (tf == null)
            return;

        AtlasManager atlasMgr = GameController.Instance.AtlasManager;
        Sprite icon_sprite = atlasMgr.GetSprite(Atlas.UIComponent, iconName);

        if (icon_sprite != null)
        {
            Image img = tf.GetComponent<Image>();
            img.sprite = icon_sprite;
            img.SetNativeSize();

            tf.localScale = new Vector3(iconScale, iconScale, 1);
        }
    }

    /// <summary>
    /// 設置獎勵數量
    /// </summary>
    /// <param name="count"></param>
    public void SetRewardCount(int count)
    {
        Transform tf = transform.Find("RewardTxt");
        if (tf == null)
            return;

        Text txt = tf.GetComponent<Text>();
        txt.text = "x" + count;
        txt.SetNativeSize();
    }

    /// <summary>
    /// 設置成就描述
    /// </summary>
    /// <param name="desc"></param>
    public void SetAchieveDesc(string desc)
    {
        Transform tf = transform.Find("TitleTxt");
        if (tf == null)
            return;

        Text txt = tf.GetComponent<Text>();
        txt.text = desc;
        txt.SetNativeSize();
    }

    /// <summary>
    /// 設置成就目標
    /// </summary>
    /// <param name="target"></param>
    public void SetAchieveTarget(int target)
    {
        Transform progress = transform.Find("Progress");

        // 進度條max value
        Transform slitf = progress.Find("Slider");
        Slider slider = slitf.GetComponent<Slider>();
        slider.maxValue = target;

        // 進度條上的文字
        Transform protf = progress.Find("Text");
        Text pro = protf.GetComponent<Text>();
        pro.text = slider.minValue + "/" + target;
        pro.SetNativeSize();
    }

    /// <summary>
    /// 設置成就進度
    /// </summary>
    /// <param name="progress"></param>
    public void SetAchieveProgress(int pro)
    {
        Transform progress = transform.Find("Progress");

        // 進度條min value
        Transform slitf = progress.Find("Slider");
        Slider slider = slitf.GetComponent<Slider>();
        slider.value = pro;

        // 進度條上的文字
        Transform protf = progress.Find("Text");
        Text txt = protf.GetComponent<Text>();
        txt.text = pro + "/" + slider.maxValue;
        txt.SetNativeSize();
    }

    /// <summary>
    /// 設置狀態
    /// </summary>
    /// <param name="state"></param>
    public void SetStatus(int state)
    {
        //(1-未達成，2-達成，3-已領取)
        if (state == 1)
        {
            Button.transform.gameObject.SetActive(true);
            Receive.transform.gameObject.SetActive(false);
            Button.interactable = false;
            ButtonImg.sprite = BsDisble;
            ButtonImg.SetNativeSize();
        }
        else if (state == 2)
        {
            Button.transform.gameObject.SetActive(true);
            Receive.transform.gameObject.SetActive(false);
            Button.interactable = true;
            ButtonImg.sprite = BsComm;
            ButtonImg.SetNativeSize();
        }
        else if (state == 3)
        {
            Button.transform.gameObject.SetActive(false);
            Receive.transform.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 點擊了按鈕
    /// </summary>
    /// <param name="button"></param>
    private void OnClick(UIButtonBehaviour button)
    {
        if (_data == null)
            return;
        // 領取獎勵請求
        //GameController.Instance.AudioManager.PlayAudio("SD_ui_7day_open");
        GameController.Instance.AudioManager.PlayAudio("Vo_achieve_click");
        GameController.Instance.Client.RewardAchieve(_data.AchieveId);
    }

    public void OnDestroy()
    {
        if (Button != null)
            Button.onClick -= OnClick;
    }
}
