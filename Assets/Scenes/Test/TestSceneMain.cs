using Com4Love.Qmax;
using System.Collections;
using UnityEngine;

public class TestSceneMain : MonoBehaviour {
    public Transform Go;
    LTDescr ltD;
	// Use this for initialization
	void Start () {
        Invoke("Test", 3f);
    }

    void Test()
    {
        GameController.Instance.Popup.Open(PopupID.UINoticeWindow,null, true, true);
    }
}


