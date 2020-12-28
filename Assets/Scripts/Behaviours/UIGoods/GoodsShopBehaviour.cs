using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using Com4Love.Qmax;

public class GoodsShopBehaviour : MonoBehaviour {


    public Button Button_Buy;
    public Button Button_Cancel;
    public InputField InputField_ID;

	// Use this for initialization
	void Start () {

        Button_Buy.onClick.AddListener(this.OnBuyGoods);
        Button_Cancel.onClick.AddListener(this.OnCancel);
    }

    private void OnBuyGoods()
    {
        int id = Convert.ToInt32(InputField_ID.text);
        Q.Assert(GameController.Instance.Model.ShopConfigs.ContainsKey(id),"Can't find shop config id!!");
        GameController.Instance.Client.BuyGoods(id);
        this.Close();
    }

    private void OnCancel()
    {
        this.Close();
    }

    private void Close()
    {
        if(GameController.Instance.Popup.IsPopup(PopupID.UILeanShop))
        {
            GameController.Instance.Popup.Close(PopupID.UILeanShop, true);
        }

    }
	
	// Update is called once per frame
	void Update () {

    }
}
