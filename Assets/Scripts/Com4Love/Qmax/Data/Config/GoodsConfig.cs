using System.Collections.Generic;

public class GoodsConfig
{
    public enum EType : int
    {
        Recover = 1,
        Fight = 2,
        Other = 3
    }

    public enum SType : int
    {
        None = 0,
        RecoverPower = 1,
        UpgradePower = 2,
        PassiveProp = 3,
        ActiveProp = 4,
    }


    public int UID;

    public int SortId;

    public int GoodsStringId;

    public int GoodsContentId;

    public EType GoodsType;

    public SType SubType;

    public bool Usable;

    public string[] Arg0;

    public string Arg1;

    public string Arg2;

    public string GoodsIcon;

    public string EliminateAnim;

    public string GoodsTipsGif;


    public GoodsConfig(XMLInStream inStream)
    {
        int goodsType;
        int subType;
        int usable;
        string arg0Str;

        inStream.Attribute("id", out UID)
            .Attribute("sortId", out SortId)
            .Attribute("goodsStringId", out GoodsStringId)
            .Attribute("goodsContentId", out GoodsContentId)
            .Attribute("goodsType", out goodsType)
            .Attribute("subType", out subType)
            .Attribute("usable", out usable)
            .Attribute("arg0", out arg0Str)
            .Attribute("arg1", out Arg1)
            .Attribute("arg2", out Arg2)
            .Attribute("goodsIcon", out GoodsIcon)
            .Attribute("eliminateAnim", out EliminateAnim)
            .Attribute("goodsTipsGif", out GoodsTipsGif);

        Arg0 = arg0Str.Split(',');
        this.GoodsType = (EType)goodsType;
        this.SubType = (SType)subType;
        this.Usable = usable != 0 ? true: false;
    }
}

