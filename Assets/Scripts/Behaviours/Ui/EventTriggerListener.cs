using UnityEngine;
using UnityEngine.EventSystems;

//EventTrigger 會把所有的事件都截取掉， 修改為自己實現各種事件接口
public class EventTriggerListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler,ISelectHandler
{
    public delegate void VoidDelegate(GameObject go);
    public event VoidDelegate onClick;
    public event VoidDelegate onDown;
    public event VoidDelegate onEnter;
    public event VoidDelegate onExit;
    public event VoidDelegate onUp;
    public event VoidDelegate onSelect;
    public event VoidDelegate onUpdateSelect;

    static public EventTriggerListener Get(GameObject go)
    {
        EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
        if (listener == null) listener = go.AddComponent<EventTriggerListener>();
        return listener;
    }

    public void Clear()
    {
        onClick = null;
        onDown = null;
        onEnter = null;
        onExit = null;
        onUp = null;
        onSelect = null;
        onUpdateSelect = null;
    }

    protected void OnDestroy()
    {
        Clear();
    }

    

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null) onClick(gameObject);
        //Debug.Log("OnPointerClick   " + gameObject.name);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (onDown != null) onDown(gameObject);
        //Debug.Log("OnPointerDown   " + gameObject.name);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onEnter != null) onEnter(gameObject);
        //Debug.Log("OnPointerEnter   " + gameObject.name);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (onExit != null) onExit(gameObject);
        //Debug.Log("OnPointerExit   " + gameObject.name);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (onUp != null) onUp(gameObject);
        //Debug.Log("OnPointerUp   " + gameObject.name);
    }
    public void OnSelect(BaseEventData eventData)
    {
        if (onSelect != null) onSelect(gameObject);
        //Debug.Log("OnSelect   " + gameObject.name);
    }
    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (onUpdateSelect != null) onUpdateSelect(gameObject);
        //Debug.Log("OnUpdateSelected   " + gameObject.name);
    }
}
