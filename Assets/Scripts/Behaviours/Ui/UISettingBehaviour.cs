using Com4Love.Qmax;
using UnityEngine;
using UnityEngine.UI;

public class UISettingBehaviour : MonoBehaviour
{

    public UIButtonBehaviour ButtonExchangeCode;
    //public UIButtonBehaviour ButtonDailyReward;
    public UIButtonBehaviour ButtonMessagePushSwitch;
    //public UIButtonBehaviour ButtonProduction;
    public UIButtonBehaviour ButtonLanguage;
    //public UIButtonBehaviour ButtonBulletin;
    public UIButtonBehaviour ButtonFeedback;

    public UIButtonBehaviour ButtonClose;
    public UIButtonBehaviour ButtonMSwitch;
    public UIButtonBehaviour ButtonSSwitch;

    public VerticalLayoutGroup ButtonCanvas;

    private GameObject MSwitchCloseImage;
    private GameObject SSwitchCloseImage;
    private GameObject MessagePushSwitchCloseImage;
    void Awake()
    {
        MSwitchCloseImage = ButtonMSwitch.transform.Find("ButtonOK/Image_Close").gameObject;
        SSwitchCloseImage = ButtonSSwitch.transform.Find("ButtonOK/Image_Close").gameObject;
        MessagePushSwitchCloseImage = ButtonMessagePushSwitch.transform.Find("ButtonOK/Image_Close").gameObject;

        //iOS平台審核時，不允許出現兌換碼
        if (Application.platform == RuntimePlatform.OSXEditor ||
           Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ButtonExchangeCode.gameObject.SetActive(false);
        }

    }

    void Start()
    {
        MSwitchCloseImage.SetActive(!GameController.Instance.AudioManager.MusicSwitch);
        SSwitchCloseImage.SetActive(!GameController.Instance.AudioManager.SoundSwitch);
        MessagePushSwitchCloseImage.SetActive(!GameController.Instance.Model.IsCanMessagePush);

//         ButtonDailyReward.interactable = false;
//         ButtonBulletin.interactable = false;
//         ButtonProduction.interactable = false;

        ButtonMSwitch.onClick += OnMSwitchClick;
        ButtonSSwitch.onClick += OnSSwitchClick;
        ButtonMessagePushSwitch.onClick += OnMessagePushSwitchClick;
        ButtonClose.onClick += OnCloseClick;
        ButtonExchangeCode.onClick += OnExchangeCodeClick;
//        ButtonProduction.onClick += OnProductionClick;
        ButtonLanguage.onClick += OnLanguageClick;
        ButtonFeedback.onClick += ButtonFeedback_onClick;

    }

    private void ButtonFeedback_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UISetting, false);
        UIMail uiMail = GameController.Instance.Popup.Open(PopupID.UIMail, null, true, true).GetComponent<UIMail>();
        uiMail.Caller = UIMail.CallerEnum.UISetting;
    }

    public void OnDestroy()
    {
        ButtonMSwitch.onClick -= OnMSwitchClick;
        ButtonSSwitch.onClick -= OnSSwitchClick;
        ButtonMessagePushSwitch.onClick -= OnMessagePushSwitchClick;
        ButtonClose.onClick -= OnCloseClick;
        ButtonExchangeCode.onClick -= OnExchangeCodeClick;
//        ButtonProduction.onClick -= OnProductionClick;
        ButtonLanguage.onClick -= OnLanguageClick;
    }

    void OnCloseClick(UIButtonBehaviour go)
    {
        GameController.Instance.Popup.Close(PopupID.UISetting);
    }

    void OnMessagePushSwitchClick(UIButtonBehaviour go)
    {
        GameController.Instance.Model.IsCanMessagePush = !GameController.Instance.Model.IsCanMessagePush;
        MessagePushSwitchCloseImage.SetActive(!GameController.Instance.Model.IsCanMessagePush);
    }

    void OnProductionClick(UIButtonBehaviour go)
    {

    }
    void OnLanguageClick(UIButtonBehaviour go)
    {
        GameController.Instance.Popup.Close(PopupID.UISetting, false);
        GameController.Instance.Popup.Open(PopupID.UILanguage, null, true, true);

    }

    void OnExchangeCodeClick(UIButtonBehaviour go)
    {
        GameController.Instance.Popup.Close(PopupID.UISetting, false);
        GameController.Instance.Popup.Open(PopupID.UIExchangeCode, null, true, true);
    }

    void OnMSwitchClick(UIButtonBehaviour go)
    {
        GameController.Instance.AudioManager.MusicSwitch = !GameController.Instance.AudioManager.MusicSwitch;
        GameController.Instance.AudioManager.SaveMusicLocal();
        MSwitchCloseImage.SetActive(!GameController.Instance.AudioManager.MusicSwitch);
    }
    void OnSSwitchClick(UIButtonBehaviour go)
    {
        GameController.Instance.AudioManager.SoundSwitch = !GameController.Instance.AudioManager.SoundSwitch;
        GameController.Instance.AudioManager.SaveMusicLocal();
        SSwitchCloseImage.SetActive(!GameController.Instance.AudioManager.SoundSwitch);
    }

}
