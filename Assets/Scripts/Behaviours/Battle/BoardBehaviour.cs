using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Helper;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.TileBehaviour;
using Com4Love.Qmax.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Com4Love.Qmax.Net.Protocols.goods;
using Com4Love.Qmax.Net.Protocols.Stage;
/// <summary>
/// 棋盤的行為操控----12/23
/// </summary>
public class BoardBehaviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{


    /// <summary>
    /// 顯示tips的時間間隔
    /// </summary>
#if AUTO_FIGHT
    static public double ShowTipsInterval = 500;
#else
    static public double ShowTipsInterval = 7000;
#endif

    /// <summary>
    /// 元素閃光tips的是間間隔
    /// </summary>
    static public double LightTipsInterval = 5000;

    /// <summary>
    /// 物件下落一格所花的時間
    /// </summary>
    public float MoveSpeed = 0.3f;
    public int TestLevel = 3;

    /// <summary>
    /// 全透明的圖片用材質
    /// </summary>
    public Material BlockImgMaterial;

    /// <summary>
    /// 物件下落的速度曲線
    /// </summary>
    public LeanTweenType MoveEaseType = LeanTweenType.linear;

    /// <summary>
    /// 掉血特效
    /// </summary>
    public ImageNumberBehaviour BloodNumEffect;


    /// <summary>
    /// Combo效果
    /// </summary>
    public ComboEffBehaviour ComboEffBeh;

    public RectTransform UnitLayer;


    [HideInInspector]
    public UIPauseBehaviour UIPauseBeh;

    public GameObject LockPanel;

    /// <summary>
    /// 棋盤背景
    /// </summary>
    public Image BoardBackground;

    /// <summary>
    /// 夥伴站立的地面
    /// </summary>
    public Image UnitBackground;


    ///道具背景//
    public Image PropBG;

    /// <summary>
    /// 敵人免疫圖標
    /// </summary>
    public Image ImmuneSp;

    public Transform World;

    public List<Button> UnitButton = new List<Button>();

    bool IsUnitButtonLock = false;


    [HideInInspector]
    /// <summary>
    /// 小夥伴
    /// </summary>
    public Dictionary<ColorType, Animator> UnitAnims;

    [HideInInspector]
    /// <summary>
    /// 技能Loading圖標
    /// </summary>    
    public Dictionary<ColorType, SkillLoadingBehaviour> SkillLoadings;


    public Dictionary<ColorType, BattleUnitInfoBehaviour> BattleUnitInfo;

    [HideInInspector]
    /// <summary>
    /// 消除特效層
    /// </summary>
    public EliminateEffectBehaviour EliminateEffect;


    [HideInInspector]
    /// <summary>
    /// 受擊特效控制
    /// </summary>
    public HitEffectBehaviour HitEffect;

    [HideInInspector]
    /// <summary>
    /// 蓄力特效
    /// </summary>
    public ChargeEffectBehaviour ChargeEffectBehaviour;

    [HideInInspector]
    public BattleBehaviour CrtBattleBeh;

    [HideInInspector]
    public HitEnemyBehaviour HitEnemyBeh;

    [HideInInspector]
    public Camera BoardCamera;

    /// <summary>
    /// 當前的敵人的位置
    /// </summary>
    [HideInInspector]
    public EnemyPoint CrtEnemyPoint;

    [HideInInspector]
    public RectTransform ElementLayer;
    [HideInInspector]
    public RectTransform CollectLayer;
    [HideInInspector]
    public RectTransform ObstacleLayer;
    [HideInInspector]
    public RectTransform CoverLayer;
    [HideInInspector]
    public RectTransform SeperaterHLayer;
    [HideInInspector]
    public RectTransform SeperaterVLayer;
    [HideInInspector]
    public RectTransform BottomLayer;

    [HideInInspector]
    /// <summary>
    /// 是否是大樹活動進行中
    /// </summary>
    public bool IsTreeActivity = false;

    public TreeFightCtr TreeFightCtr;

    /// <summary>
    /// 飛行層。由於層次問題，消除物在蓄力、打擊敵人過程中，會先放到這個層，再飛向敵人。
    /// </summary>
    [HideInInspector]
    public RectTransform FlyLayer;

    /// <summary>
    /// 棋盤獎勵浮層
    /// </summary>
    public RectTransform EliminateAwardLayer;

    /// <summary>
    /// 消滅敵人獎勵浮層
    /// </summary>
    public RectTransform EnemyAwardLayer;

    /// <summary>
    /// 交互層
    /// </summary>
    private RectTransform interactLayer;

    //按下去交互層
    private RectTransform interactDownLayer;

    /// <summary>
    /// 裝飾物層
    /// </summary>
    private RectTransform decorateLayer;
    private UILinkLineBehaviour linkLine;


    private GameObject elementPrefab;
    private GameObject collectPrefab;
    private GameObject obstaclePrefab;
    private GameObject coverPrefab;
    private GameObject bottomPrefab;
    private GameObject seprHPrefab;
    private GameObject seprVPrefab;

    private PlayingRuleCtr playingRuleCtr;

    /// <summary>
    /// 棋盤上的消除物
    /// </summary>
    public GameObject[,] eleViews;
    public GameObject[,] collectViews;
    public GameObject[,] obstacleViews;
    public GameObject[,] seperatorHViews;
    public GameObject[,] seperatorVViews;
    public GameObject[,] coverViews;
    public GameObject[,] bottomViews;
    public RectTransform[,] interactRect;

    /// <summary>
    /// 待消除隊列
    /// </summary>
    private List<Position> eliminateQueue;
    /// <summary>
    /// 受影響的地形物隊列
    /// </summary>
    //private List<GameObject> afffectedQueue;

    /// <summary>
    /// Model發過來的事件隊列
    /// </summary>
    private Queue<EventArgs> eventQueue;
    /// <summary>
    /// 事件執行鎖，>0時，不往下執行事件
    /// </summary>
    private int eventLock = 0;

    /// <summary>
    /// Element的對像池
    /// </summary>
    private List<GameObject> eleGameObjPool;


    /// <summary>
    /// 小交互鎖。 > 0時，棋盤無法操作
    /// </summary>
    private int interactLock = 0;

    private LinkInteractBehaviour linkInteractBeh;

    private QmaxModel model;

    private BattleModel battleModel;

    private ModelEventSystem modelEventSystem;

    private ViewEventSystem viewEventSystem;

    /// <summary>
    /// 提示的Timer
    /// </summary>
    private Timer tipsTimer;
    /// <summary>
    /// 元素閃光提示Timer
    /// </summary>
    private Timer lightTimer;

    private bool isTimeShowTip = false;

    private int timeShowTipCount = 0;


    private GameController gameCtrl;

    /// <summary>
    /// 當前怪物的回合數
    /// </summary>
    public int RoundNum = 0;
    /// <summary>
    /// 當前怪物的總回合數
    /// </summary>
    public int TotalRoundNum = 0;

    private MonstorImmuneBehaviour ImmuneBehaviour;

    private bool isFightStep;

    private bool isFightGuide = false;

    private bool tipFlag = true;

    public RectTransform UnitPanel;

    public GameObject UIBattle;

    public GameObject UIBattleTreeActivity;

    [HideInInspector]
    /// <summary>
    /// 通常模式下的UI層
    /// </summary>
    public UIBattleBehaviour UIBattleBeh;

    [HideInInspector]
    /// <summary>
    /// 大樹活動時的UI層
    /// </summary>
    public UIBattleTreeActivityBehaviour UIBattleTreeBeh;

    /// <summary>
    /// 是否正在顯示夥伴信息面板
    /// </summary>
    private bool isShowUnitPanel;

    /// <summary>
    /// 是否正在顯示目標面板
    /// </summary>
    private bool initAnimComplete;

    private Dictionary<int, int> goalId;

    private GameObject pandaSpawnEffect;

    private int guideLinkCont;

    private bool isInLinkGuide;

    [HideInInspector]
    public bool IsGuide;

    bool shouldEnemyShow = true;

    /// <summary>
    /// 取消連線提示
    /// </summary>
    UILinkTipsHelper UILinkTipsHelp;

    //[HideInInspector]
    public bool ShouldEnemyShow
    {
        get
        {
            return shouldEnemyShow;
        }
        set
        {
            if (shouldEnemyShow != value)
            {
                shouldEnemyShow = value;

                if (viewEventSystem.OnBattleEnemyDisplayChange != null)
                {
                    viewEventSystem.OnBattleEnemyDisplayChange(shouldEnemyShow);
                }
            }
        }
    }

    [HideInInspector]
    Vector3[] BossHpPosition =
    {
        new Vector3(0,-42f,0),
        new Vector3(0,-63f,0),
        new Vector3(0,-110f,0)
    };
    [HideInInspector]
    Vector3[] ImmuneSpPosition =
    {
        new Vector3(0,557f,0),
        new Vector3(0,536f,0),
        new Vector3(0,487f,0)
    };

    /// <summary>
    /// 是否有戰鬥結果///
    /// </summary>
    private bool _IsHaveResult = false;
    private bool IsHaveResult
    {
        get { return _IsHaveResult; }
        set
        {
            _IsHaveResult = value;
            if (UIBattleBeh != null)
                UIBattleBeh.IsHaveResul = _IsHaveResult;
        }
    }


    private float guideAudioTime = 0;

    /// <summary>
    /// 消除幫助//
    /// </summary>
    EliminateElementsHelper EliminateHelper;

    /// <summary>
    /// 道具技能幫助///
    /// </summary>
    NoCDThrowSkillHelper NoCDSkillHelper;

    /// <summary>
    /// 道具按鈕預製體//
    /// </summary>
    public UIPropItem PropItemPrefab;

    /// <summary>
    /// 道具按鈕父節點///
    /// </summary>
    public Transform PropItemParent;

    /// <summary>
    /// 存儲道具按鈕的list//
    /// </summary>
    [HideInInspector]
    Dictionary<PropType, UIPropItem> ActivePropItemDic;

    private float linkUnableArea = 260f;


    private bool isGiveUp = false;

    /// <summary>
    /// 是否播放戰鬥攻擊過程
    /// </summary>
    private bool isPlayBattleAttackProcess;

    private Vector2 UpPosition;

    /// <summary>
    /// 第一個手指觸控點ID
    /// </summary>
#if UNITY_EDITOR || UNITY_STANDALONE
    int PointerID = -1;
#else
    int PointerID = 0;
#endif

    public void StartDrawLine(Position pos)
    {
        if (eleViews[pos.Row, pos.Col] != null)
            linkLine.StartDraw(eleViews[pos.Row, pos.Col].transform as RectTransform);
    }

    public void DrawToPoint(Position pos)
    {
        if (eleViews[pos.Row, pos.Col] != null)
            linkLine.DrawToPoint(eleViews[pos.Row, pos.Col].transform as RectTransform);
    }

    public void ClearDrawLine()
    {
        linkLine.Clear();
    }

    /// <summary>
    /// 交互鎖+1
    /// </summary>
    public void PlusInteractLock(bool useBlackPanel = false, bool useWeakenElement = true)
    {
        interactLock++;

        linkInteractBeh.PlusInteractLock();

        if (tipsTimer.Enabled)
        {
            //Q.Log("stop timer");
            tipsTimer.Enabled = false;
            tipsTimer.Stop();
        }

        if (useBlackPanel)
            SetLockPanelVDisplay(true);

        if (!isWeakenElement && useWeakenElement)
            SetWeakenElement(true);

        SetPropButtonActive(false);
    }

    /// <summary>
    /// 交互鎖-1
    /// </summary>
    public void MinusInteractLock()
    {
        interactLock--;

        linkInteractBeh.MinusInteractLock();
        if (interactLock == 0)
        {
            if (isWeakenElement)
                SetWeakenElement(false);

            if (!tipsTimer.Enabled)
            {
                //Q.Log("start timer");
                tipsTimer.Enabled = true;
                tipsTimer.Stop();
                tipsTimer.Start();
            }
            SetPropButtonActive(true);
            SetLockPanelVDisplay(false);
        }
    }

    /// <summary>
    /// 設置元素顯示弱化
    /// </summary>
    /// <param name="isWeaken"></param>
    private bool isWeakenElement;
    private void SetWeakenElement(bool isWeaken)
    {
        if (!initAnimComplete)
            return;

        isWeakenElement = isWeaken;
        for (int r = 0, n = eleViews.GetLength(0); r < n; r++)
        {
            for (int c = 0, m = eleViews.GetLength(0); c < m; c++)
            {
                if (eleViews[r, c] == null)
                    continue;

                BaseTileBehaviour beh = eleViews[r, c].GetComponent<BaseTileBehaviour>();
                Q.Assert(beh.Data != null, "{0}", beh.gameObject.name);

                beh.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                if (!isWeaken)
                {
                    LeanTween.cancel(beh.gameObject);
                    LeanTween.value(beh.gameObject, beh.GetComponent<Image>().color.a, 1.0f, 0.1f)
                    .setOnUpdate(delegate(float val)
                    {
                        beh.GetComponent<Image>().color = new Color(1, 1, 1, val);
                    });
                }
                else
                {
                    beh.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
                }
            }
        }
    }


    /// <summary>
    /// 設置集氣特效, color==ColorType.None則全部不顯示
    /// </summary>
    /// <param name="color"></param>
    /// <param name="level"></param>
    public void SetChargeEffect(ColorType color, int level)
    {
        linkLine.EffectChange(level, color);

        if (!UnitAnims.ContainsKey(color))
            color = ColorType.None;

        ChargeEffectBehaviour.PlayEffect(color, level);
    }

    public void SetStarEffect(ColorType color, int level)
    {
        if (!UnitAnims.ContainsKey(color))
            color = ColorType.None;
        ChargeEffectBehaviour.PlayStar(color, level);
    }


    /// <summary>
    /// 獲取該位置是否有可連接的消除物
    /// </summary>
    /// <param name="r"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    public ElementBehaviour GetLinkableElementAt(int r, int c)
    {
        if (eleViews[r, c] == null || coverViews[r, c] != null)
            return null;

        return eleViews[r, c].GetComponent<ElementBehaviour>();
    }

    /// <summary>
    /// 播放掉血特效
    /// </summary>
    /// <param name="hurtValue"></param>
    /// <param name="remainHP">剩餘血量</param>
    public void PlayBloodNumAnim(int hurtValue, int remainHP)
    {
        //Q.Log(LogTag.UI, "PlayBloodNumAnim {0}", hurtValue);
        Action<Animator, int> StateMachineExit = null;
        StateMachineExit = delegate(Animator animator, int stateMachinePathHash)
        {
            Q.Assert(animator != null);
            BaseStateMachineBehaviour stateBeh = animator.GetBehaviour<BaseStateMachineBehaviour>();
            //Q.Assert(stateBeh != null, animator.gameObject.name);
            //這裡偶爾獲取到stateBeh為null，因為animator.enabled = false導致。
            //但仍不清楚原因
            if (stateBeh != null)
            {
                stateBeh.StateMachineExitEvent -= StateMachineExit;
            }
            StartCoroutine(Utils.DelayDeactive(BloodNumEffect.gameObject));
        };

        BloodNumEffect.Number = hurtValue;
        //因為在Mac/iOS 下，直接播放會crash，不清楚什麼原因，懷疑是動態插入Image，導致Animation控制scale時出現問題
        //用延遲調用的方法可以解決
        LeanTween.delayedCall(0.1f, delegate()
        {
            BloodNumEffect.gameObject.SetActive(true);
            Animator anim = BloodNumEffect.GetComponent<Animator>();
            anim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent += StateMachineExit;
        });

        //傷血後拋出怪物血量改變事件 uibattle自己監聽
        //if (!IsTreeActivity && ShouldEnemyShow)
        //    UIBattleBeh.ChangeEnemyHpTo(remainHP);
    }

    public void LockTile(int[] points)
    {
        for (int r = 0, n = eleViews.GetLength(0); r < n; r++)
        {
            for (int c = 0, m = eleViews.GetLength(0); c < m; c++)
            {
                GameObject ga = eleViews[r, c];
                if (ga == null)
                    continue;
                bool needAlpha = false;
                for (int i = 0; i < points.Length; i++)
                {
                    if ((r * 7 + c) == points[i])
                    {
                        needAlpha = true;
                        break;
                    }
                }
                ga.GetComponent<Animator>().enabled = false;
                ga.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                ga.GetComponent<Image>().color = new Color(1, 1, 1, needAlpha ? 1.0f : 0.5f);
            }
        }
    }

    public GameObject[,] GetTypeGameObjects(TileType type)
    {
        switch (type)
        {
            case TileType.Element:
                return eleViews;
            case TileType.Obstacle:
                return obstacleViews;
            case TileType.SeperatorH:
                return seperatorHViews;
            case TileType.SeperatorV:
                return seperatorVViews;
            case TileType.Cover:
                return coverViews;
            case TileType.Bottom:
                return bottomViews;
            case TileType.Collect:
                return collectViews;
        }
        return null;
    }


