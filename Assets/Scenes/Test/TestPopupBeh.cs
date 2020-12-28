using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax;

public class TestPopupBeh : MonoBehaviour
{
    public Button NoticeBtn;
    public Button GemBtn;
    public Button CloseBtn;

    private Popup popup;

    // Use this for initialization
    void Start()
    {
        popup = new Popup();
        NoticeBtn.onClick.AddListener(delegate()
        {
            popup.Open(PopupID.UINoticeWindow, Camera.main, true,true);
        });


        GemBtn.onClick.AddListener(delegate()
        {
            popup.Open(PopupID.UIShop, Camera.main, true,true);
        });

        CloseBtn.onClick.AddListener(delegate()
        {
            popup.Close(popup.GetCrtPopupID());
        });

    }

}
