using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax;

public class GuideDialogItemBehaviour : MonoBehaviour
{

    public Image DialogBg;
    public Text DialogContent;
    public Image UnitBody;
    public GameObject Dialog;
    public bool LeftOrRight;
    //public Image UnitFoundation;
    public Image Mask;
    public Image ArrowImage;
    private int m_unitId;
    private string m_content;
    private float arrowMaxH = 404;

    private Animator dialogAnim;
    private Animator bodyAnim;


    public void setData(int unitId, string content)
    {
        m_unitId = unitId;
        m_content = content;
        LeanTween.cancel(ArrowImage.gameObject);
        changeData();
    }

    void changeData()
    {
        DialogBg.gameObject.SetActive(false);
        ArrowImage.gameObject.SetActive(false);
        DialogContent.gameObject.SetActive(false);
        Mask.gameObject.SetActive(false);
        if (m_unitId == 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            Mask.gameObject.SetActive(true);
            GameController gameCtr = GameController.Instance;
            string unitName = "";
            if (gameCtr.Model.UnitConfigs.ContainsKey(m_unitId))
            {
                UnitConfig unitConfig = gameCtr.Model.UnitConfigs[m_unitId];
                UnitBody.sprite = GameController.Instance.QMaxAssetsFactory.CreteDialogUnitSprite(unitConfig, new Vector2(.5f, .5f));
                //UnitBody.SetNativeSize();
                unitName = GameController.Instance.UnitCtr.GetUnitNameStr(unitConfig);
                //string[] btArr = { "0", "UIDialog_005", "UIDialog_003", "UIDialog_002", "UIDialog_006", "UIDialog_004" };
                //int index = (int)unitConfig.UnitColor;
                //string foundUrl = btArr[index];
                //UnitFoundation.sprite = GameController.Instance.AtlasManager.GetSprite(Atlas.UIDialog, foundUrl);
                //UnitFoundation.SetNativeSize();
            }
            if (m_content != null)
            {
                Mask.gameObject.SetActive(false);
                DialogBg.gameObject.SetActive(true);
                ArrowImage.gameObject.SetActive(true);
                DialogContent.gameObject.SetActive(true);
                DialogContent.supportRichText = true;
                //string styleName = string.Format("<color=green>{0}：</color>", unitName)
                string styleName = string.Format("{0}：", unitName);
                DialogContent.text = styleName + m_content;
                //DialogBg.rectTransform.sizeDelta = new Vector2(DialogBg.rectTransform.sizeDelta.x, DialogContent.preferredHeight + 100);
                //if (arrowMaxH == 0)
                //{
                //arrowMaxH = ArrowImage.rectTransform.localPosition.y - 10;
                // }

                arrowAnimation();

//                 dialogAnim = Dialog.GetComponent<Animator>();
//                 bodyAnim = UnitBody.GetComponent<Animator>();

                if (LeftOrRight)
                {
                    //dialogAnim.SetTrigger("DialogLeft"); //left
                    //bodyAnim.SetTrigger("BodyLeft"); 
                }
                else
                {
//                     dialogAnim.SetTrigger("DialogRight"); //right
//                     bodyAnim.SetTrigger("BodyRight");
                }
                    

            }
        }
    }


    public void OnDestroy()
    {
        if (ArrowImage != null)
            LeanTween.cancel(ArrowImage.gameObject);
    }

    void arrowAnimation()
    {
        LeanTween.moveLocalY(ArrowImage.gameObject, arrowMaxH, 0.5f).setOnComplete(delegate ()
        {
            arrowAnimation2();
        });
    }

    void arrowAnimation2()
    {
        LeanTween.moveLocalY(ArrowImage.gameObject, arrowMaxH + 10, 0.2f).setOnComplete(delegate ()
        {
            arrowAnimation();
        });
    }
}
