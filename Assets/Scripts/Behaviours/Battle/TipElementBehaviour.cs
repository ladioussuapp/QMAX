using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Com4Love.Qmax.TileBehaviour;
using Com4Love.Qmax.Tools;
using Com4Love.Qmax;
using System;

public class TipElementBehaviour : MonoBehaviour {

    private GameObject tipPrefab;

    private List<GameObject> showList;
    private List<GameObject> freeList;

	void Start () {
        showList = new List<GameObject>();
        freeList = new List<GameObject>();
        tipPrefab = Resources.Load<GameObject>("Prefabs/TipElement");
	}


    public void AddTipElement(GameObject element)
    {
        if (element == null || tipPrefab == null)
            return;

        BaseTileBehaviour tile = element.GetComponent<BaseTileBehaviour>();
        if (tile == null)
            return;

        // 普通元素
        //bool isNormalElement = false;

        //ElementBehaviour elemBeh = tile as ElementBehaviour;
        //if (elemBeh != null)
        //    isNormalElement = (elemBeh.Type == ElementBehaviour.ElementType.Normal);

        AtlasManager atlasMgr = GameController.Instance.AtlasManager;
        string spriteName = tile.Data.Config.ResourceIcon;

        GameObject tipObj = null;
        if (freeList.Count > 0)
        {
            tipObj = freeList[0];
            freeList.RemoveAt(0);
            showList.Add(tipObj);
        }
        else
        {
            // 創建一個新的
            tipObj = Instantiate(tipPrefab);
        }
        if (tipObj == null)
            return;

        if (!tipObj.activeSelf)
            tipObj.SetActive(true);

        Image img = tipObj.GetComponent<Image>();
        if (img.sprite == null || (img.sprite != null && img.sprite.name != spriteName))
        {
            Sprite s = atlasMgr.GetSprite(Atlas.Tile, spriteName);
            if (s != null)
            {
                img.sprite = s;
                img.SetNativeSize();
            }
        }
        Transform child = tipObj.transform.Find("Image");
        Image childImg = child.gameObject.GetComponent<Image>();
        if (childImg != null)
        {
            //string spriteName2 = isNormalElement ? spriteName + "TL" : spriteName + "HL";
            Sprite s = atlasMgr.GetSprite(Atlas.Tile, spriteName);
            if (s != null)
            {
                childImg.sprite = s;
                childImg.SetNativeSize();
            }

            Material material = Resources.Load<Material>("Materials/" + spriteName);
            childImg.material = material;
        }

        tipObj.transform.position = element.transform.position;
        tipObj.transform.SetParent(gameObject.transform);

        Action<GameObject> OnStop = null;
        OnStop = delegate(GameObject stopObject)
        {
            TipTileBehaviour tipBeh = stopObject.GetComponent<TipTileBehaviour>();
            if (tipBeh != null)
                tipBeh.StopCallback -= OnStop;

            showList.Remove(stopObject);
            freeList.Add(stopObject);
        };

        TipTileBehaviour ttBeh = tipObj.GetComponent<TipTileBehaviour>();
        ttBeh.StopCallback += OnStop;
        ttBeh.element = element;
        ttBeh.Play();
    }

    public void StopAllTipElement()
    {
        foreach (GameObject go in showList)
        {
            TipTileBehaviour tipBeh = go.GetComponent<TipTileBehaviour>();
            if (tipBeh != null)
                tipBeh.Stop();
        }
    }

}
