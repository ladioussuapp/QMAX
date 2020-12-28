using Com4Love.Qmax;
using UnityEngine;

public class BootstrapSceneBehaviour : MonoBehaviour {
    public CanvasGroup LogoGroup;

    void Awake()
    {
        LogoGroup.alpha = 0;
    }

    public void Show()
    {
        //添加了0.1秒延遲 ResolutionBehaviour 需要1幀的黑屏去切換分辨率
        LeanTween.value(LogoGroup.gameObject, 0f, 1f, 1.5f).setOnUpdate(delegate(float val)
        {
            LogoGroup.alpha = val;
        }).setDelay(0.1f).setOnComplete(delegate()
        {
            Invoke("Hide", .5f);
        });
    }

    bool isInitFail = false;
    public void initSDKFailShow()
    {
        isInitFail = true;
        Show();
    }


    void Hide()
    {
        if (isInitFail)
        {
            if (!GameController.Instance.Popup.IsPopup(PopupID.UIReconnect))
            {
                Transform rect = GameController.Instance.Popup.Open(PopupID.UIReconnect);
                UIReconnectBehaviour beh = rect.GetComponent<UIReconnectBehaviour>();
                if (beh != null)
                {
                    string msg = string.Format("網絡異常\n請重新登錄");
                    //beh.setWindowData(msg, "UIWIFI_0010", "SingleAnima");
                    beh.setWindowData(msg, 68, "SingleAnima");
                    beh.ButtonOK.onClick.AddListener(delegate()
                    {
                        Application.Quit();
                    });
                }
            }
        }
        else
        {
            LeanTween.value(LogoGroup.gameObject, 1f, 0f, .5f).setOnUpdate(delegate(float val)
            {
                LogoGroup.alpha = val;
            }).setOnComplete(delegate()
            {
                Invoke("GoToLogin", .2f);
            });
        }
    }

    void GoToLogin()
    {
        //GameController.Instance.SceneCtr.LoadLevel(Scenes.UpdateAssetsScene, null, false);
        GameController.Instance.SceneCtr.LoadLevel(Scenes.LoginScene,null, false);
    }
}
