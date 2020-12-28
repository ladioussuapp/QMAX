using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Data;

public class GuideUtility
{
    public static Dictionary<int, Dictionary<int, int>> contentName;
    public static Dictionary<int, Dictionary<int, string>> audioName;
    public static Dictionary<int, int> GuideIDs;
    public static Dictionary<int, Dictionary<int, Vector3>> fingerPos;

    public static void initGuideFightData()
    {
        GuideIDs = new Dictionary<int, int>();
        contentName = new Dictionary<int, Dictionary<int, int>>();
        audioName = new Dictionary<int, Dictionary<int, string>>();
        fingerPos = new Dictionary<int, Dictionary<int, Vector3>>();
        //addFightGuideData();
    }

    public static void addFightGuideData(BattleModel model)
    {
        GuideIDs.Clear();
        GuideIDs.Add(1, 1);
        GuideIDs.Add(2, 1);
        GuideIDs.Add(3, 1);
        GuideIDs.Add(4, 1);
        GuideIDs.Add(6, 1);
        GuideIDs.Add(9, 1);

        contentName.Clear();

        string dialogId = model.CrtStageConfig.DialogId_v1;
        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                dialogId = model.CrtStageConfig.DialogId_v2;
                break;
            default:
                break;
        }
        if (dialogId != "")
        {
            string[] stepDialogGroupArr = dialogId.Split('|');
            Dictionary<int, int> stepDialog = new Dictionary<int, int>();
            for (int i = 0; i < stepDialogGroupArr.Length; i++)
            {
                string stepDialogItem = stepDialogGroupArr[i];
                string[] stepDialogCfg = stepDialogItem.Split(',');
                if (stepDialogCfg.Length == 2)
                {
                    int stepIndex = model.StageLimit - int.Parse(stepDialogCfg[0]);
                    int dialogIndex = int.Parse(stepDialogCfg[1]);
                    stepDialog.Add(stepIndex, dialogIndex);
                }
            }

            contentName.Add(model.CrtStageConfig.ID, stepDialog);
        }


        audioName.Clear();
        Dictionary<int, string> audioStep1 = new Dictionary<int, string>();
        audioStep1.Add(0, "SD_guide_lvone2");
        audioStep1.Add(1, "SD_guide_lvone3");
        audioName.Add(1, audioStep1);
        //添加手指引导
        fingerPos.Clear();
        Vector3 aimPoint = new Vector3(180, 630, -100);
        Dictionary<int, Vector3> fingerStep1 = new Dictionary<int, Vector3>();
        fingerStep1.Add(2, aimPoint);

        Dictionary<int, Vector3> fingerStep2 = new Dictionary<int, Vector3>();
        fingerStep2.Add(1, aimPoint);

        Dictionary<int, Vector3> fingerStep3 = new Dictionary<int, Vector3>();
        fingerStep3.Add(0, new Vector3(60,380,-100));

        Dictionary<int, Vector3> fingerStep4 = new Dictionary<int, Vector3>();
        fingerStep4.Add(0, aimPoint);

        Dictionary<int, Vector3> fingerStep5 = new Dictionary<int, Vector3>();
        fingerStep5.Add(0,new Vector3(60, 380, -100));

        Dictionary<int, Vector3> fingerStep6 = new Dictionary<int, Vector3>();
        fingerStep6.Add(0, aimPoint);

        Dictionary<int, Vector3> fingerStep7 = new Dictionary<int, Vector3>();
        fingerStep7.Add(0, aimPoint);

        if (GuideManager.getInstance().version == GuideVersion.Version_1)
        {
            fingerPos.Add(1, fingerStep1);
            fingerPos.Add(2, fingerStep2);
            fingerPos.Add(4, fingerStep3);

            fingerPos.Add(5, fingerStep6);

            fingerPos.Add(9, fingerStep4);
            fingerPos.Add(12, fingerStep7);

            fingerPos.Add(16, fingerStep5);
        }
        //
    }
}
