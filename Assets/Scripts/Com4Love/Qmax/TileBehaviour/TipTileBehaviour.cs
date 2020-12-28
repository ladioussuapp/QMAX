using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using System;
using UnityEngine.UI;

public class TipTileBehaviour : MonoBehaviour
{

    public GameObject element;

    public event Action<GameObject> StopCallback;

    public void Play()
    {
        Animator anim = gameObject.GetComponent<Animator>();
        if (anim != null)
        {
            Utils.ResetAnimatorParams(anim);
            anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += OnTipsStateExit;
            anim.Play("Tip");

            if (element != null)
            {
                element.GetComponent<Image>().enabled = false;
            } 
        }
    }

    public void Stop()
    {
        Animator anim = gameObject.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("StopTipTrigger");
    }

    void OnTipsStateExit(Animator elementAnimator, AnimatorStateInfo elementAnimatorInfo, int stateMachinePathHash)
    {
        if (!elementAnimatorInfo.IsName("Tip"))
            return;

        elementAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= OnTipsStateExit;
        StartCoroutine(Utils.DelayNextFrameCall(
            delegate()
            {
                gameObject.SetActive(false);
            }
        ));
    }


    void OnDisable()
    {
        if (element != null)
        {
            element.GetComponent<Image>().enabled = true;
        }

        if (StopCallback != null)
            StopCallback(gameObject);
    }

}
