using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using Com4Love.Qmax.Net;

public class ShowDelay : MonoBehaviour {

#if DelayTest
    // Use this for initialization
    GUIStyle UISty;
    void Start () {
        UISty = new GUIStyle();
        UISty.fontSize = 30;
        UISty.normal.textColor = Color.red;

    }

    void OnGUI()
    {
        string mess = string.Format("module:{0} cmd:{1} delay:{2}s",
            BaseClient.TestModule
            , BaseClient.TestCmd
            , BaseClient.TestDelayTime);

        GUI.Label(new Rect(0,20,400,200), mess,UISty);
    }
#endif
}
