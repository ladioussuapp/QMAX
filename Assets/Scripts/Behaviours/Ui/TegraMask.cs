using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Com4Love.Qmax;

public class TegraMask : MonoBehaviour {
    string DefaultMaterialPath = "Materials/TegraMaskSource";
    string FontMaterialPath = "Materials/TegraFontMaskSource";
    string DefaultMaterialName = "";
    string FontMaterialName = "";
    public bool DebugLog = false;

    bool MaterialDirty = false;

    [NonSerialized]
    [HideInInspector]
    public Material MaskMaterial;

    [NonSerialized]
    [HideInInspector]
    public Material FontMaskMaterial;
 
    [NonSerialized]
    [HideInInspector]
    public Canvas Canvas;       //動態怎麼獲取上層的canvas
    //Mask Mask;
    RectTransform rectTransform;

    ///// <summary>
    ///// 如果自己的child也是個 TegraMask
    ///// </summary>
    List<TegraMask> ChildTMasks;

    /// <summary>
    /// 父層的mask。為null表示當前為一個頂層的mask。只有底層的mask才由update裡驅動自己計算矩形遮罩
    /// </summary>
    [HideInInspector]
    public TegraMask ParentMask;

    /// <summary>
    /// 自己的剪裁區域
    /// </summary>
    Rect clipRect;
   
    void Awake()
    {
        //Mask = GetComponent<Mask>();
        rectTransform = GetComponent<RectTransform>();
        clipRect = new Rect();
        DefaultMaterialName = string.Format("{0}-{1}-default", DefaultMaterialPath, name);
        FontMaterialName = string.Format("{0}-{1}-font", DefaultMaterialPath, name);

        //一個mask保持一個材質的引用。此mask下所有的child，公用此材質
        Material sourceMaterial = Resources.Load<Material>(DefaultMaterialPath);
        MaskMaterial = new Material(sourceMaterial);
        MaskMaterial.name = DefaultMaterialName;

        sourceMaterial = Resources.Load<Material>(FontMaterialPath);
        FontMaskMaterial = new Material(sourceMaterial);
        FontMaskMaterial.name = FontMaterialName;
    }

    //待改為： 去找上層TegraMask，註冊進去，上層的TegraMask在執行剪裁計算時會對子層的shader也執行一次裁剪
    public void OnEnable()
    {
        //TODO 待整理，需要獲取到物體的canvas
        CacheCanvas();
 
        FindParentMaskAndRegis(true);
        SetMaterialDirty();
    }

    void CacheCanvas()
    {
        Graphic graphic = GetComponent<Graphic>();

        if (graphic != null)
        {
            Canvas = graphic.canvas;
        }
    }

    public void OnDisable()
    {
        FindParentMaskAndRegis(false);
    }

    /// <summary>
    /// 設置完後 mask下所有的圖片材質都會刷新。
    /// </summary>
    public void SetMaterialDirty()
    {
        MaterialDirty = true;
    }

    /// <summary>
    /// 立刻設置材質
    /// </summary>
    /// <param name="graphicsRoot"></param>
    public void RegisteredGraphics(Transform graphicsRoot)
    {
        SetGraphicsMaterial(graphicsRoot , false);
    }

    void setMaterialDirty()
    {
        SetGraphicsMaterial();
    }

    public void RegisteredTegraMask(TegraMask mask)
    {
        if(ChildTMasks == null)
        {
            ChildTMasks = new List<TegraMask>();
        }

        if (ChildTMasks.IndexOf(mask) == -1)
        {
            ChildTMasks.Add(mask);
            mask.ParentMask = this;
        }
    }

    public void UnRegisteredTegraMask(TegraMask mask)
    {
        if (ChildTMasks != null && mask != null)
        {
            ChildTMasks.Remove(mask);
            mask.ParentMask = null;
        }
    }

    /// <summary>
    /// 每個TegraMask在被激活的時候，都需要去父層的mask註冊自己的存在。父層的mask會在每幀計算完自身的顯示矩形後，傳給子的mask。
    /// </summary>
    void FindParentMaskAndRegis(bool register)
    {
        TegraMask[] TMaskInParent = GetComponentsInParent<TegraMask>();
        TegraMask TMask;

        for (int i = 0; i < TMaskInParent.Length; i++)
        {
            TMask = TMaskInParent[i];

            if (TMask != null && TMask != this)
            {
                if (register)
                {
                    TMask.RegisteredTegraMask(this);
                }
                else
                {
                    TMask.UnRegisteredTegraMask(this);
                }
            }
        }
    }

