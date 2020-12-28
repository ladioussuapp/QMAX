using UnityEngine;
using System.Collections;
using System;

public class SimpleColorAnimation : SimpleAnimationBase
{
    public Color32 From = Color.white;
    public Color32 To = Color.white;
    public bool useShareMaterial = true;
    Material material;
    string materialColorName = "";
    SpriteRenderer spriteRenderer;
    Action<float> UpdateHandler;

    // Use this for initialization
    void Start()
    {
        Renderer render = GetComponent<Renderer>();

        if (render != null)
        {
            if (render is SpriteRenderer)
            {
                spriteRenderer = (SpriteRenderer)render;
                UpdateHandler = UpdateSpriteColor;
            }
            else
            {
                material = useShareMaterial ? render.sharedMaterial : render.material;
                materialColorName = GetMaterialColorName(material);

                if (materialColorName != "")
                {
                    UpdateHandler = UpdateMaterialColor;
                }
            }
        }

        Reset();
    }

    void UpdateMaterialColor(float elapsedRate_)
    {
        material.SetColor(materialColorName, Color32.Lerp(From, To, elapsedRate_));
    }

    void UpdateSpriteColor(float elapsedRate_)
    {
        spriteRenderer.color = Color32.Lerp(From, To, elapsedRate_);
    }

    protected override void UpdateAnimation(float elapsedRate_)
    {
        base.UpdateAnimation(elapsedRate_);

        if (UpdateHandler != null)
        {
            UpdateHandler(elapsedRate_);
        }
    }

    public string GetMaterialColorName(Material mat)
    {
        string[] propertyNames = { "_Color", "_TintColor", "_EmisColor" };

        if (mat != null)
            foreach (string name in propertyNames)
                if (mat.HasProperty(name))
                    return name;
        return null;
    }
}
