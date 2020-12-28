using Com4Love.Qmax;
using UnityEngine;
using Com4Love.Qmax.Ctr;

public class UiEventHandler : MonoBehaviour {

    public void OnSceneBackButtonClick()
    {
        GameController.Instance.SceneCtr.LoadLevel(Scenes.MapScene,null,false);
    }
}
