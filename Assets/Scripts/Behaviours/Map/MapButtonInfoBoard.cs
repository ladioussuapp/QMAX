using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MapButtonInfoBoard : MonoBehaviour
{
    RectTransform boardT;
    MapLvlButton lvlButton;
    Vector2 screenPos;
    Vector2 uiPos;
    Image headImg;
    Image TipImg;
    Text TipMsgText;
    Button headButton;
    StageConfig stage;
    Camera uiCamera;
    Animator animator;
    CanvasGroup group;
    bool ActiveTrigger;

    Button coverButton;
    public static MapButtonInfoBoard TipsInstance;

    private float yPosFlag; // 用於標記在屏幕中線以上(>0) 或 以下(<0)

    public void Awake()
    {
        lvlButton = GetComponent<MapLvlButton>();
    }

    public void OnEnable()
    {
 
        if (GameController.Instance.UnitCtr.HasTypeUnit(lvlButton.Stage.UnitHeadId))
        {
            //擁有了此寵物， 將腳本直接銷毀
            Destroy(this);
            return;
        }

        if (TipImg)
        {
            TipImg.gameObject.SetActive(false);
        }
    }

    public void OnDisable()
    {
        if (headButton != null)
        {
            headButton.onClick.RemoveAllListeners();
        }

        if (boardT != null)
        {
            MapThingManager.Instance.RemoveButtonInfoBoard(boardT);
            boardT = null;
        }

        HideTip();
 
        StopAllCoroutines();
    }

    public void OnDestroy()
    {
 
        StopAllCoroutines();
    }

    public void SetData(StageConfig stage)
    {
 
        this.stage = stage;
    }

    void Create()
    {
        uiCamera = MapThingManager.Instance.InfoBoardContainer.GetComponent<Canvas>().worldCamera;

        boardT = MapThingManager.Instance.CreateButtonInfoBoard(GetAnchoredPosition() , lvlButton.Stage.ID);
        boardT.name = "board" + stage.ID;

        headImg = boardT.Find("ButtonInfoBoard").GetComponent<Image>();
        headImg.sprite = GameController.Instance.AtlasManager.GetSprite(Com4Love.Qmax.Atlas.UIMap, "Head" + stage.UnitHeadId);

        Q.Assert(headImg.sprite != null , "夥伴頭像素材缺少：" + "Head" + stage.UnitHeadId);

        headImg.SetNativeSize();
        animator = boardT.GetComponentInChildren<Animator>();
        ActiveTrigger = true;

        headButton = headImg.GetComponent<Button>();
        headButton.onClick.RemoveAllListeners();
        headButton.onClick.AddListener(OnHeadClick);

        TipImg = boardT.Find("ButtonInfoBoard/TipImg").GetComponent<Image>();
        TipMsgText = TipImg.transform.Find("MsgText").GetComponent<Text>();
        TipMsgText.text = Utils.GetTextByID(2356);
        yPosFlag = 0;
    }

    float delayTime;
    bool coroutineFlag = false;

    void OnHeadClick()
    {
        //彈出提示文字
        delayTime = 3;

        if (!coroutineFlag)
        {
            ShowTip();
            StartCoroutine(TipDelayHide());
        }
    }

    IEnumerator TipDelayHide()
    {
        coroutineFlag = true;

        while (delayTime > 0)
        {
            delayTime -= Time.deltaTime;
            yield return 0;
        }

        if (TipImg)
        {
            HideTip();
        }

        coroutineFlag = false;
    }

    public void ShowTip()
    {
        if (TipsInstance != null)
        {
            TipsInstance.HideTip();
        }

        TipsInstance = this;

        coverButton = MapThingManager.Instance.GetBoardCover().GetComponent<Button>();
        coverButton.gameObject.SetActive(true);
        coverButton.onClick.RemoveAllListeners();
        coverButton.onClick.AddListener(HideTip);

        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetTrigger("TextCreate");
            animator.SetBool("TextCreateBool", true);
        }
    }

    public void HideTip()
    {
        if (coverButton != null && TipsInstance == this)
        {
            coverButton.gameObject.SetActive(false);
            coverButton.onClick.RemoveAllListeners();
            TipsInstance = null;
        }

        StopAllCoroutines();
        coroutineFlag = false;

        if (animator != null && animator.isActiveAndEnabled)
        {
            if (animator.GetBool("TextCreateBool"))
            {
                animator.SetTrigger("TextRemove");
            }

            animator.SetBool("TextCreateBool", false);
        }
    }

    private Vector2 GetAnchoredPosition()
    {
        screenPos = MapView.Instance.mapCamera.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(MapThingManager.Instance.InfoBoardContainer, screenPos, uiCamera, out uiPos);

        return uiPos;
    }

    public void LateUpdate()
    {
        if (lvlButton.state == MapThing.ThingState.STATE_LOCKED)
        {
            //被鎖關卡按鈕默認不會去檢測可見性
            lvlButton.UpdateToCheckVisible();
        }
 
        if (lvlButton.CheckVisible())
        {
            if (boardT == null)
            {
                Create();
            }

            ActiveBoard();
            boardT.anchoredPosition = GetAnchoredPosition();
            playAudioEff();
        }
        else
        {
            HideBoard();
        }
    }

    void playAudioEff()
    {
        string audio = null;
        switch(stage.ID)
        {
            case 5:
                audio = "Vo_accompany_16";
                break;
            case 7:
                audio = "Vo_accompany_17";
                break;
            case 9:
                audio = "Vo_accompany_18";
                break;
            case 12:
                audio = "Vo_accompany_19";
                break;
        }
        if (audio == null)
        {
            return;
        }
        if ((yPosFlag < 0 && boardT.localPosition.y > 0) || (yPosFlag > 0 && boardT.localPosition.y < 0))
        {
            GameController.Instance.AudioManager.PlayAudio(audio);
        }
        yPosFlag = boardT.localPosition.y;
    }

    void ActiveBoard()
    {
        if (!ActiveTrigger && animator.isActiveAndEnabled)
        {
            animator.SetTrigger("Create");
            ActiveTrigger = true;
        }
    }

    void HideBoard()
    {
        if (ActiveTrigger && animator.isActiveAndEnabled)
        {
            animator.SetTrigger("Remove");
            HideTip();
            ActiveTrigger = false;
        }
    }
 
}
