using Com4Love.Qmax;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeMeCrash : MonoBehaviour
{
    public Button Btn1;
    public Button Btn2;

    public Transform TestObjLayer;

    
    public Transform OtherLayer;

    public GameObject OnePrefab;

    private Action<int, int> emptyDelegate;

    private List<Transform> eleList;


    void Start()
    {
        Btn1.onClick.AddListener(delegate()
        {
            Crash1();
        });

        Btn2.onClick.AddListener(delegate()
        {
            Crash2();
        });
    }


    /// <summary>
    /// 在Animator的StateMachineExitEvent事件回调函数中，执行以下步骤，crash.
    /// Unity 5.0.0f4
    /// 该问题在以下两个地方有描述，但又并非完全一样。所以不确定新版本的Unity是否已经修复
    /// http://cikusa.lofter.com/post/1cba884a_33f2d07
    /// http://forum.unity3d.com/threads/statemachinebehaviour-onstateexit-crashes-unity.314737/
    /// 
    /// Untiy 5.2.3f1 Editor状态不会Crash了，Android上Crash
    ///
    /// </summary>
    private void Crash1()
    {
        Debug.Log("Crash1");
        Action<Animator, int> OnStateMachineExit = null;
        OnStateMachineExit = delegate(Animator elementAnimator, int stateMachinePathHash)
        {
            elementAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent -= OnStateMachineExit;

            //注意三个条件的排列顺序
            //条件1
            elementAnimator.gameObject.SetActive(false);
            //条件2
            UnityEngine.Object.Instantiate<GameObject>(OnePrefab);
            //条件3
            throw new Exception("One Exception");
        };

        eleList = new List<Transform>();
        for (int i = 0, n = TestObjLayer.childCount; i < n; i++)
        {
            Transform trans = TestObjLayer.GetChild(i);
            if (!trans.gameObject.activeSelf)
                trans.gameObject.SetActive(true);
            Animator anim = trans.GetComponent<Animator>();
            eleList.Add(TestObjLayer.GetChild(i));
            anim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent += OnStateMachineExit;

            anim.SetInteger("ColorType", UnityEngine.Random.Range(1, 5));
            //随机选择一条路线
            anim.SetInteger("PathID", UnityEngine.Random.Range(1, 3));
            //随机选择一条弹开路线
            anim.SetInteger("Bounce", UnityEngine.Random.Range(0, 3));
        }
    }


    /// <summary>
    /// Untiy 5.2.3f1 Editor状态不会Crash了，Android上Crash
    /// 
    /// </summary>
    private void Crash2()
    {
        Debug.Log("Crash2");
        Action<Animator, int> OnStateMachineExit = null;
        OnStateMachineExit = delegate(Animator elementAnimator, int stateMachinePathHash)
        {
            elementAnimator.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent -= OnStateMachineExit;

            //创建实例就会crash
            GameObject newEle = UnityEngine.Object.Instantiate<GameObject>(elementAnimator.gameObject);
            newEle.transform.SetParent(elementAnimator.transform.parent);
            newEle.transform.localPosition = elementAnimator.transform.localPosition;
            newEle.transform.localScale = elementAnimator.transform.localScale;
        };

        //Action CreateOtherEle = delegate()
        //{
            
        //    GameObject newEle = UnityEngine.Object.Instantiate<GameObject>(OnePrefab);
        //    newEle.transform.SetParent(CreateOtherEle)
        //};

        eleList = new List<Transform>();
        for (int i = 0, n = TestObjLayer.childCount; i < n; i++)
        {
            Transform trans = TestObjLayer.GetChild(i);
            if (!trans.gameObject.activeSelf)
                trans.gameObject.SetActive(true);
            Animator anim = trans.GetComponent<Animator>();
            eleList.Add(TestObjLayer.GetChild(i));
            anim.GetBehaviour<BaseStateMachineBehaviour>().StateMachineExitEvent += OnStateMachineExit;

            anim.SetInteger("ColorType", UnityEngine.Random.Range(1, 5));
            //随机选择一条路线
            anim.SetInteger("PathID", UnityEngine.Random.Range(1, 3));
            //随机选择一条弹开路线
            anim.SetInteger("Bounce", UnityEngine.Random.Range(0, 3));
        }
    }
}
