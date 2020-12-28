using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetBattleLv : MonoBehaviour {

    public void Start()
    {
        GameController.Instance.ModelEventSystem.OnBeginCounterpartStage();
        GameController.Instance.TreeFightCtr.Init(10);

    }


}
