using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoading:MonoBehaviour {
    public void Start()
    {
        GameController gameCtr = GameController.Instance;
        
    }
    public void LoadTestScene() {
        GameController.Instance.SceneCtr.LoadLevel(Scenes.Test3MatchStage, null, false);
    }
}
