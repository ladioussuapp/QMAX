using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.TileBehaviour;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct StepAnimationItem
{
    public GameObject mainTile;

    /// <summary>
    /// mainTile對應的消除範圍內的消除物
    /// </summary>
    public List<GameObject> ElimRangeTiles;

    public List<int> ElimOrders;

    /// <summary>
    /// 消除後得到分數
    /// </summary>
    public int EliminateScore;

    public StepAnimationItem(int Dirct)
    {
        mainTile = null;
        ElimRangeTiles = new List<GameObject>();
        ElimOrders = new List<int>();

        EliminateScore = 0;
    }
}

/// <summary>
/// 剩餘步數獎勵
/// </summary>
public class LeftStepAwardHelper
{
    private BattleModel battleModel;
    private Image stepOriImage;
    private BoardBehaviour boardBeh;
    //private GameController gameCtrl;
    private RectTransform flyLayer;
    //private Camera boardCamera;
    public const float FLY_DELAY = 0.1f;
    private RectTransform elementLayer;
    private Action callBack;
    private int index;
    private StepAnimationItem stepItem;
    public static void Play(BoardBehaviour boardBeh, BattleModel battleModel, Image stepOriImage,
        RectTransform elementLayer, RectTransform flyLayer, Action callBack)
    {
        LeftStepAwardHelper left = new LeftStepAwardHelper();
        left.init(boardBeh, battleModel, stepOriImage, elementLayer, flyLayer, callBack);
    }

    public void init(BoardBehaviour boardBeh, BattleModel battleModel,
        Image stepOriImage, RectTransform elementLayer, RectTransform flyLayer, Action callBack)
    {
        this.battleModel = battleModel;
        this.boardBeh = boardBeh;
        this.stepOriImage = stepOriImage;
        this.elementLayer = elementLayer;
        //boardCamera = Camera.main;
        this.flyLayer = flyLayer;
        this.callBack = callBack;
        index = 0;
        StartPlay();
    }

    public LeftStepAwardHelper()
    {
    }

    void StartPlay()
    {
        if (index < battleModel.StepAwardList.Count)
        {
            Step1();
        }
        else
        {
            if (callBack != null)
            {
                callBack();
            }
        }
    }

    private void Step1()
    {
        AwardShowController itemAward = new AwardShowController(boardBeh, battleModel, stepOriImage, elementLayer, flyLayer);
        itemAward.ShowAward(index);
        //float deltaT = itemAward.deltaTime;
        //float tempTime = deltaT - 0.1f;
        LeanTween.delayedCall(0.2f, delegate()
        {
            index++;
            StartPlay();
        });
    }
}

class AwardShowController
{
    private BattleModel battleModel;
    private Image stepOriImage;
    private BoardBehaviour boardBeh;
    private GameController gameCtrl;
    private RectTransform flyLayer;
    private Camera boardCamera;
    public const float FLY_DELAY = 0.1f;
    //private RectTransform elementLayer;
    private int Dire;
    private int index;
    private StepAnimationItem stepItem;
    private int totalStep;
    public float deltaTime;

    public AwardShowController(BoardBehaviour boardBeh, BattleModel battleModel,
        Image stepOriImage, RectTransform elementLayer, RectTransform flyLayer)
    {
        this.battleModel = battleModel;
        this.boardBeh = boardBeh;
        this.stepOriImage = stepOriImage;
        //this.elementLayer = elementLayer;
        gameCtrl = GameController.Instance;
        boardCamera = Camera.main;
        this.flyLayer = flyLayer;
    }

    public void ShowAward(int index)
    {
        deltaTime = 0;
        this.index = index;
        //elementLayer.gameObject.SetActive(true);
        GameObject flyObj = new GameObject();
        flyObj.transform.SetParent(flyLayer);
        flyObj.SetActive(false);
        flyObj.name = "flyObj" + battleModel.RemainSteps;
        Image flyGoImage = flyObj.AddComponent<Image>();
        flyGoImage.overrideSprite = gameCtrl.AtlasManager.GetSprite(Atlas.UIBattle, "Battle017");
        flyGoImage.rectTransform.localScale = new Vector3(1, 1, 1);


        Vector3 OriginLocalPos = getTargetTilePos(stepOriImage.transform);
        flyObj.transform.localPosition = OriginLocalPos;

        Vector3 targetLocalPos = new Vector3(-1, -1, -1);

        flyObj.SetActive(true);
        stepItem = battleModel.StepAwardList[this.index];
        GameObject targetGA = stepItem.mainTile;
        BaseTileBehaviour beh = targetGA.GetComponent<BaseTileBehaviour>();
        if (beh != null)
        {
            RectTransform targetUnitRect = targetGA.transform as RectTransform;
            targetLocalPos = getTargetTilePos(targetUnitRect);
        }
        else
        {
            return;
        }

        float disX = flyObj.transform.localPosition.x - targetLocalPos.x;
        float disY = flyObj.transform.localPosition.y - targetLocalPos.y;
        float dis = Mathf.Sqrt(disX * disX + disY * disY);
        float deltaT = dis / 1600;
        LeanTween.moveLocal(flyObj, targetLocalPos, deltaT).setEase(LeanTweenType.easeOutQuad).setOnComplete(delegate(object flyTempObj)
        {
            GameObject temp = (GameObject)flyTempObj;
            GameObject.Destroy(temp);
            temp = null;
            Step3();
        }).setOnCompleteParam(flyObj);

        battleModel.RemainSteps = battleModel.RemainSteps - 1;

        //RemainSteps  被更改後自動拋事件通知view
        //if (boardBeh.UIBattleBeh != null)
        //    boardBeh.UIBattleBeh.SetSteps(battleModel.RemainSteps);

        deltaTime = deltaT - 0.1f;
    }

