using UnityEngine;
using System.Collections;
using Com4Love.Qmax;

public class MapSkyBehaviour : MonoBehaviour {
    public MeshRenderer Background;
    public MeshRenderer PreBackground;
    
    [HideInInspector]
    public string CutTextureName;

    private Material CutMaterial;
    private Material BackgroundM;
    private Material PreBackgroundM;
    private Texture texture;

    void Awake()
    {
        if (Background == null || PreBackground == null)
        {
            return;
        }

        CutMaterial = BackgroundM = Background.materials[0];
        PreBackgroundM = PreBackground.materials[0];
    }
 
    public void ChangeTexture(string textureName)
    {
        if (CutTextureName == textureName)
        {
            return;
        }

        CutTextureName = textureName;
        LeanTween.cancel(PreBackground.gameObject);
        LeanTween.cancel(Background.gameObject);

        if (CutMaterial == BackgroundM)
        {
            CutMaterial = PreBackgroundM;
            PreBackground.gameObject.SetActive(true);

            LeanTween.alpha(Background.gameObject, 0f, .5f).setOnComplete(delegate()
            {
                Background.gameObject.SetActive(false);
            });

            LeanTween.alpha(PreBackground.gameObject, 1f, .5f);
        }
        else
        {
            CutMaterial = BackgroundM;
            Background.gameObject.SetActive(true);

            LeanTween.alpha(PreBackground.gameObject, 0f, .5f).setOnComplete(delegate() {
                PreBackground.gameObject.SetActive(false);     
            });

            LeanTween.alpha(Background.gameObject, 1f, .5f);
        }

        texture = Resources.Load<Texture>(textureName);
        Q.Assert(texture != null, "背景貼圖缺失：" + textureName);

        CutMaterial.mainTexture = Resources.Load<Texture>(textureName);
    }
}
