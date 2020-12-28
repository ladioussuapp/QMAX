using Com4Love.Qmax.TileBehaviour;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Helper
{
    /// <summary>
    /// 元素掉落動作的輔助類
    /// </summary>
    public class DropElementHelper
    {
        static public float MoveSpeed = 0f;
        static public LeanTweenType MoveEaseType = LeanTweenType.linear;


        static public void Play(BoardBehaviour boardBeh,
            List<List<ModelEventSystem.Move>> moves,
            Action callback)
        {
            DropElementHelper ins = new DropElementHelper(boardBeh, moves, callback);
            ins.Play();
        }



        private BoardBehaviour boardBeh;
        private List<List<ModelEventSystem.Move>> moves;
        private Action callback;
        private GameObject[,] eleGameObjs;
        private int numRow = 0;
        private int numCol = 0;
        private int completeCount = 0;

        public DropElementHelper(BoardBehaviour boardBeh,
                                 List<List<ModelEventSystem.Move>> moves,
                                 Action callback)
        {
            this.boardBeh = boardBeh;
            this.moves = moves;
            this.callback = callback;
        }


        public void Play()
        {
            Q.Assert(moves != null, "DropElementHelper:Play Assert 1");
            if (moves == null && callback != null)
            {
                callback();
                return;
            }

            RectTransform elementLayer = boardBeh.ElementLayer;
            eleGameObjs = boardBeh.GetTypeGameObjects(TileType.Element);
            numRow = eleGameObjs.GetLength(0);
            numCol = eleGameObjs.GetLength(1);

            float offsetX = elementLayer.rect.width / numCol;
            float offsetY = elementLayer.rect.height / numRow;
            //Q.Log("onMove dropTimes={0}", moves.Count);
            completeCount = 0;

            //由於障礙物關係，有可能導致消除後沒有新的掉落
            if (moves.Count == 0 && callback != null)
            {
                callback();
                return;
            }

            int count = 0;
            for (int i = 0, n = moves.Count; i < n; i++)
            {
                List<ModelEventSystem.Move> oneDrop = moves[i];
                for (int i2 = 0, n2 = oneDrop.Count; i2 < n2; i2++)
                {
                    GameObject targetGA = null;
                    Vector3 nextPosition = new Vector3();
                    List<Vector3> path;
                    float delay = 0.0f;

                    ModelEventSystem.Move move = moves[i][i2];
                    if (move == null)
                        continue;

                    if (move.OriRow == -1)
                    {
                        targetGA = boardBeh.GetTileGameObj(-1, move.OriCol, move.NewValue);
                        RectTransform rect = targetGA.transform as RectTransform;
                        //如果是新增元素，要現將其設好位置
                        //比如掉落在(0,3)位置，則將其原點設置在(0,3)，位置在(0,3)以上一個位置。
                        SetElementAnchor(rect, 0, move.OriCol, numRow, numCol);
                        Vector3 aPos = rect.anchoredPosition3D;
                        aPos.y += offsetY;
                        rect.anchoredPosition3D = aPos;
                    }
                    else if (move.NewValue.Config.ObjectType == TileType.Element)
                    {
                        targetGA = eleGameObjs[move.OriRow, move.OriCol];
                        //記得把原位置清除
                        eleGameObjs[move.OriRow, move.OriCol] = null;
                    }
                    else if (move.NewValue.Config.ObjectType == TileType.Obstacle)
                    {
                        targetGA = boardBeh.obstacleViews[move.OriRow, move.OriCol];
                        //記得把原位置清除
                        boardBeh.obstacleViews[move.OriRow, move.OriCol] = null;
                    }
                    Q.Assert(targetGA != null, "i={0}, r={1}, c={2}", i, move.OriRow, move.OriCol);
                    Q.Assert(targetGA.transform is RectTransform);
                    nextPosition = (targetGA.transform as RectTransform).anchoredPosition3D;
                    path = new List<Vector3>();
                    delay = i * MoveSpeed;

                    int nextR = move.OriRow;
                    int nextC = move.OriCol;

                    Dictionary<string, int> collect = new Dictionary<string, int>();
                    foreach(var obj in move.collectPosition)
                    {
                        if (!collect.ContainsKey(obj.Key))
                        {
                            collect.Add(obj.Key, obj.Value);
                        }
                    }

                    //尋找下落隊列
                    for (int i3 = i, n3 = moves.Count; i3 < n3; i3++)
                    {
                        bool flg = false;
                        for (int i4 = 0, n4 = moves[i3].Count; i4 < n4; i4++)
                        {
                            ModelEventSystem.Move move2 = moves[i3][i4];
                            if (move2 == null)
                                continue;

                            foreach(var obj in move2.collectPosition)
                            {
                                if (!collect.ContainsKey(obj.Key))
                                {
                                    collect.Add(obj.Key, obj.Value);
                                }
                            }

                            if (nextR != move2.OriRow || nextC != move2.OriCol)
                                continue;

                            flg = true;
                            moves[i3][i4] = null;
                            switch (move2.Direction)
                            {
                                case Direction.D:
                                    nextPosition += new Vector3(0, -offsetY, 0);
                                    nextR++;
                                    break;
                                case Direction.DL:
                                    nextPosition += new Vector3(-offsetX, -offsetY, 0);
                                    nextR++;
                                    nextC--;
                                    break;
                                case Direction.DR:
                                    nextPosition += new Vector3(offsetX, -offsetY, 0);
                                    nextR++;
                                    nextC++;
                                    break;
                                default:
                                    Q.Assert(false);
                                    break;
                            }
                            path.Add(nextPosition);
                            break;
                        }//for 4

                        if (!flg)
                        {
                            //即使在這一輪中沒有找到， 也還要繼續找，因為可能出現下面情況：
                            //在回合1掉落了，回合2停頓，回合3繼續掉落
                            path.Add(new Vector3(-1, -1));
                        }
                    }//for 3
                    targetGA.GetComponent<BaseTileBehaviour>().Data = move.NewValue;
                    completeCount++;
                    targetGA.GetComponent<Animator>().enabled = false;
                    targetGA.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    MoveByPath(targetGA.GetComponent<BaseTileBehaviour>(), path, delay, collect, OnMoveCompleted, 0);
                    ++count;
                }//for 2

                if (count > 500)
                {
                    Q.Assert(false, "掉落死循環 DropElementHelper:Play() count>500");
                    break;
                }
            }//for 1
        }

        private void OnMoveCompleted(BaseTileBehaviour target, Dictionary<string, int> collect)
        {
            //Q.Log("onMoveComplete {0}", target);
            //移動完成之後，重新設定Anchor
            if (target.Config.ObjectType == TileType.Element)
            {
                eleGameObjs[target.Row, target.Col] = target.gameObject;
                SetElementAnchor(target.transform as RectTransform, target.Row, target.Col, numCol, numRow);
                JellyJump(target.GetComponent<Animator>(), null);
            }
            else if (target.Config.ObjectType == TileType.Obstacle)
            {
                bool isCollect = false;
                if (collect != null && collect.ContainsKey(target.Row + "$" + target.Col))
                {
                    isCollect = true;
                }
                if (!isCollect)
                {
                    boardBeh.obstacleViews[target.Row, target.Col] = target.gameObject;
                    SetElementAnchor(target.transform as RectTransform, target.Row, target.Col, numCol, numRow);
                }
                else
                {
                    boardBeh.obstacleViews[target.Row, target.Col] = null;
                    boardBeh.PlayCollectFly(target);
                }
            }
            
            if (--completeCount <= 0)
                callback();
        }


        private void JellyJumpCallback()
        {
            if (--completeCount <= 0)
                callback();
        }

        private void MoveByPath(BaseTileBehaviour eleBeh,
            List<Vector3> path,
            float delay,
            Dictionary<string, int> collect,
            Action<BaseTileBehaviour, Dictionary<string, int>> callback,
            int startIndex = 0)
        {
            Action<object> onComplete = delegate(object index)
            {
                MoveByPath(eleBeh, path, 0f, collect, callback, (int)index);
            };

            if (startIndex >= path.Count)
            {
                if (callback != null)
                    callback(eleBeh, collect);
                return;
            }

            RectTransform rectTransform = (RectTransform)(eleBeh.transform);
            //float dis = Vector3.Distance(rectTransform.anchoredPosition3D, path[startIndex]);
            //Q.Log ("MoveByPath r={0}, c={1}, index={2}, dis={3}", eleBeh.Row, eleBeh.Col, startIndex, dis);
            Vector3 nextPos = path[startIndex];

            if (nextPos.x == -1 && nextPos.y == -1)
            {
                //r=-1,c=-1標記這一輪不掉落，只停頓
                LeanTween.delayedCall(delay, delegate() { onComplete(startIndex + 1); });
            }
            else
            {
                LeanTween.move(rectTransform, path[startIndex], MoveSpeed)
                    .setEase(MoveEaseType)
                    .setDelay(delay)
                    .setOnComplete(onComplete)
                    .setOnCompleteParam(startIndex + 1);
            }
        }

        private void JellyJump(Animator anim, Action callback)
        {
            Action<Animator, AnimatorStateInfo, int> AnimStateExitEvent = null;
            AnimStateExitEvent = delegate(Animator arg1, AnimatorStateInfo arg2, int arg3)
            {
                if (!arg2.IsName("Jelly"))
                {
                    return;
                }


                arg1.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= AnimStateExitEvent;
                arg1.enabled = false;
                if (callback != null)
                    boardBeh.StartCoroutine(Utils.DelayNextFrameCall(callback));
            };

            //Q.Log("JellyJump");
            anim.enabled = true;
            BaseStateMachineBehaviour beh = anim.GetBehaviour<BaseStateMachineBehaviour>();
            if (beh != null)
            {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Jelly"))
                {
                    anim.SetTrigger("TriggerJelly");
                    anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += AnimStateExitEvent;
                    //元素掉落到目標位置之後，會抖動一下

                }
            }
            else
            {
                //某些情況下會跑到這裡，但導致元素無法掉落，但無法確定是怎麼觸發的
                Q.Assert(false, GetType().Name + ":JellyJump Assert 1");
                if (callback != null)
                    callback();
            }
        }



        private void SetElementAnchor(RectTransform rectTransform, int row, int col, int numCol, int numRow)
        {
            //設置位置
            rectTransform.localScale = new Vector3(1, 1, 1);
            float anchorOffsetX = 1.0f / numCol;
            float anchorOffsetY = 1.0f / numRow;
            rectTransform.anchorMax = new Vector2(anchorOffsetX * (col + 0.5f), 1 - anchorOffsetY * (row + 0.5f));
            rectTransform.anchorMin = rectTransform.anchorMax;
            rectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            //Q.Log("{0}, localPosition={1}, position={1}", eleBeh.name, eleBeh.transform.localPosition, eleBeh.transform.position);
        }
    }
}
