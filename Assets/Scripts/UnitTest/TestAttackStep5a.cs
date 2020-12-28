using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Helper;
using Com4Love.Qmax.TileBehaviour;
using System.Collections.Generic;
using System;
using Com4Love.Qmax;

public class TestAttackStep5a : MonoBehaviour
{
    public BoardBehaviour BoardBeh;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(StartTest());
    }


    private IEnumerator StartTest()
    {
        yield return new WaitForSeconds(5);

        //BattleAttackProcessHelper helper = new BattleAttackProcessHelper(BoardBeh);
        //List<BaseTileBehaviour> list = new List<BaseTileBehaviour>();

        //int count = 3;
        //for (int i = 0, n = BoardBeh.eleViews.GetLength(0); i < n; i++)
        //{
        //    for (int j = 0, m = BoardBeh.eleViews.GetLength(1); j < m; j++)
        //    {
        //        if (BoardBeh.eleViews[i, j] == null)
        //            continue;
        //        list.Add(BoardBeh.eleViews[i, j].GetComponent<BaseTileBehaviour>());
        //        if (--count <= 0)
        //            break;
        //    }
        //}
        //Q.Assert(list[0] != null && list[0].Config != null);
        //helper.__TestStep5a(list, list[0].Config.ColorType, delegate() { });
    }

}
