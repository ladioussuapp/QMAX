using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using Com4Love.Qmax;

public class TestLinkInteract : MonoBehaviour
{
    private LinkInteractBehaviour linkBeh;

    public RectTransform InteractLayer;


    private void LinkUpEvent(Vector2 screenPos)
    {
        ClearColor();
    }

    private void LinkDragEvent(Position obj, Vector2 screenPos)
    {
        //Q.Log("Drag {0}", obj);
        SetActiveTile(obj.Row, obj.Col);
    }

    private void LinkDownEvent(Position obj, Vector2 screenPos)
    {
        //Q.Log("Down {0}", obj);
        SetActiveTile(obj.Row, obj.Col);
    }

    void Start()
    {
        linkBeh = GetComponent<LinkInteractBehaviour>();
        linkBeh.DownEvent += LinkDownEvent;
        linkBeh.DragEvent += LinkDragEvent;
        linkBeh.UpEvent += LinkUpEvent;
    }

    private void SetActiveTile(int r, int c)
    {
        for (int i = 0, n = InteractLayer.childCount; i < n; i++)
        {
            string[] arr = InteractLayer.GetChild(i).name.Split('$');
            int tRow = Convert.ToInt32(arr[0]);
            int tCol = Convert.ToInt32(arr[1]);
            if (r == tRow && c == tCol)
            {
                InteractLayer.GetChild(i).GetComponent<Image>().color = Color.red;
            }
        }
    }

    private void ClearColor()
    {
        for (int i = 0, n = InteractLayer.childCount; i < n; i++)
        {
            Image img = InteractLayer.GetChild(i).GetComponent<Image>();
            img.color = Color.white;
            img.material = null;
        }
    }
}
