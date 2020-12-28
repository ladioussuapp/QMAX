using UnityEngine;
using System.Collections;
using System;

namespace Com4Love.Qmax.Helper
{

    public class UILinkTipsHelper : MonoBehaviour
    {
        UILinkTipsBehaviour TextTips;

        Transform AniTips;

        BaseStateMachineBehaviour AniTipsState;
        Animator AniTipsAnimator;


        BaseStateMachineBehaviour TextTipsState;
        public void ShowTextTips(string text)
        {
            if (TextTips == null)
                CreateTextTips(text);
            else
            {
                TextTips.SetText(text);
                TextTips.gameObject.SetActive(true);
                ResetPos(TextTips.transform);

                AddTextTipsEvent();
            }

        }

        public void CloseTextTips()
        {
            if (TextTips != null)
            {
                StartCoroutine(Utils.DelayNextFrameCall(delegate ()
                {
                    TextTips.gameObject.SetActive(false);
                }));
            }

        }

        public void ShowAniTips()
        {
            if (AniTips == null)
                CreateAniTips();
            else
            {
                AniTips.gameObject.SetActive(true);
                ResetPos(AniTips);
                PlayAniTipsAni();
            }
                
        }

        public void CloseAniTips()
        {
            if (AniTips != null)
            {
                StartCoroutine(Utils.DelayNextFrameCall(delegate ()
                {
                    AniTips.gameObject.SetActive(false);
                }));
            }
        }

        private void CreateTextTips(string text)
        {
            Transform tips = GameController.Instance.Popup.ShowTextFloat(LayerCtrlBehaviour.ActiveLayer.FloatLayer as RectTransform, "Prefabs/Ui/UILinkTips");

            TextTips = tips.GetComponent<UILinkTipsBehaviour>();
            TextTips.SetText(text);
            TextTips.gameObject.SetActive(true);

            AddTextTipsEvent();
        }

        void AddTextTipsEvent()
        {
            if (TextTipsState != null)
                return;

            TextTipsState = TextTips.GetComponent<Animator>().GetBehaviour<BaseStateMachineBehaviour>();

            Action<Animator, int> StateMachineExitEvent = null;
            StateMachineExitEvent = delegate (Animator arg1, int arg2)
            {
                CloseTextTips();
            };
            TextTipsState.StateMachineExitEvent += StateMachineExitEvent;
        }


        void CreateAniTips()
        {
            Transform tips = Resources.Load<Transform>("Prefabs/Ui/UIRevokeTips");

            AniTips = Instantiate<Transform>(tips);

            AniTips.gameObject.SetActive(true);
            AniTips.SetParent(LayerCtrlBehaviour.ActiveLayer.FloatLayer);
            ResetPos(AniTips);

            PlayAniTipsAni();

            //AniTipsAnimator.transform.FindChild("Xian").localScale = new Vector3(Screen.width / 960f,1f,1f);
        }

        void PlayAniTipsAni()
        {
            if (AniTipsState != null)
                return;

            if (AniTipsAnimator == null)
                AniTipsAnimator = AniTips.Find("Ani").GetComponent<Animator>();

            AniTipsState = AniTipsAnimator.GetBehaviour<BaseStateMachineBehaviour>();
            Action<Animator, AnimatorStateInfo, int> exitEvent = null;
            exitEvent = delegate (Animator ani, AnimatorStateInfo info, int index)
            {
                if(!info.IsName("Start"))
                    CloseAniTips();
            };
            AniTipsState.StateExitEvent += exitEvent;

            AniTipsAnimator.SetTrigger("Play");
        }

        void ResetPos(Transform tran)
        {
            tran.localPosition = Vector3.zero;
            tran.localScale = Vector3.one;
            tran.localRotation = Quaternion.identity;
        }

    }
}

