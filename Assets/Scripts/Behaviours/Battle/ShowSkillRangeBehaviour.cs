using Com4Love.Qmax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 顯示技能影響範圍
/// </summary>
public class ShowSkillRangeBehaviour : MonoBehaviour
{
    /// <summary>
    /// 這個層的RectTransform必須和Elements一致
    /// </summary>
    public RectTransform Layer;

    private GameObject prefab;
    private GameController gc;

    private Stack<RectTransform> unusedPool;
    private RectTransform[,] usingEff;

    private BoardBehaviour boardBeh;

    public void ShowEffAt(Position tile)
    {
        int row = (int)tile.Row;
        int col = (int)tile.Col;
        //Q.Log("ShowEffAt {0}, {1}", row, col);
        //該位置已經有顯示效果了
        if (usingEff[row, col] != null)
        {
            return;
        }

        RectTransform rect = null;
        if (unusedPool.Count > 0)
            rect = unusedPool.Pop();
        else
        {
            rect = Instantiate(prefab).transform as RectTransform;
            Sprite s = gc.AtlasManager.GetSprite(Atlas.Tile, "Selected");
            rect.GetComponent<Image>().sprite = s;
            rect.SetParent(Layer);
        }

        usingEff[row, col] = rect;
        boardBeh.SetElementAnchor(row, col, rect);
        rect.gameObject.SetActive(true);
    }

    public void ClearEffAt(int row, int col)
    {
        if (usingEff[row, col] != null)
        {
            RectTransform rect = usingEff[row, col];
            usingEff[row, col] = null;
            rect.gameObject.SetActive(false);
            unusedPool.Push(rect);
        }
    }

    public void ClearAll()
    {
        for (int r = 0, n = usingEff.GetLength(0); r < n; r++)
        {
            for (int c = 0, nn = usingEff.GetLength(1); c < nn; c++)
            {
                ClearEffAt(r, c);
            }
        }
    }

    void Start()
    {
        boardBeh = GetComponent<BoardBehaviour>();
        prefab = Layer.GetChild(0).gameObject;
        gc = GameController.Instance;
        unusedPool = new Stack<RectTransform>();
        usingEff = new RectTransform[7, 7];

        gc.ViewEventSystem.ControlSkillRangeEvent += OnControlSkillRangeEvent;
    }


    private void OnControlSkillRangeEvent(int display, List<Position> list)
    {
        Q.Assert(display == 1 || display == 2, "ShowSkillRangeBehaviour:OnControlSkillRangeEvent assert1, display={0}", display);

        if (display == 2)
        {
            ClearAll();
        }
        else if (display == 1)
        {
            for (int i = 0, n = list.Count; i < n; i++)
            {
                ShowEffAt(list[i]);
            }
        }
    }


    private String VectorToString(IList list)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0, n = list.Count; i < n; i++)
        {
            sb.Append(list[i]);
            sb.Append(",");
        }
        return sb.ToString();
    }


    void OnDestroy()
    {
        gc.ViewEventSystem.ControlSkillRangeEvent -= OnControlSkillRangeEvent;
    }
}
