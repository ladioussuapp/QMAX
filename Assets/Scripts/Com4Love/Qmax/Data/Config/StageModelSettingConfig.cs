
namespace Com4Love.Qmax.Data.Config
{
    public class StageModelSettingConfig
    {
        //是ID也是索引
        public int MapId;
        public string MapModel;
        public string[] levelRange;
        public string SkyTexture;

        public StageModelSettingConfig(XMLInStream inStream)
        {
            string levelRangeStr;

            inStream.Attribute("mapId", out MapId)
                .Attribute("mapModel", out MapModel)
                .Attribute("skyTexture", out SkyTexture)
                .Attribute("levelRange", out levelRangeStr);

            levelRange = levelRangeStr.Split(',');
        }
    }
}
