//#define EDITOR_TEST

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//如果發現是Tegra 芯片，動態的添加TegraMask腳本，自己實現遮罩
public class AutoTegraMask : MonoBehaviour
{
    // Use this for initialization
    static string graphicsDeviceName;

    void Awake()
    {
        graphicsDeviceName = SystemInfo.graphicsDeviceName.ToUpper();

        if (CheckIsTegra())
        {
            Create();
        }
    }

    public static bool CheckIsTegra()
    {
#if UNITY_EDITOR && EDITOR_TEST
        //限制在編輯器中才有測試的效果可以看
        return true;
#else
        return graphicsDeviceName.IndexOf("TEGRA") > -1;
#endif
    }
 
    void Create()
    {
        if (gameObject.GetComponent<TegraMask>() == null)
        {
            gameObject.AddComponent<TegraMask>();
            Image image = GetComponent<Image>();
            Mask mask = GetComponent<Mask>();

            if (image != null)
            {
                image.enabled = false;
            }

            if (mask != null)
            {
                mask.enabled = false;
            }
        }
    }
}