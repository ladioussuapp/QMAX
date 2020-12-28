using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax;
using System;
using System.Collections.Generic;
using Com4Love.Qmax.Data.Config;
using UnityEngine.UI;

public class UpgradSceneBehaviour : MonoBehaviour {
    //public UIUpgradBehaviour UIUpgrad;
    public ActorGameResponse playerData;

	// Use this for initialization
	void Start () {
        //UIUpgrad = this.GetComponent<UIUpgradBehaviour>();
        if(GameController.Instance.PlayerCtr.PlayerData == null)
        {
            GameController.Instance.ModelEventSystem.OnPlayerInfoInit += ModelEventSystem_OnPlayerInfoInit;
        }
        else
        {
            UnitDataUpdate(true);
        }
	}

    void ModelEventSystem_OnPlayerInfoInit(Com4Love.Qmax.Net.Protocols.ActorGame.ActorGameResponse obj)
    {
        GameController.Instance.ModelEventSystem.OnPlayerInfoInit -= ModelEventSystem_OnPlayerInfoInit;
        //數據初始化
        UnitDataUpdate(true);
    }

    public void OnEnable()
    {
        GameController.Instance.ModelEventSystem.OnPlayerInfoRef += ModelEventSystem_OnPlayerInfoRef;
        GameController.Instance.ModelEventSystem.OnUnitUpgrad += OnUnitUpgrad;
    }

    void OnUnitUpgrad()
    {
        //音效不對
        GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.UPGRAD_SCENE_UPGRAD);
        //玩傢伙伴刷新
        UnitDataUpdate(false);
    }

    void ModelEventSystem_OnPlayerInfoRef(List<RewardType> arg1, ActorGameResponse arg2)
    {
        //玩家資源刷新
    }

    public void OnDisable()
    {
        GameController.Instance.ModelEventSystem.OnPlayerInfoRef -= ModelEventSystem_OnPlayerInfoRef;
        GameController.Instance.ModelEventSystem.OnUnitUpgrad -= OnUnitUpgrad;
    }
 
    protected void UnitDataUpdate(bool isInit)
    {
        playerData = GameController.Instance.PlayerCtr.PlayerData;
        int defaultSelectIndex = -1;

        //5種顏色
        UIUpgradStageItemData[] uiDatas = new UIUpgradStageItemData[5];
        UIUpgradStageItemData uiData;
        UpGradUnitItem.ItemData uiUnitData;
        //擁有的伙伴
        List<UnitConfig> ownUnits;
        //鎖定的伙伴
        List<UnitConfig> lockedUnits;
        //顯示的伙伴 = 擁有的伙伴 + 鎖定的伙伴
        List<UnitConfig> displayUnits;

        for (int i = 1; i <= 5; i++)
        {
            //每個界面的數據 最多4只夥伴
            uiData = new UIUpgradStageItemData();
            uiData.unitItemDatas = new List<UpGradUnitItem.ItemData>();
            
            uiDatas[i - 1] = uiData;
            uiData.ColorType = (ColorType)i;
 
            ownUnits = GameController.Instance.UnitCtr.GetOwnUnits(uiData.ColorType);
            lockedUnits = GameController.Instance.UnitCtr.GetLockUnits(100, (ColorType)i);
            displayUnits = new List<UnitConfig>();
            displayUnits.AddRange(ownUnits);
            displayUnits.AddRange(lockedUnits);

            //每個界面4只夥伴
            for (int j = 0; j < 4; j++)
            {
                if (displayUnits.Count <= j)
                {
                    //空夥伴，不顯示
                    uiData.unitItemDatas.Add(null);
                    continue;
                }

                uiUnitData = new UpGradUnitItem.ItemData();
                uiUnitData.config = displayUnits[j];
                uiUnitData.isLock = ownUnits.IndexOf(uiUnitData.config) > -1 ? false : true;
                uiUnitData.isSelect = GameController.Instance.Model.PlayerData.lastFightUnits.IndexOf(uiUnitData.config.ID) > -1;
                //沒有被鎖，並且資源夠升級
                uiUnitData.upgradAble = !uiUnitData.isLock && GameController.Instance.UnitCtr.CheckUpgradAble(uiUnitData.config,true,true);       
                uiData.unitItemDatas.Add(uiUnitData);

                if (isInit && uiUnitData.upgradAble)
                {
                    if (defaultSelectIndex == -1 || uiUnitData.config.UnitColor == ColorType.Wood)
                    {
                        //如果綠色也可以升級 則默認一定是綠色
                        defaultSelectIndex = i - 1;
                    }
                }
            }
        }

        if (isInit && defaultSelectIndex == -1)
        {
            //默認綠色
            defaultSelectIndex = 2;
        }
        UIUpgradBehaviour UIUpgrad = this.gameObject.GetComponent<UIUpgradBehaviour>();
        UIUpgrad.SetDatas(uiDatas, defaultSelectIndex);
    }
}
