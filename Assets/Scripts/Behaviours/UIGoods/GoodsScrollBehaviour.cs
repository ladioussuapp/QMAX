using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class GoodsScrollBehaviour : MonoBehaviour, IDragHandler, IBeginDragHandler
{

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void OnBeginDrag(PointerEventData eventData)
    {
        GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear();
    }

    public void OnDrag(PointerEventData eventData)
    {
        GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear();
    }



}
