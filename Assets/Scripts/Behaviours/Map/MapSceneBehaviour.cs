using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Com4Love.Qmax.Net.Protocols.Unit;
using Com4Love.Qmax;
using UnityEngine.EventSystems;

/// <summary>
/// 地圖場景的行為類
/// </summary>
public class MapSceneBehaviour : MonoBehaviour
{
    public Camera MapCamera;
    public MapView MapView;
    public GameObject MapRootGO;
    public EventSystem EventSystem;

    // 在mapview中會在start裡面做一些事情，需要保證此邏輯先執行
    void Awake()
    {
        if (GameController.Instance.PlayerCtr.PlayerData.passStageId == 9 ||
            GameController.Instance.PlayerCtr.PlayerData.passStageId == 3 ||
            GameController.Instance.PlayerCtr.PlayerData.passStageId == 1)
        {
            if (!GuideManager.getInstance().IsGuideOver())
            {
                //新手引導的原因需要停止地圖滾動
                GameController.Instance.Model.IsStagePassedInLastFight = false;
            }
        }
        //當關卡達到第九關時未因為缺少資料而卡住，直接於客戶端賦值，使狀態達到第10關來避免。
        if (GameController.Instance.PlayerCtr.PlayerData.passStageId == 9) {
            GameController.Instance.PlayerCtr.PlayerData.passStageId = 10;
        }
        
    }

    void Start()
    {
        //之前的邏輯 從抽獎場景，夥伴場景返回地圖場景的時候，都會走一遍新手引導邏輯。
        //夥伴場景和抽獎場景被做成了prefab直接打開，所以需要檢測這倆窗口的關閉事件，手動執行新手引導邏輯
        GameController.Instance.Popup.OnCloseComplete += Popup_OnCloseComplete;
        GameController.Instance.Popup.OnOpenComplete += Popup_OnOpenComplete;
        GameController.Instance.ViewEventSystem.MoveToMapLvl += MoveToMapLvl;

        if (GameController.Instance.SceneCtr.CloudFlag)
        {
            //會有云層出現
            StartWaitForMoveToNext();       //等待驅動完成 需要鎖定不讓點擊UI
            StartCoroutine(EffectStart());
        }
        else
        {
            CheckGuide();
        }
    }

     void MoveToMapLvl(int mapLvl)
    {
        MapView.MoveToLvlButton(mapLvl);
    }

    private void Popup_OnOpenComplete(Com4Love.Qmax.PopupID obj)
    {
        switch (obj)
        {
            case PopupID.UIGetChance:
            case PopupID.UIUpgrad:
                //在地圖場景打開抽獎界面與夥伴界面 則在打開完成後將霧效，地圖等關閉，直到此界面被關閉
                RenderSettings.fog = false;         //霧效暫時關掉
                MapRootGO.SetActive(false);
                LayerCtrlBehaviour.ActiveLayer.NormalLayer.gameObject.SetActive(false);
                break;

            default:
                break;
        }
    }

