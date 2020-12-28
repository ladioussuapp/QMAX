using System.Collections.Generic;

namespace Com4Love.Tiled
{
    public class TMXLayer
    {
        public List<int> Data;
        public int Height;
        public int Width;
        public string Name;
        public float Opacity = 0.0f;
        public TMXLayerType Type;
        public bool Visible;
        public float X, Y;
        public Dictionary<string, string> Properties;
    }
}
