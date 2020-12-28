using UnityEngine;
using System.Collections;
using System;
using Com4Love.Qmax;
using UnityEngine.EventSystems;

public class MapMove : MonoBehaviour
{
    public delegate void MapMoveHandler(float deltaY);

    public event MapMoveHandler onMapMove;
    /// <summary>
    /// 執行一次就清除了
    /// </summary>
    public event Action OnMapMoveFinish;

    public float RotateSpeed = 0.5f;
    public float SlideSpeed = 2.8f;
    public float SlideSmooth = 1.6f;
    public float BounceAngle = 5;

    [NonSerialized]
    [HideInInspector]
    /// <summary>
    /// 限制拖動之後能拖動的距離
    /// </summary>
    public float TouchAngleLimit = 1440f;
    public float touchAngleLimit;
    //float rotateSpeed;
    //float moveLimitRotateSpeed;

    public Transform wind; //跟著一起旋轉
    //選中按鈕朝向的觀察方向
    public Transform LookForDir;
    public bool TouchAble = true;

    protected Quaternion quaternionSpeed;
    protected Quaternion moveToQuaternion;
    protected Quaternion limitQuaternion;

    [HideInInspector]
    public Camera Camera;

    //可以滾動，放手後會回彈。
    bool moveLimit = false;
    bool moveUpLimit = false;
    bool moveDownLimit = false;

    public bool MoveUpLimit
    {
        set
        {
            if (moveUpLimit != value)
            {
                moveUpLimit = value;
                moveLimit = moveUpLimit || moveDownLimit;

                if (moveLimit)
                {

                    touchAngleLimit = TouchAngleLimit;
                    limitQuaternion = transform.rotation * Quaternion.AngleAxis(-BounceAngle, Vector3.right);
                }
            }
        }
        get
        {
            return moveUpLimit;
        }
    }

    public bool MoveDownLimit
    {
        set
        {
            if (moveDownLimit != value)
            {
                moveDownLimit = value;
                moveLimit = moveUpLimit || moveDownLimit;

                if (moveLimit)
                {
                    touchAngleLimit = TouchAngleLimit;
                    limitQuaternion = transform.rotation * Quaternion.AngleAxis(BounceAngle, Vector3.right);
                }
            }
        }
        get
        {
            return moveDownLimit;
        }
    }

    public bool MoveLimit
    {
        get
        {
            return moveLimit;
        }
    }

    public bool TouchDown
    {
        get
        {
            return touchDown;
        }
    }
 
    public void Start()
    {
        moveToQuaternion = transform.rotation;
        wind.rotation = this.transform.rotation;
    }

    float preTouchY = 0f;
    float touchY = 0f;
    float deltaTouchY = 0f;
    bool touchDown = false;

    public IEnumerator LookButton(Transform t, bool tween = true)
    {
        //目標方向
        Vector3 lookDir = transform.InverseTransformVector(LookForDir.forward);
        //源方向
        Vector3 sourceDir = transform.InverseTransformVector(-t.forward);
        Quaternion q = Quaternion.FromToRotation(sourceDir, lookDir);
        moveToQuaternion = transform.rotation * q;

        if (!tween)
        {
            wind.rotation = transform.rotation = moveToQuaternion;

            if (onMapMove != null)
            {
                onMapMove(deltaTouchY);
            }
        }

        bool complete = false; 

        Action LookButtonComplete = null;

        LookButtonComplete = delegate () {
            OnMapMoveFinish -= LookButtonComplete;
            complete = true;
        };

        OnMapMoveFinish += LookButtonComplete;

        //點擊會中斷找按鈕這個動作。當TouchAble為false時，touchDown不會因為點擊而賦值
        while (!complete && !touchDown)
        {
            yield return 0;
        }

        OnMapMoveFinish -= LookButtonComplete;
    }

    Touch touch;

