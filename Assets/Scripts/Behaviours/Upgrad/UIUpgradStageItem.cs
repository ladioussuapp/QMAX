using UnityEngine;
using System.Collections;
using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using System.Collections.Generic;

//每屏的內容 需要根據分辨率匹配動態去設置寬度
public class UIUpgradStageItem : ScrollRectItem
{

    public UpGradUnitItem[] UnitItems;

    /// <summary>
    /// 新手引導開關
    /// </summary>
    private bool checkGuide;
 
    protected override void OnDataChange()
    {
        base.OnDataChange();

        UIUpgradStageItemData stageItemData = data as UIUpgradStageItemData;
        List<UpGradUnitItem.ItemData> unitItemdatas = stageItemData.unitItemDatas;
        UpGradUnitItem.ItemData unitItemdata;
        UpGradUnitItem item;

        for (int i = 0; i < unitItemdatas.Count; i++)
        {
            unitItemdata = unitItemdatas[i];

            if (i < UnitItems.Length)
            {
                item = UnitItems[i];
                item.SetData(unitItemdata , !stageItemData.IsDefaultStage);     //不是默認第一頁的都要延遲加載素材
            }
        }
        switch (GuideManager.getInstance().version)
        {
            case GuideVersion.Version_1:
                if (stageItemData.ColorType == ColorType.Wood)
                {
                    Debug.Log("UnitUpgradeIcon:" + index);
                    StartCoroutine(addFliter());
                }
                break;
            default:
                break;
        }
    }


    public void OnDestory()
    {
        removeFliter();
    }

    void Update()
    {
        //GuideVersion.Version_1 的邏輯
        //if (!checkGuide || index != 4 || GuideManager.getInstance().guideIndex != 3)
        //    return;
        if (!checkGuide || index != 4 || GuideManager.getInstance().CurrentGuideID() != 3)
            return;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPos.x < Screen.width)
        {
            if (!UIUpgradBehaviour.Instance.StageList.IsScrolling && IsSelect)
            {
                Debug.Log("UnitUpgradeIcon:" + index);
                StartCoroutine(addFliter());
                checkGuide = false;
            }
        }
    }

    private IEnumerator addFliter()
    {
        yield return new WaitForSeconds(1.0f);
        GuideNode node = new GuideNode();
        node.TargetNode = UnitItems[0];
        node.TarCamera = Camera.main;
        node.index = 2;
        node.ShowMask = false;
        node.ShowTips = true;
        node.CallBack = delegate()
        {
            UpGradUnitItem item = UnitItems[0];
            item.OnUpgradButtonClick();
        };
        GuideManager.getInstance().addGuideNode("UnitUpgradeIcon", node);

    }

    private void removeFliter()
    {
        GuideManager.getInstance().removeGuideNode("UnitUpgradeIcon");
    }
}

public class UIUpgradStageItemData : ScrollRectItemData
{
    public bool IsDefaultStage = false;     //如果是默認的頁面則直接加載夥伴圖片，否則延遲一幀加載
    public UnitConfig upgradeConfig;    //正在升級的伙伴
    public ColorType ColorType;
    public List<UpGradUnitItem.ItemData> unitItemDatas;
}
