using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenesLoading : MonoBehaviour
{
    AsyncOperation asyncOp;
    Scenes targetScene;


    public void Start()
    {
        GameController gameCtr = GameController.Instance;

        targetScene = GameController.Instance.SceneCtr.TargetScene;
        Q.Assert(targetScene != Scenes.None);
        if (GameController.Instance.Popup.HasPopup)
            GameController.Instance.Popup.CloseAll();


        if (targetScene != Scenes.BattleScene)
        {
            StartCoroutine(LoadScene());
            return;
        }


        int lv = (int)gameCtr.SceneCtr.SceneData["lvl"];
        List<int> unitList = (List<int>)gameCtr.SceneCtr.SceneData["units"];
        unitList = new List<int>(unitList);

        StageConfig stageCfg = gameCtr.Model.StageConfigs[lv];
        if (stageCfg.MonsterUnitID.Length > 0)
            unitList.Add(stageCfg.MonsterUnitID[0]);

        int count = 0;

        if (unitList.Count == 0)
        {
            StartCoroutine(LoadScene());
            return;
        }

        for (int i = 0, n = unitList.Count; i < n; i++)
        {
            UnitConfig cfg = gameCtr.Model.UnitConfigs[unitList[i]];
            GameController.Instance.PoolManager.GetUnitInstance(
                cfg,
                delegate(string key, Transform unit)
                {
                    unit.localPosition = Vector3.zero;
                    //提前加載Unit，放入緩衝池內
                    gameCtr.PoolManager.PushToInstancePool(cfg.PrefabPath, unit);

                    if (++count >= n)
                    {
                        StartCoroutine(LoadScene());
                    }
                });
        }
    }


    protected IEnumerator LoadScene()
    {
        //asyncOp = Application.LoadLevelAsync(GameController.Instance.SceneCtr.TargetScene.ToString());
        //asyncOp.allowSceneActivation = false;

        //while (asyncOp.progress < 0.9f && !asyncOp.isDone)
        //{
        //    yield return new WaitForEndOfFrame();
        //}

        
        //asyncOp.allowSceneActivation = true;



        int displayProgress = 0;
        int toProgress = 0;
        asyncOp = Application.LoadLevelAsync(GameController.Instance.SceneCtr.TargetScene.ToString());
        asyncOp.allowSceneActivation = false;
        while (asyncOp.progress < 0.9f)
        {
            toProgress = (int)(asyncOp.progress * 100);
            while (displayProgress < toProgress)
            {
                ++displayProgress;
            }
            yield return new WaitForEndOfFrame();
        }

        toProgress = 50;
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            yield return new WaitForEndOfFrame();
        }
        asyncOp.allowSceneActivation = true;
        GameController.Instance.SceneCtr.LoadComplete(targetScene);
    }
}