    /// <summary>
    /// 可以給自己所有的children添加材質，也可以給傳進來
    /// </summary>
    /// <param name="root"></param>
    void SetGraphicsMaterial(Transform root = null, bool needCheck = true)
    {
        root = root == null ? transform : root;

        MaskableGraphic[] MaskableGraphics = root.GetComponentsInChildren<MaskableGraphic>(true);
        MaskableGraphic graphic;
        int count = 0;      //測試用的計數器

        for (int i = 0; i < MaskableGraphics.Length; i++)
        {
            graphic = MaskableGraphics[i];
 
            if (needCheck && !CheckGraphic(graphic))
            {
                //Debug.Log(graphic.ToString() + "  continue");
                continue;
            }

            //Debug.Log(graphic.ToString() + "  setMatial");

            //圖片，text ,rawimage 設置不同的材質
            if (graphic is Image)
            {
                graphic.material = MaskMaterial;
            }
            else if (graphic is Text)
            {
                graphic.material = FontMaskMaterial;
            }
            else if (graphic is RawImage)
            {
                graphic.material = MaskMaterial;
            }

            count++;
        }

        Q.Log("TegraMask：{0}    刷新材質數量：{1}", name, count);
    }

    bool CheckGraphic(MaskableGraphic graphic)
    {
        RectTransform t = graphic.rectTransform;
        RectTransform pT = t.parent as RectTransform;

        while (pT != null)
        {
            TegraMask tm = pT.GetComponent<TegraMask>();
            pT = pT.parent as RectTransform;

            if (tm != null)
            {
                //如果下層也是個TegraMask，則不改變下層的材質
                return tm == this;
            }
        }

        return false;
    }

    /// <summary>
    /// 執行遮罩剪裁。需要改為，當前的TegraMask剪裁自己的材質，並且要剪裁自己下面所有的TegraMask的材質。
    /// 默認同一個canvas
    /// </summary>
    public void CutOut(Rect clipRect)
    {
        MaskMaterial.SetFloat("_MinX", clipRect.x);
        MaskMaterial.SetFloat("_MinY", clipRect.y);
        MaskMaterial.SetFloat("_MaxX", clipRect.xMax);
        MaskMaterial.SetFloat("_MaxY", clipRect.yMax);

        FontMaskMaterial.SetFloat("_MinX", clipRect.x);
        FontMaskMaterial.SetFloat("_MinY", clipRect.y);
        FontMaskMaterial.SetFloat("_MaxX", clipRect.xMax);
        FontMaskMaterial.SetFloat("_MaxY", clipRect.yMax);

#if UNITY_EDITOR
        if (DebugLog)
        {
            Debug.Log(clipRect);
        }
#endif
    }

    public void CalcClipRect()
    {
        Vector2 vect2Tmp;

        if (Canvas == null)
        {
            CacheCanvas();
            return;
        }

        if (Canvas.worldCamera == null)
        {
            return;
        }

        vect2Tmp = Canvas.worldCamera.WorldToScreenPoint(rectTransform.position);

        clipRect.Set(vect2Tmp.x + rectTransform.rect.x * Canvas.scaleFactor * rectTransform.localScale.x
            , vect2Tmp.y + rectTransform.rect.y * Canvas.scaleFactor * rectTransform.localScale.y
            , rectTransform.rect.width * Canvas.scaleFactor * rectTransform.localScale.x
            , rectTransform.rect.height * Canvas.scaleFactor * rectTransform.localScale.y);
    }

    public void CalcClipRect(Rect pClipRect)
    {
        CalcClipRect();     //接受到父層傳來的裁剪區域時，先計算自己的區域，再將自己的區域與父層區域計算矩形交集。得出的結果才為最終的顯示區域

        clipRect = pClipRect;       //TODO  暫時直接使用父層的剪裁區域
    }

    /// <summary>
    /// 父層驅動子層計算遮罩。調用此方法時，需要保證 clipRect 已經被計算過了
    /// </summary>
    void ClipRectChildMasks()
    {
        if (ChildTMasks != null)
        {
            for (int i = 0; i < ChildTMasks.Count; i++)
            {
                TegraMask tMask = ChildTMasks[i];

                if (tMask != null)
                {
                    tMask.CalcClipRect(clipRect);
                }
            }
        }
    }

    //遮罩邏輯為。父層mask計算自己的Rect。並傳給子層mask。子層計算完自己的mask，並且計算自身Rect與父層Rect的交集，此交集Rect才是最終的Rect
    void Update()
    {
        if (ParentMask == null)
        {
            CalcClipRect();
            ClipRectChildMasks();
        }

        if (MaterialDirty)
        {
            MaterialDirty = false;
            setMaterialDirty();
        }
    }

    public void LateUpdate()
    {
        CutOut(clipRect);
    }
 
}


