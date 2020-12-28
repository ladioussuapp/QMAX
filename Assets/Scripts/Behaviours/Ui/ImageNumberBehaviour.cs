using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 圖片字體元件
/// </summary>
public class ImageNumberBehaviour : MonoBehaviour
{
    public List<Sprite> Textures;
    public bool IsFormatNum = false;

    public Vector2 MarkSize = new Vector2(15f, 15f);

    private int number = 0;
    public int Number
    {
        set
        {
            SetValue(value);
        }
        get { return number; }
    }

    private void SetValue(int value)
    {
        if (value == number)
            return;

        number = value;
        GameObject ga = transform.GetChild(0).gameObject;
        string numstr = IsFormatNum? string.Format("{0:N0}", value): value.ToString();
        char[] str = numstr.ToCharArray();
        //char[] str = value.ToString().ToCharArray();
        GameObject targetGA = null;

        if (str.Length > transform.childCount)
        {
            //補全需要的Image
            for (int i = 0, n = str.Length - transform.childCount; i < n; i++)
            {
                targetGA = Instantiate(ga);
                targetGA.transform.SetParent(transform);
                targetGA.transform.localScale = new Vector3(1, 1, 1);
                targetGA.transform.localPosition = new Vector3(0, 0, 0);
                targetGA.name = "Num" + (transform.childCount - 1);
            }
        }
        Vector2 befsize = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
        for (int i = 0, n = transform.childCount; i < n; i++)
        {
            targetGA = transform.GetChild(i).gameObject;
            targetGA.SetActive(i < str.Length);
            if (i >= str.Length)
                continue;

            LayoutElement layer = targetGA.GetComponent<LayoutElement>();
            RectTransform markima = targetGA.GetComponent<RectTransform>();

            if (str[i] != ',')
            {
                if (markima != null)
                    markima.sizeDelta = befsize;
                if (layer != null)
                {
                    layer.preferredWidth = befsize.x;
                    layer.preferredHeight = befsize.y;
                }
                targetGA.GetComponent<Image>().sprite = Textures[(int)char.GetNumericValue(str[i])];
            }       
            else
            {
                targetGA.GetComponent<Image>().sprite = Textures[10];

                if (layer != null && markima != null)
                {
                    layer.preferredWidth = MarkSize.x;
                    layer.preferredHeight = MarkSize.y;

                    markima.sizeDelta = MarkSize;
                }
            }
                
            //Sprite t2d = Textures[(int)char.GetNumericValue(str[i])];
            //targetGA.GetComponent<Image>().sprite = t2d;
            //Sprite.Create(t2d, new Rect(0, 0, t2d.width, t2d.height), new Vector2(0.5f, 0.5f));
        }
    }

}
