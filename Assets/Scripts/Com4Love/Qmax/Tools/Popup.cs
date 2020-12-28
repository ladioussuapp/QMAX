using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Com4Love.Qmax.Ctr;

namespace Com4Love.Qmax.Tools
{
    public class Popup
    {
        //舞台上的東西默認從100開始
        public const int PLANE_DIS_START = 90;
        public const int DIS_GAP = 10;

        /// <summary>
        /// 新增事件，在調用打開接口之後就會派發
        /// </summary>
        public event Action<PopupID> OnOpen;
        /// <summary>
        /// 關閉動作完成   
        /// @param:popupid
        /// </summary>
        public event Action<PopupID> OnCloseComplete;
        public event Action<PopupID> OnOpenComplete;
        public LayerCtrlBehaviour LayerCtrlBeh;
        //彈出順序隊列
        protected List<PopupID> popUpQueue;
        
        protected Dictionary<PopupID, UIPopUpBehaviour> popUps;
        private GameObject textFloatPrefab;

        /// <summary>
        /// 全局Loading的使用計數，當值>0時顯示全局Loading；==0時隱藏。
        /// </summary>
        private OperLock loadingLock = new OperLock();

        private Dictionary<PopupID, Action> PopupCloseEvent = new Dictionary<PopupID, Action>();

        /// <summary>
        /// 彈窗是否可以點擊空白區域關閉///
        /// </summary>
        private Dictionary<PopupID, bool> IsCanCloseByBG = new Dictionary<PopupID, bool>();

        public Popup()
        {
            popUps = new Dictionary<PopupID, UIPopUpBehaviour>();
            popUpQueue = new List<PopupID>();
            OnOpenComplete += SendUIMessage;
        }

        /// <summary>
        /// 顯示輕量Loadig
        /// </summary>
        public void ShowLightLoading()
        {
            if (loadingLock.PlusLock() == 1)
            {
                if (!IsPopup(PopupID.LightLoading))
                {
                    Open(PopupID.LightLoading, null, false , false , 0);
                }
            }
            return;
        }


        /// <summary>
        /// 隱藏輕量Loading
        /// </summary>
        public void HideLightLoading()
        {
            if (loadingLock.GetValue() == 0)
            {
                Q.Log(LogTag.Error, "HideLightLoading() fails. loadingLock==0");
                return;
            }

            if (loadingLock.MinusLock() <= 0)
            {
                Close(PopupID.LightLoading, false);
            }

            Q.Log("HideLightLoading() loadingLock={0}", loadingLock.GetValue());
            return;
        }


        /// <summary>
        /// 彈出一個文字浮層。然後就不用管他了，自動會消失的。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parent">在哪個層級彈出</param>
        public void ShowTextFloat(string text, RectTransform parent)
        {
            TextFloatBehaviour textFloatBehaviour = CreateTextFloat(parent).GetComponent<TextFloatBehaviour>();
            textFloatBehaviour.GetComponent<TextFloatBehaviour>().SetText(text);
            textFloatBehaviour.SetBgVisbile(true);
        }

        /// <summary>
        /// 同上。用圖片代替文字顯示，其餘不變
        /// </summary>
        /// <param name="text"></param>
        /// <param name="parent"></param>
        public void ShowTextFloat(Sprite text, RectTransform parent, bool hideBg = true)
        {
            TextFloatBehaviour textFloatBehaviour = CreateTextFloat(parent).GetComponent<TextFloatBehaviour>();
            textFloatBehaviour.SetTextSprite(text);
            textFloatBehaviour.SetBgVisbile(!hideBg);
        }

        public Transform ShowTextFloat(RectTransform parent,string prefabsRes)
        {
            return CreateTextFloat(parent, prefabsRes);
        }

        RectTransform CreateTextFloat(RectTransform parent)
        {
            if (textFloatPrefab == null)
            {
                textFloatPrefab = Resources.Load<GameObject>("Prefabs/Ui/TextFloat");
            }

            GameObject ga = UnityEngine.Object.Instantiate<GameObject>(textFloatPrefab);
            ga.SetActive(true);
            RectTransform rect = ga.transform as RectTransform;
            rect.SetParent(parent);
            rect.localScale = new Vector3(1, 1, 1);
            rect.anchoredPosition3D = Vector3.zero;

            return rect;
        }

