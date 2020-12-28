using Com4Love.Qmax.Data;
using System;
using UnityEngine;
using UnityEngine.UI;

//只监听和处理SDK登录的消息
public class UILoginWindowBehaviour : MonoBehaviour
{
    public event Action<string, string> OnClickLogin;
    public event Action<string, string> OnClickRegister;

    public InputField UsernameInput;
    public InputField PasswordInput;
    public Text InfoText;
    public Text TextVersion;
    public Button LoginBtn;
    public Button RegisterBtn;
    public Toggle NetToggle;

    public void SetAccount(string username, string password)
    {
        UsernameInput.text = username;
        PasswordInput.text = password;
    }


    public void SetMsg(string msg)
    {
        InfoText.text = msg;
    }

    protected void Awake()
    {
        SetMsg("");
    }

    protected void Start()
    {
        LoginBtn.onClick.AddListener(() =>
        {
            if (UsernameInput.text == "" || PasswordInput.text == "")
                return;

            SetMsg("");
            OnClickLogin(UsernameInput.text, PasswordInput.text);
        });


        RegisterBtn.onClick.AddListener(() =>
        {
            SetMsg("");
            OnClickRegister(UsernameInput.text, PasswordInput.text);
        });

        //登陆界面显示版本号
        TextVersion.text = "v" + PackageConfig.Version;
    }
}
