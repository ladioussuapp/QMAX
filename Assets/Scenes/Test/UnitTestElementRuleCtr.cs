using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTestElementRuleCtr : MonoBehaviour
{
    private const int SEPR_H1 = 43;
    private const int SEPR_H2 = 44;
    private const int SEPR_H3 = 45;
    private const int SEPR_H4 = 46;
    private const int SEPR_H5 = 47;
    private const int SEPR_V1 = 83;
    private const int SEPR_V2 = 84;
    private const int SEPR_V3 = 85;
    private const int SEPR_V4 = 86;
    private const int SEPR_V5 = 87;
    private const int SEPR_H_MAX = 12;
    private const int SEPR_V_MAX = 62;

    private const int BOX_MAX = 224;
    private const int BOX_1 = 225;
    private const int BOX_2 = 226;
    private const int BOX_3 = 227;


    private const int BOMB_H_YEL_1 = 551;
    private const int BOMB_H_YEL_2 = 552;
    private const int BOMB_H_YEL_3 = 553;
    private const int BOMB_H_YEL_4 = 554;
    private const int BOMB_H_YEL_5 = 555;

    private const int BOMB_V_YEL_1 = 501;
    private const int BOMB_V_YEL_2 = 502;
    private const int BOMB_V_YEL_3 = 503;
    private const int BOMB_V_YEL_4 = 504;
    private const int BOMB_V_YEL_5 = 505;

    private const int GEM = 222;

    private const int COVER_1 = 214;



    ElementRuleCtr target;
    Dictionary<int, TileObjectConfig> tileObjectConfigs;




    IEnumerator Start()
    {
        yield return 0;

#if UNITY_EDITOR
        LoadObjConfig();
        target = new ElementRuleCtr(null, tileObjectConfigs, 7, 7);
        ElementRuleCtr.UnitTestMode = true;

        yield return 0;
        TestPosition();
        yield return 0;
        TestVBomb();
        yield return 0;
        TestHBomb();
        yield return 0;
        TestBombCascade();
        yield return 0;
        TestBombCascade2();
        yield return 0;
        TestBombCascade3();
        yield return 0;

        TestCalcElimPathByLinkPath1();
        yield return 0;

        TestHurtTile();
        yield return 0;
        TestCalcTileHurtByElim();
        yield return 0;
        TestCalcTileHurtByAffect();
        yield return 0;
        TestEliminate1();
        yield return 0;
        TestEliminate4();
        TestEliminate5();
        yield return 0;
#endif
        yield return 0;
    }


    #region 测试计算消除范围

    private void TestPosition()
    {
        Position p1 = new Position(1, 2);
        Position p2 = new Position(1, 2);
        Position p3 = p1;
        Position p4 = new Position(0, 2);
        List<Position> list = new List<Position>();
        list.Add(p1);
        Q.Assert(p1 == p2);
        Q.Assert(p1 == p3);
        Q.Assert(list.Contains(p2));
        Q.Assert(list.Contains(p3));
        Q.Assert(!list.Contains(p4));
        Q.Assert(p1 != p4);
    }


    private void TestCalcElimPathByLinkPath1()
    {
        Debug.Log("-------------------- TestCalcElimPathByLinkPath1");
        InitTarget();
        //5级间隔物H
        SetTileDataAt(target.__TestSeprH, 0, 5, SEPR_H5);
        //3级间隔物V
        SetTileDataAt(target.__TestSeprV, 1, 4, SEPR_V3);
        //3级木箱，破坏后会获得橘子
        SetTileDataAt(target.__TestElements, 1, 5, 0);
        SetTileDataAt(target.__TestObstacles, 1, 5, BOX_3);
        //紅色水平炸彈2级，消除等级2
        SetTileDataAt(target.__TestElements, 1, 3, 562);

        List<Position> path = new List<Position>();
        path.Add(new Position(1, 3));
        List<int> orders = new List<int>();
        for (int i = 0, n = path.Count; i < n; i++)
        {
            orders.Add(i);
        }
        List<Position> retPath = new List<Position>();
        List<int> retOrders = new List<int>();
        target.CalcElimPathByLinkPath(path, out retPath, out retOrders);

        Q.Assert(retPath.Contains(new Position(1, 0)));
        Q.Assert(retPath.Contains(new Position(1, 1)));
        Q.Assert(retPath.Contains(new Position(1, 2)));
        Q.Assert(retPath.Contains(new Position(1, 3)));
        Q.Assert(retPath.Contains(new Position(1, 4)));
        Q.Assert(retPath.Contains(new Position(1, 5)));

        //Q.Assert(retOrders[retPath.IndexOf(new Position(1, 0))] == 3);
        //Q.Assert(retOrders[retPath.IndexOf(new Position(1, 1))] == 2);
        //Q.Assert(retOrders[retPath.IndexOf(new Position(1, 2))] == 1);
        //Q.Assert(retOrders[retPath.IndexOf(new Position(1, 3))] == 0);
        //Q.Assert(retOrders[retPath.IndexOf(new Position(1, 4))] == 1);
        //Q.Assert(retOrders[retPath.IndexOf(new Position(1, 5))] == 2);

        //Q.Assert(retHurts.Count == 7);
        //for (int i = 0, n = retHurts.Count; i < n; i++)
        //{
        //    if (retPath[i] == new Position(1, 5))//炸弹+障碍物
        //        Q.Assert(retHurts[i] == 2);
        //    else if (retPath[i] == new Position(1, 3))//只有炸弹的威力
        //        Q.Assert(retHurts[i] == 2);
        //    else//普通消除物 + 炸弹威力
        //        Q.Assert(retHurts[i] == 3, retPath[i].ToString());
        //}
    }


    private void TestVBomb()
    {
        InitTarget();
        //黄色垂直炸弹2级，半径5，威力3
        SetTileDataAt(target.__TestElements, 2, 2, BOMB_V_YEL_2);


        List<Position> path = new List<Position>();
        path.Add(new Position(2, 2));
        List<int> orders = new List<int>();
        for (int i = 0, n = path.Count; i < n; i++)
        {
            orders.Add(i);
        }
        List<Position> retPath = new List<Position>();
        List<int> retOrders = new List<int>();
        target.__TestRecursiveCalcElimPath(path, orders, retPath, retOrders);

        Q.Assert(retPath.Contains(new Position(0, 2)));
        Q.Assert(retPath.Contains(new Position(1, 2)));
        Q.Assert(retPath.Contains(new Position(2, 2)));
        Q.Assert(retPath.Contains(new Position(3, 2)));
        Q.Assert(retPath.Contains(new Position(5, 2)));

        Q.Assert(retOrders[retPath.IndexOf(new Position(0, 2))] == 2);
        Q.Assert(retOrders[retPath.IndexOf(new Position(1, 2))] == 1);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 2))] == 0);
        Q.Assert(retOrders[retPath.IndexOf(new Position(3, 2))] == 1);
        Q.Assert(retOrders[retPath.IndexOf(new Position(4, 2))] == 2);
        Q.Assert(retOrders[retPath.IndexOf(new Position(5, 2))] == 3);
        Q.Assert(retOrders[retPath.IndexOf(new Position(6, 2))] == 4);
    }


    private void TestHBomb()
    {
        InitTarget();
        //黄色横向炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 1, 1, BOMB_H_YEL_1);


        List<Position> path = new List<Position>();
        path.Add(new Position(1, 1));
        List<int> orders = new List<int>();
        for (int i = 0, n = path.Count; i < n; i++)
        {
            orders.Add(i);
        }
        List<Position> retPath = new List<Position>();
        List<int> retOrders = new List<int>();
        target.__TestRecursiveCalcElimPath(path, orders, retPath, retOrders);

        Q.Assert(retPath.Count == 6, retPath.Count.ToString());
        Q.Assert(retPath.Contains(new Position(1, 0)));
        Q.Assert(retPath.Contains(new Position(1, 1)));
        Q.Assert(retPath.Contains(new Position(1, 2)));
        Q.Assert(retPath.Contains(new Position(1, 3)));
        Q.Assert(retPath.Contains(new Position(1, 4)));
        Q.Assert(retPath.Contains(new Position(1, 5)));

        Q.Assert(retOrders[retPath.IndexOf(new Position(1, 0))] == 1);
        Q.Assert(retOrders[retPath.IndexOf(new Position(1, 1))] == 0);
        Q.Assert(retOrders[retPath.IndexOf(new Position(1, 2))] == 1);
        Q.Assert(retOrders[retPath.IndexOf(new Position(1, 3))] == 2);
        Q.Assert(retOrders[retPath.IndexOf(new Position(1, 4))] == 3);
        Q.Assert(retOrders[retPath.IndexOf(new Position(1, 5))] == 4);
    }


    /// <summary>
    /// 炸弹级联效果
    /// </summary>
    private void TestBombCascade()
    {
        InitTarget();
        //黄色水平炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 2, 2, BOMB_H_YEL_1);
        //黄色垂直炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 4, 2, BOMB_V_YEL_1);


        List<Position> path = new List<Position>();
        path.Add(new Position(4, 2));
        path.Add(new Position(2, 2));
        List<int> orders = new List<int>();
        for (int i = 0, n = path.Count; i < n; i++)
        {
            orders.Add(i);
        }
        List<Position> retPath = new List<Position>();
        List<int> retOrders = new List<int>();
        target.__TestRecursiveCalcElimPath(path, orders, retPath, retOrders);

        Q.Assert(retPath.Count == 13, retPath.Count.ToString());
        Q.Assert(retPath.Contains(new Position(2, 0)));
        Q.Assert(retPath.Contains(new Position(2, 1)));
        Q.Assert(retPath.Contains(new Position(2, 2)));
        Q.Assert(retPath.Contains(new Position(2, 3)));
        Q.Assert(retPath.Contains(new Position(2, 4)));
        Q.Assert(retPath.Contains(new Position(2, 5)));
        Q.Assert(retPath.Contains(new Position(2, 6)));
        Q.Assert(retPath.Contains(new Position(0, 2)));
        Q.Assert(retPath.Contains(new Position(1, 2)));
        //重复
        //Q.Assert(retPath.Contains(new Position(2, 2)));
        Q.Assert(retPath.Contains(new Position(3, 2)));
        Q.Assert(retPath.Contains(new Position(4, 2)));
        Q.Assert(retPath.Contains(new Position(5, 2)));
        Q.Assert(retPath.Contains(new Position(6, 2)));

        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 2))] == 1);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 1))] == 2);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 0))] == 3);
        Q.Assert(retOrders[retPath.IndexOf(new Position(4, 2))] == 0);
        Q.Assert(retOrders[retPath.IndexOf(new Position(6, 2))] == 2);
    }


    /// <summary>
    /// 炸弹级联效果
    /// </summary>
    private void TestBombCascade2()
    {
        InitTarget();
        //黄色水平炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 2, 2, BOMB_H_YEL_1);
        //黄色垂直炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 4, 2, BOMB_V_YEL_1);


        List<Position> path = new List<Position>();
        path.Add(new Position(4, 2));
        List<int> orders = new List<int>();
        for (int i = 0, n = path.Count; i < n; i++)
        {
            orders.Add(i);
        }
        List<Position> retPath = new List<Position>();
        List<int> retOrders = new List<int>();
        target.__TestRecursiveCalcElimPath(path, orders, retPath, retOrders);

        Q.Assert(retPath.Count == 13, retPath.Count.ToString());
        Q.Assert(retPath.Contains(new Position(2, 0)));
        Q.Assert(retPath.Contains(new Position(2, 1)));
        Q.Assert(retPath.Contains(new Position(2, 2)));
        Q.Assert(retPath.Contains(new Position(2, 3)));
        Q.Assert(retPath.Contains(new Position(2, 4)));
        Q.Assert(retPath.Contains(new Position(2, 5)));
        Q.Assert(retPath.Contains(new Position(2, 6)));
        Q.Assert(retPath.Contains(new Position(0, 2)));
        Q.Assert(retPath.Contains(new Position(1, 2)));
        //重复
        //Q.Assert(retPath.Contains(new Position(2, 2)));
        Q.Assert(retPath.Contains(new Position(3, 2)));
        Q.Assert(retPath.Contains(new Position(4, 2)));
        Q.Assert(retPath.Contains(new Position(5, 2)));
        Q.Assert(retPath.Contains(new Position(6, 2)));

        Q.Assert(retOrders[retPath.IndexOf(new Position(4, 2))] == 0);
        Q.Assert(retOrders[retPath.IndexOf(new Position(6, 2))] == 2);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 2))] == 2);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 1))] == 3);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 0))] == 4);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 6))] == 6);
    }


    /// <summary>
    /// 炸弹级联效果
    /// 循环
    /// </summary>
    private void TestBombCascade3()
    {
        InitTarget();

        //黄色水平炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 4, 1, BOMB_H_YEL_1);
        //黄色垂直炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 4, 3, BOMB_V_YEL_1);
        //黄色水平炸弹1级，半径4，威力1  
        SetTileDataAt(target.__TestElements, 2, 3, BOMB_H_YEL_1);
        //黄色垂直炸弹1级，半径4，威力1
        SetTileDataAt(target.__TestElements, 2, 1, BOMB_V_YEL_1);


        List<Position> path = new List<Position>();
        path.Add(new Position(4, 1));
        //path.Add(new Position(4, 3));
        //path.Add(new Position(2, 3));
        //path.Add(new Position(2, 1));
        List<int> orders = new List<int>();
        for (int i = 0, n = path.Count; i < n; i++)
        {
            orders.Add(i);
        }
        List<Position> retPath = new List<Position>();
        List<int> retOrders = new List<int>();
        target.__TestRecursiveCalcElimPath(path, orders, retPath, retOrders);

        Q.Assert(retPath.Count == 23, retPath.Count.ToString());
        Q.Assert(retPath.Contains(new Position(0, 1)));
        Q.Assert(retPath.Contains(new Position(1, 1)));
        Q.Assert(retPath.Contains(new Position(2, 1)));
        Q.Assert(retPath.Contains(new Position(3, 1)));
        Q.Assert(retPath.Contains(new Position(4, 1)));
        Q.Assert(retPath.Contains(new Position(5, 1)));
        Q.Assert(retPath.Contains(new Position(6, 1)));

        Q.Assert(retPath.Contains(new Position(0, 3)));
        Q.Assert(retPath.Contains(new Position(1, 3)));
        Q.Assert(retPath.Contains(new Position(2, 3)));
        Q.Assert(retPath.Contains(new Position(3, 3)));
        Q.Assert(retPath.Contains(new Position(4, 3)));
        Q.Assert(retPath.Contains(new Position(5, 3)));
        Q.Assert(retPath.Contains(new Position(6, 3)));

        Q.Assert(retPath.Contains(new Position(2, 0)));
        Q.Assert(retPath.Contains(new Position(2, 1)));
        Q.Assert(retPath.Contains(new Position(2, 2)));
        Q.Assert(retPath.Contains(new Position(2, 3)));
        Q.Assert(retPath.Contains(new Position(2, 4)));
        Q.Assert(retPath.Contains(new Position(2, 5)));
        Q.Assert(retPath.Contains(new Position(2, 6)));

        Q.Assert(retPath.Contains(new Position(4, 0)));
        Q.Assert(retPath.Contains(new Position(4, 1)));
        Q.Assert(retPath.Contains(new Position(4, 2)));
        Q.Assert(retPath.Contains(new Position(4, 3)));
        Q.Assert(retPath.Contains(new Position(4, 4)));
        Q.Assert(retPath.Contains(new Position(4, 5)));

        Q.Assert(retOrders[retPath.IndexOf(new Position(4, 1))] == 0, retOrders[retPath.IndexOf(new Position(4, 1))].ToString());
        Q.Assert(retOrders[retPath.IndexOf(new Position(4, 3))] == 2);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 3))] == 4);
        Q.Assert(retOrders[retPath.IndexOf(new Position(2, 1))] == 6);
    }

    #endregion

    #region 测试消除逻辑

    #region TestEliminate
    private void TestEliminate1()
    {
        Debug.Log("TestEliminate ---------------------------------------");
        List<Position> linkPath = new List<Position>();
        List<TileObject> retOriDatas = null;
        List<TileObject> retNewDatas = null;
        List<int> retOrders = null;
        List<ItemQtt[]> retElimRewards = null;

        Debug.Log("---------- TestCase1: 基础测试案例");
        InitTarget();
        linkPath.Clear();
        linkPath.Add(new Position(0, 0));
        linkPath.Add(new Position(0, 1));
        linkPath.Add(new Position(0, 2));
        target.__TestEliminate(
            linkPath,
            out retOriDatas,
            out retNewDatas,
            out retOrders,
            out retElimRewards);
        Q.Assert(retOrders.Count == 3, retOrders.Count.ToString());
        Q.Assert(retElimRewards.Count == 3);
        for (int i = 0, n = retOrders.Count; i < n; i++)
        {
            Q.Assert(retOrders[i] == i);
            Q.Assert(retElimRewards[i] == null);
            Q.Assert(retNewDatas[i] == null);
        }


        Debug.Log("---------- TestCase2: 消钻石");
        InitTarget();
        linkPath.Clear();
        SetTileDataAt(target.__TestObstacles, 1, 1, 0);
        SetTileDataAt(target.__TestObstacles, 1, 1, GEM);
        linkPath.Add(new Position(0, 0));
        linkPath.Add(new Position(0, 1));
        linkPath.Add(new Position(0, 2));
        target.__TestEliminate(
            linkPath,
            out retOriDatas,
            out retNewDatas,
            out retOrders,
            out retElimRewards);
        Q.Assert(retOrders.Count == 4, retOrders.Count.ToString());
        Q.Assert(retElimRewards.Count == 4);
        for (int i = 0, n = retOrders.Count; i < n; i++)
        {
            if (retOriDatas[i].ConfigID == GEM)
            {
                Q.Assert(retNewDatas[i] == null);
                Q.Assert(retOrders[i] == 2, retOrders[i].ToString());
                Q.Assert(retElimRewards[i].Length == 1);
                Q.Assert(retElimRewards[i][0].type == RewardType.Gem);
                Q.Assert(retElimRewards[i][0].Qtt == 1);
            }
        }


        Debug.Log("---------- TestCase3: 围绕木箱消除");
        InitTarget();
        linkPath.Clear();
        //3级木箱，破坏后会获得橘子
        SetTileDataAt(target.__TestObstacles, 1, 5, BOX_3);
        //围绕木箱消除，对木箱造成伤害3
        linkPath.Add(new Position(2, 4));
        linkPath.Add(new Position(1, 4));
        linkPath.Add(new Position(0, 4));
        linkPath.Add(new Position(0, 5));
        linkPath.Add(new Position(0, 6));
        linkPath.Add(new Position(1, 6));
        target.__TestEliminate(
            linkPath,
            out retOriDatas,
            out retNewDatas,
            out retOrders,
            out retElimRewards);
        Q.Assert(retOrders.Count == 7, retOrders.Count.ToString());
        Q.Assert(retElimRewards.Count == 7);
        for (int i = 0, n = retOrders.Count; i < n; i++)
        {
            if (retOriDatas[i].ConfigID == BOX_3)
            {
                Q.Assert(retNewDatas[i].ConfigID == BOX_MAX, retNewDatas[i].ConfigID.ToString());
                Q.Assert(retOrders[i] == 2, retOrders[i].ToString());
                Q.Assert(retElimRewards[i].Length == 1, retElimRewards[i].Length.ToString());
                Q.Assert(retElimRewards[i][0].type == RewardType.UpgradeA);
                Q.Assert(retElimRewards[i][0].Qtt == 15);
            }
        }
    }


    private void TestEliminate4()
    {
        Debug.Log("---------- TestEliminate4: 围绕间隔物消除");
        List<Position> linkPath = new List<Position>();
        List<TileObject> retOriDatas = null;
        List<TileObject> retNewDatas = null;
        List<int> retOrders = null;
        List<ItemQtt[]> retElimRewards = null;

        InitTarget();
        //5级间隔物H
        SetTileDataAt(target.__TestSeprH, 0, 5, SEPR_H5);
        //3级间隔物V
        SetTileDataAt(target.__TestSeprV, 1, 4, SEPR_V3);
        linkPath.Add(new Position(2, 4));
        linkPath.Add(new Position(1, 4));
        linkPath.Add(new Position(0, 4));
        linkPath.Add(new Position(0, 5));
        linkPath.Add(new Position(0, 6));
        linkPath.Add(new Position(1, 6));
        linkPath.Add(new Position(1, 5));
        target.__TestEliminate(
            linkPath,
            out retOriDatas,
            out retNewDatas,
            out retOrders,
            out retElimRewards);
        Q.Assert(retOrders.Count == 9, retOrders.Count.ToString());
        Q.Assert(retElimRewards.Count == 9);
        for (int i = 0, n = retOrders.Count; i < n; i++)
        {
            if (retOriDatas[i].ConfigID == SEPR_H5)
            {
                Q.Assert(retNewDatas[i].ConfigID == SEPR_H3, retNewDatas[i].ConfigID.ToString());
                Q.Assert(retOrders[i] == 4, retOrders[i].ToString());
                Q.Assert(retElimRewards[i] == null);
            }
            else if (retOriDatas[i].ConfigID == SEPR_V3)
            {
                Q.Assert(retNewDatas[i].ConfigID == SEPR_V1, retNewDatas[i].ConfigID.ToString());
                Q.Assert(retOrders[i] == 2, retOrders[i].ToString());
                Q.Assert(retElimRewards[i] == null);
            }
        }
    }


    private void TestEliminate5()
    {
        Debug.Log("---------- TestEliminate5: 炸弹消除");
        List<Position> linkPath = new List<Position>();
        List<TileObject> retOriDatas = null;
        List<TileObject> retNewDatas = null;
        List<int> retOrders = null;
        List<ItemQtt[]> retElimRewards = null;

        InitTarget();
        //5级间隔物H，不会被伤害，因为(1,5)位置没有消除物被破坏
        SetTileDataAt(target.__TestSeprH, 0, 5, SEPR_H5);
        //3级间隔物V
        SetTileDataAt(target.__TestSeprV, 1, 4, SEPR_V3);
        //3级木箱，破坏后会获得橘子
        SetTileDataAt(target.__TestElements, 1, 5, 0);
        SetTileDataAt(target.__TestObstacles, 1, 5, BOX_3);
        //紅色水平炸彈2级，半径4，威力2
        SetTileDataAt(target.__TestElements, 1, 3, BOMB_H_YEL_2);
        linkPath.Add(new Position(1, 3));
        target.__TestEliminate(
            linkPath,
            out retOriDatas,
            out retNewDatas,
            out retOrders,
            out retElimRewards);
        Q.Assert(retOrders.Count == 8, retOrders.Count.ToString());
        Q.Assert(retElimRewards.Count == 8);
        for (int i = 0, n = retOrders.Count; i < n; i++)
        {
            Q.Log("elim p={0}, id={1}", new Position(retOriDatas[i].Row, retOriDatas[i].Col), retOriDatas[i].ConfigID);
            if (retOriDatas[i].ConfigID == SEPR_V3)
            {
                //收到伤害6
                Q.Assert(retNewDatas[i].ConfigID == SEPR_V2);
                Q.Assert(retOrders[i] == 2, retOrders[i].ToString());
                Q.Assert(retElimRewards[i] == null);
            }
            else if (retOriDatas[i].ConfigID == BOX_3)
            {
                //收到伤害5
                Q.Assert(retNewDatas[i].ConfigID == BOX_MAX, retNewDatas[i].ConfigID.ToString());
                Q.Assert(retOrders[i] == 2, retOrders[i].ToString());
                Q.Assert(retElimRewards[i].Length == 1);
            }
        }
    }

    #endregion

    private void TestHurtTile()
    {
        //---------- TestCase1 ----------
        //期待结果：玻璃被消到只剩1层
        InitTarget();

        TileObject newTileData;

        ItemQtt[] rewards = null;
        //1级蜘蛛网
        target.__TestHurtTile(GenTileData(0, 0, COVER_1), 2, out newTileData, out rewards);
        Q.Assert(newTileData == null);
        Q.Assert(rewards == null);

        //2级玻璃
        target.__TestHurtTile(GenTileData(0, 0, 265), 1, out newTileData, out rewards);
        Q.Assert(newTileData != null && newTileData.Config.Level == 1);
        Q.Assert(rewards == null);

        //-1级木箱
        target.__TestHurtTile(GenTileData(0, 0, BOX_MAX), 99, out newTileData, out rewards);
        Q.Assert(newTileData != null && newTileData.ConfigID == BOX_MAX);
        Q.Assert(rewards == null);

        //2级木箱
        target.__TestHurtTile(GenTileData(0, 0, BOX_2), 99, out newTileData, out rewards);
        Q.Assert(newTileData != null && newTileData.ConfigID == BOX_MAX);
        Q.Assert(rewards.Length == 1);

        //没有受到破坏
        //1级蜘蛛网
        target.__TestHurtTile(GenTileData(0, 0, COVER_1), 0, out newTileData, out rewards);
        Q.Assert(newTileData != null && newTileData.Config.ID == COVER_1);
        Q.Assert(rewards == null);

        //2级玻璃
        target.__TestHurtTile(GenTileData(0, 0, 265), 1, out newTileData, out rewards);
        Q.Assert(newTileData != null && newTileData.Config.Level == 1, "lv={0}", newTileData.Config.Level);
        Q.Assert(rewards == null);

        //普通消除物
        target.__TestHurtTile(GenTileData(0, 0, 1), 2, out newTileData, out rewards);
        Q.Assert(newTileData == null);
        Q.Assert(rewards == null);
    }

    private void TestCalcTileHurtByElim()
    {
        Debug.Log("---------- TestCase1: 玻璃被消到只剩1层");
        InitTarget();
        //1级蜘蛛网
        SetTileDataAt(target.__TestCovers, 1, 2, COVER_1);
        //2级玻璃
        SetTileDataAt(target.__TestBottoms, 1, 2, 265);
        int hurt = 2;
        Dictionary<TileType, int> ret = null;

        ret = target.__TestCalcTileHurtByElim(new Position(1, 2), hurt);
        Q.Assert(ret.Count == 3);
        foreach (var pair in ret)
        {
            Q.Assert(pair.Value == 1);
        }


        Debug.Log("---------- TestCase2: 底层物不受障碍物破坏影响，玻璃被消到只剩1层");
        InitTarget();
        //元素位置留空
        SetTileDataAt(target.__TestElements, 1, 2, 0);
        //1级障碍物
        SetTileDataAt(target.__TestObstacles, 1, 2, 203);
        //1级蜘蛛网
        SetTileDataAt(target.__TestCovers, 1, 2, COVER_1);
        //2级玻璃
        SetTileDataAt(target.__TestBottoms, 1, 2, 265);
        hurt = 3;

        ret = target.__TestCalcTileHurtByElim(new Position(1, 2), hurt);
        Q.Assert(ret.Count == 3);
        foreach (var pair in ret)
        {
            Q.Assert(pair.Value == 1);
        }

        Debug.Log("---------- TestCase3: 刚好全部地形物都被破坏完");
        InitTarget();
        //元素位置留空
        SetTileDataAt(target.__TestElements, 1, 2, 0);
        //1级障碍物
        SetTileDataAt(target.__TestObstacles, 1, 2, 203);
        //1级蜘蛛网
        SetTileDataAt(target.__TestCovers, 1, 2, COVER_1);
        //2级玻璃
        SetTileDataAt(target.__TestBottoms, 1, 2, 265);
        hurt = 4;

        ret = target.__TestCalcTileHurtByElim(new Position(1, 2), hurt);
        Q.Assert(ret.Count == 3);
        foreach (var pair in ret)
        {
            if (pair.Key == TileType.Bottom)
                Q.Assert(pair.Value == 2);
            else
                Q.Assert(pair.Value == 1);
        }


        Debug.Log("---------- TestCase4: 障碍物变成一个永久残留，level==-1会格挡掉所有伤害");
        InitTarget();
        //改位置的消除物清空
        SetTileDataAt(target.__TestElements, 1, 2, 0);
        //3级木箱
        SetTileDataAt(target.__TestObstacles, 1, 2, 231);
        //1级蜘蛛网
        SetTileDataAt(target.__TestCovers, 1, 2, COVER_1);
        //2级玻璃
        SetTileDataAt(target.__TestBottoms, 1, 2, 265);
        hurt = 99;
        ret = target.__TestCalcTileHurtByElim(new Position(1, 2), hurt);
        Q.Assert(ret.Count == 2, ret.Count.ToString());
        foreach (var pair in ret)
        {
            if (pair.Key == TileType.Cover)
                Q.Assert(pair.Value == 1);
            else if (pair.Key == TileType.Obstacle)
                Q.Assert(pair.Value == 3);
            else
                Q.Assert(false, pair.Key.ToString());
        }


        Debug.Log("---------- TestCase5: 消除物一个-1级的障碍物，不造成任何伤害");
        InitTarget();
        //改位置的消除物清空
        SetTileDataAt(target.__TestElements, 1, 2, 0);
        //-1级木箱
        SetTileDataAt(target.__TestObstacles, 1, 2, BOX_MAX);
        hurt = 99;
        ret = target.__TestCalcTileHurtByElim(new Position(1, 2), hurt);
        Q.Assert(ret.Count == 0, ret.Count.ToString());
    }

    #endregion

    #region 测试影响逻辑
    /// <summary>
    /// 测试影响逻辑
    /// </summary>
    private void TestCalcTileHurtByAffect()
    {
        Debug.Log("---------- TestCase1: 没有任何改变");
        InitTarget();
        int hurt = 99;
        Dictionary<TileType, Dictionary<Position, int>> ret = null;
        ret = target.__TestCalcTileHurtByAffect(new Position(3, 1), hurt);
        Q.Assert(ret.Count == 0);


        Debug.Log("---------- TestCase2: 影响木条");
        InitTarget();
        hurt = 2;
        //横向木条2级
        SetTileDataAt(target.__TestSeprH, 2, 1, SEPR_H2);
        ret = target.__TestCalcTileHurtByAffect(new Position(3, 1), hurt);
        Q.Assert(ret.Count == 1);
        Q.Assert(ret[TileType.SeperatorH][new Position(2, 1)] == 2);


        Debug.Log("---------- TestCase3: 覆盖物影响层次高于障碍物");
        InitTarget();
        hurt = 4;
        //蜘蛛网1级
        SetTileDataAt(target.__TestCovers, 2, 1, COVER_1);
        //横向木条2级
        SetTileDataAt(target.__TestSeprH, 2, 1, SEPR_H2);
        //障碍物1级
        SetTileDataAt(target.__TestObstacles, 2, 1, BOX_1);
        ret = target.__TestCalcTileHurtByAffect(new Position(3, 1), hurt);
        Q.Assert(ret.Count == 3);
        Q.Assert(ret[TileType.Cover][new Position(2, 1)] == 1);
        Q.Assert(ret[TileType.SeperatorH][new Position(2, 1)] == 2);


        Debug.Log("---------- TestCase4: 影响障碍物、横向木条");
        InitTarget();
        hurt = 4;
        //横向木条2级
        SetTileDataAt(target.__TestSeprH, 2, 1, SEPR_H2);
        //障碍物1级，变为不可消除的障碍物
        SetTileDataAt(target.__TestObstacles, 2, 1, BOX_1);
        ret = target.__TestCalcTileHurtByAffect(new Position(3, 1), hurt);
        Q.Assert(ret.Count == 2);
        Q.Assert(ret[TileType.Obstacle][new Position(2, 1)] == 1);
        Q.Assert(ret[TileType.SeperatorH][new Position(2, 1)] == 2);



        Debug.Log("---------- TestCase5: 边界测试");
        InitTarget();
        hurt = 2;
        //横向木条2级
        SetTileDataAt(target.__TestSeprH, 6, 0, SEPR_H2);
        ret = target.__TestCalcTileHurtByAffect(new Position(6, 0), hurt);
        Q.Assert(ret.Count == 1);
        Q.Assert(ret[TileType.SeperatorH][new Position(6, 0)] == 2);


        Debug.Log("---------- TestCase6: 综合测试");
        InitTarget();
        hurt = 4;
        //横向木条2级
        SetTileDataAt(target.__TestSeprH, 2, 1, SEPR_H2);
        //横向木条2级
        SetTileDataAt(target.__TestSeprH, 3, 1, SEPR_H2);
        //纵向木条MAX
        SetTileDataAt(target.__TestSeprV, 3, 0, SEPR_V_MAX);
        //纵向木条3级
        SetTileDataAt(target.__TestSeprV, 3, 1, SEPR_V3);
        //障碍物1级，变为不可消除的障碍物
        SetTileDataAt(target.__TestObstacles, 2, 1, BOX_1);
        SetTileDataAt(target.__TestElements, 2, 1, 0);
        //障碍物1级，变为不可消除的障碍物
        SetTileDataAt(target.__TestObstacles, 3, 0, BOX_1);
        SetTileDataAt(target.__TestElements, 3, 0, 0);
        //障碍物1级，变为不可消除的障碍物
        SetTileDataAt(target.__TestObstacles, 4, 1, BOX_1);
        SetTileDataAt(target.__TestElements, 4, 1, 0);
        //蜘蛛网1级
        SetTileDataAt(target.__TestCovers, 4, 1, COVER_1);
        ret = target.__TestCalcTileHurtByAffect(new Position(3, 1), hurt);
        //各个方向的影响地形物数量: U:2, D:2, L:0, R:1
        Q.Assert(ret.Count == 4, ret.Count.ToString());
        //U
        Q.Assert(ret[TileType.SeperatorH][new Position(2, 1)] == 2);
        Q.Assert(ret[TileType.Obstacle][new Position(2, 1)] == 1);
        //D
        Q.Assert(ret[TileType.SeperatorH][new Position(3, 1)] == 2);
        Q.Assert(ret[TileType.Cover][new Position(4, 1)] == 1);
        //R
        Q.Assert(ret[TileType.SeperatorV][new Position(3, 1)] == 3);
    }

    #endregion


    #region 辅助的函数

    /// <summary>
    /// 加载地形物配置
    /// </summary>
    private void LoadObjConfig()
    {
#if UNITY_EDITOR
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/ExternalRes/Config/ObjectConfig.xml");
        tileObjectConfigs = new Dictionary<int, TileObjectConfig>();
        XMLInStream stream = new XMLInStream(textAsset.text);
        stream.List("item", delegate(XMLInStream itemStream)
        {
            TileObjectConfig cfg = new TileObjectConfig(itemStream);
            tileObjectConfigs.Add(cfg.ID, cfg);
        });
#endif
    }


    /// <summary>
    /// 初始化棋盘，全部填充为基础消除物
    /// </summary>
    private void InitTarget()
    {
        TileObject[,] covers = new TileObject[7, 7];
        TileObject[,] seprHs = new TileObject[7, 7];
        TileObject[,] seprVs = new TileObject[7, 7];
        TileObject[,] elements = new TileObject[7, 7];
        TileObject[,] obstacles = new TileObject[7, 7];
        TileObject[,] bottoms = new TileObject[7, 7];
        target.__TestElements = elements;
        target.__TestCovers = covers;
        target.__TestBottoms = bottoms;
        target.__TestObstacles = obstacles;
        target.__TestSeprH = seprHs;
        target.__TestSeprV = seprVs;

        //全部填充紫色基础消除物
        for (int r = 0, n = elements.GetLength(0); r < n; r++)
        {
            for (int c = 0, m = elements.GetLength(1); c < m; c++)
            {
                elements[r, c] = GenTileData(r, c, 1);
            }
        }
    }

    private void SetTileDataAt(TileObject[,] map, int r, int c, int objId)
    {
        map[r, c] =
            objId == 0 ?
            null :
            new TileObject(r, c, tileObjectConfigs[objId]);
    }

    private TileObject GenTileData(int r, int c, int objId)
    {
        return new TileObject(r, c, tileObjectConfigs[objId]);
    }
    #endregion
}
