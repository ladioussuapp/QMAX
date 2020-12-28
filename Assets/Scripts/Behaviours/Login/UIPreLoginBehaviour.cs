using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 在登錄場景，做登錄前處理的事務處理。包括檢查更新資源更新、顯示公告
/// </summary>
public class UIPreLoginBehaviour : MonoBehaviour
{
    /// <summary>
    /// “點擊切換賬號按鈕”
    /// </summary>
    public Button SwtichAccountBtn;

    public UIButtonBehaviour BG;

    /// <summary>
    /// 開始遊戲按鈕
    /// </summary>
    public UIButtonBehaviour StartButton;

    public Transform StartText;

    public Text QQMsgText;
    public RectTransform UICanvas;
    public RectTransform UIUpgrade;

    public Transform BackgroundAudio;

    public StartMovieBehaviour StartMovieBeh;

    public Text VerText;

    public RectTransform ProgressBar;
    public Text UpgradeTextField;
    public Image ProgressBg;
    public Text PrecentText;



    private bool isAnimation = true;
    private UIBaseLoginBehaviour loginBeh;
    private bool IsAlphaAdd = true;
    private AssetBundleManager assetBundleMrg;
    private CanvasGroup textCanvasGroup;

    public void Start()
    {
        if (VerText != null)
            VerText.text = "v" + PackageConfig.Version;

        assetBundleMrg = GameController.Instance.AssetBundleMrg;
        if (PackageConfig.BASE_LOGIN)
        {
            loginBeh = gameObject.AddComponent<UIEditorLoginBehaviour>();
        }
        else//DOSDK
        {
            loginBeh = gameObject.AddComponent<UISDKLoginBehaviour>();
        }

        GameController.Instance.ModelEventSystem.OnStageListDataInit += OnStageListInit;
        GameController.Instance.AudioManager.CheckMusicCache();

        InitUI();
        LoadNotice();
        WaitAnimCompleteToShowUI();


        //這裡不應該出現引導
        GameObject guideObject = GameObject.Find("GuideLayerCanvasMode(Clone)");
        if (guideObject != null)
        {
            Destroy(guideObject);
        }
    }

    /// <summary>
    /// 檢查更新
    /// </summary>
    private void CheckUpdate()
    {
        UICanvas.gameObject.SetActive(false);
        UIUpgrade.gameObject.SetActive(true);
        UpdateState(0);
        StartLoad();
    }


    /// <summary>
    /// 根據不同情況，初始化UI
    /// </summary>
    private void InitUI()
    {
        UICanvas.gameObject.SetActive(false);
        UIUpgrade.gameObject.SetActive(false);
        //之前是否有過登錄記錄
        bool hasLoginAccount =
            PlayerPrefsTools.HasKey(OnOff.Account) &&
            !PlayerPrefsTools.GetStringValue(OnOff.Account).Equals("");

        if (hasLoginAccount)
        {
            BG.onClick += OnClickBG;
            StartText.gameObject.SetActive(true);

            textCanvasGroup = StartText.GetComponent<CanvasGroup>();
            StartButton.gameObject.SetActive(false);
        }
        else
        {
            StartButton.gameObject.SetActive(true);
            StartText.gameObject.SetActive(false);
            StartButton.onClick += OnClickBG;
        }
        

#if UNITY_ANDROID_DOSDK || UNITY_IPHONE_DOSDK
        SwtichAccountBtn.gameObject.SetActive(false);
#endif

#if UNITY_EDITOR
        SwtichAccountBtn.gameObject.SetActive(true);
#endif
        SwtichAccountBtn.onClick.AddListener(OnSwitchAccountClick);
    }


    /// <summary>
    /// 等待初始動畫完成，顯示UI
    /// </summary>
    private void WaitAnimCompleteToShowUI()
    {
        isAnimation = true;
        Animator anitor = gameObject.GetComponent<Animator>();
        Action<Animator, AnimatorStateInfo, int> AnimStateExitEvent = null;
        AnimStateExitEvent = delegate(Animator arg1, AnimatorStateInfo arg2, int arg3)
        {
            if (!arg2.IsName("Start"))
                return;
            isAnimation = false;

            CheckUpdate();

            arg1.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= AnimStateExitEvent;
        };

        anitor.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += AnimStateExitEvent;
    }


