
namespace Com4Love.Qmax.Ctr
{
    //這個應該算做是個統計用的util類與DoSDK這種東西類似，不應該是遊戲的控制類。
    //可以考慮增加兩個接口 彈出窗口 與 跳轉場景。在gameCtr中監聽窗口的彈出與場景切換，然後調用此工具的接口作統計用
    public class SendHttpMessageCtr
    {

        private static SendHttpMessageCtr instance;



        long ActorID
        {
            get
            {
                if (GameController.Instance.Model.LoginData != null)
                {
                    return GameController.Instance.Model.LoginData.actorId;
                }
                return 0;
            }

        }

#if UNITY_EDITOR
        /// <summary>
        /// 編輯器用內網地址///
        /// </summary>
        const string URL = "http://192.168.103.11:8080/bdata/showWindowlog";
#else
        /// 其他環境用北京服務器地址///
        const string URL = "http://203.195.243.221:8080/bdata/showWindowlog";
#endif
        public static SendHttpMessageCtr Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SendHttpMessageCtr();
                }

                return instance;
            }

        }

        public void SendUIMessage(string uiName)
        {
            var channelID = GameController.Instance.Model.SdkData.channalID;
            string check = Utils.GetMd5Hash(ActorID + channelID);
            object data = new { actorId = ActorID, channelId = channelID, uiName = uiName, token = check };
            GameController.Instance.Client.SendMessageNoRe(URL, data);
        }




    }
}

