using Com4Love.Qmax;
using Com4Love.Qmax.Data.Config;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 
/// </summary>
public class MonstorImmuneBehaviour
{

    private Image m_immuneSp;
    private GameController gameCtrl;
    private RectTransform m_flyLayer;
    //private BoardBehaviour m_boardBev;
    public MonstorImmuneBehaviour(Image immuneSp, RectTransform flyLayer, BoardBehaviour boardBev)
    {
        m_immuneSp = immuneSp;
        m_flyLayer = flyLayer;
        //m_boardBev = boardBev;
        gameCtrl = GameController.Instance;
    }


    /// <summary>
    /// 觸發怪物免疫狀態，在連接時，會在手指連接處出現盾牌
    /// </summary>
    /// <param name="count"></param>
    /// <param name="screenPoint"></param>
    /// <param name="ImmuneGO"></param>
    public void ImmuneOnDrag(int count, Vector2 screenPoint, GameObject ImmuneGO)
    {
        if (count == 1)
        {
            if (ImmuneGO == null)
            {
                ImmuneGO = new GameObject();
                Sprite immune = gameCtrl.AtlasManager.GetSprite(Atlas.UIBattle, "ShieldHL");
                Image immuneImage = ImmuneGO.AddComponent<Image>();
                immuneImage.overrideSprite = immune;
                immuneImage.SetNativeSize();
                ImmuneGO.transform.SetParent(m_flyLayer);
                ImmuneGO.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        Vector2 tempPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_flyLayer, screenPoint, Camera.main, out tempPoint);
        Vector3 targetLocalPos = new Vector3(tempPoint.x, tempPoint.y);
        ImmuneGO.transform.localPosition = targetLocalPos;
    }

    public void battleAttackProOver(ColorType colorType, EnemyPoint crtEnemyPoint)
    {
        if (crtEnemyPoint != null && crtEnemyPoint.HasEnemy)
        {
            SkillConfig skillCfg = crtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
            if (skillCfg != null && skillCfg.SkillColor == colorType)
            {
                changeImmuneIcon(skillCfg.ResourceIcon);
            }
        }
    }

    public void BattleEnemyHitEff(ColorType colorType, EnemyPoint crtEnemyPoint)
    {
        if (crtEnemyPoint != null && crtEnemyPoint.HasEnemy)
        {
            SkillConfig skillCfg = crtEnemyPoint.GetSkillConfigByType(SkillType.Shield);
            if (skillCfg != null && skillCfg.SkillColor == colorType)
            {
                changeImmuneIcon(skillCfg.ResourceIcon + "HL");
            }
        }
    }

    public void changeImmuneIcon(string iconName)
    {
        Sprite immuSp = gameCtrl.AtlasManager.GetSprite(Atlas.UIBattle, iconName);
        m_immuneSp.overrideSprite = immuSp;
        Animator ani = m_immuneSp.GetComponent<Animator>();

        if (ani == null)
            return;

        if (iconName.Contains("HL"))
        {
            ///播放防禦動畫///
            ani.Play("ImmuneIconActive");

            //Vector3 oriScale = new Vector3(0.8f, 0.8f, 0.8f);
            //Vector3 newScale = new Vector3(1, 1, 1);
            //m_immuneSp.rectTransform.localScale = oriScale;
            //m_immuneSp.rectTransform.localScale = newScale;

            //LeanTween.delayedCall(0.05f, delegate()
            //        {
            //            m_immuneSp.rectTransform.localScale = oriScale;
            //        });
        }
        else
        {
            ////播放正常动画
            ani.Play("ImmuneIconNormal");
            //m_immuneSp.rectTransform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }
    }

    public void unitAttackImmune(string spriteName)
    {
        m_immuneSp.gameObject.SetActive(true);
        m_immuneSp.overrideSprite = gameCtrl.AtlasManager.GetSprite(Atlas.UIBattle, spriteName);
    }

    public void cancelUnitAttackImmune()
    {
        //1209 Image物件有遺失，暫時註解觀察情況。
        m_immuneSp.gameObject.SetActive(false);
    }
}
