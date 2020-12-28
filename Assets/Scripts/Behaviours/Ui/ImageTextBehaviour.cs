using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Com4Love.Qmax;

public class ImageTextBehaviour : MonoBehaviour {

    private string[] Sprites1;
    private string[] Sprites2;
    private string[] Sprites3;
    private string[] Sprites4;
    public char[] Chars;

    private int level=1;
    public int Level
    {
        set{
            level = value;
            SetText(text);
        }
        get{
            return level;
        }
    }
    public float Alpha = 1.0f;

    private string text="";
    public string Text
    {
        set { SetText(value); }
        get { return text; }
    }

    private void SetText(string value)
    {
        
        if (value == text)
            return;

        text = value;
        GameObject ga = transform.GetChild(0).gameObject;
        char[] str = value.ToString().ToCharArray();
        GameObject targetGA = null;

        if (str.Length > transform.childCount)
        {
            //補全需要的Image
            for (int i = 0, n = str.Length - transform.childCount; i < n; i++)
            {
                targetGA = Instantiate(ga);
                targetGA.transform.SetParent(transform);
                targetGA.transform.localScale = new Vector3(1, 1, 1);
                targetGA.transform.localPosition = new Vector3(0, 0, 0);
                targetGA.name = "Char" + (transform.childCount - 1);
            }
        }

        for (int i = 0, n = transform.childCount; i < n; i++)
        {
            targetGA = transform.GetChild(i).gameObject;
            targetGA.SetActive(i < str.Length);
            if (i >= str.Length)
                continue;
            string[] sprites = null;
            switch(level){
                case 1:
                    sprites = Sprites1;
                    break;
                case 2:
                    sprites = Sprites2;
                    break;
                case 3:
                    sprites = Sprites3;
                    break;
                case 4:
                    sprites = Sprites4;
                    break;
            }
            int textureIndex = GetTextureIndex(str[i]);
            if (textureIndex>=0){
                targetGA.GetComponent<Image>().sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIBattle,sprites[textureIndex]);
            }
        }
    }

    private int GetTextureIndex(char str)
    {
        for (int i = 0; i < Chars.Length;i++ )
        {
            if(Chars[i] == str){
                return i;
            }
        }
        return -1;
    }

	// Use this for initialization
    void Start()
    {
        GameController.Instance.AtlasManager.LoadAtlas(Atlas.UIBattle);
        Sprites1 = new string[11] { "Combat_Combo_Whitenum0", "Combat_Combo_Whitenum1", "Combat_Combo_Whitenum2", "Combat_Combo_Whitenum3", "Combat_Combo_Whitenum4", "Combat_Combo_Whitenum5",
                                    "Combat_Combo_Whitenum6", "Combat_Combo_Whitenum7", "Combat_Combo_Whitenum8", "Combat_Combo_Whitenum9", "Combat_Combo_Whitelash" };
        Sprites2 = new string[11] { "Combat_Combo_Bluenum0", "Combat_Combo_Bluenum1", "Combat_Combo_Bluenum2", "Combat_Combo_Bluenum3", "Combat_Combo_Bluenum4", "Combat_Combo_Bluenum5",
                                    "Combat_Combo_Bluenum6", "Combat_Combo_Bluenum7", "Combat_Combo_Bluenum8", "Combat_Combo_Bluenum9", "Combat_Combo_Blueslash" };
        Sprites3 = new string[11] { "Combat_Combo_Greennum0", "Combat_Combo_Greennum1", "Combat_Combo_Greennum2", "Combat_Combo_Greennum3", "Combat_Combo_Greennum4", "Combat_Combo_Greennum5",
                                    "Combat_Combo_Greennum6", "Combat_Combo_Greennum7", "Combat_Combo_Greennum8", "Combat_Combo_Greennum9", "Combat_Combo_Greenslash" };
        Sprites4 = new string[11] { "Combat_Combo_Orangenum0", "Combat_Combo_Orangenum1", "Combat_Combo_Orangenum2", "Combat_Combo_Orangenum3", "Combat_Combo_Orangenum4", "Combat_Combo_Orangenum5",
                                    "Combat_Combo_Orangenum6", "Combat_Combo_Orangenum7", "Combat_Combo_Orangenum8", "Combat_Combo_Orangenum9", "Combat_Combo_Orangeslash" };
	}

    void OnDestroy()
    {
        GameController.Instance.AtlasManager.UnloadAtlas(Atlas.UIBattle);
    }
}