    /// <summary>
    /// 更新檢查完成，開始登錄
    /// </summary>
    private void StartLogin()
    {
        QQMsgText.text = string.Format("{0}484099420", "加Q群送大禮：");
        UICanvas.gameObject.SetActive(true);
        UIUpgrade.gameObject.SetActive(false);
        loginBeh.InitLogin();
    }

    private void OnClickBG(UIButtonBehaviour button)
    {
        if (!isAnimation)
        {
            GameController.Instance.AudioManager.PlayMusic("Opening", true);
            loginBeh.OnStartClick();
        }
    }

    private void OnSwitchAccountClick()
    {
        if (!isAnimation)
        {
            loginBeh.OnClickSwitchAcount();
        }
    }


    private void LoadNotice()
    {
        GameController.Instance.Popup.ShowLightLoading();
        GameController.Instance.LoginCtr.ReqNotice(
            (bool success, List<NoticeInfo> notices) =>
            {
                GameController.Instance.Popup.HideLightLoading();
                if (success && notices.Count > 0)
                {
                    var win = GameController.Instance.Popup.Open(PopupID.UINoticeWindow, null, true, true).GetComponent<UINoticeWindow>();
                    win.SetData(notices);
                }
                else
                {
                    //拉取公告失敗不影響流程
                    Q.Log("拉取公告失敗.");
                }
            });
    }


    void LoadNoticeOnPopClose(PopupID id)
    {
        if (id == PopupID.UIReconnect)
        {
            GameController.Instance.Popup.OnCloseComplete -= LoadNoticeOnPopClose;

            LoadNotice();
        }
    }


    private void OnStageListInit()
    {
        //登錄成功並且玩家數據初始化完成
        EnterGame();
    }


