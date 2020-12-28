using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using System.Collections.Generic;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data.Config;


public class TreeActivityHpProgressBar : MonoBehaviour
{
    public Slider[] LockSliders;
    public Slider PreSlider;
    public Slider HpSlider; //之前的3個進度條，換成1個

    public Image[] MarkImgs;
    public GameObject[] EffectFlyPrefabs;       //飛行動畫需要的3種prefab
    public EffectSpawn[] MarkEffectSpawns;
    public EffectSpawn[] MarkTipEffectSpawns;
    public Sprite MarkImgSpriteLight;
    public Sprite MarkImgSpriteDark;
    public Text HpText;

    public Transform[] HpAwardEffectPoints; //左上角橘子桃子鑽石的位置
    public Transform HpAwardEffectRoot;

    /// <summary>
    /// 3管血在整條血中的佔比
    /// </summary>
    float[] HpRatios = new float[3] { 0f, 0f, 0f };

    /// <summary>
    /// 0未達到  1預加載達到階段   2真實傷害達到階段
    /// </summary>
    int[] GroupState = new int[3]{0,0,0};

    //參數1： 0 , 1 , 2 橘子桃子鑽石 參數2：數量
    public event System.Action<int, int> OnAwardFlyIn;    //奖励飞入事件

    string[] EffectFlyPoolKeys;
    int locked = -1;    //哪個階段上了鎖
    //int maxHp = 0;
    int damageTotal = 0;
    int levelHpTotal = 0;
    int cutGroupHpTotal = 0;    

    public void InitMaxHp(int maxHp_)
    {
        //最大血量 超過最大血量時需要鎖定剩餘的進度條
        //maxHp = maxHp_;
        //HpText.text = string.Format("{0}/{1}", cutHp, maxHp);
    }

    #region 舊邏輯，有三個進度條。新邏輯改為一個進度條，通過進度條的百分比進度判斷到達哪個階段
    ///// <summary>
    ///// 設置進度 待更改成與預扣血相似邏輯，用單獨一個進度條
    ///// 在獲得獎勵後才播放特效
    ///// </summary>
    ///// <param name="val">數值</param>
    ///// <param name="maxValIndex">有三個進度條 索引表示屬於哪個進度條</param>
    ///// <param name="DamageTotal_"></param>
    ///// <param name="levelHpTotal_">當前進度的血量總和</param>
    //public void SetVal(int val, int maxValIndex, int DamageTotal_, int levelHpTotal_)
    //{
    //    Q.Assert(maxValIndex < 3, "TreeActivityHpProgressBar::SetVal maxValIndex超限");

    //    levelHpTotal = levelHpTotal_;
    //    damageTotal = DamageTotal_;

    //    HpText.text = string.Format("{0}/{1}", damageTotal, levelHpTotal_);

    //    if (maxValIndex == locked)
    //    {
    //        //被鎖了就不更新進度條
    //        return;
    //    }

    //    if (val > 0)
    //    {
    //        Sliders[maxValIndex].value = val;

    //        if (Sliders[maxValIndex].value == Sliders[maxValIndex].maxValue)
    //        {
    //            MarkEffect(maxValIndex, false);
    //        }
    //    }

    //    //打到下一條血條 則上一條血條置滿。當前是最後一條血時除外
    //    if (maxValIndex > 0)
    //    {
    //        MarkEffect(0, false);
    //        Sliders[0].value = Sliders[0].maxValue;

    //        if (maxValIndex == 2)
    //        {
    //            Sliders[1].value = Sliders[1].maxValue;
    //            MarkEffect(1, false);

    //            if (Sliders[2].normalizedValue == 1f)
    //            {
    //                MarkEffect(2, false);
    //            }
    //        }
    //    }
    //}
    #endregion

    public void SetValHp(float loseHpInGroup, int levelGroupHpTotal, int DamageTotal_, int levelHpTotal_)
    {
        levelHpTotal = levelHpTotal_;
        damageTotal = DamageTotal_;
        HpText.text = string.Format("{0}/{1}", damageTotal, levelHpTotal);
        HpSlider.value = loseHpInGroup;

        if (HpSlider.normalizedValue >= HpRatios[0] && locked != 0)
        {
            //達成第一個階段
            MarkEffect(0, true);

            if (HpSlider.normalizedValue >= HpRatios[1] + HpRatios[0] && locked != 1)
            {
                MarkEffect(1, true);

                if (HpSlider.normalizedValue == 1 && locked != 2)
                {
                    MarkEffect(2, true);
                }
                else
                {
                    CancelMarkEffect(2, true);
                }
            }
            else
            {
                CancelMarkEffect(1, true);
            }
        }
        else
        {
            CancelMarkEffect(0, true);
        }
    }