    IEnumerator Start()
    {
        Q.Log("BoardBehaviour.Start()");
        linkInteractBeh = GetComponent<LinkInteractBehaviour>();
        linkInteractBeh.DownEvent += OnLinkDownEvent;
        linkInteractBeh.UpEvent += OnLinkUpEvent;
        linkInteractBeh.DragEvent += OnLinkDragEvent;
        linkInteractBeh.MoveToCancelAreaEvent += DragToCancelLink;
        linkInteractBeh.CancelLinkEvent += UpCancelLink;

        UpPosition = Vector2.zero;
        initAnimComplete = false;
        isShowUnitPanel = false;
        ShouldEnemyShow = true;
        IsGuide = false;
        IsHaveResult = false;
        isGiveUp = false;
        UnitPanel.gameObject.SetActive(isShowUnitPanel);
        GuideManager.getInstance().initGuideData();

        tipsTimer = new Timer(ShowTipsInterval);
        tipsTimer.Elapsed += OnTipsTimer;
        tipsTimer.Start();

        lightTimer = new Timer(LightTipsInterval);
        lightTimer.Elapsed += OnTimeToLight;
        lightTimer.Start();

        EliminateHelper = new EliminateElementsHelper(this);
        NoCDSkillHelper = new NoCDThrowSkillHelper(this);

        UILinkTipsHelp = gameObject.GetComponent<UILinkTipsHelper>();

        if(UILinkTipsHelp == null)
            UILinkTipsHelp = gameObject.AddComponent<UILinkTipsHelper>();

        gameCtrl = GameController.Instance;
        model = gameCtrl.Model;
        battleModel = model.BattleModel;

        modelEventSystem = gameCtrl.ModelEventSystem;
        BoardCamera = Camera.main;
        playingRuleCtr = gameCtrl.PlayingRuleCtr;
        isFightStep = false;
        int lv = TestLevel;
        List<int> units = new List<int>();

        if (gameCtrl.SceneCtr.SceneData != null)
        {
            lv = (int)gameCtrl.SceneCtr.SceneData["lvl"];
            IsTreeActivity = gameCtrl.StageCtr.IsActivityStage(lv);
            units = (List<int>)gameCtrl.SceneCtr.SceneData["units"];

            if (gameCtrl.SceneCtr.SceneData.ContainsKey("guideFlag"))
            {
                IsGuide = (bool)gameCtrl.SceneCtr.SceneData["guideFlag"];
                if (IsGuide)
                {
                    //隱藏
                    UnitLayer.GetChild((int)ColorType.Wood - 1).gameObject.SetActive(false);
                    //UnitLayer.GetChild((int)ColorType.Wood - 1).gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                    //ShouldEnemyShow = false;
                }
            }
            else
            {
                IsGuide = false;
            }
        }
        else
        {
            //沒有選中伙伴的數據，說明是在BattleScene中直接測試，因此需要設置加數據
            units = new List<int>();
            units.Add(1315);//Frog
            units.Add(2101);//Fox
            units.Add(3101);//Panda
            units.Add(4101);//Penguin
            units.Add(5101);//Monkey
        }
        //yield return new WaitForEndOfFrame();
        //加載熊貓出現時的特效
        pandaSpawnEffect = Resources.Load<GameObject>("Prefabs/Effects/EffectUnitSpawn");

        //yield return new WaitForEndOfFrame();

        ComboEffBeh = GetComponent<ComboEffBehaviour>();

        DropElementHelper.MoveSpeed = MoveSpeed;
        DropElementHelper.MoveEaseType = MoveEaseType;

        EliminateEffect = GetComponent<EliminateEffectBehaviour>();
        HitEffect = GetComponent<HitEffectBehaviour>();
        ChargeEffectBehaviour = GetComponent<ChargeEffectBehaviour>();
        elementPrefab = Resources.Load<GameObject>("Prefabs/Element");
        collectPrefab = Resources.Load<GameObject>("Prefabs/Collect");
        obstaclePrefab = Resources.Load<GameObject>("Prefabs/Obstacle");
        coverPrefab = Resources.Load<GameObject>("Prefabs/Cover");
        bottomPrefab = Resources.Load<GameObject>("Prefabs/Ice");
        seprHPrefab = Resources.Load<GameObject>("Prefabs/SeperatorH");
        seprVPrefab = Resources.Load<GameObject>("Prefabs/SeperatorV");

        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "Before load battle model");
        //加載戰鬥場景
        CrtBattleBeh = CreateBattle(lv);
        HitEnemyBeh = GetComponent<HitEnemyBehaviour>();
        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "After load battle model");
        linkLine = GameObject.Find("LinkLineLayer").GetComponent<UILinkLineBehaviour>();
        Transform boardFront = GameObject.Find("BoardFront").GetComponent<Transform>();
        interactLayer = boardFront.Find("InteractLayer") as RectTransform;
        interactDownLayer = boardFront.Find("InteractDownLayer") as RectTransform;
        FlyLayer = boardFront.Find("FlyLayer") as RectTransform;
        ElementLayer = boardFront.Find("Elements") as RectTransform;
        CoverLayer = boardFront.Find("Covers") as RectTransform;
        SeperaterHLayer = boardFront.Find("SeperatorsH") as RectTransform;
        SeperaterVLayer = boardFront.Find("SeperatorsV") as RectTransform;
        BottomLayer = boardFront.Find("BottomLayer") as RectTransform;
        CollectLayer = boardFront.Find("Collect") as RectTransform;
        ObstacleLayer = boardFront.Find("Obstacles") as RectTransform;
        decorateLayer = boardFront.Find("Decorate") as RectTransform;

        eliminateQueue = new List<Position>();
        eventQueue = new Queue<EventArgs>();
        eleGameObjPool = new List<GameObject>();

        viewEventSystem = gameCtrl.ViewEventSystem;
        viewEventSystem.BattleInitAnimCompleteEvent += OnInitAnimCompleteEvent;

        gameCtrl.Popup.OnOpenComplete += Popup_OnOpenComplete;
        gameCtrl.Popup.OnCloseComplete += Popup_OnCloseComplete;
        modelEventSystem.OnBoardEliminate += OnModelEvent;
        modelEventSystem.OnBoardInit += OnModelEvent;
        modelEventSystem.OnBoardMove += OnModelEvent;

        if (!IsTreeActivity)
        {
            //ImmuneSp = UIBattleBeh.ImmuneSp;
            ImmuneBehaviour = new MonstorImmuneBehaviour(ImmuneSp, FlyLayer, this);
        }

        //Q.Log("Load level={0}", lv);
        playingRuleCtr.InitWithLevel(this, lv, units);
        GuideUtility.addFightGuideData(battleModel);

        goalId = new Dictionary<int, int>();
        List<StageConfig.Goal> tList = new List<StageConfig.Goal>();
        for (int i = 0; i < battleModel.CurrentGoal.Count; i++)
        {
            StageConfig.Goal goal = battleModel.CurrentGoal[i];
            if (goal.Type == BattleGoal.Score)
                continue;
            tList.Add(goal);
        }
        for (int i = 0; i < tList.Count; i++)
        {
            StageConfig.Goal goal = tList[i];
            goalId.Add(goal.RelativeID, i);
        }

        yield return 0;
        //uiBattle與uiTreeBattle的初始化
        InitBattleUI(battleModel, units);

        yield return new WaitForEndOfFrame();


        modelEventSystem.BeforeSceneChangeEvent += OnBeforeSceneChangeEvent;
        modelEventSystem.OnBoardRearrange += OnModelEvent;
        modelEventSystem.OnBattleResult += OnModelEvent;
        modelEventSystem.OnThrowTile += OnModelEvent;
        modelEventSystem.onReachGoalEvent += OnModelEvent;
        modelEventSystem.OnStepEmpty += OnModelEvent;
        modelEventSystem.OnTimeEmpty += OnModelEvent;
        modelEventSystem.OnBuyMoves += modelEventSystem_OnBuyMoves;
        modelEventSystem.OnBuyTime += modelEventSystem_OnBuyTime;
        modelEventSystem.OnBuyGoods += UpdateAndSelectCurItem;
        modelEventSystem.OnDialogShow += OnFightDialogShow;
        modelEventSystem.OnDialogHide += OnFightDialogHide;

        if (IsTreeActivity)
            modelEventSystem.OnTreeFightComplete += OnModelEvent;

        viewEventSystem.BoardLinkEvent += OnBoardLink;
        viewEventSystem.ClickEscapeEvent += viewEventSystem_OnEscapeClickEvent;
        viewEventSystem.ApplicationPauseEvent += onListenApplicationPauseEvent;
        viewEventSystem.AffectedChannge += OnAffectedChannge;


        ///設置Enemy血條位置////
        SetHpPos();

        if (IsGuide)
        {
            ChargeEffectBehaviour.ColorLayerRoots[2].gameObject.SetActive(false);
            //ChargeEffectBehaviour.ChargeEffLayer.transform.FindChild("Wood").gameObject.SetActive(false);
            //if (UIBattleBeh != null) UIBattleBeh.ControlEnemyHpDisplay(false);        //血條隱藏邏輯  待調試
            CrtBattleBeh.OnEnemyPoitsShow(ShouldEnemyShow);
        }
        yield return new WaitForEndOfFrame();

        foreach (var button in UnitButton)
        {
            EventTriggerListener eventLis = EventTriggerListener.Get(button.gameObject);
            eventLis.onClick += UnitButton_OnClick;
        }

        EventTriggerListener.Get(UnitPanel.gameObject).onClick += delegate(GameObject button)
        {
            OnClickUnit();
        };

        PropCtr propCtr = GameController.Instance.PropCtr;
        ActivePropItemDic = new Dictionary<PropType, UIPropItem>();

        propCtr.ClearPropSelectAction();

        ///創建主動道具按鈕//
        UISelectPropBehaviour.CreatePropButton(propCtr.GetAllSelectActivePropIDlist(), PropItemParent,
            PropItemPrefab.gameObject, null, ActivePropItemDic, true);

        //清除道具選擇狀態，被動道具的使用狀態不能清除//
        propCtr.ClearSelect();
        foreach (var ac in ActivePropItemDic)
        {
            propCtr.PropSelectActionDic.Add(ac.Key, PropExclusive);
        }

        //GameController.Instance.ViewEventSystem.JumpSceneHideCloudEvent(null, null);
        //活動關卡不走新手引導
        if (!IsGuideState() && !gameCtrl.StageCtr.IsActivityStage(battleModel.CurStage.stageId))
        {
            this.AddFliter();
        }

        // 開始時鎖上操作鎖
        PlusInteractLock(false, false);

        Q.Assert(FlyLayer.anchoredPosition == ElementLayer.anchoredPosition);
        Q.Assert(interactDownLayer.anchoredPosition == ElementLayer.anchoredPosition);
        Q.Assert(interactLayer.anchoredPosition == ElementLayer.anchoredPosition);
    }


    void InitBattleUI(BattleModel battleModel, List<int> units)
    {
        GameObject uiPrefab = null;

        if (IsTreeActivity)
        {
            //uiPrefab = Resources.Load<GameObject>("Prefabs/Ui/UIBattleTreeActivity");
            uiPrefab = UIBattleTreeActivity;
            UIBattleTreeActivity.SetActive(true);
            UIBattle.SetActive(false);
            UIBattleTreeBeh = uiPrefab.GetComponent<UIBattleTreeActivityBehaviour>();
            UIBattleTreeBeh.SetCamera(BoardCamera);
            TreeFightCtr = GameController.Instance.TreeFightCtr;

            UIBattleTreeBeh.OnClickPauseButton += OnClickPauseButton;
        }
        else
        {
            //uiPrefab = Resources.Load<GameObject>("Prefabs/Ui/UIBattle");
            uiPrefab = UIBattle;
            UIBattleTreeActivity.SetActive(false);
            UIBattle.SetActive(true);
            UIBattleBeh = uiPrefab.GetComponent<UIBattleBehaviour>();
            UIBattleBeh.SetCamera(BoardCamera);
            UIBattleBeh.units = units;
            UIBattleBeh.PauseButton.enabled = false;
            UIBattleBeh.SetData(battleModel);

            UIBattleBeh.OnClickPauseButton += OnClickPauseButton;
        }

        uiPrefab.transform.SetParent(transform.parent);
        uiPrefab.transform.localPosition = Vector3.zero;
        uiPrefab.transform.localScale = new Vector3(1, 1, 1);
    }

    void OnDestroy()
    {
        isFightStep = false;
        if (tipsTimer != null)
        {
            tipsTimer.Elapsed -= OnTipsTimer;
            tipsTimer.Stop();
        }
        if (lightTimer != null)
        {
            lightTimer.Elapsed -= OnTimeToLight;
            lightTimer.Stop();
        }
        // 銷毀計時模式的定時器
        viewEventSystem.ReadyGo = null;
        battleModel.DestroyTimer();

        //Q.Log("BattleSceneBehaviour::OnDestroy()");
        gameCtrl.ViewEventSystem.BoardLinkEvent -= OnBoardLink;
        //GameController.Instance.ModelEventSystem.OnStageBeginEvent -= onLoseStageBeginEvent;
        //GameController.Instance.ModelEventSystem.OnStageBeginEvent -= onResponeRestartBattle;
        gameCtrl.Popup.OnOpenComplete -= Popup_OnOpenComplete;
        gameCtrl.Popup.OnCloseComplete -= Popup_OnCloseComplete;
        modelEventSystem.OnBoardEliminate -= OnModelEvent;
        modelEventSystem.OnBoardInit -= OnModelEvent;
        modelEventSystem.OnBoardMove -= OnModelEvent;
        modelEventSystem.OnBoardRearrange -= OnModelEvent;
        modelEventSystem.OnBattleResult -= OnModelEvent;
        modelEventSystem.OnThrowTile -= OnModelEvent;
        modelEventSystem.onReachGoalEvent -= OnModelEvent;
        modelEventSystem.OnStepEmpty -= OnModelEvent;
        modelEventSystem.OnTimeEmpty -= OnModelEvent;
        modelEventSystem.OnBuyMoves -= modelEventSystem_OnBuyMoves;
        modelEventSystem.OnBuyTime -= modelEventSystem_OnBuyTime;
        modelEventSystem.BeforeSceneChangeEvent -= OnBeforeSceneChangeEvent;
        modelEventSystem.OnBuyGoods -= UpdateAndSelectCurItem;
        modelEventSystem.OnDialogShow -= OnFightDialogShow;
        modelEventSystem.OnDialogHide -= OnFightDialogHide;
        if (IsTreeActivity)
            modelEventSystem.OnTreeFightComplete -= OnModelEvent;

        viewEventSystem.BattleInitAnimCompleteEvent -= OnInitAnimCompleteEvent;
        viewEventSystem.BoardLinkEvent -= OnBoardLink;
        viewEventSystem.ClickEscapeEvent -= viewEventSystem_OnEscapeClickEvent;
        viewEventSystem.AffectedChannge -= OnAffectedChannge;

        GameController.Instance.PropCtr.ClearPropSelectAction();

        this.RemoveFliter();
    }

    void OnFightDialogShow()
    {
        Debug.Log("OnFightDialogShow");
        UIBattleBeh.StopGuide();
    }
    void OnFightDialogHide()
    {
        Debug.Log("OnFightDialogHide");
        UpGuide();
    }

    void Popup_OnCloseComplete(PopupID obj)
    {
        if (!isGiveUp && !gameCtrl.Popup.HasPopup && battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
        {
            // 關閉介面，開啟計時模式的準備動畫
            if (ReadyGoHelper.OnUIPopupClose(obj, battleModel, UIBattleBeh, viewEventSystem))
            {
                PlusInteractLock(false, false);
                Action rgEnd = null;
                rgEnd = delegate()
                {
                    viewEventSystem.ReadyGo -= rgEnd;
                    MinusInteractLock();
                };
                viewEventSystem.ReadyGo += rgEnd;

                //會監聽 UIBattleBeh 的暫停事件
                //UIBattleBeh.PauseButton.enabled = false;
            }
        }
    }

    void Popup_OnOpenComplete(PopupID obj)
    {
        if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
        {
            //待解耦 
            // 打開界面，暫停計時模式的定時器 uibattle部分閃爍邏輯待修改----------------------TODO
            if (ReadyGoHelper.OnUIPopupOpen(obj, battleModel))
            {
                UIBattleBeh.StopFlashEffect();
            }
        }
        if (obj == PopupID.UIWin || obj == PopupID.UILose)
        {
            //勝利界面或者失敗界面被打開後隱藏world 停掉音樂 在大樹戰鬥中邏輯一樣。
            //這兩個界面會蓋住屏幕，此時隱藏界面與音效最合適
            //舊的邏輯中在
            World.gameObject.SetActive(false);
            LayerCtrlBehaviour.ActiveLayer.NormalLayer.gameObject.SetActive(false);
        }
        else if (obj == PopupID.UIGetChance || obj == PopupID.UIUpgrad)
        {
            if (gameCtrl.Popup.IsPopup(PopupID.UIWin))
            {
                gameCtrl.Popup.Close(PopupID.UIWin, false);
            }
            if (gameCtrl.Popup.IsPopup(PopupID.UILose))
            {
                gameCtrl.Popup.Close(PopupID.UILose, false);
            }
        }
    }

    /// <summary>
    /// 遊戲進入後台時後觸發這個事件
    /// </summary>
    /// <param name="obj"> true :進入後台   false:進入遊戲</param>
    private void onListenApplicationPauseEvent(bool obj)
    {
        if (obj)
        {
            CancelLink();
        }
    }

    /// <summary>
    /// 取消連線
    /// </summary>
    private void CancelLink()
    {
        if (eliminateQueue != null && eliminateQueue.Count > 0)
        {
            RepealElimminate();
            tipsTimer.Start();
            SetEleAnimatorEnabled(true);
            SetProminentElement(ColorType.All);
        }
    }


    /// <summary>
    /// 進入關卡的初始動畫播放完成
    /// </summary>
    /// <param name="obj"></param>
    private void OnInitAnimCompleteEvent(Action callbackBeforeInit)
    {
        initAnimComplete = true;
        MinusInteractLock();

        //從戰鬥模式中可以區分是否是大樹
        if (battleModel.CrtStageConfig.Mode == BattleMode.Normal || battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
        {
            bool timeLimit = battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit;
            //UIBattleBeh.PauseButton.enabled = true;           //在監聽暫停事件中做處理  如果需要屏蔽暫停按鈕的點擊動畫另外再處理
            int step = timeLimit ? 0 : battleModel.StageLimit - battleModel.RemainSteps;
            GuideManager.getInstance().StartFightGuide(battleModel.CrtStageConfig.ID, step, callbackBeforeInit, timeLimit);
            GuideManager.getInstance().StartPropGuide(battleModel.CrtStageConfig.ID);
            if (timeLimit)
            {
                PlusInteractLock(false, false);
                Action readyEnd = null;
                readyEnd = delegate()
                {
                    viewEventSystem.ReadyGo -= readyEnd;
                    MinusInteractLock();
                };
                viewEventSystem.ReadyGo += readyEnd;
            }
        }
    }

    private void OnBeforeSceneChangeEvent(Scenes arg1, Scenes arg2)
    {
        //把夥伴都緩存起來
        //用對應的UnitConfig.PrefabPath
        foreach (var pair in battleModel.CrtUnitDict)
        {
            ColorType c = pair.Key;
            if (!UnitAnims.ContainsKey(c) || UnitAnims[c] == null)
                continue;

            Animator anim = UnitAnims[c];
            if (anim != null && anim.GetBehaviour<BaseStateMachineBehaviour>() != null)
            {
                anim.GetBehaviour<BaseStateMachineBehaviour>().ClearEventListeners();
                GameController.Instance.PoolManager.PushToInstancePool(
                    pair.Value.Config.PrefabPath,
                    anim.gameObject.transform);
            }
        }
    }

    //購買了步數返回
    void modelEventSystem_OnBuyMoves(int addMove)
    {
        if (gameCtrl.Popup.IsPopup(PopupID.UIAddMove))
        {
            gameCtrl.Popup.Close(PopupID.UIAddMove, true);
        }
    }

    void modelEventSystem_OnBuyTime(int addTime)
    {
        if (gameCtrl.Popup.IsPopup(PopupID.UIAddMove))
        {
            gameCtrl.Popup.Close(PopupID.UIAddMove, true);
        }

        if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
        {
            PlusInteractLock(false, false);
            Action readyEnd = null;
            readyEnd = delegate()
            {
                MinusInteractLock();
            };
            viewEventSystem.ReadyGo += readyEnd;
        }
    }

    private void viewEventSystem_OnEscapeClickEvent()
    {
        //返回鍵被按
        OpenPauseWin();
    }


    ///// <summary>
    ///// 更新關卡目標   
    ///// </summary>
    //void InitGoalTip()
    //{
    //    if (IsTreeActivity)
    //        return;

    //    UIGoalTip.Data tipData = new UIGoalTip.Data();
    //    tipData.Title = Utils.GetTextByStringID(battleModel.CrtStageConfig.NameStringID);
    //    tipData.goals = new List<StageConfig.Goal>();
    //    foreach (var go in battleModel.CurrentGoal)
    //    {
    //        if (go.Type != BattleGoal.Score)
    //            tipData.goals.Add(go);
    //    }
    //    //tipData.goals = battleModel.CurrentGoal;
    //    UIBattleBeh.UiGoalTip.SetData(tipData);
    //}


    /// <summary>
    /// 創建戰場物體
    /// </summary>
    private BattleBehaviour CreateBattle(int stageId)
    {
        StageConfig sConfig = gameCtrl.Model.StageConfigs[stageId];
        Transform t = gameCtrl.QMaxAssetsFactory.CreateBattlePrefab(sConfig);
        t.gameObject.SetActive(true);
        BattleBehaviour battleBehaviour = t.GetComponent<BattleBehaviour>();
        t.SetParent(World);

        return battleBehaviour;
    }

    /// <summary>
    /// 完成一個目標的事件
    /// </summary>
    /// <param name="completeStar"></param>
    /// <param name="nextGoal"></param>
    //private void OnBattleGoalComplete(int completeStar, List<StageConfig.Goal> nextGoal)
    //{
    //    if (nextGoal != null)
    //    {
    //        UpdateGoalUI(nextGoal, true);
    //    }
    //    else//所有目標已經完成，保持現狀
    //    {

    //    }
    //}

    private void OnClickPauseButton(object obj)
    {
        if (initAnimComplete && !IsHaveResult)
        {
            OpenPauseWin();
        }
    }

    void OpenPauseWin()
    {
        //交互鎖>0時，無法點擊暫停界面
        //if (interactLock > 0 || gameCtrl.Popup.IsPopup(PopupID.UIPause))
        //{
        //    return;
        //}

        UIPauseBeh = gameCtrl.Popup.Open(PopupID.UIPause, null, true, true).GetComponent<UIPauseBehaviour>();
        addPauseBehEvent();
        UIPauseBeh.ChangeCheckPropButton(!IsGuideState());
    }

    bool IsGuideState()
    {
        return battleModel.CrtStageConfig.ID <= GuideManager.GuideStageID;
    }

    private void addPauseBehEvent()
    {
        UIPauseBeh.ButtonClose.onClick += UIPauseBehClloe;
        UIPauseBeh.ButtonContinue.onClick += UIPauseBehContinue;
        UIPauseBeh.ButtonRestart.onClick += UIPauseBehRestart;
        UIPauseBeh.ButtonGiveUp.onClick += UIPauseBehGiveUp;
        UIPauseBeh.ButtonMSwitch.onClick += UIPauseBehMSwitch;
        UIPauseBeh.ButtonSSwitch.onClick += UIPauseBehSSwitch;

        //GameController.Instance.ModelEventSystem.OnStageBeginEvent += onResponeRestartBattle;
    }

    /// <summary>
    /// 暫時沒有用處//
    /// </summary>
    private void onResponeRestartBattle()
    {
        removePauseBehEvent();
        gameCtrl.Popup.Close(PopupID.UIPause, false);
        Dictionary<string, object> sceneData = new Dictionary<string, object>();
        sceneData.Add("lvl", battleModel.CrtStageConfig.ShowNum);
        List<int> units = new List<int>();
        foreach (KeyValuePair<ColorType, Unit> pair in battleModel.CrtUnitDict)
        {
            units.Add(pair.Value.Config.ID);
        }
        sceneData.Add("units", units);
        //GameController.Instance.Client.BeginStage(battleModel.CrtStageConfig.ID, units);
        gameCtrl.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData, true, true);
    }

    private void removePauseBehEvent()
    {
        //GameController.Instance.ModelEventSystem.OnStageBeginEvent -= onResponeRestartBattle;
        UIPauseBeh.ButtonClose.onClick -= UIPauseBehClloe;
        UIPauseBeh.ButtonContinue.onClick -= UIPauseBehContinue;
        UIPauseBeh.ButtonRestart.onClick -= UIPauseBehRestart;
        UIPauseBeh.ButtonGiveUp.onClick -= UIPauseBehGiveUp;
        UIPauseBeh.ButtonMSwitch.onClick -= UIPauseBehMSwitch;
        UIPauseBeh.ButtonSSwitch.onClick -= UIPauseBehSSwitch;
    }
    private void UIPauseBehContinue(UIButtonBehaviour go)
    {
        removePauseBehEvent();
        gameCtrl.Popup.Close(PopupID.UIPause, false);
    }
    private void UIPauseBehRestart(UIButtonBehaviour go)
    {
        //battleModel.CrtStageConfig.CostEnergy
        if (!GameController.Instance.StageCtr.CheckEnergyEnough(battleModel.CrtStageConfig.ID))
        {
            GameController.Instance.Popup.Open(PopupID.UIPowerShop, null, true, true);
            return;
        }

        //removePauseBehEvent();
        //gameCtrl.Popup.Close(PopupID.UIPause, false);
        Dictionary<string, object> sceneData = new Dictionary<string, object>();
        sceneData.Add("lvl", battleModel.CrtStageConfig.ShowNum);
        List<int> units = new List<int>();
        foreach (KeyValuePair<ColorType, Unit> pair in battleModel.CrtUnitDict)
        {
            units.Add(pair.Value.Config.ID);
        }
        sceneData.Add("units", units);

        ///清空當前遊戲所選的所用道具///
        GameController.Instance.PropCtr.ClearSelectAndUse();

        if (UIPauseBeh.IsCheckUnit && !UIPauseBeh.IsCheckProp)
        {
            UIUnitSelectWindowBehaviour selectUnit = GameController.Instance.Popup.Open(PopupID.UISelectHero, null, true, true).GetComponent<UIUnitSelectWindowBehaviour>();

            selectUnit.SetData(battleModel.CrtStageConfig, true);
        }
        else if (UIPauseBeh.IsCheckUnit && UIPauseBeh.IsCheckProp)
        {
            UIUnitSelectWindowBehaviour selectUnit = GameController.Instance.Popup.Open(PopupID.UISelectHero, null, true, true).GetComponent<UIUnitSelectWindowBehaviour>();

            selectUnit.SetData(battleModel.CrtStageConfig);
        }
        else if (!UIPauseBeh.IsCheckUnit && UIPauseBeh.IsCheckProp)
        {
            ///暫時邏輯,屏蔽教學//
            if (IsGuideState())
                return;
            UISelectPropBehaviour selectProp = GameController.Instance.Popup.Open(PopupID.UISelectProp, null, true, true).GetComponent<UISelectPropBehaviour>();
            selectProp.SetData(battleModel.CrtStageConfig, units);
        }
        else if (!UIPauseBeh.IsCheckUnit && !UIPauseBeh.IsCheckProp)
        {
            //使用道具為空///
            Dictionary<int, int> usegoods = new Dictionary<int, int>();
            GameController.Instance.Client.BeginStage(battleModel.CrtStageConfig.ID, units, usegoods);
            //gameCtrl.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData);
            gameCtrl.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData, true, true);
        }

        ///如果不是新手教學關卡默認勾選主動道具//
        if (!IsGuideState())
        {
            List<int> activeprop = GameController.Instance.PropCtr.GetAllActivePropIDList();
            GameController.Instance.PropCtr.SetPropSelect(activeprop);
        }

        ///遊戲中重新開始邏輯待確定///
        //Debug.LogError("要修改重新開始遊戲邏輯");
        //GameController.Instance.Client.BeginStage(battleModel.CrtStageConfig.ID, units);
        //gameCtrl.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData);
    }
    private void UIPauseBehGiveUp(UIButtonBehaviour go)
    {
        isGiveUp = true;
        removePauseBehEvent();
        gameCtrl.Popup.Close(PopupID.UIPause, false);
        gameCtrl.SceneCtr.LoadLevel(Scenes.MapScene, null, true, true);

        // 關卡失敗事件，放棄關卡
        if (GameController.Instance.ModelEventSystem.OnStageLose != null)
            GameController.Instance.ModelEventSystem.OnStageLose();
    }
    private void UIPauseBehMSwitch(UIButtonBehaviour go)
    {
    }
    private void UIPauseBehSSwitch(UIButtonBehaviour go)
    {
    }

    private void UIPauseBehClloe(UIButtonBehaviour go)
    {
        removePauseBehEvent();
        gameCtrl.Popup.Close(PopupID.UIPause);
    }

    void OpenUIWin(ModelEventSystem.BattleResultEventArgs args)
    {
        Q.Log("關卡勝利");
        if (battleModel.CrtStageConfig.ID == 1)
        {
            // 第一關第一次勝利，播放台詞音效
            if (!PlayerPrefsTools.HasKey(OnOff.FirstTimeWin, true))
            {
                PlayerPrefsTools.SetIntValue(OnOff.FirstTimeWin, 1, true);
                GameController.Instance.AudioManager.PlayAudio("Vo_accompany_4");
            }
            //if (Persistence.Instance.GetValue("FirstTimeWin") == null)
            //{
            //    Persistence.Instance.SetValue("FirstTimeWin", "true");
            //    GameController.Instance.AudioManager.PlayAudio("Vo_accompany_4");
            //}
        }
        ActorGameResponse player = model.PlayerData;
        StageConfig stageConfig = battleModel.CrtStageConfig;

        UIWinBehaviour winBeh = gameCtrl.Popup.Open(PopupID.UIWin, null, false).GetComponent<UIWinBehaviour>();
        winBeh.OnClickOKButton += OnClickResultBackButton;

        //for test
        //int starNum = 3;

        winBeh.SetCollected(args.Gem, args.Key, args.UpgradeA, args.UpgradeB, args.Coin);

        //for test end
        winBeh.SetCrtStatus(
            player.energy, player.energyMax,
            player.key, player.gem,
            player.upgradeA, player.upgradeB, player.coin);

        //winBeh.SetStageInfo(stageConfig.ShowNum, Utils.GetTextByStringID(stageConfig.NameStringID), starNum);
        winBeh.SetStageInfo(stageConfig.ShowNum, Utils.GetTextByStringID(stageConfig.NameStringID), battleModel.CrtStar);
    }

    void OpenUILose(ModelEventSystem.BattleResultEventArgs args)
    {

        Q.Log("關卡失敗");
        // 第一次失敗，播放台詞音效
        if (!PlayerPrefsTools.HasKey(OnOff.FirstTimeLose, true))
        {
            PlayerPrefsTools.SetIntValue(OnOff.FirstTimeLose, 1, true);
            GameController.Instance.AudioManager.PlayAudio("Vo_accompany_3");
        }
        //if (Persistence.Instance.GetValue("FirstTimeLose") == null)
        //{
        //    Persistence.Instance.SetValue("FirstTimeLose", "true");
        //    GameController.Instance.AudioManager.PlayAudio("Vo_accompany_3");
        //}
        ActorGameResponse player = model.PlayerData;
        StageConfig stageConfig = battleModel.CrtStageConfig;

        UILoseBehaviour uILose = gameCtrl.Popup.Open(PopupID.UILose).GetComponent<UILoseBehaviour>();

        uILose.OnClickRestartButton += delegate(object sender)
        {
            if (!GameController.Instance.StageCtr.CheckEnergyEnough(battleModel.CrtStageConfig.ID))
            {
                GameController.Instance.Popup.Open(PopupID.UIPowerShop, null, true, true);
                return;
            }

            List<int> units = new List<int>();
            foreach (KeyValuePair<ColorType, Unit> pair in battleModel.CrtUnitDict)
            {
                units.Add(pair.Value.Config.ID);
            }

            UIUnitSelectWindowBehaviour selectUnit = GameController.Instance.Popup.Open(PopupID.UISelectHero, null, true, true).GetComponent<UIUnitSelectWindowBehaviour>();
            selectUnit.SetData(stageConfig);

            ///失敗重開邏輯待確定///
            //Debug.LogError("要修改重新開始遊戲邏輯");
            //GameController.Instance.Client.BeginStage(battleModel.CrtStageConfig.ID, units);
        };

        //GameController.Instance.ModelEventSystem.OnStageBeginEvent += onLoseStageBeginEvent;

        uILose.OnClickMapButton += OnClickResultBackButton;
        uILose.OnClickKeyButton += loseBeh_OnClickKeyButton;
        uILose.OnClickRestartButton += loseBeh_OnClickRestartButton;

        uILose.SetOpenBoxAble(gameCtrl.PlayerCtr.CheckGetChanceAble());

        //for test
        int reason = -1;
        reason = CheckFailReason();
        int failLang = -1;
        if (reason == 0)
        {
            failLang = UnityEngine.Random.Range(528, 532);
        }
        else if (reason == 1)
        {
            failLang = UnityEngine.Random.Range(532, 535);
        }
        else if (reason == 2)
        {
            failLang = UnityEngine.Random.Range(535, 538);
        }
        string tips = "每次都連最長不一定是最好的策略哦！";
        if (failLang != -1)
        {
            tips = Utils.GetTextByID(failLang);
        }

        uILose.SetCollected(args.Gem, args.Key, args.UpgradeA, args.UpgradeB, args.Coin);
        //for test end
        uILose.SetCrtStatus(
            player.energy, player.energyMax,
            player.key, player.gem,
            player.upgradeA, player.upgradeB, player.coin);
        uILose.SetStageInfo(stageConfig.ShowNum, Utils.GetTextByStringID(stageConfig.NameStringID), tips, stageConfig.CostEnergy);
    }

    private void onLoseStageBeginEvent()
    {
        Dictionary<string, object> sceneData = new Dictionary<string, object>();
        sceneData.Add("lvl", battleModel.CrtStageConfig.ShowNum);
        List<int> units = new List<int>();
        foreach (KeyValuePair<ColorType, Unit> pair in battleModel.CrtUnitDict)
        {
            units.Add(pair.Value.Config.ID);
        }
        sceneData.Add("units", units);
        GameController.Instance.ModelEventSystem.OnStageBeginEvent -= onLoseStageBeginEvent;
        //gameCtrl.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData);
        gameCtrl.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData, true, true);
    }

    void OpenAddMove()
    {
        if (!gameCtrl.Popup.IsPopup(PopupID.UIAddMove))
        {
            UIAddMove uiAddMove = gameCtrl.Popup.Open(PopupID.UIAddMove).GetComponent<UIAddMove>();
            uiAddMove.SetData(battleModel);
            uiAddMove.OnEndLevel += uiAddMove_OnEndLevel;
        }
    }

    void uiAddMove_OnEndLevel(UIAddMove uiAddMove)
    {
        //不買步數 戰鬥失敗
        uiAddMove.OnEndLevel -= uiAddMove_OnEndLevel;
        battleModel.SubmitFightRequest();
    }

    IEnumerator BattleResult(ModelEventSystem.BattleResultEventArgs args)
    {
        PlusInteractLock();
        bool result = args.Result;

        if (result)
        {
            if (tipsTimer != null)
            {
                tipsTimer.Stop();
                tipsTimer.Elapsed -= OnTipsTimer;
                isTimeShowTip = false;
                timeShowTipCount = 0;
            }

            // 勝利了，事件通知
            if (GameController.Instance.ModelEventSystem.OnStageWin != null)
                GameController.Instance.ModelEventSystem.OnStageWin();

            GameController.Instance.AudioManager.PlayAudio("SD_cheer");


            if (UIBattleBeh != null)
            {
                //播放剩餘步數獎勵
                yield return new WaitForSeconds(.5f);
                if (!isFightStep)
                {
                    PlusInteractLock();
                    isFightStep = true;

                    //剩餘步數投遞到盤面時，播放台詞音效
                    GameController.Instance.AudioManager.PlayAudio("Vo_accompany_13");
                    LeftStepAwardHelper.Play(this, battleModel, UIBattleBeh.StepOriImage, ElementLayer, FlyLayer, delegate()
                    {
                        LeanTween.delayedCall(1.0f, delegate()
                        {
                            BattleEnd(args);
                        });
                    });
                }
            }

            if (!IsTreeActivity)
            {
                //大樹活動，不播放攝像機遊走效果
                CrtBattleBeh.TimeScale = .1f;
                CrtBattleBeh.UseLinear = true;
                CrtBattleBeh.RoamNext();
                if (GameController.Instance.ViewEventSystem.OnBattleCameraMove != null)
                    GameController.Instance.ViewEventSystem.OnBattleCameraMove(0);
            }

            foreach (KeyValuePair<ColorType, Animator> keVal in UnitAnims)
            {
                Utils.ResetAnimatorParams(keVal.Value);
                keVal.Value.Play("Win");

                //keVal.Value.SetTrigger("TriggerWin");
            }

            yield return new WaitForSeconds(3f);
        }
        else
        {
            if (gameCtrl.Popup.IsPopup(PopupID.UIAddMove))
            {
                gameCtrl.Popup.Close(PopupID.UIAddMove, false);
            }

            if (tipsTimer != null)
            {
                tipsTimer.Stop();
                tipsTimer.Elapsed -= OnTipsTimer;
                isTimeShowTip = false;
                timeShowTipCount = 0;
            }

            OpenUILose(args);

            World.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 戰鬥結束（BattleResult調用）
    /// </summary>
    private void BattleEnd(ModelEventSystem.BattleResultEventArgs args)
    {



        List<List<ModelEventSystem.Move>> drop = playingRuleCtr.Drop();
        DropElementHelper.Play(this, drop, delegate()
        {
            LeanTween.delayedCall(1.5f, delegate()
            {
                MinusInteractLock();
                OpenUIWin(args);
                World.gameObject.SetActive(false);
            });
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>-1 格式不對，0是沒殺死怪沒有收集元素，1是有沒收集完的元素，沒有要殺死的怪，2是兩個都有</returns>
    private int CheckFailReason()
    {
        int reason = -1;
        bool collect = false;
        bool killMonster = false;
        StageConfig.Goal goal;
        int left = 0;
        for (int i = 0; i < 4; i++)
        {
            if (i < battleModel.CurrentGoal.Count)
            {
                goal = battleModel.CurrentGoal[i];
                if (goal.Type == BattleGoal.Unit)
                {
                    //怪
                    left = goal.Num - battleModel.UnitGoal;
                    left = Mathf.Max(0, left);
                    if (left > 0)
                    {
                        killMonster = true;
                    }
                }
                else if (goal.Type == BattleGoal.Object)
                {
                    left = goal.Num - battleModel.ObjectGoal[goal.RelativeID];
                    if (left > 0)
                    {
                        collect = true;
                    }
                }
            }
        }

        if (killMonster && collect)
        {
            reason = 2;
        }
        else
        {
            if (killMonster)
            {
                reason = 0;
            }
            if (collect)
            {
                reason = 1;
            }
        }

        return reason;
    }

    //收到關卡失敗或成功消息 鏡頭緩緩向前，然後彈出窗口
    private void OnBattleResult(ModelEventSystem.BattleResultEventArgs args)
    {
        IsHaveResult = true;
        PlusInteractLock();
        //收到戰鬥結果，把正在連的元素回退
        if (eliminateQueue.Count > 0)
            RepealElimminate();

        viewEventSystem.ApplicationPauseEvent -= onListenApplicationPauseEvent;
        StartCoroutine(BattleResult(args));

    }

    void loseBeh_OnClickRestartButton(object obj)
    {
        //失敗介面重新闖關
        (obj as UILoseBehaviour).OnClickRestartButton -= loseBeh_OnClickRestartButton;
    }

    void loseBeh_OnClickKeyButton(object obj)
    {
        //失敗介面點擊 抽獎按鈕
        if (gameCtrl.PlayerCtr.CheckGetChanceAble())
        {
            (obj as UILoseBehaviour).OnClickKeyButton -= loseBeh_OnClickKeyButton;
            //gameCtrl.SceneCtr.LoadLevel(Scenes.GetChance);
            gameCtrl.Popup.Open(PopupID.UIGetChance);
        }
    }

    private void OnClickResultBackButton(object sender)
    {
        Q.Log(this.GetType().Name + ":OnClickResultBackButton");

        //邏輯修改 勝利介面返回邏輯不變，失敗介面點擊返回後直接進入地圖場景

        if (sender is UIWinBehaviour)
        {
            (sender as UIWinBehaviour).OnClickOKButton -= OnClickResultBackButton;

            if (gameCtrl.PlayerCtr.CheckGetChanceAble())
            {
                Q.Log(this.GetType().Name + ":LoadLevel(Scenes.GetChance)");
                //gameCtrl.SceneCtr.LoadLevel(Scenes.GetChance);
                gameCtrl.Popup.Open(PopupID.UIGetChance);

                if (gameCtrl.Popup.IsPopup(PopupID.UIWin))
                {
                    gameCtrl.Popup.Close(PopupID.UIWin);
                }
            }
            else
            {
                Q.Log(this.GetType().Name + ":LoadLevel(Scenes.MapScene)");
                gameCtrl.SceneCtr.LoadLevel(Scenes.MapScene, null, true, true);
            }
        }
        else if (sender is UILoseBehaviour)
        {
            (sender as UILoseBehaviour).OnClickRestartButton -= OnClickResultBackButton;
            gameCtrl.SceneCtr.LoadLevel(Scenes.MapScene, null, true, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (eventLock == 0 && eventQueue != null && eventQueue.Count > 0)
        {
            EventArgs args = eventQueue.Dequeue();
            if (args is ModelEventSystem.EliminateEventArgs)
                OnModelEliminate(args as ModelEventSystem.EliminateEventArgs);
            else if (args is ModelEventSystem.InitEventArgs)
                OnInit(args as ModelEventSystem.InitEventArgs);
            else if (args is ModelEventSystem.MoveEventArgs)
                OnMove(args as ModelEventSystem.MoveEventArgs);
            else if (args is ModelEventSystem.RearrangeEventArgs)
                OnBoardRearrange(args as ModelEventSystem.RearrangeEventArgs);
            else if (args is ModelEventSystem.BattleResultEventArgs)
                OnBattleResult(args as ModelEventSystem.BattleResultEventArgs);
            else if (args is ModelEventSystem.StepEmptyEventArgs)
                OnStepEmpty();
            else if (args is ModelEventSystem.TimeEmptyEventArgs)
                OnTimeEmpty();
            else if (args is ModelEventSystem.ReachGoalEventArgs)
                OnReachGoal();
        }
        ///暫時做限制，還要修改///
        if (guideAudioTime >= 0)
            guideAudioTime -= Time.deltaTime;

        //提示光
        CheckAndPlayLight();

        if (isTimeShowTip && !GameController.Instance.Popup.HasPopup)
        {
            isTimeShowTip = false;
#if AUTO_FIGHT
            TileObject[] tipsPath = battleModel.LinkableStack.ToArray();
            bool isStop = (battleModel.RemainSteps == 1) || (tipsPath == null) || (tipsPath.Length <= 0);

            if (!isStop)
            {
                for (int i = 0, n = tipsPath.Length; i < n; i++)
                {
                    GameObject ga = eleViews[tipsPath[i].Row, tipsPath[i].Col];
                    if (ga == null)
                    {
                        continue;
                    }
                    BaseTileBehaviour crtEle = ga.GetComponent<BaseTileBehaviour>();
                    if (crtEle == null || crtEle.Data == null)
                    {
                        continue;
                    }

                    ElementBehaviour eleb = crtEle as ElementBehaviour;
                    if (i == 0 && eleb != null && eleb.Type == ElementType.ConvertBlock)
                    {
                        isStop = true;
                        break;
                    }

                    int aCount = eliminateQueue.Count;
                    BaseTileBehaviour prevEle = null;
                    if (eliminateQueue.Count > 0)
                    {
                        prevEle = eliminateQueue[eliminateQueue.Count - 1].GetComponent<BaseTileBehaviour>();
                    }

                    int linkResturn = crtEle.Link(eliminateQueue, afffectedQueue);
                    if (linkResturn == -1)
                    {
                        continue;
                    }
                    int mode = 0;
                    if (linkResturn == 1)
                    {
                        if (eliminateQueue.Count < aCount && prevEle != null)
                        {
                            //記得考慮在 BaseTileBehaviour 裡給出調用彈動動畫的方法
                            Animator prevEleAnim = prevEle.GetComponent<Animator>();
                            if (prevEleAnim != null)
                            {
                                prevEleAnim.enabled = true;
                                prevEleAnim.SetTrigger("TriggerJelly");
                            }
                        }
                        mode = 3;
                    }
                    else if (linkResturn == 0)
                    {
                        Animator anim = crtEle.GetComponent<Animator>();
                        if (anim != null)
                        {
                            anim.enabled = true;
                            anim.SetTrigger("TriggerJelly");
                        }
                        mode = 1;
                    }

                    if (battleModel.GetCrtEnemy() != null)
                    {
                        SkillConfig skillCfg = CrtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
                        if (skillCfg != null && skillCfg.SkillColor == crtEle.Data.Config.ColorType)
                        {
                            changeImmuneIcon(skillCfg.ResourceIcon + "HL");
                        }
                        else
                        {
                            ShowEnemyBloodVolume(crtEle.Data.Config.ColorType, true);
                        }
                    }

                    ComboEffBeh.SetCombatCmoboEffect(true, eliminateQueue.Count);
                    viewEventSystem.BoardLinkEvent(mode, crtEle.Config.ColorType, eliminateQueue, afffectedQueue, null);
                }
                if (!isStop)
                {
                    RequestModelEliminate();
                    tipsTimer.Start();
                    SetEleAnimatorEnabled(true);
                    SetProminentElement(ColorType.All);
                }
            }
            if (isStop)
            {
                battleModel.CrtStar = 3;
                battleModel.SubmitFightRequest();
                tipsTimer.Stop();
            }
#else
            //帶消除數量大於0，不應該提示
            if (eliminateQueue.Count > 0)
                return;

            ShowBoardTipsHelper.Play(this, eleViews, battleModel.LinkableStack.ToArray(), null);

            // 超時提示2次(延遲2秒執行)，播放台詞音效
            if (timeShowTipCount >= 4 && !GameController.Instance.Popup.HasPopup && tipFlag)
            {
                StartCoroutine(Utils.DelayToInvokeDo(
                    delegate()
                    {
                        if (timeShowTipCount >= 4)
                        {
                            timeShowTipCount = 0;
                            GameController.Instance.AudioManager.PlayAudio("Vo_accompany_2");
                            tipFlag = false;
                        }
                    }, 2
                ));
            }
#endif
        }
    }

    /// <summary>
    /// 步數用完通知
    /// </summary>
    private void OnStepEmpty()
    {
        // 步數用完
        OpenAddMove();
        // 關卡失敗事件，其實並不是真的失敗了，步數用完了。
        if (GameController.Instance.ModelEventSystem.OnStageLose != null)
            GameController.Instance.ModelEventSystem.OnStageLose();
    }

    /// <summary>
    ///時間用完通知
    /// </summary>
    private void OnTimeEmpty()
    {
        if (linkInteractBeh.IsTouchDown)
        {
            RepealElimminate();
        }
        if (!isPlayBattleAttackProcess)
        {
            battleModel.CheckLevelGoal();
        }
    }

    private void OnReachGoal()
    {
        playingRuleCtr.CalcLeftStepAward();
        battleModel.SubmitFightRequest();
    }

    private void OnInit(ModelEventSystem.InitEventArgs args)
    {
        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "OnInit 1");
        eventLock++;
        //battleModel.Dump();
        int numRow = battleModel.CrtLevelConfig.NumRow;
        int numCol = battleModel.CrtLevelConfig.NumCol;
        eleViews = new GameObject[numRow, numCol];
        collectViews = new GameObject[numRow, numCol];
        obstacleViews = new GameObject[numRow, numCol];
        seperatorHViews = new GameObject[numRow, numCol];
        seperatorVViews = new GameObject[numRow, numCol];
        coverViews = new GameObject[numRow, numCol];
        bottomViews = new GameObject[numRow, numCol];
        interactRect = new RectTransform[numRow, numCol];

        BoardBackground.sprite = Resources.Load<Sprite>("Textures/BoardBackground/" + battleModel.CrtStageConfig.Map);
        UnitBackground.sprite = Resources.Load<Sprite>("Textures/UnitBackground/" + battleModel.CrtStageConfig.Map);
        PropBG.sprite = Resources.Load<Sprite>("Textures/UIPropButtonBG/" + battleModel.CrtStageConfig.Map);

        List<GameObject[,]> list = new List<GameObject[,]>();
        list.Add(eleViews);
        list.Add(collectViews);
        list.Add(obstacleViews);
        list.Add(seperatorHViews);
        list.Add(seperatorVViews);
        list.Add(coverViews);
        list.Add(bottomViews);

        List<TileObject[,]> dataList = new List<TileObject[,]>();
        dataList.Add(args.Elements);
        dataList.Add(args.Collects);
        dataList.Add(args.Obstacles);
        dataList.Add(args.SeperatorH);
        dataList.Add(args.SeperatorV);
        dataList.Add(args.Covers);
        dataList.Add(args.Bottom);

        //填充elementsLayer
        for (int c = 0, n = list.Count; c < n; c++)
        {
            GameObject[,] l = list[c];
            TileObject[,] data = dataList[c];
            for (int i = 0; i < numRow; i++)
            {
                for (int ii = 0; ii < numCol; ii++)
                {
                    //行從上到下排列
                    //列從左到右排列
                    if (data[i, ii] != null)
                    {
                        GameObject ga = GetTileGameObj(i, ii, data[i, ii]);
                        l[i, ii] = ga;
                        if (c == 0 && ga.GetComponent<ElementBehaviour>().Type == ElementType.Bomb)
                        {
                            //添加炸彈發光
                            UnityEngine.Object obj = Resources.Load("Prefabs/Effects/EffectJinengtubiao");//Prefab名字不能改，美術以後會改動
                            if (obj != null)
                            {
                                GameObject gameObject = GameObject.Instantiate(obj) as GameObject;
                                gameObject.name = "EffectJinengtubiao";
                                gameObject.transform.SetParent(ga.transform);
                                gameObject.transform.localPosition = Vector3.zero;
                            }
                        }
                    }
                }
            }
        }

        PlusInteractLock();
        //初始化掉落

        BattleInitDropHelper.Play(this, ElementLayer, eleViews, delegate()
        {
            MinusInteractLock();
            UpGuide();
        });

        //設置位置
        float anchorOffsetX = 1.0f / battleModel.CrtLevelConfig.NumCol;
        float anchorOffsetY = 1.0f / battleModel.CrtLevelConfig.NumRow;

        for (int i = 0; i < numRow; i++)
        {
            for (int ii = 0; ii < numCol; ii++)
            {
                //行從上到下排列
                //列從左到右排列
                GameObject ga = new GameObject();
                Image img = ga.AddComponent<Image>();
                img.material = BlockImgMaterial;
                //img.color = new Color(0, 0, 0, 0);
                ga.name = i + "$" + ii;
                ga.transform.SetParent(interactDownLayer);
                RectTransform rect = ga.transform as RectTransform;
                rect.localScale = new Vector3(1, 1, 1);
                //修改相應區域大小
                rect.sizeDelta = new Vector2(100, 100);
                rect.anchorMax = new Vector2(anchorOffsetX * (ii + 0.5f), 1 - anchorOffsetY * (i + 0.5f));
                rect.anchorMin = rect.anchorMax;
                rect.anchoredPosition3D = new Vector3(0, 0, 0);
                interactRect[i, ii] = rect;
            }
        }


        //填充interactLayer
        for (int i = 0; i < numRow; i++)
        {
            for (int ii = 0; ii < numCol; ii++)
            {
                //行從上到下排列
                //列從左到右排列
                GameObject ga = new GameObject();
                Image img = ga.AddComponent<Image>();
                img.material = BlockImgMaterial;
                //img.color = new Color(0, 0, 0, 0);
                ga.name = i + "_" + ii;
                ga.transform.SetParent(interactLayer);
                RectTransform rect = ga.transform as RectTransform;
                rect.localScale = new Vector3(1, 1, 1);
                //修改相應區域大小
                rect.sizeDelta = new Vector2(80, 80);
                rect.anchorMax = new Vector2(anchorOffsetX * (ii + 0.5f), 1 - anchorOffsetY * (i + 0.5f));
                rect.anchorMin = rect.anchorMax;
                rect.anchoredPosition3D = new Vector3(0, 0, 0);
                interactRect[i, ii] = rect;
            }
        }

        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "OnInit 2");
        UnitAnims = new Dictionary<ColorType, Animator>();
        SkillLoadings = new Dictionary<ColorType, SkillLoadingBehaviour>();
        BattleUnitInfo = new Dictionary<ColorType, BattleUnitInfoBehaviour>();
        List<UnitConfig> unitConfigs = new List<UnitConfig>();
        foreach (KeyValuePair<ColorType, Unit> pair in args.Units)
        {
            Unit unit = pair.Value;
            //設置技能圖標
            RectTransform posRect = UnitLayer.GetChild((int)unit.Config.UnitColor - 1) as RectTransform;
            //假定第一個是SkillLoading Component，默認隱藏
            SkillLoadingBehaviour skillLoading = posRect.GetChild(0).GetComponent<SkillLoadingBehaviour>();
            SkillConfig skillConf = gameCtrl.Model.SkillConfigs[unit.Config.UnitSkillId];
            Sprite skillIcon = gameCtrl.AtlasManager.GetSprite(Atlas.Tile, skillConf.ResourceIcon);
            Q.Assert(skillIcon != null);
            skillLoading.SetSprite(skillIcon);
            skillLoading.gameObject.SetActive(false);
            SkillLoadings.Add(unit.Config.UnitColor, skillLoading);

            //設置夥伴
            unitConfigs.Add(unit.Config);
        }

        //MoveBattleCameraToNext(true);

        StartCoroutine(StartLoadResource(unitConfigs));
        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "OnInit 3");

        eventLock--;
        Q.LogElapsedSeconds(GameController.Instance.TestPrefID, "OnInit 4");

        // 填充裝飾物層（从Start移到OnInit）
        FillDecorateLayer();
    }

    IEnumerator StartLoadResource(List<UnitConfig> unitConfigs)
    {
        yield return new WaitForEndOfFrame();
        LoadUnits(unitConfigs);
        yield return new WaitForSeconds(1f);
        LoadUnitInfo(unitConfigs);
    }

    /// <summary>
    /// 初始化地圖攝像機。   在舊邏輯中，發現雲彩打開後地圖攝像機會有個跳躍的動作
    /// MoveBattleCameraToNext中初始化邏輯抽離到此
    /// </summary>
    public void InitBattleCamera()
    {
        CrtBattleBeh.First(GameController.Instance.Model.BattleModel.CrtStageConfig);
    }

    public void MoveBattleCameraToNext(Action<EnemyPoint> callback = null)
    {
        Action<int> OnMoveComplete = null;
        OnMoveComplete = delegate(int index)
        {
            if (viewEventSystem.BattleFirstMoveCompleteEvent != null)
                viewEventSystem.BattleFirstMoveCompleteEvent();

            CrtBattleBeh.OnMoveComplete -= OnMoveComplete;
            battleModel.CrtEnemyIndex = CrtBattleBeh.EnemyIndex;
            CrtEnemyPoint = CrtBattleBeh.GetCurrentEnemyPoint();

            //Q.Log("MoveBattleCameraToNext::OnMoveComplete, CrtEnemyPoint.Data==null {0}", CrtEnemyPoint.Data == null);
            if (CrtEnemyPoint != null && CrtEnemyPoint.HasEnemy)
            {
                RoundNum = 0;
                SkillConfig EnemySkill = CrtEnemyPoint.GetSkillConfigByType(SkillType.Enemy);
                if (EnemySkill != null)
                {
                    TotalRoundNum = EnemySkill.SkillCD;
                }
                else
                {
                    //當這個怪物沒有技能的時候把總的技能CD設置為999
                    //表示這個怪物不能施放技能
                    TotalRoundNum = 999;
                }

                SkillConfig ImmuneSkill = CrtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
                if (ImmuneSkill != null)
                {
                    string spriteName = ImmuneSkill.ResourceIcon;
                    UnitAttackImmune(spriteName);
                }

                SetHpPos();
            }
            else
            {
                CancelUnitAttackImmune();
            }
            MinusInteractLock();
            if (callback != null)
                callback(CrtEnemyPoint);

            if (GameController.Instance.ViewEventSystem.OnBattleCameraMove != null)
            {
                //移動完成
                GameController.Instance.ViewEventSystem.OnBattleCameraMove(2);
            }
        };

        //移動之前把“攻擊免疫”的標記隱藏
        CancelUnitAttackImmune();
        PlusInteractLock();
        CrtBattleBeh.OnMoveComplete += OnMoveComplete;
        CrtBattleBeh.Next();

        if (GameController.Instance.ViewEventSystem.OnBattleCameraMove != null)
        {
            //移動開始 uibattle可能需要把血條隱藏
            GameController.Instance.ViewEventSystem.OnBattleCameraMove(0);
        }
    }

    private void LoadUnits(List<UnitConfig> unitConfigs)
    {
        this.gameObject.AddComponent(typeof(InitHelper));
        InitHelper initHelper = this.gameObject.GetComponent<InitHelper>();
        initHelper.UnitLayer = UnitLayer;
        //int count = 0;
        initHelper.OnProgress += delegate(ColorType color, GameObject ga)
        {
            Animator anim = ga.GetComponent<Animator>();

            UnitAnims.Add(color, anim);

            //UnitConfig config = unitConfigs[count];
            //count++;
            //loadUnitInfo(config, anim);
            //Q.Assert(anim != null);
            //Q.Assert(anim.gameObject.activeSelf);
            //Q.Assert(anim.isActiveAndEnabled);
            //Q.Assert(anim.GetBehaviour<BaseStateMachineBehaviour>() != null);
            //if (anim.GetBehaviour<BaseStateMachineBehaviour>()!=null)
            //{
            //    anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += OnUnitStateExit;
            //}
        };

        initHelper.OnComplete += delegate()
        {
            Q.Log(LogTag.TestPerf, "OnLoadUnitComplete {0}", Q.ElapsedSeconds(GameController.Instance.TestPrefID));
            Destroy(initHelper);
            InitBattleCamera();         //初始化地圖部分的攝像機，完成後再執行打開雲彩部分
            GameController.Instance.ViewEventSystem.JumpSceneHideCloudEvent(null, HideCloudOver);
        };
        initHelper.LoadUnits(unitConfigs, 0.5f);
    }

    private void HideCloudOver()
    {
        if (UIBattleBeh != null)
        {
            UIBattleBeh.PlayGoalPanelAnima();
        }

        MoveBattleCameraToNext();
    }

    void LoadUnitInfo(List<UnitConfig> unitConfigs)
    {
        int count = UnitLayer.childCount;
        for (int i = 0; i < count; i++)
        {
            Transform goTrans = UnitLayer.GetChild(i);
            UnitConfig config = null;

            for (int j = 0; j < unitConfigs.Count; j++)
            {
                if ((ColorType)(i + 1) == unitConfigs[j].UnitColor)
                {
                    config = unitConfigs[j];
                }
            }

            GameObject prefab = Resources.Load<GameObject>("Prefabs/BattleUnitInfo");
            RectTransform target = Instantiate(prefab).transform as RectTransform;
            target.SetParent(UnitPanel);
            target.localPosition = new Vector3(0, 0, 0);
            target.localScale = new Vector3(1, 1, 1);

            RectTransform targetUnitRect = goTrans.transform as RectTransform;
            Vector3 unitWorldPoint = targetUnitRect.TransformPoint(0, 0, 0);
            Vector3 unitScreenPoint = BoardCamera.WorldToScreenPoint(unitWorldPoint);
            Vector2 tempPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UnitPanel, unitScreenPoint, BoardCamera, out tempPoint);
            target.localPosition = new Vector3(tempPoint.x, tempPoint.y - 65, 0);

            BattleUnitInfoBehaviour unitInfoBeh = target.GetComponent<BattleUnitInfoBehaviour>();
            if (unitInfoBeh != null)
            {
                ColorType type = (ColorType)(i + 1);
                unitInfoBeh.initUnitConfig(battleModel, config, type);

                if (!BattleUnitInfo.ContainsKey(type))
                {
                    BattleUnitInfo.Add(type, unitInfoBeh);
                }
            }
        }
    }

    private void OnMove(ModelEventSystem.MoveEventArgs args)
    {
        eventLock++;
        List<List<ModelEventSystem.Move>> moves = args.Moves;
        //Q.Log("onMove dropTimes={0}", moves.Count);
        PlusInteractLock();

        //調用專門的輔助類，完成後再解鎖界面
        DropElementHelper.Play(this, moves,
            delegate()
            {
                //setAnimation(false);
                MinusInteractLock();
                eventLock--;
            }
        );
    }

    public void ActiveAllElementAnimator(bool flag)
    {
        for (int r = 0, numRow = eleViews.GetLength(0); r < numRow; r++)
        {
            for (int c = 0, numCol = eleViews.GetLength(1); c < numCol; c++)
            {
                GameObject ga = eleViews[r, c];
                if (ga == null)
                    continue;
                Animator animator = ga.transform.GetComponent<Animator>();
                animator.enabled = flag;
            }//for
        }
    }

    public void OnClickUnit()
    {
        if (!initAnimComplete || interactLock > 0)
            return;

        //正在新手引導中
        if (UIBattleBeh != null && UIBattleBeh.BoardGuide.activeInHierarchy)
            return;
        if (isInLinkGuide)
            return;
        //         ///如果夥伴動畫沒播完不收起///
        //         if (IsUnitButtonLock)
        //         {
        //             return;
        //         }

        isShowUnitPanel = !isShowUnitPanel;
        UnitPanel.gameObject.SetActive(isShowUnitPanel);
        foreach (var item in BattleUnitInfo)
        {
            item.Value.UpdateSkill();
        }
    }

    void UnitButton_OnClick(GameObject button)
    {
        if (IsUnitButtonLock || isInLinkGuide)
        {
            return;
        }

        ///播放夥伴動畫/////
        foreach (var ani in UnitAnims)
        {
            if (ani.Value.transform.parent == button.transform.parent)
            {
                PropCtr propCtr = GameController.Instance.PropCtr;
                PropType propType = PropType.NoCDSkill;

                if (propCtr.GetPropSelect(propType))
                {
                    ShowBoardTipsHelper.Stop(this);

                    Action noCDEvent = null;
                    noCDEvent = delegate()
                    {
                        NoCDSkillHelper.Play(ani.Key);
                        //ActivePropItemDic[propType].UpdateUI();
                        UsedActiveProp(propType);
                    };
                    ///使用技能投擲道具//
                    propCtr.UseProp(propType, noCDEvent);

                    return;
                }


                PlayUnitAnimationOnClick(ani.Value);
                break;
            }
        }
    }

    void PlayUnitAnimationOnClick(Animator ani)
    {
        ///不是待機動作不能點擊夥伴出面板////
        AnimatorStateInfo info = ani.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(GuyAnim.IDLE) && !info.IsName(GuyAnim.IDLE2))
            return;

        ///打開或者關閉面板////
        OnClickUnit();

        IsUnitButtonLock = true;

        ani.SetTrigger("TriggerWin");

        StartCoroutine(Utils.DelayToInvokeDo(delegate()
        {
            IsUnitButtonLock = false;

            if (!ani.GetCurrentAnimatorStateInfo(0).IsName(GuyAnim.IDLE)
                        && !ani.GetCurrentAnimatorStateInfo(0).IsName(GuyAnim.IDLE2))
            {
                ani.SetTrigger("TriggerIdle");

            }
        }, 2f));

    }


    private void OnBoardRearrange(ModelEventSystem.RearrangeEventArgs e)
    {
        Q.Log("OnBoardRearrange");
        eventLock++;
        PlusInteractLock();
        //改成圖片
        Sprite textSprite = gameCtrl.AtlasManager.GetSprite(Atlas.UIBattle, "Battle040");
        gameCtrl.Popup.ShowTextFloat(textSprite, this.transform as RectTransform);
        RearrangeElementHelper.Play(
            e.NewElementData, e.OriPositions,
            battleModel.CrtLevelConfig.NumRow, battleModel.CrtLevelConfig.NumCol,
            ElementLayer, eleViews,
            delegate()
            {
                MinusInteractLock();
                eventLock--;
                Q.Log("OnRearrange Complete.");

                if (eventQueue.Count == 0)
                {
                    playingRuleCtr.__CheckModelAndView();
                }
            });

    }


    /// <summary>
    /// 更新新手引導
    /// </summary>
    private bool _isGuideLink;
    private void UpGuide()
    {
        if (IsTreeActivity)
            return;
        _isGuideLink = false;
        int step = battleModel.StageLimit - battleModel.RemainSteps;
        if (step >= 0 && GuideUtility.GuideIDs.ContainsKey(battleModel.CrtStageConfig.ID))
        {
            if (UIBattleBeh.GuideConfigData.GetStepLength(battleModel.CrtStageConfig.ID) > step)
            {
                LockTile(UIBattleBeh.GuideConfigData.GetGuidePointsAt(battleModel.CrtStageConfig.ID, step));
                List<RectTransform> listV3 = new List<RectTransform>();
                //for (int i = 0; i < UIBattleBeh.GuideConfigData.GuidePoints[step].Length; i++)
                for (int i = 0; i < UIBattleBeh.GuideConfigData.getCurrentPoints(battleModel.CrtStageConfig.ID)[step].Length; i++)
                {
                    int pos = UIBattleBeh.GuideConfigData.getCurrentPoints(battleModel.CrtStageConfig.ID)[step][i];
                    int x = pos % 7;
                    int y = pos / 7;
                    RectTransform rectTransform = eleViews[y, x].GetComponent<RectTransform>();

                    AudioSource asour = rectTransform.GetComponent<AudioSource>();
                    if (asour != null)
                        asour.enabled = false;

                    listV3.Add(rectTransform);
                }
                guideLinkCont = listV3.Count;
                _isGuideLink = guideLinkCont > 0;
                switch (GuideManager.getInstance().version)
                {
                    case GuideVersion.Version_1:
                        {
                            UIBattleBeh.PlayGuide(step, listV3, this.OnStepLink);
                        }
                        break;
                    default:
                        break;
                }

                tipsTimer.Stop();

                isInLinkGuide = true;
                userTouchDown = false;
            }
            else
            {
                UIBattleBeh.StopGuide();

                isInLinkGuide = false;
            }
            if (initAnimComplete)
            {
                if (GuideManager.getInstance().version == GuideVersion.Version_1)
                {
                    //彈出熊貓
                    if (step == 2)
                    {
                        if (IsGuide)
                        {
                            //顯示敵人
                            bool hasEnemy = model.BattleModel.GetCrtEnemy() != null;
                            CrtBattleBeh.OnEnemyPoitsShow(hasEnemy);
                            //顯示效果層
                            ChargeEffectBehaviour.ColorLayerRoots[2].gameObject.SetActive(true);
                            //ChargeEffectBehaviour.ChargeEffLayer.transform.FindChild("Wood").gameObject.SetActive(true);
                            //
                            GameObject gb = UnitLayer.GetChild((int)ColorType.Wood - 1).gameObject;
                            gb.SetActive(true);
                            //添加煙霧效果      
                            GameObject effect = Instantiate<GameObject>(pandaSpawnEffect);
                            effect.transform.SetParent(gb.transform);
                            effect.transform.position = new Vector3(gb.transform.position.x, gb.transform.position.y, 0);
                            effect.gameObject.SetActive(true);
                            //播放完後，再把它幹掉
                            LeanTween.delayedCall(3, delegate()
                            {
                                Destroy(effect);
                            });
                        }
                        IsGuide = false;
                    }
                }
                GuideManager.getInstance().StartFightGuide(battleModel.CrtStageConfig.ID, step);
            }
        }
    }

    private bool _userTouchDown;
    private bool userTouchDown
    {
        get
        {
            return _userTouchDown;
        }
        set
        {
            _userTouchDown = value;
            tipFlag = true;
            if (_userTouchDown)
            {
                isFightGuide = false;
                for (int i = 0; i < guideGameObjPath.Count; i++)
                {
                    Link(guideGameObjPath[i]);
                }
                if (guideGameObjPath.Count > 0)
                    this.RepealElimminate();

                guideGameObjPath.Clear();
            }

            if (UIBattleBeh != null)
            {
                if (_userTouchDown)
                {
                    UIBattleBeh.PauseGuide();
                }
                else
                {
                    LeanTween.delayedCall(0.5f, delegate()
                    {
                        if (!_userTouchDown)
                        {
                            UIBattleBeh.ResumeGuide();
                        }
                    });
                    //UIBattleBeh.ResumeGuide();
                }
            }
        }
    }


    private List<GameObject> guideGameObjPath = new List<GameObject>();
    /// <summary>
    /// 第連到一個元素就回調
    /// </summary>
    private void OnStepLink(List<RectTransform> path, int index)
    {
        if (userTouchDown)
        {
            isFightGuide = false;
            for (int i = 0; i < guideGameObjPath.Count; i++)
            {
                Link(guideGameObjPath[i]);
            }
            this.RepealElimminate();
            guideGameObjPath.Clear();
            return;
        }
        isFightGuide = true;
        guideGameObjPath.Clear();
        for (int i = 0; i < path.Count; i++)
        {
            guideGameObjPath.Add(path[i].gameObject);
        }

        GameObject ga = path[index - 1].gameObject;
        Link(ga);

        if (path.Count == index)
        {
            LeanTween.delayedCall(0.1f, delegate()
            {
                if (!IsHaveResult)
                {
                    guideGameObjPath.Clear();
                    this.RepealElimminate();
                }
            });

            //暫停1秒
        }
        tipsTimer.Stop();
    }

    private void Link(GameObject ga)
    {
        BaseTileBehaviour crtEle = ga.GetComponent<BaseTileBehaviour>();
        int aCount = eliminateQueue.Count;
        BaseTileBehaviour prevEle = null;
        if (eliminateQueue.Count > 0)
        {
            Position lastPos = eliminateQueue[eliminateQueue.Count - 1];
            prevEle = eleViews[lastPos.Row, lastPos.Col].GetComponent<BaseTileBehaviour>();
        }


        int linkResturn = crtEle.Link(eliminateQueue);
        if (linkResturn == 1)
        {
            if (eliminateQueue.Count < aCount && prevEle != null)
            {
                //記得考慮在 BaseTileBehaviour 裡給出調用彈動動畫的方法
                Animator prevEleAnim = prevEle.GetComponent<Animator>();
                if (prevEleAnim != null)
                {
                    prevEleAnim.enabled = true;
                    prevEleAnim.SetTrigger("TriggerJellyGuide");
                }
            }
        }
        else if (linkResturn == 0)
        {
            Animator anim = crtEle.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = true;
                anim.SetTrigger("TriggerJellyGuide");
            }
        }
    }


    private void OnModelEliminate(ModelEventSystem.EliminateEventArgs args)
    {
        UnityEngine.Debug.LogFormat("BoardBeh:OnModelEliminate, {0}.{1}.{2}.{3}",
            args.OriTileDatas.Count,
            args.NewTileDatas.Count,
            args.ElimOrders.Count,
            args.DropList.Count);

        eventLock++;

        //  RemainSteps 修改後自動發事件，不手動刷新
        //if (!IsTreeActivity)
        //{
        //    //設置UI
        //    if (battleModel.CrtStageConfig.Mode != BattleMode.TimeLimit)
        //        UIBattleBeh.SetSteps(battleModel.RemainSteps);
        //}

        //TODO    暫時直接在此拋model事件，待修改 在控制層，數據被改變後直接拋事件
        if (modelEventSystem.OnBattleInfoUpdate != null)
        {
            modelEventSystem.OnBattleInfoUpdate(ModelEventSystem.BattleInfoType.Goal);
        }

        //播放第一次消除竹筍的聲音
        PlayFirstBambooAudioEffect(args.OriTileDatas);

        //播放消除特效
        List<TileObject> data = args.OriTileDatas;
        Q.Assert(data.Count >= 3);
        //消除特效在每次移除前做清理
        EliminateEffect.ClearAll();
        int EleCount = 0;

        for (int i = 0, n = data.Count; i < n; i++)
        {
            if (data[i].Config.ObjectType == TileType.Element)
                EleCount++;
        }

        //去掉“連消X.X”的效果，改成Slogan
        ComboEffBeh.SetCombatCmoboEffect(false);
        //出現Combo飄字
        ComboEffBeh.PlayComboSlogan(EleCount);

        //播放攻擊過程
        //SetLockPanelVDisplay(true);

        //普通關卡的攻擊
        int oriSlider1Value = 0;
        if (!IsTreeActivity)
            oriSlider1Value = (int)UIBattleBeh.SliderEnemyHp1.value;
        Action<int, int, int> attackCountCallBack1 = delegate(int count, int totalCount, int totalHurtValue)
        {
            if (count < totalCount)
            {
                //UIBattleBeh.SliderEnemyHp1是下面的血條
                //UIBattleBeh.SliderEnemyHp2是上面的血條
                //讓Slider1逐漸逼近Slider2
                float r = 1 - (float)count / totalCount;
                UIBattleBeh.SliderEnemyHp1.value =
                    UIBattleBeh.SliderEnemyHp2.value +
                    (int)((oriSlider1Value - UIBattleBeh.SliderEnemyHp2.value) * r);
            }
            else
            {
                UIBattleBeh.SliderEnemyHp1.value = UIBattleBeh.SliderEnemyHp2.value;
            }
        };

        //大樹關卡的攻擊        
        Action<int, int, int> attackCountCallBack2 = delegate(int count, int totalCount, int totalHurtValue)
        {
            Q.Log(LogTag.Battle, "attackCount {0},{1},{2}", count, totalCount, totalHurtValue);
            double val = Math.Floor(1.0 / totalCount * totalHurtValue);
            TreeFightCtr.AtkDamage((int)val);
        };

        Action attackProgressCompleteCallback = delegate()
        {
            isPlayBattleAttackProcess = false;
            //Q.Log(GetType().Name + ":attackProgressCompleteCallback CrtEnemyIndex={0}", battleModel.CrtEnemyIndex);
            if (modelEventSystem.OnBattleInfoUpdate != null)
            {
                modelEventSystem.OnBattleInfoUpdate(ModelEventSystem.BattleInfoType.Goal);
            }

            if (UIBattleBeh != null)
            {
                //TODO uibattle監聽score數據層事件，或者監聽分數的顯示層刷新事件
                UIBattleBeh.SetScore(battleModel.Score);
            }

            RoundNum++;
            battleModel.CheckLevelGoal();
            //檢查重新排列
            if (!playingRuleCtr.CheckElimatablePath(null, battleModel.LinkableStack))
            {
                // 取消連線
                CancelLink();
                // 重新排列
                playingRuleCtr.Rearrange();
            }

            eventLock--;
            if (UIBattleBeh != null)
            {
                UIBattleBeh.EnemyBloodVolume.gameObject.SetActive(false);
                UIBattleBeh.ScoreBgSlider.gameObject.SetActive(false);
            }
            // 元素會在回合結束漸變亮起，這裡註釋了
            //SetProminentElement(ColorType.All);

            // 取消所有夥伴的蓄力動作（強制保護）
            SetChargeEffect(ColorType.None, 0);
            ForceUnitsIdle();
            ForceEnemyIdleOrWeakIdle();
            UpGuide();


            if (eventQueue.Count == 0)
            {
                playingRuleCtr.__CheckModelAndView();
            }

            // 上拋剩餘步數的事件
            if (modelEventSystem.OnLastStep != null)
                modelEventSystem.OnLastStep(battleModel.RemainSteps);

            ///延遲原因是如果有結果要等update裡面的結果函數執行才能知道ishaveresult的結果///
            StartCoroutine(Utils.DelayToInvokeDo(delegate()
            {
                if (battleModel.RemainSteps == 5 && !IsHaveResult)
                {
                    // 還剩下5步，播放台詞音效
                    GameController.Instance.AudioManager.PlayAudio("Vo_accompany_1");
                }
            }, 0.3f));
        };
        //如果是大樹活動，為了節約時間，不飛向夥伴，直接在敵人身上爆炸，夥伴直接播放對應攻擊動作
        isPlayBattleAttackProcess = true;
        BattleAttackProcessHelper.Play(this, args, model, !IsTreeActivity,
            attackProgressCompleteCallback,
            IsTreeActivity ? attackCountCallBack2 : attackCountCallBack1);
        ///從驚嚇動作中恢復待機動作///
        ///因為攻擊動作完成有恢復待機動作方法調用，所以這裡不再調用這個方法///
        //UnitShockBehaviour.Play(0, args.MainColor, UnitAnims);
    }


    public void HurtAddSkillCD()
    {
        TotalRoundNum += 2;
    }

    void PlayGemScale(RectTransform rect)
    {
        rect.gameObject.SetActive(true);
        Vector3 targetPos = new Vector3(0.0f, rect.localPosition.y + 200, 0.0f);
        float initY = rect.localPosition.y;
        LeanTween.moveLocal(rect.gameObject, targetPos, 1.0f).setEase(LeanTweenType.linear).setOnUpdate(delegate(Vector3 val)
        {
            float scale = 1 + (val.y - initY) / 300;
            rect.localScale = new Vector3(scale, scale, scale);
        }).setOnComplete(delegate()
        {
            rect.gameObject.SetActive(false);
            Destroy(rect.gameObject);
        });
    }

    /// <summary>
    /// 是否在連線區域上
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsLinkAbleArea(Vector2 pos)
    {
        Vector2 outPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform
            , pos, Camera.main, out outPos);

        return outPos.y < linkUnableArea;
    }

    /// <summary>
    /// 請求消除
    /// </summary>
    /// <returns>是否觸發消除</returns>
    private bool RequestModelEliminate()
    {
        if (eliminateQueue == null || eliminateQueue.Count == 0)
            return false;

        bool success = eliminateQueue.Count >= 3;

        if (isInLinkGuide && eliminateQueue.Count < guideLinkCont)
            success = false;

        if (!IsLinkAbleArea(UpPosition))
            success = false;

        //移除連線特效
        ClearDrawLine();

        if (success)
        {
            //播放大樹消除獎勵
            PlayTreeaActElimRewards(eliminateQueue);

            Eliminate(eliminateQueue);

            eliminateQueue.Clear();
        }
        else
        {
            RepealElimminate();
        }
        return success;
    }


    /// <summary>
    /// 第一次消除竹筍，需要播放特定音效
    /// </summary>
    private void PlayFirstBambooAudioEffect(List<TileObject> elimObjs)
    {
        //已經觸發過這個邏輯
        if (PlayerPrefsTools.HasKey(OnOff.FirstTimeBamboo, true))
            return;

        // 是否含有竹筍
        bool hasBamboo = false;

        for (int i = 0, n = elimObjs.Count; i < n; i++)
        {
            TileObject tile = elimObjs[i];
            if (tile.ConfigID == 236 || tile.ConfigID == 247)
            {
                hasBamboo = true;
                break;
            }
        }

        // 第一次消除筍，播放台詞音效
        if (hasBamboo)
        {
            PlayerPrefsTools.SetIntValue(OnOff.FirstTimeBamboo, 1, true);
            GameController.Instance.AudioManager.PlayAudio("Vo_accompany_12");
        }
    }


    /// <summary>
    /// 播放大樹活動中的消除獎勵
    /// </summary>
    /// <param name="linkPathLen">連接路徑的長度</param>
    /// <param name="tilesBeforeElim">總共消除的消除物數量</param>
    private void PlayTreeaActElimRewards(List<Position> linkPath)
    {
        //不在大樹活動中，直接退出
        if (!IsTreeActivity)
            return;

        List<Position> elimPath = null;
        List<int> orders = null;
        playingRuleCtr.CalcEliminatePath(linkPath, out elimPath, out orders);

        int eleCount = 0;
        for (int i = 0, n = elimPath.Count; i < n; i++)
        {
            Position pos = elimPath[i];
            if (eleViews[pos.Row, pos.Col] != null)
                eleCount++;
        }

        List<ItemQtt> rewards = TreeFightCtr.Eliminate(linkPath.Count, eleCount);

        //在最後一個消除元素的位置，飄出消除獎勵
        Position lastPos = linkPath[linkPath.Count - 1];
        RectTransform lastGOPos = eleViews[lastPos.Row, lastPos.Col].transform as RectTransform;
        for (int i = 0, n = rewards.Count; i < n; i++)
        {
            ItemQtt itemQtt = rewards[i];
            Sprite awardIcon = BattleTools.GetBattleAwardIconByID(itemQtt.type);
            Vector2 anchorMax = lastGOPos.anchorMax;
            Vector2 anchorMin = lastGOPos.anchorMin;
            Vector2 anchoredPosition = lastGOPos.anchoredPosition;

            LeanTween.delayedCall(i * 0.3f, delegate()
            {
                float randomX = UnityEngine.Random.Range(-300f, 300f);
                BattleTools.CreateBoardFlyAward(awardIcon, EliminateAwardLayer,
                    anchorMax, anchorMin, anchoredPosition,
                    randomX, itemQtt.Qtt);
            });
        }
    }

    /// <summary>
    /// 普通消除hurt默認不設置，對單個消除物消除時可以設置hurt上限，使得單個消除物必定被消除///
    /// </summary>
    /// <param name="linkPath"></param>
    /// <param name="afffectedQueue"></param>
    /// <param name="hurt"> 這個是用來計算單個消除物傷害的</param>
    /// <returns></returns>
    public void Eliminate(List<Position> linkPath)
    {
        StopAllBombFuseAudioEffect();
        playingRuleCtr.Eliminate(linkPath);
    }




    /// <summary>
    /// 連線消除失敗，回退
    /// </summary>
    /// <param name="mainColor"></param>
    private void RepealElimminate()
    {
        if (eliminateQueue.Count <= 0)
            return;
        if (IsHaveResult)
            return;

        Position firstPos = eliminateQueue[0];
        ColorType mainColor = eleViews[firstPos.Row, firstPos.Col].GetComponent<ElementBehaviour>().Config.ColorType;

        if (IsTreeActivity)
        {
            UIBattleTreeBeh.HpBackup();
        }
        else
        {
            UIBattleBeh.EnemyBloodVolume.gameObject.SetActive(false);
            UIBattleBeh.ScoreBgSlider.gameObject.SetActive(false);
            UIBattleBeh.SliderEnemyHp2.value = UIBattleBeh.SliderEnemyHp1.value;
        }

        ComboEffBeh.SetCombatCmoboEffect(false);
        //這裡要逆序逐個unlink
        for (int i = eliminateQueue.Count - 1; i >= 0; i--)
        {
            Position pos = eliminateQueue[i];
            BaseTileBehaviour beh = eleViews[pos.Row, pos.Col].GetComponent<BaseTileBehaviour>();
            if (beh != null && beh.IsLinked)
                beh.CancelLink(eliminateQueue);
        }
        Q.Assert(eliminateQueue.Count == 0);

        viewEventSystem.BoardLinkEvent(2, mainColor, eliminateQueue, null);
    }


    private void OnModelEvent(EventArgs arg)
    {
        eventQueue.Enqueue(arg);
    }
    //滑鼠按著移動
    public void OnDrag(PointerEventData eventData)
    {
        if (!initAnimComplete)
        {
            return;
        }

        if (ImmuneGO)
        {
            ImmuneOnDrag(2, eventData.position);
            
        }

    }

    //滑鼠點擊
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!initAnimComplete)
        {
            return;
        }

        if (eventData.pointerId != PointerID)
        {
            return;
        }


        if (!IsTreeActivity)
        {
            userTouchDown = true;
        }

    }

    //滑鼠鬆開
    public void OnPointerUp(PointerEventData eventData)
    {
        if (interactLock > 0)
            return;

        if (ImmuneGO)
        {
            GameObject.DestroyObject(ImmuneGO);
            ImmuneGO = null;
            isShield = false;
        }
    }

    void CancleChangeLink()
    {
        int numRow = battleModel.CrtLevelConfig.NumRow;
        int numCol = battleModel.CrtLevelConfig.NumCol;
        for (int row = 0; row < numRow; row++)
        {
            for (int col = 0; col < numCol; col++)
            {
                if (obstacleViews[row, col] != null)
                {
                    obstacleViews[row, col].GetComponent<BaseTileBehaviour>().SetHighLight(false);
                }
            }
        }
    }

    void ChangeLink(List<TileObject> oridata)
    {
        int numRow = battleModel.CrtLevelConfig.NumRow;
        int numCol = battleModel.CrtLevelConfig.NumCol;
        for (int row =0; row< numRow;row++ )
        {
            for (int col = 0; col < numCol; col++)
            {
                if (obstacleViews[row, col] != null)
                {
                    bool isinpath = false;
                    foreach (var pos in oridata)
                    {
                        if (pos == null || pos.Config.ObjectType != TileType.Obstacle)
                            continue;

                        if (pos.Row == row && pos.Col == col)
                        {
                            isinpath = true;
                            break;
                        }
                    }

                    obstacleViews[row, col].GetComponent<BaseTileBehaviour>().SetHighLight(isinpath);
                }
            }
        }

    }

    void OnLinkDragEvent(Position tilePos, Vector2 screenPos)
    {
        if (!initAnimComplete)
        {
            return;
        }

        //if (!IsLinkAbleArea(screenPos))
        //    UILinkTipsHelp.ShowTextTips("鬆開手指，取消連線");


        //if (battleModel.CrtStageConfig.ID == 1)
        if (GuideUtility.GuideIDs.ContainsKey(battleModel.CrtStageConfig.ID))
        {
            int step = battleModel.StageLimit - battleModel.RemainSteps;
            if (UIBattleBeh.GuideConfigData.GetStepLength(battleModel.CrtStageConfig.ID) > step)
            {
                if (eliminateQueue.Count == 0)
                {
                    return;
                }
            }
        }

        BaseTileBehaviour crtEle = GetLinkableElementAt(tilePos.Row, tilePos.Col);
        if (crtEle == null)
            return;

        if (crtEle.Data == null)
        {
            //某些情況下會觸發這種情況
            //可能是因為消除過程中仍然可以消除導致的
            Q.Assert(false, "OnDrag interactBlockName={0}", crtEle.name);
            return;
        }

        if (_isGuideLink)
        {
            // 新手引導會取消聲音，這裡要恢復
            AudioSource asour = crtEle.GetComponent<AudioSource>();
            if (asour != null)
                asour.enabled = true;
        }


        int aCount = eliminateQueue.Count;

        Q.Assert(crtEle != null, "{0}, {1}", crtEle.Row, crtEle.Col);
        int linkResturn = crtEle.Link(eliminateQueue);

        if (linkResturn == -1)
        {
            return;
        }

        Position first = eliminateQueue[0];
        ElementBehaviour firstEle = GetLinkableElementAt(first.Row, first.Col);
        ColorType mainColor = firstEle.Config.ColorType;

        BaseTileBehaviour prevEle = null;
        if (eliminateQueue.Count > 0)
        {
            Position lastPos = eliminateQueue[eliminateQueue.Count - 1];
            prevEle = eleViews[lastPos.Row, lastPos.Col].GetComponent<BaseTileBehaviour>();
        }

        int mode = 0;
        if (linkResturn == 1)
        {
            if (eliminateQueue.Count < aCount && prevEle != null)
            {
                //記得考慮在 BaseTileBehaviour 裡給出調用彈動動畫的方法
                Animator prevEleAnim = prevEle.GetComponent<Animator>();
                if (prevEleAnim != null)
                {
                    prevEleAnim.enabled = true;
                    prevEleAnim.SetTrigger("TriggerJelly");
                }
            }
            mode = 3;
            if (eliminateQueue.Count >= 3)
            {
                //UILinkTipsBehaviour.Show("手指上滑，取消連線");
                UILinkTipsHelp.ShowAniTips();
            }
        }
        else if (linkResturn == 0)
        {
            Animator anim = crtEle.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = true;
                anim.SetTrigger("TriggerJelly");
            }
            mode = 1;
            if (eliminateQueue.Count >= 3)
            {
                //if (UILinkTipsBehaviour.Instance != null)
                //    UILinkTipsBehaviour.Instance.OnClose();

                UILinkTipsHelp.CloseAniTips();
                UILinkTipsHelp.CloseTextTips();
            }
        }


        if (battleModel.GetCrtEnemy() != null)
        {
            // 敵人的護盾
            SkillConfig skillCfg = CrtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
            if (skillCfg != null && skillCfg.SkillColor == mainColor)
            {
                ChangeImmuneIcon(skillCfg.ResourceIcon + "HL");
            }
            else
            {
                ShowEnemyBloodVolume(mainColor, true);
            }
        }

        //顯示可以達到的分數進度
        ShowScoreBgValue(mainColor);

        ComboEffBeh.SetCombatCmoboEffect(true, eliminateQueue.Count);
        viewEventSystem.BoardLinkEvent(mode, mainColor, eliminateQueue, null);
    }

    void OnLinkUpEvent(Vector2 screenPos)
    {
        if (!initAnimComplete)
        {
            return;
        }

        if (GuideUtility.GuideIDs.ContainsKey(battleModel.CrtStageConfig.ID))
        {
            int step = battleModel.StageLimit - battleModel.RemainSteps;
            if (step >= 0 && UIBattleBeh.GuideConfigData.GetStepLength(battleModel.CrtStageConfig.ID) > step)
            {
                if (RequestModelEliminate())
                {
                    if (!IsTreeActivity)
                        userTouchDown = false;
                    UIBattleBeh.StopGuide();
                }
                else
                {
                    UpGuide();
                    if (guideAudioTime < 0)
                    {
                        GameController.Instance.AudioManager.PlayAudio("Vo_accompany_21");
                        guideAudioTime = 2f;
                    }

                }
                return;
            }
        }


        if (eliminateQueue.Count == 1)
        {
            ElementBehaviour lastEle = eleViews[eliminateQueue[0].Row, eliminateQueue[0].Col].GetComponent<ElementBehaviour>();
            if (lastEle.Type == ElementType.Bomb && lastEle.Config.ColorType == ColorType.Wood)
            {
                UIBattleTipsBehaviour.Show(UIBattleTipsBehaviour.AnimType.GreenBomb);
            }
        }

        RequestModelEliminate();
        tipsTimer.Start();
        SetEleAnimatorEnabled(true);
        SetProminentElement(ColorType.All);
        CancleChangeLink();
    }

    void OnLinkDownEvent(Position tilePos, Vector2 screenPos)
    {
        if (!initAnimComplete)
        {
            return;
        }
        // 停止提示
        timeShowTipCount = 0;
        ShowBoardTipsHelper.Stop(this);

        if (GuideUtility.GuideIDs.ContainsKey(battleModel.CrtStageConfig.ID))
        {
            int step = battleModel.StageLimit - battleModel.RemainSteps;
            if (UIBattleBeh.GuideConfigData.GetStepLength(battleModel.CrtStageConfig.ID) > step)
            {
                int[] steps = UIBattleBeh.GuideConfigData.GetGuidePointsAt(battleModel.CrtStageConfig.ID, step);
                if (steps != null && steps[0] != tilePos.Row * 7 + tilePos.Col)
                {
                    return;
                }
            }
        }

        Position crtPos = new Position(tilePos.Row, tilePos.Col);
        //如果點擊使用了道具，則不走正常連接的流程
        if (!UsePropLogic(crtPos, screenPos))
            NormalLinkCrtEle(crtPos, screenPos);
    }

    void DragToCancelLink(Vector2 pos)
    {
        if (!IsLinkAbleArea(pos))
            UILinkTipsHelp.ShowTextTips("鬆開手指，取消連線");
    }

    void UpCancelLink(Vector2 pos)
    {
        UpPosition = pos;
    }

    /// <summary>
    /// 普通元素連接的邏輯
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="pointerScreenPoint"></param>
    private void NormalLinkCrtEle(Position pos, Vector2 pointerScreenPoint)
    {
        ElementBehaviour crtEle = GetLinkableElementAt(pos.Row, pos.Col);
        if (crtEle == null)
            return;

        if (_isGuideLink)
        {
            // 新手引導會取消聲音，這裡要恢復
            AudioSource asour = crtEle.GetComponent<AudioSource>();
            if (asour != null)
                asour.enabled = true;
        }

        int linkReturn = crtEle.Link(eliminateQueue);

        ColorType mainColor = crtEle.Config.ColorType;

        if (linkReturn != -1)
        {
            // main color
            Position first = eliminateQueue[0];
            ElementBehaviour firstEle = GetLinkableElementAt(first.Row, first.Col);
            mainColor = firstEle.Config.ColorType;

            viewEventSystem.BoardLinkEvent(3, mainColor, eliminateQueue, null);
        }


        if (linkReturn != 0)
            return;

        PlayEnemyEffect(mainColor, pointerScreenPoint);
        ShowScoreBgValue(mainColor);
        PlayComboEffect(crtEle);
    }


    private delegate bool DelegateMethod(PropType a, Action b);
    private DelegateMethod delegateUse = GameController.Instance.PropCtr.UseProp;
    /// <summary>
    /// 使用道具的邏輯
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="pointerScreenPoint"></param>
    /// <returns></returns>
    private bool UsePropLogic(Position pos, Vector2 pointerScreenPoint)
    {
        PropCtr propCtr = GameController.Instance.PropCtr;
        PropType proptype = PropType.None;
        bool ret = false;

        ElementBehaviour crtEle = GetLinkableElementAt(pos.Row, pos.Col);
        ///按顏色排序道具邏輯///
        if (propCtr.GetPropSelect(PropType.RearrangeByColor))
        {
            proptype = PropType.RearrangeByColor;
            Action reaAc = delegate()
            {
                playingRuleCtr.RearrangeByColor();
                //ActivePropItemDic[proptype].UpdateUI();
                UsedActiveProp(proptype);
            };
            //propCtr.UseProp(proptype, reaAc);
            delegateUse(proptype, reaAc);
            ret = true;
        }
        ///使用單個消除道具邏輯//
        else if (propCtr.GetPropSelect(PropType.EliminateOne))
        {
            proptype = PropType.EliminateOne;
            if (EliminateHelper.IsCanEliminateByOne(pos.Row, pos.Col))
            {
                Action oneAc = delegate()
                {
                    ///使用消除單個消除物的道具步數不減少//
                    battleModel.IsCanMinusSteps = false;
                    if (EliminateHelper.EliminateOneElement(pos.Row, pos.Col))
                    {
                        if (crtEle != null)
                        {
                            PlayEnemyEffect(crtEle.Config.ColorType, pointerScreenPoint);
                            PlayComboEffect(crtEle);
                        }
                        //ActivePropItemDic[proptype].UpdateUI();
                        UsedActiveProp(proptype);
                    }
                    ///消除完成要把步數減少的鎖定打開//
                    battleModel.IsCanMinusSteps = true;
                };

                //propCtr.UseProp(proptype, oneAc);
                delegateUse(proptype, oneAc);
            }
            ret = true;
        }
        ///消除同種顏色道具邏輯///
        else if (propCtr.GetPropSelect(PropType.EliminateColor))
        {
            proptype = PropType.EliminateColor;
            if (crtEle != null)
            {
                Action colorAc = delegate()
                {
                    ///使用消除同種顏色道具步數不減少//
                    battleModel.IsCanMinusSteps = false;
                    PlayEnemyEffect(crtEle.Config.ColorType, pointerScreenPoint);
                    PlayComboEffect(crtEle);
                    EliminateHelper.EliminateColorElements(crtEle.Data.Config.ColorType);
                    //ActivePropItemDic[proptype].UpdateUI();
                    UsedActiveProp(proptype);
                    ///消除完成要把步數減少的鎖定打開//
                    battleModel.IsCanMinusSteps = true;
                };
                //propCtr.UseProp(proptype, colorAc);
                delegateUse(proptype, colorAc);
            }
            ret = true;
        }

        return ret;
    }

    void UsedActiveProp(PropType proptye)
    {
        if (ActivePropItemDic.ContainsKey(proptye))
        {
            UIBattlePropButton button = ActivePropItemDic[proptye] as UIBattlePropButton;

            if (button != null)
            {
                button.UpdateUI();
                button.PlayUsedEff();
            }
        }
    }



    /// <summary>
    /// 播放敵人相關特效
    /// </summary>
    /// <param name="mainColor"></param>
    /// <param name="pointerScreenPosition"></param>
    private void PlayEnemyEffect(ColorType mainColor, Vector2 pointerScreenPosition)
    {
        if (battleModel.GetCrtEnemy() != null)
        {
            SkillConfig skillCfg = CrtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
            if (skillCfg != null && skillCfg.SkillColor == mainColor)
            {
                ImmuneOnDrag(1, pointerScreenPosition);
                ChangeImmuneIcon(skillCfg.ResourceIcon + "HL");
            }
            else
            {
                ShowEnemyBloodVolume(mainColor, true);
            }
        }
    }


    /// <summary>
    /// 播放Combo的特效
    /// </summary>
    /// <param name="tile"></param>
    private void PlayComboEffect(ElementBehaviour tile)
    {
        ComboEffBeh.SetCombatCmoboEffect(true, eliminateQueue.Count);
        if (eliminateQueue.Count == 1)
        {
            tipsTimer.Stop();
            SetEleAnimatorEnabled(false);
            viewEventSystem.BoardLinkEvent(1, tile.Config.ColorType, eliminateQueue, null);
            SetProminentElement(tile.Data.Config.ColorType);

            //記得考慮在 BaseTileBehaviour 裡給出調用彈動動畫的方法
            Animator anim = tile.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = true;
                anim.SetTrigger("TriggerJelly");
            }
        }
    }





    #region 免疫功能

    private GameObject ImmuneGO;
    private bool isShield;
    public void ImmuneOnDrag(int count, Vector2 screenPoint)
    {
        if (count == 1 && ImmuneGO == null)
        {
            ImmuneGO = new GameObject();
            Sprite immune = gameCtrl.AtlasManager.GetSprite(Atlas.UIBattle, "ShieldHL");
            Image immuneImage = ImmuneGO.AddComponent<Image>();
            immuneImage.overrideSprite = immune;
            immuneImage.SetNativeSize();
            ImmuneGO.transform.SetParent(FlyLayer);
            ImmuneGO.transform.localScale = new Vector3(1, 1, 1);
            isShield = true;

            //1秒後提示防禦音效
            LeanTween.delayedCall(1, delegate()
            {
                if (isShield)
                {
                    GameController.Instance.AudioManager.PlayAudio("Vo_accompany_20");
                }
            });
        }
        if (ImmuneBehaviour != null)
        {
            ImmuneBehaviour.ImmuneOnDrag(count, screenPoint, ImmuneGO);
        }

    }

    public void BattleAttackProOver(ColorType colorType)
    {
        if (ImmuneBehaviour != null)
        {
            ImmuneBehaviour.battleAttackProOver(colorType, CrtEnemyPoint);
        }
    }

    /// <summary>
    /// 敵人受擊免疫的特效
    /// </summary>
    /// <param name="colorType"></param>
    public void EnemyHitImmuneEff(ColorType colorType)
    {
        if (ImmuneBehaviour != null)
        {
            ImmuneBehaviour.BattleEnemyHitEff(colorType, CrtEnemyPoint);
        }
    }

    private void ChangeImmuneIcon(string iconName)
    {
        if (ImmuneBehaviour != null)
        {
            ImmuneBehaviour.changeImmuneIcon(iconName);
        }

    }

    /// <summary>
    /// 攻擊免疫效果
    /// </summary>
    public void UnitAttackImmune(string spriteName)
    {
        if (ImmuneBehaviour != null)
        {
            ImmuneBehaviour.unitAttackImmune(spriteName);
        }
    }

    /// <summary>
    /// 隱藏免疫效果
    /// </summary>
    public void CancelUnitAttackImmune()
    {
        if (ImmuneBehaviour != null)
        {
            ImmuneBehaviour.cancelUnitAttackImmune();
            isShield = false;
        }
    }


    public void DestroyImmuneGo()
    {
        if (ImmuneGO)
        {
            GameObject.DestroyObject(ImmuneGO);
            ImmuneGO = null;
            isShield = false;
        }
    }

    #endregion 免疫功能

    public void SetEleAnimatorEnabled(bool enabled)
    {
        for (int r = 0, n = eleViews.GetLength(0); r < n; r++)
        {
            for (int c = 0, m = eleViews.GetLength(0); c < m; c++)
            {
                if (eleViews[r, c] == null)
                    continue;
                GameObject ga = eleViews[r, c];
                ga.GetComponent<Animator>().enabled = enabled;
                ga.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
        }
    }



    /// <summary>
    /// 控制開啟關閉棋盤黑色遮擋
    /// </summary>
    /// <param name="visible"></param>
    public void SetLockPanelVDisplay(bool visible)
    {
        if (LockPanel == null)
        {
            Debug.LogWarning("開啟 or 關閉棋盤黑色遮擋LockPanel == null，檢查事件是否有清除");
            return;
        }
        if (visible && !LockPanel.activeSelf)
        {
            LockPanel.SetActive(true);
            //LockPanel.GetComponent<MeshRenderer>().enabled = true;
            LockPanel.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
            LeanTween.value(LockPanel, 0, 1, 0.2f).setOnUpdate(delegate(float val)
            {
                if (LockPanel.activeSelf)
                    LockPanel.GetComponent<CanvasRenderer>().SetAlpha(val);
            });
        }
        else if (!visible && LockPanel.activeSelf)
        {
            //關閉遮擋
            LeanTween.value(LockPanel, 1, 0, 0.3f).setOnUpdate(delegate(float val)
            {
                if (LockPanel.activeSelf)
                    LockPanel.GetComponent<CanvasRenderer>().SetAlpha(val);
            }).setOnComplete(delegate()
            {
                LockPanel.SetActive(false);
                //LockPanel.GetComponent<MeshRenderer>().enabled = false;
            });
        }
    }


    void ShowScoreBgValue(ColorType color)
    {
        List<Position> elimPath = null;
        List<int> orders = null;
        playingRuleCtr.CalcEliminatePath(eliminateQueue, out elimPath, out orders);
        int hurtva = battleModel.CountEliminateHurt(battleModel.GetCrtEnemy(), color, elimPath);

        hurtva = gameCtrl.PropCtr.AddHurt(hurtva);

        int afscore = gameCtrl.PropCtr.AddScore(battleModel, hurtva, false);

        if (UIBattleBeh != null)
        {
            //TODO  在battlemodel內的分數被修改後立刻就能拋事件通知view 不需要在此做另外處理。 
            //也可以在某一時刻拋view層的事件通知uibattle刷新
            UIBattleBeh.ShowBgSlider(battleModel.Score + afscore);
        }
    }

    /// <summary>
    /// 設置UI層怪物預測掉血量
    /// </summary>
    /// <param name="colorType"></param>
    /// <param name="playTextAnimaction"></param>
    private void ShowEnemyBloodVolume(ColorType colorType, bool playTextAnimaction)
    {
        if (eliminateQueue.Count == 0)
            return;

        List<Position> elimPath = null;
        List<int> orders = null;
        playingRuleCtr.CalcEliminatePath(eliminateQueue, out elimPath, out orders);

        int comboLv = 1;
        float comboRate = 0;
        bool beShielded = false;

        Unit enemy = battleModel.GetCrtEnemy();
        //計算傷害值
        int hurtValue = battleModel.CalcAttackResult(
            enemy, colorType, elimPath,
            out comboLv, out comboRate, out beShielded);

        ///計算道具加成後傷害
        hurtValue = GameController.Instance.PropCtr.AddHurt(hurtValue);
        //Q.Log("預計傷害值:{0}", hurtValue);
        if (enemy != null && !IsTreeActivity)
        {
            UIBattleBeh.EnemyBloodVolume.gameObject.SetActive(true);
            UIBattleBeh.EnemyBloodVolume.Level = comboLv;
            UIBattleBeh.EnemyBloodVolume.Text = hurtValue.ToString() + "/" + enemy.Hp.ToString();

            if (playTextAnimaction)
            {
                UIBattleBeh.EnemyBloodVolume.GetComponent<Animator>().Play("BloodVolume");
            }

            UIBattleBeh.SliderEnemyHp2.minValue = 0;
            UIBattleBeh.SliderEnemyHp2.maxValue = UIBattleBeh.SliderEnemyHp1.maxValue;
            //Q.Log("crtHp={0}, h={1}, v={2}", crtEnemyPoint.Data.Hp, hurtValue, crtEnemyPoint.Data.Hp - hurtValue);
            UIBattleBeh.SliderEnemyHp2.value = Math.Max(0, enemy.Hp - hurtValue);
        }

        if (IsTreeActivity)
        {
            TreeFightCtr.AtkDamage(hurtValue, true);
        }
    }

    /// <summary>
    /// 增加分數
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="mainColor"></param>
    /// <param name="elements"></param>
    /// <param name="beShield"></param>
    /// <returns></returns>
    public void AddScore(int score)
    {
        int addsc = GameController.Instance.PropCtr.AddScore(battleModel, score,false);

        if (UIBattleBeh != null)
        {
            //待優化   battleModel的分數改變後發事件出來 其它的view監聽事件刷新顯示
            UIBattleBeh.SetScore(addsc,true);
        }
    }


    /// <summary>
    /// 設置需要突出的元素
    /// </summary>
    /// <param name="color"></param>
    public void SetProminentElement(ColorType color)
    {
        for (int r = 0, n = eleViews.GetLength(0); r < n; r++)
        {
            for (int c = 0, m = eleViews.GetLength(0); c < m; c++)
            {
                if (eleViews[r, c] == null)
                    continue;
                BaseTileBehaviour beh = eleViews[r, c].GetComponent<BaseTileBehaviour>();
                Q.Assert(beh.Data != null, "{0}", beh.gameObject.name);

                bool needAlpha = beh.Data.Config.ColorType == color || color == ColorType.All;
                if (!needAlpha && beh.Data.Config.ElementType == ElementType.MultiColor)
                {
                    foreach (ColorType ct in beh.Data.Config.AllColors)
                    {
                        if (color == ct)
                        {
                            needAlpha = true;
                            break;
                        }
                    }
                }
                //modify by ybq
                //beh.GetComponent<Animator>().enabled = false;
                beh.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                beh.GetComponent<Image>().color = new Color(1, 1, 1, needAlpha ? 1.0f : 0.25f);
            }
        }
    }


    /// <summary>
    /// 讓item與BoardBehaviour取消聯繫
    /// </summary>
    /// <param name="item"></param>
    public void DeattachElementGameObject(GameObject item)
    {
        BaseTileBehaviour beh = item.GetComponent<BaseTileBehaviour>();
        if (beh == null)
            Debug.Log("item's name is " + item.name);
        GameObject[,] list = GetTypeGameObjects(beh.Config.ObjectType);
        if (beh.Row == -1 && beh.Col == -1)
            return;
        //Q.Assert(list[beh.Row, beh.Col] == item);
        list[beh.Row, beh.Col] = null;
        beh.Data.Row = -1;
        beh.Data.Col = -1;
        return;
    }


    /// <summary>
    /// 回收回資源池
    /// </summary>
    /// <param name="item"></param>
    public void GcTileGameObj(GameObject item)
    {
        //避免重複GC
        Q.Assert(!eleGameObjPool.Contains(item), "BoardBeh:Gc assert 1");
        if (eleGameObjPool.Contains(item))
            return;

        //刪除炸彈之類的技能發光特效
        if (item.GetComponent<RectTransform>().Find("EffectJinengtubiao") != null)
        {
            GameObject.Destroy(item.GetComponent<RectTransform>().Find("EffectJinengtubiao").gameObject);//删除子级特效
        }

        DeattachElementGameObject(item);
        BaseTileBehaviour beh = item.GetComponent<BaseTileBehaviour>();
        //只對ElementBehaviour的GameObject重複利用
        bool recycle = beh.Config.ObjectType == TileType.Element;
        beh.Dispose();

        if (recycle)
        {
            //這裡要延遲，是因為Unity5.0.0有bug
            //詳見MakeMeCrash:Crash1()

            //不能在這裡延遲deactive，不然同一幀裡如果馬上調用GetTileGameObj，會導致異步錯誤
            //beh.StartCoroutine(Utils.DelayDeactive(item));
            //modify by ybq
            //item.GetComponent<Image>().enabled = false;
            Transform tf = item.transform.Find("EffectBoomIcon");
            if (tf != null)
                tf.gameObject.SetActive(false);

            item.SetActive(false);
            //item.GetComponent<Animator>().enabled = false;
            eleGameObjPool.Add(item);
        }
        else
            Destroy(item);
    }


    /// <summary>
    /// 獲取一個BaseTileBehaviour的GameObject
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public GameObject GetTileGameObj(int row, int col, TileObject data)
    {
        if (data == null)
            return null;


        GameObject ga = null;
        BaseTileBehaviour baseTile;
        if (data.Config.ObjectType == TileType.Element)
        {
            if (eleGameObjPool.Count > 0)
            {
                ga = eleGameObjPool[0];
                Image gaImage = ga.GetComponent<Image>();
                if (gaImage != null)
                {
                    gaImage.color = Color.white;
                }
                eleGameObjPool.RemoveAt(0);
            }
            else
            {
                ga = Instantiate(elementPrefab);
            }
            baseTile = ga.GetComponent<BaseTileBehaviour>();
        }
        else if (data.Config.ObjectType == TileType.Obstacle)
        {
            ga = Instantiate(obstaclePrefab);
            baseTile = ga.GetComponent<ObstacleBehaviour>();
        }
        else if (data.Config.ObjectType == TileType.Cover)
        {
            ga = Instantiate(coverPrefab);
            baseTile = ga.GetComponent<CoverTileBehaviour>();
        }
        else if (data.Config.ObjectType == TileType.Bottom)
        {
            ga = Instantiate(bottomPrefab);
            baseTile = ga.GetComponent<BottomTileBehaviour>();
        }
        else if (data.Config.ObjectType == TileType.SeperatorH)
        {
            ga = Instantiate(seprHPrefab);
            baseTile = ga.GetComponent<SeperatorBehaviour>();
        }
        else if (data.Config.ObjectType == TileType.SeperatorV)
        {
            ga = Instantiate(seprVPrefab);
            baseTile = ga.GetComponent<SeperatorBehaviour>();
        }
        else if (data.Config.ObjectType == TileType.Collect)
        {
            ga = Instantiate(collectPrefab);
            baseTile = ga.GetComponent<CollectBehaviour>();
        }
        else
        {
            return null;
        }

        switch (data.Config.ObjectType)
        {
            case TileType.Element:
                ga.transform.SetParent(ElementLayer);
                break;
            case TileType.Obstacle:
                ga.transform.SetParent(ObstacleLayer);
                break;
            case TileType.Cover:
                ga.transform.SetParent(CoverLayer);
                break;
            case TileType.SeperatorH:
                ga.transform.SetParent(SeperaterHLayer);
                break;
            case TileType.SeperatorV:
                ga.transform.SetParent(SeperaterVLayer);
                break;
            case TileType.Bottom:
                ga.transform.SetParent(BottomLayer);
                break;
            case TileType.Collect:
                ga.transform.SetParent(CollectLayer);
                break;
        }

        baseTile.GetComponent<Image>().enabled = true;
        baseTile.GetComponent<Image>().raycastTarget = false;
        Animator anim = baseTile.GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            Utils.ResetAnimatorParams(anim);
            anim.Play("Init");
        }
        baseTile.BoardBehaviour = this;
        baseTile.Data = data;
        SetElementAnchor(baseTile);

        // 星星收集器位置的偏移量
        if (data.Config.ObjectType == TileType.Collect)
        {
            RectTransform rect = baseTile.transform as RectTransform;
            if (rect != null)
            {
                int offset = int.Parse(data.Config.Arg4);
                rect.anchoredPosition3D = new Vector3(0, offset, 0);
            }
        }

        ga.SetActive(true);

        if (isWeakenElement && data.Config.ObjectType == TileType.Element)
        {
            // 弱化顯示（變成半透明效果）
            baseTile.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            baseTile.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
        return ga;
    }

    public void SetElementAnchor(BaseTileBehaviour baseTile)
    {
        SetElementAnchor(baseTile.Row, baseTile.Col, baseTile.transform as RectTransform);
    }

    public void SetElementAnchor(int row, int col, RectTransform rect)
    {
        //設置位置
        rect.localScale = new Vector3(1, 1, 1);
        float anchorOffsetX = 1.0f / battleModel.CrtLevelConfig.NumCol;
        float anchorOffsetY = 1.0f / battleModel.CrtLevelConfig.NumRow;
        rect.anchorMax = new Vector2(anchorOffsetX * (col + 0.5f), 1 - anchorOffsetY * (row + 0.5f));
        rect.anchorMin = rect.anchorMax;
        rect.anchoredPosition3D = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// 填充裝飾物層
    /// </summary>
    private void FillDecorateLayer()
    {
        const int numCol = 11;
        const int numRow = 7;
        int startCol = -2;
        int startRow = 0;
        AtlasManager atlasMgr = gameCtrl.AtlasManager;
        string decorate = "ObstacleGrassGreen1";
        int[] decorateIds = battleModel.CrtStageConfig.DecorateObjIds;
        int index = UnityEngine.Random.Range(0, decorateIds.Length - 1);
        int randDecorateId = decorateIds[index];
        if (model.TileObjectConfigs.ContainsKey(randDecorateId))
        {
            TileObjectConfig conf = model.TileObjectConfigs[randDecorateId];
            if (conf != null)
            {
                decorate = conf.ResourceIcon;
            }
        }

        if (!atlasMgr.ContainsAtlas(Atlas.Tile))
            atlasMgr.LoadAtlas(Atlas.Tile);
        GameObject prefab = (GameObject)Resources.Load("Prefabs/Map/Battle/DecorateItem");
        for (int r = 0; r < numRow + 1; r++)
        {
            for (int c = 0; c < numCol; c++)
            {
                int realR = r + startRow;
                int realC = c + startCol;
                if (realR >= 0 && realR < 7 && realC >= 0 && realC < 7)
                    continue;

                // 第8行上面有收集器
                if (realR == 7 && realC >= 0 && realC < 7 && collectViews[realR - 1, realC] != null)
                    continue;

                GameObject ga = GameObject.Instantiate(prefab);
                Image img = ga.GetComponent<Image>();
                ga.isStatic = true;
                img.sprite = atlasMgr.GetSprite(Atlas.Tile, decorate);
                img.SetNativeSize();
                ga.transform.SetParent(decorateLayer);
                ga.name = "Decorate" + realR + "_" + realC;
                RectTransform rect = ga.transform as RectTransform;
                float anchorOffsetX = 1.0f / numCol;
                float anchorOffsetY = 1.0f / numRow;
                rect.anchorMax = new Vector2(anchorOffsetX * (c + 0.5f), 1 - anchorOffsetY * (r + 0.5f));
                rect.anchorMin = rect.anchorMax;
                rect.anchoredPosition3D = new Vector3(0, 0, 0);
                rect.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    /// <summary>
    /// TODO 放到一個專門控制Link邏輯的類裡
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="colorType"></param>
    /// <param name="linkPath"></param>
    /// <param name="dragbackTile"></param>
    private void OnBoardLink(int mode,
                             ColorType colorType,
                             List<Position> linkPath,
                             BaseTileBehaviour dragbackTile)
    {
        List<Position> elimPath = null;
        List<int> orders = null;
        playingRuleCtr.CalcEliminatePath(linkPath, out elimPath, out orders);
        //ResetLinkStatus(elimPath);


        int mainColorCount = battleModel.GetColorNumByPath(linkPath, colorType);
        //設置消除物的抖動狀態
        SetElementShake(linkPath);

        List<int> ordPath;
        List<TileObject> oridata;
        List<TileObject> newdata;
        List<ItemQtt[]> itemqtt;
        playingRuleCtr.PreviewEliminate(linkPath, out oridata, out newdata, out ordPath, out itemqtt);

        // 炸彈效果狀態改變
        BoomEffectChannge(elimPath, linkPath.Count >= 3);

        //金幣，鑽石...高亮
        ChangeLink(oridata);

        if (!UnitAnims.ContainsKey(colorType))
            return;

        if (isFightGuide)
            return;

        //播放蓄力動作
        Animator anim = UnitAnims[colorType];
        //夥伴沒有被隱藏的時播放蓄力動作
        if (anim.transform.parent.gameObject.activeInHierarchy)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (linkPath.Count == 0 && (!stateInfo.IsName(UnitAnimation.Idle.ToString()) && !stateInfo.IsName(UnitAnimation.Idle2.ToString())))
            {
                anim.SetTrigger("TriggerIdle");
            }
            else if (linkPath.Count != 0 && !stateInfo.IsName(UnitAnimation.Charge.ToString()))
            {
                anim.SetTrigger("TriggerCharge");

                UnitSoundBehaviour us = anim.GetComponent<UnitSoundBehaviour>();
                if (us != null)
                {
                    us.PlayChargeSound();
                }
            }
        }


        //顯示技能球進度
        SkillLoadingBehaviour skillLoading = SkillLoadings[colorType];
        if (linkPath.Count == 0)
        {
            if (battleModel.GetCrtEnemy() != null)
            {
                SkillConfig skillCfg = CrtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
                if (skillCfg != null)
                {
                    ChangeImmuneIcon(skillCfg.ResourceIcon);
                }
            }
            skillLoading.gameObject.SetActive(false);
        }
        else if (battleModel.SkillConfDict.ContainsKey(colorType))
        {
            SkillConfig skillConf = battleModel.SkillConfDict[colorType];
            int skillCD = battleModel.SkillCDDict[colorType] + linkPath.Count;
            skillLoading.gameObject.SetActive(true);
            skillLoading.SetForecastPercentage(Math.Min(1, (float)skillCD / skillConf.SkillCD));
        }

        //夥伴受到驚嚇動畫
        UnitShockBehaviour.Play(mainColorCount, colorType, UnitAnims);

        float comboRate = 0;
        if (mainColorCount == 0)
        {
            //取消小伙伴蓄力特效
            SetChargeEffect(colorType, 0);
        }
        else if (mainColorCount > 0 && GameController.Instance.Model.ComboConfigs != null)
        {
            ComboConfig conf = GameController.Instance.Model.ComboConfigs[mainColorCount];
            comboRate = conf.ComboRate;
            //設置相關夥伴的蓄力特效
            SetChargeEffect(colorType, conf.ComboLevel);
            if (conf.ComboLevel >= 2)
            {
                SetStarEffect(colorType, conf.ComboLevel);
            }
        }
        ///怪物受到驚嚇動畫///
        MonstorShockHelper.MonstorShock(this, comboRate);
    }


    private void ResetLinkStatus(List<Position> elimPath)
    {
        for (int r = 0, numRow = eleViews.GetLength(0); r < numRow; r++)
        {
            for (int c = 0, numCol = eleViews.GetLength(1); c < numCol; c++)
            {
                Position p = new Position(r, c);
                bool isLinkedPos = elimPath != null && elimPath.Contains(p);
                if (eleViews[r, c] != null)
                {
                    eleViews[r, c].GetComponent<BaseTileBehaviour>().IsLinked = isLinkedPos;
                }
                if (obstacleViews[r, c] != null)
                {
                    obstacleViews[r, c].GetComponent<BaseTileBehaviour>().IsLinked = isLinkedPos;
                }
            }
        }
    }


    private void SetElementShake(List<Position> linkPath)
    {
        float comboRate = linkPath.Count == 0 ? 0 : GameController.Instance.Model.ComboConfigs[linkPath.Count].ComboRate;
        // 這裡是元素抖動
        for (int i = 0, n = linkPath.Count; i < n; i++)
        {
            Position pos = linkPath[i];
            GameObject go = eleViews[pos.Row, pos.Col];
            ElementBehaviour elem = go.GetComponent<ElementBehaviour>();
            if (elem != null)
            {
                float time = UnityEngine.Random.Range(0.0f, 1.0f);
                elem.PlayLinkShake(comboRate, time);
            }
        }
    }

    private void OnAffectedChannge(BaseTileBehaviour tile, bool isLink)
    {
        ElementBehaviour elem = tile as ElementBehaviour;
        if (elem == null)
            return;

        if (elem.Type != ElementType.Bomb)
            return;

        elem.SetBoomEffectEnable(isLink);
    }


    /// <summary>
    /// 把所有炸彈的火花特效都關閉
    /// </summary>
    private void StopAllBombFuseEff()
    {
        for (int r = 0, numRow = eleViews.GetLength(0); r < numRow; r++)
        {
            for (int c = 0, numCol = eleViews.GetLength(1); c < numCol; c++)
            {
                if (eleViews[r, c] != null)
                {
                    ElementBehaviour elemBeh = eleViews[r, c].GetComponent<ElementBehaviour>();
                    if (elemBeh != null && elemBeh.Type == ElementType.Bomb)
                    {
                        elemBeh.SetBoomEffectEnable(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void BoomEffectChannge(List<Position> elimPath,bool iscaneli)
    {
        //只激活消除路徑中的炸彈，其他不激活
        for (int r = 0, numRow = eleViews.GetLength(0); r < numRow; r++)
        {
            for (int c = 0, numCol = eleViews.GetLength(1); c < numCol; c++)
            {
                Position p = new Position(r, c);
                if (eleViews[r, c] == null)
                    continue;

                ElementBehaviour elemBeh = eleViews[r, c].GetComponent<ElementBehaviour>();

                if (elimPath.Contains(p) && elemBeh.Type == ElementType.Bomb)
                {
                    //elemBeh.SetBoomEffectEnable(true);
                    elemBeh.SetBoomEffectEnable(iscaneli);
                }
                else
                {
                    elemBeh.SetBoomEffectEnable(false);
                }
            }
        }
    }


    /// <summary>
    /// 強制所有夥伴播放Idle狀態
    /// </summary>
    public void ForceUnitsIdle()
    {
        //這裡的邏輯是，攻擊結束之後，少數情況下，夥伴會保持Charge狀態
        //此時強制讓其去到Idle狀態
        foreach (var pair in UnitAnims)
        {
            if (pair.Value == null)
                continue;
            //Q.Assert(pair.Value.GetCurrentAnimatorStateInfo(0).IsName(GuyAnim.IDLE),
            //    "BoardBehaviour:CancelAllUnitsCharge Assert 1");
            Utils.ResetAnimatorParams(pair.Value);
            if (!pair.Value.GetCurrentAnimatorStateInfo(0).IsName(GuyAnim.IDLE) && !pair.Value.GetCurrentAnimatorStateInfo(0).IsName(GuyAnim.IDLE2))
            {
                int idleRandom = UnityEngine.Random.Range(0, 2);
                if (idleRandom == 0)
                {
                    pair.Value.Play(GuyAnim.IDLE);
                }
                else
                {
                    pair.Value.Play(GuyAnim.IDLE2);
                }

            }

            pair.Value.speed = 1f;
        }
    }

    /// <summary>
    /// 如果有敵人強制播放待機狀態
    /// 因為可能出現怪物一直播放驚嚇動作，原因還待查//
    /// </summary>
    public void ForceEnemyIdleOrWeakIdle()
    {
        Animator ani = CrtEnemyPoint.EnemyAnimator;

        if (ani == null)
            return;
        if (!ani.GetCurrentAnimatorStateInfo(0).IsName(EnemyAnim.SHOCK))
            return;
        Q.Log(LogTag.Assert, "{0}::{1}-----!!!!!!!!!!!!!!!!!!!!!!----", GetType().Name, "ForceEnemyIdleOrWeakIdle");
        Unit crtEnemy = model.BattleModel.GetCrtEnemy();
        Utils.ResetAnimatorParams(ani);

        //if (crtEnemy.Hp < crtEnemy.Config.UnitHp / 2)
        if (crtEnemy.Hp < crtEnemy.HpMax / 2)
        {
            ani.Play(EnemyAnim.WEAK_IDLE);
        }
        else
        {
            ani.Play(EnemyAnim.IDLE);
        }
    }

    /// <summary>
    /// 提示計時器
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTipsTimer(object sender, ElapsedEventArgs e)
    {
        //Q.Log("OnTipsTimer");
        isTimeShowTip = true;
        timeShowTipCount++;
    }

    void SetHpPos()
    {
        Unit crtEnemy = GameController.Instance.Model.BattleModel.GetCrtEnemy();

        if (crtEnemy == null)
            return;

        int num = crtEnemy.Config.HpPos;

        //Debug.Log("------Num is "+num);

        if (num >= ImmuneSpPosition.Length)
        {
            Debug.LogWarning("config is wrong");
            return;
        }
        //float[] yPos = { 577f,570f, 567f };
        //Vector3 pos = new Vector3(0, yPos[num], 0); 
        if (ImmuneSp)
        {
            ImmuneSp.transform.localPosition = ImmuneSpPosition[num];
            //ImmuneSp.transform.localPosition = pos;
        }

        if (GameController.Instance.ViewEventSystem.OnBattleEnemyPosChange != null)
        {
            GameController.Instance.ViewEventSystem.OnBattleEnemyPosChange(BossHpPosition[num]);
        }
    }

    public int GetEliminateQueueNum()
    {
        return eliminateQueue.Count;
    }

    /// <summary>
    /// 播放聲音
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="loop"></param>
    public void PlaySimpleAudio(AudioClip clip, bool loop)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.enabled = true;
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.Play();
        }
    }


    /// <summary>
    /// 停止播放聲音
    /// </summary>
    public void StopSimpleAudio()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.enabled = false;
        }
    }

    /// <summary>
    /// 炸彈導火線音效計數
    /// </summary>
    private int bombFuseAudioCount;

    /// <summary>
    /// 炸彈導火線音效
    /// </summary>
    /// <param name="enable"></param>
    public void BombFuseAudioEffect(bool enable)
    {
        if (enable)
            bombFuseAudioCount++;
        else
            bombFuseAudioCount--;

        if (enable && bombFuseAudioCount == 1)
        {
            // play
            gameCtrl.QMaxAssetsFactory.CreateEffectAudio("SD_attack_detonator_burn", delegate(AudioClip clip)
            {
                if (clip != null)
                    PlaySimpleAudio(clip, true);
            });
        }
        else if (bombFuseAudioCount <= 0)
        {
            StopAllBombFuseAudioEffect();
        }
    }

    public void StopAllBombFuseAudioEffect()
    {
        bombFuseAudioCount = 0;
        StopSimpleAudio();
    }

    /// <summary>
    /// 設置所有道具按鈕是否可點擊狀態//
    /// </summary>
    /// <param name="buttonActive"></param>
    void SetPropButtonActive(bool buttonActive = true)
    {
        if (ActivePropItemDic == null)
            return;

        foreach (var ac in ActivePropItemDic)
        {
            if (ac.Value.BuyButton == null)
                continue; ac.Value.BuyButton.interactable = buttonActive;
        }
    }

    /// <summary>
    /// 道具購買回調///
    /// </summary>
    /// <param name="num"></param>
    /// <param name="res"></param>
    public void UpdateAndSelectCurItem(int num, GoodsItem goodsItem)
    {
        PropType proptype = (PropType)goodsItem.id;

        if (ActivePropItemDic.ContainsKey(proptype))
        {
            ActivePropItemDic[proptype].UpdateUI();

            if (gameCtrl.PropCtr.TemSelectedProp != PropType.None)
            {
                //自己打開商城購買道具也會調用到這裡。  當不是點擊道具直接購買時，不選中按鈕，只刷新數量。
                ActivePropItemDic[proptype].OnClickPropButton();
            }
        }
    }

    /// <summary>
    /// 道具按鈕點擊事件//
    /// </summary>
    /// <param name="proptype"></param>
    void PropExclusive(PropType proptype)
    {
        PropCtr propCtr = GameController.Instance.PropCtr;
        foreach (var ac in ActivePropItemDic)
        {
            if (ac.Key != proptype)
            {
                propCtr.SetPropSelect(ac.Key, false);
                ac.Value.UpdateUI();
            }
        }

        //技能冷卻道具按鈕點擊事件///
        if (proptype == PropType.NoCDSkill)
        {
            bool select = propCtr.GetPropSelect(proptype);

            foreach (var skill in SkillLoadings)
            {
                skill.Value.gameObject.SetActive(select);
                SkillConfig skillConf = battleModel.SkillConfDict[skill.Key];
                int skillCD = battleModel.SkillCDDict[skill.Key];
                skill.Value.SetForecastPercentage(Math.Min(1, (float)skillCD / skillConf.SkillCD));
            }
        }
    }

    /// <summary>
    /// 播放收集元素飛向目標
    /// </summary>
    public void PlayCollectFly(BaseTileBehaviour tile)
    {
        int id = tile.Data.Config.ID;
        if (!goalId.ContainsKey(id))
        {
            GcTileGameObj(tile.gameObject);
            return;
        }
        RectTransform eleRect = tile.transform as RectTransform;
        eleRect.transform.SetParent(FlyLayer);

        int index = goalId[id];
        Transform goaltf = UIBattleBeh.GetGoalTransform(index);
        Vector3 targetPos = goaltf.TransformPoint(0, 0, 0);
        Vector3 targetScreenPos = BoardCamera.WorldToScreenPoint(targetPos);
        Vector2 targetLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(FlyLayer, targetScreenPos, BoardCamera, out targetLocalPos);

        Action<object> onUpComplete = delegate(object rectTransform)
        {
            EffectSpawn es = goaltf.parent.GetComponent<EffectSpawn>();
            es.enabled = true;

            // 播放星星收集音效
            GameController.Instance.AudioManager.PlayAudio("SD_target_aura");

            Transform tf = rectTransform as Transform;
            tf.gameObject.SetActive(false);
            StartCoroutine(Utils.DelayToInvokeDo(delegate()
            {
                es.enabled = false;
                GcTileGameObj(tf.gameObject);
            }, 2.0f));
        };
        Action<object> onDwonComplete = delegate(object rectTransform)
        {
            Transform tf = rectTransform as Transform;
            LeanTween.moveLocal(tf.gameObject, targetLocalPos, 0.8f)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(onUpComplete)
                .setOnCompleteParam(tf);
        };
        Vector3 downPoint = eleRect.localPosition - new Vector3(0, 90, 0);
        LeanTween.moveLocal(eleRect.gameObject, downPoint, 0.3f)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(onDwonComplete)
            .setOnCompleteParam(eleRect);
        LeanTween.scale(eleRect.gameObject, new Vector3(0.7f, 0.7f, 1), 0.3f);
    }


    void OnTimeToLight(object sender, ElapsedEventArgs e)
    {
        lightFlag = true;
    }

    private List<ColorType> goalColor = new List<ColorType>();
    private static bool lightFlag;
    void CheckAndPlayLight()
    {
        if (lightFlag)
        {
            List<StageConfig.Goal> currentGoal = battleModel.CurrentGoal;
            goalColor.Clear();
            if (currentGoal.Count > 0)
            {
                for (int i = 0; i < currentGoal.Count; i++)
                {
                    StageConfig.Goal g = currentGoal[i];
                    if (g.Type == BattleGoal.Object && battleModel.ObjectGoal[g.RelativeID] < g.Num)
                    {
                        ColorType color = model.TileObjectConfigs[g.RelativeID].ColorType;
                        if (!goalColor.Contains(color))
                        {
                            goalColor.Add(color);
                        }
                    }
                }
            }
            if (goalColor.Count > 0)
            {
                ColorType color = goalColor[UnityEngine.Random.Range(1, goalColor.Count) - 1];
                this.PlayElementLight(color);
            }
            lightFlag = false;
        }

    }

    void PlayElementLight(ColorType color)
    {
        foreach (var item in eleViews)
        {
            if (item != null)
            {
                if (item.GetComponent<ElementBehaviour>().Config.ColorType == color)
                {
                    GameObject light = item.transform.Find("Light").gameObject;
                    light.SetActive(true);
                    light.GetComponent<Animator>().SetTrigger("Play");
                }
            }
        }
    }
    #region 教學系統
    void AddFliter()
    {
        GuideNode introSwordProp = new GuideNode();
        introSwordProp.TargetNode = null;
        introSwordProp.TarCamera = Camera.main;
        introSwordProp.index = 3;
        introSwordProp.ShowTips = true;
        introSwordProp.ShowMask = false;


        GuideNode clickSwordProp = new GuideNode();
        clickSwordProp.TargetNode = ActivePropItemDic[PropType.EliminateColor];
        clickSwordProp.TarCamera = Camera.main;
        clickSwordProp.index = 3;
        clickSwordProp.ShowMask = true;
        clickSwordProp.CallBack = delegate()
        {
            GameController.Instance.PropCtr.SetPropSelect(PropType.EliminateColor, true);
            ActivePropItemDic[PropType.EliminateColor].UpdateUI();
        };


        GuideNode useSwordProp = new GuideNode();
        if (eleViews[3, 3] != null)
        {
            useSwordProp.TargetNode = eleViews[3, 3].transform;
        }

        useSwordProp.TarCamera = Camera.main;
        useSwordProp.index = 3;
        useSwordProp.ShowMask = true;
        useSwordProp.CallBack = delegate()
        {
            delegateUse = GameController.Instance.PropCtr.UsePropByGuide;
            UsePropLogic(new Position(3, 3), new Vector2(230, 262));
            delegateUse = GameController.Instance.PropCtr.UseProp;
        };

        GuideNode endSwordProp = new GuideNode();
        endSwordProp.TargetNode = null;
        endSwordProp.TarCamera = Camera.main;
        endSwordProp.index = 3;
        endSwordProp.Delay = 2;
        endSwordProp.ShowTips = true;
        endSwordProp.ShowMask = false;


        GuideManager.getInstance().addGuideNode("IntroSwordProp", introSwordProp);
        GuideManager.getInstance().addGuideNode("ClickSwordProp", clickSwordProp);
        GuideManager.getInstance().addGuideNode("UseSwordProp", useSwordProp);
        GuideManager.getInstance().addGuideNode("EndSwordProp", endSwordProp);
        /////////////////////////////////////////////////////////////////////////////////////////////////
        GuideNode introWandProp = new GuideNode();
        introWandProp.TargetNode = null;
        introWandProp.TarCamera = Camera.main;
        introWandProp.index = 3;
        introWandProp.ShowTips = true;
        introWandProp.ShowMask = true;

        GuideNode clickWandProp = new GuideNode();
        clickWandProp.TargetNode = ActivePropItemDic[PropType.RearrangeByColor];
        clickWandProp.TarCamera = Camera.main;
        clickWandProp.index = 3;
        clickWandProp.ShowMask = true;
        clickWandProp.CallBack = delegate()
        {
            GameController.Instance.PropCtr.SetPropSelect(PropType.RearrangeByColor, true);
            ActivePropItemDic[PropType.RearrangeByColor].UpdateUI();
        };

        GuideNode useWandProp = new GuideNode();
        if (eleViews[3, 3] != null)
        {
            useWandProp.TargetNode = eleViews[3, 3].transform;
        }
        useWandProp.TarCamera = Camera.main;
        useWandProp.index = 3;
        useWandProp.ShowMask = true;
        useWandProp.CallBack = delegate()
        {
            delegateUse = GameController.Instance.PropCtr.UsePropByGuide;
            UsePropLogic(new Position(3, 3), new Vector2(230, 262));
            delegateUse = GameController.Instance.PropCtr.UseProp;
        };

        GuideNode endWandProp = new GuideNode();
        endWandProp.TargetNode = null;
        endWandProp.TarCamera = Camera.main;
        endWandProp.index = 3;
        endWandProp.ShowTips = true;
        endWandProp.Delay = 1;

        GuideManager.getInstance().addGuideNode("IntroWandProp", introWandProp);
        GuideManager.getInstance().addGuideNode("ClickWandProp", clickWandProp);
        GuideManager.getInstance().addGuideNode("UseWandProp", useWandProp);
        GuideManager.getInstance().addGuideNode("EndWandProp", endWandProp);
    }

    void RemoveFliter()
    {
        GuideManager.getInstance().removeGuideNode("IntroSwordProp");
        GuideManager.getInstance().removeGuideNode("ClickSwordProp");
        GuideManager.getInstance().removeGuideNode("UseSwordProp");
        GuideManager.getInstance().removeGuideNode("EndSwordProp");

        GuideManager.getInstance().removeGuideNode("IntroWandProp");
        GuideManager.getInstance().removeGuideNode("ClickWandProp");
        GuideManager.getInstance().removeGuideNode("UseWandProp");
        GuideManager.getInstance().removeGuideNode("EndWandProp");
    }
    #endregion
}
