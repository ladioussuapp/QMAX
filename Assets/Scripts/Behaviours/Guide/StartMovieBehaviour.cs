using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StartMovieBehaviour : MonoBehaviour
{

    // Use this for initialization
    int MovieStep;

    int MaxStep;

    public Button TipButton;

    public Animator MovieAnimator1;
    public Animator MovieAnimator2;
    public Animator MovieAnimator3;

    public System.Action OverEvent;
    
    private BaseStateMachineBehaviour[] BaseStates;
    void Start()
    {
        
        EventTriggerListener eventTri = EventTriggerListener.Get(TipButton.gameObject);

        eventTri.onClick += OnTipClick;

        MaxStep = 3;

    }

    void OnDestroy()
    {

    }

    public void Play(int moviestep = 1)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource.isPlaying)
            audioSource.Stop();

        MovieStep = moviestep;
        switch (MovieStep)
        {
            case 1:
                MovieAnimator1.SetTrigger("play");
                GameController.Instance.QMaxAssetsFactory.CreateEffectAudio("caricature_first", delegate (AudioClip clip)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                });
                break;
            case 2:
                MovieAnimator2.SetTrigger("play");
                GameController.Instance.QMaxAssetsFactory.CreateEffectAudio("caricature_second", delegate (AudioClip clip)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                });
                break;
            case 3:
                MovieAnimator3.SetTrigger("play");
                GameController.Instance.QMaxAssetsFactory.CreateEffectAudio("caricature_third", delegate (AudioClip clip)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                });
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// 當前動畫退出
    /// </summary>
    /// <param name="ani"></param>
    /// <param name="info"></param>
    /// <param name="index"></param>
    public void OnMovieExit(int layerIndex)
    {
        Debug.Log("step is out  " + MovieStep);

        if (MovieStep == MaxStep)
        {
            if (OverEvent != null)
                OverEvent();

            Debug.Log("it is over");
            return;
        }

        TipButton.transform.parent.gameObject.SetActive(true);
    }

    /// <summary>
    /// 點擊提示按鈕
    /// </summary>
    /// <param name="button"></param>
    public void OnTipClick(GameObject button)
    {
        if (MovieStep < MaxStep)
        {
            Play(MovieStep + 1);
            TipButton.transform.parent.gameObject.SetActive(false);
        }

        Debug.Log("click step is " + MovieStep);
    }
}

class PlayerMachineBehaviour : BaseStateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
