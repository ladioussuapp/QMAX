using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

/// <summary>
/// 數字滾動效果
/// </summary>
public class TextScrollEffect : MonoBehaviour
{
    public float from;
    public float to;
    public float time;
    public Text text;
    public AudioClip AudioClip;     //音效    設置音效的話一定要在text身上有audiosource才行

    AudioSource AudioSource;

    private Action finish;

    // Use this for initialization
    void Awake()
    {
        if (text == null)
        {
            text = GetComponent<Text>();
        }

        if (AudioSource == null)
        {
            AudioSource = GetComponent<AudioSource>();
        }
    }

    public void Run(Action onFinish)
    {
        this.finish = onFinish;

        LeanTween.cancel(text.gameObject);

        if (AudioClip != null && AudioSource != null)
        {
            AudioSource.clip = AudioClip;
            AudioSource.loop = true;
            AudioSource.Play();
        }

        LTDescr ltDescr = LeanTween.value(text.gameObject, from, to, time);
        ltDescr = ltDescr.setOnUpdate(OnUpdate);
        ltDescr.setOnComplete(OnComplete);
    }

    void OnComplete()
    {
        if (finish != null)
            finish();

        if (AudioSource != null && AudioClip != null)
        {
            AudioSource.Stop();
            AudioSource.clip = null;
            AudioClip = null;
        }

        Destroy(this);
    }

    void OnUpdate(float valF)
    {
        text.text = ((int)valF).ToString();
    }

    public void OnDestroy()
    {
        finish = null;
    }
}
