
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 專門用於播放消除特效的類
/// TODO 各個消除特效做一個對像池
/// </summary>
public class EliminateEffectBehaviour : MonoBehaviour
{
    /// <summary>
    /// 用於播放消除特效的層次
    /// </summary>
    public RectTransform Layer;

    /// <summary>
    /// 通用消除特效
    /// </summary>
    public GameObject NormalEffectPrefab;
    public GameObject BombEffectPrefab;

    //玻璃消除特效//
    public GameObject GlassEffectPrefab;

    /// <summary>
    /// 炸弹消除音效
    /// </summary>
    public AudioClip[] BoomAudio;

    /// <summary>
    /// 普通消除物消除音效
    /// </summary>
    public AudioClip NormalElimAudio;

    float AudioTime = 0;

    /// <summary>
    /// 所有動態在對像池創建的對象的key先緩存，在銷毀的時候再銷毀所有包含的對像池。
    /// 臨時用，後期需要改為在進戰鬥時，預加載所有的特效並且保存
    /// </summary>
    List<string> despoolKeysTmp = new List<string>();

    /// <summary>
    /// 延遲時間的播放次數//
    /// </summary>
    private Dictionary<float, int> AudioTimeDic = new Dictionary<float, int>();

    /// <summary>
    /// 同一個延時限定聲音播放次數///
    /// </summary>
    private int AudioPlaySomeTimeNumMax = 2;

    string BOMB_EFFECT_KEY;
    string ELEMENT_EFFECT_KEY;



#if EFFECT_HIDE
    public void Awake()
    {
        BombEffectPrefab = Resources.Load<GameObject>(GOPoolManager.BLOCK_EFFECT_PREFAB);
        NormalEffectPrefab = Resources.Load<GameObject>(GOPoolManager.BLOCK_EFFECT_PREFAB);
    }
#endif

    void Start()
    {
        BOMB_EFFECT_KEY = string.Concat("BOMB_EFFECT_KEY_", BombEffectPrefab.name);
        ELEMENT_EFFECT_KEY = string.Concat("ELEMENT_EFFECT_KEY_", NormalEffectPrefab.name);

        //緩存炸彈 緩存10個就好 發現不夠的話會做增量創建
        GameController.Instance.PoolManager.PrePrefabSpawn(BombEffectPrefab.transform, BOMB_EFFECT_KEY, 10);
        GameController.Instance.PoolManager.PrePrefabSpawn(NormalEffectPrefab.transform, ELEMENT_EFFECT_KEY, 10);

        GameController.Instance.PoolManager.PrePrefabSpawn(GlassEffectPrefab.transform, GlassEffectPrefab.name, 10);
    }

    void OnDestroy()
    {
        GameController.Instance.PoolManager.Despool(BOMB_EFFECT_KEY);
        GameController.Instance.PoolManager.Despool(ELEMENT_EFFECT_KEY);

        GameController.Instance.PoolManager.Despool(GlassEffectPrefab.name);

        for (int i = 0; i < despoolKeysTmp.Count; i++)
        {
            GameController.Instance.PoolManager.Despool(despoolKeysTmp[i]);
        }

        despoolKeysTmp = null;
    }

    /// <summary>
    /// 播放消除特效
    /// </summary>
    /// <param name="conf"></param>
    /// <param name="refRect"></param>
    /// <param name="delay"></param>
    /// <param name="isBombEffect">是否被炸彈範圍影響</param>
    public void PlayAt(TileObjectConfig conf, RectTransform refRect, float delay = 0, bool isBombEffect = false)
    {
        RectTransform target = null;

        if (conf.ElementType == ElementType.Bomb || isBombEffect)
        {
            target = (RectTransform)GameController.Instance.PoolManager.PrefabSpawn(BOMB_EFFECT_KEY);
        }
        else if (conf.ElementType == ElementType.Normal)
        {
            //消除特效
            target = (RectTransform)GameController.Instance.PoolManager.PrefabSpawn(ELEMENT_EFFECT_KEY);
        }
        else
        {
            if (conf.EliminateAnim == "")
                return;

            string key = string.Concat("Prefabs/Effects/", conf.EliminateAnim);
            //只管創建 期望在進入到場景的時候就預先加載可能會需要的特效（動態），在銷毀的時候再將預加載的特效全部銷毀
            target = (RectTransform)GameController.Instance.PoolManager.PrefabSpawn(key);

            if (despoolKeysTmp.IndexOf(key) == -1)
            {
                despoolKeysTmp.Add(key);
            }
        }

        SetPostion(target, refRect, Layer);
        AudioSource audioS = target.GetComponent<AudioSource>();
        if (audioS != null)
        {
            audioS.enabled = false;
            if (!isBombEffect)
            {
                ///只對消除元素做音效控制////
                if (conf.ObjectType == TileType.Element)
                {
                    if (AudioTimeDic.ContainsKey(delay))
                    {
                        AudioTimeDic[delay]++;
                    }
                    else
                    {
                        AudioTimeDic.Add(delay, 1);
                    }
                    //Debug.Log(string.Format("delay is {0} ,num is {1}", delay, AudioTimeDic[delay]));

                    if (AudioTimeDic[delay] <= AudioPlaySomeTimeNumMax)
                    {
                        audioS.clip = NormalElimAudio;
                        audioS.enabled = true;
                    }
                }
                else
                {
                    //其它地形物：冰塊、木樁，直接播放
                    audioS.enabled = true;
                }
            }
            else//播放炸彈特效、音效
            {
                if (AudioTimeDic.ContainsKey(delay))
                {
                    AudioTimeDic[delay]++;
                }
                else
                {
                    AudioTimeDic.Add(delay, 1);
                }

                if (AudioTimeDic[delay] <= AudioPlaySomeTimeNumMax)
                {
                    audioS.clip = BoomAudio[UnityEngine.Random.Range(0, 4)];
                    audioS.enabled = true;
                }
            }
        }

        ///現在只作為開關用//
        ///用於情況數據///
        AudioTime = 1.8f;

        if (delay == 0)
        {
            PlayAnim(target, conf);
        }
        else
        {
            target.gameObject.SetActive(false);
            StartCoroutine(Utils.DelayToInvokeDo(
                delegate ()
                {
                    PlayAnim(target, conf);
                }, delay
            ));
        }
    }


