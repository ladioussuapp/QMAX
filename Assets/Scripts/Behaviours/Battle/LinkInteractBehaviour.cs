using Com4Love.Qmax;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/// <summary>
/// 控制連線行為的Behaviour
/// </summary>
public class LinkInteractBehaviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public RectTransform InteractLayer;

    public event Action<Position, Vector2> DownEvent;
    public event Action<Position, Vector2> DragEvent;
    public event Action<Vector2> UpEvent;

    /// <summary>
    /// 移動到取消區域的事件
    /// </summary>
    public event Action<Vector2> MoveToCancelAreaEvent;

    /// <summary>
    /// 取消連接事件
    /// </summary>
    public event Action<Vector2> CancelLinkEvent;

    private bool _isTouchDown = false;
    /// <summary>
    /// 是否正在
    /// </summary>
    public bool IsTouchDown { get { return _isTouchDown; } }

    private int numRow;
    private int numCol;

    private Vector2 lastEventPos;

    private Rect interactRect;

    private int interactLock = 0;

    public int PlusInteractLock() { interactLock++; return interactLock; }


    public int MinusInteractLock() { interactLock--; return interactLock; }

    /// <summary>
    /// 第一個手指觸控點ID
    /// </summary>
#if UNITY_EDITOR || UNITY_STANDALONE
    const int FIRST_POINTER_ID = -1;
#else
    const int FIRST_POINTER_ID = 0;
