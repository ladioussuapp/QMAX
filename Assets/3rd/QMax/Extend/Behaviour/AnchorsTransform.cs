using UnityEngine;
using System.Collections;

public class AnchorsTransform : MonoBehaviour
{
    //public Vector3 anchors;
    protected Vector2 _anchors2d;       //0到1
    public Camera MainCamera;
    protected float aspect;

    protected Vector2 disAnchors2d;     //與錨點的距離

    public Vector2 anchors2d
    {
        set
        {
            _anchors2d = value;
            CalcDisAnchors2d();
        }
        get
        {
            return _anchors2d;
        }
    }


    // Use this for initialization
    void Start()
    {
        if (MainCamera == null)
        {
            MainCamera = Camera.main;
        }
 
    }

    protected void CalcDisAnchors2d()
    {
        if (MainCamera == null)
        {
            return;
        }

        Vector2 anchors2dPosition = MainCamera.ScreenToWorldPoint(anchors2d);
        Vector2 position = transform.position;

        disAnchors2d = anchors2dPosition - position;
        Debug.Log(disAnchors2d);
    }

    protected void Match()
    {
        Vector2 cutAnchors2dPosition = MainCamera.ScreenToWorldPoint(anchors2d);
        Vector2 cutPosition = transform.position;
        Vector2 cutDisAnchors2d = cutAnchors2dPosition - cutPosition;

        Debug.Log(disAnchors2d + "|" + cutDisAnchors2d);
 
    }

    // Update is called once per frame
    void Update()
    {
        if (aspect != MainCamera.aspect)
        {
            Match();
            aspect = MainCamera.aspect;
        }
    }
}
