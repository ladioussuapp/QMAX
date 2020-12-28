using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UINoticeListItem : MonoBehaviour
{
    public Text TitleText; 
    public Text MsgText;
    public Image LogoImg;
 
    public void SetDatas(string title, string msg, string sprite)
    {
        TitleText.text = title;
        MsgText.text = msg;

        LogoImg.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UINotice, sprite);
        LogoImg.SetNativeSize();
    }
}


