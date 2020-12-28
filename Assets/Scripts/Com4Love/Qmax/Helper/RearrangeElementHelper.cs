using Com4Love.Qmax.Data.VO;
using Com4Love.Qmax.TileBehaviour;
using System;
using UnityEngine;

namespace Com4Love.Qmax.Helper
{
    /// <summary>
    /// 把棋盤元素重新排列
    /// </summary>
    /// Model層判斷需要Rearrange，重新組合出一個可以有連接的情況（此時model層數據已經更新）
    /// Model層發出事件，RearrangeElementHelper根據事件作出位移動畫
    public class RearrangeElementHelper
    {
        //const float MoveSpeedPerTile = 9f;
        static public LeanTweenType MoveEaseType = LeanTweenType.linear;
        const float duration = .4f;     //速度一樣

        static public void Play(TileObject[,] newElements,
            Vector2[,] oriPositions,
            int numCol, int numRow,
            RectTransform elementLayer,
            GameObject[,] elementGAs,
            Action callback)
        {
            float offsetX = elementLayer.rect.width / numCol;
            float offsetY = elementLayer.rect.height / numRow;
            int completeCount = 0;

            Action<object> onComplete = delegate(object transform)
            {
                RectTransform rectTransform = transform as RectTransform;
                BaseTileBehaviour tileBeh = rectTransform.GetComponent<BaseTileBehaviour>();
                elementGAs[tileBeh.Row, tileBeh.Col] = tileBeh.gameObject;
                //設置位置
                rectTransform.localScale = new Vector3(1, 1, 1);
                float anchorOffsetX = 1.0f / numCol;
                float anchorOffsetY = 1.0f / numRow;
                rectTransform.anchorMax = new Vector2(anchorOffsetX * (tileBeh.Col + 0.5f), 1 - anchorOffsetY * (tileBeh.Row + 0.5f));
                rectTransform.anchorMin = rectTransform.anchorMax;
                rectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
                Animator animator = rectTransform.GetComponent<Animator>();

                if (animator)
                {
                    JellyJump(animator, null);
                }

                if (--completeCount > 0)
                    return;

                if (callback != null)
                    callback();
            };

            for (int r = 0; r < numRow; r++)
            {
                for (int c = 0; c < numCol; c++)
                {
                    TileObject tileObject = newElements[r, c];
                    if (tileObject == null)
                        continue;
                    //[r, c]位置的元素原來的位置
                    int oriRow = (int)oriPositions[r, c].x;
                    int oriCol = (int)oriPositions[r, c].y;
                    RectTransform tileTransfrom = elementGAs[oriRow, oriCol].transform as RectTransform;
                    tileTransfrom.GetComponent<BaseTileBehaviour>().Data = tileObject;
                    Vector3 newPosition = tileTransfrom.anchoredPosition3D;
                    //Q.Log(LogTag.Test, "oriRow={0}, oriCol={1}, pos={2}", oriRow, oriCol, newPosition);
                    newPosition.x += (c - oriCol) * offsetX;
                    newPosition.y -= (r - oriRow) * offsetY;
                    
                    //Q.Log(LogTag.Test, "r={0}, c={1}, pos={2}", r, c, newPosition);
 
                    completeCount++;
                    LeanTween.move(tileTransfrom, newPosition, duration)
                        .setEase(MoveEaseType)
                        .setOnComplete(onComplete, tileTransfrom);
                }
            }//for
        }//Play

        private static void JellyJump(Animator anim, Action callback)
        {
            Action<Animator, AnimatorStateInfo, int> AnimStateExitEvent = null;
            AnimStateExitEvent = delegate(Animator arg1, AnimatorStateInfo arg2, int arg3)
            {
                if (!arg2.IsName("Jelly"))
                    return;

                arg1.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= AnimStateExitEvent;
                arg1.enabled = false;
            };

            //Q.Log("JellyJump");
            anim.enabled = true;
            BaseStateMachineBehaviour beh = anim.GetBehaviour<BaseStateMachineBehaviour>();
            if (beh != null)
            {
                anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += AnimStateExitEvent;
 
                anim.SetTrigger("TriggerJelly");
            }
            else
            {
                if (callback != null)
                    callback();
            }
        }
    }
}
