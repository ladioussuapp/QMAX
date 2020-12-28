using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using System.Collections.Generic;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax;
using Com4Love.Qmax.Net.Protocols.Stage;

public class LevelTool : MonoBehaviour
{

    //BattleModelModifyAgent batModel = null;

    //ModelEventSystem modelEvtSys;
    //ViewEventSystem viewEvtSys;

    //ElementRuleCtr elementRuleCtr;
    ////BoardModifyingRules target;
    //PlayingRuleCtr playingRuleCtr;

    //DataConfigContainer confContainer;
    //LevelConfigContainer lvConfContainer;

    //IEnumerator Start()
    //{
    //    confContainer = new DataConfigContainer();
    //    lvConfContainer = new LevelConfigContainer(null);

    //    yield return StartCoroutine(confContainer.LoadDataConfig(null,
    //        delegate(bool success) {/*do nothing*/}
    //        ));

    //    viewEvtSys = new ViewEventSystem();
    //    modelEvtSys = new ModelEventSystem();
    //    batModel = new BattleModelModifyAgent(
    //        confContainer.TileObjectConfigs,
    //        confContainer.GameSystemConfig,
    //        confContainer.SkillConfigs,
    //        confContainer.ComboConfigs,
    //        null);

    //    elementRuleCtr = new ElementRuleCtr(batModel, confContainer.TileObjectConfigs, 7, 7);

    //    playingRuleCtr = new PlayingRuleCtr(
    //        modelEvtSys,
    //        viewEvtSys);

    //    playingRuleCtr.SetConfigs(
    //        confContainer.UnitConfigs,
    //        confContainer.TileObjectConfigs,
    //        confContainer.ComboConfigs,
    //        confContainer.SkillConfigs);

    //    int level = 1;
    //    StageConfig stageConf = confContainer.StageConfigs[level];
    //    LevelConfig lvConf = lvConfContainer.GetBattleLevel(stageConf.GameSetting + "_1");
    //    Stage stageData = new Stage();
    //    List<int> units = new List<int>();
    //    units.Add(1102);
    //    playingRuleCtr.InitWithLevel(null,
    //        batModel,
    //        lvConf,
    //        stageConf,
    //        stageData,
    //        units);

    //    target = new BoardModifyingRules(
    //        batModel, null,
    //        elementRuleCtr,
    //        viewEvtSys,
    //        null,
    //        confContainer.TileObjectConfigs,
    //        7,
    //        7
    //    );
    //}
}
