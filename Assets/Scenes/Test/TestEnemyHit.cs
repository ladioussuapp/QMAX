using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//[IntegrationTest.DynamicTestAttribute("UnitTestBattle")]
//[IntegrationTest.Ignore]
//[IntegrationTest.ExpectExceptions(false, typeof(ArgumentException))]
//[IntegrationTest.SucceedWithAssertions]
//[IntegrationTest.TimeoutAttribute(2)]
//[IntegrationTest.ExcludePlatformAttribute(RuntimePlatform.Android, RuntimePlatform.LinuxPlayer)]
public class TestEnemyHit : MonoBehaviour
{
    public Button Btn1;
    public Button Btn2;
    public Button Btn3;

    /// <summary>
    ///  关卡
    /// </summary>
    private int Level = 15;

    private BattleBehaviour battleBeh;
    private HitEnemyBehaviour enemyHitBeh;
    private PlayingRuleCtr playingRuleCtr;
    private BattleModel battleModel;

    void Start()
    {
        enemyHitBeh = Object.FindObjectOfType<HitEnemyBehaviour>();
        Btn1.onClick.AddListener(delegate()
        {
            battleBeh.Next();
        });

        Btn2.onClick.AddListener(delegate()
        {
            StartCoroutine(HitEnemy5());
        });


        Btn3.onClick.AddListener(delegate()
        {
            battleModel.MinusCrtEnemyHp(99);
            Unit enemy = battleModel.GetCrtEnemy();
            bool isKilled = enemy == null;
            //bool isWeakState = !isKilled && enemy.Hp < enemy.Config.UnitHp;
            bool isWeakState = !isKilled && enemy.Hp < enemy.HpMax;
            enemyHitBeh.PlayHit(battleBeh.GetCurrentEnemyPoint(), 1, 1, 99, isWeakState, enemy != null);
        });
        StartCoroutine(LateInit());
    }


    IEnumerator LateInit()
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
        playingRuleCtr = gc.PlayingRuleCtr;
        gc.Model.BattleModel = new BattleModelModifyAgent(gc.Model, gc.ModelEventSystem);
        battleModel = gc.Model.BattleModel;
        playingRuleCtr.InitWithLevel(null, Level, units);
        First();
    }

    private void OnBattleMoveComplete(int enmeyIndex)
    {
        Debug.Log("OnBattleMoveComplete");
    }

    private IEnumerator HitEnemy5()
    {
        battleModel.MinusCrtEnemyHp(99);
        Unit enemy = battleModel.GetCrtEnemy();
        bool isKilled = enemy == null;
        //bool isWeakState = !isKilled && enemy.Hp < enemy.Config.UnitHp;
        bool isWeakState = !isKilled && enemy.Hp < enemy.HpMax;
        enemyHitBeh.HitCompleteEvent += OnHitCompleteEvent;
        for (int i = 1, n = 5; i <= n; i++)
        {
            enemyHitBeh.PlayHit(battleBeh.GetCurrentEnemyPoint(), i, n, 9, isWeakState, isKilled);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnHitCompleteEvent(bool obj)
    {
        enemyHitBeh.HitCompleteEvent -= OnHitCompleteEvent;
        Debug.LogFormat("OnHitCompleteEvent");
    }


    private void First()
    {
        StageConfig config = GameController.Instance.Model.BattleModel.CrtStageConfig;
        battleBeh.First(config);
    }
}
