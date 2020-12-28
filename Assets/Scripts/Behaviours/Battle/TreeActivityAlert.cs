using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using UnityEngine.UI;
using System.Collections.Generic;
using Com4Love.Qmax.Data;

public class TreeActivityAlertHelper {

    //彈出大樹活動外提示窗口
    public static void TimeOutTipAlert()
    {
        //int languageId0 = 10008;        //活动次数用完
        //int languageId1 = 10009;        //不在活动时间内

        //string infoText;
        //TreeActivityData Data = GameController.Instance.TreeActivityCtr.Data;

        ////if (Data.CutLeftCount > 0)
        ////{
        ////    //有次數可以打 那麼就是活動時間還沒到
        ////    infoText = Utils.GetTextByID(languageId1);
        ////}
        ////else
        ////{
        ////    infoText = Utils.GetTextByID(languageId0);
        ////}

        //UIAlertBehaviour alert = UIAlertBehaviour.Alert(Utils.GetText("活動開放時間"), "TreeAlertContent", infoText, 2, 2, 1);
        ////活動次數已用完，改天再來

        //RectTransform alertBody = alert.GetBodyContent();
        //Text[] texts = alertBody.GetComponentsInChildren<Text>();
        //List<string> timeList = GameController.Instance.TreeActivityCtr.Data.timeList;

        //if (timeList != null)
        //{
        //    for (int i = 0; i < texts.Length; i++)
        //    {
        //        Text text = texts[i];

        //        if (i < timeList.Count)
        //        {
        //            text.gameObject.SetActive(true);
        //            string val = Utils.GetTextByID(int.Parse(timeList[i]));
        //            val = Utils.DecodeHtmlStringInXml(val);
        //            text.text = val;
        //        }
        //        else
        //        {
        //            text.gameObject.SetActive(false);
        //        }
        //    }
        //}
    }

}
