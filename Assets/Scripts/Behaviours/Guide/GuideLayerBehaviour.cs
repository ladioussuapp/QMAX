using UnityEngine;
using UnityEngine.UI;
using Com4Love.Qmax;
using System;


public class GuideLayerBehaviour : MonoBehaviour
{

    public Camera GuideCamera;
    //public RectTransform RootLeftPanel;
    //public RectTransform RootRightPanel;
    public Image DialogBg;
    public Image DialogContent;
    private Action m_callBack;
    void Start()
    {

    }

    void Awake()
    {
        GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIDialog);
    }

    public void setData(string contentUrl, string audioUrl, Action callBack)
    {
        m_callBack = callBack;
        Sprite content = GameController.Instance.AtlasManager.GetSprite(Atlas.UIDialog, contentUrl);
        DialogContent.overrideSprite = content;
        DialogContent.SetNativeSize();
        Vector2 size = DialogContent.overrideSprite.textureRect.size;
        DialogBg.rectTransform.sizeDelta = new Vector2(size.x + 80, size.y + 120);

        if (audioUrl != "")
        {
            GameController.Instance.AudioManager.PlayAudio(audioUrl);
        }
    }

    public void onClickGuide()
    {
        if (m_callBack != null)
        {
            m_callBack();
        }
        GameObject.DestroyObject(this.gameObject);
    }

    void Update()
    {

    }
}
