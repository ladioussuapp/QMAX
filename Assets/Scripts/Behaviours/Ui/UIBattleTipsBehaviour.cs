using UnityEngine;
using System.Collections;
using Com4Love.Qmax.TileBehaviour;
using Com4Love.Qmax;
using UnityEngine.UI;

public class UIBattleTipsBehaviour : MonoBehaviour
{

    public enum AnimType
    {
        GreenBomb,
        BlackBomb,
        YellowCover
    }

    public Toggle ShowSelected;
    public Text Tips;
    public Text Title;
    public Text Label;

    private Animator controller;
    private CanvasGroup textCanvasGroup;
    private bool autoSave = true;
    void Awake()
    {
        controller = this.GetComponentInChildren<Animator>();
        ShowSelected.onValueChanged.AddListener(this.OnUserSelected);
        textCanvasGroup = Tips.GetComponent<CanvasGroup>();
        Tips.text = Utils.GetTextByID(1733);
        Label.text = Utils.GetTextByID(1732);
    }

    // Use this for initialization
    void Start()
    {        
        this.PlayAnim(mode);
        //默認選中
    }

    void OnDestroy()
    {
        OnUserSelected(autoSave);
    }

    void OnUserSelected(bool value)
    {
        autoSave = value;
        PlayerPrefsTools.SetBoolValue(mode.ToString(), value, true);
        GameController.Instance.ModelEventSystem.OnBattleTipsCheck(value,mode);
    }

    void PlayAnim(AnimType type = AnimType.GreenBomb)
    {
        switch (type)
        {
            case AnimType.GreenBomb:
                {
                    Title.text = Utils.GetTextByID(1711);
                    controller.SetTrigger("GreenBomb");
                }
                break;
            case AnimType.BlackBomb:
                {
                    Title.text = Utils.GetTextByID(1722);
                    controller.SetTrigger("BlackBomb");
                }
                break;
            case AnimType.YellowCover:
                {
                    Title.text = Utils.GetTextByID(1713);
                    controller.SetTrigger("YellowCover");
                }
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ChangeTextAlpha();
    }
    bool IsAlphaAdd = true;
    void ChangeTextAlpha()
    {
        if (textCanvasGroup != null)
        {

            float speed = 1.1f * Time.deltaTime;

            if (textCanvasGroup.alpha >= 1)
            {
                IsAlphaAdd = false;
            }
            else if (textCanvasGroup.alpha <= 0)
            {
                IsAlphaAdd = true;
            }

            speed = IsAlphaAdd ? speed : -speed;

            textCanvasGroup.alpha += speed;

        }
    }

    private static UIBattleTipsBehaviour.AnimType mode = AnimType.GreenBomb;
    public static void Show(UIBattleTipsBehaviour.AnimType _mode)
    {
        mode = _mode;
        if(!PlayerPrefsTools.GetBoolValue(mode.ToString(),true))
        {
            GameController.Instance.Popup.Open(PopupID.UIBattleTips, null, true, true,0.51f);
        }        
    }
}
