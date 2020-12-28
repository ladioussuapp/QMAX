using System.Collections.Generic;

namespace Com4Love.Tiled
{
    public enum TMXOrientation
    {
        Orthogonal, Isometric
    }

    public enum RenderOrder
    {
        RightDown,
        RightUp,
        LeftDown,
        LeftUp
    }

    public enum TMXLayerType
    {
        TileLayer, ImageLayer, ObjectGroup
    }

    public class TMXMap
    {
        public List<TMXLayer> Layers;
        public int Width, Height;
        public int TileWidth, TileHeight;
        public int NextOjbectID;
        public RenderOrder renderorder;
        public TMXOrientation Orientation;
        public Dictionary<string, string> Properties;
        public List<TMXTileSet> TileSets;
        public float Version;
    }
}
