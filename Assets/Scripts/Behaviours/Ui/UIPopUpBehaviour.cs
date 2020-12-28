using Com4Love.Qmax;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIPopUpBehaviour : MonoBehaviour
{
    public event Action<UIPopUpBehaviour> PopUpInComplete;
    public event Action<UIPopUpBehaviour> PopUpOutComplete;
    //public Transform Plane;
    [HideInInspector]
    public PopupID popId;
    public Animator animator;
    public Transform PanelT;
    public Transform BackT;
    public Transform CoverT;

    public Button BGButton;

    void Awake()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator)
        {
            animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += StateExitAction;
        }

        if (PanelT == null)
        {
            PanelT = transform.Find("Panel");
        }
    }

    void Start()
    {
        CanvasGroup canvasGroup = PanelT.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            PanelT.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0;
        StartCoroutine(DelayShow(canvasGroup));

        if (BGButton != null)
        {
            BGButton.onClick.AddListener(CloseTopPopup);
        }
    }

    void OnDestroy()
    {
        PopUpInComplete = null;
        PopUpOutComplete = null;

        if (animator != null && animator.GetBehaviour<BaseStateMachineBehaviour>() != null)
        {
            animator.GetBehaviour<BaseStateMachineBehaviour>().ClearEventListeners();
        }
    }
 
    public void PopUpIn()
    {
        if (animator != null)
        {
            animator.Play("PopupWindowUp");
        }
        else
        {

        }
    }

    public void PopUpOut()
    {
        if (animator != null)
        {
            animator.Play("PopupWindowDowm");
        }
        else
        {

        }
    }

    private void StateExitAction(Animator animator, AnimatorStateInfo info, int i)
    {
        //animator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= StateExitAction;
        if (info.IsName("PopupWindowUp"))
        {
            if (PopUpInComplete != null)
            {
                if (this.isActiveAndEnabled)
                {
                    StartCoroutine(Utils.DelayNextFrameCall(delegate()
                    {
                        PopUpInComplete(this);
                    }));
                }
                else
                {
                    PopUpInComplete(this);
                }
            }
        }
        else if (info.IsName("PopupWindowDowm"))
        {
            if (PopUpOutComplete != null)
            {
                if (this.isActiveAndEnabled)
                {
                    StartCoroutine(Utils.DelayNextFrameCall(delegate()
                    {
                        PopUpOutComplete(this);
                    }));
                }
                else
                {
                    PopUpOutComplete(this);
                }
            }
        }
    }

    IEnumerator DelayShow(CanvasGroup cg)
    {
        cg.alpha = 0f;
        yield return 0;
        yield return 0;
        cg.alpha = 1f;
        PopUpIn();
    }

    public void CloseTopPopup()
    {
        BGButton.onClick.RemoveAllListeners();
        GameController.Instance.Popup.CloseTopPopup();
    }
}
