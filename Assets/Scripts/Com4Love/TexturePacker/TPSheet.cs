using System.Collections.Generic;

namespace Com4Love.TexturePacker
{
    public class TPSprite
    {
        public string Name;
        public int Width;
        public int Height;
        public int Left;
        public int Bottom;
        public float PivotX;
        public float PivotY;
    }

    public class TPSheet
    {
        public string Name;
        public string Path;
        public List<TPSprite> sprites;
    }
}
