using System;
using UnityEngine;

namespace Com4Love.Qmax.Helper
{
    /// <summary>
    /// 棋盘初始化掉落过程
    /// </summary>
    public class BattleInitDropHelper
    {
        static public void Play(BoardBehaviour boardBeh,
            RectTransform elementLayer,
            GameObject[,] eleBehaviours,
            Action callback)
        {
            int animCount = 0;

            Action<Animator, AnimatorStateInfo, int> StateExitEventDelegate = null;
            StateExitEventDelegate = delegate(Animator animator, AnimatorStateInfo stateInfo, int arg)
            {
                if (!stateInfo.IsName("InitDrop"))
                {
                    return;
                }
                   

                animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= StateExitEventDelegate;
                animator.enabled = false;

                if (--animCount <= 0 && callback != null)
                {
                    boardBeh.StartCoroutine(Utils.DelayNextFrameCall(callback));
                }
            };

            for (int r = 0, numRow = eleBehaviours.GetLength(0); r < numRow; r++)
            {
                for (int c = 0, numCol = eleBehaviours.GetLength(1); c < numCol; c++)
                {
                    GameObject ga = eleBehaviours[r, c];
                    if (ga == null)
                        continue;
                    RectTransform rect = ga.transform as RectTransform;
                    Animator animator = rect.GetComponent<Animator>();
                    animator.enabled = true;
                    //Utils.ResetAnimatorParams(animator);
                    //animator.Play("InitDrop");
                    animator.SetTrigger("TriggerInitDrop");
                    animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += StateExitEventDelegate;
                    animCount++;
                }//for
            }//for
        }
    }
}
