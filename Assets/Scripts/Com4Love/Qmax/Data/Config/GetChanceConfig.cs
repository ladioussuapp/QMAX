
namespace Com4Love.Qmax.Data.Config
{
    public class GetChanceConfig
    {
        public int ID;
        public string Icon;
        public string Sound;
        public int GiftType;
        public int title = -1;
        public int info = -1;

        public GetChanceConfig(XMLInStream inStream)
        {
            string getUi = "";
            inStream.Attribute("id", out ID).
            Attribute("giftType", out GiftType).
            Attribute("icon", out Icon).
            Attribute("sound", out Sound).
            Attribute("getUi", out getUi);

            if (!string.IsNullOrEmpty(getUi))
            {
                string[] ui = getUi.Split(',');

                if (ui.Length >= 2)
                {
                    title = int.Parse(ui[0]);
                    info = int.Parse(ui[1]);
                }
            }
        }

        public bool IsHaveDialog()
        {
            if (title != -1 && info != -1)
            {
                return true;
            }

            return false;
        }
    }
}
