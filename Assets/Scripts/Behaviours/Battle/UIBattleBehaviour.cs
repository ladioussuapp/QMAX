using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net.Protocols.Stage;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 戰鬥面板的控制----12/23
/// </summary>
public class UIBattleBehaviour : MonoBehaviour
{
    public event Action<object> OnClickPauseButton;

    public GameObject BoardGuide;
    public UIButtonBehaviour PauseButton;
    public Text TextSteps;
    public List<Image> GoalIcons;
    public List<Image> GoalIconsAnimationTex;
    public List<Text> GoalTexts;
    public List<Image> GoalCompleteIcons;
    public ImageTextBehaviour EnemyBloodVolume;
    public Slider SliderEnemyHp1;
    public Slider SliderEnemyHp2;
    public Slider SliderPlayerHp;
    //public Image ImmuneSp;

    public UIGoalTip UiGoalTipDisplay;
    public Animator animator;

    public Transform EnemyInfo;

    public Transform ReadyTime;
    public TreeBattleTipTimeEffect TipTimeTick;

    public ImageNumberBehaviour Score;
    public Slider ScoreSlider;

    /// <summary>
    /// 分數預計達到進度條
    /// </summary>
    public Slider ScoreBgSlider;
    //時間模式，事件進度顯示//
    public Slider TimeSlider;
    public Image StepOrTimeTitle;

    public GameObject[] Star;
    public GameObject[] GoalBg;

    public GameObject LessStepWaring;
    public GameObject ScoreText;

    public Image FlashImg;

    public GameObject[] StarAni;

    /// <summary>
    /// 分數目標值
    /// </summary>
    public ImageNumberBehaviour ScoreGoal;

    public Transform ScoreTick;

    public Text ScoreTipsNum;

    /// <summary>
    /// 分數提示父節點
    /// </summary>
    private Transform ScoreTipsParent;

    /// <summary>
    /// 顯示目標分數的父節點
    /// </summary>
    public Transform ScoreGoalParent;

    [HideInInspector]
    public List<int> units = new List<int>();
    /// <summary>
    /// 
    /// </summary>
    public Image StepOriImage;

    private int crtStageId;
    private GameObject lessStepTips;

    ///目標物體顯示///
    public Transform AllGoalParent;

    ///沒有分數時顯示界面//
    public Transform NoScore;
    [HideInInspector]
    public bool IsHaveResul = false;

    BattleModel battleModel;

    /// <summary>
    /// 1,2,3星分數///
    /// </summary>
    [HideInInspector]
    public int[] StarScore;

    private int TimeNum;

    bool shouldEnemyShow = true;

    public Transform TipsParent;

    public Animator AddStepsAni;

