using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.VO;
using System.Collections.Generic;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Ctr;

public class UIUnlock : MonoBehaviour {
    public UIButtonBehaviour CloseButton;
    public UIButtonBehaviour UnLockButton;
    public Text GemText;
    public UIUnlockListItem[] Items;
    public Animator animator;
    private StageConfig stageConfig;
    [HideInInspector]
    public bool NeedGem4Unlock;     //是否需要鑽石去解鎖
    //public Transform GemGroup;
    public Transform TextOr;
    public Transform GemIcon;
    public RectTransform BgImage;

    [HideInInspector]
    Vector3 UnLockButtonPos = new Vector3(0, -495.4f,0);
    [HideInInspector]
    Vector3 OrImagePos = new Vector3(0,-355f,0);
	// Use this for initialization
	void Start () {
        CloseButton.onClick += CloseButton_onClick;
        UnLockButton.onClick += UnLockButton_onClick;

        foreach (UIUnlockListItem item in Items)
        {
            item.OnEnterClick += item_OnEnterClick;
        }

#if AUTO_FIGHT
        //test-------------
        Invoke("Test", 2f);
#endif


	}

    #if AUTO_FIGHT
    void Test()
    {
        UnLockButton_onClick(UnLockButton);
    }
#endif



    void UnLockButton_onClick(UIButtonBehaviour button)
    {
        if (NeedGem4Unlock && stageConfig.CostGem.Qtt > GameController.Instance.PlayerCtr.PlayerData.gem)
        {
            //錢不夠
            UIShop.Open(UIShop.TapIndex.GEM_INDEX);
            return;
        }
        else
        {
            UnLockButton.onClick -= UnLockButton_onClick;

            GameController.Instance.StageCtr.GemUnLockStage(stageConfig.ID);
            GameController.Instance.ModelEventSystem.OnStageUnlocked += ModelEventSystem_OnStageUnlocked;
        }
    }

    void ModelEventSystem_OnStageUnlocked(int obj)
    {
        GameController.Instance.ModelEventSystem.OnStageUnlocked -= ModelEventSystem_OnStageUnlocked;
        animator.SetTrigger("Start");

        StartCoroutine(AutoClose());
    }
 
    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(2.4f);

        GameController.Instance.Popup.Close(PopupID.UIUnlock);
    }

    void OnDestroy()
    {
        CloseButton.onClick -= CloseButton_onClick;
        GameController.Instance.ModelEventSystem.OnStageUnlocked -= ModelEventSystem_OnStageUnlocked;

        foreach (UIUnlockListItem item in Items)
        {
            item.OnEnterClick -= item_OnEnterClick;
        }
    }

    void item_OnEnterClick(StageLockInfo obj)
    {
        //1.過完前一關
        //2.VIP等級
        //3.特殊條件
        //4.夥伴數量
        //5.夥伴等級
        //6.星星數量
        //跳轉到各種地方
        switch (obj.Type)
        {
            case 2:
                break;
            case 3:
                break;
            case 4:
                //跳轉抽獎場景
                GameController.Instance.Popup.Close(PopupID.UIUnlock,false);
                //GameController.Instance.SceneCtr.LoadLevel(Scenes.GetChance);
                GameController.Instance.Popup.Open(PopupID.UIGetChance);
                break;
            case 5:
                //跳轉夥伴場景
                GameController.Instance.Popup.Close(PopupID.UIUnlock,false);
                //GameController.Instance.SceneCtr.LoadLevel(Scenes.UpgradScene);
                GameController.Instance.Popup.Open(PopupID.UIUpgrad);
                break;
            case 6:
                break;
            default:
                break;
        }
    }

    void CloseButton_onClick(UIButtonBehaviour button)
    {
        GameController.Instance.Popup.Close(PopupID.UIUnlock);
    }
 
    public void SetData(List<StageLockInfo> lockInfos, bool needGem, StageConfig config)
    {
        stageConfig = config;
        NeedGem4Unlock = needGem;
        GemText.text = config.CostGem.Qtt.ToString();   //暫時直接取消費的數量
        UIUnlockListItem item;

        GemText.gameObject.SetActive(NeedGem4Unlock);
        GemIcon.gameObject.SetActive(NeedGem4Unlock);
        TextOr.gameObject.SetActive(NeedGem4Unlock);

        for (int i = 0;  i < Items.Length; i++)
        {
            item = Items[i];

            if (i < lockInfos.Count)
            {
                item.gameObject.SetActive(true);
                item.SetData(lockInfos[i]);
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
        LayoutElement layout =Items[0].GetComponent<LayoutElement>();
        float high = layout == null ? 0 : layout.preferredHeight;

        Vector3 movePos = new Vector3(0, (Items.Length - lockInfos.Count)*high, 0);

        UnLockButton.transform.localPosition = UnLockButtonPos + movePos;
        TextOr.transform.localPosition = OrImagePos + movePos;

        BgImage.sizeDelta = new Vector2(BgImage.sizeDelta.x, BgImage.sizeDelta.y - movePos.y);
    }
}
