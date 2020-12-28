using UnityEngine;

public class ScreenCamera : MonoBehaviour
{
    public float match = 0f;        //0 - 1 之間 0表示寬度完全不變 1表示高度完全不變
    public float standardWidth = 640;     //標準分辨率寬
    public float standardHeight = 960;     //標準分辨率高

    //以FieldOfView或者orthographicSize 為標準
    public float maxHeightFieldOfView = 0;                 //最大的縱向投影
    public float minHeightFieldOfView = 0;
    public float maxWidthFieldOfView = 0;
    public float minWidthFieldOfView = 0;

    //最大橫向角度
    protected float maxHFov = 0;
    //最小横向角度
    protected float minHFov = 0;
    //最大縱向角度
    protected float maxVFov = 0;
    //最小縱向角度
    protected float minVFov = 0;

    protected Camera theCamera;
    protected float standardAspect;  //標準的寬高比
    protected float standardHFov;     //標準的橫向投影角度
    protected float standardVFov;      //縱向投影角度
    protected float screenAspect;       //當前寬高比

    void Awake()
    {
        theCamera = GetComponent<Camera>();
        standardAspect = standardWidth / standardHeight;

        standardVFov = theCamera.orthographic ? theCamera.orthographicSize : theCamera.fieldOfView;                   //fieldOfView默认会保持不变
        standardHFov = standardAspect * standardVFov;       //保持橫向角度不變 動態修改縱向角度

        maxVFov = maxHeightFieldOfView;
        minVFov = minHeightFieldOfView;
        maxHFov = maxWidthFieldOfView * standardAspect;
        minHFov = minWidthFieldOfView * standardAspect;
        screenAspect = theCamera.aspect;           //當前的寬高比 

        CameraMatch();
    }

    //匹配寬度 高度會上下調整
    protected void CameraMatch()
    {
        float targetAspect = screenAspect;          //能成功匹配的情況下 當前的寬高比是不能變的
        float targetHFov = standardVFov * targetAspect;         //standrdVFov既是標準的fieldOfView 也是當前的fieldOfView
        targetHFov = (targetHFov - standardHFov) * match + standardHFov;
        float targetVFov = targetHFov / targetAspect;

        if (maxVFov > 0 && targetVFov > maxVFov)
        {
            //縱向可視範圍太大   
            targetVFov = maxVFov;
            targetHFov = targetVFov * targetAspect;
            //橫向範圍變小

            if (minHFov > 0 && targetHFov < minHFov)
            {
                //無法匹配 只有不等比縮放 修改targetAspect
                Debug.Log("無法匹配");

                targetAspect = minHFov / maxVFov;
            }
        }
        else if (minVFov > 0 && targetVFov < minVFov)
        {
            //縱向可視範圍過小
            targetVFov = minVFov;
            targetHFov = targetVFov * targetAspect;
            //橫向範圍變大

            if (maxHFov > 0 && targetHFov > maxHFov)
            {
                Debug.Log("無法匹配");

                targetAspect = maxHFov / minVFov;
            }
        }

        if (targetAspect != screenAspect)
        {
            theCamera.aspect = targetAspect;
        }

        if (theCamera.orthographic)
        {
            theCamera.orthographicSize = targetVFov;
        }
        else
        {
            theCamera.fieldOfView = targetVFov;
        }
    }
}