    void MoveHandler()
    {
        if (!CheckTouchCount() || !TouchAble)
        {
            return;
        }

        GetTouch();

        if (GameController.Instance.Popup.HasPopup || (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null))
        {
            return;
        }

        float toAngle;

        if (CheckTouchDown())
        {
            //當前如果是限制滾動狀態，則不接收新的手指動作
            touchDown = true;
            touchY = preTouchY = GetTouchY();
        }
        else if (touchDown && CheckTouchMove())
        {
            touchY = GetTouchY();
            deltaTouchY = touchY - preTouchY;
            preTouchY = touchY;

            if (deltaTouchY == 0)
            {
                return;
            }

            deltaTouchY = deltaTouchY > 100f ? 100f : deltaTouchY;
            deltaTouchY = deltaTouchY < -100f ? -100f : deltaTouchY;

            toAngle = RotateSpeed * deltaTouchY;

            if (moveLimit)
            {
                if (moveDownLimit && toAngle < 0)
                {
                    touchAngleLimit += toAngle;
                    toAngle = touchAngleLimit > 0 ? toAngle : 0;
                }
                else if (moveUpLimit && toAngle > 0)
                {
                    touchAngleLimit -= toAngle;
                    toAngle = touchAngleLimit > 0 ? toAngle : 0;
                }
            }
 
            quaternionSpeed = Quaternion.AngleAxis(toAngle, Vector3.right);

            moveToQuaternion = this.transform.rotation * quaternionSpeed;

            if (onMapMove != null && deltaTouchY != 0)
            {
                onMapMove(deltaTouchY);
            }
        }
        else if (touchDown && CheckTouchUp())
        {

            touchDown = false;

            if (deltaTouchY == 0 || Mathf.Abs(deltaTouchY) < 5)
            {
                return;
            }

            toAngle = deltaTouchY * SlideSmooth;

            deltaTouchY = 0;

            if (toAngle > 179)
            {
                toAngle = 179;
            }
            else if (toAngle < -179)
            {
                toAngle = -179;
            }

            moveToQuaternion = transform.rotation * Quaternion.AngleAxis(toAngle, Vector3.right);
        }
    }

    bool CheckTouchCount()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return true;
#else
        return Input.touchCount > 0;
#endif
    }

    void GetTouch()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return;
#else
        touch = Input.GetTouch(0);
#endif
    }

    bool CheckTouchDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonDown(0);
#else
        return touch.phase == TouchPhase.Began;
#endif
    }

    bool CheckTouchMove()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButton(0);
#else
        return touch.phase == TouchPhase.Moved;
#endif
    }

    bool CheckTouchUp()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.GetMouseButtonUp(0);
#else
        return touch.phase == TouchPhase.Ended;
#endif
    }

    float GetTouchY()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.mousePosition.y;
#else
        return touch.position.y;
#endif
    }

    //Quaternion targetQuaternion;

    // Update is called once per frame
    void Update()
    {
        MoveHandler();
    }

    //void TouchMoveUpdate()
    //{

    //}

    public void AutoMove(float toAngle)
    {
        moveToQuaternion = transform.rotation * Quaternion.AngleAxis(toAngle, Vector3.right);
        wind.rotation = transform.rotation = moveToQuaternion;

        if (onMapMove != null)
        {
            onMapMove(deltaTouchY);
        }
    }

    public void LateUpdate()
    {
        if (moveLimit && !touchDown)
        {
            //Debug.Log("moveLimit:" + moveLimit);
            moveToQuaternion = limitQuaternion;
        }

        if (transform.rotation != moveToQuaternion)
        {
            //Debug.Log(deltaTouchY + "|(moveDownLimit:" + moveDownLimit + ", moveUpLimit:" + moveUpLimit + ")");
            wind.rotation = this.transform.rotation = Quaternion.Lerp(transform.rotation, moveToQuaternion, 0.02f * SlideSpeed);

            if (onMapMove != null)
            {
                onMapMove(deltaTouchY);
            }
        }
        else
        {
            if (OnMapMoveFinish != null)
            {
                OnMapMoveFinish();
                OnMapMoveFinish = null;
            }
        }
    }
}
