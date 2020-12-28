using UnityEngine;
using UnityEngine.UI;

public class SkillLoadingBehaviour : MonoBehaviour
{
    public Image ImgIcon;
    public Image ImgMask;
    public Image ImgMask1;
    public ParticleSystem SkillUp;

    public void SetSprite(Sprite sprite)
    {
        ImgIcon.sprite = sprite;
        ImgMask.sprite = sprite;

        if (ImgMask1)
        {
            ImgMask1.sprite = sprite;
            ImgMask1.fillAmount = 1;
        }

        ImgMask.fillAmount = 1;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">[0, 1]</param>
    public void SetPercentage(float value, bool showAnimation = false)
    {
        if (ImgMask.fillAmount == 0)
        {
            ImgMask.fillAmount = 1;
        }
        if (showAnimation)
        {
            float oldValue = ImgMask.fillAmount;
            LeanTween.value(ImgMask.gameObject, oldValue, 1 - value, 0.5f).setOnUpdate(delegate(float param)
            {
                ImgMask.fillAmount = param;
            }).setOnComplete(delegate()
            {
                ImgMask.fillAmount = 1 - value;
                if (ImgMask1)
                {
                    ImgMask1.fillAmount = 1 - value;
                }
                if (value == 1)
                {
                    if (SkillUp)
                    {
                        SkillUp.Play();
                        GameController.Instance.AudioManager.PlayAudio("SD_attack_tree_blood_aura");
                    }
                }
            });
        }
        else
        {
            ImgMask.fillAmount = 1 - value;

            if (ImgMask1)
            {
                ImgMask1.fillAmount = 1 - value;
            }
        }
        
    }

    public void SetForecastPercentage(float value)
    {
        if (ImgMask.fillAmount == 0)
        {
            ImgMask.fillAmount = 1;
        }
        if (ImgMask1)
        {
            ImgMask1.fillAmount = 1 - value;
        }
    }
}