    //一個進度條測試 暫時用於預扣血 考慮將真實扣血用的3個進度條也換成一個進度條，可以根據進度比率來判斷是否達到某個階段
    public void SetValPreview(float loseHpInGroup, int newDamage)
    {
        if (loseHpInGroup > PreSlider.maxValue)
        {
            loseHpInGroup = PreSlider.maxValue;
        }

        HpText.text = string.Format("{0}/{1}", newDamage + damageTotal, levelHpTotal);
        PreSlider.value = loseHpInGroup;

        if (PreSlider.normalizedValue >= HpRatios[0] && locked != 0)
        {
            //達成第一個階段
            MarkEffect(0, true);

            if (PreSlider.normalizedValue >= HpRatios[1] + HpRatios[0] && locked != 1)
            {
                MarkEffect(1, true);

                if (PreSlider.normalizedValue == 1 && locked != 2)
                {
                    MarkEffect(2, true);
                }
                else
                {
                    CancelMarkEffect(2, true);
                }
            }
            else
            {
                CancelMarkEffect(1, true);
            }
        }
        else
        {
            CancelMarkEffect(0, true);
        }
    }

    public void PreViewBack()
    {
        PreSlider.value = 0;
    }

    /// <summary>
    /// lock 為0時 表示連第一個都要鎖定 為2時只要鎖定最後一個
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void Lock(int index)
    {
        locked = index;

        if (locked == -1)
        {
            //未鎖定
            return;
        }

        if (locked <= 2)
        {
            UpdateLockSliderPostion(2);

            if (locked <= 1)
            {
                UpdateLockSliderPostion(1);

                if (locked == 0)
                {
                    UpdateLockSliderPostion(0);
                }
            }
        }
    }

    /// <summary>
    /// 刷新 出現新的進度條 刷新刻度。 3段最大值
    /// </summary>
    public void Ref(int maxVal1, int maxVal2, int maxVal3)
    {
        if (locked == -1)
        {
            MarkImgs[0].sprite = MarkImgs[1].sprite = MarkImgs[2].sprite = MarkImgSpriteDark;
            PreSlider.value = 0;
            HpSlider.value = 0;
        }

        cutGroupHpTotal = 0;
        TreeFightCtr fightCtr = GameController.Instance.TreeFightCtr;
        cutGroupHpTotal = fightCtr.CutGroupHpTotal;

        //在awake裡面創建HpRatios只有1個元素？
        //HpRatios = new float[3];
        HpRatios[0] = (float)maxVal1 / cutGroupHpTotal;
        HpRatios[1] = (float)maxVal2 / cutGroupHpTotal;
        HpRatios[2] = 1 - HpRatios[0] - HpRatios[1];
 
        GroupState[0] = GroupState[1] = GroupState[2] = 0;
        PreSlider.maxValue = cutGroupHpTotal;
        HpSlider.maxValue = cutGroupHpTotal;
        UpdateSliderPosition();
        
    }

    void UpdateLockSliderPostion(int index)
    {
        Slider slider = LockSliders[index];
        slider.gameObject.SetActive(true);
        RectTransform sliderRT = slider.GetComponent<RectTransform>();
        RectTransform bodyRT = GetComponent<RectTransform>();

        Vector2 anchoredPosition = new Vector2(0, sliderRT.anchoredPosition.y);
 
        if (index == 1)
        {
            anchoredPosition = new Vector2(bodyRT.sizeDelta.x * HpRatios[0], sliderRT.anchoredPosition.y);
        }
        else if (index == 2)
        {
            anchoredPosition = new Vector2(bodyRT.sizeDelta.x * (HpRatios[1] + HpRatios[0]), sliderRT.anchoredPosition.y);
        }

        Vector2 sizeDelta = new Vector2(bodyRT.sizeDelta.x * HpRatios[index], sliderRT.sizeDelta.y);

        sliderRT.anchoredPosition = anchoredPosition;
        sliderRT.sizeDelta = sizeDelta;
    }

