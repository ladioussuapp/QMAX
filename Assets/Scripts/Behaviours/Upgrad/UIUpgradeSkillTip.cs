using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;
using System.Collections.Generic;
using System;
using Com4Love.Qmax.Ctr;

public class UIUpgradeSkillTip : MonoBehaviour
{
    public Text SkillNameText;
    public Text SkillInfoText;
    public Sprite nextLvlTileSprite;
    public TileGrid TileGridS;
    public TileGrid TileGridB;
    public RectTransform TilePrefab;
    public UnitConfig LvlUnitData;
    public UnitConfig NextLvlUnitData;      //下一個夥伴等級的數據 
    public Button CoverButton;

    // Use this for initialization
    void Start()
    {
        TilePrefab.gameObject.SetActive(false);
        TileGridS.gameObject.SetActive(false);
        TileGridB.gameObject.SetActive(false);

        CoverButton.onClick.AddListener(EventListener_onClick);
        SkillConfig skillConfig = GameController.Instance.Model.SkillConfigs[LvlUnitData.UnitSkillId];
        SkillConfig nextSkillConfig = GameController.Instance.Model.SkillConfigs[NextLvlUnitData.UnitSkillId];
        TileObjectConfig skillTileObjConfig = GameController.Instance.Model.TileObjectConfigs[skillConfig.arg0];
        TileObjectConfig nextSkillTileObjConfig = GameController.Instance.Model.TileObjectConfigs[nextSkillConfig.arg0];
        int normalEleId = (int)skillTileObjConfig.ColorType;
        TileObjectConfig nomalObjConfig = GameController.Instance.Model.TileObjectConfigs[normalEleId];

        SkillNameText.text = GameController.Instance.UnitCtr.GetSkillNameStr(skillConfig);
        string skillInfo = GameController.Instance.UnitCtr.GetSkillEffectStr(skillConfig);
        string atkInfo = Utils.GetTextByID(1731, LvlUnitData.UnitAtk);
        SkillInfoText.text = string.Concat(skillInfo, "\n", atkInfo);

        GenerateTils(skillTileObjConfig, nextSkillTileObjConfig, nomalObjConfig);
    }


    public void OnDestroy()
    {
        CoverButton.onClick.RemoveAllListeners();
    }

    private void EventListener_onClick()
    {
        GameController.Instance.Popup.Close(Com4Love.Qmax.PopupID.UIUpgradeSkillTip);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cutSkillTOConfig">當前的技能配置</param>
    /// <param name="nextSkillTOConfig">下一等级的技能配置</param>
    /// <param name="nomalTOConfig">普通消除物的技能配置</param>
    void GenerateTils(TileObjectConfig cutSkillTOConfig, TileObjectConfig nextSkillTOConfig, TileObjectConfig nomalTOConfig)
    {
        //計算下一等級的技能排版。如果 TileGradS 放不下 則要使用 TileGradB
        int range = int.Parse(nextSkillTOConfig.Arg1);
        TileGrid tileGrid = range <= 3 ? TileGridS : TileGridB;
        tileGrid.gameObject.SetActive(true);
        List<Position> posesNext = ElementRuleCtr.CalcElementRange(nextSkillTOConfig, new Position(0, 0));
        List<Position> poses = ElementRuleCtr.CalcElementRange(cutSkillTOConfig, new Position(0, 0));
        string[] tmpPoses = new string[poses.Count];

        for (int i = 0; i < poses.Count; i++)
        {
            Position p = poses[i];

            if (p.Row == 0 && p.Col == 0)
            {
                //中心點，自己添加
                continue;
            }

            RectTransform tile = CreateTileImg(nomalTOConfig, false);
            tileGrid.Add(tile, p, new Vector3(1, 1, 1));
            tmpPoses[i] = p.ToString();
        }

        for (int j = 0; j < posesNext.Count; j++)
        {
            Position p = posesNext[j];

            //下一等級的技能，只需要顯示比當前等級技能增加的部分
            if (Array.IndexOf(tmpPoses, p.ToString()) > -1 || (p.Row == 0 && p.Col == 0))
            {
                continue;
            }

            RectTransform tile = CreateTileImg(nextSkillTOConfig, true);
            tileGrid.Add(tile, p, new Vector3(1, 1, 1));
        }

        RectTransform skillTile = CreateTileImg(cutSkillTOConfig, false);
        tileGrid.Add(skillTile, new Position(0, 0), new Vector3(1.3f, 1.3f));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tileObjConfig">需要根據配置顯示不同的技能ICON</param>
    /// <param name="preView">是否是下一個等級的技能效果</param>
    /// <returns></returns>
    RectTransform CreateTileImg(TileObjectConfig tileObjConfig, bool preView)
    {
        RectTransform tile = GameObject.Instantiate<RectTransform>(TilePrefab);
        tile.gameObject.SetActive(true);
        Sprite sprite;
        Image tileImg = tile.GetComponent<Image>();

        if (preView)
        {
            sprite = nextLvlTileSprite;
        }
        else
        {
            //下一等級的技能，用另外的素材
            string ResourceIcon = tileObjConfig.ResourceIcon + "HL";
            sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.Tile, ResourceIcon);
        }

        tileImg.sprite = sprite;

        return tile;
    }
}
