using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class TreeActivityAwardPanel : MonoBehaviour
{
    public Image[] icons;
    public Text[] texts;
    public int upgradeA = 0;
    public int upgradeB = 0;
    public int gem = 0;

    public void SetAward(int upgradeA_, int upgradeB_, int gem_)
    {
        upgradeA += upgradeA_;
        upgradeB += upgradeB_;
        gem += gem_;
        texts[0].text = upgradeA.ToString();
        texts[1].text = upgradeB.ToString();
        texts[2].text = gem.ToString();

    }

    public IEnumerator ScrollText()
    {
        int textCount = 3;

        Action ScrollComplete = delegate ()
        {
            textCount--;
        };

        foreach (Text item in texts)
        {
            int from = int.Parse(item.text);

            GameController.Instance.EffectProxy.ScrollText(item, from, 0, true, ScrollComplete);
        }

        while (textCount > 0)
        {
            yield return 0;
        }
    }
}
