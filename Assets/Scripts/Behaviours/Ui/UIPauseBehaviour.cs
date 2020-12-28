using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax;

public class UIPauseBehaviour : MonoBehaviour
{

    public UIButtonBehaviour ButtonClose;
    public UIButtonBehaviour ButtonContinue;
    public UIButtonBehaviour ButtonRestart;
    public UIButtonBehaviour ButtonGiveUp;
    public UIButtonBehaviour ButtonMSwitch;
    public UIButtonBehaviour ButtonSSwitch;
    public Text EnergyNum;
    public Text TitleText;
    private GameObject MSwitchCloseImage;
    private GameObject SSwitchCloseImage;
    [HideInInspector]
    public bool IsCheckUnit;
    [HideInInspector]
    public bool IsCheckProp;

    public Text CheckPropText;

    public UIButtonBehaviour ButtonCheckUnit;
    public UIButtonBehaviour ButtonCheckProp;
    public Transform TickCheckUnit;
    public Transform TickCheckProp;
    void Awake()
    {
        MSwitchCloseImage = ButtonMSwitch.transform.Find("ButtonOK/Image_Close").gameObject;
        SSwitchCloseImage = ButtonSSwitch.transform.Find("ButtonOK/Image_Close").gameObject;
    }

    void Start()
    {
        MSwitchCloseImage.SetActive(!GameController.Instance.AudioManager.MusicSwitch);
        SSwitchCloseImage.SetActive(!GameController.Instance.AudioManager.SoundSwitch);

        ButtonMSwitch.onClick += OnMSwitchClick;
        ButtonSSwitch.onClick += OnSSwitchClick;

        ButtonCheckUnit.onClick += OnClickUnit;
        ButtonCheckProp.onClick += OnClickProp;

        StageConfig conf = GameController.Instance.Model.BattleModel.CrtStageConfig;
        string title = Utils.GetTextByStringID(conf.NameStringID);
        TitleText.text = title;
        ButtonRestart.interactable = !GameController.Instance.StageCtr.IsActivityStage(conf.ID);
        EnergyNum.text = conf.CostEnergy.ToString();

        IsCheckProp = PlayerPrefsTools.GetBoolValue(OnOff.PauseCheckProp,true);
        IsCheckUnit = PlayerPrefsTools.GetBoolValue(OnOff.PauseCheckUnit,true);

        TickCheckUnit.gameObject.SetActive(IsCheckUnit);
        TickCheckProp.gameObject.SetActive(IsCheckProp);
    }

    void OnClickUnit(UIButtonBehaviour button)
    {
        IsCheckUnit = !IsCheckUnit;
        TickCheckUnit.gameObject.SetActive(IsCheckUnit);

        PlayerPrefsTools.SetBoolValue(OnOff.PauseCheckUnit, IsCheckUnit, true);
    }

    void OnClickProp(UIButtonBehaviour button)
    {
        IsCheckProp = !IsCheckProp;
        TickCheckProp.gameObject.SetActive(IsCheckProp);
        PlayerPrefsTools.SetBoolValue(OnOff.PauseCheckProp, IsCheckProp, true);
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


    void SetTickActive(GameObject tick,bool active)
    {
        if (tick != null)
            tick.SetActive(active);
    }

    public void ChangeCheckPropButton( bool ac)
    {
        ButtonCheckProp.interactable = ac;
        CheckPropText.color = ac ? Color.white : Color.gray;
    }

}
