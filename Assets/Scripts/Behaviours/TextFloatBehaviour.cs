using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextFloatBehaviour : MonoBehaviour
{
    public Text Text;
    public Image ImgText;
    [System.NonSerialized]
    public Image Bg;
 
    public void SetText(string value)
    {
        Text.text = value;
    }

    public void SetTextSprite(Sprite sprite)
    {
        Text.gameObject.SetActive(false);
        ImgText.gameObject.SetActive(true);
        ImgText.sprite = sprite;
        ImgText.SetNativeSize();
    }

    public void SetBgVisbile(bool val)
    {
        Bg.enabled = val;
    }

    void Awake()
    {
        Bg = GetComponent<Image>();
    }

    // Use this for initialization
    void Start()
    {
        BaseStateMachineBehaviour beh = GetComponent<Animator>().GetBehaviour<BaseStateMachineBehaviour>();

        Action<Animator, int> StateMachineExitEvent = null;
        StateMachineExitEvent = delegate(Animator arg1, int arg2)
        {
            beh.StateMachineExitEvent -= StateMachineExitEvent;
            StartCoroutine(DelayDestroy());
        };
        beh.StateMachineExitEvent += StateMachineExitEvent;
    }

    private IEnumerator DelayDestroy()
    {
        yield return null;
        Destroy(gameObject);
    }
}