        RectTransform CreateTextFloat(RectTransform parent, string userFloatWin)
        {
            GameObject userTextFloatPrefab = Resources.Load<GameObject>(userFloatWin);

            GameObject ga = UnityEngine.Object.Instantiate<GameObject>(userTextFloatPrefab);
            ga.SetActive(true);
            RectTransform rect = ga.transform as RectTransform;
            rect.SetParent(parent);
            rect.localScale = new Vector3(1, 1, 1);
            rect.anchoredPosition3D = Vector3.zero;

            return rect;
        }

        /// <summary>
        /// 有沒有東西被彈出
        /// </summary>
        public bool HasPopup
        {
            get
            {
                return popUps.Count > 0;
            }
        }


        private Transform GetUIPrefab(PopupID popUpId, Camera camera = null)
        {
            Transform uiPrefab = CreateUiPrefab(popUpId);
            Canvas canvas = uiPrefab.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = camera != null ? camera : Camera.main;
                //canvas.sortingOrder = sortingOrder;
            }
            uiPrefab.name = popUpId.ToString();
            return uiPrefab;
        }

        /// <summary>
        /// 彈出邏輯 每個彈出窗口，自身就擁有一個或多個canvas
        /// 
        /// </summary>
        /// <param name="popUpId"></param>
        /// <param name="camera"></param>
        /// <param name="animation"></param>
        /// <param name="closeByBG">彈窗是否可以點擊空白處關閉</param>
        /// <param name="opacity">背景的透明度  -1表示不指定</param>
        /// <param name="popupPrefab">包裝窗口的prefab路徑，如果再有背景透明度變化，要不要背景，等需求，直接用自己的prefab包裝。另外</param>
        /// <returns>彈出的內容（不是包裝用的prefab）</returns>
        public Transform Open(PopupID popUpId, Camera camera = null,
            bool animation = true,
            bool closeByBG = false,
            float opacity = -1f  , 
            string popupPrefab = "Prefabs/Ui/UIPopUp")
        {
            if (IsPopup(popUpId))
            {
                Q.Warning(popUpId + "已彈出");
                return GetPopup(popUpId);
            }
 
            Transform uiPopupT = GameController.Instance.QMaxAssetsFactory.CreatePrefab(popupPrefab);

            if (LayerCtrlBehaviour.ActiveLayer != null)
            {
                uiPopupT.SetParent(LayerCtrlBehaviour.ActiveLayer.PopupLayer);
            }

            UIPopUpBehaviour uiPopup = uiPopupT.GetComponent<UIPopUpBehaviour>();
            uiPopup.popId = popUpId;

            Action<UIPopUpBehaviour> onComplete = delegate(UIPopUpBehaviour uiPopUp)
            {
                if (OnOpenComplete != null)
                {
                    OnOpenComplete(uiPopUp.popId);
                }
            };

            ////如果是lightLoading界面則不顯示黑色BG///
            //直接在shoLightLoading的接口裡改，不應該在這裡寫。
            //if (popUpId == PopupID.LightLoading)
            //{
            //    uiPopup.BGButton.gameObject.SetActive(false);
            //}
            //else
            //{
            //    uiPopup.BGButton.gameObject.SetActive(true);
            //}

            Transform uiT = CreateUiPrefab(popUpId);
            PopupEventCor popcor = uiT.GetComponent<PopupEventCor>();

            if (opacity != -1)
            {
                Image UiPopupBackImage = uiPopup.CoverT.GetComponent<Image>();

                if (UiPopupBackImage)
                {
                    UiPopupBackImage.color = new Color(0, 0, 0, opacity);
                }
            }
            
            if (popcor != null)
            {
                ///點擊空白區域的關閉事件///
                if (!PopupCloseEvent.ContainsKey(popUpId))
                {
                    PopupCloseEvent.Add(popUpId, popcor.Close);
                }
            }
            //可以控制多窗口的層級顯示 不同窗口之間間隔2. 窗口與自己的背景之間，間隔1
            ///這樣寫在顯示loading彈窗在彈出其他窗口，加載完了關閉loading彈窗就會出現canvas層級一樣的///
            ///所以修改...
            ///int planeDis = PLANE_DIS_START - (popUps.Count * DIS_GAP);
            float planeDis = PLANE_DIS_START;

            if (popUps.Count != 0 && popUpQueue.Count != 0)
            {
                PopupID beID = popUpQueue[popUpQueue.Count - 1];
                UIPopUpBehaviour beUiPopup = popUps[beID];

                Canvas[] beCanvas = beUiPopup.PanelT.GetComponentsInChildren<Canvas>();

                if (beCanvas.Length > 0)
                {
                    planeDis = beCanvas[0].planeDistance - DIS_GAP;
                }
            }

            uiPopup.popId = popUpId;
            uiT.name = popUpId.ToString();
            popUps.Add(popUpId, uiPopup);
            popUpQueue.Add(popUpId);
            uiT.gameObject.SetActive(true);
            uiT.SetParent(uiPopup.PanelT);

            //添加彈窗是否可以點擊空白區域關閉///
            //為什麼不直接監聽背景層的button事件？
            //如果不需要點擊空白後關閉，直接將UIPopUpBehaviour的背景按鈕關掉就可以了
            if (!IsCanCloseByBG.ContainsKey(popUpId))
            {
                IsCanCloseByBG.Add(popUpId, closeByBG);
            }

            Canvas[] uiCanvas = uiT.GetComponentsInChildren<Canvas>();

            foreach (Canvas canvas in uiCanvas)
            {
                canvas.worldCamera = camera == null ? Camera.main : camera;
                canvas.planeDistance = planeDis;
            }

            if (uiPopup.BackT != null)
            {
                Canvas UiPopupBackC = uiPopup.BackT.GetComponent<Canvas>();
                UiPopupBackC.worldCamera = camera == null ? Camera.main : camera;
                UiPopupBackC.planeDistance = planeDis + 1;
            }

            RefPopsGraphic();

            if (OnOpen != null)
            {
                OnOpen(uiPopup.popId);
            }

            if (!animation)
            {
                //動畫部分有可能會被關掉
                if (uiPopup.animator != null)
                {
                    uiPopup.animator.enabled = false;
                }

                onComplete(uiPopup);
            }
            else
            {
                uiPopup.PopUpInComplete += onComplete;
                uiPopup.PopUpIn();
            }
 
            return uiT;
        }

