using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugBehaviour : MonoBehaviour {
    public Text text;
    float clearTime = 5f;
    static DebugBehaviour _instance;

    public static void ClearLine()
    {
        _instance.text.text = "";
    }
 
    public static void WriteLine(object msg)
    {
        if (_instance == null || !_instance.enabled || !_instance.gameObject.activeInHierarchy)
        {
            return;
        }

        _instance.text.text = msg.ToString() + "\n" + _instance.text.text;
    }

    public static void Write(object msg)
    {
        _instance.text.text = msg.ToString();
    }
 
	// Use this for initialization
	void Start () {
        _instance = this;
	}
 
    public void OnEnable()
    {
        _instance = this;
    }

    public void OnDestroy()
    {
        _instance = null;
    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Update()
    {
        if (clearTime == 0f)
        {
            return;
        }

        clearTime -= Time.deltaTime;

        if (clearTime <= 0f)
        {
            ClearLine();
            clearTime = 0f;
        }
    }
}
