using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.Net;
using Com4Love.Qmax.Net.Protocols.Stage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitNewSelectWindowBehaviour : MonoBehaviour {

    //public UIUnitNewHeroGroup UnitGroup;
    //public StageConfig config;
    //public MapUiBehaviour mapBehaviour;
    //public Text TitleText;
    //public Text LimitText;
    //public RectTransform Goal;  //目标
    //public Image bg;
    //public Image AtlasImg;
    //public Text energyText;
    //public Transform Stars;
    //protected StageCtr stageCtr;
    
    //protected int star = 0;
    ////bool dataReady = false;
    
    //Stage data;
    //GameController gc;
    //void Start () {
    //    gc = GameController.Instance;
    //    gc.AtlasManager.AddAtlas(Atlas.UISelectUnitNew, AtlasImg.sprite.texture);

    //    GameController.Instance.Client.AddResponseCallback(Module.Stage, OnStageResponse);
    //}
	
    //// Update is called once per frame
    //void Update () {
	
    //}

    //public void OnEnable()
    //{
    //    mapBehaviour = FindObjectOfType<MapUiBehaviour>();
    //    stageCtr = GameController.Instance.StageCtr;

    //    config = GameController.Instance.Model.StageConfigs[stageId];
    //    //TitleText.text = config.ShowNum + " " + config.NameStringID;      //暂时直接取NameStringID当做名称
    //    TitleText.text = config.ShowNum + " " + Utils.GetTextByStringID(config.NameStringID);
    //    LimitText.text = config.StageLimit.ToString();
    //    energyText.text = config.CostEnergy.ToString();
    //    LoadBg();

    //    data = GameController.Instance.StageCtr.GetStageData(stageId);
    //    UpdateStar(data.star);
    //}

    ////加载背景图片
    //protected void LoadBg()
    //{
    //    Sprite sprite = GameController.Instance.QMaxAssetsFactory.CreateUiSelectUnitBg(config, new Vector2(.5f, .5f));
    //    bg.sprite = sprite;
    //}
 
    //protected void UpdateStar(int star_)
    //{
    //    star = star_;
    //    Transform t;
    //    Transform img;

    //    for (int i = 0; i < Stars.childCount; i++)
    //    {
    //        t = Stars.GetChild(i);
    //        img = t.GetChild(1);

    //        if (i < star)
    //        {
    //            img.gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            img.gameObject.SetActive(false);
    //        }
    //    }

    //    UpdateGoal();
    //}

    //protected void UpdateGoal()
    //{
    //    List<StageConfig.Goal> goals;
    //    StageConfig.Goal goal;

    //    switch (star)
    //    {
    //        case 0:
    //            goals = config.Goal1;
    //            break;
    //        case 1:
    //            goals = config.Goal2;
    //            break;
    //        default:
    //            goals = config.Goal3;
    //            break;
    //    }

    //    int goalItemCount = goals.Count;
    //    Q.Assert(goalItemCount > 0, "过关目标配置错误");

    //    Transform goalItem;
    //    GoalIcon icon;

    //    for (int i = 0; i < Goal.childCount; i++)
    //    {
    //        goalItem = Goal.GetChild(i);
    //        goalItem.gameObject.SetActive(false);
    //        icon = goalItem.GetComponent<GoalIcon>();

    //        if (i < goalItemCount)
    //        {
    //            goal = goals[i];
    //            icon.Data = goal;
    //            goalItem.gameObject.SetActive(true);
    //        }
    //    }
    //}

    //void ModelEventSystem_OnStageInfoRef(Stage obj)
    //{
    //    if (obj.stageId == config.ID)
    //    {
    //        data = obj;

    //        //dataReady = true;
    //    }
    //}

    //public void OnStartButtonClick()
    //{
    //    //lvl
    //    if(UnitGroup.b_isRunning)
    //    {
    //        return;
    //    }

    //    PlayerCtr player = GameController.Instance.PlayerCtr;
    //    if (player.PlayerData.energy < config.CostEnergy)
    //    {
    //        GameController.Instance.Popup.Open(PopupID.UIPowerShop);
    //        //GameController.Instance.Popup.ShowTextFloat("体力不足",this.transform as RectTransform);
    //        return;
    //    }

    //    Dictionary<string, object> sceneData = new Dictionary<string, object>();
    //    sceneData.Add("lvl", config.ID);
    //    List<int> units = GetSelectUnits();
    //    sceneData.Add("units", units);
    //    GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.MAP_SCENE_ENTER_BATTLE);

    //    //Dictionary<ColorType, Unit>  CrtUnitDict = new Dictionary<ColorType, Unit>();
    //    //for (int i = 0, n = units.Count; i < n; i++)
    //    //{
    //    //    QmaxModel qmaxModel = GameController.Instance.Model;
    //    //    Unit data = new Unit(qmaxModel.UnitConfigs[units[i]]);
    //    //    data.Hp = data.Config.UnitHp;
    //    //    CrtUnitDict.Add(data.Config.UnitColor, data);
    //    //}

    //    //List<int> param = new List<int>();
    //    //foreach (KeyValuePair<ColorType, Unit> pair in battleModel.CrtUnitDict)
    //    //{
    //    //    param.Add(pair.Value.Config.ID);
    //    //}
    //    gc.Client.BeginStage(config.ID, units);
    //    //GameController.Instance.Popup.Close(PopupID.UISelectHeroNew, false);
    //    //GameController.Instance.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData);
    //}

    //protected List<int> GetSelectUnits()
    //{
    //    List<int> selects = new List<int>();
    //    foreach (UnitConfig data in UnitGroup.UnitListData())
    //    {

    //        if (data != null)
    //            selects.Add(data.ID);
    //    }
    //    return selects;
    //}

    //public void OnCloseButtonClick()
    //{
    //    Debug.Log("OnCloseButtonClick");
    //    if(!UnitGroup.b_isRunning)
    //    {
    //        GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.MAP_SCENE_BACK);
    //        GameController.Instance.Popup.Close(PopupID.UISelectHeroNew);
    //    }
    //}

    //public void OnDestroy()
    //{
    //    GameController.Instance.Client.RemoveResponseCallback(Module.Stage, OnStageResponse);
    //    gc.AtlasManager.UnloadAtlas(Atlas.UISelectUnitNew);
    //}

    //private void OnStageResponse(byte module, byte cmd, short status, object value)
    //{
    //    if (cmd == (byte)StageCmd.STAGE_BEGIN)
    //    {
    //        //Dictionary<string, object> sceneData = new Dictionary<string, object>();
    //        //sceneData.Add("lvl", config.ID);
    //        //List<int> units = GetSelectUnits();
    //        //sceneData.Add("units", units);
    //        GameController.Instance.Popup.Close(PopupID.UISelectHeroNew, false);
    //        //GameController.Instance.SceneCtr.LoadLevel(Scenes.BattleScene, sceneData);
    //    }
    //}
}
