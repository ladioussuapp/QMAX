using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class SimpleSpriteAnimation : SimpleAnimationBase
{
    public Sprite[] Sprites;
    int count;
    SpriteRenderer spriteRenderer;
    public Sprite testSprite;

    /// <summary>
    /// 可能是ui的图片或者text
    /// </summary>
    Image image;
 
    public void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            image = GetComponent<Image>();
        }

        count = Sprites.Length;
    }

    void ImageUpdateHandler(Sprite sprite)
    {
        image.sprite = sprite;
    }

    void SpriteUpdateHandler(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    protected override void UpdateAnimation(float elapsedRate_)
    {
        base.UpdateAnimation(elapsedRate_);

        if (count == 0)
        {
            return;
        }

        int index = (int)(count * elapsedRate_);
        index = index >= count ? count - 1 : index;
        Sprite sprite = Sprites[index];

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
        else if (image != null)
        {
            image.sprite = sprite;
        }
    }
}
