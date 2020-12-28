using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using System.Collections.Generic;
using Com4Love.Qmax;

public class UIAddMoveGoalItem : MonoBehaviour {
    public Image IconImg;
    public Image DoneImg;
    public Text numText;

    public void SetData(int left, StageConfig.Goal goal)
    {
        IconImg.sprite = GameController.Instance.QMaxAssetsFactory.CreateGoalSprite(goal);
        IconImg.SetNativeSize();

        if (left > 0)
        {
            //未完成
            DoneImg.gameObject.SetActive(false);
            numText.gameObject.SetActive(true);
            numText.text = left.ToString();
        }
        else
        {
            //完成
            DoneImg.gameObject.SetActive(true);
            numText.gameObject.SetActive(false);
        }
    }
}
