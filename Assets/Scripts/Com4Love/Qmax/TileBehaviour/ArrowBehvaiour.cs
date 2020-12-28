using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax;
using UnityEngine.UI;

public class ArrowBehvaiour : MonoBehaviour {

    public SpriteRenderer arrowImg1;
    public SpriteRenderer arrowImg2;
    public SpriteRenderer arrowImg3;
    public SpriteRenderer arrowImg4;
    public SpriteRenderer arrowImg5;
    public SpriteRenderer arrowImg6;
    public SpriteRenderer arrowImg7;
    public SpriteRenderer arrowImg8;

    public void SetArrowImage(string img)
    {
        AtlasManager atlasMgr = GameController.Instance.AtlasManager;
        Sprite s = atlasMgr.GetSprite(Atlas.Tile, img);
        if (s == null)
            return;

        if (arrowImg1 != null)
        {
            arrowImg1.sprite = s;
        }
        if (arrowImg2 != null)
        {
            arrowImg2.sprite = s;
        }
        if (arrowImg3 != null)
        {
            arrowImg3.sprite = s;
        }
        if (arrowImg4 != null)
        {
            arrowImg4.sprite = s;
        }
        if (arrowImg5 != null)
        {
            arrowImg5.sprite = s;
        }
        if (arrowImg6 != null)
        {
            arrowImg6.sprite = s;
        }
        if (arrowImg7 != null)
        {
            arrowImg7.sprite = s;
        }
        if (arrowImg8 != null)
        {
            arrowImg8.sprite = s;
        }
    }

}