    /// <summary>
    /// 設置攝像機
    /// </summary>
    /// <param name="camera"></param>
    public void SetCamera(Camera camera)
    {
        for (int i = 0, n = transform.childCount; i < n; i++)
        {
            Canvas c = transform.GetChild(i).GetComponent<Canvas>();
            if (c != null)
                c.worldCamera = camera;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="battleModel"></param>
    public void SetData(BattleModel battleModel_)
    {
        battleModel = battleModel_;
        InitNum(battleModel.GetNoScoreGoalNum());
        SetDisplayType(battleModel.CrtStageConfig.InterfaceType);

        SetStarScore();

        if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
        {
            SetStepOrTimeImage(GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle, "battle-_029"));
            InitTimeSlider(battleModel.RemainTime);
        }

        if (GameController.Instance.StageCtr.IsAllScoreGoal(battleModel.CurrentGoal))
        {
            InitScoreGoal(battleModel.CurrentGoal);
        }

        SetStageId(battleModel.CrtStageConfig.ID);

        if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
            SetSteps(battleModel.RemainTime);
        else
            SetSteps(battleModel.RemainSteps);

        //InitGoalTip();
        UpdateGoalUI(battleModel.CurrentGoal, true);

        GameController.Instance.ButtonTipsCtr.TipsParent = TipsParent;
        AddEvents();
    }

    void AddEvents()
    {
        //GameController.Instance.ModelEventSystem.OnBuyMoves += modelEventSystem_OnBuyMoves;
        GameController.Instance.ModelEventSystem.OnBattleInfoUpdate += OnBattleInfoUpdate;
        GameController.Instance.ModelEventSystem.OnBuyTime += modelEventSystem_OnBuyTime;
        GameController.Instance.ModelEventSystem.OnLastTime += OnTimeTick;
        GameController.Instance.ViewEventSystem.OnBattleCameraMove += OnBattleCameraMove;
        GameController.Instance.ViewEventSystem.OnBattleEnemyPosChange += OnBattleEnemyPosChange;
        GameController.Instance.ViewEventSystem.OnBattleEnemyDisplayChange += OnBattleEnemyDisplayChange;
        GameController.Instance.ModelEventSystem.OnBattleGoalComplete += OnBattleGoalComplete;
    }

    void RemoveEvents()
    {
        //GameController.Instance.ModelEventSystem.OnBuyMoves -= modelEventSystem_OnBuyMoves;
        GameController.Instance.ModelEventSystem.OnBuyTime -= modelEventSystem_OnBuyTime;
        GameController.Instance.ModelEventSystem.OnLastTime -= OnTimeTick;
        GameController.Instance.ViewEventSystem.OnBattleCameraMove -= OnBattleCameraMove;
        GameController.Instance.ViewEventSystem.OnBattleEnemyPosChange -= OnBattleEnemyPosChange;
        GameController.Instance.ViewEventSystem.OnBattleEnemyDisplayChange -= OnBattleEnemyDisplayChange;
        GameController.Instance.ModelEventSystem.OnBattleInfoUpdate -= OnBattleInfoUpdate;
        GameController.Instance.ModelEventSystem.OnBattleGoalComplete -= OnBattleGoalComplete;
    }

    void OnBattleGoalComplete(int completeStar, List<StageConfig.Goal> nextGoal)
    {
        if (nextGoal != null)
        {
            UpdateGoalUI(nextGoal, true);
        }
    }

    /// <summary>
    /// 設置步數            在時間模式下 連線時也會調用到這裡 並且將步數減到了負數
    /// </summary>
    /// <param name="step"></param>
    void SetSteps(int step)
    {

        if (animator != null)
        {
            animator.enabled = true;
            if (step < 11)
                animator.SetTrigger("TriggerStep");
            //舉牌
            Action tips = delegate()
            {
                if (!IsHaveResul)
                {
                    lessStepTips.SetActive(true);
                    lessStepTips.GetComponentInChildren<Animator>().SetTrigger("play");
                }
            };
            //警告
            Transform light = this.ScoreText.transform.GetChild(0).Find("light");
            Action waring = delegate()
            {
                if (!IsHaveResul)
                {
                    this.LessStepWaring.SetActive(true);
                    light.gameObject.SetActive(true);
                    this.ScoreText.GetComponent<Animator>().SetTrigger("Play");
                }
                else
                {
                    this.LessStepWaring.SetActive(false);
                    light.gameObject.SetActive(false);
                }
            };

            //底板變色
            if (step <= 5)
            {
                if (step == 5)
                    LeanTween.delayedCall(1f, tips);
                //LeanTween.delayedCall(0f, waring);
                waring();
            }
            else
            {
                this.LessStepWaring.SetActive(false);
            }
        }
        if (TextSteps != null)
        {
            TextSteps.text = step.ToString();
        }

    }

    /// <summary>
    /// 設置時間(暫時不跟設置步數混用)
    /// </summary>
    /// <param name="time"></param>
    void SetTime(int time)
    {
        if (TextSteps != null)
        {
            TextSteps.text = time.ToString();
        }

        //TimeSlider.value = time;
        TimeNum = time;

        if (time <= 5.0f)
            StartFlashEffect();
        else if (FlashImg.gameObject.activeSelf)
            FlashImg.gameObject.SetActive(false);
    }

    void ShowStar(int score, bool isSetBg = false)
    {
        if (!Score.isActiveAndEnabled)
            ScoreSlider.value = 0;
        ///參數是0可以作為初始化//
        if (score == 0)
        {
            ScoreSlider.value = 0;
            ScoreBgSlider.value = 0;
            Star[0].SetActive(false);
            Star[1].SetActive(false);
            Star[2].SetActive(false);
            return;
        }

        if (StarScore == null || StarScore.Length < 3)
            return;

        int curscore = 0;
        int scorestep = 0;
        int starNum = 0;

        ///三個星級的進度//
        float[] point = { 0.32f, 0.6f, 1f };

        if (score < StarScore[0])
        {
            curscore = StarScore[0];
            scorestep = 0;
        }
        else if (score < StarScore[1])
        {
            curscore = StarScore[1];
            scorestep = 1;
            starNum = 1;

        }
        else if (score < StarScore[2])
        {
            curscore = StarScore[2];
            scorestep = 2;
            starNum = 2;

        }
        else
        {
            curscore = StarScore[2];
            scorestep = 2;
            starNum = 3;

        }

        float curpoint = 0;

        ///curscore為0說明沒有配置關卡分數目標//
        if (curscore != 0)
        {
            //不是一星以下階段//
            if (scorestep > 0)
            {
                curscore = curscore - StarScore[scorestep - 1];
                //前一個階段的進度加上當前分數比前個階段目標分數多出來的在當前階段的百分比//
                curpoint = point[scorestep - 1] + ((float)(score - StarScore[scorestep - 1]) / curscore) * (point[scorestep] - point[scorestep - 1]);
            }
            else
            {
                curpoint = ((float)score / curscore) * point[0];
            }
        }
        else
        {
            curpoint = 0;
        }

        if (isSetBg)
        {
            ScoreBgSlider.value = curpoint;
            return;
        }

        ///設置實際分數進度
        ScoreSlider.value = curpoint;

        ///分數背景進度值等於實際分數進度//
        ScoreBgSlider.value = ScoreSlider.value;

        ///設置星星顯示//
        Star[0].SetActive(starNum > 0);
        Star[1].SetActive(starNum > 1);
        Star[2].SetActive(starNum > 2);

        if (starNum > 0 && !StarAni[starNum - 1].activeInHierarchy)
        {
            animator.Play(string.Format("Star{0}", starNum));
        }


    }

    public void ShowBgSlider(int score)
    {
        ScoreBgSlider.gameObject.SetActive(true);
        ShowStar(score, true);
    }

    /// <summary>
    /// 達到目標分數
    /// </summary>
    void SetScoreComplete()
    {
        if (Score.Number < ScoreGoal.Number)
            return;

        if (ScoreTick != null)
            ScoreTick.gameObject.SetActive(true);
    }

    void InitScoreGoal(List<StageConfig.Goal> goal)
    {
        int num = 0;

        foreach (var go in goal)
        {
            if (go.Type == BattleGoal.Score)
                num = go.Num;
        }

        if (ScoreGoal != null)
            ScoreGoal.Number = num;

        //if (ScoreTipsNum != null)
        //    ScoreTipsNum.text = string.Format("{0:N0}", num);
    }

    public void DisplayScoreGoal(bool dis)
    {
        //if (dis && !ScoreTipsParent.gameObject.activeInHierarchy)
        //    animator.SetInteger("Goal", 5);

        ScoreGoalParent.gameObject.SetActive(dis);
        ScoreTipsParent.gameObject.SetActive(dis);
    }

    void InitTimeSlider(int timeMax)
    {
        if (TimeSlider != null)
        {
            TimeSlider.gameObject.SetActive(true);
            TimeSlider.maxValue = timeMax;
            TimeSlider.value = timeMax;
        }

        TimeNum = timeMax;

    }

    void SetStepOrTimeImage(Sprite sp)
    {
        if (StepOrTimeTitle != null)
            StepOrTimeTitle.sprite = sp;
    }

    //isAdd = true 表明是增加分數，反之則直接賦值
    public void SetScore(int score, bool isAdd = false)
    {
        if (isAdd)
            Score.Number += score;
        else
            Score.Number = score;

        ShowStar(Score.Number);

        SetScoreComplete();
    }

    public void SetStarScore()
    {
        StarScore = battleModel.StarScore;

        GameController.Instance.ButtonTipsCtr.StarScore = StarScore;
    }

    public void SetDisplayType(int Num)
    {

        /// < summary >
        /// 1 = 顯示目標和分數// 
        /// 2 = 顯示目標 //
        /// 3 = 顯示分數//
        /// </ summary >//
        /// 現在是顯示目標和分數//
        Num = 1;
        switch (Num)
        {
            case 1:
                Score.gameObject.SetActive(true);
                NoScore.gameObject.SetActive(false);
                AllGoalParent.gameObject.SetActive(true);

                break;
            case 2:
                Score.gameObject.SetActive(false);
                NoScore.gameObject.SetActive(true);
                AllGoalParent.gameObject.SetActive(true);
                break;
            case 3:
                Score.gameObject.SetActive(true);
                NoScore.gameObject.SetActive(false);
                AllGoalParent.gameObject.SetActive(false);
                break;
        }
    }

    public void SetStageId(int Id)
    {
        crtStageId = Id;
    }

    /// <summary>
    /// 設置目標數量
    /// </summary>
    /// <param name="goalNum"></param>
    public void SetGoalNum(int goalNum)
    {
        const int MaxNum = 4;
        Q.Assert(goalNum <= MaxNum);

        if (goalNum == 2)
        {
            for (int i = 0; i < AllGoalParent.childCount; i++)
            {
                Transform item = AllGoalParent.GetChild(i);
                HorizontalLayoutGroup horBeh = item.GetComponent<HorizontalLayoutGroup>();
                if (horBeh)
                {
                    horBeh.spacing = 70;
                }
            }
        }

        //for (int i = 0; i < MaxNum; i++)
        //{
        //    GoalIcons[i].gameObject.SetActive(i < goalNum);
        //    GoalTexts[i].gameObject.SetActive(i < goalNum);
        //}

        //if (goalNum > 0 && goalNum < 5)
        //    animator.SetInteger("Goal", goalNum);
    }

    /// <summary>
    /// 初始化敵人血量
    /// </summary>
    /// <param name="maxValue"></param>
    void InitEnemyHp(float maxValue, float value)
    {
        SliderEnemyHp1.minValue = 0;
        SliderEnemyHp1.maxValue = maxValue;
        SliderEnemyHp1.value = value;
        SliderEnemyHp2.minValue = 0;
        SliderEnemyHp2.maxValue = maxValue;
        SliderEnemyHp2.value = value;
    }


    /// <summary>
    /// 初始化玩家血量
    /// </summary>
    /// <param name="maxValue"></param>
    /// <param name="value"></param>
    void InitPlayerHp(int maxValue, float value)
    {
        SliderPlayerHp.minValue = 0;
        SliderPlayerHp.maxValue = maxValue;
        SliderPlayerHp.value = value;
    }

    /// <summary>
    /// 設置某個目標的icon
    /// </summary>
    /// <param name="index"></param>
    /// <param name="texture"></param>
    public void SetGoalImageAt(int index, Sprite sprite)
    {
        GoalIcons[index].sprite = sprite;
        GoalIconsAnimationTex[index].sprite = sprite;
    }


    /// <summary>
    /// 控制怪物血量是否顯示
    /// </summary>
    /// <param name="value"></param>
    void ControlEnemyHpDisplay(bool value)
    {
        SliderEnemyHp1.gameObject.SetActive(value);
        SliderEnemyHp2.gameObject.SetActive(value);
    }



    /// <summary>
    /// 設置某個目標的文字，或者打勾
    /// </summary>
    /// <param name="index"></param>
    /// <param name="text"></param>
    /// <param name="showCompleteIcon">true的話，不顯示文字，直接顯示完成符號</param>
    public void SetGoalText(int index, string text, bool showCompleteIcon = false)
    {
        if (showCompleteIcon)
        {
            GoalTexts[index].text = "";
            //GoalCompleteIcons[index].gameObject.SetActive(true);
        }
        else
        {
            GoalTexts[index].text = text;
            //GoalCompleteIcons[index].gameObject.SetActive(false);
        }

        GoalCompleteIcons[index].gameObject.SetActive(showCompleteIcon);
    }

    public void PlayGoalComplete(int index)
    {
        if (animator != null)
        {
            if (!GoalCompleteIcons[index].gameObject.activeInHierarchy)
            {
                animator.SetTrigger("GoalComplete" + (index + 1).ToString());
                // 目標達成音效
                GameController.Instance.AudioManager.PlayAudio("SD_target_draw");
            }
        }
    }

    void ChangeEnemyHpTo(float value)
    {
        SliderEnemyHp2.value = value;
        SliderEnemyHp1.value = value;
    }

    public void ChangePlayerHpTo(float value)
    {
        SliderPlayerHp.value = value;
    }

    private bool isGuidePlaying;
    private bool isPause = true;
    private List<RectTransform> listV3s;
    private static Action<List<RectTransform>, int> stepCallBack;
    public void PlayGuide(int step, List<RectTransform> listV3, Action<List<RectTransform>, int> callback = null)
    {
        if (callback != null) stepCallBack = callback;
        if (GuideConfigStep == -1)
        {
            if (GuideConfigData.GetStepLength(crtStageId) > step)
            {
                if (BoardGuide != null)
                {
                    if (!BoardGuide.activeInHierarchy)
                    {
                        listV3s = listV3;
                        GuideConfigStep = step;
                        if (GoalPanePlayComplete)
                        {
                            //GuideManager.getInstance().StartGuide(crtStageId, step);
                            BoardGuide.SetActive(true);
                            BoardGuide.transform.SetAsLastSibling();
                            StartGuide(new Vector3(0,100,0));
                            isGuidePlaying = true;
                        }
                    }
                }
            }
        }
    }
    public void PauseGuide()
    {
        if (!isGuidePlaying && !isPause) return;
        isPause = true;
        if (BoardGuide.activeInHierarchy)
        {
            BoardGuide.SetActive(false);
        }
    }
    public void ResumeGuide()
    {
        if (!isGuidePlaying && isPause) return;
        isPause = false;
        if (!BoardGuide.activeInHierarchy)
        {
            BoardGuide.SetActive(true);
            Action<Animator, AnimatorStateInfo, int> onStateExit = null;
            onStateExit = delegate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
            {
                if (animatorStateInfo.IsName("Slide"))
                {
                    animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= onStateExit;
                    if (BoardGuide.activeInHierarchy)
                    {
                        ModeGuide();
                    }
                }
            };
            BoardGuide.GetComponent<RectTransform>().localPosition = listV3s[0].localPosition;
            Animator fingerAnimator = BoardGuide.transform.Find("UIFinger").GetComponent<Animator>();
            fingerAnimator.SetTrigger("Slide");
            fingerAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += onStateExit;
        }
    }
    public void StopGuide()
    {
        if (BoardGuide != null)
        {
            if (BoardGuide.activeInHierarchy)
            {
                BoardGuide.SetActive(false);
            }
        }
        GuideConfigStep = -1;
        listV3s = null;
        isGuidePlaying = false;
    }

    bool FirstGuide = true;
    private void StartGuide(Vector3 offset = new Vector3())
    {
        if (GuideConfigStep != -1)
        {
            if (BoardGuide != null)
            {
                Animator fingerAnimator = BoardGuide.transform.Find("UIFinger").GetComponent<Animator>();
                Action<Animator, AnimatorStateInfo, int> onStateExit = null;
                onStateExit = delegate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
                {
                    if (animatorStateInfo.IsName("Slide"))
                    {
                        animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= onStateExit;
                        if (BoardGuide.activeInHierarchy)
                        {
                            ModeGuide();
                        }
                    }
                };
                BoardGuide.GetComponent<RectTransform>().localPosition = FirstGuide ? listV3s[0].localPosition + offset: listV3s[0].localPosition;
                fingerAnimator.SetTrigger("Slide");
                fingerAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += onStateExit;

                FirstGuide = false;
            }
        }
    }

    private void ModeGuide()
    {
        float MoveSpeed = 0.3f;
        LeanTweenType MoveEaseType = LeanTweenType.linear;
        RectTransform rectTransform = BoardGuide.GetComponent<RectTransform>();
        Action<GameObject> callback = delegate(GameObject gameObject)
        {
            if (BoardGuide.activeInHierarchy)
            {
                StartGuide();
            }
        };
        MoveByPath(rectTransform, BoardGuide, listV3s, MoveSpeed, MoveEaseType, callback, 1);
    }

    static private void MoveByPath(RectTransform guideRectTransform,
            GameObject finger,
            List<RectTransform> path,
            float MoveSpeed, LeanTweenType MoveEaseType,
            Action<GameObject> callback,
            int startIndex = 0)
    {
        if (!finger.activeInHierarchy)
        {
            return;
        }
        Action<object> onComplete = delegate(object index)
        {
            MoveByPath(guideRectTransform, finger, path, MoveSpeed, MoveEaseType, callback, (int)index);
        };
        if (stepCallBack != null)
        {
            stepCallBack(path, startIndex);
        }
        if (startIndex >= path.Count)
        {
            if (callback != null)
            {
                callback(finger);
            }

            return;
        }

        RectTransform rectTransform = (RectTransform)(finger.transform);
        rectTransform.localScale = new Vector3(1, 1, 1);
        //rectTransform.SetAsLastSibling();
        rectTransform.SetParent(path[startIndex].parent);
        LeanTween.move(rectTransform, path[startIndex].localPosition, MoveSpeed)
            .setEase(MoveEaseType)
            .setOnComplete(onComplete)
            .setOnCompleteParam(startIndex + 1);
    }

    public void PlayGoalPanelAnima()
    {
        InitGoalTip();
        //if (animator != null)
        //{
        //    BaseStateMachineBehaviour baseStateMachineBehaviour = animator.GetBehaviour<BaseStateMachineBehaviour>();
        //    if (baseStateMachineBehaviour != null)
        //    {
        //        baseStateMachineBehaviour.StateExitEvent += OnStateExitEvent;
        //    }

        //    animator.SetTrigger("GoalPane");
        //}
    }

    void Awake()
    {
        //UiGoalTip.gameObject.SetActive(false);      //修改UI的時候有可能會被不小心打開

        //PlayGoalPanelAnima();
        this.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
    }
    private bool GoalPanePlayComplete;
    private int GuideConfigStep = -1;
    public GuideDataConfig GuideConfigData;


    void Start()
    {
        GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIBattle);
        createGuideConfig();

        PauseButton.onClick += PauseButton_onClick;

        //for (int i = 0, n = GoalCompleteIcons.Count; i < n; i++)
        //{
        //    //GoalCompleteIcons[i].gameObject.SetActive(false);
        //    GoalCompleteIcons[i].transform.parent.gameObject.SetActive(false);
        //}

        SliderEnemyHp1.enabled = false;
        SliderEnemyHp2.enabled = false;
        ControlEnemyHpDisplay(false);           //血條默認不顯示

        if (GuideManager.getInstance().isOpenGuide && GuideManager.getInstance().IndexOfGuideStep() == 1)
        {
            PauseButton.gameObject.SetActive(false);
        }

        GameObject go = Resources.Load<GameObject>("Prefabs/Ui/LessStepTips");
        lessStepTips = Instantiate<GameObject>(go);
        lessStepTips.SetActive(false);

        lessStepTips.transform.SetParent(this.transform);
        lessStepTips.transform.localScale = new Vector3(1, 1, 1);
        //
        BoardGuide.SetActive(false);
        BoardGuide.transform.SetAsLastSibling();
        ///初始化分數進度條///
        ShowStar(0);

        AddStepsAni.gameObject.SetActive(false);
        GameController.Instance.ViewEventSystem.AddSteps += PlayAddStepsAni;
    }

