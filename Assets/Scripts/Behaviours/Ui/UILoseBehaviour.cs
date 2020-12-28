using Com4Love.Qmax;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com4Love.Qmax.Ctr;

public class UILoseBehaviour : MonoBehaviour
{
    public event Action<object> OnClickRestartButton;
    public event Action<object> OnClickMapButton;
    public event Action<object> OnClickKeyButton;

    public Button ButtonMap;
    public Button ButtonKey;

    public UIButtonBehaviour EnergyButton;
    public UIButtonBehaviour GemButton;
    public UIButtonBehaviour CoinButton;
 
    public UIButtonBehaviour ButtonRestart;
    public UIButtonBehaviour UpGradeButton;

    public Text TextEnergy;
    public Text TextGem;
    public Text TextUpgradeA;
    public Text TextUpgradeB;
    public Text TextStageTitle;
    public Text TextTips;
    public Text TextCostEnergy;
    public Text TextKey;
    /// <summary>
    /// 新增的金币 
    /// </summary>
    public Text TextCoin;
    public List<Text> TextCollecteds;

    public List<Text> TextAddAwardList;

    public Animator animator;

    Data data;

    public class Data
    {
        public int crtEnergy;
        public int crtMaxEnergy;
        public int crtkeyNum;
        public int crtGem;
        public int crtUpgradeA;
        public int crtUpgradeB;
        public int crtCoin;
        public int cltGem;
        public int cltEnergy;
        public int cltUpgradeA;
        public int cltUpgradeB;
        public int cltKeyNum;
        public int cltCoin;

        public Data()
        {
            //test
            crtEnergy = crtkeyNum = crtGem = crtUpgradeA = crtUpgradeB = crtCoin = 1;
            cltUpgradeB = cltUpgradeA = cltEnergy = cltGem = cltCoin = 99;
        }
    }

    public void SetOpenBoxAble(bool val)
    {
        ButtonKey.gameObject.SetActive(val);
    }

    /// <summary>
    /// 设置关卡信息（名称、tips、消耗体力）
    /// </summary>
    /// <param name="stageIndex"></param>
    /// <param name="title"></param>
    public void SetStageInfo(int stageIndex, string title, string tips, int costEnergy)
    {
        TextStageTitle.text = title;
        TextTips.text = tips;
        TextCostEnergy.text = costEnergy.ToString();
    }

    /// <summary>
    /// 应该是获取奖励之前的数值。 此数值加上获取到的奖励，才是当前奖励
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
        data.crtkeyNum = keyNum - data.cltKeyNum;
        data.crtGem = gem - data.cltGem;
        data.crtUpgradeA = upgradeA - data.cltUpgradeA;
        data.crtUpgradeB = upgradeB - data.cltUpgradeB;
        data.crtCoin =  coin - data.cltCoin;

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

