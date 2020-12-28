using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


public class CanvasPhysicsRaycaster : BaseRaycaster
{
    /// <summary>
    /// Const to use for clarity when no event mask is set
    /// </summary>
    protected const int kNoEventMaskSet = -1;

    protected Camera m_EventCamera;

    /// <summary>
    /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
    /// </summary>
    [SerializeField]
    protected LayerMask m_EventMask = kNoEventMaskSet;

    protected CanvasPhysicsRaycaster()
    { }

    protected override void OnEnable()
    {
        base.OnEnable();
        //CanvasRaycasterManager.AddRaycaster(this);
    }

    protected override void OnDisable()
    {
        //CanvasRaycasterManager.RemoveRaycasters(this);
        base.OnDisable();
    }

    public override Camera eventCamera
    {
        get
        {
            //if (m_EventCamera == null)
            //    m_EventCamera = GetComponent<Camera>();
            //return m_EventCamera ?? Camera.main;

            return Camera.main;
        }
    }


    /// <summary>
    /// Depth used to determine the order of event processing.
    /// </summary>
    public virtual int depth
    {
        get { return (eventCamera != null) ? (int)eventCamera.depth : 0xFFFFFF; }
    }

    /// <summary>
    /// Event mask used to determine which objects will receive events.
    /// </summary>
    public int finalEventMask
    {
        get { return (eventCamera != null) ? eventCamera.cullingMask & m_EventMask : kNoEventMaskSet; }
    }

    /// <summary>
    /// Layer mask used to filter events. Always combined with the camera's culling mask if a camera is used.
    /// </summary>
    public LayerMask eventMask
    {
        get { return m_EventMask; }
        set { m_EventMask = value; }
    }

    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        if (eventCamera == null)
            return;


        Ray ray = eventCamera.ScreenPointToRay(eventData.position);
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 99f);
        
        float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

        RaycastHit[] hits = Physics.RaycastAll(ray, dist);

        Debug.Log("eventData.position:" + eventData.position +  "       Raycast hits.count:" + hits.Length);

        if (hits.Length > 1)
            System.Array.Sort(hits, (r1, r2) => r1.distance.CompareTo(r2.distance));

        if (hits.Length != 0)
        {
            for (int b = 0, bmax = hits.Length; b < bmax; ++b)
            {
                var result = new RaycastResult
                {
                    gameObject = hits[b].collider.gameObject,
                    module = this,
                    distance = hits[b].distance,
                    worldPosition = hits[b].point,
                    worldNormal = hits[b].normal,
                    screenPosition = eventData.position,
                    index = resultAppendList.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                };

                //Debug.Log("index:" + b + " | " + result.ToString());
                resultAppendList.Add(result);
            }
        }
    }
}


internal static class CanvasRaycasterManager
{
    private static readonly List<BaseRaycaster> s_Raycasters = new List<BaseRaycaster>();

    public static void AddRaycaster(BaseRaycaster baseRaycaster)
    {
        if (s_Raycasters.Contains(baseRaycaster))
            return;

        s_Raycasters.Add(baseRaycaster);
    }

    public static List<BaseRaycaster> GetRaycasters()
    {
        return s_Raycasters;
    }

    public static void RemoveRaycasters(BaseRaycaster baseRaycaster)
    {
        if (!s_Raycasters.Contains(baseRaycaster))
            return;
        s_Raycasters.Remove(baseRaycaster);
    }
}
