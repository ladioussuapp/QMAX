using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using System.Collections.Generic;

public class UiGemListItem : MonoBehaviour
{
    public Image IconImg;
    public Image GemImgPrefab;    //鑽石飛行特效    做prefab用
    public Text MsgText;
    public Text NumText;
    public Text ButtonText;
    public int Index = -1;
    public UIButtonBehaviour button;
    public UiGemList.Data data;
 
    public Color OutlineColorStateNomal;
    public Color OutlineColorStateDisable;
    public Outline TextEffectOutline;

    public event System.Action<UiGemListItem> OnBuyGem;

    public void Reset()
    {
        NumText.text = data.GemNum.ToString();
    }

    public void Start()
    {
        //button.enabled = false;
        //button.interactable = false;
        button.interactable = true;

        if (button.interactable == false)
        {
            //充值目前沒開啟，按鈕灰顯，描邊顏色也更改
            TextEffectOutline.effectColor = OutlineColorStateDisable;
        }
 
        button.onClick += button_onClick;
    }

    void button_onClick(UIButtonBehaviour button)
    {
        //購買鑽石
        if (OnBuyGem != null)
        {
            OnBuyGem(this);
        }
    }
 
    public void OnDestroy()
    {
        button.onClick -= button_onClick;
    }

    public void SetData(UiGemList.Data data)
    {
        this.data = data;

        MsgText.text = data.Msg;
        NumText.text = data.GemNum.ToString();
        ButtonText.text = Utils.GetText("￥") + data.Price.ToString();    //乘以折扣
     
        Sprite iconSprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIGem, data.Icon);

        if (iconSprite != null)
        {
            IconImg.sprite = iconSprite;
            IconImg.SetNativeSize();
        }
    }
}
