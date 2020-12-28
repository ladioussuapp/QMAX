using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;
using Com4Love.Qmax.Tools;

/// <summary>
/// 目标图片
/// </summary>
public class GoalIcon : MonoBehaviour {
    public Text amountText;
    public Image skinImg;

    StageConfig.Goal data;

    public StageConfig.Goal Data
    {
        get
        {
            return data;
        }
        set
        {
            if (data != value)
            {
                data = value;
                UpdateSkin();
            }
        }
    }
 
    void Awake()
    {
        if (amountText == null)
        {
            amountText = GetComponentInChildren<Text>();
        }

        if (skinImg == null)
        {
            skinImg = GetComponent<Image>();        
        }
    }
 
    void UpdateSkin()
    {
        if (amountText != null)
        {
            amountText.text = data.Num.ToString();
        }
 
        skinImg.sprite = GameController.Instance.QMaxAssetsFactory.CreateGoalSprite(data);
    }
}
