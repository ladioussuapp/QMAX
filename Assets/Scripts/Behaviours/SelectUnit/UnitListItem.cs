using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;

public class UnitListItem : MonoBehaviour
{
    [HideInInspector]
    public float Width;
    [HideInInspector]
    public float Height;
    public Image SkillIcon;
    public Image body;

    protected UnitConfig data;
    protected SkillConfig skillData;

    public SkillConfig SkillData
    {
        get
        {
            return GameController.Instance.Model.SkillConfigs[data.UnitSkillId];
        }
    }

    public UnitConfig Data
    {
        get
        {
            return data;
        }
        set
        {
            if (data == value)
            {
                return;
            }

            data = value;
            UpdateColor();
            UpdateBody();
            UpdateIcon();
        }
    }

    protected bool isSelect;
    public bool IsSelect
    {
        get
        {
            return isSelect;
        }
        set
        {
            if (isSelect == value)
            {
                return;
            }

            isSelect = value;
            UpdateColor();
        }
    }

    protected bool locked = true;

    public bool Locked
    {
        get
        {
            return locked;
        }
        set
        {
            if (locked == value)
            {
                return;
            }

            locked = value;
            UpdateIcon();
            UpdateColor();

        }
    }

    //技能图标
    protected void UpdateIcon()
    {
        if (SkillData.ResourceIcon == "")
        {
            return;
        }
        //SkillIcon.gameObject.SetActive(false);
        SkillIcon.gameObject.SetActive(!locked);
        SkillIcon.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.Tile, SkillData.ResourceIcon);
        SkillIcon.SetNativeSize();
    }

    //伙伴图标
    protected void UpdateBody()
    {
        GameController.Instance.QMaxAssetsFactory.CreteSelectUnitSprite(
        data, new Vector2(.5f, 1.0f),
        delegate(Sprite s)
        {
            body.sprite = s;
            float scaleRate = data.UnitScale;
            body.rectTransform.localScale = new Vector3(scaleRate, scaleRate,1);
            //body.rectTransform.localPosition = new Vector3(body.rectTransform.localPosition.x+data.OffsetCoordinate.x,
            //    body.rectTransform.localPosition.y+data.OffsetCoordinate.y, 0);
            body.rectTransform.localPosition = new Vector3(data.OffsetCoordinate.x, data.OffsetCoordinate.y, 0); 
            //body.SetNativeSize();
        });
    }

    protected void UpdateColor()
    {
        if (locked)
        {
            body.color = Color.black;
        }
        else
        {
            body.color = Color.white;
        }

        float tweenTo = isSelect ? 1f : 0f;

        LeanTween.value(SkillIcon.gameObject, SkillIcon.color.a, tweenTo, .3f).setOnUpdate(delegate(float val)
        {
            SkillIcon.color = new Color(SkillIcon.color.r, SkillIcon.color.g, SkillIcon.color.b, val);
        });
    }

    void Awake()
    {
        rectTransform = this.transform as RectTransform;
        Width = rectTransform.rect.width;
        Height = rectTransform.rect.height;

        SkillIcon.color = new Color(SkillIcon.color.r, SkillIcon.color.g, SkillIcon.color.b, 0f);
    }


    public RectTransform rectTransform { get; set; }
}
