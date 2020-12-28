using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;
using System;

public class UIUpgradFootButton : MonoBehaviour
{
    public GameObject TipContainer;
    public Text TipText;        //顯示當前顏色可升級的伙伴數量
    public ColorType color;
    public event Action<UIUpgradFootButton> OnFootButtonClick;
    public Image body;
    public bool select = false;
    EventTriggerListener eventTrigger;

    // Use this for initialization
    void Start()
    {
        eventTrigger = EventTriggerListener.Get(gameObject);
        eventTrigger.onClick += eventTrigger_onClick;

        body.transform.localScale = select ? new Vector3(1f, 1f, 1f) : new Vector3(.8f, .8f, .8f);
        body.color = select ? Color.white : Color.gray;
    }

    void eventTrigger_onClick(GameObject go)
    {
        OnClick();
    }

    public void SetSelect(bool select_)
    {
        if (select == select_)
        {
            return;
        }

        select = select_;
        TweenToSelect();
        body.color = select ? Color.white : Color.gray;
    }

    void TweenToSelect()
    {
        Vector3 to = select ? new Vector3(1f, 1f, 1f) : new Vector3(.8f, .8f, .8f);

        LeanTween.value(body.gameObject, body.transform.localScale, to, .25f).setOnUpdate(delegate(Vector3 val)
        {
            body.transform.localScale = val;
        }).setEase(LeanTweenType.easeOutBack);
    }
 
    public void setData(UIUpgradStageItemData data)
    {
        int upgradCount = 0;
        UpGradUnitItem.ItemData itemData;

        for (int i = 0; i < data.unitItemDatas.Count; i++)
        {
            itemData = data.unitItemDatas[i];

            if (itemData != null && itemData.upgradAble)
            {
                upgradCount++;
            }
        }

        TipContainer.gameObject.SetActive(upgradCount > 0);
        TipText.text = upgradCount.ToString();
    }


    protected void OnClick()
    {
        if (OnFootButtonClick != null)
        {
            OnFootButtonClick(this);
        }
    }
}