    Vector3 getTargetTilePos(Transform targetRect)
    {

        Vector3 targetLocalPos = new Vector3(-1, -1, -1);
        if (targetRect != null)
        {
            Vector3 unitWorldPoint = targetRect.TransformPoint(0, 0, 0);
            Vector3 unitScreenPoint = boardCamera.WorldToScreenPoint(unitWorldPoint);
            Vector2 tempPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)flyLayer, unitScreenPoint, boardCamera, out tempPoint);
            targetLocalPos = new Vector3(tempPoint.x, tempPoint.y);
        }

        return targetLocalPos;
    }

    private void Step3()
    {
        int count = 0;
        foreach (GameObject ga in stepItem.ElimRangeTiles)
        {
            if (ga == null)
                continue;

            BaseTileBehaviour beh = ga.GetComponent<BaseTileBehaviour>();
            if (beh.Data != null && beh.Config.ObjectType == TileType.Element)
            {
                count++;
                battleModel.AddStepAward(beh.Config.deliverAward.type, beh.Config.deliverAward.Qtt);
            }
        }
        Vector2 mainV = new Vector2(0, 0);
        GameObject mainGo = stepItem.mainTile;
        if (mainGo != null)
        {
            BaseTileBehaviour mainBeh = mainGo.GetComponent<BaseTileBehaviour>();
            if (mainBeh != null && mainBeh.Data != null)
            {
                mainV.x = mainBeh.Row;
                mainV.y = mainBeh.Col;
            }
        }


        Q.Assert(stepItem.ElimRangeTiles.Count == stepItem.ElimOrders.Count);
        for (int i = 0, n = stepItem.ElimRangeTiles.Count; i < n; i++)
        {
            GameObject ga = stepItem.ElimRangeTiles[i];
            int order = stepItem.ElimOrders[i];
            if (ga == null)
                continue;

            BaseTileBehaviour beh = ga.GetComponent<BaseTileBehaviour>();

            if (beh == null || beh.Data == null)
                continue;

            if (beh.Config.ObjectType != TileType.Element)
            {
                continue;
            }

            float delay = FLY_DELAY * order;
            int row1 = beh.Row;
            int col1 = beh.Col;
            if (!(mainV.x == 0 && mainV.y == 0))
            {
                int min = Math.Max(Math.Abs((int)mainV.x - row1), Math.Abs((int)mainV.y - col1));
                delay = min * FLY_DELAY;
            }

            LeanTween.delayedCall(delay, delegate(object obj)
            {
                BaseTileBehaviour tObj = (BaseTileBehaviour)obj;
                if (tObj != null && tObj.Data != null)
                {
                    RectTransform refRect = tObj.transform as RectTransform;
                    Vector2 anchorMax = refRect.anchorMax;
                    Vector2 anchorMin = refRect.anchorMin;
                    Vector3 anchoredPosition3D = refRect.anchoredPosition3D;
                    ItemQtt award = tObj.Config.deliverAward;
                    Sprite icon = BattleTools.GetBattleAwardIconByID(award.type);
                    float posX = 0;
                    if (tObj.Col == 3)
                    {
                        posX = refRect.localPosition.x;
                    }
                    else if (tObj.Col > 3)
                    {
                        posX = refRect.localPosition.x - 50;
                    }
                    else if (tObj.Col < 3)
                    {
                        posX = refRect.localPosition.x + 50;
                    }
                    RectTransform eleRect = tObj.transform as RectTransform;
                    boardBeh.EliminateEffect.PlayAt(tObj.Config, eleRect, delay);
                    tObj.IsLinked = true;
                    if (tObj.Eliminate(battleModel.GetElementAt(tObj.Row, tObj.Col)))
                        boardBeh.GcTileGameObj(tObj.gameObject);

                    boardBeh.AddScore(stepItem.EliminateScore);

                    BattleTools.CreateBoardFlyAward(icon, boardBeh.EliminateEffect.Layer, anchorMax, anchorMin,
                        anchoredPosition3D, posX, award.Qtt, 0.7f, 700);
                }
            }).setOnCompleteParam(beh);
        }
    }
}
