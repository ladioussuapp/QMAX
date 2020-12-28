using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Audio;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Net.Protocols;
using Com4Love.Qmax;
using System;
using Com4Love.Qmax.Net.Protocols.activity;

public class UIExchangeCodeBehaviour : MonoBehaviour
{

    public UIButtonBehaviour ButtonExchange;
    public UIButtonBehaviour ButtonPaste;
    public UIButtonBehaviour ButtonDelete;

    public UIButtonBehaviour ButtonNum0;
    public UIButtonBehaviour ButtonNum1;
    public UIButtonBehaviour ButtonNum2;
    public UIButtonBehaviour ButtonNum3;
    public UIButtonBehaviour ButtonNum4;
    public UIButtonBehaviour ButtonNum5;
    public UIButtonBehaviour ButtonNum6;
    public UIButtonBehaviour ButtonNum7;
    public UIButtonBehaviour ButtonNum8;
    public UIButtonBehaviour ButtonNum9;

    //public Text CodeText;

    public RectTransform GetItemPane;
    public RectTransform GetItem;

    public UIButtonBehaviour ButtonClose;
    public InputField InputF;
    void Awake()
    {
        GameController.Instance.ModelEventSystem.OnExchangeCode += OnExchangeCodeResult;
        GameController.Instance.ModelEventSystem.OnExchangeCodeFail += OnExchangeCodeFailResult;
    }

    void Start()
    {
        ButtonClose.onClick += OnCloseClick;

        ButtonExchange.onClick += OnExchangeCodeClick;
        ButtonPaste.onClick += OnPasteClick;
        ButtonDelete.onClick += OnDeleteClick;

        ButtonNum0.onClick += OnNumClick;
        ButtonNum1.onClick += OnNumClick;
        ButtonNum2.onClick += OnNumClick;
        ButtonNum3.onClick += OnNumClick;
        ButtonNum4.onClick += OnNumClick;
        ButtonNum5.onClick += OnNumClick;
        ButtonNum6.onClick += OnNumClick;
        ButtonNum7.onClick += OnNumClick;
        ButtonNum8.onClick += OnNumClick;
        ButtonNum9.onClick += OnNumClick;
        GameController.Instance.AtlasManager.AddAtlas(Atlas.UIComponent);
    }

    void OnExchangeCodeFailResult(short status)
    {
        if (status == 1001)
        {
            FlyText(Utils.GetText("兌換碼已經兌換"));
        }
        else if (status == 1002)
        {
            FlyText(Utils.GetText("兌換碼不存在"));
        }
    }

    int FlyTextCount = 0;

    void FlyText(string str)
    {
        Action<object> PopUpItmeComplete = delegate(object item)
        {
            GameObject.Destroy((item as RectTransform).gameObject);
            FlyTextCount--;
            if (FlyTextCount == 0)
            {
                GetItemPane.gameObject.SetActive(false);
            }
        };
        if (!GetItemPane.gameObject.activeInHierarchy)
        {
            GetItemPane.gameObject.SetActive(true);
        }
        RectTransform tritem = GameObject.Instantiate(GetItem) as RectTransform;
        tritem.gameObject.SetActive(true);
        tritem.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        tritem.Find("Text").GetComponent<Text>().text = str;
        FlyTextCount++;
        tritem.SetParent(GetItem.parent);
        tritem.localScale = new Vector3(1, 1, 1);
        tritem.localPosition = GetItem.localPosition;
        LeanTween.moveLocal(tritem.gameObject, tritem.gameObject.transform.localPosition + new Vector3(0, 100), 1f)
                    .setOnComplete(PopUpItmeComplete)
                    .setOnCompleteParam(tritem);
    }

