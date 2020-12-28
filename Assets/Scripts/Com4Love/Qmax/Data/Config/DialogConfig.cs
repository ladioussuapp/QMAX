/********************************************************************************
** auth： johnsonybq
** date： 2015/9/8 星期二 15:14:08
** FileName：DialogConfig
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/



public class DialogConfig
{
    /// <summary>
    /// 对话编号
    /// </summary>
    public int UID;
    /// <summary>
    /// 顺序编号
    /// </summary>
    public int ID;
    /// <summary>
    /// 触发参数0
    /// </summary>
    public int StartArg0;
    /// <summary>
    /// 触发参数1
    /// </summary>
    public int StartArg1;
    /// <summary>
    /// 触发参数2
    /// </summary>
    public int StartArt2;
    /// <summary>
    /// 对白位置
    /// </summary>
    public int Location;
    /// <summary>
    /// 图片路径1
    /// </summary>
    public int UnitID1;
    /// <summary>
    /// 图片路径2
    /// </summary>
    public int UnitID2;
    /// <summary>
    /// 对话角色
    /// </summary>
    //public int UnitID;
    /// <summary>
    /// 講時特殊操作
    /// </summary>
    public int DialogAction;
    /// <summary>
    /// 撥放語音檔案素材
    /// </summary>
    public string Voice;
    /// <summary>
    /// 語言包編號
    /// </summary>
    public int LanguageID;
    /// <summary>
    /// 對白持續秒數
    /// </summary>
    public int DialogTime;
    /// <summary>
    /// 對話框位置點
    /// </summary>
    public string Coordinate;
    /// <summary>
    /// 通关贴图
    /// </summary>
    public string FinishImage;

    public FingerPoint finger;

    public bool ShowTips;

    public bool ShowFinger;
    public DialogConfig(XMLInStream inStream)
    {
        inStream.Attribute("uid", out UID)
            .Attribute("Id", out ID)
            .Attribute("startArg0", out StartArg0)
            .Attribute("startArg1", out StartArg1)
            .Attribute("location", out Location)
            .Attribute("unitID1", out UnitID1)
            .Attribute("unitID2", out UnitID2)
            .Attribute("action", out DialogAction)
            .Attribute("voice", out Voice)
            .Attribute("languageID", out LanguageID)
            .Attribute("time", out DialogTime)
            .Attribute("coordinate", out Coordinate)
            .Attribute("image", out FinishImage);
    }    
}
