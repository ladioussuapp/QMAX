using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;
using Com4Love.Qmax.Data;
using System;

public class BattleUnitInfoBehaviour : MonoBehaviour
{
    private UnitConfig config;

    public Image SkillIcon;

    public Text UnitLevel;

    public Text UnitName;

    public Text UnitAttack;

    public Image TypeIcon;

    public Image LevelBg;

    public Text LeftEle;

    private GameController gameCtr;

    private BattleModel battleModel;

    public SkillLoadingBehaviour UnitSkillLoading;

    void Awake()
    {
        gameCtr = GameController.Instance;
    }
    // Use this for initialization
    void Start()
    {
        Debug.Log("BattleUnitInfoBehaviour");
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void initUnitConfig(BattleModel battleModel, UnitConfig config, ColorType type)
    {
        this.battleModel = battleModel;
        this.config = config;
        string[] strs = { 
            "ElementPurple",
            "ElementRed",
            "ElementGreen",
            "ElementBlue",
            "ElementYellow"};

        if (this.config != null)
        {
            int skillId = config.UnitSkillId;
            SkillConfig skillCfg = gameCtr.Model.SkillConfigs[skillId];
            Sprite skillIcon = gameCtr.AtlasManager.GetSprite(Atlas.Tile, skillCfg.ResourceIcon);
            SkillIcon.overrideSprite = skillIcon;
            SkillIcon.SetNativeSize();

            //ColorType colorType = config.UnitColor;
            //int skillCD = battleModel.SkillCDDict[colorType];
            //int left = skillCfg.SkillCD - skillCD;
            UnitLevel.text = config.Level + "";

            UnitName.text = GameController.Instance.UnitCtr.GetUnitNameStr(config);
            UnitSkillLoading.SetSprite(skillIcon);
            UnitAttack.text = config.UnitAtk + "";
            
            TypeIcon.overrideSprite = gameCtr.AtlasManager.GetSprite(Atlas.Tile, strs[(int)config.UnitColor - 1]);
            TypeIcon.SetNativeSize();

            string levelBgName = "Battle_LevelBG_" + config.UnitColor;
            Sprite levelIcon = gameCtr.AtlasManager.GetSprite(Atlas.UIBattle, levelBgName);
            LevelBg.overrideSprite = levelIcon;
            LevelBg.SetNativeSize();
        }
        else
        {
            UnitSkillLoading.gameObject.SetActive(false);
            UnitLevel.gameObject.SetActive(false);
            LevelBg.gameObject.SetActive(false);
            TypeIcon.overrideSprite = gameCtr.AtlasManager.GetSprite(Atlas.Tile, strs[(int)type - 1]);
            TypeIcon.SetNativeSize();
            UnitAttack.text = "55";
            UnitName.text = "空置";
        }
        
    }

    public void UpdateSkill()
    {
        if(this.config != null)
        {
            int skillId = config.UnitSkillId;

            SkillConfig skillCfg = gameCtr.Model.SkillConfigs[skillId];
            ColorType colorType = config.UnitColor;
            int skillCD = battleModel.SkillCDDict[colorType];
            int left = skillCfg.SkillCD - skillCD;
            LeftEle.text = left + "";

            //ColorType colorType = config.UnitColor;
            //int skillCD = battleModel.SkillCDDict[colorType];
            UnitSkillLoading.gameObject.SetActive(true);
            
            
            UnitSkillLoading.SetPercentage(Math.Min(1, (float)skillCD / skillCfg.SkillCD));
        }
    }
}
