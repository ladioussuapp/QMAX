using Com4Love.Qmax;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILinkLineBehaviour : MonoBehaviour {

    //不同的顏色不同的prefab?
    public GameObject EffectLinkPrefab;
    /// <summary>
    /// 素材線段的長度（Uint）
    /// </summary>
    public float SegmentLength = 0.64f;
    protected float MoveSpeed = 0f;

    //不同等級的UV移動速度
    public float[] LvlSpeed;
    //暫時不分顏色
    public Texture[] Textures;
    //選中的對象
    List<RectTransform> selection = new List<RectTransform>();
    List<GameObject> lines;
    Rect uvRect = new Rect(0,0,1,1);
    int lvl = 0;

    ColorType color;

    //combo等級索引  從1開始
    public int Lvl
    {
        get
        {
            return lvl;
        }
        set
        {
            if (lvl != value)
            {
                lvl = value;
                EffectChange(lvl, color);
            }
        }
    }

    public ColorType Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color != value)
            {
                color = value;
                EffectChange(lvl, color);
            }
        }
    }

    public void EffectChange(int lvl, ColorType color)
    {
        //Q.Log("{0}::{1}, {2} lv={3}, color={4}", GetType().Name, "EffectChange", 1, lvl, color);

        if (lvl == 0)
        {
            Clear();
            return;
        }
        else if (lines != null && lvl <= Textures.Length)
        {
            foreach (var item in lines)
            {
                //根據color與lvl改編貼圖
                item.GetComponent<RawImage>().texture = Textures[lvl - 1];     //根據等級改編貼圖
                MoveSpeed = LvlSpeed[lvl - 1];
            }

        }
        
    }

    //public Texture2D lineTexture;
    public RectTransform[] testTransforms;

    public void StartDraw(RectTransform transform_)
    {
        Clear();
        selection.Clear();
        selection.Add(transform_);

        Lvl = 1;
    }

    /// <summary>
    /// 清除最後一個點
    /// </summary>
    public void ClearLastPoint()
    {
        Q.Assert(selection.Count > 0);
        selection.RemoveAt(selection.Count - 1);
        GameObject segment = lines[lines.Count - 1];
        lines.RemoveAt(lines.Count - 1);
        Destroy(segment);
    }

    public void DrawToPoint(RectTransform transform_)
    {
        int sIndex = selection.IndexOf(transform_);
        int sCount = selection.Count;

        if (sCount == 0)
        {
            return;
        }

        if (sCount > 0 && sIndex == sCount - 1)
        {
            return;
        }

        if (sIndex > -1)
        {
            if (sIndex == sCount - 2)
            {
                ClearLastPoint();
            }
            return;
        }


        GameObject segment = Instantiate(EffectLinkPrefab);
        segment.transform.SetParent(transform);

        RectTransform startT = selection[sCount - 1];
        segment.transform.position = new Vector3(startT.position.x, startT.position.y, transform.position.z);

        Vector2 start = startT.position;
        Vector2 end = transform_.position;

        selection.Add(transform_);
        float distance = Vector3.Distance(end, start);
        float rad = Mathf.Atan2(end.y - start.y, end.x - start.x);
        float deg = rad * Mathf.Rad2Deg;
        //Q.Log("rad={0} deg={1}, len={2}", rad, deg, distance);
        float newScaleX = distance / SegmentLength;
        segment.transform.localScale = new Vector3(newScaleX, 1.0f, 0);
        segment.transform.localEulerAngles = new Vector3(0, 0, deg);
        lines.Add(segment);
    }

    public void Clear()
    {
        uvRect = new Rect(0, 0, 1, 1);
        if (lines != null)
        {
            foreach (var item in lines)
            {
                item.GetComponent<RawImage>().uvRect = uvRect;
            }
        }
        
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();
        selection.Clear();
    }

    void Awake()
    {
        lines = new List<GameObject>();
        selection = new List<RectTransform>();
    }

    void Update()
    {
        if (lines.Count <=0 )
        {
            return;
        }
        foreach (var item in lines)
        {
            uvRect = new Rect(uvRect.position - Vector2.right * MoveSpeed, uvRect.size);
            item.GetComponent<RawImage>().uvRect = uvRect;
        }
        //lineMaterial.mainTextureOffset += Vector2.right * MoveSpeed;

    }
    
}
