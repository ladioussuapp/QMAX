using UnityEngine;
using System.Collections;
using System;

public class SimpleTouch : MonoBehaviour
{
    public LayerMask TouchLayer;
    public Camera Camera;
    public event Action<SimpleFinger> OnSimpleTap;
    public event Action<SimpleFinger> OnTouchDown;
    public event Action<SimpleFinger> OnTouchUp;

    private SimpleTouchInput input;
    private SimpleFinger currentFinger;

    public static SimpleTouch Instance;

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        input = new SimpleTouchInput();
    }

    // Update is called once per frame
    void Update()
    {
        int count = input.GetTouchCount();

        if (count == 0)
        {
            return;
        }

        input.UpdateFingers();

        //for (int i = 0; i < count; i++)
        //{
        //    SimpleFinger finger = input.fingers[i];

        //    if (finger.Phase == TouchPhase.Began)
        //    {
        //        finger.PickGameObj = GetCameraPickGo();
        //    }
        //    else if (finger.Phase == TouchPhase.Canceled)
        //    {

        //    }
        //}

        //暫時只管單點的情況
        if (count == 1)
        {
            if (currentFinger == null)
            {
                //第一次點擊
                currentFinger = input.fingers[0];
            }

            GameObject pickGo = GetCameraPickGo();

            if (currentFinger.Phase == TouchPhase.Began)
            {
                currentFinger.PickGameObj = pickGo;

                if (OnTouchDown != null)
                {
                    OnTouchDown(currentFinger);
                }
            }
            else if (currentFinger.Phase == TouchPhase.Ended || currentFinger.Phase == TouchPhase.Canceled)
            {
                if (currentFinger.Phase == TouchPhase.Ended)
                {
                    if (pickGo == currentFinger.PickGameObj)
                    {
                        if (OnSimpleTap != null)
                        {
                            OnSimpleTap(currentFinger.Clone());
                        }
                    }
                }

                if (OnTouchUp != null)
                {
                    OnTouchUp(currentFinger);
                }

                currentFinger = null;
            }
            //    else if (currentFinger.Phase == TouchPhase.Moved)
            //    {
            //        if (currentFinger.PickGameObj != null && pickGo != currentFinger.PickGameObj)
            //        {
            //            //手指移開

            //            if (OnTouchUp != null)
            //            {
            //                OnTouchUp(currentFinger);
            //            }


            //        }
            //    }
        }
        else
        {
            currentFinger = null;
        }
    }

    GameObject GetCameraPickGo()
    {
        if (currentFinger == null)
        {
            return null;
        }

        GameObject go = null;
        Vector2 touchPos = currentFinger.Pos;
        Ray ray = Camera.ScreenPointToRay(touchPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, float.MaxValue, TouchLayer))
        {
            go = hit.collider.gameObject;
        }

        return go;
    }
}

public class SimpleFinger
{
    public int Id = int.MinValue;  //index 或者 touchid
    public Vector2 Pos = Vector2.zero;
    public TouchPhase Phase;
    public GameObject PickGameObj = null;
    public SimpleFinger Clone()
    {
        SimpleFinger finger = new SimpleFinger();
        finger.Id = this.Id;
        finger.Pos = this.Pos;
        finger.PickGameObj = this.PickGameObj;

        return finger;
    }
}

public class SimpleTouchInput
{
    public SimpleFinger[] fingers;
    public bool RemoteTest = false;

    public SimpleTouchInput()
    {
        fingers = new SimpleFinger[10];

        for (int i = 0; i < 10; i++)
        {
            fingers[i] = new SimpleFinger();
        }
    }

    public int GetTouchCount()
    {
        int count = 0;

#if (((UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_BLACKBERRY) && !UNITY_EDITOR)) 
        count = Input.touchCount;
#else
        if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            count = 1;
        }
#endif

        return count;
    }

    public void UpdateFingers()
    {
#if (((UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_BLACKBERRY) && !UNITY_EDITOR))

        //for (int i = 0; i < Input.touchCount; i++)
        //{
        //    Touch touch = Input.touches[i];
        //    SimpleFinger finger = fingers[i];
        //    finger.Phase = touch.phase;
        //    finger.Id = touch.fingerId;
        //    finger.Pos = touch.position;
        //}

        //单点
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            SimpleFinger finger = fingers[0];
            finger.Phase = touch.phase;
            finger.Id = touch.fingerId;
            finger.Pos = touch.position;
        }
#else
        if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
        {
            SimpleFinger finger = fingers[0];
            finger.Id = 0;
            finger.Pos = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                finger.Phase = TouchPhase.Began;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                finger.Phase = TouchPhase.Ended;
            }
            else
            {
                finger.Phase = TouchPhase.Moved;
            }
        }
#endif
    }
}
