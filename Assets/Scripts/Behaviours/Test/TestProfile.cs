using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using UnityEngine.UI;
using Com4Love.Qmax.Tools;

public class TestProfile : MonoBehaviour
{
    public Button AddButton;
    public Button ReleaseButton;
    public Image TestImage;


    // Use this for initialization
    void Start()
    {
        AtlasManager resMgr = new AtlasManager("Textures/SpriteSheets");
        AddButton.onClick.AddListener(delegate()
        {
            GameObject ga2 = Instantiate(TestImage.gameObject);
            Sprite t2d = resMgr.GetSprite(Atlas.Tile, "AreaBombBlue1HL");
            ga2.transform.SetParent(TestImage.transform.parent);
            ga2.transform.localPosition = TestImage.transform.localPosition + new Vector3(Random.Range(-250, 250), Random.Range(-250, 250));
            ga2.GetComponent<Image>().sprite = t2d;
        });

        ReleaseButton.onClick.AddListener(delegate()
        {
            resMgr.UnloadAtlas(Atlas.Tile);
        });

        Sprite t2d1 = resMgr.GetSprite(Atlas.Tile, "AreaBombBlue1");
        //Sprite t2d1 = Resources.Load<Sprite>("Textures/Tile_TD/AreaBombBlue1");
        TestImage.sprite = t2d1;
        TestImage.SetNativeSize();
    }

    void OnDestroy()
    {

    }

}