        if (keyNum > 99)
        {
            TextKey.text = "N";
        }
        else
        {
            TextKey.text = keyNum.ToString();
        }
    }

    /// <summary>
    /// 设置收集到的道具数量
    /// </summary>
    /// <param name="gem"></param>
    /// <param name="key"></param>
    /// <param name="upgradeA"></param>
    /// <param name="upgradeB"></param>
    public void SetCollected(int gem, int key, int upgradeA, int upgradeB , int coin)
    {
        data.cltGem = gem;
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
            // 这里有点蹩脚，要维护请大吼文华
            TextCollecteds[0] = SetSropValue(TextAddAwardList[0], gem, pv, delegate ()
            {
                animator.SetInteger("Gem", data.cltGem);
            });
            TextCollecteds[2] = SetSropValue(TextAddAwardList[2], upgradeA, pv, delegate ()
            {
                animator.SetInteger("UpgradeA", data.cltUpgradeA);
            });
            TextCollecteds[3] = SetSropValue(TextAddAwardList[3], upgradeB, pv, delegate ()
            {
                animator.SetInteger("UpgradeB", data.cltUpgradeB);
            });
            TextCollecteds[4] = SetSropValue(TextAddAwardList[4], coin, pv, delegate ()
            {
                animator.SetInteger("Coin", data.cltCoin);
            });
        }
        else
        {
            SetTextAddAwardActive(false);
            SetTextCollectedActive(true);

            TextCollecteds[0].text = gem.ToString();
            TextCollecteds[1].text = key.ToString();
            TextCollecteds[2].text = upgradeA.ToString();
            TextCollecteds[3].text = upgradeB.ToString();
            TextCollecteds[4].text = coin.ToString();

            animator.SetInteger("Coin", data.cltCoin);
            animator.SetInteger("Gem", data.cltGem);
            animator.SetInteger("UpgradeA", data.cltUpgradeA);
            animator.SetInteger("UpgradeB", data.cltUpgradeB);
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
        if (TextCollecteds == null)
            return;

        foreach (Text text in TextCollecteds)
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

    public void OnDestroy()
    {
        OnClickRestartButton = OnClickMapButton = OnClickKeyButton = null;
        EnergyButton.onClick -= LeftButton_onClick;
        GemButton.onClick -= RightButton_onClick;
        ButtonRestart.onClick -= ButtonRestart_onClick;
        UpGradeButton.onClick -= ButtonUpGrade_OnClick;
    }

    public void Start()
    {
        if (GameController.Instance.AudioManager != null)
        {
            GameController.Instance.AudioManager.PauseBgm();
        }
    }

    // Use this for initialization
    protected void Awake()
    {
        data = new Data();
        EnergyButton.onClick += LeftButton_onClick;
        GemButton.onClick += RightButton_onClick;
        CoinButton.onClick += CoinButton_onClick;

        UpGradeButton.onClick += ButtonUpGrade_OnClick;
        ButtonRestart.onClick += ButtonRestart_onClick;
 
        ButtonMap.onClick.AddListener(delegate()
        {
            if (OnClickMapButton != null)
                OnClickMapButton(this);
        });

        ButtonKey.onClick.AddListener(delegate()
        {
            if (OnClickKeyButton != null)
                OnClickKeyButton(this);
        });

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

    void ButtonUpGrade_OnClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Open(PopupID.UIUpgrad);
        //GameController.Instance.SceneCtr.LoadLevel(Scenes.UpgradScene, null);
    }
    void ButtonRestart_onClick(UIButtonBehaviour button)
    {
        if (OnClickRestartButton != null)
            OnClickRestartButton(this);
    }

    void RightButton_onClick(UIButtonBehaviour button)
    {
        UIShop.Open(UIShop.TapIndex.GEM_INDEX);
    }

    void LeftButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Open(PopupID.UIPowerShop,null,true,true);
    }

    void stateMachine_StateExitEvent(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= stateMachine_StateExitEvent;
        string layerName = animator.GetLayerName(layerIndex);
        //避免Unity 5.0的一个bug，详见MakeMeCrash:Crash1
        StartCoroutine(DelayStateExitEvent(stateInfo, layerName));
    }

    private IEnumerator DelayStateExitEvent(AnimatorStateInfo stateInfo, string animLayerName)
    {
        yield return null;
        switch (animLayerName)
        {
            case "FlyGem":
                //钻石
                if (stateInfo.IsName("FlyGem"))
                {
                    NumEffect(TextCollecteds[0], data.cltGem, 0);
                    NumEffect(TextGem, data.crtGem, data.crtGem + data.cltGem);
                }
                break;
            case "FlyUpgradeA":
                //橘子
                if (stateInfo.IsName("FlyUpgradeA"))
                {
                    NumEffect(TextCollecteds[2], data.cltUpgradeA, 0);
                    NumEffect(TextUpgradeA, data.crtUpgradeA, data.crtUpgradeA + data.cltUpgradeA);
                }
                break;
            case "FlyUpgradeB":
                //水蜜桃
                if (stateInfo.IsName("FlyUpgradeB"))
                {
                    NumEffect(TextCollecteds[3], data.cltUpgradeB, 0);
                    NumEffect(TextUpgradeB, data.crtUpgradeB, data.crtUpgradeB + data.cltUpgradeB);
                }
                break;
            case "FlyCoin":
                if (stateInfo.IsName("FlyCoin"))
                {
                    NumEffect(TextCollecteds[4], data.cltCoin, 0);
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
