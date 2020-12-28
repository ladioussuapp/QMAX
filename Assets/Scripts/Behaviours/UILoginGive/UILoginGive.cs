using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using System.Collections.Generic;
using Com4Love.Qmax.Net.Protocols;
using System;

public class UILoginGive : MonoBehaviour {
    public Image AtlasImg;
    public Transform[] ItemPlaceHolders;
    [System.NonSerialized]
    public int RewardId = 1;    //測試 外部調用時會更改
    public List<ValueResult> Reward;
    public UIButtonBehaviour OKButton;

    /// <summary>
    /// 一個是顯示button 一個是功能button是為了使得button的點擊範圍比顯示範圍大
    /// </summary>
    public UIButtonBehaviour AwardButton;
    public UIButtonBehaviour AwardImageButton;

    public Action SuccessEvent;

	// Use this for initialization
	void Start () {
        //測試用
        if (GameController.Instance.PlayerCtr.PlayerData == null)
        {
            GameController.Instance.ModelEventSystem.OnPlayerInfoInit += ModelEventSystem_OnPlayerInfoInit;
        }
        else
        {
            Init();
        }

        //GameController.Instance.Popup.ShowLightLoading();

    }

    void ModelEventSystem_OnPlayerInfoInit(Com4Love.Qmax.Net.Protocols.ActorGame.ActorGameResponse obj)
    {
        Init();
    }

    void Init()
    {
        //動畫完成之後再顯示關閉按鈕
        //OKButton.gameObject.SetActive(false);

        //AwardButton.gameObject.SetActive( GameController.Instance.PlayerCtr.IsHaveLoginAward);
        //AwardButton.button.interactable = GameController.Instance.PlayerCtr.IsHaveLoginAward;
        if (GameController.Instance.Model.LoginGiveData.Days == -1)
        {
            return;
        }

        Reward = GameController.Instance.Model.LoginGiveData.list;

        OKButton.onClick += OKButton_onClick;

        AwardButton.onClick += delegate(UIButtonBehaviour button)
        {
            ///鎖定按鈕點擊事件//
            AwardButton.interactable = false;

            Sign();

        };
        //AwardButton.button.interactable = GameController.Instance.Model.LoginGiveData.IsCanAward;
        UpdateUI();

        UILoginGiveItem.Data itemData = new UILoginGiveItem.Data();
        //放入mapUi中
        GameController.Instance.AtlasManager.AddAtlas(Atlas.UILoginGive, AtlasImg.sprite.texture);

        for (int i = 0; i < GameController.Instance.Model.LoginRewardConfigs.Count; i++)
        {
            //id從1開始
            LoginRewardConfig config = GameController.Instance.Model.LoginRewardConfigs[i + 1];

            if (i < ItemPlaceHolders.Length)
            {
                Transform placeHolder = ItemPlaceHolders[i];
                UILoginGiveItem item = placeHolder.GetComponentInChildren<UILoginGiveItem>();
                itemData.Config = config;

                //if (config.Id < this.RewardId)
                if(config.Id < GameController.Instance.Model.LoginGiveData.Days)
                {
                    //已經領過
                    itemData.State = 1;
                }
                else if (config.Id == GameController.Instance.Model.LoginGiveData.Days)
                {
                    //本次領取
                    itemData.State = 0;
                    //層級放到最高
                    placeHolder.SetAsLastSibling();
                }
                else
                {
                    //領不到
                    itemData.State = -1;
                }

                item.SetData(itemData);
            }
        }
    }

    public void Complete()
    {
        //動畫領取完成
        OKButton.gameObject.SetActive(true);

        if (Reward != null)
        {
            GameController.Instance.PlayerCtr.UpdateByResponse(Reward);
        }
    }

    void OKButton_onClick(UIButtonBehaviour button)
    {
        if (GameController.Instance.Popup.IsPopup(PopupID.UILoginGive))
        {
            GameController.Instance.Popup.Close(PopupID.UILoginGive);
            OKButton.onClick -= OKButton_onClick;
            //OKButton.GetComponent<CanvasGroup>().alpha = 0f;
        }
    }

    public void OnDestroy()
    {
        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UILoginGive);
        OKButton.onClick -= OKButton_onClick;
        GameController.Instance.ModelEventSystem.OnSignEvent -= ModelEventSystem_OnSignEvent;
        GameController.Instance.ModelEventSystem.OnSignUnlockEvent -= UnlockButton;

    }

    void Sign()
    {
        GameController.Instance.ModelEventSystem.OnSignEvent -= ModelEventSystem_OnSignEvent;
        GameController.Instance.ModelEventSystem.OnSignEvent += ModelEventSystem_OnSignEvent;

        GameController.Instance.ModelEventSystem.OnSignUnlockEvent -= UnlockButton;
        GameController.Instance.ModelEventSystem.OnSignUnlockEvent += UnlockButton;

        GameController.Instance.PlayerCtr.Sign();
    }
    void ModelEventSystem_OnSignEvent(Com4Love.Qmax.Net.Protocols.sign.SignResponse res)
    {
        GameController gc = GameController.Instance;
        gc.ModelEventSystem.OnSignEvent -= ModelEventSystem_OnSignEvent;

        ///獎勵已經領取不可重複領取///
        gc.Model.LoginGiveData.IsCanAward = false;
        if (res.valueResultListResponse.list != null)
        {
            ///刷新玩家數據///
            gc.PlayerCtr.UpdateByResponse(res.valueResultListResponse.list);
        }

        if (res.goodsItems.Count != 0)
            GameController.Instance.GoodsCtr.AddGoodsItem(res.goodsItems);

        if (gc.ViewEventSystem.LoginSuccessAwardEvent != null)
            gc.ViewEventSystem.LoginSuccessAwardEvent();

        gc.AudioManager.PlayAudio("SD_ui_seal");

        UpdateUI();
    }

    void UpdateUI()
    {
        bool isactive = GameController.Instance.Model.LoginGiveData.IsCanAward;

        AwardButton.gameObject.SetActive(isactive);
        AwardImageButton.gameObject.SetActive(isactive);

        ///播放延遲出現動畫///
        if (isactive)  
            AwardImageButton.DelayDisplayAni();


    }

    void UnlockButton()
    {
        GameController.Instance.ModelEventSystem.OnSignUnlockEvent -= UnlockButton;

        AwardButton.interactable = true;
    }

}
