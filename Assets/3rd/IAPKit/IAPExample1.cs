using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class IAPExample1 : MonoBehaviour
{

    private IAPKit iapKit;

    void Start()
    {

        iapKit = GetComponent<IAPKit>();
        iapKit.OnPaymentComplete += (string transactionID, string username, string receipt) =>
        {
            Debug.Log("OnPaymentComplete: " + transactionID);
            iapKit.FinishTransactionByID(transactionID);
        };
        iapKit.OnPaymentFail += (string transactionID, string username) =>
        {
            Debug.Log("OnPaymentFail: " + transactionID);
            iapKit.FinishTransactionByID(transactionID);
        };
        iapKit.OnPaymentRestore += (string transactionID, string username) =>
        {
            iapKit.FinishTransactionByID(transactionID);
        };
        iapKit.OnProductsReqResponse += (string json)=>
        {
            Debug.Log("OnProductsReqResponse: " + json);
        };
        iapKit.InitKit("Main");
    }
    
    void OnGUI()
    {
        /*if(GUILayout.Button("Test 1",GUILayout.Width(200), GUILayout.Height(100)))
            TestMsg();
        
        GUILayout.Space (200);
        if(GUILayout.Button("Test 1",GUILayout.Width(200), GUILayout.Height(100)))
            TestSendString("This is a msg form unity3d\tt1\tt2\tt3\tt4");
        
        GUILayout.Space (200);
        if(GUILayout.Button("Test 1",GUILayout.Width(200), GUILayout.Height(100)))
            TestGetString();
        /********通信测试***********/
        
        if (Btn("GetProducts"))
        {
            if (!iapKit.IsProductsAvailable())
                throw new System.Exception("IAP not enabled");
            List<string> productIDs = new List<string>();
            productIDs.Add("diamond60");
            productIDs.Add("diamond330");
            productIDs.Add("diamand3");
            iapKit.StartProductsRequest(productIDs);
        }

        if (Btn("diamond60"))
        {
            iapKit.StartPayment("diamond60", 1, "mobi101");
        }


        if (Btn("GameCenter"))
        {
            Debug.LogFormat("SystemInfo.deviceUniqueIdentifier={0}", SystemInfo.deviceUniqueIdentifier);
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Social.localUser.Authenticate((bool ret) =>
                    {
                        if (ret)
                        {
                            Debug.LogFormat("GameCenter 登录成功 id={0}, username={1}, state={2}", 
                                Social.localUser.id,
                                Social.localUser.userName,
                                Social.localUser.state);
                        }
                        else
                        {
                            Debug.Log("GameCenter 登录失败");
                        }
                    });
            }
        }


        if (Btn("ShowLeaderboardUI"))
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Social.ShowLeaderboardUI();
            }
        }
        
        GUILayout.Space(40);
        
        // for (int i = 0; i < productInfo.Count; i++)
        // {
        //     if (GUILayout.Button(productInfo[i], GUILayout.Height(100), GUILayout.MinWidth(200)))
        //     {
        //         string[] cell = productInfo[i].Split('\t');
        //         Debug.Log("[Buy]" + cell[cell.Length - 1]);
        //         BuyProduct(cell[cell.Length - 1]);
        //     }
        // }
    }
    
    bool Btn(string msg)
    {
        GUILayout.Space(100);
        return  GUILayout.Button(msg, GUILayout.Width(200), GUILayout.Height(100));
    }
}
