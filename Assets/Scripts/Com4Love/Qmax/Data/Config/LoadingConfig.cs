
namespace Com4Love.Qmax.Data.Config
{
    public class LoadingConfig
    {
        public int ID;
        public int MsgId;
        public int tielId;  //不知道干啥的
        public string Icon;

        public LoadingConfig(XMLInStream inStream)
        {
            inStream.Attribute("id", out ID)
                .Attribute("msgId", out MsgId)
            .Attribute("icon", out Icon);
        }
    }
}
