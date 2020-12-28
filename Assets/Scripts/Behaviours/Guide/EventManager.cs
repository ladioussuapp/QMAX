/********************************************************************************
** auth： johnsonybq
** date： 2015/9/8 星期二 10:37:38
** FileName：EventManager
** desc： 尚未編寫描述
** Ver.:  V1.0.0
*********************************************************************************/

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 事件類型
///(根據需要取名稱，不得重複)
/// </summary>
public enum CustomEventType
{
    StartGame,
    ClickBlock
}


/// <summary>
/// 事件管理器
/// </summary>
public class EventManager
{

    /// <summary>
    /// 事件監聽池
    /// </summary>
    private static Dictionary<CustomEventType, DelegateEvent> eventTypeListeners = new Dictionary<CustomEventType, DelegateEvent>();

    /// <summary>
    /// 添加事件
    /// </summary>
    /// <param name="type">事件類型</param>
    /// <param name="listenerFunc">監聽函數</param>
    public static void addEventListener(CustomEventType type, DelegateEvent.EventHandler listenerFunc)
    {
        DelegateEvent delegateEvent;
        if (eventTypeListeners.ContainsKey(type))
        {
            delegateEvent = eventTypeListeners[type];
        }
        else
        {
            delegateEvent = new DelegateEvent();
            eventTypeListeners[type] = delegateEvent;
        }
        delegateEvent.addListener(listenerFunc);
    }

    /// <summary>
    /// 刪除事件
    /// </summary>
    /// <param name="type">事件類型</param>
    /// <param name="listenerFunc">監聽函數</param>
    public static void removeEventListener(CustomEventType type, DelegateEvent.EventHandler listenerFunc)
    {
        if (listenerFunc == null)
        {
            return;
        }
        if (!eventTypeListeners.ContainsKey(type))
        {
            return;
        }
        DelegateEvent delegateEvent = eventTypeListeners[type];
        delegateEvent.removeListener(listenerFunc);
    }

    /// <summary>
    /// 觸發某一類型的事件 並傳遞數據
    /// </summary>
    /// <param name="type">事件類型</param>
    /// <param name="data">事件的數據(可為null)</param>
    public static void dispatchEvent(CustomEventType type, object data)
    {
        if (!eventTypeListeners.ContainsKey(type))
        {
            return;
        }
        //創建事件數據
        EventData eventData = new EventData();
        eventData.type = type;
        eventData.data = data;

        DelegateEvent delegateEvent = eventTypeListeners[type];
        delegateEvent.Handle(eventData);
    }

}

/// <summary>
/// 事件類
/// </summary>
public class DelegateEvent
{
    /// <summary>
    /// 定義委託函數
    /// </summary>
    /// <param name="data"></param>
    public delegate void EventHandler(EventData data);
    /// <summary>
    /// 定義基於委託函數的事件
    /// </summary>
    public event EventHandler eventHandle;

    /// <summary>
    /// 觸發監聽事件
    /// </summary>
    /// <param name="data"></param>
    public void Handle(EventData data)
    {
        if (eventHandle != null)
            eventHandle(data);
    }

    /// <summary>
    /// 刪除監聽函數
    /// </summary>
    /// <param name="removeHandle"></param>
    public void removeListener(EventHandler removeHandle)
    {
        if (eventHandle != null)
            eventHandle -= removeHandle;
    }

    /// <summary>
    /// 添加監聽函數
    /// </summary>
    /// <param name="addHandle"></param>
    public void addListener(EventHandler addHandle)
    {
        eventHandle += addHandle;
    }
}

/// <summary>
/// 事件數據
/// </summary>
public class EventData
{
    /// <summary>
    /// 事件類型
    /// </summary>
    public CustomEventType type;
    /// <summary>
    /// 事件傳遞的數據
    /// </summary>
    public object data;
}