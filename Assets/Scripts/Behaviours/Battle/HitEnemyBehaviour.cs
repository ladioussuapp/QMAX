using Com4Love.Qmax;
using System;
using UnityEngine;

/// <summary>
/// 控制當前的怪物受擊、掉血、扔技能的邏輯
/// </summary>
public class HitEnemyBehaviour : MonoBehaviour
{
    /// <summary>
    /// 怪物受擊結束，調用該事件
    /// </summary>
    /// <param>是否死亡</param>
    public event Action<bool> HitCompleteEvent;

    /// <summary>
    /// 掉血動畫
    /// </summary>
    public Animator BloodNumAnim;

    public UIBattleBehaviour BattleUI;

    public ViewEventSystem viewEvtSys;

    /// <summary>
    /// 播放連續受擊以及掉血動畫
    /// 當crtHitNum==totalHitNum，認為攻擊完成
    /// </summary>
    /// <param name="crtHitNum">當前攻擊計數，[1, totalHitNum]</param>
    /// <param name="totalHitNum">總共攻擊次數</param>
    /// <param name="dropHp">掉血量</param>
    public void PlayHit(EnemyPoint ep, int crtHitNum, int totalHitNum, int dropHp,
        bool isWeakState, bool isKilled)
    {
        Q.Log("EnemyHitBehaviour:PlayHit crt={0}, total={1}", crtHitNum, totalHitNum);
        if (ep.EnemyAnimator == null)
        {
            PlayBloodAnim(dropHp);
            return;
        }

        Animator anim = ep.EnemyAnimator;
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        if (crtHitNum == 1)
        {
            Q.Assert(
                info.IsName(EnemyAnim.IDLE) ||
                info.IsName(EnemyAnim.WEAK_IDLE),
                GetType().Name + ":PlayHit Assert 1");

            if (info.IsName(EnemyAnim.IDLE))
                anim.SetTrigger("TriggerHit");
            else if (info.IsName(EnemyAnim.WEAK_IDLE))
                anim.SetTrigger("TriggerWeakHit");
            else
            {
                //意外情況處理，強制跳到idle狀態
                Utils.ResetAnimatorParams(anim);
                if (!isWeakState)
                {
                    anim.Play(EnemyAnim.IDLE);
                    anim.SetTrigger("TriggerHit");
                }
                else
                {
                    anim.Play(EnemyAnim.WEAK_IDLE);
                    anim.SetTrigger("TriggerWeakHit");
                }
            }

            if (totalHitNum > 1)
            {
                //連續攻擊
                anim.SetBool("IsComboAttack", true);
            }
        }

        if (crtHitNum != totalHitNum)
            return;

        //最後一下受擊之後，根據狀態去到Idle或者死亡狀態

        PlayBloodAnim(dropHp);
        anim.SetBool("IsComboAttack", false);

        if (!isWeakState)
        {
            //怪物Idle狀態
            anim.SetTrigger("TriggerIdle");
            if (HitCompleteEvent != null)
                HitCompleteEvent(false);
            return;
        }
        else if (!isKilled)
        {
            anim.SetTrigger("TriggerWeakIdle");
            if (HitCompleteEvent != null)
                HitCompleteEvent(false);
            return;
        }

        //怪物被打死了
        Action<Animator, AnimatorStateInfo, int> OnDieStateExit = null;
        OnDieStateExit = delegate(Animator monsterAnim, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //怪物死亡動畫的回調函數
            if (!stateInfo.IsName(EnemyAnim.DIE) && !stateInfo.IsName(EnemyAnim.WEAK_DIE))
                return;

            monsterAnim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= OnDieStateExit;

            //消失
            StartCoroutine(Utils.DelayNextFrameCall(delegate() { ep.Clear(); }));

            if (HitCompleteEvent != null)
                HitCompleteEvent(true);
        };

        anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += OnDieStateExit;
        //已經把怪物打死
        if (info.IsName(EnemyAnim.HIT))
            anim.SetTrigger("TriggerDie");
        else if (info.IsName(EnemyAnim.WEAK_HIT))
            anim.SetTrigger("TriggerWeakDie");
        else
        {
            Q.Assert(false, GetType().Name + ":PlayHit Assert 2");
            anim.Play(EnemyAnim.WEAK_DIE);
        }

        //播放死亡音效
        UnitSoundBehaviour usb = anim.GetComponent<UnitSoundBehaviour>();
        if (usb != null)
        {
            usb.PlayDieSound();
        }
    }


    /// <summary>
    /// 播放掉血特效
    /// </summary>
    /// <param name="value"></param>
    /// <param name="callback"></param>
    private void PlayBloodAnim(int value, Action callback = null)
    {
        if (BloodNumAnim == null)
            return;

        if (!BloodNumAnim.enabled)
            BloodNumAnim.enabled = true;

        BloodNumAnim.GetComponent<ImageNumberBehaviour>().Number = value;

        if (!BloodNumAnim.gameObject.activeSelf)
            BloodNumAnim.gameObject.SetActive(true);

        Action<Animator, int> OnStateMachineExit = null;
        OnStateMachineExit = delegate(Animator anim, int hash)
        {
            anim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent -= OnStateMachineExit;
            StartCoroutine(Utils.DelayNextFrameCall(delegate()
            {
                BloodNumAnim.gameObject.SetActive(false);
            }));
        };

        BloodNumAnim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent += OnStateMachineExit;
        BloodNumAnim.Play("BloodNum");
    }
}