#endif

    public void OnPointerDown(PointerEventData eventData)
    {
        //只允許單指操作
        if (eventData.pointerId != FIRST_POINTER_ID)
            return;

        _isTouchDown = true;


        if (interactLock > 0)
        {
            return;
        }


        if (eventData.pointerCurrentRaycast.gameObject == null)
            return;

        int r = -1;//int.Parse(s[0]);
        int c = -1;//int.Parse(s[1]);

        string[] s = eventData.pointerCurrentRaycast.gameObject.name.Split('$');
        if (s.Length >= 2)
        {
            r = int.Parse(s[0]);
            c = int.Parse(s[1]);
        }

        s = eventData.pointerCurrentRaycast.gameObject.name.Split('_');
        if (s.Length >= 2)
        {
            r = int.Parse(s[0]);
            c = int.Parse(s[1]);
        }

        if (r == -1 && c == -1)
        {
            return;
        }

        Q.Log("down: [{0},{1}]", r, c);
        lastEventPos = eventData.position;

        Q.Assert(DownEvent != null);
        if (DownEvent != null)
            DownEvent(new Position(r, c), eventData.position);
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        //只允許單指操作
        if (eventData.pointerId != FIRST_POINTER_ID)
            return;

        _isTouchDown = false;


        if (interactLock > 0)
        {
            return;
        }
        CancelLinkEvent(eventData.position);

        lastEventPos = new Vector2(-1, -1);
        UpEvent(eventData.position);
        
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != FIRST_POINTER_ID || interactLock > 0)
        {
            return;
        }

        MoveToCancelAreaEvent(eventData.position);
        if (eventData.pointerCurrentRaycast.gameObject == null)
            return;


        string[] s = eventData.pointerCurrentRaycast.gameObject.name.Split('_');
        if (s.Length != 2)
        {
            return;
        }

        int r = 0, c = 0;
        try
        {
            r = int.Parse(s[0]);
            c = int.Parse(s[1]);
        }
        catch (Exception e)
        {
            Q.Warning(eventData.pointerCurrentRaycast.gameObject.name + "\n" + e.ToString());
            return;
        }

        if (r == -1 && c == -1)
        {
            return;
        }

        //說明之前沒有經過PointerDown
        if (lastEventPos == new Vector2(-1, -1))
        {
            return;
        }

        Vector2 startLocalPos, crtLocalPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            InteractLayer,
            eventData.position,
            eventData.enterEventCamera,
            out crtLocalPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            InteractLayer,
            lastEventPos,
            eventData.enterEventCamera,
            out startLocalPos);

        //Q.Log("drag: [{0},{1}]", r, c);
        List<Position> tileList = IsLinkableTiles(startLocalPos, crtLocalPos);
        lastEventPos = eventData.position;


        if (tileList == null || tileList.Count <= 1)
        {
            DragEvent(new Position(r, c), eventData.position);
        }
        else if (tileList.Count > 1)
        {
            //Q.Log("觸發連線算法優化邏輯, count={0}", tileList.Count);
            //連接算法優化
            for (int i = 0, n = tileList.Count; i < n; i++)
            {
                DragEvent(tileList[i], eventData.position);
            }
        }
    }


    void Start()
    {
        interactRect = InteractLayer.rect;
        //Q.Log("InteractLayer.rect={0}", InteractLayer.rect);

        numCol = 7;
        numRow = 7;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="startLocalPos"></param>
    /// <param name="endLocalPos"></param>
    /// <returns></returns>
    private List<Position> IsLinkableTiles(Vector2 startLocalPos, Vector2 endLocalPos)
    {
        List<Position> passedTiles = CalcPassTiles(startLocalPos, endLocalPos);
        if (passedTiles == null || passedTiles.Count <= 1)
            return null;

        bool isLinkable = true;
        //判斷前後的Tile是否相鄰
        for (int i = 1, n = passedTiles.Count; i < n; i++)
        {
            if (i >= 1)
            {
                Position tile = passedTiles[i];
                Position preTile = passedTiles[i - 1];

                if (Math.Abs(preTile.Row - tile.Row) > 1 || Math.Abs(preTile.Col - tile.Col) > 1)
                {
                    isLinkable = false;
                    break;
                }
            }
        }

        return isLinkable ? passedTiles : null;
    }


    /// <summary>
    /// 給出兩個屏幕坐標，算出這兩個屏幕坐標經過哪些Tile
    /// </summary>
    /// <param name="startLocalPos"></param>
    /// <param name="endLocalPos"></param>
    /// <returns></returns>
    private List<Position> CalcPassTiles(Vector2 startLocalPos, Vector2 endLocalPos)
    {
        List<Position> ret = new List<Position>();

        Position startTile = LocalPos2TilePos(startLocalPos);
        Position endTile = LocalPos2TilePos(endLocalPos);

        if (startTile.Row < 0 || startTile.Row >= numRow ||
            startTile.Col < 0 || startTile.Col >= numCol)
        {
            return ret;
        }

        ret.Add(startTile);

        if (startTile != endTile)
        {
            int minRow = startTile.Row > endTile.Row ? endTile.Row : startTile.Row;
            int maxRow = startTile.Row < endTile.Row ? endTile.Row : startTile.Row;
            int minCol = startTile.Col > endTile.Col ? endTile.Col : startTile.Col;
            int maxCol = startTile.Col < endTile.Col ? endTile.Col : startTile.Col;

            for (int r = minRow; r <= maxRow; r++)
            {
                for (int c = minCol; c <= maxCol; c++)
                {
                    Position pos = new Position(r, c);
                    Rect rect = CalcRectAt(r, c);
                    bool flg = SegmentAndRectIntersection(startLocalPos, endLocalPos, rect);
                    //Q.Log("{0}, {1}", pos, flg);
                    if (flg)
                    {
                        ret.Add(pos);
                    }
                }
            }
        }

        //Q.Log("start={0}, end={1}, c={2}", startTile, endTile, ret.Count);

        //需要根據和startTile的距離來排序
        ret.Sort(delegate(Position p1, Position p2)
        {
            Vector2 v1 = new Vector2(p1.Row, p1.Col);
            Vector2 v2 = new Vector2(p2.Row, p2.Col);
            Vector2 startV = new Vector2(startTile.Row, startTile.Col);
            float dis1 = Vector2.Distance(v1, startV);
            float dis2 = Vector2.Distance(v2, startV);
            if (dis1 > dis2)
                return 1;
            else if (dis1 < dis2)
                return -1;
            else
                return 0;
        });

        return ret;
    }


    private Rect CalcRectAt(int r, int c)
    {
        //這裡在計算每個tile所佔的範圍時，每個tile之間必須會留一定比例的縫隙
        float radio = 0.8f;
        float w = interactRect.width / numCol;
        float h = interactRect.height / numRow;
        float xMin = (c * w + interactRect.x) + w * (1 - radio) * 0.5f;
        float yMin = ((numRow - r - 1) * h + interactRect.y) + h * (1 - radio) * 0.5f;
        Rect ret = new Rect(xMin, yMin, w * radio, h * radio);
        return ret;
    }

    private Position LocalPos2TilePos(Vector2 localPos)
    {
        //         Q.Log("{0},{1}",
        //             localPos,
        //             interactRect);
        int c2 = (int)Math.Floor((localPos.x - interactRect.x) / (interactRect.width / numCol));
        //localPos.y的值是從上到下，值變小
        //Position.y的值是從上到下，值變大
        int r2 = numRow - (int)Math.Ceiling((localPos.y - interactRect.y) / (interactRect.height / numRow));

        //Q.Assert(r2 >= 0 && r2 < numRow, r2.ToString());
        //Q.Assert(c2 >= 0 && c2 < numCol, c2.ToString());
        return new Position(r2, c2);
    }


    /// <summary>
    /// 判斷線段與矩形是否相交
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    private static bool SegmentAndRectIntersection(Vector2 p1, Vector2 p2, Rect rect)
    {
        Vector2 intersection = Vector2.zero;

        Vector2 rectBottomLeft = new Vector2(rect.x, rect.y);
        Vector2 rectBottomRight = new Vector2(rect.x + rect.width, rect.y);
        Vector2 rectTopLeft = new Vector2(rect.x, rect.y + rect.height);
        Vector2 rectTopRight = new Vector2(rect.x + rect.width, rect.y + rect.height);

        bool ret = LineIntersection(p1, p2, rectBottomLeft, rectBottomRight, ref intersection);
        ret = ret || LineIntersection(p1, p2, rectBottomLeft, rectTopLeft, ref intersection);
        ret = ret || LineIntersection(p1, p2, rectTopLeft, rectTopRight, ref intersection);
        ret = ret || LineIntersection(p1, p2, rectTopRight, rectBottomRight, ref intersection);

        return ret;
    }


    /// <summary>
    /// 判斷線段是否相交
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="p4"></param>
    /// <param name="intersection"></param>
    /// <returns></returns>
    private static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)
    {
        float Ax, Bx, Cx, Ay, By, Cy, d, e, f, num, offset;
        float x1lo, x1hi, y1lo, y1hi;

        Ax = p2.x - p1.x;
        Bx = p3.x - p4.x;

        // X bound box test/
        if (Ax < 0)
        {
            x1lo = p2.x; x1hi = p1.x;
        }
        else
        {
            x1hi = p2.x; x1lo = p1.x;
        }

        if (Bx > 0)
        {
            if (x1hi < p4.x || p3.x < x1lo) return false;
        }
        else
        {
            if (x1hi < p3.x || p4.x < x1lo) return false;
        }

        Ay = p2.y - p1.y;
        By = p3.y - p4.y;

        // Y bound box test//
        if (Ay < 0)
        {
            y1lo = p2.y; y1hi = p1.y;
        }
        else
        {
            y1hi = p2.y; y1lo = p1.y;
        }

        if (By > 0)
        {
            if (y1hi < p4.y || p3.y < y1lo) return false;
        }
        else
        {
            if (y1hi < p3.y || p4.y < y1lo) return false;
        }

        Cx = p1.x - p3.x;
        Cy = p1.y - p3.y;
        d = By * Cx - Bx * Cy;  // alpha numerator//
        f = Ay * Bx - Ax * By;  // both denominator//

        // alpha tests//
        if (f > 0)
        {
            if (d < 0 || d > f) return false;
        }
        else
        {
            if (d > 0 || d < f) return false;
        }

        e = Ax * Cy - Ay * Cx;  // beta numerator//

        // beta tests //
        if (f > 0)
        {
            if (e < 0 || e > f) return false;
        }
        else
        {
            if (e > 0 || e < f) return false;
        }

        // check if they are parallel
        if (f == 0) return false;

        // compute intersection coordinates //
        num = d * Ax; // numerator //
        offset = same_sign(num, f) ? f * 0.5f : -f * 0.5f;   // round direction //
        intersection.x = p1.x + (num + offset) / f;

        num = d * Ay;
        offset = same_sign(num, f) ? f * 0.5f : -f * 0.5f;
        intersection.y = p1.y + (num + offset) / f;

        return true;
    }

    private static bool same_sign(float a, float b)
    {
        return ((a * b) >= 0f);
    }

}
