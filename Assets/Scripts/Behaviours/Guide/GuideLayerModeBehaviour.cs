using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Com4Love.Qmax;

public class GuideLayerModeBehaviour : MonoBehaviour  , ICanvasRaycastFilter
{

    public Image GuideImage;
    private Material m_ImageMaterial;
    public RectTransform FightAni;
    public Camera GuideCamera;
    public bool isFilter;
    private Action m_callBack;
    public Button GuideButton;

    private bool isAnimator;
    void Start()
    {
        m_ImageMaterial = new Material(GuideImage.material);
        GuideImage.material = m_ImageMaterial;      //不用共享材質 直接拷貝材質出來
        m_ImageMaterial.SetTextureOffset("_Mask", new Vector2(0, 0));
        isAnimator = false;
        moveOutScreen();
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="point">屏幕坐標</param>
    /// <param name="callBack">回調函數</param>
    public void SetData(Vector3 point, Action callBack, bool showMask = false)
    {
        //moveOutScreen();
        m_callBack = callBack;
        GuideImage.gameObject.SetActive(true);
        m_ImageMaterial = GuideImage.material;
        GuideImage.gameObject.SetActive(showMask);
        m_ImageMaterial.SetTextureOffset("_Mask", new Vector2(0, 0));

        if(!this.isAnimator)
        {
            FightAni.gameObject.SetActive(true);
            Animator fingerAnimator = FightAni.transform.Find("UIFinger").GetComponent<Animator>();
            fingerAnimator.SetTrigger("Click");
            fingerAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += onStateExit;
            this.isAnimator = true;
        }
        
        Vector2 outPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GuideImage.rectTransform, point, GuideCamera, out outPos);
        Vector3 targetLocalPos = new Vector3(outPos.x, outPos.y);
        //Vector2 tempV = m_ImageMaterial.GetTextureScale("_MainTex");
        //m_ImageMaterial.SetTextureScale("_Mask", tempV);

        //Debug.Log()

        m_ImageMaterial.SetTextureOffset("_Mask", new Vector2(-targetLocalPos.x / (GuideImage.rectTransform.sizeDelta.x), -targetLocalPos.y / GuideImage.rectTransform.sizeDelta.y));
        FightAni.localPosition = targetLocalPos;
        GuideButton.transform.localPosition = targetLocalPos;
    }

    private void onStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        Animator fingerAnimator = FightAni.transform.Find("UIFinger").GetComponent<Animator>();
        fingerAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= onStateExit;
        StartCoroutine(Utils.DelayToInvokeDo(
                delegate()
                {
                    fingerAnimator.SetTrigger("Click");
                    fingerAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += onStateExit;
                }, 1.0f
            ));

        UnityEngine.Object.FindObjectOfType<MapView>();
    }

    public void OnDestroy()
    {
        //Animator fingerAnimator = FightAni.transform.FindChild("Finger").GetComponent<Animator>();
        //if (FightAni != null && fingerAnimator.GetBehaviour<BaseStateMachineBehaviour>() != null)
        //{
        //    fingerAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent -= onStateExit;
        //}
    }

    public void OnClickHandler()
    {
        if (m_callBack != null)
        {
            m_callBack();
            moveOutScreen();
        }
    }


    private void moveOutScreen()
    {
        SetData(new Vector3(2000, 2000, 0), null);
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        return true;
    }
}
