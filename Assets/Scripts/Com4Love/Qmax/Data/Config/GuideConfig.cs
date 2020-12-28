/********************************************************************************
** auth： johnsonybq
** date： 2015/9/8 星期二 15:37:07
** FileName：GuideConfig
** desc： 尚未编写描述
** Ver.:  V1.0.0
*********************************************************************************/

using System.Collections.Generic;

public class GuideConfig
{
    public int UID;

    public string GuideName;

    public string NodeName;

    public bool IsSave;

    public int[] DialogID;

    public List<int> UserData;

    public string SaveName;

    public int IndexNum;

    /// <summary>
    /// 教学步骤ID
    /// </summary>
    public int GuideID;

    public List<Dictionary<int, DialogConfig>> StepDialogConfigs;

    public GuideConfig(XMLInStream inStream, Dictionary<string, DialogConfig> dialogConfigs)
    {
        int isSave = 0;
        string userdata;
        string dialogid;
        inStream.Attribute("uid", out UID)
           .Attribute("guideName", out GuideName)
           .Attribute("guideId", out GuideID)
           .Attribute("nodeName", out NodeName)
           .Attribute("issave", out isSave)
           .Attribute("dialogId", out dialogid)
           .Attribute("userData", out userdata);

        if (isSave == 0)
        {
            IsSave = false;
        }
        else if(isSave == 1)
        {
            IsSave = true;
        }

        ///默认选择第一个
        IndexNum = 0;

        UserData = new List<int>();

        string[] userdatalist = userdata.Split(',');

        for (int i = 0; i < userdatalist.Length; i++)
        {
            int data = 0;
            int.TryParse(userdatalist[i], out data);

            if(data != 0)
                UserData.Add(data);
        }

        string[] dialogidlist = dialogid.Split(',');
        DialogID = new int[dialogidlist.Length];

        for (int i = 0; i < dialogidlist.Length; i++)
        {
            int data = 0;
            int.TryParse(dialogidlist[i], out data);

            DialogID[i] = data;
        }

        StepDialogConfigs = new List<Dictionary<int, DialogConfig>>();
        for (int i = 0; i < dialogidlist.Length; i++)
        {
            Dictionary <int, DialogConfig> con = new Dictionary<int, DialogConfig>();

            foreach (DialogConfig item in dialogConfigs.Values)
            {
                if (item.UID == DialogID[i])
                {
                    con.Add(item.ID, item);
                }
            }

            StepDialogConfigs.Add(con);
        }
        

    }

    /// <summary>
    /// 返回-1是表示没有
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public int HasData(int state)
    {
        int indexNum = -1;
        for (int i = 0; i < UserData.Count; i++)
        {
            if (UserData[i] == state)
            {
                indexNum = i;
                break;
            }
        }
        return indexNum;
    }
}