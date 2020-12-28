using UnityEngine;
using System.Collections;
using Com4Love.Qmax;


public class UIBattleTipsButton : MonoBehaviour {

    public BattleTips TipsKind;

    UIBattleTipsDialog TipsUI;

    UIButtonBehaviour ButtonBeh;

    void Awake()
    {
        if (ButtonBeh == null)
            ButtonBeh = GetComponent<UIButtonBehaviour>();

        ButtonBeh.onClick += OnClickButton;
    }
    // Use this for initialization
    void Start () {
	
	}

    void OnClickButton(UIButtonBehaviour button)
    {
        if (TipsUI == null)
            TipsUI = Instantiate(Resources.Load<GameObject>("Prefabs/Ui/UIBattle/UIBattleButtonTip")).transform.GetComponent<UIBattleTipsDialog>();

        GameController gamectr = GameController.Instance;
        gamectr.ButtonTipsCtr.LastTips = TipsUI;

        string info = "";
        Vector3 tipsPos = Vector3.zero;
        Vector3 arrowspos = Vector3.zero;
        //Vector2 bgSize = Vector2.zero;
        switch (TipsKind)
        {
            case BattleTips.Moves:
                tipsPos = new Vector3(-293,-144);
                arrowspos = new Vector3(-50,0);
                info = Utils.GetTextByID(547);
                break;
            case BattleTips.Goal:
                tipsPos = new Vector3(138, -130);
                info = Utils.GetTextByID(545);
                break;
            case BattleTips.Score:
                tipsPos = new Vector3(-120, -130);
                arrowspos = new Vector3(-31, 0);
                info = Utils.GetTextByID(546);
                break;
            case BattleTips.Star1:
                tipsPos = new Vector3(-311, -192);
                //arrowspos = new Vector3(-31,0);
                info = gamectr.ButtonTipsCtr.StarScore[0].ToString();
                break;
            case BattleTips.Star2:
                tipsPos = new Vector3(-253, -155);
                //arrowspos = new Vector3(-75, 0);
                info = gamectr.ButtonTipsCtr.StarScore[1].ToString();
                break;
            case BattleTips.Star3:
                tipsPos = new Vector3(-242, -86);
                //arrowspos = new Vector3(-77, 0);
                info = gamectr.ButtonTipsCtr.StarScore[2].ToString();
                break;
            case BattleTips.StarBG:
                tipsPos = new Vector3(-265, -192);
                info = Utils.GetTextByID(548);
                break;

        }
        TipsUI.transform.SetParent(gamectr.ButtonTipsCtr.TipsParent);
        TipsUI.transform.localPosition = transform.localPosition;
        TipsUI.transform.localScale = Vector3.one;

        TipsUI.transform.SetAsLastSibling();

        TipsUI.SetData(info, tipsPos, arrowspos);

    }
}
