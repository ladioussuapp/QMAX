using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com4Love.Qmax;

public class GoodsItemInfoBehaviour : MonoBehaviour
{
    public enum EType : int
    {
        Left,
        Center,
        Right
    }

    [System.Serializable]
    public struct InfoOffset
    {
        public Vector3 DialogOff;
        public Vector3 ArrowOff;
        public Vector3 ArrowScale;
        public Vector3 ArrowRotation;
    }

    public GameObject Board;
    public GameObject UseButton;
    public GameObject InfoText;
    public GameObject TitleText;
    public GameObject Arrow;
   

    public InfoOffset[] Offsets;

    private GoodsConfig goodsConfig;
    private EType type;
    private GameController gameCtr;

    void Awake()
    {
        gameCtr = GameController.Instance;
    }
    // Use this for initialization
    void Start()
    {
        UseButton.GetComponent<Button>().onClick.AddListener(this.OnUseItem);
    }

    public EType Type
    {
        set
        {
            type = value;
            this.RefreshOffset();
        }
        get
        {
            return type;
        }
    }

    private void RefreshOffset()
    {
        switch (this.type)
        {
            case EType.Left:
                {
                    Board.transform.localPosition =  Offsets[0].DialogOff;
                    Arrow.transform.localPosition = Offsets[0].ArrowOff;

                    Quaternion ro = Arrow.transform.localRotation;
                    ro.eulerAngles = Offsets[0].ArrowRotation;
                    Arrow.transform.localRotation = ro;
                }
                break;
            case EType.Center:
                {
                    Board.transform.localPosition =  Offsets[1].DialogOff;
                    Arrow.transform.localPosition = Offsets[1].ArrowOff;

                    Quaternion ro = Arrow.transform.localRotation;
                    ro.eulerAngles = Offsets[1].ArrowRotation;
                    Arrow.transform.localRotation = ro;
                }
                break;
            case EType.Right:
                {
                    Board.transform.localPosition = Offsets[2].DialogOff;
                    Arrow.transform.localPosition = Offsets[2].ArrowOff;

                    Quaternion ro = Arrow.transform.localRotation;
                    ro.eulerAngles = Offsets[2].ArrowRotation;
                    Arrow.transform.localRotation = ro;
                }
                break;
            default:
                break;
        }
    }

    //private int _num;
    public int Num
    {
        set
        {
            //_num = value;
        }
    }

    public GoodsConfig GoodsConfig
    {
        set
        {
            goodsConfig = value;
            UseButton.SetActive(goodsConfig.Usable);

            TitleText.GetComponent<Text>().text = gameCtr.GoodsCtr.GetGoodsNameStr(goodsConfig);
            InfoText.GetComponent<Text>().text = gameCtr.GoodsCtr.GetGoodsContentStr(goodsConfig);
        }
        get
        {
            return goodsConfig;
        }
    }

    private void OnUseItem()
    {

        if(goodsConfig.UID == (int)PropType.EnergyB && gameCtr.PlayerCtr.PlayerData.energyMax >= GameController.Instance.Model.GameSystemConfig.itemMaxEnergy)
        {
            return;
        }
        else
        {     
            GameController.Instance.Client.UseGoods(goodsConfig.UID, 1);
            GameController.Instance.ModelEventSystem.OnGoodsItemInfoClear();
        }

    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
