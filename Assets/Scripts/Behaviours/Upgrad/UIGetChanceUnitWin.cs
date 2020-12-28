using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax;
using System;
using System.Collections.Generic;

public class UIGetChanceUnitWin : PopupEventCor
{
    public Text HpText;
    public Text AtkText;
    public Image SkillImg;
    public Image AtkImg;
    public UnitNamesBar namesBar;
    public Text InfoText;
    public Transform BodyPlaceHolder;
    public Text[] CNTexts;
    public Color[] Body_BG_COLORS;
    AudioSource AudioSource;
    Animator UnitAnimator;
    public UIButtonBehaviour GradeButton;

    private string unitKeyInPool;

    public event Action<bool> OnCloseCallback;

    float BackAniTime = 2f;
    string CurrentAniName;


    string[] RadomAniName = new string[] { "Win", "Charge" };

    public override void Close()
    {
        GameController.Instance.Popup.Close(PopupID.UIGetChanceUnitWin);
        if (OnCloseCallback != null)
        {
            OnCloseCallback(true);
        }
    }

    public void SetData(Data data_)
    {
        data = data_;
        namesBar.SetDatas(data.uConfig);

        AtlasManager atlasMgr = GameController.Instance.AtlasManager;
        SkillImg.sprite = atlasMgr.GetSprite(Atlas.Tile, data.sConfig.ResourceIcon);
        SkillImg.SetNativeSize();

        string[] strs = { 
            "ElementPurple",
            "ElementRed",
            "ElementGreen",
            "ElementBlue",
            "ElementYellow"};
        AtkImg.sprite = atlasMgr.GetSprite(Atlas.Tile, strs[(int)data.uConfig.UnitColor - 1]);
        AtkImg.SetNativeSize();
        InfoText.text = GameController.Instance.UnitCtr.GetStroyStr(data.uConfig);
        AtkText.text = data.uConfig.UnitAtk.ToString();
        HpText.text = data.uConfig.UnitHp.ToString();

        ChangeBody();
    }

    void Awake()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        ChangeLanguage();
        GameController.Instance.Popup.OnOpenComplete += onOpenWindow;
        GradeButton.onClick += GradeButton_onClick;

        EventTriggerListener eventLis = EventTriggerListener.Get(BodyPlaceHolder.gameObject);
        eventLis.onClick += OnBodyClick;
    }

    void GradeButton_onClick(UIButtonBehaviour button)
    {
        //OnClose();
        Close();
    }

    private void onOpenWindow(PopupID obj)
    {
        if (obj == PopupID.UIGetChanceUnitWin)
        {
            GameController.Instance.Popup.OnOpenComplete -= onOpenWindow;
            addFliter();
        }
    }

    public void OnDestroy()
    {
        OnCloseCallback = null;

        GradeButton.onClick -= GradeButton_onClick;

        if (UnitAnimator != null)
        {
            UnitAnimator.gameObject.layer = LayerMask.NameToLayer(Layer.Default);

            GameController.Instance.PoolManager.PushToInstancePool(
                unitKeyInPool, UnitAnimator.transform);
            unitKeyInPool = null;
        }

        removeFliter();
    }
    //language相關.....12/22標記
    void ChangeLanguage()
    {
        //CNTexts
        foreach (Text text in CNTexts)
        {
            text.text = Utils.GetText(text.text);
        }
    }

    void ChangeBody()
    {
        GameController.Instance.PoolManager.GetUnitInstance(data.uConfig,
            delegate(string key, Transform ins)
            {
                RectTransform rT = ins as RectTransform;
                unitKeyInPool = key;
                if (rT != null)
                {
                    ////夥伴添加後設置parent
                    rT.SetParent(BodyPlaceHolder.parent);
                    rT.localScale = BodyPlaceHolder.localScale;
                    rT.anchorMax = (BodyPlaceHolder as RectTransform).anchorMax;
                    rT.anchorMin = (BodyPlaceHolder as RectTransform).anchorMin;
                    rT.anchoredPosition3D = (BodyPlaceHolder as RectTransform).anchoredPosition3D;
                    rT.gameObject.layer = LayerMask.NameToLayer(Layer.UI);
                    UnitAnimator = rT.GetComponent<Animator>();
                    //UnitAnimation();
                    UnitAniPlay();
                }
            }
        );
    }

    bool animationPlaying = false;

    public void UnitAnimation(string animation = "TriggerWin")
    {
        if (animationPlaying)
        {
            return;
        }

        animationPlaying = true;
        UnitAnimator.SetBool(animation, true);
        Invoke("UnitAnimationStop", 2.0f);
        //播放音樂
        GameController.Instance.QMaxAssetsFactory.CreateRandomUnitUpgradeAudio(
        data.uConfig.UnitColor, 1, delegate(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.clip = clip;
                AudioSource.Play();
            }
        });

    }

    void UnitAnimationStop()
    {
        animationPlaying = false;
        
        UnitAnimator.SetBool("TriggerIdle", true);
    }

    Data data;

    public struct Data
    {
        public UnitConfig uConfig;
        public SkillConfig sConfig;
    }

    private void addFliter()
    {
        GuideNode node = new GuideNode();
        node.TargetNode = GradeButton;
        node.TarCamera = Camera.main;
        node.index = 3;
        node.ShowMask = false;
        node.CallBack = delegate()
        {
            //OnClose();
            Close();
        };
        GuideManager.getInstance().addGuideNode("AwardPageOkBtn", node);
    }

    private void removeFliter()
    {
        GuideManager.getInstance().removeGuideNode("AwardPageOkBtn");
    }

    void Update()
    {
        if (BackAniTime > 0)
        {
            BackAniTime -= Time.deltaTime;
        }
        UnitBackAni(BackAniTime);
    }

    void UnitBackAni(float backtime)
    {
        string backName = "Idle";
        if (backName.Equals(CurrentAniName))
        {
            return;
        }
        if (backtime <= 0 && UnitAnimator)
        {
            UnitAnimator.Play(backName);
        }
    }

    void OnBodyClick(GameObject button)
    {
        //Debug.Log("--------it is click----------");
        BackAniTime = 2f;

        string RadomName = RadomAniName[UnityEngine.Random.Range(0, RadomAniName.Length)];
        UnitAniPlay(RadomName);
    }

    void UnitAniPlay(string aniName = "Win")
    {
        if (aniName.Equals(CurrentAniName))
        {
            List<string> newName = new List<string>();
            for (int name = 0; name < RadomAniName.Length; name++)
            {
                if (!RadomAniName[name].Equals(aniName))
                {
                    newName.Add(RadomAniName[name]);
                }
            }

            aniName = newName[UnityEngine.Random.Range(0, newName.Count)];
        }
        CurrentAniName = aniName;

        UnitAnimator.Play(aniName);
    }
}
