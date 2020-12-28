using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;
using System.Collections.Generic;

//unit的相關操作都移到了這裡 本身佔位用，當需要顯示時load一個content。當被隱藏時，移除content
public class UIUpgradWinContentItem : ScrollRectItem
{
    [HideInInspector]
    public Button BodyButton;
    
    RectTransform Content;
    Image ImgBg;
    Image BodyColorBg;
    RectTransform PlaceHolder;
    Camera RenderCamera;     //顯示rendertext的攝像機
    RawImage RenderImg;

    string UnitPrefabName = "";
    RectTransform crtUnit;

    /// <summary>
    /// 在播放特效的時候創建下一等級的模型
    /// </summary>
    RectTransform nextLvUnit;

    /// <summary>
    /// crtUnit在對像池中的key
    /// </summary>
    string crtUnitKeyInPool;
    /// <summary>
    /// nextLvUnit在對像池中的key
    /// </summary>
    private string nextLvUnitKeyInPool;

    float AniOverTime = 0f;
    string CurrentAniName;
    string[] RadomAniName = new string[] { "Win", "Charge" };
    string ContentRenderKey = "UIUpgradWin::ContentRenderPrefab";
    bool touchAble = true;
    
    bool IsWaitForUpdate = false;
    bool DRAW_INVALIDE = false;
    bool CLEAR_INVALIDE = false;
    bool DATACHANGE_INVALIDE = false;
    bool BODYCHANGE_INVALIDE = false;

    public bool TouchAble
    {
        get
        {
            return touchAble;
        }
        set
        {
            touchAble = value;
        }
    }

    void InvalideVal()
    {
        if (!IsWaitForUpdate)
        {
            IsWaitForUpdate = true;
            StartCoroutine(DelayUpdate());
        }
    }

    IEnumerator DelayUpdate()
    {
        yield return new WaitForEndOfFrame();
        IsWaitForUpdate = false;

        if (DRAW_INVALIDE)
        {
            draw();
            DRAW_INVALIDE = false;
            DATACHANGE_INVALIDE = true;     //從對像池裡面取背景，可以認為是數據也更改了
        }

        if (CLEAR_INVALIDE)
        {
            clear();
            CLEAR_INVALIDE = false;
        }

        if (DATACHANGE_INVALIDE && ItemData != null && Content != null)
        {
            changeSkin();
            DATACHANGE_INVALIDE = false;
        }

        if (BODYCHANGE_INVALIDE && ItemData != null && Content != null)
        {
            bodyChange();
            BODYCHANGE_INVALIDE = false;
        }

        TegraMask tMask = GetComponentInParent<TegraMask>();

        if (tMask != null)
        {
            tMask.SetMaterialDirty();
        }
    }

    public void Draw()
    {
        DRAW_INVALIDE = true;
        InvalideVal();
    }

    void draw()
    {
        if (Content != null)
        {
            return;
        }
 
        Content = GameController.Instance.PoolManager.PrefabSpawn(ContentRenderKey) as RectTransform;
        Content.gameObject.SetActive(true);
        Content.SetParent(transform);
        Content.anchoredPosition3D = Vector3.zero;

        Content.localScale = new Vector3(1, 1, 1);
        ImgBg = Content.Find("ImgBg").GetComponent<Image>();
        BodyColorBg = Content.Find("Body/Bg").GetComponent<Image>();
        PlaceHolder = Content.Find("Body/PlaceHolder").GetComponent<RectTransform>();
        RenderImg = Content.Find("Body/Renderer").GetComponent<RawImage>();
        BodyButton = PlaceHolder.GetComponent<Button>();
        RenderCamera = Content.Find("Body/RenderCamera").GetComponent<Camera>();
        BodyButton.onClick.AddListener(OnBodyClick);

        if (RenderCamera.targetTexture == null)
        {
            RenderTexture rTexture = new RenderTexture(400, 400, 0);
            RenderCamera.targetTexture = rTexture;
            RenderImg.texture = rTexture;
        }

        RenderCamera.gameObject.SetActive(false);
        RenderImg.gameObject.SetActive(false);
    }
        
    public void Clear()
    {
        CLEAR_INVALIDE = true;
        InvalideVal();
    }

    void clear()
    {
        if (Content == null)
        {
            return;
        }

        AniOverTime = 0f;
        GameController.Instance.PoolManager.Despawn(Content);
        Content = null;
 
        if (BodyButton != null)
        {
            BodyButton.onClick.RemoveAllListeners();
        }

        if (crtUnit != null)
        {
            crtUnit.gameObject.layer = LayerMask.NameToLayer(Layer.Default);
            GameController.Instance.PoolManager.PushToInstancePool(crtUnitKeyInPool, crtUnit.transform);
            crtUnit = null;
        }

        if (nextLvUnit != null)
        {
            nextLvUnit.gameObject.layer = LayerMask.NameToLayer(Layer.Default);
            GameController.Instance.PoolManager.PushToInstancePool(nextLvUnitKeyInPool, nextLvUnit.transform);
            nextLvUnit = null;
        }
    }

    void changeSkin()
    {
        ImgBg.color = ItemData.BodyBgColor;
        BodyColorBg.color = ItemData.BodyBgColor;
    }


    void OnDestroy()
    {
        if (BodyButton != null)
        {
            BodyButton.onClick.RemoveAllListeners();
        }

        if (crtUnit != null && nextLvUnit == null)
        {
            //增加判斷 當有了nextLvUnit時，表示棄用當前的伙伴模型，不返回對像池
            crtUnit.gameObject.layer = LayerMask.NameToLayer(Layer.Default);
            GameController.Instance.PoolManager.PushToInstancePool(crtUnitKeyInPool, crtUnit.transform);
            crtUnit = null;
        }

        if (nextLvUnit != null)
        {
            nextLvUnit.gameObject.layer = LayerMask.NameToLayer(Layer.Default);
            GameController.Instance.PoolManager.PushToInstancePool(nextLvUnitKeyInPool, nextLvUnit.transform);
            nextLvUnit = null;
        }
    }

