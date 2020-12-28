using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using UnityEngine;
using UnityEngine.UI;


public class UIGoalTipListItem : MonoBehaviour
{
    StageConfig.Goal data;
    public Text NumTextTip; //數字單獨在此顯示
    public GoalIcon ImgIcon;

    public void SetData(StageConfig.Goal value)
    {
        data = value;
        DataChange();
    }

    void DataChange()
    {
        ImgIcon.Data = data;
        NumTextTip.text = data.Num.ToString();

        //TileObjectConfig oConfig;
        //switch (data.Type)
        //{
        //    case Com4Love.Qmax.BattleGoal.Unit:
        //        break;
        //    case Com4Love.Qmax.BattleGoal.Object:
        //        oConfig = GameController.Instance.Model.TileObjectConfigs[data.RelativeID];
        //        break;
        //    default:
        //        break;
        //}
    }

    //string GetEmemyTip(int num)
    //{
    //    return Utils.GetText("擊敗");
    //}

    //string GetObjectTip(TileObjectConfig oConfig, int num)
    //{
    //    string tip = "";

    //    switch (oConfig.ObjectType)
    //    {
    //        case Com4Love.Qmax.TileType.Element:
    //            tip = Utils.GetText("收集");
    //            break;
    //        case Com4Love.Qmax.TileType.Cover:
    //            tip = Utils.GetText("打開");
    //            break;
    //        case Com4Love.Qmax.TileType.Obstacle:
    //        case Com4Love.Qmax.TileType.Gift:
    //        case Com4Love.Qmax.TileType.Bottom:
    //            tip = Utils.GetText("收集");
    //            break;
    //        default:
    //            break;
    //    }

    //    return tip;
    //}
}
