using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UiUpgradSelectEffect : MonoBehaviour {
    public Image BgImg;

    public void ChangeBgColor(Color color)
    {
        BgImg.color = color;
    }
}