        /// <summary>
        /// 測試邏輯 打開窗口和關閉窗口後刷新其它窗口及ui層的是否接受點擊
        /// </summary>
        private void RefPopsGraphic()
        {
            GraphicRaycaster raycaster;
            GraphicRaycaster[] allRaycaster;
            bool normalTouch = false;
            bool popTouch = false;

            if (HasPopup)
            {
                //舞台上UI要禁用點擊
                normalTouch = false;
            }
            else
            {
                normalTouch = true;
            }

            if (LayerCtrlBehaviour.ActiveLayer != null)
            {
                raycaster = LayerCtrlBehaviour.ActiveLayer.GetComponent<GraphicRaycaster>();

                if (raycaster != null)
                {
                    raycaster.enabled = normalTouch;
                }

                allRaycaster = LayerCtrlBehaviour.ActiveLayer.NormalLayer.GetComponentsInChildren<GraphicRaycaster>();

                for (int i = 0; i < allRaycaster.Length; i++)
                {
                    raycaster = allRaycaster[i];
                    raycaster.enabled = normalTouch;
                }
            }
            Transform popT;
            //最上面的窗口可點擊，其餘窗口不可點擊
            for (int i = 0; i < popUpQueue.Count; i++)
            {
                if (i == popUpQueue.Count - 1)
                {
                    //最後一個彈出窗口可點 其餘不可點
                    popTouch = true;
                }
                else
                {
                    popTouch = false;
                }

                popT = GetPopup(popUpQueue[i]);

                if (popT == null)
                    continue;

                allRaycaster = popT.GetComponentsInChildren<GraphicRaycaster>();

                for (int j = 0; j < allRaycaster.Length; j++)
                {
                    raycaster = allRaycaster[j];
                    raycaster.enabled = popTouch;
                }
            }
        }

        /// <summary>
        /// 關閉所有彈出Popup層，包括Loading也會被關閉
        /// </summary>
        public void CloseAll()
        {
            foreach (KeyValuePair<PopupID, UIPopUpBehaviour> item in popUps)
            {
                if (item.Value != null)
                {
                    item.Value.gameObject.SetActive(false);
                    GameObject.Destroy(item.Value.gameObject);
                }
            }

            loadingLock.Clear();
            popUps.Clear();
            popUpQueue.Clear();
            PopupCloseEvent.Clear();
            IsCanCloseByBG.Clear();
        }