    void OnExchangeCodeResult(ExchangeCodeResponse res)
    {
        ValueResultListResponse  valueResult = res.valueResultListResponse;

        List<Goods> datas = new List<Goods>();
        object[] dataNum = new object[valueResult.list.Count];

        valueResult.list.Sort(GameController.Instance.PlayerCtr.ValueResultSort);

        for (int i = 0; i < valueResult.list.Count; i++)
        {
            ValueResult vr = valueResult.list[i];
            if (vr.changeType == 1 && vr.valuesType != (int)RewardType.Unit)
            {
                Goods data = new Goods();
                dataNum[i] = vr.changeValue.ToString();

                //switch (vr.valuesType)
                //{
                //    case (int)PlayerValueType.Key:
                //        texName = "icon8";
                //        //itemName = "剪刀";
                //        break;
                //    case (int)PlayerValueType.UpgradeA:
                //        texName = "UpgradeA_b";
                //        //itemName = "橘子";
                //        break;
                //    case (int)PlayerValueType.UpgradeB:
                //        texName = "UpgradeB_b";
                //        //itemName = "桃子";
                //        break;
                //    case (int)PlayerValueType.Gem:
                //        texName = "Icon2";
                //        //itemName = "鑽石";
                //        break;
                //    case (int)PlayerValueType.EnergyMax:
                //        texName = "EnergyCache";
                //        //itemName = "體力上限";
                //        break;
                //    case (int)PlayerValueType.Energy:
                //        texName = "Icon1";
                //        //itemName = "體力";
                //        break;
                //}
                //data.GoodsSprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIComponent, texName);
                data.GoodsSprite = GameController.Instance.AtlasManager.GetSpriteInUIComponent((RewardType)vr.valuesType);
                data.Num = vr.changeValue.ToString();
                data.GoodsSpriteSize = new Vector2(120f,120f);

                datas.Add(data);

            }
        }

        int ID = res.configId;
        Dictionary<int,ExchangeCodeConfig> ExchangeConfigs = GameController.Instance.Model.ExchangeCodeConfigs;

        ExchangeCodeConfig config;
        if (ExchangeConfigs.ContainsKey(ID))
        {
            config = ExchangeConfigs[ID];

            if (config != null)
            {
                string title =Utils.GetTextByID(config.TitleID);
                string info = string.Format(Utils.GetTextByID(config.InfoID),dataNum);

                UICommonDialogBehaviour uiCommon = GameController.Instance.Popup.Open(PopupID.UICommonDialog, null, true, true).GetComponent<UICommonDialogBehaviour>();
                uiCommon.SetInfo(title, info, datas);
            }
        }


        
    }


    void OnNumClick(UIButtonBehaviour go)
    {
        string[] str = go.gameObject.name.Split('_');
        InputF.text += str[str.Length - 1];
    }

    void OnCloseClick(UIButtonBehaviour go)
    {
        GameController.Instance.Popup.Close(PopupID.UIExchangeCode, false);
        GameController.Instance.Popup.Open(PopupID.UISetting,null,true,true);
    }
    void OnPasteClick(UIButtonBehaviour go)
    {
        TextEditor te = new TextEditor();
        string strGet;
        te.OnFocus();
        te.Paste();
        strGet = te.content.text;
        if (strGet == string.Empty)
        {
            return;
        }
        InputF.text += strGet;
    }
    void OnDeleteClick(UIButtonBehaviour go)
    {
        if (InputF.text.Length > 0)
        {
            InputF.text = new string(InputF.text.ToCharArray(0, InputF.text.Length - 1));
        }
       
    }

    void OnExchangeCodeClick(UIButtonBehaviour go)
    {

//#if UNITY_EDITOR
//        ExchangeCodeResponse res = new ExchangeCodeResponse();
//        res.configId = 1;
//        res.valueResultListResponse = new ValueResultListResponse();
//        res.valueResultListResponse.list = new List<ValueResult>();
//        foreach (PlayerValueType type in Enum.GetValues(typeof(PlayerValueType)))
//        {
//            ValueResult vr = new ValueResult();
//            vr.valuesType = (int)type;
//            vr.changeValue = 100 * (int)type;
//            vr.changeType = 1;
//            res.valueResultListResponse.list.Add(vr);
//        }

//        OnExchangeCodeResult(res);
//#else
       GameController.Instance.Client.ExchangeCode(InputF.text);
//#endif
    }

    void OnDestroy()
    {
        GameController.Instance.ModelEventSystem.OnExchangeCode -= OnExchangeCodeResult;
        GameController.Instance.ModelEventSystem.OnExchangeCodeFail -= OnExchangeCodeFailResult;
        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIExchangeCode);
    }

}
