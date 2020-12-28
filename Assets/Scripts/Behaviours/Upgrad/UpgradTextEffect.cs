using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UpgradTextEffect : MonoBehaviour {
    const string TEXT_KEY = "Prefabs/Effects/EffectUpgradeText";
 
    public void Play(string msg)
    {
        RectTransform textT = GameController.Instance.PoolManager.PrefabSpawn(TEXT_KEY) as RectTransform;
        Text text = textT.GetComponentInChildren<Text>();

        if (text == null)
        {
            //隱藏特效測試的時候 可能沒有這個
            return;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
        text.text = msg;
        textT.SetParent(transform);
        textT.anchoredPosition3D = Vector3.zero;
        textT.localScale = new Vector3(.3f,.3f,.3f);

        LTDescr ltDescr = LeanTween.value(textT.gameObject, textT.anchoredPosition3D.y, textT.anchoredPosition3D.y + 50f, .8f);

        //ltDescr = ltDescr.setEase(LeanTweenType.easeInCubic);
 
        ltDescr = ltDescr.setOnUpdate(delegate(float valF)
        {
            textT.anchoredPosition = new Vector2(textT.anchoredPosition.x, valF);
        });

        ltDescr = LeanTween.value(textT.gameObject, textT.localScale, new Vector3(1f,1f,1f), .7f);

        ltDescr.setEase(LeanTweenType.easeOutQuad);

        ltDescr.setOnUpdate(delegate(Vector3 valVect) {
            textT.localScale = valVect;
        });

        ltDescr = ltDescr.setOnComplete(delegate()
        {
            ltDescr = LeanTween.value(textT.gameObject, textT.anchoredPosition3D.y, textT.anchoredPosition3D.y + 120f, 1f);

            ltDescr = ltDescr.setOnUpdate(delegate(float valF)
            {
                textT.anchoredPosition = new Vector2(textT.anchoredPosition.x, valF);
            });

            ltDescr = LeanTween.value(textT.gameObject, textT.localScale, new Vector3(.3f, .3f, .3f), 1f);

            ltDescr.setOnUpdate(delegate(Vector3 valVect)
            {
                textT.localScale = valVect;
            });

            ltDescr = LeanTween.value(textT.gameObject, text.color.a, 0f, 1f);

            ltDescr.setOnUpdate(delegate(float val)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, val);
            });

            ltDescr = ltDescr.setOnComplete(delegate()
            {
                LeanTween.cancel(textT.gameObject);
                GameController.Instance.PoolManager.Despawn(textT);
            });
        });
    }

	// Update is called once per frame
	void Update () {
	    
	}
}
