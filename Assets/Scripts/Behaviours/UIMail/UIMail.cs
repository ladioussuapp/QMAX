using UnityEngine;
using System.Collections;
using Com4Love.Qmax;

public class UIMail : MonoBehaviour
{
    public enum CallerEnum
    {
        UISetting
    }

    public UIButtonBehaviour CloseButton;
    public CallerEnum Caller;        //通過這個判斷是從什麼地方觸發。如果是設置界面觸發，則在關閉後打開設置界面 

    // Use this for initialization
    void Start()
    {
        CloseButton.onClick += CloseButton_onClick;
    }
    public void OnDestroy()
    {
        CloseButton.onClick -= CloseButton_onClick;

        if (Caller == CallerEnum.UISetting)
        {
            GameController.Instance.Popup.Open(PopupID.UISetting, null, true, true);
        }
    }
   
    private void CloseButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UIMail , false);
    }


}
