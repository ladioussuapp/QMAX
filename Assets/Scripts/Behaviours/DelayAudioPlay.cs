using UnityEngine;
using System.Collections;
using Com4Love.Qmax;

/// <summary>
/// 延遲播放聲音
/// </summary>
public class DelayAudioPlay : MonoBehaviour
{

    public float DelayTime = 1f;

    private AudioSource audioSource;
    private FMOD_StudioEventEmitter fomdSource;

    // Use this for initialization
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        fomdSource = GetComponent<FMOD_StudioEventEmitter>();
    }


    public void OnEnable()
    {
        Invoke("Play", DelayTime);
    }

    void Play()
    {
        if (audioSource != null)
        {
            if (!audioSource.enabled)
            {
                audioSource.enabled = true;
            }

            if (audioSource.clip != null && audioSource.gameObject.activeInHierarchy && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        if (fomdSource != null)
        {
            if (!fomdSource.enabled)
            {
                fomdSource.enabled = true;
            }
            fomdSource.Play();
        }
    }

}
