using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 透明玻璃效果 目前只支持掛在RawImage上 並且需要此圖片尺寸為全屏
/// 
/// </summary>
public class BlurEffect : MonoBehaviour
{
    public Camera TargetCamera;
    public Material BlurMaterial;
    public RawImage TargetImg;
    public float BlurDis;

    RenderTexture rTexture;
 
    // Use this for initialization
    void Start()
    {
        RenderTexture snapshotRT = GameController.Instance.EffectProxy.SnapshotTexture;
        rTexture = new RenderTexture(snapshotRT.width, snapshotRT.height, snapshotRT.depth);
        Graphics.Blit(snapshotRT, rTexture, BlurMaterial);
        GameController.Instance.EffectProxy.ClearSnapshotCache();

        TargetImg.enabled = true;
        TargetImg.texture = rTexture;
    }

    public void OnDestroy()
    {
        if (rTexture != null)
        {
            rTexture.Release();
        }
    }
}
