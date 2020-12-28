using UnityEngine;
using System.Collections;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Stage;

public class MapStarInfoBoard : MapThing
{
    RectTransform boardT;
    MapLvlButton lvlButton;
    Vector2 screenPos;
    Vector2 uiPos;
    StageConfig stageConfig;
    Stage stage;
    CanvasGroup group;
    Camera uiCamera;
    bool playEffect;


    public void Awake()
    {
        lvlButton = GetComponent<MapLvlButton>();
    }
    // Use this for initialization
    void Start()
    {

    }

    public void SetData(StageConfig stage, bool effect = false)
    {
        this.stageConfig = stage;
        playEffect = effect;
    }

    void Create()
    {
        uiCamera = MapThingManager.Instance.InfoBoardContainer.GetComponent<Canvas>().worldCamera;
        boardT = MapThingManager.Instance.CreateStarInfoBoard(GetAnchoredPosition(), lvlButton.Stage.ID);
        boardT.name = "star" + stageConfig.ID;
        //boardT.anchoredPosition = GetAnchoredPosition();

        stage = GameController.Instance.StageCtr.GetStageData(lvlButton.Stage.ID);
        group = boardT.GetComponentInChildren<CanvasGroup>();

        if (playEffect)
        {          
            GameController.Instance.ModelEventSystem.OnCloudeShow += OnCloudEffectComplete;
        }
        else
        {
            StartCoroutine(this.SetStar(stage.star, false));
        }
    }

    private void OnCloudEffectComplete()
    {
        StartCoroutine(this.SetStar(stage.star, true));
        GameController.Instance.ModelEventSystem.OnCloudeShow -= OnCloudEffectComplete;
    }

    private Vector2 GetAnchoredPosition()
    {
        screenPos = MapView.Instance.mapCamera.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(MapThingManager.Instance.InfoBoardContainer, screenPos, uiCamera, out uiPos);

        return uiPos;
    }

    void LateUpdate()
    {
        if (CheckVisible())
        {
            if (boardT == null)
            {
                Create();
            }
            else
            {
                boardT.gameObject.SetActive(true);
                boardT.anchoredPosition = GetAnchoredPosition();

                group.alpha = 1.0f - boardT.anchoredPosition.y / 200;
                Transform stars = boardT.GetChild(0);
                stars.transform.localScale = new Vector3(group.alpha, group.alpha, group.alpha);
                stars.transform.localPosition = new Vector3(0, -68f+(1- group.alpha)*60, -2.5f);
            }
        }
        else
        {
            HideBoard();
        }

    }

    void HideBoard()
    {
        if (boardT != null)
        {
            boardT.gameObject.SetActive(false);
            //扔回池裡
            //MapThingManager.Instance.RemoveStarInfoBoard(boardT);
        }
    }

    protected IEnumerator SetStar(int star, bool effect)
    {
        Transform stars = boardT.GetChild(0);
        for (int i = 0; i < star; i++)
        {
            Transform parent = stars.GetChild(i);
            Transform rT = null;
            if (parent.childCount <= 0)
            {
                GameObject startIns = Resources.Load<GameObject>("Prefabs/Map/Star");
                rT = Instantiate<GameObject>(startIns).transform;
                rT.SetParent(parent);
                rT.localPosition = new Vector3();
                rT.localScale = new Vector3(1, 1, 1);
                rT.localRotation = Quaternion.identity;
            }
            else
            {
                rT = parent.GetChild(i);
            }
            if (effect)
            {
                rT.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.25f);
                rT.gameObject.SetActive(true);

                if (rT.GetComponentInChildren<Animator>() != null)
                {
                    rT.GetComponentInChildren<Animator>().SetTrigger("Show");
                }
            }
        }

        if (GameController.Instance.ModelEventSystem.OnStarEffectOver != null)
        {
            GameController.Instance.ModelEventSystem.OnStarEffectOver();
        }

        yield return 1;
    }

    public override bool CheckVisible()
    {
        if (lvlButton != null)
        {
            return lvlButton.CheckVisible();
        }
        else
        {
            return false;
        }
    }
}