    void OnDestroy()
    {
        if (animator != null)
        {
            BaseStateMachineBehaviour baseStateMachineBehaviour = animator.GetBehaviour<BaseStateMachineBehaviour>();
            if (baseStateMachineBehaviour != null)
            {
                baseStateMachineBehaviour.StateExitEvent -= OnStateExitEvent;
            }
        }
        OnClickPauseButton = null;
        PauseButton.onClick -= PauseButton_onClick;

        if (GoalCompleteIcons != null)
        {
            for (int i = 0; i < GoalCompleteIcons.Count; i++)
            {
                Destroy(GoalCompleteIcons[i].gameObject);
            }
        }

        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIBattle);

        RemoveEvents();

        GameController.Instance.ViewEventSystem.AddSteps -= PlayAddStepsAni;
    }

    void OnBattleInfoUpdate(ModelEventSystem.BattleInfoType type)
    {
        switch (type)
        {
            case ModelEventSystem.BattleInfoType.Step:
                if (battleModel.CrtStageConfig.Mode == BattleMode.Normal)
                {
                    SetSteps(battleModel.RemainSteps);
                }

                break;
            case ModelEventSystem.BattleInfoType.Time:
                if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
                {
                    SetTime(battleModel.RemainTime);
                }

                break;
            case ModelEventSystem.BattleInfoType.EnemyHp:
                Unit enemy = battleModel.GetCrtEnemy();
                ChangeEnemyHpTo(enemy == null ? 0 : enemy.Hp);
                break;
            case ModelEventSystem.BattleInfoType.Goal:
                UpdateGoalUI(battleModel.CurrentGoal);
                break;
            default:
                break;
        }
    }

    void InitGoalTip()
    {
        UiGoalTipDisplay = GameController.Instance.Popup.Open(PopupID.UIGoalTip,null,false,false,0f).Find("UiGoalTip").GetComponent<UIGoalTip>();
        UIGoalTip.Data tipData = new UIGoalTip.Data();
        tipData.Title = Utils.GetTextByStringID(battleModel.CrtStageConfig.NameStringID);
        tipData.goals = new List<StageConfig.Goal>();

        foreach (var go in battleModel.CurrentGoal)
        {
            if (go.Type != BattleGoal.Score)
                tipData.goals.Add(go);
        }
        ScoreTipsParent = UiGoalTipDisplay.ScoreTipsParent;
        int aniNum = 0;

        Animator tipsAni = UiGoalTipDisplay.GetComponent<Animator>();

        if (tipData.goals.Count != 0)
        {
            aniNum = tipData.goals.Count;
            UiGoalTipDisplay.SetData(tipData);
            SetGoalNum(tipData.goals.Count);
            DisplayScoreGoal(false);

            for (int i = 0; i < tipData.goals.Count; i++)
            {
                DisplayGoalAndNum(tipsAni, i);
            }
        }
        else
        {
            aniNum = 5;

            UiGoalTipDisplay.ScoreTipsNum.text = string.Format("{0:N0}", ScoreGoal.Number);

            DisplayScoreGoal(true);

            DisplayGoalAndNum(tipsAni, 1,true);
        }

        if (aniNum > 0)
            tipsAni.SetInteger("GoalTip", aniNum);

        Action<Animator, AnimatorStateInfo, int> complteAc = null;

        complteAc = delegate (Animator ani, AnimatorStateInfo info, int sta) {

            if ( !info.IsName("Start"))
            {
                tipsAni.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= complteAc;

                StartCoroutine(Utils.DelayToInvokeDo(delegate ()
                {

                    if (GameController.Instance.Popup.IsPopup(PopupID.UIGoalTip))
                        GameController.Instance.Popup.Close(PopupID.UIGoalTip);

                    OnStateExitEvent(ani, info, sta);

                }, 0.5f));
            }
        };

        tipsAni.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += complteAc; 
    }

    /// <summary>
    /// 更新UI界面的勝利目標
    /// </summary>
    /// <param name="goalList"></param>
    /// <param name="needUpdateImg"></param>
    private void UpdateGoalUI(List<StageConfig.Goal> goalList, bool needUpdateImg = false)
    {
        List<StageConfig.Goal> newGoalList = new List<StageConfig.Goal>();
        foreach (var go in goalList)
        {
            if (go.Type != BattleGoal.Score)
                newGoalList.Add(go);
        }

        //SetGoalNum(newGoalList.Count);

        /////沒有消除物目標顯示分數目標
        //if (newGoalList.Count == 0)
        //    DisplayScoreGoal(true);

        for (int i = 0, n = newGoalList.Count; i < n; i++)
        {
            StageConfig.Goal g = newGoalList[i];
            if (g.Type == BattleGoal.Object)
            {
                string iconName = GameController.Instance.Model.TileObjectConfigs[g.RelativeID].ResourceIcon;
                int count = battleModel.ObjectGoal[g.RelativeID];
                if (g.Num <= count)
                {
                    PlayGoalComplete(i);
                }
                SetGoalText(i, (g.Num - count).ToString(), g.Num <= count);

                if (needUpdateImg)
                    SetGoalImageAt(i, GameController.Instance.AtlasManager.GetSprite(Atlas.Tile, iconName));
            }
            else if (g.Type == BattleGoal.Unit)
            {
                if (g.Num <= battleModel.UnitGoal)
                {
                    PlayGoalComplete(i);
                }
                SetGoalText(i, (g.Num - battleModel.UnitGoal).ToString(), g.Num <= battleModel.UnitGoal);

                if (needUpdateImg)
                    SetGoalImageAt(i, GameController.Instance.AtlasManager.GetSprite(Atlas.Tile, "ENEMY")); //怪物的图片也用Tile里面的图
            }
        }
    }

    void OnBattleEnemyDisplayChange(bool shouldEnemyShow_)
    {
        shouldEnemyShow = shouldEnemyShow_;
    }

    void OnBattleCameraMove(int state)
    {
        if (state == 0)
        {
            //開始移動 隱藏血條
            ControlEnemyHpDisplay(false);
        }
        else if (state == 2)
        {
            //移動完成
            Unit enemy = battleModel.GetCrtEnemy();
            ControlEnemyHpDisplay(shouldEnemyShow && enemy != null);            //血條邏輯 待整理

            if (enemy != null)
            {
                InitEnemyHp(enemy.HpMax, enemy.Hp);
            }
        }
    }

    void OnBattleEnemyPosChange(Vector3 pos)
    {
        SetEnemyInfoPos(pos);
    }

    void OnEliminateStep()
    {
        if (battleModel.CrtStageConfig.Mode != BattleMode.TimeLimit)
            SetSteps(battleModel.RemainSteps);
    }

    void OnTimeTick(int time)
    {
        if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
            SetTime(time);
    }

    //void modelEventSystem_OnBuyMoves(int addSteps)
    //{
    //    SetSteps(addSteps);
    //}

    //購買時間 需要測試
    void modelEventSystem_OnBuyTime(int addTime)
    {
        ViewEventSystem viewEventSystem = GameController.Instance.ViewEventSystem;

        if (battleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
        {
            Action readyEnd = null;
            readyEnd = delegate()
            {
                viewEventSystem.ReadyGo -= readyEnd;
                //重新啟動timer TODO
                battleModel.SetTimerPause(false);
            };
            viewEventSystem.ReadyGo += readyEnd;
            SetTime(battleModel.RemainTime);
            PlayReadyTime();
            PauseButton.enabled = false;
        }
    }


    void Update()
    {

        if (TimeSlider != null && TimeSlider.value > 0)
        {
            float speed = TimeSlider.value - TimeNum;

            //增加時間速度加快//
            if (speed < 0)
                speed *= 10;

            TimeSlider.value -= speed * Time.deltaTime;
        }
    }

    /// <summary>
    /// 初始化目標數量///
    /// </summary>
    /// <param name="Num"></param>
    void InitNum(int Num)
    {
        int maxcount = 4;
        for (int i = 0; i < maxcount; i++)
        {
            bool ac = i < Num;
            GoalIcons[i].gameObject.SetActive(false);
            GoalIcons[i].transform.parent.gameObject.SetActive(ac);
            GoalTexts[i].gameObject.SetActive(false);
            GoalTexts[i].transform.parent.gameObject.SetActive(ac);
            GoalBg[i].SetActive(ac);
            GoalCompleteIcons[i].transform.parent.gameObject.SetActive(ac);
            GoalIconsAnimationTex[i].transform.parent.gameObject.SetActive(ac);
            ScoreGoal.gameObject.SetActive(false);
        }

    }

    float DisplayGoalAndNum(Animator ani ,int num,bool isscore = false )
    {
        float distime = 0;
        for (int i = 0, n = ani.parameters.GetLength(0); i < n; i++)
        {
            AnimatorControllerParameter p = ani.parameters[i];

            if (p.name.Equals("DelayTime" + num))
            {
                distime = p.defaultFloat;
                break;
            }            
        }

        StartCoroutine(Utils.DelayToInvokeDo(delegate() {

            if (isscore)
            {
                ScoreGoalParent.gameObject.SetActive(true);
                ScoreGoal.gameObject.SetActive(true);
            }
            else
            {
                GoalIcons[num].gameObject.SetActive(true);
                GoalTexts[num].gameObject.SetActive(true);
            }

        }, distime));

        return distime;

    }

    public Transform GetGoalTransform(int index)
    {
        return GoalIcons[index].transform;
    }

    void PauseButton_onClick(UIButtonBehaviour button)
    {
        if (OnClickPauseButton != null)
            OnClickPauseButton(this);
    }

    /// <summary>
    /// 關卡連線引導配置
    /// </summary>
    void createGuideConfig()
    {
        GuideConfigData = new GuideDataConfig();
        List<int[]> guidePointsLevel1 = new List<int[]>();
        guidePointsLevel1.Add(new int[3] { 16, 17, 18 });
        guidePointsLevel1.Add(new int[4] { 22, 23, 17, 18 });

        GuideConfigData.AddGuideConfig(1, guidePointsLevel1);

        List<int[]> guidePointsLevel2 = new List<int[]>();
        guidePointsLevel2.Add(new int[15] { 8, 15, 22, 29, 36, 43, 44, 45, 46, 47, 40, 33, 26, 19, 12});

        if (units != null && units.Contains(3201) || units.Contains(3202) ||
             units.Contains(3203) || units.Contains(3204) || units.Contains(3205))
            guidePointsLevel2.Add(new int[3] { 17, 10, 3 });
        else
            guidePointsLevel2.Add(new int[3] { 3, 10, 17 });

        GuideConfigData.AddGuideConfig(2, guidePointsLevel2);

        List<int[]> guidePointsLevel3 = new List<int[]>();
        guidePointsLevel3.Add(new int[9] { 31, 24, 32, 25, 18, 17, 16, 23, 30 });
        GuideConfigData.AddGuideConfig(3, guidePointsLevel3);

        List<int[]> guidePointsLevel9 = new List<int[]>();
        guidePointsLevel9.Add(new int[4] { 10, 17, 24, 31 });
        GuideConfigData.AddGuideConfig(9, guidePointsLevel9);
    }

    void ChangeGuideConfig(int unitId)
    {
        List<int[]> guidePointsLevel2 = new List<int[]>();
        guidePointsLevel2.Add(new int[17] { 35, 28, 21, 22, 23, 30, 37, 44, 45, 46, 39, 32, 25, 26, 27, 34, 41 });

        if (unitId == 3201)
        {
            guidePointsLevel2.Add(new int[3] { 17, 10, 4 });
        }
        else
        {
            guidePointsLevel2.Add(new int[3] { 4, 10, 17 });
        }

        GuideConfigData.AddGuideConfig(2, guidePointsLevel2);
    }

    void OnGoalPaneComplete()
    {

    }

    public void PlayReadyTime()
    {
        TipTimeTick.Reset();
        ReadyTime.gameObject.SetActive(true);
        Animator readyAnim = ReadyTime.GetComponent<Animator>();
        //readyAnim.SetTrigger("TimeTickTrigger");
        readyAnim.SetTrigger("ReadyTrigger");
        BaseStateMachineBehaviour machineBeh = readyAnim.GetBehaviour<BaseStateMachineBehaviour>();
        machineBeh.StateExitEvent += OnReadyTimeStateExitEvent;
    }

    void OnStateExitEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //if (stateInfo.IsName("GoalPane"))
        {
            if (GameController.Instance.Model.BattleModel.CrtStageConfig.Mode == BattleMode.Normal)
            {
                // 普通模式直接初始化完成
                BattleInitAnimComplete(null);
            }
            else if (GameController.Instance.Model.BattleModel.CrtStageConfig.Mode == BattleMode.TimeLimit)
            {
                //PlayReadyTime();
                BattleInitAnimComplete(PlayReadyTime);
            }
        }
    }

    void OnReadyTimeStateExitEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("Start"))
        {
            BaseStateMachineBehaviour machineBeh = animator.GetBehaviour<BaseStateMachineBehaviour>();
            Q.Assert(machineBeh != null, "UIBattleBeh:OnReadyTimeStateExitEvent assert");
            if (machineBeh != null)
            {
                machineBeh.StateExitEvent -= OnReadyTimeStateExitEvent;
            }

            // 準備動畫播放完畢
            ReadyTime.gameObject.SetActive(false);
            // 開始計時
            GameController.Instance.Model.BattleModel.StartTimer();

            // 事件通知
            if (GameController.Instance.ViewEventSystem.ReadyGo != null)
                GameController.Instance.ViewEventSystem.ReadyGo();
        }
        else if (stateInfo.IsName("TipTimeTick"))
        {
            if (!TipTimeTick.Next())
            {
                animator.SetTrigger("ReadyTrigger");
                animator.SetBool("TimeTickEnd", true);
            }
        }
    }

    /// <summary>
    /// 戰鬥初始化完成
    /// </summary>
    private void BattleInitAnimComplete(Action callbackBeforeInit)
    {
        GoalPanePlayComplete = true;
        if (GameController.Instance.ViewEventSystem.BattleInitAnimCompleteEvent != null)
            GameController.Instance.ViewEventSystem.BattleInitAnimCompleteEvent(callbackBeforeInit);
        if (listV3s != null && GuideConfigStep != -1)
        {
            int step = GuideConfigStep;
            GuideConfigStep = -1;
            PlayGuide(step, listV3s, stepCallBack);
            BaseStateMachineBehaviour baseStateMachineBehaviour = animator.GetBehaviour<BaseStateMachineBehaviour>();
            baseStateMachineBehaviour.StateExitEvent -= OnStateExitEvent;
        }
    }


    void SetEnemyInfoPos(Vector3 pos)
    {
        if (EnemyInfo != null)
        {
            EnemyInfo.localPosition = pos;
        }
        else
        {
            Debug.Log("EnemyInfo is null");
        }
    }

    void StartFlashEffect()
    {
        if (LeanTween.isTweening(FlashImg.gameObject))
            return;

        FlashImg.gameObject.SetActive(true);

        LeanTween.value(FlashImg.gameObject, 0f, 1f, .4f).setLoopType(LeanTweenType.pingPong).setOnUpdate(delegate(float val)
        {
            FlashImg.color = new Color(FlashImg.color.r, FlashImg.color.g, FlashImg.color.b, val);
        });
    }

    public void StopFlashEffect()
    {
        LeanTween.cancel(FlashImg.gameObject);
    }

    void PlayAddStepsAni()
    {
        AddStepsAni.gameObject.SetActive(true);
        AddStepsAni.SetTrigger("Play");

        Action<Animator, AnimatorStateInfo, int> ex = null;

        ex = delegate (Animator ani, AnimatorStateInfo info, int sta) {

            if (!info.IsName("Start"))
            {
                AddStepsAni.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= ex;

                StartCoroutine(Utils.DelayNextFrameCall(delegate() {

                    AddStepsAni.gameObject.SetActive(false);
                }));
            }
        };

       // AddStepsAni.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += ex;
    }
}

