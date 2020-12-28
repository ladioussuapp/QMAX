using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data.Config;
using System;

public class BattleMusicPlay : MonoBehaviour
{
    public int TestLv = 1;

    private bool isComb2 = false;

    void Start()
    {
        if (GameController.Instance.ModelEventSystem != null)
        {
            GameController.Instance.ModelEventSystem.OnStageWin += StageWin;
            GameController.Instance.ModelEventSystem.OnStageLose += StageLose;
            GameController.Instance.ModelEventSystem.OnLastStep += LastStep;
            GameController.Instance.ModelEventSystem.OnLastTime += LastTime;
            GameController.Instance.ModelEventSystem.OnBuyMoves += BugMoves;
            GameController.Instance.ModelEventSystem.OnBuyTime += BuyTime;
        }

        if (GameController.Instance.SceneCtr.SceneData != null)
        {
            TestLv = (int)GameController.Instance.SceneCtr.SceneData["lvl"];
        }

        StageConfig stage = GameController.Instance.Model.StageConfigs[TestLv];
        FMOD_StudioEventEmitter fomdSource = GetComponent<FMOD_StudioEventEmitter>();
        if (fomdSource != null)
        {
            string path = stage.Gamemusic;
            if (!path.StartsWith("event:/music/"))
                fomdSource.path = "event:/music/" + path;
            else
                fomdSource.path = path;
        }
    }

    void OnDestroy()
    {
        if (GameController.Instance.ModelEventSystem != null)
        {
            GameController.Instance.ModelEventSystem.OnStageWin -= StageWin;
            GameController.Instance.ModelEventSystem.OnStageLose -= StageLose;
            GameController.Instance.ModelEventSystem.OnLastStep -= LastStep;
            GameController.Instance.ModelEventSystem.OnLastTime -= LastTime;
            GameController.Instance.ModelEventSystem.OnBuyMoves -= BugMoves;
            GameController.Instance.ModelEventSystem.OnBuyTime -= BuyTime;
        }
    }

    void BugMoves(int addMov)
    {
        if (addMov <= 0)
            return;

        FMOD_StudioEventEmitter fomdSource = GetComponent<FMOD_StudioEventEmitter>();
        if (fomdSource == null)
            return;

        fomdSource.setParameterValue("lose", 0);
        fomdSource.setParameterValue("comb2", 1);
    }

    void BuyTime(int addTime)
    {
        if (addTime <= 0)
            return;

        FMOD_StudioEventEmitter fomdSource = GetComponent<FMOD_StudioEventEmitter>();
        if (fomdSource == null)
            return;

        fomdSource.setParameterValue("lose", 0);
        fomdSource.setParameterValue("comb1", 0);
    }

    void StageLose()
    {
        FMOD_StudioEventEmitter fomdSource = GetComponent<FMOD_StudioEventEmitter>();
        if (fomdSource == null)
            return;

        fomdSource.setParameterValue("lose", 1);
        if (isComb2)
            fomdSource.setParameterValue("comb2", 0);
        else
            fomdSource.setParameterValue("comb1", 1);
    }

    void StageWin()
    {
        FMOD_StudioEventEmitter fomdSource = GetComponent<FMOD_StudioEventEmitter>();
        if (fomdSource == null)
            return;

        fomdSource.setParameterValue("win", 1);
        if (isComb2)
            fomdSource.setParameterValue("comb2", 0);
        else
            fomdSource.setParameterValue("comb1", 1);
    }

    void LastStep(int step)
    {
        // 剩餘5步
        if (step == 5)
        {
            FMOD_StudioEventEmitter fomdSource = GetComponent<FMOD_StudioEventEmitter>();
            if (fomdSource == null)
                return;

            isComb2 = true;
            fomdSource.setParameterValue("comb1", 1);
            fomdSource.setParameterValue("comb2", 1);
        }
    }

    void LastTime(int time)
    {
        // 剩餘6秒
        if (time == 6)
        {
            FMOD_StudioEventEmitter fomdSource = GetComponent<FMOD_StudioEventEmitter>();
            if (fomdSource == null)
                return;

            isComb2 = true;
            fomdSource.setParameterValue("comb1", 1);
            fomdSource.setParameterValue("comb2", 1);
        }
    }

}
