using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using System.Collections.Generic;
using Com4Love.Qmax.Data;

public class UnitShockBehaviour : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    private static void PlayShockRight(ColorType colorType,Dictionary<ColorType, Animator> UnitAnims)
    {
        if (UnitAnims.ContainsKey(colorType))
        {
            Animator rightAnim = UnitAnims[colorType];
            AnimatorStateInfo rightStateInfo = rightAnim.GetCurrentAnimatorStateInfo(0);
            if (rightStateInfo.IsName("Idle") || rightStateInfo.IsName("Idle2"))
            {
                rightAnim.SetTrigger("TriggerShockRight");
            }
        }
    }

    private static void PlayShockLeft(ColorType colorType, Dictionary<ColorType, Animator> UnitAnims)
    {
        if (UnitAnims.ContainsKey(colorType))
        {
            Animator leftAnim = UnitAnims[colorType];
            AnimatorStateInfo leftStateInfo = leftAnim.GetCurrentAnimatorStateInfo(0);
            if (leftStateInfo.IsName("Idle") || leftStateInfo.IsName("Idle2"))
            {
                leftAnim.SetTrigger("TriggerShockLeft");
            }
        }
    }

    private static void PlayShockRightEnd(ColorType colorType, Dictionary<ColorType, Animator> UnitAnims)
    {
        if (UnitAnims.ContainsKey(colorType))
        {
            Animator leftAnim = UnitAnims[colorType];
            AnimatorStateInfo leftStateInfo = leftAnim.GetCurrentAnimatorStateInfo(0);
            if (leftStateInfo.IsName("Shock_right"))
            {
                leftAnim.SetTrigger("TriggerIdle");
                
            }
        }
    }

    private static void PlayShockLeftEnd(ColorType colorType, Dictionary<ColorType, Animator> UnitAnims)
    {
        if (UnitAnims.ContainsKey(colorType))
        {
            Animator rightAnim = UnitAnims[colorType];
            AnimatorStateInfo rightStateInfo = rightAnim.GetCurrentAnimatorStateInfo(0);
            if (rightStateInfo.IsName("Shock_left"))
            {
                rightAnim.SetTrigger("TriggerIdle");
            }
        }
    }

    public static void Play(int eliminateCount,ColorType colorType, Dictionary<ColorType, Animator> UnitAnims)
    {
        QmaxModel model = GameController.Instance.Model;

        float comboRate = 0;

        if (eliminateCount > 0)
        {
            comboRate = model.ComboConfigs[eliminateCount].ComboRate;
        }
         //if (eliminateCount >= model.GameSystemConfig.FriendWorship)
        if(comboRate >=1.2f)
         {
             for (int i = (int)ColorType.Earth; i < (int)ColorType.All; i++)
             {
                 if(i>=0 && i<(int)colorType)
                 {
                     PlayShockRight((ColorType)i, UnitAnims);
                 }
                 else if(i>(int)colorType && i<(int)ColorType.All)
                 {
                     PlayShockLeft((ColorType)i, UnitAnims);
                 }

                 if (colorType != (ColorType)i && UnitAnims.ContainsKey((ColorType)i))
                 {
                     if (comboRate >1.2f && comboRate <=1.5f)
                     {
                         UnitAnims[(ColorType)i].speed = 2f;
                     }
                     else if (comboRate > 1.5f && comboRate <= 2f)
                     {
                         UnitAnims[(ColorType)i].speed = 4f;
                     }
                     else
                     {
                         UnitAnims[(ColorType)i].speed = 1f;
                     }
                 }
             }
         }
         else
         {
             for (int i = (int)ColorType.Earth; i < (int)ColorType.All; i++)
             {
                 if (i >= 0 && i < (int)colorType)
                 {
                     PlayShockRightEnd((ColorType)i, UnitAnims);
                 }
                 else if (i > (int)colorType && i < (int)ColorType.All)
                 {
                     PlayShockLeftEnd((ColorType)i, UnitAnims);
                 }

                 if (UnitAnims.ContainsKey((ColorType)i))
                 {
                     UnitAnims[(ColorType)i].speed = 1f;
                 }
                 
             }
         }
         
        /*
        if (eliminateCount >= model.GameSystemConfig.FriendWorship)
        {
            ColorType leftColor = colorType - 1;
            if ((int)leftColor >= 0 && (int)leftColor <= 5 && UnitAnims.ContainsKey(leftColor))
            {
                Animator leftAnim = UnitAnims[leftColor];
                AnimatorStateInfo leftStateInfo = leftAnim.GetCurrentAnimatorStateInfo(0);
                if (leftStateInfo.IsName("Idle"))
                {
                    leftAnim.SetTrigger("TriggerShockRight");
                }
            }

            ColorType rightColor = colorType + 1;
            if ((int)rightColor >= 0 && (int)rightColor <= 5 && UnitAnims.ContainsKey(rightColor))
            {
                Animator rightAnim = UnitAnims[rightColor];
                AnimatorStateInfo rightStateInfo = rightAnim.GetCurrentAnimatorStateInfo(0);
                if (rightStateInfo.IsName("Idle"))
                {
                    rightAnim.SetTrigger("TriggerShockLeft");
                }
            }
        }
        else
        {
            ColorType leftColor = colorType - 1;
            if ((int)leftColor >= 0 && (int)leftColor <= 5 && UnitAnims.ContainsKey(leftColor))
            {
                Animator leftAnim = UnitAnims[leftColor];
                AnimatorStateInfo leftStateInfo = leftAnim.GetCurrentAnimatorStateInfo(0);
                if (leftStateInfo.IsName("Shock_right"))
                {
                    leftAnim.SetTrigger("TriggerIdle");
                }
                //leftAnim.SetTrigger("TriggerIdle");
            }

            ColorType rightColor = colorType + 1;
            if ((int)rightColor >= 0 && (int)rightColor <= 5 && UnitAnims.ContainsKey(rightColor))
            {
                Animator rightAnim = UnitAnims[rightColor];
                AnimatorStateInfo rightStateInfo = rightAnim.GetCurrentAnimatorStateInfo(0);
                if (rightStateInfo.IsName("Shock_left"))
                {
                    rightAnim.SetTrigger("TriggerIdle");
                }
                //rightAnim.SetTrigger("TriggerIdle");
            }
        }
         */
    }

    // Update is called once per frame
    void Update()
    {

    }
}
