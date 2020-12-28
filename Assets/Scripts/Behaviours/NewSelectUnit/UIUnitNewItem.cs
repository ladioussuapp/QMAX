/**
 * 描述：选择英雄界面里每单个英雄类
 * */

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;
using Com4Love.Qmax.Tools;

public class UIUnitNewItem : MonoBehaviour {

    public Image itemBg;
    //技能图标
    public Image SkillIcon;
    //显示哪个英雄
    public Image body;

    public int m_index;
    public Image button;
    protected UnitConfig data;
    protected SkillConfig skillData;
    private ColorType m_defaluteColorType;
	// Use this for initialization
    void Start()
    {

    }

    //技能数据
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
            m_defaluteColorType = data.UnitColor;
            UpdateColor();
            UpdateBody();
            UpdateIcon();
        }
    }
    public void setDefault(ColorType colorType)
    {
        Data = null;
        m_defaluteColorType = colorType;
        UpdateColor();
        UpdateBody();
        UpdateIcon();
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

    protected bool locked = false;

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
        if(data == null)
        {
            SkillIcon.gameObject.SetActive(false);
            button.GetComponent<Button>().enabled = false;
            return;
        }
        button.GetComponent<Button>().enabled = true;
        SkillIcon.gameObject.SetActive(true);
        if (SkillData.ResourceIcon == "")
        {
            return;
        }

        SkillIcon.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.Tile, SkillData.ResourceIcon);
        SkillIcon.SetNativeSize();
    }
    protected void UpdateBody()
    {
        if(this.Data != null)
        {
            string url = string.Format("Textures/UIUnitSelect/XR_body_{0}", Data.ResourceIcon);
            body.sprite = Resources.Load<Sprite>(url);
            body.SetNativeSize();
            string itemBgName = "XR_Item_"+this.Data.UnitColor;
            itemBg.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UISelectUnitNew, itemBgName);
            itemBg.gameObject.SetActive(false);
            itemBg.SetNativeSize();
        }
        else
        {
            itemBg.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UISelectUnitNew, "XR_New031");
            itemBg.SetNativeSize();
            itemBg.gameObject.SetActive(false);

            string bodyName = "XR_body_" + this.m_defaluteColorType;
            string url = "Textures/UIUnitSelect/" + bodyName;
            body.sprite = Resources.Load<Sprite>(url);
            body.SetNativeSize();
        }
    }

    //更新颜色
    protected void UpdateColor()
    {
        return;
    }

    void Awake()
    {
        rectTransform = this.transform as RectTransform;
    }

    public void OnDestroy()
    {

    }
    public RectTransform rectTransform { get; set; }
}
