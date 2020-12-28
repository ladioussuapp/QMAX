using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CloneImage : MonoBehaviour
{
    public Image Target;
    public Image Self;

    public void Awake()
    {
        Self = GetComponent<Image>();
    }
 
    public void OnEnable()
    {
        Clone();
    }

    void Clone()
    {
        if (Target == null || Self == null)
        {
            return;
        }

        Self.sprite = Target.sprite;
        RectTransform rT = Self.rectTransform;
        rT.anchoredPosition3D = Target.rectTransform.anchoredPosition3D;
        rT.localScale = Target.rectTransform.localScale;
    }
}
