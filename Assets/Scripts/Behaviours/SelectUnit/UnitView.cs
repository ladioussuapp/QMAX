using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

/// <summary>
/// 单个的伙伴选择界面
/// </summary>
public class UnitView : MonoBehaviour {
    public GameObject ArrowUp;
    public GameObject ArrowDown;
    public Image SkillIcon;
    public Text SkillMsgText;
    public UnitList UnitList;
    public Image LockImg; 
    public ColorType Color;

	// Use this for initialization
	void Start () {
        //arrowUp.SetActive(false);
        //arrowDown.SetActive(false);



        //Debug.Log(GameBehaviour.Instance.Model.UnitConfigs);
	}

    void Awake()
    {
        UnitList.Color = Color;
        UnitList.ArrowDown = ArrowDown;
        UnitList.ArrowUp = ArrowUp;
 
        UnitList.LockImg = LockImg;
    }
	
	// Update is called once per frame
	void Update () {
	    
	}
}
