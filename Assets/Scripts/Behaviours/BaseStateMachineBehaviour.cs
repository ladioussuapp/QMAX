using UnityEngine;
using System.Collections;
using System;
using Com4Love.Qmax;

public class BaseStateMachineBehaviour : StateMachineBehaviour
{
    public event Action<Animator, AnimatorStateInfo, int> StateExitEvent;
    public event Action<Animator, AnimatorStateInfo, int> StateEnterEvent;
    public event Action<Animator, int> StateMachineEnterEvent;
    public event Action<Animator, int> StateMachineExitEvent;

    public void ClearEventListeners()
    {
        StateMachineExitEvent = null;
        StateMachineEnterEvent = null;
        StateEnterEvent = null;
        StateExitEvent = null;
    }

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (StateEnterEvent != null)
            StateEnterEvent(animator, stateInfo, layerIndex);
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{

    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (StateExitEvent != null)
            StateExitEvent(animator, stateInfo, layerIndex);
    }

    // OnStateMove is called before OnStateMove is called on any state inside this state machine
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
      
    //}

    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMachineEnter is called when entering a statemachine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (StateMachineEnterEvent != null)
            StateMachineEnterEvent(animator, stateMachinePathHash);
    }

    // OnStateMachineExit is called when exiting a statemachine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        if (StateMachineExitEvent != null)
            StateMachineExitEvent(animator, stateMachinePathHash);
    }
}
