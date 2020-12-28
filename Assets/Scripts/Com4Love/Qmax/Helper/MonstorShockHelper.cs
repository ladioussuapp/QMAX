using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data.VO;

namespace Com4Love.Qmax.Helper
{
    public class MonstorShockHelper
    {
        static public void MonstorShock(BoardBehaviour boardbeh, float comboRate)
        {
            Animator ani = boardbeh.CrtEnemyPoint.EnemyAnimator;

            if (ani == null)
                return;

            MonstorShock(ani, comboRate);

        }

        static public void MonstorShock(Animator ani, float comboRate, bool revert = true)
        {
            if (ani == null)
                return;
            ///combo 到了1.5播放驚嚇動畫，到達2.0播放驚嚇動畫同時播放速度增加30%///
            if (comboRate < 1.2f)
            {
                SetMonstorAnimatorSpeed(ani, 1f);

                if (revert)
                {
                    Unit crtEnemy = GameController.Instance.Model.BattleModel.GetCrtEnemy();
                    Utils.ResetAnimatorParams(ani);

                    //if (crtEnemy.Hp < crtEnemy.Config.UnitHp / 2)
                    if (crtEnemy.Hp < crtEnemy.HpMax / 2)
                    {
                        ani.Play(EnemyAnim.WEAK_IDLE);
                    }
                    else
                    {
                        ani.Play(EnemyAnim.IDLE);
                    }
                }

            }
            else if (comboRate >= 1.2f && comboRate < 1.5f)
            {
                SetMonstorAnimatorSpeed(ani, 1f);
                PlayShockAni(ani);

            }
            else if (comboRate >= 1.5f && comboRate < 2f)
            {
                SetMonstorAnimatorSpeed(ani, 2f);
                PlayShockAni(ani);

            }
            else if (comboRate >= 2f)
            {
                PlayShockAni(ani);
                SetMonstorAnimatorSpeed(ani, 4f);
            }
        }
        static public void SetMonstorAnimatorSpeed(Animator ani, float speed)
        {
            ani.speed = speed;
        }

        static void PlayShockAni(Animator ani)
        {
            if (!ani.GetCurrentAnimatorStateInfo(0).IsName("Shock"))
            {
                ani.SetTrigger("TriggerShock");
            }
        }
    }
}

