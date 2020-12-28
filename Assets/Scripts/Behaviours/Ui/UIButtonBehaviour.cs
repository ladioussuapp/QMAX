using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Com4Love.Qmax;


public class UIButtonBehaviour : MonoBehaviour
{
    public delegate void VoidDelegate(UIButtonBehaviour button);
    public event VoidDelegate onClick;
    public Button button;
    /// <summary>
    ///用圖片做的label 默認為正常狀態下的圖標
    /// </summary>
    public Image ImageWord;
    //普通狀態下的按鈕文字圖片
    public Sprite ImageWordNomal;
    //還需要在禁用狀態時指定不同的按鈕文字圖片
    public Sprite ImageWordDisable;

    private Animator animator;
    private AudioSource audioSource;
    private EventTriggerListener eventTriggerListener;

    ///需要聯動的gameobject///
    public GameObject LinkGameObject;

    public bool PointerDown = false;
    public bool FocusIn = false;

    public bool interactable
    {
        get
        {
            return button.interactable;
        }
        set
        {
            if (button == null)
            {
                button = transform.Find("ButtonOK").GetComponent<Button>();
            }

            button.interactable = value;

            if (ImageWord != null)
            {
                if (ImageWordDisable != null)
                {
                    ImageWord.sprite = value ? ImageWordNomal : ImageWordDisable;
                }
            }
        }
    }
 
    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (button == null)
        {
            button = transform.Find("ButtonOK").GetComponent<Button>();
        }

        Q.Assert(button != null, "UIButtonBehaviour下的button不能為空");

        if (ImageWordDisable != null && ImageWordNomal == null)
        {
            Q.Assert(false, "設置了禁用皮膚時一定要設置普通狀態的文字皮膚");
        }

        eventTriggerListener = EventTriggerListener.Get(button.gameObject);
        interactable = button.interactable;
        AddEvent();
    }

    void OnEnable()
    {
        PointerDown = false;
        FocusIn = false;
    }

    void AddEvent()
    {
        eventTriggerListener.onDown += OnDown;
        eventTriggerListener.onClick += OnClick;
        eventTriggerListener.onUp += OnUp;
        eventTriggerListener.onEnter += OnEnter;
        eventTriggerListener.onExit += OnExit;
    }

    void RemoveEvent()
    {
        eventTriggerListener.onDown -= OnDown;
        eventTriggerListener.onClick -= OnClick;
        eventTriggerListener.onUp -= OnUp;
        eventTriggerListener.onEnter -= OnEnter;
        eventTriggerListener.onExit -= OnExit;
    }

    void OnEnter(GameObject go)
    {
        //Debug.Log("OnEnter  pointerDown:" + PointerDown + " | focusIn:" + FocusIn + " | name:" + gameObject.name);

        if (!interactable)
        {
            return;
        }

        FocusIn = true;

        if (!PointerDown)
        {
            return;
        }

        if (true)
        {
            
        }
 
        AnimatorPlay("ButtonDown");
    }
 
    private void OnClick(GameObject go)
    {
        if (!interactable)
        {
            return;
        }

        if (gameObject.activeInHierarchy && audioSource != null && audioSource.clip != null)
        {
            //兼容之前設置的AudioClip 直接使用 AudioManager 播放audioSource的clip。
            GameController.Instance.AudioManager.PlayAudio(audioSource.clip);
        }
        if (onClick != null)
        {
            onClick(this);
        }
    }

    private void OnUp(GameObject go)
    {
        //Debug.Log("OnUP  pointerDown:" + PointerDown + " | focusIn:" + FocusIn + " | name:" + gameObject.name);

        if (!interactable)
        {
            return;
        }

        PointerDown = false;

        if (!FocusIn)
        {
            return;
        }

        AnimatorPlay("ButtonUp");
    }

    private void OnDown(GameObject go)
    {
        //Debug.Log("OnDown  pointerDown:" + PointerDown + " | focusIn:" + FocusIn + " | name:" + gameObject.name);

        if (!interactable)
        {
            return;
        }

        AnimatorPlay("ButtonDown");
        PointerDown = true;
    }

    private void OnExit(GameObject go)
    {
        //Debug.Log("OnExit  pointerDown:" + PointerDown + " | focusIn:" + FocusIn + " | name:" + gameObject.name);

        if (!interactable)
        {
            return;
        }

        if (!PointerDown)
        {
            return;
        }

        AnimatorPlay("ButtonUp");
        FocusIn = false;
    }
 
    private void AnimatorPlay(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }

        if (LinkGameObject != null)
        {
            Animator ani = LinkGameObject.GetComponent<Animator>();
            if (ani != null)
            {
                ani.Play(animationName);
            }
        }
    }

    void OnDestroy()
    {
        onClick = null;
        eventTriggerListener.Clear();
    }

    public void PlayHintAni()
    {
        AnimatorPlay("NewIconStart");

        Transform eff = transform.Find("EffectsLIGHT");

        if (eff != null)
        {
            ///第六幀播放例子效果///
            StartCoroutine(Utils.DelayToInvokeDo(delegate()
            {
                eff.gameObject.SetActive(true);
            }, .16f
            ));
        }
    }

    public void DelayDisplayAni()
    {
        AnimatorPlay("ICONAni");
    }

}
