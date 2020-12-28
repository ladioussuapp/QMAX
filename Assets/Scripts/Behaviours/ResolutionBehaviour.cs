using UnityEngine;
using System.Collections;

/// <summary>
/// 渲染分辨率指定 需要有一幀的黑屏去切換分辨率
/// </summary>
public class ResolutionBehaviour : MonoBehaviour {
    public int DESIGN_WIDTH = 640;
    public int DESIGN_HEIGHT = 960;
    [Tooltip("最大縮放係數")]
    public float MAX_SCALE_RATIO = 2;

#if !UNITY_EDITOR
    // Use this for initialization
    void Start()
    {
        float width = Screen.currentResolution.width;
        float height = Screen.currentResolution.height;

        float cutRatio = width / height;           //寬高
        float designRatio = (float)DESIGN_WIDTH / (float)DESIGN_HEIGHT;
        float targetScale = 1f;
        Resolution targetResolution = new Resolution();

        if (width > DESIGN_WIDTH && height > DESIGN_HEIGHT)
        {
            if (cutRatio < designRatio)
            {
                //更窄
                targetScale = width / DESIGN_WIDTH;
            }
            else if (cutRatio > designRatio)
            {
                //更寬
                targetScale = height / DESIGN_HEIGHT;
            }

            //如果手機分辨率特別大時，限制縮放倍數
            targetScale = Mathf.Min(targetScale, MAX_SCALE_RATIO);

            targetResolution.width = (int)(width / targetScale);
            targetResolution.height = (int)(height / targetScale);

            //Debug.Log("目標分辨率：" + targetResolution.width + "x" + targetResolution.height);
            Screen.SetResolution(targetResolution.width, targetResolution.height, true);
        }
    }
#endif
 
}
