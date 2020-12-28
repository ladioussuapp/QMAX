using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class IAPKit : MonoBehaviour
{
    // [DllImport("__Internal")]
    // private static extern float TestMsg();
    // //測試信息發送

    // [DllImport("__Internal")]
    // private static extern void TestSendString(string s);
    // //測試發送字符串

    // [DllImport("__Internal")]
    // private static extern void TestGetString();
    //測試接收字符串

    [DllImport("__Internal")]
    private static extern void Init(string prefabName);
    //初始化

    [DllImport("__Internal")]
    private static extern bool CanMakePayment();
    //判斷是否可以購買

    [DllImport("__Internal")]
    private static extern void RequstProductInfo(string s);
    //獲取商品信息

    [DllImport("__Internal")]
    //購買商品
    private static extern void BuyProduct(string s);

    [DllImport("__Internal")]
    ///處理完一個訂單
    private static extern void FinishTransaction(string s);

    //參數：transactionID, appUsername, receipt
    public event Action<string, string, string> OnPaymentComplete;

    //參數：transactionID, appUsername
    public event Action<string, string> OnPaymentFail;

    //參數：transactionID, appUsername
    public event Action<string, string> OnPaymentRestore;

    ///productInfo list
    ///Invalid product id
    //OnProductsReqResponse: 
    /// 實例
    /// [{"productIdentifier":"diamond330","localizedTitle":"一包钻石","localizedDescription":"获得330个钻石，用于购买道具、金币、增加体力值或升级宠物。","price":"30"},{"productIdentifier":"diamond60","localizedTitle":"一捧钻石","localizedDescription":"获得60个钻石，用于购买道具、金币、增加体力值或升级宠物。","price":"6"}]
    /// pretty print
    /*
    [
      {
        "productIdentifier" : "diamond330",
        "localizedTitle" : "一包鑽石",
        "localizedDescription" : "獲得330個鑽石，用於購買道具、金幣、增加體力值或升級寵物。",
        "price" : "30"
      },
      {
        "productIdentifier" : "diamond60",
        "localizedTitle" : "一捧鑽石",
        "localizedDescription" : "獲得60個鑽石，用於購買道具、金幣、增加體力值或升級寵物。",
        "price" : "6"
      }
    ]
    */
    public event Action<string> OnProductsReqResponse;

    public static bool isAvailable = Application.isMobilePlatform && Application.platform == RuntimePlatform.IPhonePlayer;

    ///初始化
    ///註冊回調地址
    public void InitKit(string prefabName)
    {
        Debug.Log("IAPKit:Init");
        if (isAvailable)
        {
            IAPKit.Init(prefabName);
        }
    }

    ///是否可以有購買行為
    public bool IsProductsAvailable()
    {
        bool ret = false;
        if (isAvailable)
        {
            ret = IAPKit.CanMakePayment();
        }

        Debug.LogFormat("IAPKit:IsProductsAvailable {0}", ret);
        return ret;
    }

    ///請求商品信息
    public void StartProductsRequest(List<string> productIDs)
    {
        if (isAvailable)
        {
            string str = "";
            for (int i = 0, n = productIDs.Count; i < n; i++)
            {
                str += productIDs[i] + "\t";
            }
            Debug.Log("IAPKit:StartProductsRequest " + str);
            IAPKit.RequstProductInfo(str);
        }
    }

    ///開始一個購買行為
    public void StartPayment(string productId, int num, string username)
    {
        if (isAvailable)
        {
            String str = String.Format("{0}\t{1}\t{2}", productId, num, username);
            Debug.Log("IAPKit:StartPayment " + str);
            IAPKit.BuyProduct(str);
        }
    }


    public void FinishTransactionByID(string transactionID)
    {
        Debug.Log("IAPKit:FinishTransactionByID " + transactionID);
        if (isAvailable)
        {
            IAPKit.FinishTransaction(transactionID);
        }
    }


    private void onProductsInfoResponse(string infoJson)
    {
        Debug.LogFormat("IAPKit:onProductsInfoResponse, {0}", infoJson);
        OnProductsReqResponse(infoJson);
    }


    /// <summary>
    /// iOS調用
    /// </summary>
    /// <param name="paramList"></param>
    private void onPaymentTransactionComplete(string paramList)
    {
        Debug.LogFormat("IAPKit:onPaymentTransactionComplete, {0}", paramList);
        string[] arr = paramList.Split('\t');
        string transactionID = arr[0];
        string appUsername = arr[1];
        string receipt = arr[2];
        //VerifyReceipt(receipt);
        OnPaymentComplete(transactionID, appUsername, receipt);
    }

    /// <summary>
    /// iOS調用
    /// </summary>
    /// <param name="paramList"></param>
    private void onPaymentTransactionFail(string paramList)
    {
        Debug.LogFormat("IAPKit:onPaymentTransactionFail, {0}", paramList);
        string[] arr = paramList.Split('\t');
        string transactionID = arr[0];
        string appUsername = arr[1];
        OnPaymentFail(transactionID, appUsername);
    }

    /// <summary>
    /// iOS調用
    /// </summary>
    /// <param name="paramList"></param>
    private void onPaymentTransactionRestore(string paramList)
    {
        Debug.LogFormat("IAPKit:onPaymentTransactionRestore, {0}", paramList);
        string[] arr = paramList.Split('\t');
        string transactionID = arr[0];
        string appUsername = arr[1];
        OnPaymentRestore(transactionID, appUsername);
    }


    ///驗證Rectipt
    // IEnumerator VerifyReceipt(string receipt)
    // {
    //     Debug.Log("VerifyReceipt " + ServerValidateUrl);
    //     string json = string.Format("{{\"receipt-data\":\"{0}\"}}", receipt);
    //     // string json = "{\"receipt-data\":\"+YourReceiptCodeBase64+\"}";
    //     //json = string.Format("{{\"receipt-data\":\"{0}\"}}", "abc");
    //     Debug.Log(json);

    //     WWWForm wwwForm = new WWWForm();
    //     wwwForm.AddField("receipt", json);
    //     WWW www = new WWW(ServerValidateUrl, wwwForm);
    //     string[] arr = { "123" };
    //     www.InitWWW(ServerValidateUrl, Encoding.UTF8.GetBytes(json), arr);
    //     while (!www.isDone)
    //     {
    //         yield return null;
    //     }

    //     if (!string.IsNullOrEmpty(www.error))
    //     {
    //         //TODO
    //         Debug.LogFormat("VerifyReceipt error={0}", www.error);
    //     }
    //     else
    //     {
    //         Debug.LogFormat("VerifyReceipt data={0}", www.text);
    //         if (OnPaymentComplete != null)
    //             OnPaymentComplete(receipt);
    //     }
    // }




    ///验证Rectipt
    // void VerifyReceipt(string receipt)
    // {
    //     Thread thread = new Thread(delegate ()
    //         {
    //             try
    //             {
    //                 Debug.Log("VerifyReceipt " + ServerValidateUrl);
    //                 string json = string.Format("{{\"receipt-data\":\"{0}\"}}", receipt);
    //                 //json = string.Format("{{\"receipt-data\":\"{0}\"}}", "abc");
    //                 Debug.Log(json);

    //                 // ASCIIEncoding ascii = new ASCIIEncoding();
    //                 byte[] postBytes = Encoding.UTF8.GetBytes(json);

    //                 var request = HttpWebRequest.Create(ServerValidateUrl);
    //                 request.Method = "POST";
    //                 request.ContentType = "application/json";
    //                 request.ContentLength = postBytes.Length;

    //                 using (var stream = request.GetRequestStream())
    //                 {
    //                     stream.Write(postBytes, 0, postBytes.Length);
    //                     stream.Flush();
    //                 }

    //                 var response = request.GetResponse();
    //                 string sendresponsetext = "";
    //                 using (var streamReader = new StreamReader(response.GetResponseStream()))
    //                 {
    //                     sendresponsetext = streamReader.ReadToEnd().Trim();
    //                 }
    //                 Debug.LogFormat("VerifyReceipt3 data={0}", sendresponsetext);
    //             }
    //             catch (Exception)
    //             {

    //             }
    //         });
    //     thread.Start();
    // }

}
