using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TreeBattleTipTimeEffect : MonoBehaviour {
    public Image NumImg1;
    public Image NumImg2;
    public Transform Star;
    public Transform ReadySound;
    public Transform GoSound;

    public Sprite[] NumSprite;

    private int _index = -1;

	void Start () {
        Reset();
    }

    public void Reset()
    {
        if (NumSprite != null)
        {
            //Star.gameObject.SetActive(false);
            ReadySound.gameObject.SetActive(false);
            GoSound.gameObject.SetActive(false);

            _index = NumSprite.Length - 1;
            SetVal(_index);
        }
    }

    public bool Next()
    {
        if (_index > 0)
        {
            _index--;
            SetVal(_index);
            return _index > 0;
        }
        return false;
    }

    public void SetVal(int num)
    {
        if (num > 5)
            return;

        Sprite sprite = NumSprite[num];
        NumImg1.sprite = sprite;
        NumImg2.sprite = sprite;
        NumImg1.SetNativeSize();
        NumImg2.SetNativeSize();
    }
}
