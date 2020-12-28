using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols.getchance;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax.Net.Protocols.goods;

public class UIGetChanceBoxItem : MonoBehaviour
{
    Animator boxAnimator;
    Button button;
    AudioSource audioSource;

    public int index;
    public event Action<int> OnOpenBox;
    /// <summary>
    /// 盒子被打開 是否是夥伴,圖標
    /// </summary>
    public event Action<bool, UIGetChanceBoxItem> OnBoxTip;
    public event Action<Data> OnFlyEffectComplete;

    public bool effectWaitting = false;

    // Use this for initialization
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxAnimator = GetComponentInChildren<Animator>();
        button = GetComponent<Button>();
        //button.onClick.AddListener(OnClick);

        EventTriggerListener.Get(button.gameObject).onDown += UIGetChanceBoxItem_onDown;
    }

    void UIGetChanceBoxItem_onDown(GameObject go)
    {
        OnClick();
    }

    public void OnDestroy()
    {
        //StopAllCoroutines();
        EventTriggerListener.Get(button.gameObject).onDown -= UIGetChanceBoxItem_onDown;
    }

    public RectTransform RectTransform
    {
        get
        {
            return transform as RectTransform;
        }
    }

    void OnClick()
    {
        //PlayGift();

        if (OnOpenBox != null)
        {
            OnOpenBox(index);
        }
    }

    public void Open()
    {
        PlayGift();
    }

    /// <summary>
    /// 抖動動畫
    /// </summary>
    public void PlayRefresh()
    {
        boxAnimator.SetTrigger("RefreshTrigger");
        boxAnimator.SetBool("Refresh", true);
    }

    /// <summary>
    /// 爆炸動畫
    /// </summary>
    public void PlayGift()
    {
        boxAnimator.SetTrigger("GiftTrigger");
        boxAnimator.SetBool("Gift", true);
    }

    public void PlayIdle()
    {
        boxAnimator.SetBool("Gift", false);
        boxAnimator.SetBool("Refresh", false);
        boxAnimator.SetBool("Click", false);
    }

    public void PlayClick()
    {
        boxAnimator.SetBool("Click", true);
        boxAnimator.SetTrigger("ClickTrigger");
    }

    public void Reset()
    {
        boxAnimator.gameObject.SetActive(true);
        //NumText.gameObject.SetActive(true);

        PlayIdle();
    }

    public IEnumerator BoxEffect()
    {
        PlayGift();
        yield return new WaitForSeconds(.1f);

        //爆炸
        Transform effect = GameController.Instance.PoolManager.PrefabSpawn(UIGetChanceBehaviour.BOMB_EFFECT_SPAWN_KEY);
        effect.position = transform.position - Vector3.forward * 3;
        PlaySpawanAudio();
        yield return new WaitForSeconds(.2f);

        if (data.GConfig.GiftType == (int)RewardType.Unit)
        {
            RectTransform unitGift = GameController.Instance.PoolManager.PrefabSpawn(UIGetChanceBehaviour.UNITGIFT_UI_SPAWN_KEY) as RectTransform;
            Image unitBody = unitGift.Find("Body").GetComponent<Image>();

            UnitConfig uConfig = GameController.Instance.Model.UnitConfigs[data.vr.changeValue];

            //gift.gameObject.SetActive(false);
            boxAnimator.gameObject.SetActive(false);

            unitGift.gameObject.SetActive(true);
            GameController.Instance.QMaxAssetsFactory.CreateUnitSprite(
                uConfig, new Vector2(.5f, .5f),
                delegate (Sprite s)
                {
                    unitBody.sprite = s;
                    unitBody.SetNativeSize();
                });
            unitGift.SetParent(data.EffectRoot);
            unitGift.localScale = new Vector3(1f, 1f, 1f);
            unitGift.position = transform.position;

            yield return new WaitForSeconds(1.2f);

            //夥伴 彈出窗口
            if (OnBoxTip != null)
            {
                OnBoxTip(true, null);
            }
            GameController.Instance.PoolManager.Despawn(unitGift);
        }
        else if (data.GConfig.GiftType == (int)RewardType.Good)
        {
            //抽到物品
            //抽到的數量
            int getCount = data.vr == null ? data.GI.num : data.vr.changeValue;
            Sprite giftSprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIGetChance, data.GConfig.Icon);

            Q.Assert(giftSprite != null, string.Concat("抽奖素材缺少  :"), data.GConfig.Icon);

            RectTransform gift = GameController.Instance.PoolManager.PrefabSpawn(UIGetChanceBehaviour.GIFT_UI_SPAWN_KEY) as RectTransform;
            gift.gameObject.SetActive(true);
            Image giftBody = gift.GetComponent<Image>();
            giftBody.color = Color.white;
            Text NumText = gift.Find("NumText").GetComponent<Text>();
            NumText.text = getCount.ToString();
            NumText.gameObject.SetActive(true);
            Button giftButton = gift.Find("Button").GetComponent<Button>();
            giftButton.gameObject.SetActive(data.NeedTip);      //透明的點擊按鈕
            giftButton.onClick.AddListener(OnGiftClick);
            Transform TipImage = gift.Find("TipImage");
            TipImage.gameObject.SetActive(data.NeedTip);

            boxAnimator.gameObject.SetActive(false);

            giftBody.sprite = giftSprite;
            giftBody.SetNativeSize();

            gift.SetParent(data.EffectRoot);
            gift.localScale = new Vector3(1, 1, 1);
            gift.position = transform.position;

            yield return new WaitForSeconds(0.6f);

            if (data.NeedTip)
            {
                yield return new WaitForSeconds(0.4f);
            }

            //如果提示窗口被彈出，則需要將動畫暫時停住，直到窗口被關閉
            while (effectWaitting)
            {
                yield return 0;
            }

            if (data.NeedTip)
            {
                yield return new WaitForSeconds(.4f);
            }

            NumText.gameObject.SetActive(false);
            TipImage.gameObject.SetActive(false);

            if (giftButton != null)
            {
                giftButton.onClick.RemoveListener(OnGiftClick);
            }

            LeanTween.value(giftBody.gameObject, 1, 0, .8f).setOnUpdate(delegate (float val)
            {
                giftBody.color = new Color(giftBody.color.r, giftBody.color.g, giftBody.color.b, val);
            });

            yield return new WaitForSeconds(0.8f);

            if (gift != null)
            {
                GameController.Instance.PoolManager.Despawn(gift);
            }
        }
        else
        {
            //抽到材料
            //抽到的數量
            int getCount = data.vr == null ? data.GI.num : data.vr.changeValue;
            Sprite giftSprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIGetChance, data.GConfig.Icon);

            Q.Assert(giftSprite != null, string.Concat("抽奖素材缺少  :"), data.GConfig.Icon);

            RectTransform gift = GameController.Instance.PoolManager.PrefabSpawn(UIGetChanceBehaviour.GIFT_UI_SPAWN_KEY) as RectTransform;
            gift.gameObject.SetActive(true);

            Image giftBody = gift.GetComponent<Image>();
            giftBody.color = Color.white;
            Text NumText = gift.Find("NumText").GetComponent<Text>();
            NumText.text = getCount.ToString();
            NumText.gameObject.SetActive(true);
            Button giftButton = gift.Find("Button").GetComponent<Button>();
            giftButton.gameObject.SetActive(data.NeedTip);      //透明的點擊按鈕
            giftButton.onClick.AddListener(OnGiftClick);
            Transform TipImage = gift.Find("TipImage");
            TipImage.gameObject.SetActive(data.NeedTip);

            boxAnimator.gameObject.SetActive(false);

            giftBody.sprite = giftSprite;
            giftBody.SetNativeSize();

            gift.SetParent(data.EffectRoot);
            gift.localScale = new Vector3(1, 1, 1);
            gift.position = transform.position;
            Vector3 toPosition = new Vector3(data.FlyPoint.position.x, data.FlyPoint.position.y, gift.position.z);

            yield return new WaitForSeconds(0.6f);

            //有可能點擊的材料時間延長一點
            if (data.NeedTip)
            {
                yield return new WaitForSeconds(0.4f);
            }

            //如果提示窗口被彈出，則需要將動畫暫時停住，直到窗口被關閉
            while (effectWaitting)
            {
                yield return 0;
            }

            if (data.NeedTip)
            {
                yield return new WaitForSeconds(.4f);
            }

            NumText.gameObject.SetActive(false);
            TipImage.gameObject.SetActive(false);

            if (giftButton != null)
            {
                giftButton.onClick.RemoveListener(OnGiftClick);
            }

            //LTDescr lt;

            //圖標飛到位置
            LeanTween.value(gift.gameObject, gift.position, toPosition, .8f).setOnUpdate(delegate (Vector3 val)
            {
                gift.position = val;
            }).setEase(LeanTweenType.easeInQuart);

            yield return new WaitForSeconds(0.8f);

            //圖標飛完之後通知外部播放動畫
            if (OnFlyEffectComplete != null)
            {
                OnFlyEffectComplete(data);
            }

            NumText.gameObject.SetActive(true);

            if (gift != null)
            {
                GameController.Instance.PoolManager.Despawn(gift);
            }
        }

        PlayFlaycompleteAudio();
        GameController.Instance.PoolManager.Despawn(effect);
    }

    void OnGiftClick()
    {
        //Debug.Log("OnGiftClick");

        if (data.NeedTip && OnBoxTip != null)
        {
            OnBoxTip(false, this);
        }
    }

    void PlaySpawanAudio()
    {
        string acName = data.GConfig.Sound;

        if (string.IsNullOrEmpty(acName))
            return;

        GameController.Instance.QMaxAssetsFactory.CreateEffectAudio(
            acName, delegate (AudioClip clip)
            {
                audioSource.clip = clip;
                audioSource.Play();
            });
    }

    void PlayFlaycompleteAudio()
    {
        string acName = data.FlyCompleteClipName;

        if (string.IsNullOrEmpty(acName))
            return;

            GameController.Instance.QMaxAssetsFactory.CreateEffectAudio(
        acName, delegate (AudioClip clip)
        {
            if (audioSource.isActiveAndEnabled)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        });

        //Debug.LogWarning("播放聲音：" + acName);
    }

    public struct Data
    {
        /// <summary>
        /// vr與GI有一個會是null
        /// </summary>
        public ValueResult vr;
        public GoodsItem GI;
        public GetChanceConfig GConfig;
        public Transform EffectRoot;
        public string FlyCompleteClipName;
        public RectTransform FlyPoint;
        public bool NeedTip;

    }

    Data data;

    public Data GetData()
    {
        return data;
    }

    public void SetData(Data val)
    {
        data = val;
        //整個福袋面板會動態的隱藏和顯示 所以不能在此處執行協程
        //StartCoroutine(BoxEffect());
    }
}
