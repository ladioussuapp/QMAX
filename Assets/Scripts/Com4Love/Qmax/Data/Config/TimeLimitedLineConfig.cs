
namespace Com4Love.Qmax.Data.Config
{
    public class TimeLimitedLineConfig
    {
        public int ID;
        public int Number;

        public TimeLimitedLineConfig(XMLInStream inStream)
        {
            inStream.Attribute("id", out ID)
                .Attribute("number", out Number);
        }
    }
}
