using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Stage;

namespace Com4Love.Qmax.Helper
{

    public class StartGameHelper
    {
        bool IsCanClickStart = true;
        List<int> UseUnitsList;
        StageConfig StageConfig;

        public void GoToGame(StageConfig config, List<int> useUnit)
        {
            if (!IsCanClickStart)
                return;

            IsCanClickStart = false;

            SetData(config, useUnit);

            //是否是活動場景
            if (GameController.Instance.StageCtr.IsActivityStage(config))
            {
                if (config.Mode == BattleMode.Tree)
                {
                    //現在的活動關卡
                    EnterTreeFight();
                }
            }
            else
            {
                EnterFight();
            }

            GameController.Instance.ModelEventSystem.OnStageBeginErrorEvent += SetStartButtonCanClick;
        }

        void SetStartButtonCanClick()
        {
            GameController.Instance.ModelEventSystem.OnStageBeginErrorEvent -= SetStartButtonCanClick;
            IsCanClickStart = true;
        }

        void EnterFight()
        {
            //進入普通戰鬥
            if (!GameController.Instance.StageCtr.CheckEnergyEnough(StageConfig.ID))
            {
                IsCanClickStart = true;
                GameController.Instance.Popup.Open(PopupID.UIPowerShop, null, true, true);
                return;
            }
 
            GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.MAP_SCENE_ENTER_BATTLE);
            GameController.Instance.ModelEventSystem.OnStageBeginEvent += ModelEventSystem_OnStageBeginEvent;

            //默認選擇所有主動道具帶入遊戲//
            if (StageConfig.ID > GuideManager.GuideStageID)
            {
                List<int> activeprop = GameController.Instance.PropCtr.GetAllActivePropIDList();
                GameController.Instance.PropCtr.SetPropSelect(activeprop);
            }

            ///所有使用的被動道具數量///
            Dictionary<int, int> usegoods = GameController.Instance.PropCtr.GetAllPassivePropUseDic();
            GameController.Instance.StageCtr.BeginStage(StageConfig.ID, UseUnitsList, usegoods);
        }

        //現在是進入活動關卡
        void EnterTreeFight()
        {
            if (!GameController.Instance.StageCtr.CheckEnergyEnough(StageConfig.ID))
            {
                IsCanClickStart = true;
                GameController.Instance.Popup.Open(PopupID.UIPowerShop, null, true, true);
                return;
            }

            GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.MAP_SCENE_ENTER_BATTLE);

            //Debug.Log("測試協議 臨時標記 別刪");
            //GameController.Instance.StageCtr.GetCounterpartStageList();
            //return;

            //進入大樹戰鬥
            GameController.Instance.ModelEventSystem.OnBeginCounterpartStage += OnBeginCounterpartStage;
            GameController.Instance.StageCtr.BeginCounterpartStage(StageConfig.ID, UseUnitsList);
        }

        void ModelEventSystem_OnStageBeginEvent()
        {
            IsCanClickStart = true;
            GameController.Instance.ModelEventSystem.OnStageBeginEvent -= ModelEventSystem_OnStageBeginEvent;

            LoadBattleLevel();
        }

        void OnBeginCounterpartStage()
        {
            IsCanClickStart = true;
            GameController.Instance.ModelEventSystem.OnBeginCounterpartStage -= OnBeginCounterpartStage;
            LoadBattleLevel(true);
        }

        void LoadBattleLevel(bool isTree = false)
        {
            Dictionary<string, object> sceneData = new Dictionary<string, object>();

            sceneData.Add("lvl", StageConfig.ID);
            sceneData.Add("units", UseUnitsList);
            if (isTree)
            {
                sceneData.Add(Scenes.Tree.ToString(), Scenes.Tree);
            }
            if (GameController.Instance.Popup.IsPopup(PopupID.UISelectHero))
                GameController.Instance.Popup.Close(PopupID.UISelectHero, false);

            if (GameController.Instance.Popup.IsPopup(PopupID.UISelectProp))
                GameController.Instance.Popup.Close(PopupID.UISelectProp, false);

            GameController.Instance.Model.PlayerData.lastFightUnits = UseUnitsList;        //刷新上次出战伙伴   新选人界面待加上
            GameController.Instance.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData, true, true);

        }

        /// <summary>
        /// 設置道歉選擇道具界面數據信息//
        /// </summary>
        public void SetData(StageConfig config, List<int> useUnit)
        {
            StageConfig = config;
            //Stage = GameController.Instance.StageCtr.GetStageData(StageConfig.ID);
            UseUnitsList = useUnit;
        }

        public void Clear()
        {
            GameController.Instance.ModelEventSystem.OnStageBeginEvent -= ModelEventSystem_OnStageBeginEvent;
            GameController.Instance.ModelEventSystem.OnBeginCounterpartStage -= OnBeginCounterpartStage;
        }

    }
}