    void UpdateSliderPosition()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Vector2 Mark0 = new Vector2(rt.sizeDelta.x * HpRatios[0], MarkImgs[0].rectTransform.anchoredPosition.y);
        Vector2 Mark1 = new Vector2(rt.sizeDelta.x * HpRatios[1] + Mark0.x, MarkImgs[1].rectTransform.anchoredPosition.y);

        LeanTween.value(MarkImgs[0].gameObject, MarkImgs[0].rectTransform.anchoredPosition, Mark0, .3f).setOnUpdate(delegate(Vector2 val)
        {
            MarkImgs[0].rectTransform.anchoredPosition = val;
        });

        LeanTween.value(MarkImgs[1].gameObject, MarkImgs[1].rectTransform.anchoredPosition, Mark1, .3f).setOnUpdate(delegate(Vector2 val)
        {
            MarkImgs[1].rectTransform.anchoredPosition = val;
        });
    }

    void CancelMarkEffect(int index, bool preView)
    {
        if (preView)
        {
            if (GroupState[index] == 1)
            {
                MarkImgs[index].sprite = MarkImgSpriteDark;

                if (MarkTipEffectSpawns[index].LastEffectSpawned != null && MarkTipEffectSpawns[index].LastEffectSpawned.gameObject.activeSelf)
                {
                    MarkTipEffectSpawns[index].LastEffectSpawned.gameObject.SetActive(false);
                }

                GroupState[index] = 0;
            }
        }
        else
        {
            if (GroupState[index] == 2)
            {
                MarkImgs[index].sprite = MarkImgSpriteDark;

                GroupState[index] = 0;
            }
        }
    }

    //考慮將所有的特效調用都放在接受到ctr獲得獎勵邏輯裡
    void MarkEffect(int index, bool preView)
    {
        if (preView)
        {
            if (GroupState[index] == 0)
            {
                GroupState[index] = 1;
                MarkImgs[index].sprite = MarkImgSpriteLight;

                if (MarkTipEffectSpawns[index].LastEffectSpawned == null)
                {
                    MarkTipEffectSpawns[index].Spawn();
                }
                else if (!MarkTipEffectSpawns[index].LastEffectSpawned.gameObject.activeSelf)
                {
                    MarkTipEffectSpawns[index].LastEffectSpawned.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (GroupState[index] != 2)
            {
                CancelMarkEffect(index, true);

                GroupState[index] = 2;
                MarkEffectSpawns[index].Spawn();
                MarkImgs[index].sprite = MarkImgSpriteLight;
            }
        }
    }

    public void HpReward(TimeLimitedHPConfig hpConfig, int index)
    {
        //MarkEffect(index, false);     //用ctr獎勵事件做調用在界面上顯得有點奇怪
        MarkFlyEffect(hpConfig.UpgradeA, hpConfig.UpgradeB, hpConfig.Gem, index);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="upgradeA"></param>
    /// <param name="upgradeB"></param>
    /// <param name="gem"></param>
    /// <param name="index"></param>
    void MarkFlyEffect(int upgradeA, int upgradeB, int gem, int index)
    {
        List<TreeActivityFlyItemData> flyItems = new List<TreeActivityFlyItemData>();
        TreeActivityFlyItemData item;
        Vector3 position = MarkImgs[index].transform.position;
        int count;

        if (upgradeA > 0)
        {
            //count = (int)(upgradeA / 5) + 1;
            count = upgradeA;
            count = Mathf.Min(5, count);
            count = Mathf.Max(3, count);

            for (int i = 0; i < count; i++)
            {
                item = GetMarkPrefab(0, position);
                flyItems.Add(item);

                if (i == 0)
                {
                    item.Award = upgradeA;
                }
            }
        }

        if (upgradeB > 0)
        {
            count = upgradeB;
            count = Mathf.Min(3, count);

            for (int i = 0; i < count; i++)
            {
                item = GetMarkPrefab(1, position);
                flyItems.Add(item);

                if (i == 0)
                {
                    item.Award = upgradeB;
                }
            }
        }

        if (gem > 0)
        {
            count = gem;
            count = Mathf.Min(2, count);

            for (int i = 0; i < count; i++)
            {
                item = GetMarkPrefab(2, position);
                flyItems.Add(item);

                if (i == 0)
                {
                    item.Award = gem;
                }
            }
        }
 
        FlyItemsEffect(flyItems, position, index);
    }

    List<TreeActivityFlyItemData> flyItemQueue = new List<TreeActivityFlyItemData>();

    void FlyItemsEffect(List<TreeActivityFlyItemData> flyItems, Vector3 center , int orgIndex)
    {
        //分散 再緩動到左上角
        int rotaitonStart = UnityEngine.Random.Range(0, 360);
        flyItemQueue = flyItems;

        for (int i = 0; i < flyItems.Count; i++)
        {
            TreeActivityFlyItemData item = flyItemQueue[i];
            int randomRotaiton = rotaitonStart + UnityEngine.Random.Range(30, 80);
            rotaitonStart = randomRotaiton;
            Vector3 randomDir = (Quaternion.AngleAxis(randomRotaiton, Vector3.forward) * Vector3.down * 0.7f);
            //Vector3 fromPosition = item.transform.position;
            Vector3 toPosition1 = center + randomDir;
            Vector3 toPosition2 = HpAwardEffectPoints[item.Id].position;
            Vector3 ctrPosition = center + randomDir * 1.5f;
            Vector3[] paths = new Vector3[4] { toPosition1, ctrPosition, ctrPosition, toPosition2 };
            float delayTime1 = (flyItemQueue.Count - i) * 0.04f;
            float time1 = 0.3f;
            float delayTime2 = delayTime1 + time1 - 0.05f;
            float time2 = orgIndex * 0.15f + 0.6f;

            FlyItemsEffectStept0(item, toPosition1, rotaitonStart, time1, delayTime1);
            FlyItemsEffectStept1(item, paths, time2, delayTime2);
        }
    }

    void FlyItemsEffectStept0(TreeActivityFlyItemData item, Vector3 toPosition, int rotaitonStart, float time, float delayTime)
    {
        LeanTween.move(item.gameObject, toPosition, time).setDelay(delayTime).setEase(LeanTweenType.easeOutQuad);
    }

    void FlyItemsEffectStept1(TreeActivityFlyItemData item, Vector3[] paths, float time, float delayTime)
    {
        LeanTween.move(item.gameObject, paths, time).setDelay(delayTime)
        .setEase(LeanTweenType.easeInQuad)
        .setOnComplete(delegate()
        {
            if (OnAwardFlyIn != null && item.Award > 0)
            {
                OnAwardFlyIn(item.Id, item.Award);
            }

            GameController.Instance.PoolManager.Despawn(item.transform);
        });
    }

    //awardIndex  表示橘子桃子鑽石
    TreeActivityFlyItemData GetMarkPrefab(int awardIndex, Vector3 position)
    {
        //根據數量，創建不同數目的飛行物
        Transform markIconGo = GameController.Instance.PoolManager.PrefabSpawn(EffectFlyPoolKeys[awardIndex]);
        markIconGo.transform.SetParent(HpAwardEffectRoot);
        markIconGo.transform.position = position + Vector3.forward * 5;
        markIconGo.transform.localScale = new Vector3(1, 1, 1);
        markIconGo.gameObject.SetActive(true);
        TreeActivityFlyItemData item = markIconGo.GetComponent<TreeActivityFlyItemData>();
        item.Id = awardIndex;
        item.Award = 0;     //會放入對像池 所以要在這裡清零

        return item;
    }
 
    public void Start()
    {
        GroupState = new int[3];
        EffectFlyPoolKeys = new string[3];
        string key;

        for (int i = 0; i < EffectFlyPrefabs.Length; i++)
        {
            key = string.Format("TreeActivityHpProgressBar_POOL_KEY_{0}", i);                       //1点钟   脑袋一片浆糊
            GameController.Instance.PoolManager.PrePrefabSpawn(EffectFlyPrefabs[i].transform, key);
            EffectFlyPoolKeys[i] = key;
        }
    }

    public void OnDestroy()
    {
        if (EffectFlyPoolKeys != null)
        {
            for (int i = 0; i < EffectFlyPoolKeys.Length; i++)
            {
                GameController.Instance.PoolManager.Despool(EffectFlyPoolKeys[i]);
            }
        }
    }
 
}


