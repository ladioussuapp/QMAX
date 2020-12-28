using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SheetData
{

}

[System.Serializable]
public class UISheetBehaviour : MonoBehaviour {

    public Sprite select;
    public Sprite unSelect;

    public SheetData Data;

    private bool _isSelect;
    private bool _isInit;

    public bool IsSelect
    {
        get { return _isSelect; }
        set
        {
            if (_isInit && _isSelect == value)
                return;

            Transform tf = transform.Find("ButtonOK");
            if (tf == null)
                return;

            _isSelect = value;
            Image img = tf.gameObject.GetComponent<Image>();
            if (img == null)
                return;

            if (_isSelect)
                img.sprite = select;
            else
                img.sprite = unSelect;
            img.SetNativeSize();

            if (_isSelect)
            {
                if (!_isInit)
                    transform.localScale = new Vector3(1, 1, 1);
                else
                    LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.2f).setEase(LeanTweenType.easeInOutBack);

                //可以改为  UISheetGroupBehaviour 监听此处的事件然后设置选中项
                UISheetGroupBehaviour group = transform.parent.GetComponent<UISheetGroupBehaviour>();
                if (group != null)
                    group.SetSelected(this);
            }
            else
            {
                if (!_isInit)
                    transform.localScale = new Vector3(0.9f, 0.9f, 1);
                else
                    LeanTween.scale(gameObject, new Vector3(0.9f, 0.9f, 1), 0.2f);
            }
            _isInit = true;
        }
    }

    void Awake()
    {
        UIButtonBehaviour ubb = GetComponent<UIButtonBehaviour>();
        if (ubb != null)
            ubb.onClick += click;
    }

    void Destroy()
    {
        UIButtonBehaviour ubb = GetComponent<UIButtonBehaviour>();
        if (ubb != null)
            ubb.onClick -= click;
    }

    private void click(UIButtonBehaviour button)
    {
        if (!_isSelect)
            IsSelect = true;
    }

}
