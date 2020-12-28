using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UIAlertBehaviour : MonoBehaviour
{
    const string BODY_SPRITE_PATH = "Textures/UIAlert/{0}";
    const string BODY_PREFAB_PATH = "Prefabs/Ui/UIAlert/{0}";

    public event Action<UIButtonBehaviour> OnClickOKButton;
    public event Action<UIButtonBehaviour> OnClickCacelButton;

    public Text TextInfo;
    public UIButtonBehaviour ButtonOK;
    public UIButtonBehaviour ButtonCancel;
    public RectTransform ButtonContainer;

    public Image BodyImg;       //内容有可能是一张图片有可能是一个prefab
    public RectTransform TitleContainer;
    public Image TitleImg;     //标题有可能是图片也可能是文字
    public Text TitleText;
    public RectTransform DisplayNoMoreElement;

    /// <summary>
    /// 不在提示按钮
    /// </summary>
    public Toggle DisplayNoMoreToggle;  

    RectTransform bodyContent;   //加载进来的bodycontent
 
    public enum ButtonStates:byte
    {
        ButtonOk = 1,
        ButtonCancel = 2,
        DisplayNoMoreToggle = 4
    }

    /// <summary>
    /// 获取加载进来的bodyprefab
    /// </summary>
    /// <returns></returns>
    public RectTransform GetBodyContent()
    {
        return bodyContent;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type">0隐藏 1文字</param>
    public void SetInfo(string value, byte type)
    {
        TextInfo.gameObject.SetActive(type != 0);

        TextInfo.text = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key">图片的名称或者prefab的名称。  
    /// </param>
    /// <param name="type">0隐藏  1 sprite  2 prefab</param>
    public void SetBody(string key, byte type)
    {
        if (key != "")
        {
            switch (type)
            {
                case 1:
                    BodyImg.gameObject.SetActive(true);
                    Sprite sprite = GameController.Instance.QMaxAssetsFactory.CreateSprite(string.Format(BODY_SPRITE_PATH , key), new Vector2(.5f, .5f));
                    BodyImg.sprite = sprite;
                    BodyImg.SetNativeSize();
                    break;
                case 2:
                    bodyContent = GameController.Instance.QMaxAssetsFactory.CreatePrefab(string.Format(BODY_PREFAB_PATH, key)) as RectTransform;
                    bodyContent.SetParent(BodyImg.transform.parent);
                    bodyContent.localScale = new Vector3(1, 1, 1);
                    bodyContent.anchoredPosition3D = Vector3.zero;
                    bodyContent.SetSiblingIndex(BodyImg.rectTransform.GetSiblingIndex());
                    BodyImg.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="type">0隐藏 1 sprite(文字的图片放在UIComponet中) 2 文字</param>
    public void SetTitle(string key, byte type)
    {
        TitleContainer.gameObject.SetActive(type != 0);

        if (key != "")
        {
            switch (type)
            {
                case 0:
                    TitleImg.gameObject.SetActive(false);
                    TitleText.gameObject.SetActive(false);
                    break;
                case 1:
                    Sprite sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, key);
                    TitleImg.sprite = sprite;
                    TitleImg.SetNativeSize();
                    break;
                case 2:
                    TitleImg.gameObject.SetActive(false);
                    TitleText.text = key;
                    break;
            }
        }
    }

    /// <summary>
    /// 可以这么用  比如想显示所有按钮  可以传入： (byte)ButtonStates.ButtonOk | (byte)ButtonStates.ButtonCancel | (byte)ButtonStates.DisplayNoMoreToggle
    /// </summary>
    /// <param name="state"></param>
    public void SetButton(byte state)
    {
        bool okVisble = (state & (byte)ButtonStates.ButtonOk) == (byte)ButtonStates.ButtonOk;
        bool cancelVisble = (state & (byte)ButtonStates.ButtonCancel) == (byte)ButtonStates.ButtonCancel;
        bool dToggle = (state & (byte)ButtonStates.DisplayNoMoreToggle) == (byte)ButtonStates.DisplayNoMoreToggle;

        ButtonOK.gameObject.SetActive(okVisble);
        ButtonCancel.gameObject.SetActive(cancelVisble);
        DisplayNoMoreElement.gameObject.SetActive(dToggle);
        ButtonContainer.gameObject.SetActive(okVisble || cancelVisble);
    }
 
    void Start()
    {
        ButtonOK.onClick += ButtonOK_onClick;
        ButtonCancel.onClick += ButtonCancel_onClick;
        DisplayNoMoreToggle.isOn = false;
    }

    public void OnDestroy()
    {
        ButtonOK.onClick -= ButtonOK_onClick;
        ButtonCancel.onClick -= ButtonCancel_onClick;

        OnClickOKButton = OnClickCacelButton = null;
    }
 
    /// <summary>
    /// prefab默认路径放在Resource/Prefabs/Ui/UIAlert/ 下。   
    /// 图片默认路径放在Resource/Textures/UIAlert/
    /// alert是否允许弹出多次？？？？？？
    /// </summary>
    /// <param name="title">标题  可以是文字  也可以是图片的路径 </param>
    /// <param name="body">可以是图片可以是prefab</param>
    /// <param name="info">文字内容</param>
    /// <param name="titleType">0隐藏 1 sprite(文字的图片放在UIComponet中) 2 文字 </param>
    /// <param name="bodyType">0隐藏 1 sprite(打包到UIAlert中)  2 prefab(基于Resource/Prefabs/Ui/UIAlert/路径下)</param>
    /// <param name="infoType">0隐藏 1 文字</param>
    /// <param name="buttonState">类似这种： (byte)ButtonStates.ButtonOk | (byte)ButtonStates.ButtonCancel | (byte)ButtonStates.DisplayNoMoreToggle</param>
    /// <returns></returns>
    public static UIAlertBehaviour Alert(string title = "", string body = "", string info = "", byte titleType = 0, byte bodyType = 0, byte infoType = 0, byte buttonState = (byte)ButtonStates.ButtonOk)
    {
        UIAlertBehaviour UIAlert = GameController.Instance.Popup.Open(PopupID.UIAlert, null, true, true).GetComponent<UIAlertBehaviour>();
        UIAlert.SetTitle(title, titleType);
        UIAlert.SetBody(body, bodyType);
        UIAlert.SetInfo(info , infoType);
        UIAlert.SetButton(buttonState);

        return UIAlert;
    }
 
    void ButtonOK_onClick(UIButtonBehaviour button)
    {
        if (OnClickOKButton != null)
        {
            OnClickOKButton(button);
        }

        GameController.Instance.Popup.Close(PopupID.UIAlert, false);
    }

    void ButtonCancel_onClick(UIButtonBehaviour button)
    {
        if (OnClickCacelButton != null)
        {
            OnClickCacelButton(button);
        }

        GameController.Instance.Popup.Close(PopupID.UIAlert, false);
    }
}
