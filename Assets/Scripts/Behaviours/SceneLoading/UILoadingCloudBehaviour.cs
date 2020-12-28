using UnityEngine;
using System.Collections;
using System;

public class UILoadingCloudBehaviour : MonoBehaviour {

    public Canvas canvas;

	void Start () {
        //canvas = gameObject.GetComponent<Canvas>();
        //canvas.worldCamera = Camera.main;
	}
 
	void Update ()
    {
        //canvas.worldCamera = Camera.main;
	}

    public void PlayShowCloud(Action showCallBack)
    {
        //StartCoroutine(ShowCloud(showCallBack));
        ShowCloud(showCallBack);
    }

    private void ShowCloud(Action showCallBack)
    {
        //yield return new WaitForEndOfFrame();
        gameObject.SetActive(true);
        Animator anim = gameObject.GetComponent<Animator>();
        Action<Animator, AnimatorStateInfo, int> StateExitHandler = null;
        StateExitHandler = delegate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!stateInfo.IsName("Start"))
            {
                return;
            }
            anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= StateExitHandler;
            if (showCallBack != null)
            {
                showCallBack();
            }

        };
        BaseStateMachineBehaviour baseState = anim.GetBehaviour<BaseStateMachineBehaviour>();
        anim.SetTrigger("TriggerStart");
        baseState.StateExitEvent += StateExitHandler;

        GameController.Instance.AudioManager.PlayAudio("SD_ui_cloud_close");
    }

    public void PlayHideCloud(Action callBack1, Action callBack2)
    {
        if (this.gameObject.activeInHierarchy)
        {
            Animator anim = gameObject.GetComponent<Animator>();
            Action<Animator, AnimatorStateInfo, int> StateExitHandler = null;
            StateExitHandler = delegate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                if (!stateInfo.IsName("Over"))
                {
                    return;
                }
                if (callBack2 != null)
                {
                    callBack2();
                }

                if(GameController.Instance.ModelEventSystem.OnCloudeShow != null)
                {
                    GameController.Instance.ModelEventSystem.OnCloudeShow();
                }

                GameController.Instance.SceneCtr.CloudFlag = false;
                anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= StateExitHandler;
                //gameObject.SetActive(false);
            };
            Action<Animator, AnimatorStateInfo, int> StateEnterHandler = null;
            StateEnterHandler = delegate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                if (!stateInfo.IsName("Over"))
                {
                    return;
                }
                if (callBack1 != null)
                {
                    callBack1();
                }
                if(GameController.Instance.ModelEventSystem.OnCloudeHide!=null)
                {
                    GameController.Instance.ModelEventSystem.OnCloudeHide();
                }
                anim.GetBehaviour<BaseStateMachineBehaviour>().StateEnterEvent -= StateEnterHandler;
                GameController.Instance.AudioManager.PlayAudio("SD_ui_cloud_open");
            };

            anim.SetTrigger("TriggerOver");
            BaseStateMachineBehaviour baseState = anim.GetBehaviour<BaseStateMachineBehaviour>();
            baseState.StateExitEvent += StateExitHandler;
            baseState.StateEnterEvent += StateEnterHandler;
        }
    }
}
