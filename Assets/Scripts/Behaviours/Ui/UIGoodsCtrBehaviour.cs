using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIGoodsCtrBehaviour : MonoBehaviour
{
    public Image GoodsImage;
    public Text GoodsNum;

    LayoutElement layout;

    void Start()
    {
        FindLayout();
    }

    public void SetNativeSize()
    {
        GoodsImage.SetNativeSize();

        FindLayout();
        layout.preferredHeight = GoodsImage.preferredHeight;
        layout.preferredWidth = GoodsImage.preferredWidth;

    }

    public void SetImageSize(Vector2 size)
    {
        FindLayout();
        GoodsImage.GetComponent<RectTransform>().sizeDelta = size;

        layout.preferredWidth= size.x;
        layout.preferredHeight = size.y;
    }

    public void SetImageLayoutSize(Vector2 size)
    {
        FindLayout();
        layout.preferredWidth = size.x;
        layout.preferredHeight = size.y;
    }

    public Vector2 GetImageLayoutSize()
    {
        FindLayout();

        return new Vector2(layout.preferredWidth, layout.preferredHeight);
    }

    void FindLayout()
    {
        if(layout == null)
            layout = transform.GetComponent<LayoutElement>();

        if (layout == null)
            layout = gameObject.AddComponent<LayoutElement>();

    }

}
