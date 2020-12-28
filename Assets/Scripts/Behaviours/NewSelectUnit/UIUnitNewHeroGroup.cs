using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.Unit;
using System.Collections.Generic;

using UnityEngine.UI;
public class UIUnitNewHeroGroup : MonoBehaviour {

	// Use this for initialization
    public int m_currentSelectIndex;
    [HideInInspector]
    public Transform m_transform;
    private LeanTweenType m_tweenType;
    public UIUnitNewItem[] m_unitList;
    private List<UnitConfig> m_unitListData;
    public Image m_mask;
    [HideInInspector]
    public bool b_isRunning;
    private List<ColorType> hasAttribles;
	void Start () 
    {
        hasAttribles = new List<ColorType>();
        m_transform = this.transform;
        m_tweenType = LeanTweenType.easeInOutSine;
        List<Unit> units = GameController.Instance.PlayerCtr.PlayerData.list;
        UnitConfig config = null;
        m_unitListData = new List<UnitConfig>();
        int count = units.Count;

        //UnitConfig[] configList = new UnitConfig[5];
        for (int ii = 0; ii < 5; ii++)
        {
            if (ii < count)
            {
                Unit item = units[ii];
                config = GameController.Instance.Model.UnitConfigs[item.unitId];
                if (config.UnitColor != 0)
                {
                    m_unitListData.Add(config);
                    if (!hasAttribles.Contains(config.UnitColor))
                    {
                        hasAttribles.Add(config.UnitColor);
                    }
                    UpdateItem(config, false, ii);
                }
            }
            else
            {
                UpdateItem(null, false, ii);
            }
        }    

        m_mask.gameObject.SetActive(true);
        
        m_currentSelectIndex = 1;
        for (int i = 0; i < 5; i++)
        {
            UIUnitNewItem item = m_unitList[i];
            GameObject go = item.gameObject;
            go.transform.localPosition = new Vector3(0, 0, 0);
        }
        b_isRunning = true;
        LeanTween.delayedCall(0.1f, PopUpInComplete);
	}

    ColorType getLeftColorRandom()
    {
        ColorType[] list = {ColorType.Earth,ColorType.Fire,ColorType.Golden,ColorType.Water,ColorType.Wood};
        ColorType result;
        do{
            int random = UnityEngine.Random.Range(0, 5);
            result = list[random];

        } while (hasAttribles.Contains(result));

        hasAttribles.Add(result);
        return result;
    }
    public List<UnitConfig> UnitListData()
    {
        return m_unitListData;
    }

    private void PopUpInComplete()
    {
        //b_isRunning = false;
        int radio = 254;                    //半徑
        int startAngle = 162;
        for (int i = 0; i < 5; i++)
        {
            UIUnitNewItem item = m_unitList[i];
            if(item != null)
            {
                GameObject go = item.gameObject;
                int angle1 = startAngle + i * 72;
                float x = Mathf.Cos(Mathf.PI * angle1 / 180) * radio;
                float y = Mathf.Sin(Mathf.PI * angle1 / 180) * radio;
                go.transform.localPosition = new Vector3(0, 0, 0);
                LeanTween.moveLocal(go, new Vector3(x, y, 0), 0.3f).setEase(m_tweenType).setOnUpdate(delegate(Vector3 val)
                {
                    go.transform.localPosition = val;
                }).setOnComplete(delegate()
                {
                    m_mask.gameObject.SetActive(false);
                });
            }
            
            RotationPanel(216);
        }
    }

    void Awake()
    {
        b_isRunning = false;
    }

    private UIUnitNewItem UpdateItem(UnitConfig config, bool flag, int index)
    {
        if((index>=0)&&(index <5))
        {
            UIUnitNewItem item = m_unitList[index];
            if(config != null)
            {
                item.Data = config;
                return item;
            }
            else
            {
                ColorType type = getLeftColorRandom();
                item.setDefault(type);
                return null;
            }
        }
        return null;
    }
	
	// Update is called once per frame
	void Update () {
	
	}


    /**
     * @des 點擊英雄後進行旋轉
     * @param int index 選中的英雄的索引
     **/
    public void onSelectHeroItem(int index)
    {

        GameController.Instance.Popup.ShowTextFloat("新版本將有更多夥伴加入", this.transform.parent as RectTransform);
        return;
        //if (index == m_currentSelectIndex || b_isRunning)
        //    return;
        //int dis = index - m_currentSelectIndex;
        //if(dis < 0)
        //{
        //    dis = dis + 5;
        //}
        //int rotation = dis * 72;
        //m_currentSelectIndex = index;

        //RotationPanel(rotation);
    }

    private void RotationPanel(int rotation)
    {
        b_isRunning = true;
        float roTime = 0.5f;
        int res2 = Mathf.RoundToInt(gameObject.transform.localEulerAngles.z) - rotation;
        LeanTween.value(gameObject, gameObject.transform.localEulerAngles.z, res2, roTime).setEase(m_tweenType).setOnUpdate(delegate(float val)
        {
            gameObject.transform.localEulerAngles = new Vector3(gameObject.transform.localEulerAngles.x, gameObject.transform.localEulerAngles.y, val);
        }).setOnComplete(delegate()
        {
            b_isRunning = false;
            gameObject.transform.localEulerAngles = new Vector3(gameObject.transform.localEulerAngles.x, gameObject.transform.localEulerAngles.y, res2);
        });

        foreach (UIUnitNewItem item in m_unitList)
        {
            if(item != null)
            {
                GameObject go = item.gameObject;
                int result = Mathf.RoundToInt(go.transform.localEulerAngles.z) + rotation;
                LeanTween.value(go, go.transform.localEulerAngles.z, result, roTime).setEase(m_tweenType).setOnUpdate(delegate(float val1)
                {
                    go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, go.transform.localEulerAngles.y, val1);
                }).setOnComplete(delegate()
                {
                    go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, go.transform.localEulerAngles.y, result);
                });
            }
            else
            {
                b_isRunning = false;
            }
        }
    }

    public void OnDestroy()
    {
        LeanTween.cancelAll(false);
        LeanTween.cancel(gameObject);
        for (int i = 0; i < 5; i++)
        {
            UIUnitNewItem item = m_unitList[i];
            LeanTween.cancel(item.gameObject);
            item.OnDestroy();
            item = null;
        }
        m_unitListData.Clear();
        b_isRunning = false;

        
    }
}
