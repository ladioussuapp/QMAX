using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UIAddMoveTimeGoal : MonoBehaviour {

    public Text ScoresTitle;
    public Text Scores;

    public void setData(int scores)
    {
        ScoresTitle.text = Utils.GetTextByID(541);
        Scores.text = string.Format("{0:N0}", scores);
    }


}
