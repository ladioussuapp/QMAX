using Com4Love.Qmax;
using Com4Love.Qmax.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIWinBehaviour : MonoBehaviour
{
    public event Action<object> OnClickOKButton;
    public event Action<object> OnClickShareButton;
 
    public UIButtonBehaviour ButtonOK;
    public Button ButtonShare;
    public Text TextEnergy;
    public Text TextGem;
    public Text TextCoin;
    public Text TextUpgradeA;
    public Text TextUpgradeB;
    public Text TextStageTitle;
    public List<Image> ImageStars;
    /// <summary>
    /// 鑽石 鑰匙 橘子 桃子 金幣
    /// </summary>
    public List<Text> TextCollectedList;

    public List<Text> TextAddAwardList;

    public List<Image> ImageUnitList;

    public UIButtonBehaviour EnergyButton;
    public UIButtonBehaviour GemButton;
    public UIButtonBehaviour CoinButton;

    Data data;
 

    Animator animator;

    public class Data
    {
        public int crtEnergy;
        public int crtMaxEnergy;
        public int crtkeyNum;
        public int crtGem;
        public int crtUpgradeA;
        public int crtUpgradeB;
        public int crtCoin;
        public int cltGems;
        public int cltKeyNum;
        public int cltUpgradeA;
        public int cltUpgradeB;
        public int cltCoin;

        public Data()
        {
            crtEnergy = crtMaxEnergy = crtkeyNum = crtGem = crtUpgradeA = crtUpgradeB = 0;
            cltUpgradeB = cltUpgradeA = cltKeyNum = cltGems = crtCoin = cltCoin = 0;
        }
    }

    /// <summary>
    /// 設置關卡信息（名稱、星星數量）
    /// </summary>
    /// <param name="stageIndex"></param>
    /// <param name="title"></param>
    public void SetStageInfo(int stageIndex, string title, int starNum)
    {
        TextStageTitle.text = title;
 
        Q.Assert(starNum <= ImageStars.Count);
        for (int i = 0, n = ImageStars.Count; i < n; i++)
        {
            ImageStars[i].gameObject.SetActive(i <= starNum);
        }

        animator.SetInteger("Star", starNum);
    }

    /// <summary>
    /// 應該是獲取獎勵之前的數值。此數值加上獲取到的獎勵，才是當前獎勵
    /// </summary>
    /// <param name="energy"></param>
    /// <param name="maxEnergy"></param>
    /// <param name="keyNum"></param>
    /// <param name="gem"></param>
    /// <param name="upgradeA"></param>
    /// <param name="upgradeB"></param>
    public void SetCrtStatus(int energy, int maxEnergy, int keyNum, int gem, int upgradeA, int upgradeB , int coin)
    {
        data.crtEnergy = energy;
        data.crtMaxEnergy = maxEnergy;
        data.crtkeyNum = keyNum;
        data.crtGem = gem - data.cltGems;              //動畫漲上去
        data.crtUpgradeA = upgradeA - data.cltUpgradeA;
        data.crtUpgradeB = upgradeB - data.cltUpgradeB;
        data.crtCoin = coin - data.cltCoin;

        //TextEnergy.text = energy + "/" + maxEnergy;
        if (energy == -1 && maxEnergy == -1)
        {
            TextEnergy.text = string.Format("{0} / {1}", "∞", "∞");
        }
        else
        {
            TextEnergy.text = energy + "/" + maxEnergy;
        }
        TextGem.text = data.crtGem.ToString();
        TextUpgradeA.text = data.crtUpgradeA.ToString();
        TextUpgradeB.text = data.crtUpgradeB.ToString();
        TextCoin.text = data.crtCoin.ToString();
    }

    /// <summary>
    /// 設置收集到的道具數量
    /// </summary>
    /// <param name="Gem"></param>
    /// <param name="energy"></param>
    /// <param name="upgradeA"></param>
    /// <param name="upgradeB"></param>
    public void SetCollected(int Gem, int key, int upgradeA, int upgradeB ,  int coin)
    {
        data.cltGems = Gem;
        data.cltKeyNum = key;
        data.cltUpgradeA = upgradeA;
        data.cltUpgradeB = upgradeB;
        data.cltCoin = coin;
        if (GameController.Instance.PropCtr.GetPropUseNum(PropType.AddAward) > 0)
        {
            float pv = GameController.Instance.PropCtr.GetPropValue(PropType.AddAward);
            SetTextAddAwardActive(true);
            SetTextCollectedActive(false);

            SetSropValue(TextAddAwardList[1], key, pv);
            // 這裡有點蹩腳，要維護請大吼文華
            TextCollectedList[0] = SetSropValue(TextAddAwardList[0], Gem, pv, delegate ()
            {
                animator.SetInteger("Gem", data.cltGems);
            });
            TextCollectedList[2] = SetSropValue(TextAddAwardList[2], upgradeA, pv, delegate ()
            {
                animator.SetInteger("UpgradeA", data.cltUpgradeA);
            });
            TextCollectedList[3] = SetSropValue(TextAddAwardList[3], upgradeB, pv, delegate ()
            {
                animator.SetInteger("UpgradeB", data.cltUpgradeB);
            });
            TextCollectedList[4] = SetSropValue(TextAddAwardList[4], coin, pv, delegate ()
            {
                animator.SetInteger("Coin", data.cltCoin);
            });
        }
        else
        {
            SetTextAddAwardActive(false);
            SetTextCollectedActive(true);

            TextCollectedList[0].text = Gem.ToString();
            TextCollectedList[1].text = key.ToString();
            TextCollectedList[2].text = upgradeA.ToString();
            TextCollectedList[3].text = upgradeB.ToString();
            TextCollectedList[4].text = coin.ToString();

            animator.SetInteger("Gem", data.cltGems);
            animator.SetInteger("UpgradeA", data.cltUpgradeA);
            animator.SetInteger("UpgradeB", data.cltUpgradeB);
            animator.SetInteger("Coin", data.cltCoin);
        }
    }


    private void SetTextAddAwardActive(bool isActive)
    {
        if (TextAddAwardList == null)
            return;

        foreach (Text text in TextAddAwardList)
        {
            text.transform.gameObject.SetActive(isActive);
        }
    }

    private void SetTextCollectedActive(bool isActive)
    {
        if (TextCollectedList == null)
            return;

        foreach(Text text in TextCollectedList)
        {
            text.transform.gameObject.SetActive(isActive);
        }
    }

    private Text SetSropValue(Text text, int count, float pv, Action cb = null)
    {
        int a = Mathf.CeilToInt((count / (1 + pv / 100)));
        int b = count - a;
        Text texta = text.transform.Find("a").GetComponent<Text>();
        Text textb = text.transform.Find("b").GetComponent<Text>();
        if (cb == null)
        {
            texta.text = a.ToString();
            textb.text = b.ToString();
        }
        else
        {
            texta.text = a.ToString();
            textb.text = b.ToString();
            StartCoroutine(Utils.DelayToInvokeDo(delegate ()
            {
                NumEffect(texta, a, a + b);
                NumEffect(textb, b, 0, cb);
            }, 1.0f));
        }
        return texta;
    }

    void OnDestroy()
    {
        OnClickOKButton = null;
        OnClickShareButton = null;
        ButtonOK.onClick -= ButtonOK_onClick;
        EnergyButton.onClick -= LeftButton_onClick;
        GemButton.onClick -= RightButton_onClick;
        CoinButton.onClick -= CoinButton_onClick;
    }

    // Use this for initialization
    void Awake()
    {
        data = new Data();

        ButtonOK.onClick += ButtonOK_onClick;
        EnergyButton.onClick += LeftButton_onClick;
        GemButton.onClick += RightButton_onClick;
        CoinButton.onClick += CoinButton_onClick;

        ButtonShare.onClick.AddListener(delegate()
        {
            if (OnClickShareButton != null)
                OnClickShareButton(this);
        });

        animator = GetComponent<Animator>();
        BaseStateMachineBehaviour[] stateMachines = animator.GetBehaviours<BaseStateMachineBehaviour>();

        foreach (BaseStateMachineBehaviour stateMachine in stateMachines)
        {
            stateMachine.StateExitEvent += stateMachine_StateExitEvent;
        }
    }

    private void CoinButton_onClick(UIButtonBehaviour button)
    {
        UIShop.Open(UIShop.TapIndex.COIN_INDEX);
    }

#if AUTO_FIGHT
    public void Start()
    {

        //測試 自動點很好
        Invoke("Test", 1.5f);
    }

    void Test()
    {
        //暫時不模擬按鈕 直接調用方法跳轉
        ButtonOK_onClick(ButtonOK);
    }
#endif


    void RightButton_onClick(UIButtonBehaviour button)
    {
        UIShop.Open(UIShop.TapIndex.GEM_INDEX);
    }

    void LeftButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Open(PopupID.UIPowerShop,null,true,true);
    }


    void ButtonOK_onClick(UIButtonBehaviour button)
    {
        if (OnClickOKButton != null)
            OnClickOKButton(this);
    }

    public void OnEnable()
    {
        if (GameController.Instance.AudioManager != null)
        {
            GameController.Instance.AudioManager.PauseBgm();
        }
    }

    void stateMachine_StateExitEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= stateMachine_StateExitEvent;
        StartCoroutine(DelayStateExitEvent(animator, stateInfo, layerIndex));
    }

    private IEnumerator DelayStateExitEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        yield return null;
        string layerName = animator.GetLayerName(layerIndex);
        switch (layerName)
        {
            case "FlyGem":
                //鑽石之前叫法是金幣
                if (stateInfo.IsName("FlyGem"))
                {
                    NumEffect(TextCollectedList[0], data.cltGems, 0);
                    NumEffect(TextGem, data.crtGem, data.crtGem + data.cltGems);
                }
                break;
            case "FlyUpgradeA":
                //橘子
                if (stateInfo.IsName("FlyUpgradeA"))
                {
                    NumEffect(TextCollectedList[2], data.cltUpgradeA, 0);
                    NumEffect(TextUpgradeA, data.crtUpgradeA, data.crtUpgradeA + data.cltUpgradeA);
                }
                break;
            case "FlyUpgradeB":
                //水蜜桃
                if (stateInfo.IsName("FlyUpgradeB"))
                {
                    NumEffect(TextCollectedList[3], data.cltUpgradeB, 0);
                    NumEffect(TextUpgradeB, data.crtUpgradeB, data.crtUpgradeB + data.cltUpgradeB);
                }
                break;
            case "FlyCoin":
                if (stateInfo.IsName("FlyCoin"))
                {
                    NumEffect(TextCollectedList[4], data.cltCoin, 0);
                    NumEffect(TextCoin, data.crtCoin, data.crtCoin + data.cltCoin);
                }
                break;
            default:
                break;
        }
    }

    void NumEffect(Text text, int from, int to, Action cb = null)
    {
        GameController.Instance.EffectProxy.ScrollText(text, from, to, 0.3f, false, cb);
    }

}
