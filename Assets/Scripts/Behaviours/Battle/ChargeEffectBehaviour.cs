using Com4Love.Qmax;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeEffectBehaviour : MonoBehaviour
{
    /// <summary>
    /// EffectEarth, EffectFire, EffectWood, EffectWater, EffectGold
    /// </summary>
    [Tooltip("EffectEarth, EffectFire, EffectWood, EffectWater, EffectGold")]
    public RectTransform[] ColorLayerRoots;
    private GameObject StarEffer;

    // [Tooltip("Earth,Fire,Wood,Water,Gold")]
    //public Transform[] ChargeEffects;
    string[] chargeEffectsName = { "C_earth0113", "C_fire0113", "C_wood0113", "C_water0113", "C_gold0113" };
    string[] chargeAudioName = {  "SD_purpleSelectd_loop",
                                                    "SD_redSelectd_loop",
                                                    "SD_greenSelectd_loop",
                                                    "SD_blueSelectd_loop",
                                                    "SD_yellowSelectd_loop"
                                                };
    Animator[] chargeAnimators = { null,null,null,null,null};

    private Dictionary<ColorType, Transform[]> allEffs;


    void Start()
    {
        StartCoroutine(LoadEffect());        
    }

    //考虑放外部，有多少个伙伴就缓存多少特效，没有对应颜色的伙伴并不需要缓存
    IEnumerator LoadEffect()
    {
        yield return 0;

        StarEffer = GameController.Instance.PoolManager.PopFromInstancePool("Prefabs/Effects/EffectStar", true).gameObject;
        StarEffer.SetActive(false);

        string chargeEffectRoot = "Prefabs/Effects/Charge/";

        for (int i = 0; i < chargeEffectsName.Length; i++)
        {
            Transform t = GameController.Instance.PoolManager.PopFromInstancePool(string.Concat(chargeEffectRoot, chargeEffectsName[i]), true);
            t.gameObject.SetActive(false);
            t.SetParent(ColorLayerRoots[i]);
            t.localPosition = Vector3.zero;
            t.localScale = new Vector3(1, 1, 1);
            chargeAnimators[i] = t.GetComponent<Animator>();
        }
    }
 
    /// <summary>
    /// 设置集气特效, color==ColorType.None则全部不显示
    /// </summary>
    /// <param name="color"></param>
    /// <param name="level">[1,4]共有4阶特效  level>0 </param>
    public void PlayEffect(ColorType color, int level)
    {
        //Debug.Log("color:" + color);
        //Q.Log("PlayCharge c={0}, lv={1}", color, level);
        Q.Assert(level >= 0 && level <= 4);
        int effectIndex = (int)color - 1;
        Animator effectAnimator;

        for (int i = 0; i < chargeAnimators.Length; i++)
        {
            effectAnimator = chargeAnimators[i];

            if (effectAnimator == null)
            {
                continue;
            }

            if (i == effectIndex && level > 0)
            {
                //当前只
                effectAnimator.gameObject.SetActive(true);

                ///教学可能会隐藏伙伴
                if(effectAnimator.transform.parent.gameObject.activeInHierarchy)
                    effectAnimator.SetInteger("level", level);
            }
            else
            {
                effectAnimator.gameObject.SetActive(false);
            }
        }

        if (color == ColorType.None)
            return;

        AudioSource loopAudioSrc = chargeAnimators[effectIndex].GetComponent<AudioSource>();

        if (level <= 1)
        {
            loopAudioSrc.Stop();
        }
        else
        {
            int lv = Mathf.Min(level, 4);
            GameController.Instance.QMaxAssetsFactory.CreateEffectAudio(
            chargeAudioName[(int)color - 1] + lv,
            delegate(AudioClip clip)
            {
                loopAudioSrc.clip = clip;
                loopAudioSrc.Play();
            });
        }
    }

    public void PlayStar(ColorType color, int level)
    {
        if ((int)color >= 1)
        {
            StarEffer.transform.SetParent(ColorLayerRoots[(int)color - 1]);
            StarEffer.transform.localScale = new Vector3(1, 1, 1);
            StarEffer.transform.localPosition = Vector3.zero;
            StarEffer.transform.gameObject.SetActive(false);
            StarEffer.transform.gameObject.SetActive(true);
            //StarEffer.transform.gameObject.GetComponentInChildren<ParticleSystem>().Play();
        }
    }
}
