using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;

public class GuideDialogBehaviour : MonoBehaviour
{
    public Camera GuideCamera;
    public RectTransform RootLeftPanel;
    public RectTransform RootRightPanel;
    public  RectTransform Finger;
    public Transform Tips;
    private Action<bool> m_callBack;
    private DialogConfig m_config;
    public Image Mask;
    public RectTransform Light;
    private GuideDialogItemBehaviour leftPanelBeh;
    private GuideDialogItemBehaviour rightPanelBeh;
    private Material m_ImageMaterial;

    private UIBattleBehaviour UIBattleBeh;

    public static RectTransform FingerIns;

    void Awake()
    {
        //DontDestroyOnLoad(transform.gameObject);
        GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIDialog);

        if (Mask != null && Mask.material != null)
        {
            //不用共享材質，copy一個出來
            Mask.material = new Material(Mask.material);
        }
    }
    void Start()
    {
        //executeData();
        m_ImageMaterial = Mask.material;
        Vector2 tempV = m_ImageMaterial.GetTextureScale("_MainTex");
        m_ImageMaterial.SetTextureScale("_Mask", tempV);

        if(m_config.finger!=null && m_config.finger.IsShow)
        {
            Vector3 targetLocalPos = new Vector3(Finger.localPosition.x - 50, Finger.localPosition.y + 50);
            m_ImageMaterial.SetTextureOffset("_Mask", new Vector2(-targetLocalPos.x / Mask.rectTransform.sizeDelta.x, -targetLocalPos.y / Mask.rectTransform.sizeDelta.y));

            Light.localPosition = targetLocalPos;
        }
        else
        {
            m_ImageMaterial.SetTextureOffset("_Mask", new Vector2(100, 100));
        }

        GameObject UIObject = GameObject.Find("UIBattle(Clone)");
        if(UIObject!=null)
        {
            UIBattleBeh = UIObject.GetComponent<UIBattleBehaviour>();
            UIBattleBeh.PauseGuide();
        }
        FingerIns = Finger;
        FingerIns.transform.localPosition = new Vector3(FingerIns.transform.localPosition.x - 80, FingerIns.transform.localPosition.y + 50, FingerIns.transform.localPosition.z);
        FingerIns.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if(UIBattleBeh!=null)
        {
            UIBattleBeh.ResumeGuide();
        }
        if (GameController.Instance.ModelEventSystem.OnDialogHide != null)
        {
            GameController.Instance.ModelEventSystem.OnDialogHide();
        }
    }

    public void setData(DialogConfig config, Action<bool> callBack)
    {
        //config.UnitID1 = 5101;
        //config.UnitID2 = 3101;

        m_callBack = callBack;
        m_config = config;

        executeData();
    }

    private void executeData()
    {
        Mask.gameObject.SetActive(false);
        leftPanelBeh = RootLeftPanel.GetComponent<GuideDialogItemBehaviour>();
        rightPanelBeh = RootRightPanel.GetComponent<GuideDialogItemBehaviour>();

        if (m_config.UnitID1 == 0 || m_config.UnitID2 == 0)
        {
            Mask.gameObject.SetActive(true);
        }

        Tips.gameObject.SetActive(m_config.ShowTips);

        int subIndex1 = RootLeftPanel.GetSiblingIndex();
        int subIndex2 = RootRightPanel.GetSiblingIndex();

        int maxSub = Math.Max(subIndex1, subIndex2);
        int minSub = Math.Min(subIndex1, subIndex2);

        if (m_config.Location == 1)
        {
            RootLeftPanel.SetSiblingIndex(maxSub);
            RootRightPanel.SetSiblingIndex(minSub);
        }
        else if (m_config.Location == 2)
        {
            RootLeftPanel.SetSiblingIndex(minSub);
            RootRightPanel.SetSiblingIndex(maxSub);
        }

        string content1 = null;
        string content2 = null;
        if (m_config.Location == 1)
        {
            content1 = Utils.GetTextByID(m_config.LanguageID);
            //content1 = content1 + content1;
        }
        else
        {
            content2 = Utils.GetTextByID(m_config.LanguageID);
        }

        leftPanelBeh.setData(m_config.UnitID1, content1);
        rightPanelBeh.setData(m_config.UnitID2, content2);
        if(m_config.finger!=null && m_config.finger.IsShow)
        {
            //Action<Animator, AnimatorStateInfo, int> onStateExit = delegate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
            //{
            //    animator.SetTrigger("Click");
            //};
            Finger.gameObject.SetActive(m_config.ShowFinger);
            Finger.localPosition = m_config.finger.Pos;
            Animator anim = Finger.GetComponent<Animator>();
            anim.SetTrigger("Click");
            //anim.GetBehaviour<BaseStateMachineBehaviour>().StateExitEvent += onStateExit;

            Light.gameObject.SetActive(true);
        }
        else
        {
            Finger.gameObject.SetActive(false);
            Light.gameObject.SetActive(false);
        }

        if(GameController.Instance.ModelEventSystem.OnDialogShow!=null)
        {
            GameController.Instance.ModelEventSystem.OnDialogShow();
        }
    }

    public void onSkipDialog()
    {
        if (m_callBack != null)
        {
            m_callBack(true);
        }
    }

    public void onClickGuide()
    {
        if (m_callBack != null)
        {
            m_callBack(false);
        }
    }
}