    private void Popup_OnCloseComplete(Com4Love.Qmax.PopupID obj)
    {
        switch (obj)
        {
            case Com4Love.Qmax.PopupID.UIGetChance:
            case Com4Love.Qmax.PopupID.UIUpgrad:
                CheckGuide();       //檢查新手引導

                RenderSettings.fog = true;         //霧效暫時關掉
                MapRootGO.SetActive(true);
                LayerCtrlBehaviour.ActiveLayer.NormalLayer.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void OnDestroy()
    {
        GameController.Instance.Popup.OnCloseComplete -= Popup_OnCloseComplete;
        GameController.Instance.Popup.OnOpenComplete -= Popup_OnOpenComplete;
        GameController.Instance.ModelEventSystem.OnStarEffectOver -= OnStarEffectOver;
        GameController.Instance.ViewEventSystem.MoveToMapLvl -= MoveToMapLvl;
        GameController.Instance.ViewEventSystem.OnMoveToNextLvlComplete -= CompleteMoveToNext;
        removeFliter_v1();
        RemoveTreeFliter();
    }

    IEnumerator EffectStart()
    {
        //添加雲層的回調
        GameController.Instance.ViewEventSystem.JumpSceneHideCloudEvent(null, OnCloudEffectComplete);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        GameController.Instance.EffectProxy.ZoomScene(MapCamera, 15, null);
    }

    void OnCloudEffectComplete()
    {
        if (GameController.Instance.Model.IsStagePassedInLastFight)
        {
            //上一場解鎖了關卡 需要去驅動
            GameController.Instance.ModelEventSystem.OnStarEffectOver += OnStarEffectOver;
        }
        else
        {
            //不需要滾動 直接走新手引導
            CompleteMoveToNext();
            CheckGuide();
        }
    }

    void OnStarEffectOver()
    {
        GameController.Instance.ModelEventSystem.OnStarEffectOver -= OnStarEffectOver;
        //雲層已經打開 通知地圖層做滾動
        MapView.MoveToNextLvlButton();
        //檢查新手引導
        CheckGuide();
    }

    void StartWaitForMoveToNext()
    {
        GameController.Instance.ViewEventSystem.OnMoveToNextLvlComplete += CompleteMoveToNext;
        //暫時直接把 EventSystem 關掉，待改成其它的屏蔽方式TODO
        EventSystem.enabled = false;
        SimpleTouch.Instance.enabled = false;
    }

    void CompleteMoveToNext()
    {
        GameController.Instance.ViewEventSystem.OnMoveToNextLvlComplete -= CompleteMoveToNext;
        SimpleTouch.Instance.enabled = true;
        EventSystem.enabled = true;
    }
 
    private void CheckGuide()
    {
        //Debug.Log("CheckGuide");

        ////教學從第二部開始
        ////以前教學第一步變成現在第四步
        //if (GuideManager.getInstance().guideIndex <= 1)
        //    GuideManager.getInstance().guideIndex = 2;

        if (CheckGuide_())
        {
            //Debug.Log("CheckGuide1");
            GuideManager.getInstance().StartGuide();
        }

        //switch (GuideManager.getInstance().version)
        //{
        //    case GuideVersion.Version_1:
        //        if (GuideManager.getInstance().guideIndex == 2)
        //        {
        //            addFliter_v1();
        //        }
        //        if (GuideManager.getInstance().guideIndex == 5)
        //            AddFliterTree();
        //        break;
        //    default:
        //        break;
        //}
        addFliter_v1();
        AddFliterTree();
    }

    bool CheckGuide_()
    {
        if (GuideManager.getInstance().CurrentGuideID() == 1)
        {
            if (GameController.Instance.PlayerCtr.PlayerData.passStageId == 1)
                return true;
            else if (GameController.Instance.PlayerCtr.PlayerData.passStageId > 1)
            {
                GuideManager.getInstance().SaveAndGotoNext();
            }
        }

        if (GuideManager.getInstance().CurrentGuideID() == 3)
        {
            if (GameController.Instance.PlayerCtr.PlayerData.passStageId == 3)
                return true;
            else if (GameController.Instance.PlayerCtr.PlayerData.passStageId > 3)
            {
                GuideManager.getInstance().SaveAndGotoNext();
            }
        }

        if (GuideManager.getInstance().CurrentGuideID() == 4)
        {
            if (GameController.Instance.PlayerCtr.PlayerData.passStageId == 9)
                return true;
            else if (GameController.Instance.PlayerCtr.PlayerData.passStageId > 9)
            {
                GuideManager.getInstance().SaveAndGotoNext();
            }
        }

        return false;
    }

    private void addFliter_v1()
    {
        GuideNode node = new GuideNode();
        int modelIndex; 
        node.TargetNode = MapView.GetLvlButton(2 ,out modelIndex);
        node.TarCamera = MapCamera;
        node.index = 1;
        node.CallBack = delegate ()
        {
            GameController.Instance.ViewEventSystem.ClickMapBtnEvent(2);
        };
        GuideManager.getInstance().addGuideNode("Chapter2Model2Lvl2", node);
    }

    void AddFliterTree()
    {
        Com4Love.Qmax.Data.Config.StageConfig sta = GameController.Instance.StageCtr.GetActiveStage(9);

        if (sta == null)
            return;

        int acid = sta.ID;
        int modelIndex;

        GuideNode node = new GuideNode();
        node.TargetNode = MapView.GetLvlButton(acid, out modelIndex);
        node.TarCamera = MapCamera;
        node.index = 1;
        node.ShowMask = true;
        node.ShowTips = false;
        node.CallBack = delegate ()
        {
        };
        GuideManager.getInstance().addGuideNode("TreeActive", node);

        GuideNode node1 = new GuideNode();
        node1.TargetNode = null;
        node1.TarCamera = MapCamera;
        node1.index = 3;
        node1.ShowTips = true;
        node1.CallBack = delegate ()
        {
        };
        GuideManager.getInstance().addGuideNode("TreeActiveSay", node1);
    }

    private void removeFliter_v1()
    {
        GuideManager.getInstance().removeGuideNode("Chapter2Model2Lvl2");

    }

    void RemoveTreeFliter()
    {
        GuideManager.getInstance().removeGuideNode("TreeActive");
        GuideManager.getInstance().removeGuideNode("TreeActiveSay");
    }
}
