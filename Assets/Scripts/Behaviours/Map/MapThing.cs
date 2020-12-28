using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using UnityEngine.EventSystems;

//地圖場景中的各種物品
public enum ThingType
{
    //特效，地面，按鈕，禮品，碰撞器,音效
    Effect, Plane, Button, Gift, Collider, Audio
}

//放在地圖上的各種物品 包括按鈕，可拾取物
public abstract class MapThing : MonoBehaviour
{
    protected ThingType type;
    public bool touchAble = true;
    protected bool dataReady;
    public ThingState state;
    protected bool isVisible = false;

    public enum ThingState
    {
        STATE_UNLOCK,
        STATE_LOCKED,
        STATE_SELECTED
    }
 
    public virtual void OnDestroy()
    {
        RemoveEvents();
    }

    public virtual void OnDisable()
    {
        RemoveEvents();
    }

    public virtual void OnEnable()
    {
        AddEvents();
    }

    protected virtual void AddEvents()
    {
        RemoveEvents();

        if (touchAble && SimpleTouch.Instance != null)
        {
            SimpleTouch.Instance.OnSimpleTap += Instance_OnSimpleTap;
        }
    }

    protected virtual void Instance_OnSimpleTap(SimpleFinger obj)
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }
    }
 
    protected virtual void RemoveEvents()
    {
        if (touchAble && SimpleTouch.Instance != null)
        {
            SimpleTouch.Instance.OnSimpleTap -= Instance_OnSimpleTap;
        }
    }

    /// <summary>
    /// 可見性檢測
    /// </summary>
    /// <returns></returns>
    public virtual bool CheckVisible()
    {
        return isVisible;
    }
}
