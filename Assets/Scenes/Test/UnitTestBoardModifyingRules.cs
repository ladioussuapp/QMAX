using Com4Love.Qmax;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTestBoardModifyingRules : MonoBehaviour
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

    private const int BOMB_H_GRE_1 = 571;
    private const int BOMB_V_GRE_1 = 521;

    private const int BOMB_H_RED_1 = 561;
    private const int BOMB_V_RED_1 = 511;
    private const int BOMB_A_RED_1 = 661;

    private const int BOMB_H_BLU_1 = 581;
    private const int BOMB_V_BLU_1 = 531;


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


    private const int CONVERTER_YEL_1 = 602;
    private const int CONVERTER_RED_1 = 611;
    private const int CONVERTER_GRE_1 = 621;

    private const int GEM = 222;

    private const int COVER_1 = 214;


    BattleModelModifyAgent batModel = null;
    ElementRuleCtr elementRuleCtr;
    BoardModifyingRules target;
    Dictionary<int, TileObjectConfig> tileObjectConfigs;
    ViewEventSystem viewEvtSys;

    IEnumerator Start()
    {
#if UNITY_EDITOR
        LoadObjConfig();

        viewEvtSys = new ViewEventSystem();
        batModel = new BattleModelModifyAgent(tileObjectConfigs, null, null, null, null);
        elementRuleCtr = new ElementRuleCtr(batModel, tileObjectConfigs, 7, 7);
        target = new BoardModifyingRules(
            batModel, null,
            elementRuleCtr,
            viewEvtSys,
            null,
            tileObjectConfigs,
            7,
            7
        );


        TestBombMovingRule();
        yield return 0;
        TestCoverterRule();
        yield return 0;
        TestBombCascadeRule1();
        yield return 0;
        TestBombCascadeRule2();
        yield return 0;
        TestAllRules();
        yield return 0;
        TestAllRules2();
#endif
        yield return 0;
    }



    private void TestBombMovingRule()
    {
        Q.Log("-------------------- 基本炸弹连接测试");

        InitTarget();
        SetTileDataAt(0, 0, 5);
        SetTileDataAt(1, 0, 5);
        SetTileDataAt(2, 0, BOMB_H_YEL_1);
        SetTileDataAt(3, 0, 5);
        SetTileDataAt(4, 1, BOMB_H_YEL_2);
        //连接1
        List<Position> linkPath = new List<Position>();
        Position p = new Position(0, 0);
        linkPath.Add(p);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Link);
        //连接2
        p = new Position(1, 0);
        linkPath.Add(p);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Link);
        //连接3，连炸弹
        p = new Position(2, 0);
        linkPath.Add(p);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Link);
        linkPath.Add(p);
        //连接4，炸弹跟随移动
        p = new Position(3, 0);
        linkPath.Add(p);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Link);
        Q.Assert(batModel.GetElementAt(3, 0).ConfigID == BOMB_H_YEL_1, batModel.GetElementAt(3, 0).ConfigID.ToString());
        Q.Assert(batModel.GetElementAt(2, 0).ConfigID == 5, batModel.GetElementAt(2, 0).ConfigID.ToString());
        //连接5，炸弹继续跟随移动
        p = new Position(4, 1);
        linkPath.Add(p);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Link);
        Q.Assert(batModel.GetElementAt(4, 1).ConfigID == BOMB_H_YEL_2);
        Q.Assert(batModel.GetElementAt(3, 0).ConfigID == 5, batModel.GetElementAt(3, 0).ConfigID.ToString());
        //回退
        p = new Position(4, 1);
        linkPath.RemoveAt(linkPath.Count - 1);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Unlink);
        Q.Assert(batModel.GetElementAt(4, 1).ConfigID == BOMB_H_YEL_2);
        Q.Assert(batModel.GetElementAt(3, 0).ConfigID == BOMB_H_YEL_1);
        //回退
        p = new Position(3, 0);
        linkPath.RemoveAt(linkPath.Count - 1);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Unlink);
        Q.Assert(batModel.GetElementAt(2, 0).ConfigID == BOMB_H_YEL_1);
        //回退
        p = new Position(2, 0);
        linkPath.RemoveAt(linkPath.Count - 1);
        target.BombMovingRule(p, linkPath, BoardModifyingRules.LinkType.Unlink);
        Q.Assert(batModel.GetElementAt(2, 0).ConfigID == BOMB_H_YEL_1);
        Q.Assert(batModel.GetElementAt(1, 0).ConfigID == 5);
    }


    private void TestCoverterRule()
    {
        Q.Log("-------------------- 基本转换石转换测试");

        InitTarget();
        SetTileDataAt(0, 0, 5);
        SetTileDataAt(1, 1, CONVERTER_YEL_1);

        //连接1
        List<Position> linkPath = new List<Position>();
        Position p = new Position(0, 0);
        linkPath.Add(p);
        target.ConverterRule(p, linkPath, BoardModifyingRules.LinkType.Link);
        //连接2，触发转换石
        p = new Position(1, 1);
        linkPath.Add(p);
        target.ConverterRule(p, linkPath, BoardModifyingRules.LinkType.Link);
        Q.Assert(batModel.GetElementAt(0, 0).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(1, 0).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(2, 0).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(0, 1).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == CONVERTER_YEL_1);
        Q.Assert(batModel.GetElementAt(2, 1).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(0, 2).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(2, 2).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 5);
        //回退
        p = new Position(1, 1);
        linkPath.Add(p);
        target.ConverterRule(p, linkPath, BoardModifyingRules.LinkType.Unlink);
        Q.Assert(batModel.GetElementAt(0, 0).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(1, 0).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 0).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(0, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == CONVERTER_YEL_1);
        Q.Assert(batModel.GetElementAt(2, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(0, 2).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 2).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 1);
    }


    private void TestBombCascadeRule1()
    {
        Q.Log("-------------------- 炸弹级联测试");

        InitTarget();
        SetTileDataAt(0, 0, 5);
        SetTileDataAt(1, 0, 5);
        SetTileDataAt(2, 0, BOMB_H_YEL_1);
        SetTileDataAt(2, 2, BOMB_H_GRE_1);
        SetTileDataAt(1, 2, BOMB_V_RED_1);
        SetTileDataAt(2, 3, BOMB_A_RED_1);
        //凑成一个循环
        SetTileDataAt(4, 2, BOMB_V_BLU_1);
        SetTileDataAt(4, 0, BOMB_H_BLU_1);

        //连接1
        List<Position> linkPath = new List<Position>();
        Position p = new Position(0, 0);
        linkPath.Add(p);
        target.BombCascadeRule1(p, linkPath, BoardModifyingRules.LinkType.Link);
        //连接2
        p = new Position(1, 0);
        linkPath.Add(p);
        target.BombCascadeRule1(p, linkPath, BoardModifyingRules.LinkType.Link);
        //连接3，连炸弹
        p = new Position(2, 0);
        linkPath.Add(p);
        target.BombCascadeRule1(p, linkPath, BoardModifyingRules.LinkType.Link);
        //一级级联
        Q.Assert(batModel.GetElementAt(2, 2).ConfigID == BOMB_V_GRE_1);
        //二级级联
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == BOMB_H_RED_1);
        //二级级联
        Q.Assert(batModel.GetElementAt(4, 2).ConfigID == BOMB_H_BLU_1);
        //三级级联
        Q.Assert(batModel.GetElementAt(4, 0).ConfigID == BOMB_V_BLU_1);
        //不会影响区域炸弹
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == BOMB_A_RED_1);

        //回退
        p = new Position(2, 0);
        linkPath.RemoveAt(linkPath.Count - 1);
        target.BombCascadeRule1(p, linkPath, BoardModifyingRules.LinkType.Unlink);
        Q.Assert(batModel.GetElementAt(2, 2).ConfigID == BOMB_H_GRE_1);
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == BOMB_V_RED_1);
        Q.Assert(batModel.GetElementAt(4, 2).ConfigID == BOMB_V_BLU_1);
        Q.Assert(batModel.GetElementAt(4, 0).ConfigID == BOMB_H_BLU_1);
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == BOMB_A_RED_1);
    }


    private void TestBombCascadeRule2()
    {
        Q.Log("-------------------- 炸弹+转换石级联测试");

        InitTarget();
        SetTileDataAt(0, 0, 5);
        SetTileDataAt(1, 0, 5);
        SetTileDataAt(2, 0, BOMB_H_YEL_1);
        SetTileDataAt(2, 2, CONVERTER_RED_1);
        SetTileDataAt(2, 4, CONVERTER_GRE_1);

        //连接1
        List<Position> linkPath = new List<Position>();
        Position p = new Position(0, 0);
        linkPath.Add(p);
        target.BombCascadeRule2(p, linkPath, BoardModifyingRules.LinkType.Link);
        //连接2
        p = new Position(1, 0);
        linkPath.Add(p);
        target.BombCascadeRule2(p, linkPath, BoardModifyingRules.LinkType.Link);
        //连接3，连炸弹，级联到两个转换石
        p = new Position(2, 0);
        linkPath.Add(p);
        target.BombCascadeRule2(p, linkPath, BoardModifyingRules.LinkType.Link);
        //触发红色范围
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(2, 1).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(3, 2).ConfigID == 2);
        //触发绿色范围
        Q.Assert(batModel.GetElementAt(1, 4).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(3, 4).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(1, 5).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(2, 5).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(3, 5).ConfigID == 3);
        //红色、绿色交叠范围，炸弹颜色
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(3, 3).ConfigID == 5);

        //回退
        p = new Position(2, 0);
        linkPath.RemoveAt(linkPath.Count - 1);
        target.BombCascadeRule1(p, linkPath, BoardModifyingRules.LinkType.Unlink);
        //触发红色范围
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 2).ConfigID == 1);
        //触发绿色范围
        Q.Assert(batModel.GetElementAt(1, 4).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 4).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 5).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 5).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 5).ConfigID == 1);
        //红色、绿色交叠范围，炸弹颜色
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 3).ConfigID == 1);
    }
    
    private void TestAllRules()
    {
        Q.Log("-------------------- 综合测试");

        InitTarget();
        SetTileDataAt(0, 0, BOMB_H_YEL_1);
        SetTileDataAt(1, 0, 5);
        SetTileDataAt(2, 0, 5);
        SetTileDataAt(3, 0, 5);
        //测试炸弹变向循环
        SetTileDataAt(2, 1, BOMB_H_BLU_1);
        SetTileDataAt(4, 0, BOMB_H_BLU_1);
        SetTileDataAt(4, 1, BOMB_V_BLU_1);
        //影响到转换石
        SetTileDataAt(2, 2, CONVERTER_RED_1);
        SetTileDataAt(2, 4, CONVERTER_GRE_1);

        //连接1
        List<Position> linkPath = new List<Position>();
        Position p = new Position(0, 0);
        TileObject data = batModel.GetElementAt(p.Row, p.Col);
        linkPath.Add(p);
        viewEvtSys.TileStatusChangeEvent(data, ViewEventSystem.TileStatusChangeMode.ToLink, linkPath);

        //连接2
        p = new Position(1, 0);
        data = batModel.GetElementAt(p.Row, p.Col);
        linkPath.Add(p);
        viewEvtSys.TileStatusChangeEvent(data, ViewEventSystem.TileStatusChangeMode.ToLink, linkPath);
        Q.Assert(batModel.GetElementAt(0, 0).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(1, 0).ConfigID == BOMB_H_YEL_1);

        //连接3
        p = new Position(2, 0);
        data = batModel.GetElementAt(p.Row, p.Col);
        linkPath.Add(p);
        viewEvtSys.TileStatusChangeEvent(data, ViewEventSystem.TileStatusChangeMode.ToLink, linkPath);
        Q.Assert(batModel.GetElementAt(1, 0).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(2, 0).ConfigID == BOMB_H_YEL_1);
        Q.Assert(batModel.GetElementAt(2, 1).ConfigID == BOMB_V_BLU_1, batModel.GetElementAt(2, 1).ConfigID.ToString());
        Q.Assert(batModel.GetElementAt(4, 0).ConfigID == BOMB_V_BLU_1);
        Q.Assert(batModel.GetElementAt(4, 1).ConfigID == BOMB_H_BLU_1);
        //触发红色范围
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == 2, batModel.GetElementAt(1, 1).ConfigID.ToString());
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(3, 2).ConfigID == 2);
        //触发绿色范围
        Q.Assert(batModel.GetElementAt(1, 4).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(3, 4).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(1, 5).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(2, 5).ConfigID == 3);
        Q.Assert(batModel.GetElementAt(3, 5).ConfigID == 3);
        //红色、绿色交叠范围，炸弹颜色
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(3, 3).ConfigID == 5);

        //回退
        p = new Position(2, 0);
        data = batModel.GetElementAt(p.Row, p.Col);
        linkPath.RemoveAt(linkPath.Count - 1);
        viewEvtSys.TileStatusChangeEvent(data, ViewEventSystem.TileStatusChangeMode.ToUnlink, linkPath);
        //触发红色范围
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 2).ConfigID == 1);
        //触发绿色范围
        Q.Assert(batModel.GetElementAt(1, 4).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 4).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 5).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 5).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 5).ConfigID == 1);
        //红色、绿色交叠范围，炸弹颜色
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 3).ConfigID == 1);
    }


    private void TestAllRules2()
    {
        Q.Log("-------------------- 综合测试2");

        InitTarget();
        SetTileDataAt(0, 0, BOMB_H_YEL_1);
        SetTileDataAt(1, 0, 5);
        SetTileDataAt(2, 0, 5);
        //测试炸弹变向循环
        SetTileDataAt(1, 2, BOMB_H_BLU_1);
        //影响到转换石
        SetTileDataAt(2, 2, CONVERTER_RED_1);
        SetTileDataAt(2, 4, CONVERTER_GRE_1);

        //连接1
        List<Position> linkPath = new List<Position>();
        Position p = new Position(0, 0);
        TileObject data = batModel.GetElementAt(p.Row, p.Col);
        linkPath.Add(p);
        viewEvtSys.TileStatusChangeEvent(data, ViewEventSystem.TileStatusChangeMode.ToLink, linkPath);

        //连接2
        p = new Position(1, 0);
        data = batModel.GetElementAt(p.Row, p.Col);
        linkPath.Add(p);
        viewEvtSys.TileStatusChangeEvent(data, ViewEventSystem.TileStatusChangeMode.ToLink, linkPath);
        Q.Assert(batModel.GetElementAt(0, 0).ConfigID == 5);
        Q.Assert(batModel.GetElementAt(1, 0).ConfigID == BOMB_H_YEL_1);

        Q.Assert(batModel.GetElementAt(1, 2).ConfigID == BOMB_V_BLU_1);
        //触发红色范围
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(2, 1).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(3, 2).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == 2);
        Q.Assert(batModel.GetElementAt(3, 3).ConfigID == 2);

        //回退
        p = new Position(2, 0);
        data = batModel.GetElementAt(p.Row, p.Col);
        linkPath.RemoveAt(linkPath.Count - 1);
        viewEvtSys.TileStatusChangeEvent(data, ViewEventSystem.TileStatusChangeMode.ToUnlink, linkPath);
        //触发红色范围
        Q.Assert(batModel.GetElementAt(1, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 1).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 2).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(1, 3).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(2, 3).ConfigID == 1);
        Q.Assert(batModel.GetElementAt(3, 3).ConfigID == 1);
    }



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
        int numRow = 7;
        int numCol = 7;

        LevelConfig levConfig = new LevelConfig();
        levConfig.NumRow = numRow;
        levConfig.NumCol = numCol;
        levConfig.CoveringLayer = new int[numRow, numCol];
        levConfig.SeperatorHLayer = new int[numRow, numCol];
        levConfig.SeperatorVLayer = new int[numRow, numCol];
        levConfig.ElementLayer = new int[numRow, numCol];
        levConfig.ObstacleLayer = new int[numRow, numCol];
        levConfig.BottomLayer = new int[numRow, numCol];

        TileObject[,] covers = new TileObject[7, 7];
        TileObject[,] seprHs = new TileObject[7, 7];
        TileObject[,] seprVs = new TileObject[7, 7];
        TileObject[,] elements = new TileObject[7, 7];
        TileObject[,] obstacles = new TileObject[7, 7];
        TileObject[,] bottoms = new TileObject[7, 7];
        

        //全部填充紫色基础消除物
        for (int r = 0, n = elements.GetLength(0); r < n; r++)
        {
            for (int c = 0, m = elements.GetLength(1); c < m; c++)
            {
                elements[r, c] = GenTileData(r, c, 1);
            }
        }

        batModel.CrtLevelConfig = levConfig;
        batModel.__SetTileMap(covers, TileType.Cover);
        batModel.__SetTileMap(seprHs, TileType.SeperatorH);
        batModel.__SetTileMap(seprVs, TileType.SeperatorV);
        batModel.__SetTileMap(elements, TileType.Element);
        batModel.__SetTileMap(obstacles, TileType.Obstacle);
        batModel.__SetTileMap(bottoms, TileType.Bottom);
    }

    private void SetTileDataAt(int r, int c, int objId)
    {
        TileObjectConfig conf = tileObjectConfigs[objId];
        if (conf != null)
        {
            batModel.SetDataAt(
                new TileObject(r, c, tileObjectConfigs[objId]),
                r,
                c,
                conf.ObjectType);
        }
        else
        {
            Q.Assert(false, "r={0},c={1},id={2}", r, c, objId);
        }
    }

    private TileObject GenTileData(int r, int c, int objId)
    {
        return new TileObject(r, c, tileObjectConfigs[objId]);
    }
    #endregion
}