    void PlayGemScale(RectTransform rect)
    {
        rect.gameObject.SetActive(true);
        Vector3 targetPos = new Vector3(0.0f, rect.localPosition.y + 200, 0.0f);
        float initY = rect.localPosition.y;
        LeanTween.moveLocal(rect.gameObject, targetPos, 1.0f).setEase(LeanTweenType.linear).setOnUpdate(delegate (Vector3 val)
        {
            float scale = 1 + (val.y - initY) / 300;
            rect.localScale = new Vector3(scale, scale, scale);
        }).setOnComplete(delegate ()
        {
            rect.gameObject.SetActive(false);
        });
    }

    private void PlayAnim(RectTransform rect, TileObjectConfig conf)
    {
        rect.gameObject.SetActive(true);

        //優先檢測有沒有   DurationTimeDispatcher
        DurationTimeDispatcher dtDispatcher = rect.GetComponent<DurationTimeDispatcher>();

        if (dtDispatcher != null)
        {

            Action OnDTTimeOut = null;

            OnDTTimeOut = delegate ()
            {
                dtDispatcher.OnTimeOut -= OnDTTimeOut;
                GameController.Instance.PoolManager.Despawn(rect);
            };

            dtDispatcher.OnTimeOut += OnDTTimeOut;
        }
        else
        {
            Animator anim = rect.GetComponent<Animator>();

            if (anim == null)
            {
                return;
            }

            int index = 0;
            Action<Animator, AnimatorStateInfo, int> OnStateMachineExit = null;
            string aniName = "";

            switch (conf.ObjectType)
            {
                case TileType.SeperatorH:
                    anim.SetInteger("Direction", Convert.ToInt32(conf.RangeMode));
                    anim.SetTrigger("TriggerBroken");
                    index = UnityEngine.Random.Range(1, 4);
                    anim.SetInteger("Index", index);
                    aniName = string.Format("H_Broken{0}", index);
                    break;
                case TileType.SeperatorV:
                    anim.SetInteger("Direction", Convert.ToInt32(conf.RangeMode));
                    anim.SetTrigger("TriggerBroken");
                    index = UnityEngine.Random.Range(1, 4);
                    anim.SetInteger("Index", index);
                    aniName = string.Format("V_Broken{0}", index);
                    break;
                case TileType.Obstacle:
                case TileType.Cover:
                case TileType.Bottom:
                    anim.SetTrigger("TriggerBroken");
                    index = UnityEngine.Random.Range(1, 4);
                    anim.SetInteger("Index", index);
                    aniName = string.Format("Broken{0}", index);
                    break;
            }

            OnStateMachineExit = delegate (Animator elementAnimator, AnimatorStateInfo info, int stateMachinePathHash)
            {
                if (info.IsName(aniName))
                {
                    elementAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= OnStateMachineExit;
                    GameController.Instance.PoolManager.Despawn(elementAnimator.transform);
                    //elementAnimator.gameObject.SetActive(false);
                }
            };

            anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += OnStateMachineExit;
        }
    }

    public void ClearAll()
    {
        //後面看看是否需要刪掉
    }

    private void SetPostion(RectTransform target, RectTransform refRect, RectTransform layer)
    {
        target.SetParent(layer);
        target.localScale = new Vector3(1, 1, 1);
        //設置為跟參考的Tile位置相同
        target.anchorMax = refRect.anchorMax;
        target.anchorMin = refRect.anchorMin;
        target.anchoredPosition3D = refRect.anchoredPosition3D;
        //pool.Add(target);
    }

    void Update()
    {
        if (AudioTime < 0)
            return;
        AudioTime = -1f;
        AudioTimeDic.Clear();
    }
}
