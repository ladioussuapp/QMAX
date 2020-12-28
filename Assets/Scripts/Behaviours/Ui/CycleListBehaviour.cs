using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 無限循環的列表（現在只支持垂直方向）
/// </summary>
public class CycleListBehaviour : MonoBehaviour {

    /// <summary>
    /// Item重置代理
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="index"></param>
    public delegate void ItemResetDelegate(Transform transform, int index);
    public event ItemResetDelegate OnItemReset;

    /// <summary>
    /// 滾動層
    /// </summary>
    public ScrollRect Scroll;

    /// <summary>
    /// 佔位元件
    /// </summary>
    public LayoutElement PlaceholderElement;

    /// <summary>
    /// Item數量
    /// </summary>
    public int MaxItemCount;

    /// <summary>
    /// 滑動的臨界值(用於判定Item是否越出區域)
    /// </summary>
    public float Threshold = 200;

    /// <summary>
    /// 插入索引UP
    /// </summary>
    private int insertUp;

    /// <summary>
    /// 插入索引DOWN
    /// </summary>
    private int insertDown;

    private int tmpChildCount;
    private float anchPos;
    VerticalLayoutGroup vlg;

    void Start()
    {
        insertUp = insertDown = tmpChildCount = 0;
        anchPos = ((RectTransform)transform).anchoredPosition.y;
        vlg = transform.GetComponent<VerticalLayoutGroup>();
    }


	void Update () {
        if (Scroll == null)
            return;
 
        if (vlg == null)
            return;

        // 元素數量(含占位元素)
        int childCount = transform.childCount;

        if (childCount <= 1)
        {
            return;
        }

        if (tmpChildCount != childCount)
        {
            insertDown += (childCount - tmpChildCount);
            tmpChildCount = childCount;
        }

        float curAnchPos = ((RectTransform)transform).anchoredPosition.y;
        if (curAnchPos == anchPos)
            return;

        // ScrollRect的高度
        RectTransform scrollTtransform = (RectTransform)Scroll.transform;
        float scrollHeight = scrollTtransform.sizeDelta.y;

        if (curAnchPos > anchPos)
        {
            RectTransform childTtransform = (RectTransform)transform.GetChild(1);
            Vector2 localPoint = LocalPointInScrollRect(childTtransform.position);
            // 向上移動超出臨界值
            if (localPoint.y > Threshold && PlaceholderElement != null)
            {
                if (insertDown - 1 < MaxItemCount)
                {
                    float changeValue = childTtransform.sizeDelta.y + vlg.spacing;
                    PlaceholderElement.minHeight += changeValue;
                    // 把元素放在最後一位
                    childTtransform.SetSiblingIndex(childCount - 1);

                    if (OnItemReset != null)
                    {
                        // 列表項重置回調
                        OnItemReset(childTtransform, (insertDown - 1));
                    }
                    insertDown++;
                    insertUp++;
                }
            }
        }
        else
        {
            RectTransform childTtransform = (RectTransform)transform.GetChild(childCount - 1);
            Vector2 localPoint = LocalPointInScrollRect(childTtransform.position);
            // 向下移動超出臨界值
            if (localPoint.y + scrollHeight < -Threshold && PlaceholderElement != null)
            {
                float changeValue = childTtransform.sizeDelta.y + vlg.spacing;
                if (PlaceholderElement.minHeight - changeValue >= 0) 
                {
                    PlaceholderElement.minHeight -= changeValue;
                    // 把元素放在1位(注意,0位是佔位)
                    childTtransform.SetSiblingIndex(1);

                    insertUp--;
                    insertDown--;
                    if (OnItemReset != null)
                    {
                        // 列表項重置回調
                        OnItemReset(childTtransform, insertUp);
                    }
                }
            }
        }
        anchPos = curAnchPos;
	}

    public void setItemCount(int count)
    {
        MaxItemCount = count;
    }

    private Vector2 LocalPointInScrollRect(Vector3 position)
    {
        // 轉換成屏幕坐標
        Vector2 childPoint = Camera.main.WorldToScreenPoint(position);
        Vector2 localPoint;
        // 將屏幕坐標轉換成本地坐標
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)Scroll.transform, childPoint, Camera.main, out localPoint);
        return localPoint;
    }
}
