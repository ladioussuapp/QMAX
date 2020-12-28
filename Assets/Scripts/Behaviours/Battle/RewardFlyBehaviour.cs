using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Com4Love.Qmax;

public class RewardFlyBehaviour : MonoBehaviour {

	// Use this for initialization
    public Image showImage;
    public Text showNumText;

    private Sprite iconSpite;
    private int rewardNum;

	void Start () {
        if(iconSpite != null)
        {
            showImage.sprite = iconSpite;
            showNumText.text = rewardNum.ToString();
        }

        Animator animator = this.GetComponent<Animator>();
        if (animator != null)
        {
            BaseStateMachineBehaviour machineBeh = animator.GetBehaviour<BaseStateMachineBehaviour>();
            if (machineBeh != null)
            {
                Action<Animator, AnimatorStateInfo, int> stateExit = null;
                stateExit = delegate (Animator ani, AnimatorStateInfo stateInfo, int layerIndex)
                {
                    if (!stateInfo.IsName("RewardFlyType2"))
                        return;

                    BaseStateMachineBehaviour stateBeh = ani.GetBehaviour<BaseStateMachineBehaviour>();
                    if (stateBeh != null)
                    {
                        stateBeh.StateExitEvent -= stateExit;
                    }
                    this.transform.localPosition = new Vector3(-10000, -10000, 0);
                    StartCoroutine(Utils.DelayDeactive(this.gameObject));
                };
                machineBeh.StateExitEvent += stateExit;
            }
        }

    }

    public void SetData(Sprite icon, int RewardNum = 1)
    {
        iconSpite = icon;
        rewardNum = RewardNum;
        showImage.sprite = iconSpite;
        showNumText.text = rewardNum.ToString();
    }
	
}
