using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

public class UIBattlePropButton : UIPropItem
{
    public InstanceOneEffect SelectedEffect;
    public InstanceOneEffect UsedEffect;

    // Use this for initialization
    public override void Start()
    {

        base.Start();
    }

    // Update is called once per frame
    void Update () {
	
	}

    public override void UpdateUI()
    {
        int num = GameController.Instance.PropCtr.GetPropNum(ItemPropType);

        if (AddImage != null)
            AddImage.gameObject.SetActive(num <= 0);

        if (NumText != null)
        {
            NumText.gameObject.SetActive(num>0);
            NumText.text = num > 99 ? "N" : string.Format("x{0}", num);
        }

        bool select = GameController.Instance.PropCtr.GetPropSelect(ItemPropType);

        if (SelectedEffect != null)
            SelectedEffect.gameObject.SetActive(select);
    }

    public void PlayUsedEff()
    {
        if (SelectedEffect != null)
            SelectedEffect.gameObject.SetActive(false);

        if (UsedEffect != null)
        {
            UsedEffect.gameObject.SetActive(true);
            UsedEffect.Create();
        }
    }
}
