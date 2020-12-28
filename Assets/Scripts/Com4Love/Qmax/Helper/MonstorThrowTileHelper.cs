/********************************************************************************
** auth： johnsonybq
** date： 2015/7/13 星期一 11:32:04
** FileName：MonstorThrowTileHelper
** desc： 尚未編寫描述
** Ver.:  V1.0.0
*********************************************************************************/

using Com4Love.Qmax.Data.VO;
using System;
using UnityEngine;

namespace Com4Love.Qmax.Helper
{
    public class MonstorThrowTileHelper
    {
        static public void Play(BoardBehaviour boardBeh,
                            TileObject tileObj,
                                Action callback)
        {
            Q.Log("MonstorThrowTileHelper:Play 1");
            if (tileObj.Config.ObjectType == TileType.Obstacle)
            {
                GameController.Instance.Model.BattleModel.SetDataAt(null, tileObj.Row, tileObj.Col, TileType.Element);
            }



            Animator monsterAnim = boardBeh.CrtEnemyPoint.Enemy.gameObject.GetComponent<Animator>();

            Action<Animator, AnimatorStateInfo, int> OnIdleStateEnter = null;
            OnIdleStateEnter = delegate(Animator arg1, AnimatorStateInfo arg2, int arg3)
            {
                Q.Log("MonstorThrowTileHelper:Play 4");
                arg1.GetBehaviour<BaseStateMachineBehaviour>().StateEnterEvent -= OnIdleStateEnter;
                Step1(boardBeh, tileObj, arg1, callback);
            };

            if (monsterAnim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            {
                Q.Log("MonstorThrowTileHelper:Play 2");
                monsterAnim.GetBehaviour<BaseStateMachineBehaviour>().StateEnterEvent += OnIdleStateEnter;
            }
            else if (monsterAnim.GetCurrentAnimatorStateInfo(0).IsName("Weak_Hit"))
            {
                Q.Log("MonstorThrowTileHelper:Play 3");
                monsterAnim.GetBehaviour<BaseStateMachineBehaviour>().StateEnterEvent += OnIdleStateEnter;
            }
            else
            {
                Step1(boardBeh, tileObj, monsterAnim, callback);
            }
        }


        static private void Step1(BoardBehaviour boardBeh, TileObject tileObj, Animator monsterAnim, Action callback)
        {
            //Action<Animator, string> OnAttackFrame = null;
            //OnAttackFrame = delegate(Animator anim, string name)
            //{
            //    Q.Log("MonstorThrowTileHelper:OnAttackFrame");
            //    anim.GetComponent<ResendAnimEventBehaviour>().EventDelegate -= OnAttackFrame;
            //    Step2(boardBeh, tileObj, monsterAnim, callback);
            //};
            //monsterAnim.GetComponent<ResendAnimEventBehaviour>().EventDelegate += OnAttackFrame;

            if (monsterAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                Q.Log("MonstorThrowTileHelper:Step1 1");
                monsterAnim.SetTrigger("TriggerAttack");
            }
            else if (monsterAnim.GetCurrentAnimatorStateInfo(0).IsName("Weak_Idle"))
            {
                Q.Log("MonstorThrowTileHelper:Step1 2");
                monsterAnim.SetTrigger("TriggerWeakAttack");
            }
            //else
            //{
            //    Q.Assert(false, "少數情況下會跑到這裡。此時不播放攻擊動作直接發動技能");
            //    monsterAnim.GetComponent<ResendAnimEventBehaviour>().EventDelegate -= OnAttackFrame;
            //    Step2(boardBeh, tileObj, monsterAnim, callback);
            //}

            boardBeh.StartCoroutine(Utils.DelayToInvokeDo(
                delegate()
                {
                    Step2(boardBeh, tileObj, monsterAnim, callback);
                }, 0.11f
            ));
            Q.Log("MonstorThrowTileHelper:Step 1");
        }


        static private void Step2(BoardBehaviour boardBeh,
            TileObject tileObj, Animator unitAnim,
            Action callback)
        {
            Camera battleModelCamera = boardBeh.CrtBattleBeh.Camera;
            Camera panelCamera = boardBeh.BoardCamera;

            RectTransform targetLayer = null;
            GameObject[,] targetGameObjs = null;
            if (tileObj.Config.ObjectType == TileType.Obstacle)
            {
                targetLayer = boardBeh.ObstacleLayer;
                targetGameObjs = boardBeh.GetTypeGameObjects(TileType.Obstacle);                
                GameController.Instance.Model.BattleModel.SetDataAt(null, tileObj.Row, tileObj.Col, TileType.Element);
            }
            else if (tileObj.Config.ObjectType == TileType.Cover)
            {
                targetLayer = boardBeh.CoverLayer;
                targetGameObjs = boardBeh.GetTypeGameObjects(TileType.Cover);
            }
            GameObject[,] eleGameObjs = boardBeh.GetTypeGameObjects(TileType.Element);
            RectTransform flyLayer = boardBeh.FlyLayer;

            //SkillLoadingBehaviour skillLoading = boardBeh.SkillLoadings[tileObj.Config.ColorType];

            string attackSound = "SD_attack_spit";
            GameController.Instance.AudioManager.PlayAudio(attackSound);
            GameObject go = unitAnim.gameObject;
            RectTransform eleRect = boardBeh.GetTileGameObj(tileObj.Row, tileObj.Col, tileObj).transform as RectTransform;
            //飛行層
            eleRect.transform.SetParent(flyLayer);
            //Animator anim = eleRect.GetComponent<Animator>();
            //anim.enabled = false;
            //消除物初始化是會在棋盤上正確的位置，把這個位置保存，作為飛行的目標位置
            Vector3 oriLocalPos = eleRect.localPosition;
            //將消除物設定到夥伴的位置
            Transform targetUnitRect = go.transform;
            //將夥伴的位置轉換到世界坐標
            Vector3 unitWorldPoint = targetUnitRect.TransformPoint(0, 0, 0);
            //將世界坐標轉換為屏幕坐標
            Vector3 unitScreenPoint = battleModelCamera.WorldToScreenPoint(unitWorldPoint);
            Vector2 tempPoint;
            //將屏幕坐標轉換為elementLayer的本地坐標Vector2
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetLayer, unitScreenPoint, panelCamera, out tempPoint);
            //Vector2 -> Vector3
            Vector3 targetLocalPos = new Vector3(tempPoint.x, tempPoint.y + 100);
            eleRect.localPosition = targetLocalPos;
            Action<object> onChargeComplete = delegate(object rectTransform)
            {
                eleRect.SetParent(targetLayer);
                Utils.SetCurrentPositoinToAnchor(eleRect);
                if (eleGameObjs[tileObj.Row, tileObj.Col] != null)
                {
                    if (tileObj.Config.ObjectType != TileType.Cover)
                    {

                        if (eleGameObjs[tileObj.Row, tileObj.Col] == null)
                        {
                            GameObject.Destroy(eleGameObjs[tileObj.Row, tileObj.Col]);
                        }
                        else
                        {
                            boardBeh.GcTileGameObj(eleGameObjs[tileObj.Row, tileObj.Col]);
                        }
                    }
                    if (tileObj.Config.ObjectType == TileType.Cover)
                    {
                        GameObject ga = eleGameObjs[tileObj.Row, tileObj.Col];
                        if (ga != null)
                        {
                            //怪物扔遮蓋物（比如蜘蛛網）到元素上時，元素會抖動一下
                            ga.GetComponent<Animator>().SetTrigger("TriggerJelly");
                        }
                    }
                    targetGameObjs[tileObj.Row, tileObj.Col] = eleRect.gameObject;
                }
                if (callback != null)
                    callback();
            };

            Vector3[] arr1 = {
                                   eleRect.localPosition - new Vector3(100, 100, 0),
                                   //eleRect.localPosition - new Vector3(-100, -100, 0),
                                   //eleRect.localPosition - new Vector3(200, -100, 0),
                                   //eleRect.localPosition - new Vector3(-200, -100, 0),
                               };
            Vector3 minPoint = arr1[UnityEngine.Random.Range(0, arr1.Length)];
            Vector3[] arr = { 
                                  eleRect.localPosition,
                                  minPoint,
                                  minPoint,
                                  oriLocalPos, 
                              };
            //飛到棋盤上製定位置
            //如果激活了Animator，且Animator.CullingMode==AnimatorCullingMode.AlwaysAnimate
            //會導致無法設置eleRect的Transfrom。
            //因此運動前先將其enabled=false，運動完再設為true
            LeanTween.moveLocal(eleRect.gameObject, arr, 0.2f)
                .setOnComplete(onChargeComplete)
                .setOnCompleteParam(eleRect);
        }

    }
}
