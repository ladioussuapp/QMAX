using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIBattleTipsDialog : MonoBehaviour {

    public Text InfoText;
    public Image ArrowsImage;
    public RectTransform BgImage;

    float DelayTime;
    float DelayTimeMax = 1;
    float DisplayTime;
    float DisplayTimeMax = 1;

    Vector3 ArrowsPos;
    void Awake()
    {
        ArrowsPos = ArrowsImage.transform.localPosition;
    }

    void Update()
    {
        if (DisplayTime < 0 || !isActiveAndEnabled)
            return;

        if (DelayTime > 0)
        {
            DelayTime -= Time.deltaTime;
            return;
        }

        DisplayTime -= Time.deltaTime;

        transform.localScale = Vector3.one * DisplayTime / DisplayTimeMax;

        if (transform.localScale.x < 0.1f)
            gameObject.SetActive(false);
    }

    public void SetData(string info,Vector3 pos,Vector3 arrowsPos = new Vector3(), Vector2 bgSize = new Vector2())
    {
        gameObject.SetActive(true);

        transform.localPosition = pos;

        if (arrowsPos.x != 0)
            ArrowsImage.transform.localPosition = ArrowsPos + arrowsPos;

        //if (bgSize.x != 0)
        //    BgImage.sizeDelta = bgSize;

        InfoText.text = info;

        DisplayTime = DisplayTimeMax;
        DelayTime = DelayTimeMax;
    }
}
