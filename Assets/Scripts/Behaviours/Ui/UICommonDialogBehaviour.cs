using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Com4Love.Qmax;
using System;

public class UICommonDialogBehaviour : PopupEventCor
{
    public UIGoodsCtrBehaviour GoodsPrefab;

    public Transform Grid;

    public Text Info;
    public Text Title;

    List<UIGoodsCtrBehaviour> GoodsList = new List<UIGoodsCtrBehaviour>();

    public UIButtonBehaviour OKButton;
    public UIButtonBehaviour DontShowButton;

    public Action CloseEvent;

    public Action DontShowButtonEvent;

    public Transform Arrows;
	void Start () {

        OKButton.onClick += OnSureButton_Click;
        DontShowButton.onClick += OnDontShowButton_Click;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetInfo(string title, string info, List<Goods> datas,bool setNative = false,bool showDont = false)
    {
        Title.text = title;
        Info.text = info;

        DontShowButton.gameObject.SetActive(showDont);

        if(datas == null || datas.Count == 0)
            return;

        ///只有一个显示物品的时，放大显示///
        if (datas.Count == 1)
            datas[0].GoodsSpriteSize = datas[0].GoodsSpriteSize * 1.5f;

        float alldis = 0;

        Arrows.gameObject.SetActive(datas.Count > 3);
        for (int i = 0; i < datas.Count;i++ )
        {
            UIGoodsCtrBehaviour goods = Instantiate(GoodsPrefab) as UIGoodsCtrBehaviour;
            goods.GoodsNum.text = "X" + datas[i].Num;
            goods.GoodsImage.sprite = datas[i].GoodsSprite;

            goods.transform.SetParent(Grid);
            goods.transform.localScale = Vector3.one;
            goods.transform.localPosition = Vector3.zero;
            goods.transform.localRotation = Quaternion.identity;

            //Vector2 size = goods.GoodsImage.GetComponent<RectTransform>().sizeDelta;
            //Debug.Log(string.Format("image size  x is {0}  y is {1}",size.x ,size.y));
            Vector2 imageSize = goods.GoodsImage.GetComponent<RectTransform>().sizeDelta;
            if (setNative)
            {
                goods.SetNativeSize();
            }
            else if (datas[i].GoodsSpriteSize != Vector2.zero)
            {
                goods.SetImageSize(datas[i].GoodsSpriteSize);
            }
            else
            {
                goods.SetImageLayoutSize(imageSize);
            }

            ///计算显示的前3个物品///
            if (i < 3)
            {
                //alldis += datas[i].GoodsSpriteSize.x;
                alldis += goods.GetImageLayoutSize().x;
            }

            GoodsList.Add(goods);

        }

        HorizontalLayoutGroup group = Grid.GetComponent<HorizontalLayoutGroup>();
        ///物品之间的间隔数量///
        int disNum = datas.Count - 1;

        if (disNum > 2)
            disNum = 2;

        alldis += group.spacing * disNum;
        
        SetChildAlignment(group, alldis / 2);
            


    }

    void OnSureButton_Click(UIButtonBehaviour button)
    {
        GameController.Instance.AudioManager.PlayAudio("SD_ui_back1");
        Close();
    }

    void OnDontShowButton_Click(UIButtonBehaviour button)
    {
        if (DontShowButtonEvent != null)
        {
            DontShowButtonEvent();
        }
        Close();
    }

    public override void Close()
    {
        GameController.Instance.Popup.Close(PopupID.UICommonDialog);
   
        if (CloseEvent != null)
        {
            CloseEvent();
        }
    }

    void OnDestroy()
    {
        OKButton.onClick -= OnSureButton_Click;
        DontShowButton.onClick -= OnDontShowButton_Click;
        CloseEvent = null;
        DontShowButtonEvent = null;

    }

    public void SetDontShowEvent(Action<UIButtonBehaviour> dontevent)
    {

    }

    void SetChildAlignment(HorizontalLayoutGroup group, float alldis)
    {
        if (group == null)
            return;

        int disMax = 255;
        group.padding = new RectOffset(disMax - (int)alldis, 0, 0, 0);

    }
}

public class Goods
{
    public Sprite GoodsSprite;
    public string Num;

    /// <summary>
    /// x是宽，y是高///
    /// </summary>
    public Vector2 GoodsSpriteSize = Vector2.zero;
}
