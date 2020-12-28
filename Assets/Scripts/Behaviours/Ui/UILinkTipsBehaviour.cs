using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using System;

public class UILinkTipsBehaviour : MonoBehaviour
{

    public Text text;
    public static UILinkTipsBehaviour Instance;
    // Use this for initialization
    //void Start()
    //{
    //    //BaseStateMachineBehaviour beh = GetComponent<Animator>().GetBehaviour<BaseStateMachineBehaviour>();
    //    //Action<Animator, int> StateMachineExitEvent = null;
    //    //StateMachineExitEvent = delegate (Animator arg1, int arg2)
    //    //{
    //    //    beh.StateMachineExitEvent -= StateMachineExitEvent;
    //    //    this.OnClose();
    //    //};
    //    //beh.StateMachineExitEvent += StateMachineExitEvent;
    //    //Instance = this;
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void SetText(string value)
    {
        text.text = value;
    }
    //public void OnClose()
    //{
    //    StartCoroutine(DelayDestroy());
    //}
    //private IEnumerator DelayDestroy()
    //{
    //    yield return null;
    //    if (gameObject != null)
    //        Destroy(gameObject);
    //}

    //public static void Show(string _text)
    //{
    //    if (UILinkTipsBehaviour.Instance == null)
    //    {
    //        CreateTips(_text);
    //    }
    //    else if (UILinkTipsBehaviour.Instance.text.text != _text)
    //    {
    //        UILinkTipsBehaviour.Instance.OnClose();
    //        CreateTips(_text);
    //    }

    //}

    //private static void CreateTips(string text)
    //{
    //    Transform tips = GameController.Instance.Popup.ShowTextFloat(LayerCtrlBehaviour.ActiveLayer.FloatLayer as RectTransform, "Prefabs/Ui/UILinkTips");
    //    tips.gameObject.GetComponent<UILinkTipsBehaviour>().SetText(text);
    //}
}