    protected override void OnDataChange()
    {
        base.OnDataChange();

        DATACHANGE_INVALIDE = true;
        InvalideVal();
    }

    public UIUpgradWinContentItemData ItemData
    {
        get
        {
            return data as UIUpgradWinContentItemData;
        }
    }

    /// <summary>
    /// 依賴外部調用
    /// </summary>
    public void BodyChange()
    {
        BODYCHANGE_INVALIDE = true;
        InvalideVal();
    }

    void bodyChange()
    {
        //一樣的模型 並且有小伙伴。
        if (UnitPrefabName == ItemData.uConfig.PrefabPath && crtUnit != null)
        {
            return;
        }

        if (nextLvUnit != null)
        {
            GetUnitCallBack(nextLvUnitKeyInPool, nextLvUnit);
            nextLvUnit = null;
            nextLvUnitKeyInPool = null;
        }
        else
        {
            GameController.Instance.PoolManager.GetUnitInstance(
                ItemData.uConfig,
                GetUnitCallBack
            );
        }
    }

    void GetUnitCallBack(string key, Transform trans)
    {
        RectTransform rT = trans as RectTransform;
        crtUnitKeyInPool = key;

        if (rT != null)
        {
            ////夥伴添加後設置parent
            UnitPrefabName = ItemData.uConfig.PrefabPath;
            rT.gameObject.SetActive(true);
            rT.SetParent(PlaceHolder.parent);
            rT.gameObject.layer = LayerMask.NameToLayer(Layer.TextureRenderer);

            rT.localScale = PlaceHolder.localScale;
            rT.anchorMax = PlaceHolder.anchorMax;
            rT.anchorMin = PlaceHolder.anchorMin;
            rT.anchoredPosition3D = PlaceHolder.anchoredPosition3D;
            RenderCamera.gameObject.SetActive(true);
            RenderImg.gameObject.SetActive(true);

            if (crtUnit != null)
            {
                //升级了   把老的伙伴模型直接銷毀
                Destroy(crtUnit.gameObject);
            }

            crtUnit = rT;
        }

        if (this.isSelect)
        {
            UnitAnimation();
        }
        else
        {
            AniBack();
        }
    }
 
    public void OnBodyClick()
    {
        if (!touchAble)
        {
            return;
        }

        //string radomA = radomAnimations[UnityEngine.Random.Range(0, radomAnimations.Length)];
        string RadomName = RadomAniName[UnityEngine.Random.Range(0, RadomAniName.Length)];
 
        UnitRadomAni(RadomName);
    }

    void UnitRadomAni(string aniName = "Win")
    {
        if (aniName.Equals(CurrentAniName))
        {
            List<string> newName = new List<string>();
            for (int name = 0; name < RadomAniName.Length; name++)
            {
                if (!RadomAniName[name].Equals(aniName))
                {
                    newName.Add(RadomAniName[name]);
                }
            }

            aniName = newName[UnityEngine.Random.Range(0, newName.Count)];
        }

        UnitAnimation(aniName);
    }


    public void UnitAnimation(string aniName = "Win")
    {
        AniPlay(aniName);
        CurrentAniName = aniName;

        if (AniOverTime == 0f)
        {
            AniOverTime = 2f;
            StartCoroutine(AutoAniBack());
        }
        else
        {
            AniOverTime = 2f;
        }
    }

    IEnumerator AutoAniBack()
    {
        while (AniOverTime > 0)
        {
            AniOverTime -= Time.deltaTime;
            yield return 0;
        }

        AniOverTime = 0f;
        AniBack();
    }

    void AniPlay(string aniName)
    {
        if (crtUnit != null)
        {
            Animator animator = crtUnit.GetComponent<Animator>();
            animator.Play(aniName);
        }
    }

    void AniBack()
    {
        string backName = "Idle";
        if (backName.Equals(CurrentAniName))
        {
            return;
        }

        AniPlay(backName);
        CurrentAniName = backName;
    }

    /// <summary>
    /// 播放夥伴升級後的聲音。  
    /// </summary>
    public void PlayLvlUpAudio()
    {
        //當unit升階換成其它夥伴後，聲音會被切斷，此處換成用AudioManager播放
        UnitSoundBehaviour us = crtUnit.GetComponent<UnitSoundBehaviour>();
        if (us != null)
        {
            AudioClip ac = us.GetUpgradeAudioClip();
            GameController.Instance.AudioManager.PlayMusic(ac);
        }
    }

    //將下一等級的伙伴預加載進來
    public void PreloadBody()
    {
        if (nextLvUnit != null)
        {
            return;
        }

        GameController.Instance.PoolManager.GetUnitInstance(ItemData.uConfig,
            delegate(string key, Transform ins)
            {
                nextLvUnitKeyInPool = key;
                nextLvUnit = ins as RectTransform;
                nextLvUnit.SetParent(crtUnit.transform.parent);
                //nextLvUnit.anchoredPosition = Vector2.right * 1080f;
                nextLvUnit.gameObject.SetActive(false);
            }
        );
    }

}

public class UIUpgradWinContentItemData : ScrollRectItemData
{
    public UnitConfig uConfig;
    //public UnitConfig nextUConfig;  //下一等級的的伙伴，用來預加載存放 PreloadNextLvlBody中使用
    public Color BodyBgColor;
}
