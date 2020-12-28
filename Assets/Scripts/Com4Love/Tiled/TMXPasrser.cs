using Com4Love.Qmax;
using System;
using System.Collections.Generic;

namespace Com4Love.Tiled
{
    public class TMXPasrser
    {
        static public TMXMap Parse(string json)
        {
            return Parse(SimpleJSON.JSONNode.Parse(json));
        }

        static public TMXMap Parse(SimpleJSON.JSONNode jsonNode)
        {
            TMXMap map = new TMXMap();

            map.Height = jsonNode["height"].AsInt;
            map.Width = jsonNode["width"].AsInt;
            map.NextOjbectID = jsonNode["nextobjectid"].AsInt;
            map.TileHeight = jsonNode["tileheight"].AsInt;
            map.TileWidth = jsonNode["tilewidth"].AsInt;
            map.Version = jsonNode["version"].AsFloat;
            map.Properties = ParseProperties(jsonNode["properties"]);
            switch (jsonNode["orientation"].Value)
            {
                case "orthogonal":
                    map.Orientation = TMXOrientation.Orthogonal;
                    break;
                case "iosmetric":
                    map.Orientation = TMXOrientation.Isometric;
                    break;
            }
            switch (jsonNode["renderorder"].Value)
            {
                case "right-down":
                    map.renderorder = RenderOrder.RightDown;
                    break;
                case "right-up":
                    map.renderorder = RenderOrder.RightUp;
                    break;
                case "left-down":
                    map.renderorder = RenderOrder.LeftDown;
                    break;
                case "left-up":
                    map.renderorder = RenderOrder.LeftUp;
                    break;
            }
            //layers
            SimpleJSON.JSONNode subNode = jsonNode["layers"];
            map.Layers = new List<TMXLayer>();
            for (int i = 0, n = subNode.Count; i < n; i++)
            {
                map.Layers.Add(ParseLayer(subNode[i]));
            }            
            //tilesset
            subNode = jsonNode["tilesets"];
            map.TileSets = new List<TMXTileSet>();
            for (int i = 0, n = subNode.Count; i < n; i++)
            {
                TMXTileSet tileset = ParseTileSet(subNode[i]);
                if (tileset != null)
                    map.TileSets.Add(tileset);
            } 

            return map;
        }


        static public TMXLayer ParseLayer(SimpleJSON.JSONNode jsonNode)
        {
            TMXLayer ret = new TMXLayer();
            switch (jsonNode["type"])
            {
                case "tilelayer":
                    ret.Type = TMXLayerType.TileLayer;
                    break;
                case "imageLayer":
                    ret.Type = TMXLayerType.ImageLayer;
                    break;
                case "objectgroup":
                    ret.Type = TMXLayerType.ObjectGroup;                    
                    break;
            }

            //TODO 暫時只處理一種類型
            if (ret.Type != TMXLayerType.TileLayer)
                return ret;

            ret.Height = jsonNode["height"].AsInt;
            ret.Width = jsonNode["width"].AsInt;
            ret.Name = jsonNode["name"].Value;
            ret.X = jsonNode["x"].AsInt;
            ret.Y = jsonNode["y"].AsInt;
            ret.Opacity = jsonNode["opacity"].AsFloat;
            ret.Visible = jsonNode["visible"].AsBool;
            ret.Properties = ParseProperties(jsonNode["properties"]);
            //data
            ret.Data = new List<int>();
            SimpleJSON.JSONNode subNode = jsonNode["data"];
            for (int i = 0, n = subNode.Count; i < n; i++)
            {
                ret.Data.Add(subNode[i].AsInt);
            }                
            return ret;
        }

        static public TMXTileSet ParseTileSet(SimpleJSON.JSONNode jsonNode)
        {
            TMXTileSet ret = new TMXTileSet();

            ret.FirstGID = jsonNode["firstgid"].AsInt;
            ret.Image = jsonNode["image"].Value;
            ret.ImageWidth = jsonNode["imagewidth"].AsInt;
            ret.ImageHeight = jsonNode["imageheight"].AsInt;
            ret.Margin = jsonNode["margin"].AsInt;
            ret.Name = jsonNode["name"].Value;
            ret.Spacing = jsonNode["spacing"].AsInt;
            ret.TileHeight = jsonNode["tileheight"].AsInt;
            ret.TileWidth = jsonNode["tilewidth"].AsInt;
            ret.Properties = ParseProperties(jsonNode["properties"]);
            //tileproperties
            ret.TileProperties = new Dictionary<string, Dictionary<string, string>>();
            SimpleJSON.JSONClass tileProperties = jsonNode["tileproperties"].AsObject;
            if (tileProperties != null)
            {
                foreach (KeyValuePair<string, SimpleJSON.JSONNode> N in tileProperties)
                {
                    ret.TileProperties.Add(N.Key, new Dictionary<string, string>());
                    SimpleJSON.JSONClass child = N.Value.AsObject;
                    if (child == null)
                        continue;

                    foreach (KeyValuePair<string, SimpleJSON.JSONNode> N2 in child)
                    {
                        ret.TileProperties[N.Key].Add(N2.Key, N2.Value);
                    }
                }
            }

            return ret;
        }

        static public Dictionary<string, string> ParseProperties(SimpleJSON.JSONNode jsonNode)
        {            
            SimpleJSON.JSONClass jsonClass = jsonNode.AsObject;
            if (jsonClass == null)
                return null;

            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<string, SimpleJSON.JSONNode> N in jsonClass)
            {
                ret.Add(N.Key, N.Value);
            }
            return ret;
        }

    }
}
