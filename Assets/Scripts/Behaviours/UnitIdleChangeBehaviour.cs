using UnityEngine;
using System.Collections;

public class UnitIdleChangeBehaviour : MonoBehaviour {

    private float timeLeave = 0;
    private float frameLen = 0;
    private Animator anim;
	void Start () {
        anim = gameObject.GetComponent<Animator>();
	}
	
	void Update () {
        timeLeave += Time.deltaTime;
        if (anim != null)
        {
            frameLen = anim.GetCurrentAnimatorStateInfo(0).length;
        }
       
        if (timeLeave > frameLen)
        {
            timeLeave = 0;
            ChangeIdleState();
        }
	}

    private void ChangeIdleState()
    {
        Animator anim = gameObject.GetComponent<Animator>();
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
        int seed = UnityEngine.Random.Range(0, 100);
        if (info.IsName("Idle"))
        {
            if (seed <= BattleTools.IDLERATE)
            {
                anim.SetTrigger("TriggerIdle2");
            }
        }
        else if(info.IsName("Idle2"))
        {
            if (seed > BattleTools.IDLERATE)
            {
                anim.SetTrigger("TriggerIdle");
            }
        }
    }
}
