using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using Com4Love.Qmax.Data;
using Com4Love.Qmax;

public class UIReconnectBehaviour : MonoBehaviour
{

    //public Image TitleImage;
    public Text ShowTip;
    public Button ButtonOK;
    public Button ButtonRetry;
    public RectTransform PrefabPanel;
    public Text TitleText;

    void Start()
    {

    }

    void Awake()
    {
        //GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIWifi);
    }

    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// 设置连接弹窗里的内容
    /// </summary>
    /// <param name="content">显示的文本内容</param>
    /// <param name="title">标题（图片文字）</param>
    /// <param name="content">显示的内容，可能是动画</param>
    public void setWindowData(string content, int titleUrl, string contentUrl)
    {
        ShowTip.text = content;
        string title = Utils.GetTextByID(titleUrl);
        if (title != "")
        {
            TitleText.gameObject.SetActive(true);
            TitleText.text = title;
        }
        else
        {

            TitleText.gameObject.SetActive(false);
        }

        if (contentUrl != "")
        {
            Transform target = GameController.Instance.QMaxAssetsFactory.CreateNetTipPrefab(contentUrl);

            if (target != null)
            {
                target.SetParent(PrefabPanel);
                target.transform.localPosition = new Vector3(0, 0, 0);
                target.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public void HideRetryButtom()
    {
        Transform parent = ButtonRetry.gameObject.transform.parent;
        parent.gameObject.SetActive(false);
    }

    public void OnReconnectClick()
    {
        //Debug.Log("OnReconnectClick");

    }
}
