using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax;

public class UILanguageBehaviour : MonoBehaviour
{
    public UIButtonBehaviour ButtonClose;

    public UIButtonBehaviour ButtonEnglish;
    public UIButtonBehaviour ButtonChineseSimplified;
    public UIButtonBehaviour ButtonChineseTraditional;

    private GameObject EnglishImage;
    private GameObject ChineseSimplifiedImage;
    private GameObject ChineseTraditionalImage;

    void Awake()
    {
        EnglishImage = ButtonEnglish.transform.Find("ButtonOK/Image_Icon").gameObject;
        ChineseSimplifiedImage = ButtonChineseSimplified.transform.Find("ButtonOK/Image_Icon").gameObject;
        ChineseTraditionalImage = ButtonChineseTraditional.transform.Find("ButtonOK/Image_Icon").gameObject;

        GameController.Instance.ModelEventSystem.OnLanguageChange += SetLanguage;
    }

    void Start()
    {
        ButtonClose.onClick += OnCloseClick;

        ButtonEnglish.onClick += OnLanguageClick;
        ButtonChineseSimplified.onClick += OnLanguageClick;
        ButtonChineseTraditional.onClick += OnLanguageClick;
        SetLanguage(GameController.Instance.Model.Language);

        Color grayColor = new Color(0.3686f, 0.3686f, 0.3686f);

        Outline outline;

        if (!ButtonEnglish.interactable)
        {
            outline = ButtonEnglish.GetComponentInChildren<Outline>();
            if (outline != null)
                outline.effectColor = grayColor;
        }

        if (!ButtonChineseSimplified.interactable)
        {
            outline = ButtonChineseSimplified.GetComponentInChildren<Outline>();
            if (outline != null)
                outline.effectColor = grayColor;
        }
        if (!ButtonChineseTraditional.interactable)
        {
            outline = ButtonChineseTraditional.GetComponentInChildren<Outline>();
            if (outline != null)
                outline.effectColor = grayColor;
        }

    }

    void SetLanguage(Language language)
    {
        EnglishImage.SetActive(false);
        ChineseSimplifiedImage.SetActive(false);
        ChineseTraditionalImage.SetActive(false);
        switch (language)
        {
            case Language.English:
                EnglishImage.SetActive(true);
                break;
            case Language.ChineseSimplified:
                ChineseSimplifiedImage.SetActive(true);
                break;
            case Language.ChineseTraditional:
                ChineseTraditionalImage.SetActive(true);
                break;
        }
    }

    void OnCloseClick(UIButtonBehaviour go)
    {
        GameController.Instance.Popup.Close(PopupID.UILanguage, false);
        GameController.Instance.Popup.Open(PopupID.UISetting,null,true,true);
    }

    void OnLanguageClick(UIButtonBehaviour go)
    {
        //Debug.Log("暫時無法支持語言切換");
        //return;
        if(go == ButtonEnglish){
            GameController.Instance.Model.Language = Language.English;
        }
        else if (go == ButtonChineseSimplified)
        {
            GameController.Instance.Model.Language = Language.ChineseSimplified;
        }
        else if (ButtonChineseTraditional)
        {
            GameController.Instance.Model.Language = Language.ChineseTraditional;
        }
        Debug.Log(GameController.Instance.Model.Language);
    }
    void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnLanguageChange -= SetLanguage;
    }

}
