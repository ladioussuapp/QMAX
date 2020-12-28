using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class TestEventSystem : MonoBehaviour {
    [NonSerialized]
    protected List<RaycastResult> m_RaycastResultCache = new List<RaycastResult>();

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(null);
            pointerData.position = Input.mousePosition;
            pointerData.button = PointerEventData.InputButton.Left;
            RaycastAll(pointerData, m_RaycastResultCache);

            for (int i = 0; i < m_RaycastResultCache.Count; i++)
            {
                Debug.Log(m_RaycastResultCache[i].ToString());
            }
        }
	}

    public void RaycastAll(PointerEventData eventData, List<RaycastResult> raycastResults)
    {
        raycastResults.Clear();
        List<BaseRaycaster> modules = CanvasRaycasterManager.GetRaycasters();
        for (int i = 0; i < modules.Count; ++i)
        {
            var module = modules[i];
            if (module == null || !module.IsActive())
                continue;

            module.Raycast(eventData, raycastResults);
        }

        raycastResults.Sort(RaycastComparer);
    }

    private static int RaycastComparer(RaycastResult lhs, RaycastResult rhs)
    {
        if (lhs.module != rhs.module)
        {
            if (lhs.module.eventCamera != null && rhs.module.eventCamera != null && lhs.module.eventCamera.depth != rhs.module.eventCamera.depth)
            {
                // need to reverse the standard compareTo
                if (lhs.module.eventCamera.depth < rhs.module.eventCamera.depth)
                    return 1;
                if (lhs.module.eventCamera.depth == rhs.module.eventCamera.depth)
                    return 0;

                return -1;
            }

            if (lhs.module.sortOrderPriority != rhs.module.sortOrderPriority)
                return rhs.module.sortOrderPriority.CompareTo(lhs.module.sortOrderPriority);

            if (lhs.module.renderOrderPriority != rhs.module.renderOrderPriority)
                return rhs.module.renderOrderPriority.CompareTo(lhs.module.renderOrderPriority);
        }

        if (lhs.sortingLayer != rhs.sortingLayer)
        {
            // Uses the layer value to properly compare the relative order of the layers.
            var rid = SortingLayer.GetLayerValueFromID(rhs.sortingLayer);
            var lid = SortingLayer.GetLayerValueFromID(lhs.sortingLayer);
            return rid.CompareTo(lid);
        }

        if (lhs.sortingOrder != rhs.sortingOrder)
            return rhs.sortingOrder.CompareTo(lhs.sortingOrder);

        if (lhs.depth != rhs.depth)
            return rhs.depth.CompareTo(lhs.depth);

        if (lhs.distance != rhs.distance)
            return lhs.distance.CompareTo(rhs.distance);

        return lhs.index.CompareTo(rhs.index);
    }
}
