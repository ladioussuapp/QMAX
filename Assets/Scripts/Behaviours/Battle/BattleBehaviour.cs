using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//通過怪物點找到對應的路線
public class BattleBehaviour : MonoBehaviour
{
    /// <summary>
    /// 每次走完一段路線（調用Next()），就會調用一次。
    /// 即使已經敵人已經打完無法移動了，再次調用Next()，也還是會調用這個委託。
    /// 
    /// </summary>
    public event Action<int> OnMoveComplete;

    /// <summary>
    /// 應該是所有路線已經走完，目前邏輯是所有怪被打完。
    /// </summary>
    public event Action OnBattleComplete;

    /// <summary>
    /// itween裡面漫遊的路線， 路線的數量要大於等於怪物出生點的數量
    /// </summary>
    public BattlePathData[] EyePathes;

    /// <summary>
    ///  所有的特效，會按照離攝像機距離打開
    /// </summary>
    public Transform EffectsRoot;
    protected Transform[] effects;
    public float EffectDis = 20f;

    public Camera Camera;

    /// <summary>
    /// 怪物點
    /// </summary>
    public EnemyPoint[] EnemyPoints;

    public float DefaultSpeed = 60f;

    /// <summary>
    /// 當前敵人在整個進度中的索引。
    /// </summary>
    public int EnemyIndex { get { return enemyIndex; } }


    /// <summary>
    /// 敵人剩餘數量，不計算當前位置的敵人
    /// </summary>
    public int EnemyLeft { get { return Math.Max(0, enemyCount - enemyIndex - 1); } }


    /// <summary>
    /// 戰場id
    /// </summary>
    public string BattleId;

    [HideInInspector]
    public StageConfig StageConfig;

    [HideInInspector]
    public float TimeScale = 1f;

    [HideInInspector]
    public bool UseLinear = false;

    /// <summary>
    /// 怪物索引  與 enemyCount保持一致
    /// 如果没有怪物，則為-1
    /// </summary>
    protected int enemyIndex = -1;

    /// <summary>
    /// 當前怪物出生點的索引。
    /// </summary>
    protected int enemyPointIndex = -1;

    /// <summary>
    /// 攝像機路徑的索引，攝像機路徑的數量大於等於怪物數量
    /// </summary>
    protected int pathIndex = -1;

    protected bool isMoving = false;

    /// <summary>
    /// 敵人總數量
    /// </summary>
    private int enemyCount = 0;

    private AudioSource audioSource;

    private List<string> enemyModelSources = new List<string>();

