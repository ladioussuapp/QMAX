using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com4Love.Tiled
{
    public class TMXTileSet
    {
        public string Name;
        public int FirstGID;
        public string Image;
        public int ImageWidth, ImageHeight;
        public int Margin;
        public int Spacing;
        public Dictionary<string, string> Properties;
        public int TileWidth, TileHeight;
        public Dictionary<string, Dictionary<string, string>> TileProperties;
    }
}
