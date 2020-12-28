using Com4Love.Qmax;
using Com4Love.Qmax.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HitEffectBehaviour : MonoBehaviour
{    protected class ShockwaveEntry
    {
        public GameObject elem;
        public int r;
        public int c;
    }

    /// <summary>
    /// 大爆炸特效
    /// </summary>
    public GameObject BoomEffPrefab;

    /// <summary>
    /// 防禦受擊特效
    /// </summary>
    public GameObject DefenseEffPrefab;

    /// <summary>
    /// 受擊特效
    /// </summary>
    public List<GameObject> HitEffPrefabDict;

    /// <summary>
    /// 煙花爆炸Prefab
    /// </summary>
    public List<GameObject> FireworkEffDict;

    /// <summary>
    /// 煙花拖尾特效Prefab
    /// </summary>
    public List<GameObject> FireworkTailEffDict;

    /// <summary>
    /// 受擊特效層
    /// </summary>
    public RectTransform HitEffLayer;


    /// <summary>
    /// Combo2.0時的爆炸特效
    /// </summary>
    private RectTransform boomEff;

    /// <summary>
    /// 敵人的格擋特效
    /// </summary>
    private RectTransform defenseEff;


    /// <summary>
    /// 各種顏色的受擊特效
    /// </summary>
    private Dictionary<ColorType, RectTransform> hitEffDict;

    /// <summary>
    /// 煙花爆炸特效
    /// </summary>
    private Dictionary<ColorType, RectTransform> fireworkBoomEffDict;

    /// <summary>
    /// 煙花拖尾特效
    /// </summary>
    private Dictionary<ColorType, RectTransform> fireworkTailEffDict;


    /// <summary>
    /// 最後一次播放受擊特效的時間
    /// 用來控製播放間隔
    /// </summary>
    private float lastPlayHitEffTime = 0f;

    /// <summary>
    /// 最後一次拖尾特效播放時間
    /// 用來控製播放間隔
    /// </summary>
    private float lastTailEffPlayTime = 0f;

#if EFFECT_HIDE
    public void Awake()
    {
        BoomEffPrefab = Resources.Load<GameObject>(GOPoolManager.BLOCK_EFFECT_PREFAB);
        DefenseEffPrefab = Resources.Load<GameObject>(GOPoolManager.BLOCK_EFFECT_PREFAB);
        boomEff =  Resources.Load<RectTransform>(GOPoolManager.BLOCK_EFFECT_PREFAB);
        defenseEff = Resources.Load<RectTransform>(GOPoolManager.BLOCK_EFFECT_PREFAB);

        for (int i = 0; i < 10; i++)
        {
            if (HitEffPrefabDict.Count > i)
            {
                HitEffPrefabDict[i] = Resources.Load<GameObject>(GOPoolManager.BLOCK_EFFECT_PREFAB);
            }

            if (FireworkEffDict.Count > i)
            {
                FireworkEffDict[i] = Resources.Load<GameObject>(GOPoolManager.BLOCK_EFFECT_PREFAB);
            }

            if (FireworkTailEffDict.Count > i)
            {
                FireworkTailEffDict[i] = Resources.Load<GameObject>(GOPoolManager.BLOCK_EFFECT_PREFAB);
            }
        }
    }
#endif

    IEnumerator Start()
    {

        //在Start的時候，逐步創建各種所需的特效的實例
        //一次創建，不再銷毀

        if (hitEffDict == null)
            hitEffDict = new Dictionary<ColorType, RectTransform>();

        if (fireworkBoomEffDict == null)
            fireworkBoomEffDict = new Dictionary<ColorType, RectTransform>();

        if (fireworkTailEffDict == null)
            fireworkTailEffDict = new Dictionary<ColorType, RectTransform>();

        if (boomEff == null)
        {
            boomEff = Instantiate<GameObject>(BoomEffPrefab).transform as RectTransform;
            boomEff.SetParent(HitEffLayer);
            boomEff.gameObject.SetActive(false);
            boomEff.localScale = new Vector3(1, 1, 1);
        }

        yield return 0;

        RectTransform eff = null;
        RectTransform fireEff = null;
        RectTransform tailEff = null;
        for (int i = 0; i < 5; i++)
        {
            ColorType colorType = (ColorType)(i + 1);
            GameObject prefab = HitEffPrefabDict[i];
            eff = Instantiate<GameObject>(prefab).transform as RectTransform;
            eff.gameObject.SetActive(false);
            eff.SetParent(HitEffLayer);
            eff.localScale = new Vector3(1, 1, 1);
            eff.anchoredPosition3D = (prefab.transform as RectTransform).anchoredPosition3D;

            hitEffDict.Add(colorType, eff);

            yield return 0;

            if (!fireworkBoomEffDict.ContainsKey(colorType))
            {
                fireEff = Instantiate<GameObject>(FireworkEffDict[i]).transform as RectTransform;
                fireEff.SetParent(HitEffLayer);
                fireEff.gameObject.SetActive(false);
                fireEff.localScale = new Vector3(1, 1, 1);
                fireworkBoomEffDict.Add(colorType, fireEff);
                yield return 0;
            }

            tailEff = Instantiate<GameObject>(FireworkTailEffDict[i]).transform as RectTransform;
            tailEff.SetParent(HitEffLayer);
            tailEff.gameObject.SetActive(false);
            tailEff.localScale = new Vector3(1, 1, 1);
            fireworkTailEffDict.Add(colorType, tailEff);
            yield return 0;
        }

        //一次創建不再銷毀
        if (defenseEff == null)
        {
            defenseEff = Instantiate<GameObject>(DefenseEffPrefab).transform as RectTransform;
            defenseEff.SetParent(HitEffLayer);
            defenseEff.gameObject.SetActive(false);
            defenseEff.localScale = new Vector3(1, 1, 1);
            defenseEff.anchoredPosition3D = (DefenseEffPrefab.transform as RectTransform).anchoredPosition3D;
            yield return 0;
        }
    }


    /// <summary>
    /// 播放受擊特效
    /// </summary>
    /// <param name="color"></param>
    public void PlayHitEff(ColorType color) { StartCoroutine(PlayHitEffCoroutine(color)); }

    /// <summary>
    /// 播放Combo2.0大爆炸特效
    /// </summary>
    public void PlayBoomEffect(GameObject[,] elements) { StartCoroutine(PlayBoomEffectCoroutine(elements)); }

    /// <summary>
    /// 播放防禦特效
    /// </summary>
    public void PlayDefenseEffect() { StartCoroutine(PlayDefenseEffCoroutine()); }



    /// <summary>
    /// 播放煙花特效
    /// </summary>
    /// <param name="color"></param>
    public void PlayFirework(ColorType color)
    {
        //Q.Log("PlayFirework");
        //由於菸花拋出的軌跡並不是固定的，因此要根據軌蹟的最後位置來確定煙花爆炸的位置
        PlayFireworkTailEff(color, delegate(Vector3 boomPos)
        {
            StartCoroutine(PlayFireworkBoomEff(color, boomPos));
        });
    }

    /// <summary>
    /// 播放煙花拋出的軌跡特效，類似扔消除物一樣每個顏色有三條軌跡
    /// 如果正在播放過程中被調用，不會重複播放
    /// </summary>
    /// <param name="color"></param>
    /// <param name="callback"></param>
    private void PlayFireworkTailEff(ColorType color, Action<Vector3> callback)
    {
        //Q.Log("PlayFireworkTailEff {0}", color);
        if (fireworkTailEffDict == null)
            fireworkTailEffDict = new Dictionary<ColorType, RectTransform>();

        Action<Animator, int> onSMExit = null;
        onSMExit = delegate(Animator arg1, int arg2)
        {
            Q.Assert(arg1.gameObject.activeSelf);
            Q.Assert(arg1.GetBehaviour<BaseStateMachineBehaviour>() != null);
            arg1.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent -= onSMExit;
            arg1.enabled = false;
            callback(arg1.transform.localPosition);
            StartCoroutine(Utils.DelayDeactive(arg1.gameObject));
        };

        RectTransform eff = fireworkTailEffDict[color] as RectTransform;

        //說明當前正在播放
        if (eff.gameObject.activeSelf)
            return;

        eff.gameObject.SetActive(true);

        //控制音效的播放間隔
        float now = Time.time;
        eff.gameObject.GetComponent<AudioSource>().enabled = now - lastTailEffPlayTime >= 1f;
        lastTailEffPlayTime = now;


        Animator anim = eff.GetComponent<Animator>();

        if (anim == null)
        {
            return;
        }

        anim.enabled = true;
        anim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent += onSMExit;
        anim.SetInteger("ColorType", (int)color);
        //隨機選擇一條路線
        anim.SetInteger("PathID", UnityEngine.Random.Range(1, 4));
    }


    /// <summary>
    /// 播放煙花爆炸特效
    /// </summary>
    /// <param name="color"></param>
    /// <param name="localPosition">爆炸的位置</param>
    private IEnumerator PlayFireworkBoomEff(ColorType color, Vector3 localPosition)
    {
        int index = (int)color - 1;
        if (index < 0 || index >= FireworkEffDict.Count)
            yield break;

        RectTransform eff = fireworkBoomEffDict[color] as RectTransform;

        if (eff == null)
            yield break;

        eff.localPosition = localPosition;
        if (!eff.gameObject.activeSelf)
            eff.gameObject.SetActive(true);

        ParticleSystem prtSys = eff.GetComponent<ParticleSystem>();

        if (prtSys == null)
        {
            yield break;
        }

        Q.Assert(prtSys != null && !prtSys.loop, "FireBoom c={0}", color);
        prtSys.Play();
        eff.GetComponent<AudioSource>().Play();

        while (prtSys.IsAlive())
            yield return 0;

        eff.gameObject.SetActive(false);
    }

    /// <summary>
    /// 使用協程播放防禦特效
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayDefenseEffCoroutine()
    {
        if (!defenseEff.gameObject.activeSelf)
            defenseEff.gameObject.SetActive(true);

        ParticleSystem ps = defenseEff.GetComponent<ParticleSystem>();

        if (ps == null)
        {
            yield break;
        }

        Q.Assert(!ps.loop, "HitEffBeh:PlayDefenseEffect Assert 1");

        // 防禦音效
        GameController.Instance.AudioManager.PlayAudio("SD_attack_defense");
        ps.Play();

        while (ps.IsAlive())
            yield return 0;

        defenseEff.gameObject.SetActive(false);
    }

    /// <summary>
    /// 用協程播放受擊特效
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private IEnumerator PlayHitEffCoroutine(ColorType color)
    {
        if (color == ColorType.None)
            yield break;

        RectTransform eff = hitEffDict[color];

        Q.Assert(eff != null);

        if (eff == null)
            yield break;

        if (!eff.gameObject.activeSelf)
            eff.gameObject.SetActive(true);

        ParticleSystem ps = eff.GetComponent<ParticleSystem>();

        if (ps == null)
        {
            yield break;
        }

        Q.Assert(!ps.loop, "HitEff c={0}", color);

        //播放怪物受到攻擊音效
        //已經在Component上掛了受擊音效
        //GameController.Instance.AudioManager.PlayAudio("SD_attack_jewel");

        ps.Play();

        //控制音效的播放間隔
        float now = Time.time;
        if (now - lastPlayHitEffTime >= 1f)
        {
            eff.GetComponent<AudioSource>().Play();
        }
        lastPlayHitEffTime = now;

        while (ps.IsAlive())
            yield return 0;

        eff.gameObject.SetActive(false);
    }

    /// <summary>
    /// 用協程播放大爆炸特效
    /// </summary>
    private IEnumerator PlayBoomEffectCoroutine(GameObject[,] elements)
    {
        Q.Assert(boomEff != null);
        if (boomEff == null)
            yield break;

        //播放元素散開的表現
        PlayBoomElementEffect(elements, 10, 20, 0.05f, 0.1f, 0.2f);

        if (!boomEff.gameObject.activeSelf)
            boomEff.gameObject.SetActive(true);

        ParticleSystem ps = boomEff.GetComponent<ParticleSystem>();

        if (ps == null)
        {
            yield break;
        }

        Q.Assert(!ps.loop, "boomEff");

        ps.Play();
        while (ps.IsAlive())
            yield return 0;

        ps.gameObject.SetActive(false);
    }

    private void PlayBoomElementEffect(GameObject[,] elements, float hext, float vext, float gap, float time1, float time2)
    {

        Action<object> onRetComplete = delegate(object obj)
        {
            ShockwaveEntry entry = obj as ShockwaveEntry;
            Animator animator = entry.elem.GetComponent<Animator>();
            if (animator != null)
                animator.enabled = true;
        };
        Action<object> onMoveComplete = delegate(object obj)
        {
            ShockwaveEntry entry = obj as ShockwaveEntry;

            float hz = (entry.c < 2) ? hext : ((entry.c >= elements.GetLength(1) - 2) ? -hext : 0);
            Vector3 upPoint = entry.elem.transform.localPosition + new Vector3(hz, vext, 0);
            LeanTween.moveLocal(entry.elem, upPoint, time2)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(onRetComplete)
            .setOnCompleteParam(entry);
            LeanTween.scale(entry.elem, new Vector3(1, 1, 1), time2);
        };
        for (int r = 0, n = elements.GetLength(0); r < n; r++)
        {
            for (int c = 0, m = elements.GetLength(1); c < m; c++)
            {
                if (elements[r, c] == null)
                    continue;

                ShockwaveEntry entry = new ShockwaveEntry();
                entry.elem = elements[r, c];
                entry.r = r;
                entry.c = c;

                Animator animator = entry.elem.GetComponent<Animator>();
                if (animator != null)
                    animator.enabled = false;

                float hz = (c < 2) ? hext : ((c >= elements.GetLength(1) - 2) ? -hext : 0);
                Vector3 downPoint = entry.elem.transform.localPosition - new Vector3(hz, vext, 0);
                LeanTween.moveLocal(entry.elem, downPoint, time1)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnComplete(onMoveComplete)
                .setOnCompleteParam(entry)
                .setDelay(r * gap + 0.05f);
                LeanTween.scale(entry.elem, new Vector3(1.1f, 1.1f, 1), time1);
            }
        }
    }

}
