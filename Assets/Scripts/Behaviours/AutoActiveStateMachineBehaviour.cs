using UnityEngine;
using System.Collections;
using System;

public class AutoActiveStateMachineBehaviour : StateMachineBehaviour
{
    //AnimatorStateInfo LastStateInfo;
    //AnimatorStateInfo CutStateInfo;
    Action<Animator, AnimatorStateInfo, int> StateUpdateHandler;
    public Action CallBack;
    int defaultStateInfoName = 0;

    void CheckAutoInactive(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.fullPathHash == defaultStateInfoName && stateInfo.normalizedTime >= 1)
        {
            animator.enabled = false;
        }
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        //LastStateInfo = CutStateInfo;
        //CutStateInfo = stateInfo;

        if (defaultStateInfoName == 0)
        {
            defaultStateInfoName = stateInfo.fullPathHash;
        }

        if (stateInfo.fullPathHash == defaultStateInfoName)
        {
            StateUpdateHandler = CheckAutoInactive;
        }
        else
        {
            StateUpdateHandler = null;
        }
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (StateUpdateHandler != null)
        {
            StateUpdateHandler(animator, stateInfo, layerIndex);
        }
    }
}


