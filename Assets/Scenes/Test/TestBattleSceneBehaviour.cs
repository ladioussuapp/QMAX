using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用于测试战斗场景
/// </summary>
public class TestBattleSceneBehaviour : MonoBehaviour
{
    BattleBehaviour battleBeh;
    public Button ButtonNext;
    public int Level = 0;
    //0表示不更改起始点
    public int samplingId = 0;

    private PlayingRuleCtr playingRuleCtr;

    // Use this for initialization
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        battleBeh = Object.FindObjectOfType<BattleBehaviour>();

        battleBeh.OnMoveComplete += OnBattleMoveComplete;

        List<int> units = new List<int>();
        units.Add(1110);//Frog
        units.Add(2110);//Fox
        units.Add(3110);//Panda
        units.Add(4110);//Penguin
        units.Add(5110);//Monkey

        GameController gc = GameController.Instance;
        gc.Model.LoadConfigs(delegate(bool success)
        {
            playingRuleCtr = gc.PlayingRuleCtr;
            gc.Model.BattleModel = new BattleModelModifyAgent(gc.Model, gc.ModelEventSystem);
            playingRuleCtr.InitWithLevel(null, Level, units);
            First();
        });

        ButtonNext.onClick.AddListener(delegate()
        {
            //battleBeh.RoamNext();
            battleBeh.Next();
        });
    }

    void OnDestroy()
    {
        battleBeh.OnMoveComplete -= OnBattleMoveComplete;
    }

    private void OnBattleMoveComplete(int enmeyIndex)
    {
        //Q.Log("OnBattleMoveComplete, enmeyIndex={0}, enemyLeft={0}", enmeyIndex, battleBeh.EnemyLeft);
        //if (battleBeh.EnemyLeft > 0)
        //    Invoke("Next", 1f);
    }

    private void First()
    {
        StageConfig config = GameController.Instance.Model.BattleModel.CrtStageConfig;
        config.SamplingId = samplingId;

        battleBeh.First(config);
        //battleBeh.RoamFirst(samplingId);
    }
}
