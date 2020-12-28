using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Stage;

public class MapButtonStars : MonoBehaviour {
    MapLvlButton button;
    Stage data;
    Transform stars;
    SpriteRenderer[] rendererStars;
    SpriteRenderer[] rendererBgs;
    bool effectShow = false;

    // Use this for initialization
    void Start () {
        button = GetComponent<MapLvlButton>();
        Q.Assert(button != null, "MapButtonStars依賴MapLvlButton存在");
        rendererStars = new SpriteRenderer[3];
        rendererBgs = new SpriteRenderer[3];

        if (GameController.Instance.Model.IsStagePassedInLastFight
    && GameController.Instance.Model.PlayerData.passStageId == data.stageId)
        {
            //上一次戰鬥通過了當前關   
            effectShow = true;
        }
    }
	
    public void SetData(Stage data)
    {
        this.data = data;
    }

    void CreateStars()
    {
        if (!state.Equals("create"))
        {
            LeanTween.cancel(gameObject);
 
            if (stars == null)
            {
                stars = GameController.Instance.PoolManager.PrefabSpawn("Prefabs/Map/MapButtonStarsTest");
                stars.SetParent(button.button.transform);
                stars.localPosition = Vector3.zero;
                stars.localScale = new Vector3(1, 1, 1);
                stars.localRotation = Quaternion.identity;

                rendererStars[0] = stars.Find("Star1/Img").GetComponent<SpriteRenderer>();
                rendererStars[1] = stars.Find("Star2/Img").GetComponent<SpriteRenderer>();
                rendererStars[2] = stars.Find("Star3/Img").GetComponent<SpriteRenderer>();
                rendererBgs[0] = stars.Find("Star1/Bg").GetComponent<SpriteRenderer>();
                rendererBgs[1] = stars.Find("Star2/Bg").GetComponent<SpriteRenderer>();
                rendererBgs[2] = stars.Find("Star3/Bg").GetComponent<SpriteRenderer>();
            }
 
            if (effectShow)
            {
                EffectShow();
            }
            else
            {
                Show();
            }
        }

        state = "create";
    }

    void Show()
    {
        for (int i = 0; i < 3; i++)
        {
            bool showStar = i < data.star;

            rendererStars[i].gameObject.SetActive(showStar);
            rendererBgs[i].gameObject.SetActive(!showStar);
            rendererStars[i].color = new Color(rendererStars[i].color.r, rendererStars[i].color.g, rendererStars[i].color.b, 0);
            rendererBgs[i].color = new Color(rendererStars[i].color.r, rendererStars[i].color.g, rendererStars[i].color.b, 0);
        }

        LeanTween.value(gameObject, 0, 1, 0.3f).setOnUpdate(delegate (float val)
        {
            rendererStars[0].color = new Color(rendererStars[0].color.r, rendererStars[0].color.g, rendererStars[0].color.b, val);
            rendererStars[1].color = new Color(rendererStars[1].color.r, rendererStars[1].color.g, rendererStars[1].color.b, val);
            rendererStars[2].color = new Color(rendererStars[2].color.r, rendererStars[2].color.g, rendererStars[2].color.b, val);

            rendererBgs[0].color = new Color(rendererStars[0].color.r, rendererStars[0].color.g, rendererStars[0].color.b, val);
            rendererBgs[1].color = new Color(rendererStars[1].color.r, rendererStars[1].color.g, rendererStars[1].color.b, val);
            rendererBgs[2].color = new Color(rendererStars[2].color.r, rendererStars[2].color.g, rendererStars[2].color.b, val);
        });
    }

    IEnumerator PlayEffectShow()
    {
        string url = "Prefabs/Effects/MapButtonEffectSTAR";
 
        yield return new WaitForSeconds(0.3f);
        Transform effect;

        for (int j = 0; j < data.star; j++)
        {
            effect = GameController.Instance.QMaxAssetsFactory.CreatePrefab(url);
            effect.localScale = new Vector3(1, 1, 1);
            effect.SetParent(rendererStars[j].transform);
            effect.localPosition = Vector3.zero;
            rendererStars[j].gameObject.SetActive(true);
            rendererBgs[j].gameObject.SetActive(false);

            yield return new WaitForSeconds(0.4f);
        }

        if (GameController.Instance.ModelEventSystem.OnStarEffectOver != null)
        {
            GameController.Instance.ModelEventSystem.OnStarEffectOver();
        }
    }

    void EffectShow()
    {
        Debug.Log("EffectShow");

        effectShow = false;

        for (int i = 0; i < 3; i++)
        {
            rendererStars[i].gameObject.SetActive(false);
            rendererBgs[i].gameObject.SetActive(true);
            rendererStars[i].color = new Color(rendererStars[i].color.r, rendererStars[i].color.g, rendererStars[i].color.b, 1);
            rendererBgs[i].color = new Color(rendererStars[i].color.r, rendererStars[i].color.g, rendererStars[i].color.b, 1);
        }

        GameController.Instance.ModelEventSystem.OnCloudeShow += OnCloudEffectComplete;
    }

    void OnCloudEffectComplete()
    {
        GameController.Instance.ModelEventSystem.OnCloudeShow -= OnCloudEffectComplete;
        StartCoroutine(PlayEffectShow());
    }

    void RemoveStars()
    {
        if (state.Equals("create"))
        {
            LeanTween.cancel(gameObject);
 
            LeanTween.value(gameObject, 1f ,0f , 0.3f).setOnUpdate(delegate (float val)
            {
                rendererStars[0].color = new Color(rendererStars[0].color.r, rendererStars[0].color.g, rendererStars[0].color.b, val);
                rendererStars[1].color = new Color(rendererStars[1].color.r, rendererStars[1].color.g, rendererStars[1].color.b, val);
                rendererStars[2].color = new Color(rendererStars[2].color.r, rendererStars[2].color.g, rendererStars[2].color.b, val);

                rendererBgs[0].color = new Color(rendererStars[0].color.r, rendererStars[0].color.g, rendererStars[0].color.b, val);
                rendererBgs[1].color = new Color(rendererStars[1].color.r, rendererStars[1].color.g, rendererStars[1].color.b, val);
                rendererBgs[2].color = new Color(rendererStars[2].color.r, rendererStars[2].color.g, rendererStars[2].color.b, val);

            }).setOnComplete(delegate () {
                if (stars != null)
                {
                    GameController.Instance.PoolManager.Despawn(stars);
                    stars = null;
                }
            });
        }

        state = "remove";
    }

    string state = "";
 
    // Update is called once per frame
    void LateUpdate () {
        if (button.CheckVisible() 
            && button.dirDots > 0.5f 
            && button.dirDots < 0.99f)
        {
            CreateStars();
        }
        else
        {
            RemoveStars();
        }
	}
}