        /// <summary>
        /// 關閉Popup彈窗 如果不需要播放動作，一定要將 playAnim 置為 false。
        /// </summary>
        /// <param name="popUpId"></param>
        /// <param name="playAnim">是否播放關閉動畫。在需要關閉後需要切換場景的時候，播放動畫會引起卡滯。</param>
        public void Close(PopupID popUpId, bool playAnim = true)
        {
            Q.Assert(IsPopup(popUpId), popUpId + "未弹出");

            Action<UIPopUpBehaviour> onComplete = null;
            onComplete = delegate(UIPopUpBehaviour uiPopUp)
            {
                if (uiPopUp != null)
                    uiPopUp.PopUpOutComplete -= onComplete;

                //改為動畫完成之後或者不需要動畫時才將此key移除

                popUps.Remove(popUpId);
                popUpQueue.Remove(popUpId);
                PopupCloseEvent.Remove(popUpId);
                IsCanCloseByBG.Remove(popUpId);

                if (OnCloseComplete != null)
                {
                    OnCloseComplete(popUpId);
                }

                RefPopsGraphic();

                //1. 在Unity5.0中，這樣寫Editor會崩
                //uiPopUp.gameObject.SetActive(false);
                //if (uiPopUp != null)
                //    GameObject.Destroy(uiPopUp.gameObject);

                //2. 在Unity5.2中，這樣會崩
                if (uiPopUp != null)
                {
                    uiPopUp.gameObject.SetActive(false);
                    GameObject.Destroy(uiPopUp.gameObject);
                }

                //3. 兼容方案
                //uiPopUp.GetComponent<Animator>().enabled = false;
                //GameObject.Destroy(uiPopUp.gameObject, 0.05f);
            };

            UIPopUpBehaviour uiPopup = popUps[popUpId];

            if (uiPopup == null)
            {
                Q.Assert(false, "Popup:Close Assert 1");
                //可能已經被回收掉
                onComplete(null);
                return;
            }

            bool wait4Animation = (playAnim && uiPopup.animator != null && uiPopup.animator.enabled);

            if (wait4Animation)
            {
                uiPopup.PopUpOutComplete += onComplete;
                uiPopup.PopUpOut();
            }
            else
            {
                onComplete(uiPopup);
            }
        }

        public bool IsPopup(PopupID popUpId)
        {
            return popUps.ContainsKey(popUpId);
        }

        public bool IsPopup(params PopupID[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (IsPopup(args[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="popUpId"></param>
        /// <returns>彈出的內容</returns>
        public Transform GetPopup(PopupID popUpId)
        {
            UIPopUpBehaviour pupup;

            if (IsPopup(popUpId))
            {
                pupup = popUps[popUpId];

                if (pupup != null)
                {
                    //蠻危險的
                    return pupup.PanelT.Find(popUpId.ToString());
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public PopupID GetCrtPopupID()
        {
            if (popUps.Count == 0)
                return PopupID.None;

            foreach (KeyValuePair<PopupID, UIPopUpBehaviour> pair in popUps)
                return pair.Key;

            return PopupID.None;
        }

        private Transform CreateUiPrefab(PopupID popupID)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/Ui/" + popupID);
            GameObject go = GameObject.Instantiate(prefab);

            return go.transform;
        }

        //既然是關閉頂部彈出的接口，為什麼又跟點擊背景空白區域有關呢？ ？ ？ ？ ？
        public void CloseTopPopup()
        {
            if (popUpQueue.Count == 0)
            {
                return;
            }

            PopupID id = popUpQueue[popUpQueue.Count - 1];
            ///不能點擊背景空白區域的彈窗不關閉///
            if (IsCanCloseByBG.ContainsKey(id) && !IsCanCloseByBG[id])
            {
                return;
            }
            ///不為空則調用自身的closebutton事件//
            ///不為空的前提是彈窗類繼承了PopupEventCor並重寫Close方法///
            if (PopupCloseEvent.ContainsKey(id) && PopupCloseEvent[id] != null)
            {
                PopupCloseEvent[id]();
                PopupCloseEvent.Remove(id);
            }
            else
            {
                Close(id);
            }
        }

        void SendUIMessage(PopupID ID)
        {
            string mess = "";

            string[] UseID = 
            {
                PopupID.UISelectHero.ToString(),
                PopupID.UIWin.ToString(),
                PopupID.UILose.ToString(),
                PopupID.UISetting.ToString(),
                PopupID.UILoginGive.ToString()
            };

            foreach (var uid in UseID)
            {
                if (uid.Equals(ID.ToString()))
                {
                    mess = uid;
                    break;
                }
            }

            if (string.IsNullOrEmpty(mess))
                return;

            bool havetree = false;

            if (GameController.Instance.SceneCtr.SceneData != null && GameController.Instance.SceneCtr.SceneData.ContainsKey(Scenes.Tree.ToString()))
            {
                havetree = true;
            }

            if (havetree || GameController.Instance.SceneCtr.WantToScene == Scenes.Tree)
            {
                mess += Scenes.Tree.ToString();
            }

            SendHttpMessageCtr.Instance.SendUIMessage(mess);
        }

    }
}
