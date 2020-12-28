using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using UnityEngine.UI;
using System.Collections.Generic;

public class TileGrid : MonoBehaviour {
    public int RowCount;
    public int ColCount;
    public Vector2 CellSize;
    public Vector2 Spacing;

    [HideInInspector]
    public Position CenterPos;

    void Awake()
    {
        CenterPos = new Position((int)(RowCount * .5f), (int)(ColCount * .5f));

        RectTransform rt = transform as RectTransform;
        rt.sizeDelta = new Vector2(CellSize.x * ColCount + Spacing.x * (ColCount - 2), CellSize.y * RowCount + Spacing.y * (RowCount - 2));
    }

    //pos傳進來的值為基於某一點的偏移
    public Transform Add(RectTransform tileRT , Position pos , Vector3 scale )
    {
        tileRT.SetParent(transform);
        tileRT.localScale = scale;
        tileRT.sizeDelta = CellSize;
        tileRT.anchoredPosition = new Vector2(pos.Col * CellSize.x + pos.Col * Spacing.x , -(pos.Row * tileRT.sizeDelta.y + pos.Row * Spacing.y));

        return tileRT;
    }

    /// <summary>
    /// 暫時可以不使用此判斷
    /// </summary>
    /// <param name="poses"></param>
    /// <returns></returns>
    public bool CheckContainsAble(List<Position> poses)
    {
        Dictionary<int, int> maxCountInRow = new Dictionary<int, int>();
        Dictionary<int, int> maxCountInCol = new Dictionary<int, int>();

        for (int i = 0; i < poses.Count; i++)
        {
            Position pos = poses[i];

            if (!maxCountInCol.ContainsKey(pos.Col))
            {
                maxCountInCol.Add(pos.Col, 1);
            }
            else
            {
                maxCountInCol[pos.Col] = maxCountInCol[pos.Col] + 1;
            }

            if (!maxCountInRow.ContainsKey(pos.Row))
            {
                maxCountInRow.Add(pos.Row, 1);
            }
            else
            {
                maxCountInRow[pos.Row] = maxCountInRow[pos.Row] + 1;
            }
 
            if (maxCountInCol[pos.Col] > RowCount || maxCountInRow[pos.Row] >ColCount)
            {
                return false;
            }
        }

        return true;
    }

}
