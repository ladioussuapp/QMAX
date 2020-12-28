using Com4Love.Qmax.Data.Config;
using UnityEngine;
using UnityEngine.UI;

public class UnitNamesBar : MonoBehaviour
{
    public Text LvlText;
    public Text NameText;
    public Image ColorBg;
    protected Vector3[] BG_COLORS = new Vector3[] { new Vector3(), new Vector3(178, 71, 235), new Vector3(239, 55, 10), new Vector3(155, 255, 48), new Vector3(6, 209, 247), new Vector3(255, 174, 1) };

    public void SetDatas(UnitConfig config)
    {
        LvlText.text = config.Level.ToString();
        NameText.text = GameController.Instance.UnitCtr.GetUnitNameStr(config);

        Vector3 colorV = BG_COLORS[(int)config.UnitColor] / 255f;
        ColorBg.color = new Color(colorV.x, colorV.y, colorV.z);
        //TODO 根据伙伴的颜色改变ColorBg的颜色 data.UnitColor
    }
}
