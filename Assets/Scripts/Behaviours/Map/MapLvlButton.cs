using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapLvlButton : MapThing
{
    //從1開始
    //int chapter = 0;
    public StageConfig Stage;

    [System.NonSerialized]
    public GameObject button;

    Text lvlText;

    //等級沒達到鎖定
    protected bool isLocked = false;

    protected Animator animator;

    /// <summary>
    /// 相對於攝像機正方向的投影點乘。用來判斷自身是否在攝像機的投影範圍內。
    /// </summary>
    [HideInInspector]
    public float ProjectionDots = 0;
    /// <summary>
    ///自身方向與攝像機正方向的點乘。用來判斷相對於攝像機是正面還是背面。
    /// </summary>
    [HideInInspector]
    public float dirDots = 0;
 
    public bool isGemLocked = false;

    void Awake()
    {
        type = ThingType.Button;
        state = ThingState.STATE_UNLOCK;
    }

    public void SetData(StageConfig stage)
    {
        this.Stage = stage;

    }

    //目前在跳轉場景的時候地圖塊會直接銷毀不再緩存
    //override public void OnEnable()
    //{
    //    base.OnEnable();

    //    if (button == null)
    //    {
    //        return;
    //    }

    //    UpdateSkin();
    //}

    //public override void OnDisable()
    //{
    //    base.OnDisable();
    //    StopAllCoroutines();
    //}

    public virtual void SetSelect(bool val)
    {
        ThingState _state = state;

        if (val)
        {
            state = ThingState.STATE_SELECTED;
        }
        else
        {
            if (state == ThingState.STATE_SELECTED)
            {
                state = ThingState.STATE_UNLOCK;
            }
        }

        if (_state != state)
        {
            StateChange();
        }
    }

    public virtual void SetLock(bool val)
    {
        ThingState _state = state;

        if (val)
        {
            state = ThingState.STATE_LOCKED;
        }

        if (_state != state)
        {
            StateChange();
        }
    }

    public virtual void PlayUnLockEffect()
    {
        Transform effect = MapThingManager.Instance.GetMapButtonUnlockEffect();
        effect.SetParent(transform);
        effect.localPosition = Vector3.zero;
        effect.localRotation = button.transform.localRotation;
    }

    /// <summary>
    /// 強制刷新
    /// </summary>
    public void UpdateSkinNow()
    {
        StateChange();
    }

    //每個按鈕都刷新一次太耗，待更改，直接定位到某個按鈕，然後刷新    TODO
    public void UpdateSkin()
    {
        isLocked = GameController.Instance.PlayerCtr.PlayerData.passStageId < Stage.StagePassedUnlock;
        //選中效果由外部控制
        ThingState state_ = state;

        if (isLocked)
        {
            state_ = ThingState.STATE_LOCKED;
        }
        else
        {
            state_ = ThingState.STATE_UNLOCK;
        }

        if (state_ != state)
        {
            state = state_;
            StateChange();
        }
 
        if (state != ThingState.STATE_LOCKED)
        {
            CancelInvoke("UpdateToCheckVisible");
            InvokeRepeating("UpdateToCheckVisible", 0.1f, 0.1f);
        }
    }

    protected void StateChange()
    {
        if (button != null)
        {
            GameObject.DestroyObject(button);
            CreateButton();
        }

        if (state == ThingState.STATE_SELECTED)
        {
            Transform effect = MapThingManager.Instance.GetButtonEffect();
            effect.SetParent(transform);
            effect.localPosition = Vector3.zero;
            effect.localRotation = button.transform.localRotation;
        }
    }


    protected virtual void CreateButton()
    {
        isGemLocked = GameController.Instance.StageCtr.GetStageLockState(Stage.ID) != 0;
        button = GameController.Instance.QMaxAssetsFactory.CreateMapLvlButton(state, isGemLocked).gameObject;
        button.transform.parent = transform;
        button.transform.localPosition = Vector3.zero;
        button.transform.rotation = transform.rotation;
        button.name = string.Format("mapbutton_lvl{0}", Stage.ID.ToString());
        animator = button.GetComponent<Animator>();

        if (animator != null)
        {
            //點擊後會自動先激活再播放
            animator.enabled = false;
        }

        if (state == ThingState.STATE_UNLOCK || state == ThingState.STATE_SELECTED || isGemLocked)
        {
            //未鎖定與當前選中都有文字標籤
            lvlText = button.GetComponentInChildren<Text>();

            if (lvlText)
            {
                lvlText.gameObject.SetActive(true);
                lvlText.text = Stage.ID.ToString();
            }
        }
    }

    public void Create()
    {
        state = ThingState.STATE_UNLOCK;
        transform.LookAt(transform.parent.position + Vector3.right * transform.position.x);

        if (transform.right != transform.parent.right)
        {
            transform.rotation = transform.rotation * Quaternion.AngleAxis(-180f, Vector3.forward);
        }

        CreateButton();
        UpdateSkin();
    }

    protected override void Instance_OnSimpleTap(SimpleFinger obj)
    {
        base.Instance_OnSimpleTap(obj);

        if (obj.PickGameObj != button || GameController.Instance.Popup.HasPopup || GuideManager.getInstance().IsGuideRunning)
        {
            return;
        }

        if (state == ThingState.STATE_LOCKED)
        {
            return;
        }

        if (!touchDown)
        {
            return;
        }

        GameController.Instance.AudioManager.PlayAudio(UIAudioConfig.MAP_SCENE_BUTTON_CLICK);

        if (GameController.Instance.ViewEventSystem.ClickMapBtnEvent != null)
            GameController.Instance.ViewEventSystem.ClickMapBtnEvent(Stage.ID);
    }
 
    protected override void AddEvents()
    {
        base.AddEvents();

        SimpleTouch.Instance.OnTouchDown += Instance_OnTouchDown;
        SimpleTouch.Instance.OnTouchUp += Instance_OnTouchUp;
    }

    protected override void RemoveEvents()
    {
        base.RemoveEvents();
        SimpleTouch.Instance.OnTouchDown -= Instance_OnTouchDown;
        SimpleTouch.Instance.OnTouchUp -= Instance_OnTouchUp;
    }

    protected bool touchDown = false;

    protected virtual void Instance_OnTouchUp(SimpleFinger obj)
    {
        if (obj.PickGameObj != button)
        {
            return;
        }

        if (state == ThingState.STATE_LOCKED)
        {
            return;
        }

        touchDown = false;

        if (animator != null && !animator.GetBool("IsUp"))
        {
            animator.enabled = true;
            animator.SetTrigger("UpTrigger");
            animator.SetBool("IsDown", false);
            animator.SetBool("IsUp", true);
        }
    }

    protected virtual void Instance_OnTouchDown(SimpleFinger obj)
    {
        if (obj.PickGameObj != button || GameController.Instance.Popup.HasPopup || GuideManager.getInstance().IsGuideRunning)
        {
            return;
        }

        if (state == ThingState.STATE_LOCKED)
        {
            return;
        }

        if (!CheckVisible())
        {
            return;
        }

        touchDown = true;

        if (animator != null && !animator.GetBool("IsDown"))
        {
            animator.enabled = true;
            animator.SetTrigger("DownTrigger");
            animator.SetBool("IsUp", false);
            animator.SetBool("IsDown", true);
        }
 
    }


    //檢測按鈕是否可見可以不用每幀都執行
    public void UpdateToCheckVisible()
    {
        //首先判斷這個按鈕在不在攝像機的投影範圍內再判斷這個按鈕是否背向攝像機
        Vector3 dir = transform.position - MapView.Instance.mapCamera.transform.position;
        dir = Vector3.Normalize(dir);
        ProjectionDots = Vector3.Dot(dir, MapView.Instance.mapCamera.transform.forward);

        if (ProjectionDots < MapView.Instance.visibleDots)
        {
            //不在攝像機的範圍內
            isVisible = false;
            return;
        }

        dirDots = Vector3.Dot(MapView.Instance.mapCamera.transform.forward, transform.forward);

        isVisible = dirDots > 0;
    }
}
