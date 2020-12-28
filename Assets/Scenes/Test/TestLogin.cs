using GameXP.Framewrok;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TestLogin : MonoBehaviour
{
    public const string LOGIN_URL = "http://user.looddy.cc:18080/platform/login.do";
    public const string REGISTER_URL = "http://user.looddy.cc:18080/platform/reg.do";
    //public const string LOGIN_URL = "http://192.168.103.98:8080/platform/login.do";
    //public const string REGISTER_URL = "http://192.168.103.98:8080/platform/reg.do";

    public InputField UsernameInput;
    public InputField PswInput;

    public Button BtnLogin;
    public Button BtnRegister;


    public void Start()
    {
        BtnLogin.onClick.AddListener(() =>
        {
            Login(UsernameInput.text, PswInput.text);
        });
        BtnRegister.onClick.AddListener(() =>
        {
            Register(UsernameInput.text, PswInput.text);
        });

        string s = "[{\"productIdentifier\":\"diamond330\",\"localizedTitle\":\"一包钻石\",\"localizedDescription\":\"获得330个钻石，用于购买道具、金币、增加体力值或升级宠物。\",\"price\":\"30\"},{\"productIdentifier\":\"diamond60\",\"localizedTitle\":\"一捧钻石\",\"localizedDescription\":\"获得60个钻石，用于购买道具、金币、增加体力值或升级宠物。\",\"price\":\"6\"}]\"";
        //string s = "[{\"productIdentifier\": \"diamond330\"}, {\"productIdentifier\":\"diamond1\"}]";
        //string s = "[]";
        JSONInStream json = new JSONInStream(s);
        for (int i = 0, n = json.Count; i < n; i++)
        {
            string pID = null;
            json.Start(i)
                    .Content("productIdentifier", out pID)
                .End();
            Debug.Log(pID);
        }
    }

    public void Login(string username, string psw)
    {
        //StartCoroutine(EnumLogin(username, psw));

        var data = new { username = username, password = psw };
        HttpUtils.Post(LOGIN_URL, data,
            (HttpWebResponse resp) =>
            {
                //if(resp.StatusCode == HttpStatusCode.OK)
                //{
                var stream = resp.GetResponseStream();
                var streamReader = new StreamReader(stream);
                var str = streamReader.ReadToEnd();
                Debug.Log(str);
                //}
                //else
                //{

                //}
            },
            (Exception e) =>
            {
                Debug.Log("登录请求错误：" + e.Message);
            });
    }


    public void Register(string username, string psw)
    {
        StartCoroutine(EnumRegister(username, psw));
    }



    private IEnumerator EnumLogin(string username, string psw)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", psw);

        WWW www = new WWW(LOGIN_URL, form);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            //{"statusCode":1002,"token":"","uid":0}
            Debug.Log(www.text);
        }
        else
        {
            Debug.LogFormat("登录失败：{0}", www.error);
        }
    }

    private IEnumerator EnumRegister(string username, string psw)
    {

        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("password", psw);

        WWW www = new WWW(REGISTER_URL, form);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            //{"statusCode":1002,"token":"","uid":0}
            Debug.Log(www.text);
        }
        else
        {
            Debug.LogFormat("注册失败：{0}", www.error);
        }
    }
}
