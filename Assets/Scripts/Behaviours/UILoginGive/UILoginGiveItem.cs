using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;
using System;
using System.Collections.Generic;

public class UILoginGiveItem : MonoBehaviour
{
    public Text TextNum;
 
    public Data data;
    public Transform StateGived;
    public Transform StateUnGive;
    public RectTransform UnGiveContent;
    public Image StateUnGiveIcon;
    //public RectTransform Stamp;
    //public EffectSpawn effectSpawn;
    //public EffectSpawn AwardEffect;
    //bool IsAwardClick = false;

    Animator Ani;

    public GameObject CurrDayEffect;
    public GameObject TickAward;

    public UIButtonBehaviour InfoButton;

    private int TitleID;
    private int InfoID;
    int Number;


    public struct Data
    {
        public LoginRewardConfig Config;

        /// <summary>
        /// -1未領取，不可領 0 本次可領取 1 已領取
        /// </summary>
        public int State;

        ///當天獎勵是否可領取///
        public bool IsCanAward;
    }

    public void SetData(Data data_)
    {
        InfoButton.onClick += OnInfoButtonClick;
        this.data = data_;

        StateGived.gameObject.SetActive(false);
        //StateUnGive.gameObject.SetActive(true);
        CurrDayEffect.SetActive(false);
        TickAward.SetActive(false);
        Ani = GetComponent<Animator>();

        SetInfo(data_);

        ///當天獎勵狀態//
        if (data.State == 0)
        {
            if (GameController.Instance.Model.LoginGiveData.IsCanAward)
            {
                //UILoginGive loginGive = GetComponentInParent<UILoginGive>();
                //loginGive.SuccessEvent += AwardSuccess;
                
                GameController.Instance.ViewEventSystem.LoginSuccessAwardEvent += AwardSuccess;
                Ani.Play("Star");
                ///當前天數獎勵的特效打開//
                CurrDayEffect.SetActive(true);
            }
            else
            {
                //已領取
                StateGived.gameObject.SetActive(true);
                TickAward.SetActive(true);
            }

        }
        else if (data.State == 1)///已經領取狀態//
        {
            //已領取
            StateGived.gameObject.SetActive(true);
            //StateUnGive.gameObject.SetActive(false);
            TickAward.SetActive(true);
        }
        else if (data.State == -1)///還沒有領取狀態//
        {
            ///上面代碼初始化的設置就是沒有領取狀態///
            ///所以這裡不用繼續設置///
        }


    }

    //設置獎勵圖片Icon///
    void SetInfo(Data data_)
    {
        LoginRewardConfig config = data.Config;
        string icon;
        int count;
        Atlas atlas;

        if (config.IsUnitGift)
        {

            //是夥伴獎勵時  數量顯示為夥伴的數量。當玩家擁有所有夥伴時，數量顯示為所有夥伴對應到的鑽石數量之和
            if (CheckUnitsHasAll(config.LoginGifts))
            {
                //玩家擁有所有夥伴，則顯示送鑽石。數量為所有鑽石的數量
                icon = config.Icon2;
                count = config.GemGift.AmountOrUnitId * config.LoginGifts.Length;

                if (config.Day == 7)
                {
                    //第七天的ICON替換圖片在自己紋理裡面
                    atlas = Atlas.UILoginGive;
                }
                else
                {
                    atlas = Atlas.UIComponent;
                }

                InfoID = config.InfoID2;
                TitleID = config.TitleID2;
            }
            else
            {
                //玩家擁有部分夥伴，或者沒有其中的伙伴。
                TitleID = config.TitleID;
                icon = config.Icon;
                count = config.LoginGifts.Length;
                //小伙伴的圖片不能用公用的，直接用登錄獎勵的紋理內的
                atlas = Atlas.UILoginGive;
                InfoID = config.InfoID;
                //夥伴獎勵時不顯示數字
                TextNum.gameObject.SetActive(false);
            }
        }
        else
        {
            //不是夥伴獎勵
            TitleID = config.TitleID;
            icon = config.Icon;
            InfoID = config.InfoID;
            count = config.LoginGifts[0].AmountOrUnitId;    //普通獎勵默認只有一種
            atlas = Atlas.UIComponent;
        }

        Number = count;
        TextNum.text = "x" + count;

        if (icon != "")
        {
            StateUnGiveIcon.sprite = GameController.Instance.AtlasManager.GetSprite(atlas, icon);
            Q.Assert(StateUnGiveIcon.sprite != null, "UILoginGive 缺少圖標素材：" + icon);
        }
    }

    /// <summary>
    /// 玩家是否擁有所有的伙伴。
    /// </summary>
    /// <param name="gifts"></param>
    /// <returns></returns>
    bool CheckUnitsHasAll(LoginRewardConfig.GiftItem[] gifts)
    {
        for (int i = 0; i < gifts.Length; i++)
        {
            LoginRewardConfig.GiftItem item = gifts[i];

            if (!GameController.Instance.UnitCtr.HasTypeUnit(item.AmountOrUnitId))
            {
                //擁有其中的伙伴
                return false;
            }
        }

        return true;
    }

    void AwardSuccess()
    {
        GameController.Instance.ViewEventSystem.LoginSuccessAwardEvent -= AwardSuccess;

        Ani.Play("Receive");

        CurrDayEffect.SetActive(false);
        StateGived.gameObject.SetActive(true);

    }

    void OnDestroy()
    {
        ///一定要記得銷毀註銷，否則第二次打開會調用null///
        GameController.Instance.ViewEventSystem.LoginSuccessAwardEvent -= AwardSuccess;
        InfoButton.onClick -= OnInfoButtonClick;
    }

    void OnInfoButtonClick(UIButtonBehaviour button)
    {
        GameController.Instance.AudioManager.PlayAudio("SD_ui_7day_open");
        UICommonDialogBehaviour uiCommon = GameController.Instance.Popup.Open(PopupID.UICommonDialog, null, true, true).GetComponent<UICommonDialogBehaviour>();
        uiCommon.CloseEvent += delegate()
        {
            GameController.Instance.AudioManager.PlayAudio("SD_ui_7day_close");
        };
        List<Goods> datas = new List<Goods>();
        Goods data = new Goods();
        data.GoodsSprite = StateUnGiveIcon.sprite;
        data.GoodsSpriteSize = StateUnGiveIcon.GetComponent<RectTransform>().sizeDelta;
        data.Num = TextNum.text.Replace("x", "");

        datas.Add(data);

        string info = string.Format(Utils.GetTextByID(InfoID), Number);
        string title = Utils.GetTextByID(TitleID);

        uiCommon.SetInfo(title, info, datas);
    }
}