    private void EnterGame()
    {
        if (BackgroundAudio != null)
        {
            BackgroundAudio.gameObject.SetActive(false);
        }

        GuideManager.getInstance().FindRightStep();
        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                {
                    //進入遊戲  
                    if (!GuideManager.getInstance().isOpenGuide
                        || GuideManager.getInstance().IsGuideOver()
                        || GameController.Instance.PlayerCtr.PlayerData.passStageId >= 1)
                    {
                        GameController.Instance.SceneCtr.LoadLevel(Scenes.MapScene, null, true, true);
                    }
                    //直接進入戰鬥(一定是進入第一關)
                    else if (GameController.Instance.PlayerCtr.PlayerData.passStageId <= 0)
                    {
                        //開啟過場動畫//
                        StartMovieBeh.gameObject.SetActive(true);
                        StartMovieBeh.Play();
                        StartMovieBeh.OverEvent += delegate()
                        {
                            EnterLevelOne();
                        };

                    }
                }
                break;
            default:
                break;
        }

#if UNITY_EDITOR
        GameController gc = GameController.Instance;
        ///進入遊戲前檢查一下是否配置有問題
        foreach (var sta in gc.Model.StageConfigs)
        {
            Com4Love.Qmax.Net.Protocols.Stage.Stage stage = gc.StageCtr.GetStageData(sta.Value.ID);


            ///沒有該關卡信息//
            if (stage == null)
                continue;

            List<Com4Love.Qmax.Data.Config.StageConfig.Goal> goals = BattleTools.ParseGoal(stage.targets[1]);

            foreach (var go in goals)
            {
                if (go.Type == BattleGoal.Unit)
                    Q.Assert(go.Num <= sta.Value.MonsterUnitID.Length, "--關卡{0}怪物目標比配置關卡怪物多", sta.Value.ID);
            }
        }
#endif

    }

    void EnterLevelOne()
    {
        Dictionary<string, object> sceneData = new Dictionary<string, object>();
        List<int> units = new List<int>();
        units.Add(3101);//Panda

        sceneData.Add("lvl", 1);
        sceneData.Add("units", units);
        sceneData.Add("guideFlag", true);
        GameController.Instance.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData, true, true);
        ///所有使用的被動道具數量///
        Dictionary<int, int> usegoods = new Dictionary<int, int>();
        GameController.Instance.StageCtr.BeginStage(1, units, usegoods);
    }



    public void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnStageListDataInit -= OnStageListInit;
    }

    void Update()
    {
        ChangeTextAlpha();
    }
    //字體閃爍效果
    void ChangeTextAlpha()
    {
        if (textCanvasGroup != null)
        {

            float speed = 1.1f * Time.deltaTime;

            if (textCanvasGroup.alpha >= 1)
            {
                IsAlphaAdd = false;
            }
            else if (textCanvasGroup.alpha <= 0)
            {
                IsAlphaAdd = true;
            }

            speed = IsAlphaAdd ? speed : -speed;

            textCanvasGroup.alpha += speed;

        }
    }




    ///---------------------------------------CheckUpgrade----------------------------------
    ///
    private void StartLoad()
    {
        //ProgressBar.value = 0;
        Vector2 size = ProgressBar.sizeDelta;
        Vector2 oldSize = size;
        float width = size.x;
        ProgressBar.sizeDelta = new Vector2(0, oldSize.y);

        //int total = 0;
        UpgradeTextField.text = "檢測更新資源";
        float loaded = 0;
        Action<int, AssetBundleManager.Code, string, AssetBundle, float> loadOneCallback =
            delegate(int index, AssetBundleManager.Code code, string abName, AssetBundle assetBundle, float assetSize)
            {

                loaded += assetSize;
                //UpgradeTextField.text = string.Format("正在更新素材...({0}/{1})", index, total);
                UpgradeTextField.text = string.Format("正在更新...");
                float precent = (100 * loaded) / assetBundleMrg.TotalUpgradeSize;
                //size.x = width * (float)index / total;
                size.x = width * (float)loaded / assetBundleMrg.TotalUpgradeSize;
                ProgressBar.sizeDelta = size;

                PrecentText.text = string.Format("{0}%", Convert.ToInt16(precent));
                if (code != AssetBundleManager.Code.Success)
                {
                    Q.Log("Load {0} Fail", abName);
                    return;
                }

                Q.Log("Load {0} complete", abName);
            };

        Action<List<string>, List<AssetBundleManager.Code>> allComplete =
            delegate(List<string> arg0, List<AssetBundleManager.Code> arg1)
            {
                Q.Log("全部加載完成");
                UpgradeTextField.text = "全部加載完成";
                AllDone();
            };

        assetBundleMrg.CheckAssetStatus(
            delegate(AssetBundleManager.Code code)
            {
                Q.Log("check complete {0}", code);

                if (assetBundleMrg.NeedUpdateAssetCount == 0)
                {
                    UpgradeTextField.text = "檢測更新資源";
                    ProgressBar.sizeDelta = oldSize;
                    Invoke("AllDone", 0.5f);
                    return;
                }

                Transform trf = GameController.Instance.Popup.Open(PopupID.UIHotUpdate);
                UIHotUpdateBehaviour uihotBeh = trf.GetComponent<UIHotUpdateBehaviour>();
                uihotBeh.SetData(assetBundleMrg.TotalUpgradeSize, delegate(int result)
                {
                    if (result == 1)
                    {
                        Q.Log("更新資源數量{0}", assetBundleMrg.NeedUpdateAssetCount);
                        UpdateState(1);
                        assetBundleMrg.UpdateAll();
                    }
                    else
                    {
                        GameController.Instance.QuitGame();
                        //Invoke("AllDone", 0.5f);
                    }
                });

                //total = assetBundleMrg.NeedUpdateAssetCount;
                assetBundleMrg.UpdateProgressEvent += loadOneCallback;
                assetBundleMrg.UpdateCompleteEvent += allComplete;
            }
        );
    }

    void UpdateState(int flag)
    {
        switch (flag)
        {
            case 0:
                UpgradeTextField.gameObject.SetActive(true);
                ProgressBar.gameObject.SetActive(false);
                ProgressBg.gameObject.SetActive(false);
                PrecentText.gameObject.SetActive(false);
                break;
            case 1:
                UpgradeTextField.gameObject.SetActive(true);
                ProgressBar.gameObject.SetActive(true);
                ProgressBg.gameObject.SetActive(true);
                PrecentText.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }
    private void AllDone()
    {
        GameController.Instance.Model.LoadConfigs(delegate(bool result)
        {
            if (!result)
            {
                return;
            }

            StartLogin();
        });
    }
}
