using UnityEngine;
using System.Collections;
using Com4Love.Qmax;

 
public class HeroRun : MonoBehaviour {
    public Animator[] heroAnimators;

    //public float timeScale = 1f;
    public float delayTime = 1f;
    public Transform floor;

    protected float time = 0f;
    protected bool _isRunning = false;
 
    public bool isRunning
    {
        get
        {
           return _isRunning;
        }
    }

    public void Play()
    {
        if (!this.isActiveAndEnabled)
        {
            return;
        }

        time = delayTime;

        if (!isRunning)
        {
            StartCoroutine(_Run());
            return;
        }
    }

    protected IEnumerator _Run()
    {
        _isRunning = true;

        foreach (Animator heroAnimator in heroAnimators)
        {
            if (heroAnimator != null)
            {
                heroAnimator.SetTrigger("TriggerWalk");
            }
        }
 
        yield return 0;

        while (time > 0)
        {
            time -= Time.deltaTime;

            yield return 0;
        }

        time = 0;

        foreach (Animator heroAnimator in heroAnimators)
        {
            if (heroAnimator != null)
            {
                heroAnimator.SetTrigger("TriggerIdle");
            }
        }

        _isRunning = false;
    }
 
    public void stop()
    {
                
    }

    public void Start()
    {
        //Spine.AnimationStateData data;
        //SkeletonAnimator skeletonAnimator;

        //foreach (Animator animator in heroAnimators)
        //{
        //    if (animator == null)
        //    {
        //        return;
        //    }

        //    skeletonAnimator = animator.gameObject.GetComponent<SkeletonAnimator>();
        //    Q.Assert(skeletonAnimator != null, animator.gameObject + "上找不到SkeletonAnimator组件");

        //    data = skeletonAnimator.skeletonDataAsset.GetAnimationStateData();
        //    data.SetMix("Idle", "Walk", 5f);
        //}
 
    }
}
