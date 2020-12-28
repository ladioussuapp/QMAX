using UnityEngine;
using System.Collections;

public class ShowFPS : MonoBehaviour
{
    public float f_UpdateInterval = 0.5F;
    private float f_LastInterval;
    private int i_Frames = 0;
    private float f_Fps;
    private GUIStyle style;

    void Start()
    {
        f_LastInterval = Time.realtimeSinceStartup;
        i_Frames = 0;

        style = new GUIStyle();
        style.fontSize = 36;
        style.normal.textColor = Color.red;
 
    }

    void OnGUI()
    {
        //string msg = string.Format("FPS:{0}\nVer:{1}" , f_Fps.ToString("f2"), Application.version);
        string msg = string.Format("{0}\t {1}x{2}", f_Fps.ToString("f2"), Screen.currentResolution.width , Screen.currentResolution.height);
        GUI.Label(new Rect(0, 0, 300, 100), msg, style);
    }

    void Update()
    {
        ++i_Frames;

        if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval)
        {
            f_Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);

            i_Frames = 0;

            f_LastInterval = Time.realtimeSinceStartup;
        }
    }
}