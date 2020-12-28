
namespace Com4Love.Qmax.Data.Config
{
    class LanguageConfig
    {
        public int ID;
        public int Statuscode;
        public string CNS;
        public string Text;

        public LanguageConfig(XMLInStream inStream)
        {
            inStream.Attribute("id", out ID)
                .Attribute("chs", out CNS)
                .Attribute("statuscode", out Statuscode)
                .Attribute(PackageConfig.LANGUAGE, out Text);
        }
    }
}
