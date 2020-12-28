using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.TileBehaviour;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com4Love.Qmax.Helper
{
    /// <summary>
    /// 控制攻擊效果的輔助類
    /// 一個攻擊過程中，可能攻擊多個敵人（攻擊力延續）。每攻擊一個敵人，稱之為一個Round（回合）
    ///
    /// </summary>
    /// A.有夥伴，有敵人：1, 2, 3a, 4, 5a, 6
    /// B.有夥伴，無敵人：1.飛向夥伴；2.夥伴播放攻擊動作；3.播放煙花
    /// C.無夥伴，有敵人：1.原地爆破，敵人播放受擊
    /// D.無夥伴，無敵人：1.原地爆破
    ///
    /// 同時消除多種顏色時
    /// E.部分有夥伴，無敵人：各自走B、D流程
    /// F.部分有夥伴，有敵人：
    ///
    /// 1. 播放Combo口號
    /// 2. 設置SkillLoading
    /// 3. 所有地形物按順序播放消除效果
    /// 3a. 有夥伴的消除物飛向夥伴，走Step4
    /// 3b. 無夥伴的消除物（或其他地形物）消失，並打擊敵人，敵人播放受擊動畫及特效
    /// 4. 夥伴播放攻擊動作，並在攻擊幀時觸發Step5
    /// 5. 如果有夥伴，則
    /// 5a. 有敵人：消除物飛向敵人，敵人播放受擊動畫及特效
    /// 5b. 無敵人：消除物打出煙花
    /// 6. 按需要播放掉血特效、敵人死亡特效
    /// 7. 掉落
    /// 8. 夥伴扔技能
    /// 9. 敵人扔技能
    class BattleAttackProcessHelper
    {
        /// <summary>
        /// 普通消除物的**基礎消除時間間隔**
        /// </summary>
        private const float BASE_ELIM_DELAY = 0.08f;

        /// <summary>
        /// 消除時間間隔的加速度
        /// </summary>
        private const float ATTEN_ELIM_DELAY = 0.005f;

        /// <summary>
        /// 區域炸彈（包括全屏炸彈）的消除間隔
        /// </summary>
        private const float AREA_BOMB_DELAY = 0.25f;

        /// <summary>
        /// 線性炸彈（縱向或橫向）爆炸消除的**基礎時間間隔**
        /// </summary>
        private const float BASE_LINEAR_BOMB_ELIM_DELAY = 0.16f;

        /// <summary>
        /// 消除的**最小時間間隔**
        /// </summary>
        private const float MIN_ELIM_DELAY = 0.05f;




        /// <summary>
        /// 
        /// </summary>
        /// <param name="boardBeh"></param>
        /// <param name="eliminateArg"></param>
        /// <param name="model"></param>
        /// <param name="flyToGuy">元素是否要飛向夥伴</param>
        /// <param name="callback"></param>
        /// <param name="attackCountCallBack"></param>
        static public void Play(BoardBehaviour boardBeh,
                                ModelEventSystem.EliminateEventArgs eliminateArg,
                                QmaxModel model,
                                bool flyToGuy,
                                Action callback,
                                Action<int, int, int> attackCountCallBack)
        {
            BattleAttackProcessHelper ins = new BattleAttackProcessHelper(
                boardBeh, eliminateArg, model, flyToGuy,
                callback, attackCountCallBack);

            ins.Play();
        }

        private BoardBehaviour boardBeh;
        private ColorType mainColor;

        private QmaxModel model;

        /// <summary>
        /// 當前的怪點，每回合會更新。
        /// </summary>
        private EnemyPoint crtEnemyPoint;

        /// <summary>
        /// 是否要飛向夥伴
        /// </summary>
        private bool flyToGuy = false;

        private Action allProgressCallback;

        private Action<int, int, int> attackCountCallBack;

        private Dictionary<ColorType, List<BaseTileBehaviour>> colorDict;

        private List<TileObject> oriElimDatas;

        private List<TileObject> newElimDatas;

        /// <summary>
        /// 消除順序
        /// </summary>
        private List<int> elimOrders;

        /// <summary>
        /// 消除獎勵
        /// </summary>
        private List<ItemQtt[]> elimRewards;

        /// <summary>
        /// 被消除的View層的地形物，順序與afterEliminatedData\beforeEliminatedData\elimOrders對應
        /// </summary>
        private List<BaseTileBehaviour> elimTileBehs;

        private HitEnemyBehaviour hitEnemyBeh;

        /// <summary>
        /// 當前是否有敵人。當擊退一個敵人後，這個值會更新
        /// </summary>
        private bool hasEnemy = false;

        /// <summary>
        /// 被消除的地形物中消除物的數量
        /// </summary>
        private int elementNum = 0;

        /// <summary>
        /// 已擊中敵人的消除物計數
        /// </summary>
        private int hitCount = 0;

        /// <summary>
        /// 普通消除物的消除間隔
        /// </summary>
        private float normalElimDelay;


        /// <summary>
        /// 線性炸彈（縱向或橫向）爆炸消失時間間隔
        /// </summary>
        private float linearBombElimDelay;

        /// <summary>
        /// 掉落過程是否結束
        /// </summary>
        private bool isDropElementEnd = true;

        /// <summary>
        /// 攻擊工程是否結束
        /// </summary>
        private bool isAttackProcessEnd;

        /// <summary>
        /// 記錄各種顏色消除物的數量
        /// </summary>
        private Dictionary<ColorType, int> colorCountDict;

        private Dictionary<ColorType, List<BaseTileBehaviour>> crtAttackTiles;


        private List<TileObject> throwTileList;

        /// <summary>
        /// 怪物扔出的Tile
        /// </summary>
        private TileObject monsterThrowTile;

        /// <summary>
        ///掉落隊列，只會在Round 1使用，執行完會被清空
        /// </summary>
        private List<List<ModelEventSystem.Move>> dropList;

        /// <summary>
        /// 本回合攻擊的值
        /// </summary>
        private int hurtValueThisRound;

        /// <summary>
        /// combo數量
        /// </summary>
        private float comboRate = 1;


        public BattleAttackProcessHelper(BoardBehaviour boardBeh,
                                         ModelEventSystem.EliminateEventArgs eliminateArg,
                                         QmaxModel model,
                                         bool flyToGuy,
                                         Action callback,
                                         Action<int, int, int> attackCountCallBack)
        {
            this.flyToGuy = flyToGuy;
            this.boardBeh = boardBeh;
            this.mainColor = eliminateArg.MainColor;
            this.throwTileList = eliminateArg.ThrowTileList;
            this.monsterThrowTile = eliminateArg.MonsterThrowTile;
            this.model = model;
            this.allProgressCallback = callback;
            this.attackCountCallBack = attackCountCallBack;

            dropList = eliminateArg.DropList;
            this.crtEnemyPoint = boardBeh.CrtEnemyPoint;

            Unit enemy = model.BattleModel.GetCrtEnemy();
            hasEnemy = enemy != null;

            elimTileBehs = HandleEventArgs(eliminateArg, out oriElimDatas, out newElimDatas, out elimOrders, out elimRewards);
            int mainColorCount = 0;
            colorCountDict = new Dictionary<ColorType, int>();
            //將不同顏色的消除物分組
            colorDict = new Dictionary<ColorType, List<BaseTileBehaviour>>();
            colorDict.Add(ColorType.None, new List<BaseTileBehaviour>());
            colorDict.Add(ColorType.Earth, new List<BaseTileBehaviour>());
            colorDict.Add(ColorType.Fire, new List<BaseTileBehaviour>());
            colorDict.Add(ColorType.Wood, new List<BaseTileBehaviour>());
            colorDict.Add(ColorType.Water, new List<BaseTileBehaviour>());
            colorDict.Add(ColorType.Golden, new List<BaseTileBehaviour>());
            for (int i = 0, n = elimTileBehs.Count; i < n; i++)
            {
                BaseTileBehaviour tb = elimTileBehs[i];
                ColorType c = tb.Config.ColorType;
                if (tb.Config.ObjectType == TileType.Element)
                {
                    elementNum++;
                }
                if (c == mainColor)
                    mainColorCount++;

                colorDict[c].Add(tb);
            }

            if (mainColorCount > 0)
            {
                this.comboRate = GameController.Instance.Model.ComboConfigs[mainColorCount].ComboRate;
            }
            else
            {
                this.comboRate = 1f;
            }


            //消除物逐個飛向Unit的逐個延遲
            this.normalElimDelay = BASE_ELIM_DELAY - (ATTEN_ELIM_DELAY * (elementNum - 3));
            this.linearBombElimDelay = BASE_LINEAR_BOMB_ELIM_DELAY - (ATTEN_ELIM_DELAY * (elementNum - 3));
            ///控制最小間隔時間//
            normalElimDelay = normalElimDelay < MIN_ELIM_DELAY ? MIN_ELIM_DELAY : normalElimDelay;
            linearBombElimDelay = linearBombElimDelay < MIN_ELIM_DELAY ? MIN_ELIM_DELAY : linearBombElimDelay;

            crtAttackTiles = new Dictionary<ColorType, List<BaseTileBehaviour>>();
        }


        public void Play()
        {
            Q.Log(LogTag.Battle, "BattleAttackProcessHelper::Play()");
            isAttackProcessEnd = false;
            //攻擊過程，界面加鎖
            boardBeh.PlusInteractLock(true);
            Step1(StepCallback);
        }




        /// <summary>
        /// 處理EliminateEventArgs的數據
        /// </summary>
        /// <param name="eliminateArg"></param>
        /// <param name="retOriDatas"></param>
        /// <param name="retNewDatas"></param>
        /// <param name="retOrders"></param>
        /// <param name="retRewards"></param>
        /// <returns></returns>
        private List<BaseTileBehaviour> HandleEventArgs(ModelEventSystem.EliminateEventArgs eliminateArg,
                                                        out List<TileObject> retOriDatas,
                                                        out List<TileObject> retNewDatas,
                                                        out List<int> retOrders,
                                                        out List<ItemQtt[]> retRewards)
        {
            Q.Assert(eliminateArg.OriTileDatas.Count > 0);
            Q.Assert(eliminateArg.OriTileDatas.Count == eliminateArg.NewTileDatas.Count);
            Q.Assert(eliminateArg.OriTileDatas.Count == eliminateArg.ElimOrders.Count);
            Q.Assert(eliminateArg.OriTileDatas.Count == eliminateArg.ElimRewards.Count);

            List<BaseTileBehaviour> ret = new List<BaseTileBehaviour>();
            retOriDatas = new List<TileObject>();
            retNewDatas = new List<TileObject>();
            retOrders = new List<int>();
            retRewards = new List<ItemQtt[]>();
            for (int i = 0, n = eliminateArg.OriTileDatas.Count; i < n; i++)
            {
                TileObject ta1 = eliminateArg.OriTileDatas[i];
                //TileObject ta2 = eliminateArg.NewTileDatas[i];
                Q.Assert(ta1 != null);
                GameObject[,] list = boardBeh.GetTypeGameObjects(ta1.Config.ObjectType);


                GameObject tObj = list[ta1.Row, ta1.Col];
                BaseTileBehaviour tileBeh = tObj.GetComponent<BaseTileBehaviour>();

                if (ta1.Config.ObjectType != TileType.Element ||
                    ta1.Config.ElementType != ElementType.MultiColor)
                {
                    ret.Add(tileBeh);
                    retOriDatas.Add(eliminateArg.OriTileDatas[i]);
                    retNewDatas.Add(eliminateArg.NewTileDatas[i]);
                    retOrders.Add(eliminateArg.ElimOrders[i]);
                    retRewards.Add(eliminateArg.ElimRewards[i]);
                }
                else//多色石處理
                {
                    List<BaseTileBehaviour> convertedTiles = ConvertMultiColorToNormal(tileBeh);
                    Q.Assert(eliminateArg.NewTileDatas[i] == null);
                    for (int j = 0, m = convertedTiles.Count; j < m; j++)
                    {
                        ret.Add(convertedTiles[j]);
                        retOriDatas.Add(convertedTiles[j].Data);
                        //變成普通物，必然只能消失
                        retNewDatas.Add(null);
                        //消除順序不變
                        retOrders.Add(eliminateArg.ElimOrders[i]);
                        //如果多色石有獎勵，那麼只把獎勵賦予轉換後的第一個Normal Element
                        retRewards.Add(j == 0 ? eliminateArg.ElimRewards[i] : null);
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// 把多色石轉換成多個普通消除物
        /// </summary>
        /// <param name="tileBeh"></param>
        /// <returns></returns>
        private List<BaseTileBehaviour> ConvertMultiColorToNormal(BaseTileBehaviour tileBeh, bool destroyMultiColor = true)
        {
            TileObject ta = tileBeh.Data;
            Q.Assert(ta != null);
            if (ta.Config.ObjectType != TileType.Element || ta.Config.ElementType != ElementType.MultiColor)
                return null;

            Position p = new Position(ta.Row, ta.Col);
            RectTransform polyGa = boardBeh.eleViews[p.Row, p.Col].GetComponent<RectTransform>();
            List<BaseTileBehaviour> ret = new List<BaseTileBehaviour>();
            for (int j = 0, m = ta.Config.AllColors.Count; j < m; j++)
            {
                TileObjectConfig conf = model.TileObjectConfigs[(int)ta.Config.AllColors[j]];
                TileObject newTa = new TileObject(p.Row, ta.Col, conf);
                GameObject ga = boardBeh.GetTileGameObj(ta.Row, ta.Col, newTa);
                ga.GetComponent<RectTransform>().anchoredPosition = polyGa.anchoredPosition;
                ga.GetComponent<RectTransform>().localScale = polyGa.localScale;
                ret.Add(ga.GetComponent<BaseTileBehaviour>());
            }
            boardBeh.GcTileGameObj(tileBeh.gameObject);
            return ret;
        }



        private void StepCallback(int step, int status)
        {
            //status 表示返回狀態，0為正常，其他狀態為各個步驟自己定義
            Q.Log(LogTag.Battle, "{0}::{1}, {2}, step={3}", GetType().Name, "StepCallback", 1, step);
            switch (step)
            {
                case 1:
                    Step2(StepCallback);
                    break;
                case 2:
                    Step3(StepCallback);
                    break;
                case 3:
                    Step4(StepCallback);
                    break;
                case 4:
                    Step5(StepCallback);
                    break;
                case 5:
                    Step6(StepCallback);
                    break;
                case 6:
                    Step7(StepCallback);
                    break;
                case 7:
                    //檢查是否有剩餘戰鬥力，沒有的話，走Step8
                    //有的話，走Step4
                    if (!CalcRemainAttack(out hurtValueThisRound))
                        Step8(StepCallback);
                    else
                        Step4(StepCallback);
                    break;
                case 8:
                    Step9(StepCallback);
                    break;
                case 9:
                    boardBeh.MinusInteractLock();
                    foreach (KeyValuePair<ColorType, List<BaseTileBehaviour>> pair in colorDict)
                    {
                        if (boardBeh.SkillLoadings.ContainsKey(pair.Key))
                            boardBeh.SkillLoadings[pair.Key].gameObject.SetActive(false);
                    }
                    // 整個攻擊過程完成
                    isAttackProcessEnd = true;
                    if (allProgressCallback != null && isDropElementEnd)
                        allProgressCallback();
                    break;
            }
        }

        /// <summary>
        /// 播放Combo口號
        /// </summary>
        /// <param name="callback"></param>
        private void Step1(Action<int, int> callback)
        {
            //TODO 把播放combo口號放這裡
            callback(1, 0);
        }


        /// <summary>
        /// 設置SkillLoading
        /// </summary>
        /// <param name="callback"></param>
        private void Step2(Action<int, int> callback)
        {
            foreach (KeyValuePair<ColorType, List<BaseTileBehaviour>> pair in colorDict)
            {
                //如果沒有響應的伙伴，則不用處理
                if (!boardBeh.UnitAnims.ContainsKey(pair.Key) || pair.Value.Count == 0)
                    continue;

                SkillLoadingBehaviour skillLoading = boardBeh.SkillLoadings[pair.Key];
                skillLoading.gameObject.SetActive(true);
                //這裡需要重新把技能百分比重新設置，因為某些夥伴可能是被其他顏色的炸彈波及的
                //此時數據層已經更新過了，所以直接拿數據層的數據來設置skill cd就可以了
                //但如果從數據拿到的skill cd == 0，表示該次攻擊的skill cd已經集滿歸零了
                int skillCD = model.BattleModel.SkillCDDict[pair.Key];
                int maxSkillCD = model.BattleModel.SkillConfDict[pair.Key].SkillCD;
                if (skillCD > 0)
                    skillLoading.SetPercentage((float)skillCD / maxSkillCD, true);
                else
                    skillLoading.SetPercentage(1, true);
            }
            callback(2, 0);
        }


        private void Step3(Action<int, int> callback)
        {
            int count = 0;
            Action subCallback = null;
            subCallback = delegate()
            {
                if (--count > 0)
                    return;

                callback(3, 0);
            };
            ElementBehaviour globalBomb = null;
            ElementBehaviour normalBomb = null;
            foreach (KeyValuePair<ColorType, List<BaseTileBehaviour>> pair in colorDict)
            {
                if (pair.Value.Count == 0)
                    continue;
                foreach (var item in pair.Value)
                {
                    ElementBehaviour tilbeh = item.GetComponent<ElementBehaviour>();
                    if (tilbeh != null && tilbeh.Config.ID == 706)
                    {
                        if (globalBomb == null) globalBomb = tilbeh;
                        break;
                    }

                    if (tilbeh != null &&
                        tilbeh.Type == ElementType.Bomb &&
                        tilbeh.Config.RangeMode != ElimRangeMode.Normal)
                    {
                        if (normalBomb == null)
                            normalBomb = tilbeh;

                        break;
                    }
                }
            }

            Dictionary<ColorType, Animator> UnitAnims = boardBeh.UnitAnims;
            foreach (KeyValuePair<ColorType, List<BaseTileBehaviour>> pair in colorDict)
            {
                if (pair.Value.Count == 0)
                    continue;

                count++;
                bool hasUnit = UnitAnims.ContainsKey(pair.Key);

                if (hasUnit && flyToGuy)
                {
                    if (pair.Key == ColorType.Wood && boardBeh.IsGuide)
                        Step3b(pair.Value, globalBomb, normalBomb, subCallback);
                    else//對應顏色有小伙伴，飛向小伙伴
                        Step3a(pair.Value, UnitAnims[pair.Key], globalBomb, normalBomb, subCallback);
                }
                else
                {
                    if (hasUnit && !flyToGuy)
                    {
                        Animator anim = UnitAnims[pair.Key];
                        anim.SetTrigger("TriggerAttack");
                        UnitSoundBehaviour us = anim.GetComponent<UnitSoundBehaviour>();
                        if (us != null)
                            us.PlayAttackSound();
                    }

                    //對應顏色沒有夥伴，直接在敵人身上爆炸
                    Step3b(pair.Value, globalBomb, normalBomb, subCallback);
                }
            }//for

            Q.Assert(count > 0);
            CalcRemainAttack(out hurtValueThisRound);
        }



        private void Step3a(List<BaseTileBehaviour> elements,
                            Animator currentUnit,
                            BaseTileBehaviour globalBomb,
                            BaseTileBehaviour normalBomb,
                            Action callback)
        {
            Q.Log(LogTag.Battle, "BatAtkProHel:Step3a");

            //某個顏色的消除物飛向對應的正在播放蓄力動作的伙伴

            //坐標轉換，將夥伴的坐標轉換為flyLayer的坐標
            RectTransform targetUnitRect = currentUnit.transform as RectTransform;
            Vector3 unitWorldPoint = targetUnitRect.TransformPoint(0, 0, 0);
            Vector3 unitScreenPoint = boardBeh.BoardCamera.WorldToScreenPoint(unitWorldPoint);
            Vector2 tempPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)boardBeh.ElementLayer.transform, unitScreenPoint, boardBeh.BoardCamera, out tempPoint);
            Vector3 targetLocalPos = new Vector3(tempPoint.x, tempPoint.y);
            targetLocalPos.y += 70;

            int count = 0;
            Action<object> onChargeComplete = delegate(object rectTransform)
            {
                RectTransform eleRect = rectTransform as RectTransform;
                Utils.SetCurrentPositoinToAnchor(eleRect);
                eleRect.GetComponent<Animator>().enabled = true;
                eleRect.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                //消除物飛到夥伴身上後先隱藏
                eleRect.gameObject.SetActive(false);
                if (--count > 0)
                    return;

                count = 0;
                callback();
            };


            float explodeDelay = 0;
            GameController.Instance.AudioManager.PlayAudio("SD_remove_assemble1");

            ElimRangeMode mode = ElimRangeMode.Normal;
            if (normalBomb != null)
            {
                mode = normalBomb.Config.RangeMode;
            }

            int supAudioEffCount = 0;
            int elementsCount = elements.Count;
            for (int i = 0, n = elements.Count; i < n; i++)
            {
                BaseTileBehaviour tObj = elements[i];

                explodeDelay = GetFlyDelayTime(tObj, globalBomb, mode);

                tObj.Eliminate(GetAfterEliminateData(tObj.Data));
                boardBeh.DeattachElementGameObject(tObj.gameObject);
                RectTransform eleRect = tObj.transform as RectTransform;

                Image image = eleRect.GetComponent<Image>();
                if (image != null)
                    image.color = new Color(1, 1, 1, 1);

                //播放消除特效
                boardBeh.EliminateEffect.PlayAt(tObj.Config, eleRect, explodeDelay, (globalBomb != null || normalBomb != null));

                // 元素爆裂音效疊加
                boardBeh.StartCoroutine(Utils.DelayToInvokeDo(
                    delegate()
                    {
                        if (supAudioEffCount == 0)
                            GameController.Instance.AudioManager.PlayAudio("SD_attack_jewel_break_3");
                        else if (supAudioEffCount == elementsCount - 1)
                            GameController.Instance.AudioManager.PlayAudio("SD_attack_jewel_break_4");
                        supAudioEffCount++;
                    }, explodeDelay
                ));

                //刪除炸彈之類的技能發光特效
                if (eleRect.Find("EffectJinengtubiao") != null)
                {
                    GameObject.Destroy(eleRect.Find("EffectJinengtubiao").gameObject);//刪除子級特效
                }

                if (globalBomb == null)
                    eleRect.transform.SetParent(boardBeh.FlyLayer);
                //如果激活了Animator，且Animator.CullingMode==AnimatorCullingMode.AlwaysAnimate
                //會導致無法設置eleRect的Transfrom。
                //因此運動前先將其enabled=false，運動完再設為true
                Animator anim = eleRect.GetComponent<Animator>();
                Q.Assert(anim != null);
                //anim.enabled = false; // 移到下面delayedCall了

                count++;

                if (globalBomb == null)
                {
                    LeanTween.delayedCall(explodeDelay, delegate()
                    {
                        anim.enabled = false;
                    });

                    LeanTween.moveLocal(eleRect.gameObject, targetLocalPos, 0.3f)
                    .setEase(LeanTweenType.easeInBack)
                    .setDelay(explodeDelay)
                    .setOnComplete(onChargeComplete)
                    .setOnCompleteParam(eleRect);

                    //添加漂浮效果
                    LeanTween.scale(eleRect.gameObject, new Vector3(1.8f, 1.8f, 1), 0.1f)
                    .setDelay(explodeDelay)
                    .setOnComplete(delegate()
                    {
                        LeanTween.rotateLocal(eleRect.gameObject, new Vector3(0, 0, -1800), 0.2f);
                        LeanTween.scale(eleRect.gameObject, new Vector3(1.0f, 1.0f, 1), 0.2f);
                        if (globalBomb != null) eleRect.transform.SetParent(boardBeh.FlyLayer);
                    });
                }
                else
                {
                    LeanTween.delayedCall(explodeDelay, delegate()
                    {
                        anim.enabled = false;
                    });

                    LeanTween.moveLocal(eleRect.gameObject, targetLocalPos, 0.3f)
                    .setEase(LeanTweenType.easeInBack)
                    .setDelay(explodeDelay)
                    .setOnComplete(onChargeComplete)
                    .setOnCompleteParam(eleRect);

                    //添加漂浮效果
                    LeanTween.scale(eleRect.gameObject, new Vector3(1.8f, 1.8f, 1), 0.1f)
                    .setDelay(explodeDelay)
                    .setOnComplete(delegate()
                    {
                        LeanTween.rotateLocal(eleRect.gameObject, new Vector3(0, 0, -1800), 0.2f);
                        LeanTween.scale(eleRect.gameObject, new Vector3(1.0f, 1.0f, 1), 0.2f);
                        if (globalBomb != null) eleRect.transform.SetParent(boardBeh.FlyLayer);
                    });
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="globalBomb">全屏炸彈</param>
        /// <param name="normalBomb">非全屏炸彈</param>
        /// <param name="callback"></param>
        private void Step3b(List<BaseTileBehaviour> list,
                            BaseTileBehaviour globalBomb,
                            BaseTileBehaviour normalBomb,
                            Action callback)
        {

            Q.Log(LogTag.Battle, "BatAtkProHel:Step3b");
            //無夥伴的消除物消失，並打擊敵人，敵人播放受擊動畫及特效
            int count = 0;
            ColorType color = list[0].Config.ColorType;

            bool isShield = false;
            SkillConfig skillCfg = boardBeh.CrtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
            if (skillCfg != null && skillCfg.SkillColor == color)
            {
                isShield = true;
            }

            float explodeDelay = 0;
            ElimRangeMode mode = ElimRangeMode.Normal;
            if (normalBomb != null)
            {
                mode = normalBomb.Config.RangeMode;
            }

            for (int i = 0, n = list.Count; i < n; i++)
            {
                BaseTileBehaviour tObj = list[i];
                RectTransform eleRect = tObj.transform as RectTransform;
                count++;

                explodeDelay = GetFlyDelayTime(tObj, globalBomb, mode);

                //播放消除特效
                boardBeh.EliminateEffect.PlayAt(tObj.Config, eleRect, explodeDelay, (globalBomb != null || normalBomb != null));

                if (tObj.Config.ObjectType != TileType.Element)
                {
                    LeanTween.delayedCall(explodeDelay, delegate()
                    {
                        Q.Assert(tObj.Config != null, "{0}, {1}, {2}", tObj.Row, tObj.Col, tObj.gameObject.name);
                        //檢查消除獎勵
                        ///物品受影響機制待更改，從數據層獲取最新的狀態

                        int idx = elimTileBehs.IndexOf(tObj);
                        if (idx >= 0)
                        {
                            ItemQtt[] rewardList = elimRewards[idx];
                            CreateObjectReward(rewardList, eleRect, 0);
                        }

                        if (tObj.Eliminate(GetAfterEliminateData(tObj.Data)))
                            boardBeh.GcTileGameObj(tObj.gameObject);

                        if (--count <= 0)
                        {
                            //已經處理完的顏色從crtAttackTiles裡移除
                            crtAttackTiles[color].Clear();
                            callback();
                        }
                    });
                    continue;
                }

                LeanTween.delayedCall(explodeDelay, delegate()
                {
                    if (tObj.Eliminate(GetAfterEliminateData(tObj.Data)))
                    {
                        boardBeh.GcTileGameObj(tObj.gameObject);
                    }

                    // 元素爆裂音效疊加
                    if (hitCount == 0)
                        GameController.Instance.AudioManager.PlayAudio("SD_attack_jewel_break_3");
                    else if (count <= 1)
                        GameController.Instance.AudioManager.PlayAudio("SD_attack_jewel_break_4");

                    hitCount++;
                    --count;
                    if (hasEnemy)
                    {
                        ElementHitEnemy(isShield, color);

                        //每次擊中都要調用一次回調
                        if (attackCountCallBack != null)
                            attackCountCallBack(hitCount, elementNum, hurtValueThisRound);

                        if (count == 0)
                            crtEnemyPoint.EnemyAnimator.SetBool("IsComboAttack", false);
                    }

                    if (count == 0)
                    {
                        //已經處理完的顏色從crtAttackTiles裡移除
                        //因為已經在這裡Clear，所以在Step4、Step5，該顏色的邏輯會直接跳過
                        crtAttackTiles[color].Clear();
                        callback();
                    }
                });//delayCall
            }//for
        }//function


        private void ElementHitEnemy(bool isShield, ColorType color)
        {
            ///因為這個怪物可能在播放驚恐動作的時候加速了，所以切換受打擊動作時要改成正常速度///
            crtEnemyPoint.EnemyAnimator.speed = 1f;

            if (hitCount == 1)
            {
                //被無夥伴的消除物連續擊中，敵人播放第一次持續受擊動作
                if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerHit");
                }
                else if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Weak_Idle"))
                {
                    crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerWeakHit");
                }
                else if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shock"))
                {
                    string aniName = IsEnemyWeak() ? "TriggerWeakHit" : "TriggerHit";
                    crtEnemyPoint.EnemyAnimator.SetTrigger(aniName);
                }
                //crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerHit");
                crtEnemyPoint.EnemyAnimator.SetBool("IsComboAttack", true);
            }

            if (isShield)
            {
                boardBeh.HitEffect.PlayDefenseEffect();
            }
            else
            {
                boardBeh.HitEffect.PlayHitEff(color);
            }
            boardBeh.EnemyHitImmuneEff(color);
        }


        /// <summary>
        /// 夥伴播放攻擊動作，並在攻擊幀時時回調
        /// </summary>
        /// <param name="callback"></param>
        private void Step4(Action<int, int> callback)
        {
            //dropList只会在第一回合（Round 1）有用，执行完一次掉落后，dropList，dropList就会置为null
            if (dropList != null)
            {
                DropElement();
            }

            int count = 0;
            ///如果達到combo數量並且還有戰鬥力下一個怪物播放驚恐動畫///
            ///如果沒有主顏色夥伴不播放怪物驚嚇動作///
            if (boardBeh.UnitAnims.ContainsKey(mainColor))
                MonstorShockHelper.MonstorShock(crtEnemyPoint.EnemyAnimator, comboRate, false);

            foreach (KeyValuePair<ColorType, List<BaseTileBehaviour>> pair in crtAttackTiles)
            {
                if (!boardBeh.UnitAnims.ContainsKey(pair.Key) || crtAttackTiles[pair.Key].Count == 0)
                    continue;

                Action<Animator, AnimatorStateInfo, int> OnStateExit = null;
                OnStateExit = delegate(Animator enemyAnimator, AnimatorStateInfo stateInfo, int layerIndex)
                {

                    bool hasNextAttack = CheckHasNextAttack();
                    if (stateInfo.IsName("Charge") && !hasNextAttack)
                    {
                        foreach (KeyValuePair<ColorType, Animator> unit in boardBeh.UnitAnims)
                        {
                            if (unit.Value == enemyAnimator)
                            {
                                boardBeh.SetChargeEffect(unit.Key, 0);
                                break;
                            }
                        }
                    }
                    else if (stateInfo.IsName("Attack"))
                    {
                        enemyAnimator.SetTrigger("TriggerIdle");

                        enemyAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= OnStateExit;
                    }
                };

                Action<Animator, string> OnFrameEvent = null;
                OnFrameEvent = delegate(Animator animator, string actionName)
                {
                    if (actionName != "Back")
                        return;

                    if (CheckHasNextAttack())
                        animator.Play("Charge");

                    animator.gameObject.GetComponent<ResendAnimEventBehaviour>().EventDelegate -= OnFrameEvent;
                };

                Animator anim = boardBeh.UnitAnims[pair.Key];
                count++;
                //等攻擊幀再扔出消除物
                boardBeh.StartCoroutine(Utils.DelayToInvokeDo(
                    delegate()
                    {
                        if (--count <= 0)
                            callback(4, 0);
                    },
                    0.11f
                ));

                anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += OnStateExit;
                anim.gameObject.GetComponent<ResendAnimEventBehaviour>().EventDelegate += OnFrameEvent;

                anim.SetTrigger("TriggerAttack");
                //GameController.Instance.AudioManager.PlayAudio("SD_generate_jewel");
                UnitSoundBehaviour us = anim.GetComponent<UnitSoundBehaviour>();
                if (us != null)
                {
                    us.PlayAttackSound();
                }
            }

            if (count == 0)//status==1表示沒有對應顏色夥伴
                callback(4, 1);
        }


        /// <summary>
        /// 消除物飛向敵人的過程
        /// </summary>
        /// <param name="callback"></param>
        private void Step5(Action<int, int> callback)
        {
            Q.Log(LogTag.Battle, "BatAtkProHel:Step5");
            int count = 0;
            Action onSubCallback = null;
            onSubCallback = delegate()
            {
                if (--count <= 0)
                {
                    callback(5, 0);
                }
            };

            foreach (KeyValuePair<ColorType, List<BaseTileBehaviour>> pair in crtAttackTiles)
            {
                if (pair.Value.Count == 0 || pair.Key == ColorType.None)
                    continue;

                count++;
                if (hasEnemy)
                    Step5a(pair.Value, onSubCallback);
                else
                    Step5b(pair.Value, onSubCallback);
            }

            if (count == 0)//所有可以打出的顏色都沒有對應夥伴
            {
                callback(5, 1);
            }
        }




        /// <summary>
        /// 有敵人：消除物飛向敵人，敵人播放受擊動畫及特效
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="crtEnemyPoint"></param>
        /// <param name="callback"></param>
        private void Step5a(List<BaseTileBehaviour> elements, Action callback)
        {
            Q.Log(LogTag.Battle, "BatAtkProHel:Step5a");
            int count = 0;
            bool firstAttck = false;
            ColorType color = elements[0].Config.ColorType;

            bool isShield = false;
            SkillConfig skillCfg = boardBeh.CrtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
            if (skillCfg != null && skillCfg.SkillColor == color)
            {
                isShield = true;
            }

            ///因為這個怪物可能在播放驚恐動作的時候加速了，所以切換受打擊動作時要改成正常速度///
            crtEnemyPoint.EnemyAnimator.speed = 1f;

            // 音阶：do, re, mi, fa, so, la, si, do_2
            string[] scaleName = { "do", "re", "mi", "fa", "so", "la", "si", "do_2" };
            // 需要播放的音階數量
            int scaleCount = 0;
            if (comboRate >= 2.0)
                scaleCount = scaleName.Length;
            else if (comboRate >= 1.5)
                scaleCount = 4;
            else if (comboRate >= 1.2)
                scaleCount = 3;
            // 音階間隔
            int interval = elements.Count / (scaleCount - 1);
            int scalePlayIndex = 0;

            Action<Animator, int> OnStateMachineExit = null;
            OnStateMachineExit = delegate(Animator elementAnimator, int stateMachinePathHash)
            {
                elementAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent -= OnStateMachineExit;

                Transform effectZidantuowei = elementAnimator.gameObject.transform.Find("EffectZidantuowei");
                if (effectZidantuowei != null)
                {
                    GameObject.Destroy(effectZidantuowei.gameObject);
                }
                //最後播放完，需要重新置為false
                elementAnimator.enabled = false;
                boardBeh.StartCoroutine(Utils.DelayNextFrameCall(delegate()
                {
                    boardBeh.GcTileGameObj(elementAnimator.gameObject);
                }));

                // 打擊音效疊加
                if (hitCount == 0)
                    GameController.Instance.AudioManager.PlayAudio("SD_attack_jewel_break_1");
                else if (count <= 1)
                    GameController.Instance.AudioManager.PlayAudio("SD_attack_jewel_break_2");

                // 播放音階音效
                if (scaleCount > 0 && scalePlayIndex < scaleCount && interval * scalePlayIndex == hitCount)
                {
                    string name = scaleName[scalePlayIndex];
                    GameController.Instance.AudioManager.PlayAudio("SD_attack_scale_" + name);
                    scalePlayIndex++;
                }

                --count;
                hitCount++;
                //存在敵人時候播攻擊效果
                if (firstAttck == false)
                {
                    //第一個打中之後敵人開始播放受擊動作
                    firstAttck = true;

                    //因為前面可能前面Step3b中已經觸發了Hit狀態，所以這裡要做個判斷
                    if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerHit");
                    }
                    else if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Weak_Idle"))
                    {
                        crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerWeakHit");
                    }
                    else if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shock"))
                    {
                        string aniName = IsEnemyWeak() ? "TriggerWeakHit" : "TriggerHit";
                        crtEnemyPoint.EnemyAnimator.SetTrigger(aniName);
                    }

                    if (count >= 3)
                    {
                        crtEnemyPoint.EnemyAnimator.SetBool("IsComboAttack", true);
                    }
                }
                boardBeh.EnemyHitImmuneEff(color);
                boardBeh.HitEffect.PlayHitEff(color);
                attackCountCallBack(hitCount, elementNum, hurtValueThisRound);

                if (count <= 0)
                {
                    if (comboRate >= 2f && !CheckHasNextAttack())
                    {
                        boardBeh.HitEffect.PlayBoomEffect(boardBeh.eleViews);
                    }
                    boardBeh.BattleAttackProOver(color);
                    crtEnemyPoint.EnemyAnimator.SetBool("IsComboAttack", false);

                    //當攻擊當前敵人只有一個消除物時，這裡需要延遲，否則可能造成狀態切換衝突
                    if (firstAttck)
                    {

                        LeanTween.delayedCall(0.1f, callback);
                    }
                    else
                    {
                        callback();
                    }
                }
            };//OnStateMachineExit

            Action actualOnShieldPathEnter = delegate()
            {
                --count;
                hitCount++;
                if (firstAttck == false)
                {
                    firstAttck = true;
                    if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerHit");
                    }
                    else if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Weak_Idle"))
                    {
                        crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerWeakHit");
                    }
                    else if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Shock"))
                    {
                        string aniName = IsEnemyWeak() ? "TriggerWeakHit" : "TriggerHit";
                        crtEnemyPoint.EnemyAnimator.SetTrigger(aniName);
                    }
                }
                boardBeh.EnemyHitImmuneEff(color);
                boardBeh.HitEffect.PlayDefenseEffect();
                attackCountCallBack(hitCount, elementNum, hurtValueThisRound);
                // 防禦音效
                GameController.Instance.AudioManager.PlayAudio("SD_attack_defense");

                if (count <= 0)
                {
                    boardBeh.BattleAttackProOver(color);
                    if (firstAttck)
                        LeanTween.delayedCall(0.1f, callback);
                    else
                        callback();
                }
            };

            Action<Animator, AnimatorStateInfo, int> OnShieldPathEnter = null;
            OnShieldPathEnter = delegate(Animator elementAnimator, AnimatorStateInfo elementAnimatorInfo, int stateMachinePathHash)
            {
                if (!elementAnimatorInfo.IsName("MiddleState"))
                    return;

                elementAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateEnterEvent -= OnShieldPathEnter;
                boardBeh.StartCoroutine(Utils.DelayNextFrameCall(actualOnShieldPathEnter));
            };//OnShieldPathEnter

            Action<Animator, int> OnShieldPathExit = null;
            OnShieldPathExit = delegate(Animator elementAnimator, int stateMachinePathHash)
            {
                elementAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent -= OnShieldPathExit;
                elementAnimator.enabled = false;
                Transform effectZidantuowei = elementAnimator.gameObject.transform.Find("EffectZidantuowei");
                if (effectZidantuowei != null)
                {
                    GameObject.Destroy(effectZidantuowei.gameObject);
                }
                boardBeh.StartCoroutine(Utils.DelayNextFrameCall(delegate()
                {
                    boardBeh.GcTileGameObj(elementAnimator.gameObject);
                }));

            };//OnShieldPathExit

            GameController.Instance.AudioManager.PlayAudio("SD_throw_jewel");

            for (int i = 0, n = elements.Count; i < n; i++)
            {
                BaseTileBehaviour beh = elements[i];
                beh.gameObject.SetActive(true);
                Animator anim = beh.GetComponent<Animator>();
                Q.Assert(anim != null);
                count++;

                //拖尾特效
                UnityEngine.Object ob = Resources.Load("Prefabs/Effects/tuowei/EffectZidantuowei " + ((int)beh.Config.ColorType).ToString());
                GameObject go = null;
                if (ob != null)
                {
                    go = GameObject.Instantiate(ob) as GameObject;
                    if (go != null)
                    {
                        go.transform.SetParent(beh.transform);
                        go.transform.localPosition = new Vector3(0, 0, 0);
                        go.transform.localScale = new Vector3(1, 1, 1);
                        go.SetActive(true);
                        go.name = "EffectZidantuowei";
                    }
                }

                if (isShield)
                {
                    anim.GetBehaviour<BaseStateMachineBehaviour>().StateEnterEvent += OnShieldPathEnter;
                    anim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent += OnShieldPathExit;
                }
                else
                {
                    anim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent += OnStateMachineExit;
                }

                //每一個發出都需要延遲
                boardBeh.StartCoroutine(Utils.DelayToInvokeDo(
                    delegate()
                    {
                        int pathID = UnityEngine.Random.Range(1, 4);
                        int bounce = isShield ? UnityEngine.Random.Range(1, 8) : 0;
                        anim.SetInteger("ColorType", (int)beh.Config.ColorType);
                        //隨機選擇一條路線
                        anim.SetInteger("PathID", pathID);
                        //隨機選擇一條彈開路線
                        anim.SetInteger("Bounce", bounce);
                        anim.enabled = true;
                    }, normalElimDelay * i
                ));
            }//for
        }


        /// <summary>
        /// 沒有敵人時，消除物按照相應的路線飛出，播放煙花
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="callback"></param>
        private void Step5b(List<BaseTileBehaviour> elements, Action callback)
        {
            Q.Log(LogTag.Battle, "BattleAtkProHel:Step5b 1");
            ColorType color = elements[0].Config.ColorType;

            //不存在敵人時候播煙花效果
            boardBeh.HitEffect.PlayFirework(color);
            //在這裡就gc掉消除物，播放煙花特效代替
            for (int i = 0, n = elements.Count; i < n; i++)
            {
                hitCount++;
                boardBeh.GcTileGameObj(elements[i].gameObject);
            }
            LeanTween.delayedCall(1, callback);
        }


        private void Step6(Action<int, int> callback)
        {
            Q.Log(LogTag.Battle, "BattleAtkProHel:Step6");

            Action subCallback = delegate()
            {
                callback(6, 0);
            };

            Step6a(subCallback);
        }


        /// <summary>
        /// 播放掉血效果
        /// </summary>
        /// <param name="crtEnemyPoint"></param>
        /// <param name="callback"></param>
        private void Step6a(Action callback)
        {
            Q.Log(LogTag.Battle, "BattleAtkProHel:Step6a");
            Q.Assert(hitCount == elementNum);

            ///沒有防御之前的傷害值//
            int mainColorCount = 0;
            bool beShield = false;
            int hurtBefShielded = model.BattleModel.CountEliminateHurt(null, mainColor, colorCountDict, out mainColorCount, out beShield);
            ///計算道具加成後傷害
            hurtBefShielded = GameController.Instance.PropCtr.AddHurt(hurtBefShielded);

            //model.BattleModel.AddScore(hurtBefShielded);
            GameController.Instance.PropCtr.AddScore(model.BattleModel, hurtBefShielded);

            if (!hasEnemy)
            {
                callback();
                return;
            }


            int hurtValue = 0;
            //被傷害值佔總血量比例
            float hurtRate = 0;
            //攻擊是否被阻擋了
            bool beShielded = false;

            Unit crtEnemy = model.BattleModel.GetCrtEnemy();
            int comboLv = 0;
            float cr = 0;
            hurtValue = model.BattleModel.CalcAttackResult(crtEnemy, mainColor, colorCountDict, out comboLv, out cr, out beShielded);

            ///計算道具加成後傷害
            hurtValue = GameController.Instance.PropCtr.AddHurt(hurtValue);

            //hurtRate = (float)hurtValue / crtEnemyPoint.Config.UnitHp;
            hurtRate = (float)hurtValue / crtEnemy.HpMax;
            model.BattleModel.MinusCrtEnemyHp(hurtValue);
            //這裡需要重新賦值以下，因為可能被為null了
            crtEnemy = model.BattleModel.GetCrtEnemy();
            boardBeh.PlayBloodNumAnim(hurtValue, crtEnemy != null ? crtEnemy.Hp : 0);

            if (crtEnemy != null)
            {
                if (hurtRate > 0.25)
                {
                    boardBeh.HurtAddSkillCD();
                }

                //if (crtEnemy.Hp < crtEnemy.Config.UnitHp / 2)
                if (crtEnemy.Hp < crtEnemy.HpMax / 2)
                {
                    crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerWeakIdle");
                }
                else
                {
                    //怪物Idle狀態
                    crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerIdle");
                }

                callback();
            }
            else
            {
                Action<bool> actualOnDieStateExit = delegate(bool isWeakState)
                {
                    //每打死一個怪都更新目標請求
                    if (GameController.Instance.ModelEventSystem.OnBattleGoalComplete != null)
                        GameController.Instance.ModelEventSystem.OnBattleGoalComplete(3, model.BattleModel.CurrentGoal);

                    //                     // 第6關打死第一個怪播放台詞音效
                    //                     if (model.BattleModel.CrtStageConfig.ID == 6 && model.BattleModel.CrtEnemyIndex == 0)
                    //                     {
                    //                         GameController.Instance.AudioManager.PlayAudio("Vo_accompany_7");
                    //                     }
                    //擊退敵人獎勵
                    CreateEnemyAward();
                    crtEnemyPoint.ClearAfterDieAnim(isWeakState);
                    callback();
                };

                crtEnemyPoint.EnemyAnimator.SetBool("IsComboAttack", false);
                if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
                {
                    crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerDie");
                    UnitSoundBehaviour usb = crtEnemyPoint.EnemyAnimator.GetComponent<UnitSoundBehaviour>();
                    if (usb != null)
                    {
                        usb.PlayDieSound();
                    }

                    actualOnDieStateExit(false);
                }
                else if (crtEnemyPoint.EnemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Weak_Hit"))
                {
                    crtEnemyPoint.EnemyAnimator.SetTrigger("TriggerWeakDie");
                    UnitSoundBehaviour usb = crtEnemyPoint.EnemyAnimator.GetComponent<UnitSoundBehaviour>();
                    if (usb != null)
                    {
                        usb.PlayDieSound();
                    }
                    actualOnDieStateExit(true);
                }
                else
                {
                    //防禦性代碼
                    //極少時候會走這裡，不清楚原因
                    Q.Assert(false, GetType().Name + ":Step6a Assert 1");
                    actualOnDieStateExit(false);
                }
            }
        }


        /// <summary>
        /// 執行元素掉落，在整個攻擊過程中只會執行一次
        /// </summary>
        /// <param name="callback"></param>
        private void DropElement()
        {
            Q.Assert(isDropElementEnd, GetType().Name + ":DropElement Assert 0.1");
            isDropElementEnd = false;

            //執行掉落
            if (dropList == null)
            {
                isDropElementEnd = true;
                return;
            }

            List<List<ModelEventSystem.Move>> list = new List<List<ModelEventSystem.Move>>(dropList);
            Q.Assert(list != dropList, GetType().Name + ":DropElement Assert 1");
            // 置空，防止再次進入該列表
            dropList = null;

            boardBeh.PlusInteractLock();
            // 優化體驗：元素下落前亮起黑幕，不影響操作鎖
            boardBeh.SetLockPanelVDisplay(false);

            //這裡必須延遲調用，不然會crash，原因未明
            LeanTween.delayedCall(0.3f, delegate()
            {
                boardBeh.ActiveAllElementAnimator(true);
                DropElementHelper.Play(boardBeh, list,
                    delegate()
                    {
                        boardBeh.MinusInteractLock();

                        isDropElementEnd = true;
                        // 元素下落結束後，如果整個攻擊過程完成了，直接callback
                        if (isAttackProcessEnd && this.allProgressCallback != null)
                        {
                            this.allProgressCallback();
                        }
                    });
            });

        }


        /// <summary>
        /// 攝像機移動到下一個位置
        /// </summary>
        /// <param name="callback"></param>
        private void Step7(Action<int, int> callback)
        {
            Q.Log(LogTag.Battle, "BatAtkProHel:Step7");

            Unit enemy = model.BattleModel.GetCrtEnemy();
            if (!hasEnemy && enemy != null)
                Q.Assert(false, GetType().Name + ":Step7 Assert 1");

            //hasEnemy = true, crtEnemyPoint.Data == null
            //正常邏輯：因為殺死一個敵人後，在前面步驟已經銷毀當前敵人，但還沒來得及更改hasEnemy

            //沒有敵人，或者敵人未死，都無需移動攝像頭
            if (!hasEnemy || enemy != null)
            {
                callback(7, 0);
                return;
            }

            if (boardBeh.UIBattleBeh != null)
                boardBeh.UIBattleBeh.EnemyBloodVolume.gameObject.SetActive(false);

            boardBeh.MoveBattleCameraToNext(delegate(EnemyPoint enemyPoint)
                {
                    crtEnemyPoint = enemyPoint;
                    callback(7, 0);
                }
            );
        }


        public void Step8(Action<int, int> callback)
        {
            Q.Log(LogTag.Battle, "BatAtkProHel:Step8");
            if (throwTileList == null || throwTileList.Count == 0)
            {
                callback(8, 0);
                return;
            }

            int count = throwTileList.Count;
            GameObject[,] eleGameObjList = boardBeh.GetTypeGameObjects(TileType.Element);
            Action<TileObject> AddSkillTileEffect = delegate(TileObject tObj)
            {
                //加入技能殺光效果
                //Prefab名字不能改，美術以後會改動
                UnityEngine.Object obj = Resources.Load("Prefabs/Effects/EffectJinengtubiao");
                if (obj != null)
                {
                    GameObject gameObject = GameObject.Instantiate(obj) as GameObject;
                    gameObject.name = "EffectJinengtubiao";
                    gameObject.transform.SetParent(eleGameObjList[tObj.Row, tObj.Col].transform);
                    gameObject.transform.localPosition = Vector3.zero;
                }

                if (--count <= 0)
                    callback(8, 0);
            };

            boardBeh.StartCoroutine(WaitDropComplete(delegate()
            {
                for (int i = 0, n = throwTileList.Count; i < n; i++)
                {
                    TileObject tile = throwTileList[i];
                    Animator anim = boardBeh.UnitAnims[tile.Config.ColorType];
                    ThrowTileHelper.Play(boardBeh, tile, anim, AddSkillTileEffect);
                }
            }));
        }


        public void Step9(Action<int, int> callback)
        {
            Q.Log(LogTag.Battle, "BatAtkProHel:Step9");
            //根據需要，怪物拋出技能
            //TODO 這個邏輯應該放在BattleModel中

            if (model.BattleModel.GetCrtEnemy() == null)
            {
                model.BattleModel.EnemySkillID = -1;
                callback(9, 0);
                return;
            }

            SkillConfig EnemySkill = boardBeh.CrtEnemyPoint.GetSkillConfigByType(SkillType.Enemy);
            if (EnemySkill == null || boardBeh.RoundNum < EnemySkill.SkillCD)
            {
                model.BattleModel.EnemySkillID = -1;
                callback(9, 0);
                return;
            }

            monsterThrowTile = model.BattleModel.InvokeMonstorSkill(boardBeh.CrtEnemyPoint);

            if (monsterThrowTile == null)
            {
                callback(9, 0);
                return;
            }

            boardBeh.RoundNum = 0;
            Action MonstorThorwEffice = delegate()
            {
                callback(9, 0);
            };
            boardBeh.StartCoroutine(WaitDropComplete(delegate()
            {
                MonstorThrowTileHelper.Play(boardBeh, monsterThrowTile, MonstorThorwEffice);
            }));
        }


        private bool CalcRemainAttack(out int hurtValue)
        {
            hurtValue = 0;
            bool ret = false;
            Dictionary<ColorType, List<BaseTileBehaviour>> attackDict = new Dictionary<ColorType, List<BaseTileBehaviour>>();
            attackDict.Add(ColorType.Earth, new List<BaseTileBehaviour>());
            attackDict.Add(ColorType.Fire, new List<BaseTileBehaviour>());
            attackDict.Add(ColorType.Wood, new List<BaseTileBehaviour>());
            attackDict.Add(ColorType.Water, new List<BaseTileBehaviour>());
            attackDict.Add(ColorType.Golden, new List<BaseTileBehaviour>());
            attackDict.Add(ColorType.None, new List<BaseTileBehaviour>());
            int remainHP = 0;

            Unit enemy = model.BattleModel.GetCrtEnemy();
            hasEnemy = enemy != null;

            if (hasEnemy && !boardBeh.ShouldEnemyShow)
                hasEnemy = false;

            if (hasEnemy)
                remainHP = enemy.Hp;
            else//如果沒有敵人，把剩餘HP改為最大值，保證所有消除物都會打出
                remainHP = Int32.MaxValue;
            Q.Assert(remainHP > 0, "hasEnemy={0}", hasEnemy);
            this.elementNum = 0;
            Dictionary<ColorType, int> cCount = new Dictionary<ColorType, int>();

            //這裡包括兩個邏輯：
            //1. ColorType.None表示不是普通消除物，會一次性被消除
            //2. 沒有響應夥伴的普通消除物會一次性被消除，不會積攢攻擊力
            for (int i = 0; i <= 5; i++)
            {
                ColorType c = (ColorType)i;
                if (boardBeh.UnitAnims.ContainsKey(c))
                    continue;
                List<BaseTileBehaviour> list = colorDict[c];
                if (!cCount.ContainsKey(c))
                    cCount.Add(c, 0);

                while (list.Count > 0)
                {
                    BaseTileBehaviour beh = list[0];
                    if (beh.Config.ObjectType == TileType.Element)
                        elementNum++;
                    cCount[c]++;
                    attackDict[c].Add(list[0]);
                    list.RemoveAt(0);
                }
            }

            int comboLv = 0;
            float comboRate = 0;
            bool beShielded = false;

            //這裡先把hurtValue計算一次，因為後面的for可能不會執行，那麼hurtValue就會一直為0
            hurtValue = model.BattleModel.CalcAttackResult(
                model.BattleModel.GetCrtEnemy(), mainColor, cCount,
                out comboLv, out comboRate, out beShielded);

            ///計算道具加成後傷害
            hurtValue = GameController.Instance.PropCtr.AddHurt(hurtValue);

            //剩下的都是會歸集到對應夥伴的消除物
            for (int i = 0, n = 49; i < n; i++)
            {
                bool flg = false;
                for (int ii = 1; ii <= 5; ii++)
                {
                    ColorType c = (ColorType)ii;
                    if (colorDict[c].Count > 0)
                    {
                        BaseTileBehaviour beh = colorDict[c][0];
                        attackDict[c].Add(beh);
                        colorDict[c].RemoveAt(0);
                        flg = true;
                        ret = true;

                        if (!cCount.ContainsKey(c))
                            cCount.Add(c, 1);
                        else
                            cCount[c]++;

                        if (beh.Config.ObjectType == TileType.Element)
                            elementNum++;
                        continue;
                    }
                }
                if (!flg)
                    break;

                hurtValue = model.BattleModel.CalcAttackResult(
                    model.BattleModel.GetCrtEnemy(), mainColor, cCount,
                    out comboLv, out comboRate, out beShielded);

                ///計算道具加成後傷害
                hurtValue = GameController.Instance.PropCtr.AddHurt(hurtValue);

                if (hurtValue >= remainHP)
                    break;
            }

            colorCountDict = cCount;
            crtAttackTiles = attackDict;
            this.hitCount = 0;
            return ret;
        }


        /// <summary>
        /// 計算打死當前怪物後獲得的獎勵
        /// </summary>
        private void CreateEnemyAward()
        {
            UnitConfig uConfig = crtEnemyPoint.Config;
            if (uConfig.UnitGift.Length == 0)
                return;

            for (int i = 0, n = uConfig.UnitGift.Length; i < n; i++)
            {
                ItemQtt itemQtt = uConfig.UnitGift[i];
                model.BattleModel.AddStepAward(itemQtt.type, itemQtt.Qtt);
                Sprite awardIcon = BattleTools.GetBattleAwardIconByID(itemQtt.type);
                int min = -100 * n;
                int max = 100 * n;
                int per = (max - min) / n;
                int ramMin = min + (per * i);
                int ramMax = min + (per * (i + 1));
                int randomX = UnityEngine.Random.Range(ramMin, ramMax);
                Vector2 anchorMax = new Vector2(-1, -1);
                Vector2 anchorMin = new Vector2(-1, -1);
                Vector3 anchorPosition = new Vector3(-1, -1, -1);
                LeanTween.delayedCall(i * 0.2f, delegate()
                {
                    BattleTools.CreateFlyAward(awardIcon, boardBeh.EnemyAwardLayer, anchorMax, anchorMin, anchorPosition, randomX, itemQtt.Qtt);
                });
            }
        }


        /// <summary>
        /// 消除地形物後獲得獎勵
        /// </summary>
        /// <param name="conf"></param>
        /// <param name="refRect"></param>
        /// <param name="delay"></param>
        private void CreateObjectReward(ItemQtt[] rewards, RectTransform refRect, float delay)
        {
            //沒有獎勵，退出
            if (rewards == null || rewards.Length == 0)
                return;

            Vector2 anchorMax = refRect.anchorMax;
            Vector2 anchorMin = refRect.anchorMin;
            Vector3 anchoredPosition3D = refRect.anchoredPosition3D;

            for (int i = 0, n = rewards.Length; i < n; i++)
            {
                ItemQtt item = rewards[i];
                RewardType itemType = item.type;
                int itemNum = item.Qtt;
                // 獎勵音效
                BattleTools.PlayBattleAwarAudioByID(itemType);
                // 獎勵圖標
                Sprite rewardIcon = BattleTools.GetBattleAwardIconByID(itemType);
                if (rewardIcon == null)
                {
                    Q.Assert(false, "BatAtkHelper:CreateObjectReward assert1, id={0}", itemType);
                }

                //修改數據層
                model.BattleModel.AddStepAward(itemType, itemNum);

                if (delay == 0)
                {
                    string soundUrl = BattleTools.GetBattleAwardSoundByID(itemType);
                    GameController.Instance.AudioManager.PlayAudio(soundUrl);
                    BattleTools.CreateBoardFlyAward(rewardIcon, boardBeh.EliminateAwardLayer, anchorMax, anchorMin, anchoredPosition3D, 0, itemNum);
                }
                else
                {
                    boardBeh.StartCoroutine(Utils.DelayToInvokeDo(
                        delegate()
                        {
                            string soundUrl = BattleTools.GetBattleAwardSoundByID(itemType);
                            GameController.Instance.AudioManager.PlayAudio(soundUrl);
                            BattleTools.CreateBoardFlyAward(rewardIcon, boardBeh.EliminateAwardLayer, anchorMax, anchorMin, anchoredPosition3D, 0, itemNum);
                        }, delay
                    ));
                }
            }//foreach
        }


        public TileObject GetAfterEliminateData(TileObject oriData)
        {
            for (int i = 0, n = oriElimDatas.Count; i < n; i++)
            {
                TileObject to = oriElimDatas[i];
                //if (to.ConfigID == oriData.ConfigID && to.Row == oriData.Row && to.Col == oriData.Col)
                //    return newElimDatas[i];
                if (to.Config.ObjectType == oriData.Config.ObjectType &&
                    to.Row == oriData.Row &&
                    to.Col == oriData.Col)
                {
                    return newElimDatas[i];
                }
            }
            return null;
        }


        /// <summary>
        /// 簡化版的Step5a
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="callback"></param>
        private void __Step5a_Simple(List<BaseTileBehaviour> elements, Action callback)
        {
            for (int i = 0, n = elements.Count; i < n; i++)
            {
                elements[i].GetComponent<Animator>().enabled = false;
                Transform effectZidantuowei = elements[i].transform.Find("EffectZidantuowei");
                if (effectZidantuowei != null)
                {
                    GameObject.Destroy(effectZidantuowei.gameObject);
                }
                boardBeh.GcTileGameObj(elements[i].gameObject);
                hitCount++;
            }
            attackCountCallBack(hitCount, elementNum, hurtValueThisRound);
            LeanTween.delayedCall(0.1f, callback);
        }

        private bool CheckHasNextAttack()
        {
            if (colorDict != null)
            {
                if (colorDict.ContainsKey(ColorType.Earth) && colorDict[ColorType.Earth].Count > 0)
                    return true;
                if (colorDict.ContainsKey(ColorType.Fire) && colorDict[ColorType.Fire].Count > 0)
                    return true;
                if (colorDict.ContainsKey(ColorType.Wood) && colorDict[ColorType.Wood].Count > 0)
                    return true;
                if (colorDict.ContainsKey(ColorType.Water) && colorDict[ColorType.Water].Count > 0)
                    return true;
                if (colorDict.ContainsKey(ColorType.Golden) && colorDict[ColorType.Golden].Count > 0)
                    return true;
                if (colorDict.ContainsKey(ColorType.None) && colorDict[ColorType.None].Count > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 等待元素下落完成
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private IEnumerator WaitDropComplete(Action func)
        {
            while (!isDropElementEnd)
                yield return null;

            func();
        }

        bool IsEnemyWeak()
        {
            Unit crtEnemy = GameController.Instance.Model.BattleModel.GetCrtEnemy();

            if (crtEnemy == null)
                return false;

            //return (crtEnemy.Hp < crtEnemy.Config.UnitHp / 2);
            return (crtEnemy.Hp < crtEnemy.HpMax / 2);
        }



        /// <summary>
        /// 全屏炸彈或者菱形炸彈一圈圈網外炸
        /// 其他類型的炸彈或者消除物按連線順序消除
        /// </summary>
        /// <param name="tObj"></param>
        /// <param name="bomb"></param>
        /// <param name="mode"></param>
        /// <param name="typeBomPos"></param>
        /// <returns></returns>
        private float GetFlyDelayTime(BaseTileBehaviour tObj,
                                      BaseTileBehaviour globalBomb,
                                      ElimRangeMode mode)
        {
            float ret = 0;
            int index = elimTileBehs.IndexOf(tObj);
            Q.Assert(index >= 0);
            if (globalBomb != null)
            {
                //mode == ElimRangeMode.Diamond || mode == ElimRangeMode.Rect
                int x = Math.Abs(globalBomb.Col - tObj.Col);
                int y = Math.Abs(globalBomb.Row - tObj.Row);
                ret = x > y ? x * AREA_BOMB_DELAY : y * AREA_BOMB_DELAY;
            }
            else if (mode != ElimRangeMode.Normal)
            {
                ret = linearBombElimDelay * elimOrders[index];
            }
            else
            {
                ret = normalElimDelay * elimOrders[index];
            }

            return ret;
        }

    }
}