    private GameObject enmeySpawnEffect;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (EffectsRoot != null && EffectsRoot.childCount > 0)
        {
            effects = new Transform[EffectsRoot.childCount];

            for (int i = 0; i < EffectsRoot.childCount; i++)
            {
                effects[i] = EffectsRoot.GetChild(i);
            }
        }
    }

    void Start()
    {
        enmeySpawnEffect = Resources.Load<GameObject>("Prefabs/Effects/EffectEnemySpawn");
    }

    /// <summary>
    /// 獲取當前的敵人
    /// </summary>
    /// <returns></returns>
    public EnemyPoint GetCurrentEnemyPoint()
    {
        if (enemyPointIndex < 0)
            return null;
        return EnemyPoints[enemyPointIndex];
    }



    /// <summary>
    /// 第一隻怪 初始化
    /// </summary>
    public void First(StageConfig config)
    {
        StageConfig = config;
        enemyCount = StageConfig.MonsterUnitID.Length;
        ClearEnemies();

        Q.Assert(config.SamplingId <= EyePathes.Length - 1, "起始路點配置索引超限");

        //StageConfig.SamplingId是怪物的起始點。對應於EnemyPoints與enemyPointIndex。 StageConfig.SamplingId從1開始
        //開始打第一隻怪   
        //攝像機到達第一個位置
        //從起始位置的前一個位置開始移動   StageConfig.SamplingId從1開始
        //TODO 直接調用了next。  會增加 enemyPointIndex與enemyIndex的索引，所以暫時都度減去了1 pathIndex則是在播放完成之後+1，所以
        pathIndex = StageConfig.SamplingId - 1;
        enemyPointIndex = StageConfig.SamplingId - 1 - 1;
        enemyIndex = -1;
        UpdateEnemys(enemyPointIndex + 1, enemyIndex + 1, 1);
        CamaraToPathStart();

        //2.23 不在自動調用next方法，由外部手動調用
        //Next(false);

        StopAllCoroutines();
        StartCoroutine(CheckEffectActive());
    }

    public void OnEnemyPoitsShow(bool isShow)
    {
        if (EnemyPoints[0] != null)
        {

            EnemyPoints[0].gameObject.SetActive(isShow);
            if (EnemyPoints[0].gameObject.transform.childCount > 0)
            {
                Transform ch = EnemyPoints[0].gameObject.transform.GetChild(0);
                if (isShow)
                {
                    //添加煙霧效果                
                    GameObject effect = Instantiate<GameObject>(enmeySpawnEffect);
                    effect.transform.SetParent(ch);
                    effect.transform.position = ch.position;
                    effect.gameObject.SetActive(true);
                    //播放完後，再把它幹掉
                    //                 LeanTween.delayedCall(3, delegate()
                    //                 {
                    //                     Destroy(effect);
                    //                 });
                }
            }

        }
    }

    /// <summary>
    /// 漫遊開始
    /// </summary>
    /// <param name="SamplingId_"></param>
    public void RoamFirst(int SamplingId_)
    {
        pathIndex = SamplingId_ - 1;
        CamaraToPathStart();
        RoamNext();
    }

    /// <summary>
    /// 漫遊，無視是否有怪
    /// </summary>
    /// <param name="timeScale"></param>
    public void RoamNext()
    {
        if (isMoving)
            return;

        BattlePathData pData = EyePathes[pathIndex];

        float speed = pData.speed == 0 ? DefaultSpeed : pData.speed;
        speed *= TimeScale;

        RoamPlay(pData);
    }

    /// <summary>
    /// 進行到下一個怪
    /// </summary>
    public void Next(bool audioPlay = true)
    {
        if (isMoving)
            return;

        //Q.Assert(!isMoving, "正在移動中");
        BattlePathData pData = EyePathes[pathIndex];

        if (enemyCount == 0)
        {
            //特殊邏輯，如果沒有怪，那麼需要移動到第一個點
            //怪點可以循環
            enemyPointIndex = (enemyPointIndex + 1) % EnemyPoints.Length;
            Play(pData);
        }
        else if (EnemyLeft == 0)
        {
            Q.Log("所有敵人都打完");

            if (OnMoveComplete != null)
                OnMoveComplete(enemyIndex);

            //全部打完
            if (OnBattleComplete != null)
            {
                OnBattleComplete();
            }
            return;
        }

        //下一隻怪物索引
        enemyIndex++;
        //怪點可以循環
        enemyPointIndex = (enemyPointIndex + 1) % EnemyPoints.Length;
        EnemyPoint ep = GetCurrentEnemyPoint();
        if (ep != null && ep.EnemyAnimator != null)
            ep.EnemyAnimator.enabled = true;

        //播放音效
        if (!audioPlay)
            audioSource.Play();
        Play(pData);
    }

    /// <summary>
    /// 根據enemyIndex 更新出身點的敵人prefab
    /// </summary>
    /// <param name="pointStartIndex">從哪個EnemyPoint開始放置敵人</param>
    /// <param name="enemyStartIndex">從哪個敵人開始放置</param>
    /// <param name="count">放置數量</param>
    protected void UpdateEnemys(int pointStartIndex, int enemyStartIndex, int count)
    {
        if (enemyCount == 0)
            return;

        //Q.Assert(enemyStartIndex + count <= enemyCount);
        List<Unit> enemiesData = GameController.Instance.Model.BattleModel.EnemiesData;
        for (int i = 0; i < count; i++)
        {
            //EnmeyPoint超出範圍，就從0開始佈置
            int pI = (pointStartIndex + i) % EnemyPoints.Length;
            EnemyPoint enemyPoint = EnemyPoints[pI];
            enemyPoint.Index = enemyStartIndex + i;
            enemyPoint.Config = enemiesData[enemyStartIndex + i].Config;

            if (enemyModelSources.IndexOf(enemyPoint.Config.PrefabPath) == -1)
            {
                enemyModelSources.Add(enemyPoint.Config.PrefabPath);
            }
            //Q.Log("刷新怪物：" + enemy.Data.Config.ID);
        }
    }


    /// <summary>
    /// 將場景中敵人清理
    /// </summary>
    protected void ClearEnemies()
    {
        for (int i = 0; i < EnemyPoints.Length; i++)
        {
            EnemyPoints[i].Clear();
        }
    }


    protected void CamaraToPathStart()
    {
        BattlePathData pData = EyePathes[pathIndex];
        Transform startPoint = pData.PathPoints[0];
        Camera.transform.position = startPoint.position;
        Camera.transform.forward = startPoint.forward;
    }


    protected void NextPath()
    {
        //下一條攝像機路徑
        pathIndex++;
        if (pathIndex == EyePathes.Length)
        {
            pathIndex = 0;
        }
    }

    IEnumerator CheckEffectActive()
    {
        while (true)
        {
            if (effects == null)
            {
                break;
            }

            yield return new WaitForSeconds(.1f);

            if (!isMoving)
            {
                continue;
            }

            for (int i = 0; i < effects.Length; i++)
            {
                Transform t = effects[i];
                float disZ = Camera.transform.position.z - t.position.z;        //場景的Z是反的

                //後方的特效，延遲關掉
                if (disZ < EffectDis && disZ > -EffectDis * .5f)
                {
                    t.gameObject.SetActive(true);
                }
                else
                {
                    t.gameObject.SetActive(false);
                }

                //Debug.Log(t.name + " 距離：" + disZ);
            }
        }
    }

    void RoamPlay(BattlePathData pathData)
    {
        float speed = pathData.speed == 0 ? DefaultSpeed : pathData.speed;
        speed *= TimeScale;

        //暫時只在此處判斷UseLinear
        iTween.EaseType easeType = UseLinear ? iTween.EaseType.linear : pathData.EaseType;

        Hashtable ht = iTween.Hash("movetopath", false, "path", pathData.PathPoints, "speed"
    , speed, "orienttopath", false, "looktime", 0.05f, "easetype", easeType
    , "oncomplete", "OnRoamComplete", "oncompletetarget", gameObject, "oncompleteparams", pathData);

        iTween.MoveTo(Camera.gameObject, ht);
        isMoving = true;
    }

    void Play(BattlePathData pathData)
    {
        float speed = pathData.speed == 0 ? DefaultSpeed : pathData.speed;
        speed *= TimeScale;

        Hashtable ht = iTween.Hash("movetopath", false, "path", pathData.PathPoints, "speed"
            , speed, "orienttopath", false, "looktime", 0.1f, "easetype", pathData.EaseType
            , "oncomplete", "OnComplete", "oncompletetarget", gameObject, "oncompleteparams", pathData);

        iTween.MoveTo(Camera.gameObject, ht);
        isMoving = true;
    }

    void OnRoamComplete(BattlePathData pathData)
    {
        isMoving = false;
        NextPath();
        CamaraToPathStart();

        if (!pathData.isConvey)
        {
            if (OnMoveComplete != null)
                OnMoveComplete(enemyIndex);
        }
        else
        {
            BattlePathData pData = EyePathes[pathIndex];
            RoamPlay(pData);
        }
    }


    void OnComplete(BattlePathData pathData)
    {
        isMoving = false;
        EnemyPoint ep = GetCurrentEnemyPoint();
        int stageID = GameController.Instance.Model.BattleModel.CrtStageConfig.ID;
        int enemyId = 0;
        if (ep != null && ep.HasEnemy)
        {
            enemyId = ep.Config.ID;
        }
        if (stageID == 20 && enemyId == 6203)
        {
            // 遇到6203時，播放台詞音效
            if (!PlayerPrefsTools.HasKey(OnOff.FirstTimeMeet6203, true))
            {
                PlayerPrefsTools.SetIntValue(OnOff.FirstTimeMeet6203, 1, true);
                GameController.Instance.AudioManager.PlayAudio("Vo_accompany_8");
            }
            //if (Persistence.Instance.GetValue("FirstTimeMeet6203") == null)
            //{
            //    Persistence.Instance.SetValue("FirstTimeMeet6203", "true");
            //    GameController.Instance.AudioManager.PlayAudio("Vo_accompany_8");
            //}
        }
        else if (stageID == 26 && enemyId == 6261)
        {
            // 遇到6261時，播放台詞音效
            // if (Persistence.Instance.GetValue("FirstTimeMeet6261") == null)
            if (!PlayerPrefsTools.HasKey(OnOff.FirstTimeMeet6261, true))
            {
                //Persistence.Instance.SetValue("FirstTimeMeet6261", "true");
                PlayerPrefsTools.SetIntValue(OnOff.FirstTimeMeet6261, 1, true);
                // 延遲1秒
                StartCoroutine(Utils.DelayToInvokeDo(
                    delegate()
                    {
                        GameController.Instance.AudioManager.PlayAudio("Vo_accompany_9");
                    }, 1.5f
                ));
            }
        }
        NextPath();
        CamaraToPathStart();

        //是否是中斷的一條路線
        if (!pathData.isConvey)
        {
            if (enemyIndex + 1 < enemyCount)
                UpdateEnemys(enemyPointIndex + 1, enemyIndex + 1, 1);
            if (OnMoveComplete != null)
                OnMoveComplete(enemyIndex);
        }
        else
        {
            BattlePathData pData = EyePathes[pathIndex];
            Play(pData);
        }
    }

    public void OnDestroy()
    {
        //Q.Log(this.GetType().Name + "OnDestroy 1");

        string modelSource;

        for (int i = 0; i < enemyModelSources.Count; i++)
        {
            modelSource = enemyModelSources[i];
            GameController.Instance.PoolManager.Despool(modelSource);
        }

        //Q.Log(this.GetType().Name + "OnDestroy 2");
    }

    private void OnDrawGizmos()
    {
        foreach (BattlePathData path in EyePathes)
        {
            if (path.PathPoints.Length > 0)
            {
                iTween.DrawPathGizmos(path.PathPoints, Color.blue);
            }
        }
    }
}
