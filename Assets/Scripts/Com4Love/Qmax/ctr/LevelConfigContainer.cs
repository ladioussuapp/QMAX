using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Tiled;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    public class LevelConfigContainer
    {

        /// <summary>
        /// 關卡配置，level id -> TMXMap
        /// 這個表由於全部初始化太慢了，所以改為在需要某個關卡的時候，才去解析並放入這個字典中
        /// </summary>
        private Dictionary<string, LevelConfig> levelConfigs;


        /// <summary>
        /// 存放所有關卡配置的AssetBundle，只有在需要關卡配置時，才從裡面拿
        /// </summary>
        protected AssetBundle lvConfAssetBundle = null;


        public LevelConfigContainer(AssetBundle lvConfAssetBundle)
        {
            levelConfigs = new Dictionary<string, LevelConfig>();
            this.lvConfAssetBundle = lvConfAssetBundle;
        }


        public LevelConfig GetBattleLevel(string levelName)
        {
            if (levelConfigs.ContainsKey(levelName))
                return levelConfigs[levelName];

            string assetName = string.Format("Assets/ExternalRes/Config/Levels/{0}.json", levelName).ToLower();
            TextAsset textAsset = null;
#if UNITY_EDITOR
            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
                textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(assetName);
            }
            else
            {
                Q.Assert(lvConfAssetBundle != null, "QmaxModel:GetBattleLevel Assert 1");
                textAsset = lvConfAssetBundle.LoadAsset<TextAsset>(assetName);
            }
#else
            Q.Assert(lvConfAssetBundle != null, "QmaxModel:GetBattleLevel Assert 1");
            textAsset = lvConfAssetBundle.LoadAsset<TextAsset>(assetName);
#endif
            if (textAsset == null)
                return null;

            TMXMap map = TMXPasrser.Parse(textAsset.text);
            if (map == null)
                return null;

            Dictionary<int, string> objectIdDict = new Dictionary<int, string>();
            foreach (TMXTileSet tileset in map.TileSets)
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> N in tileset.TileProperties)
                {
                    int tileID = Convert.ToInt32(N.Key) + tileset.FirstGID;

                    Q.Assert(N.Value.ContainsKey("objectID"), "Can not find tile property 'objectID'. Level={0}, tileID={1}", levelName, tileID);
                    if (N.Value.ContainsKey("objectID") && !objectIdDict.ContainsKey(tileID))
                        objectIdDict.Add(tileID, N.Value["objectID"]);
                }
            }

            LevelConfig conf = new LevelConfig();

            conf.NumCol = map.Width;
            conf.NumRow = map.Height;
            conf.ElementLayer = new int[conf.NumCol, conf.NumRow];
            conf.CollectLayer = new int[conf.NumCol, conf.NumRow];
            conf.CoveringLayer = new int[conf.NumCol, conf.NumRow];
            conf.ObstacleLayer = new int[conf.NumCol, conf.NumRow];
            conf.SeperatorHLayer = new int[conf.NumCol, conf.NumRow];
            conf.SeperatorVLayer = new int[conf.NumCol, conf.NumRow];
            conf.BottomLayer = new int[conf.NumCol, conf.NumRow];
            Dictionary<string, int[,]> layerNameDict = new Dictionary<string, int[,]>();

            layerNameDict.Add("Element", conf.ElementLayer);
            layerNameDict.Add("Collect", conf.CollectLayer);
            layerNameDict.Add("Cover", conf.CoveringLayer);
            layerNameDict.Add("Obstacle", conf.ObstacleLayer);
            layerNameDict.Add("HSeperator", conf.SeperatorHLayer);
            layerNameDict.Add("VSeperator", conf.SeperatorVLayer);
            layerNameDict.Add("BottomLayer", conf.BottomLayer);

            //Q.Assert(map.Layers.Count == 6, "There lay number is wrong. It should be {0}, but {1}.", 6, map.Layers.Count);

            foreach (TMXLayer layer in map.Layers)
            {
                int[,] targetLayer = layerNameDict[layer.Name];
                Q.Assert(targetLayer != null, "layer.Name={0}", layer.Name);

                for (int i2 = 0, n2 = layer.Data.Count; i2 < n2; i2++)
                {
                    int row = Mathf.FloorToInt(i2 / layer.Width);
                    int col = i2 % layer.Width;
                    int tileID = layer.Data[i2];
                    if (tileID == 0)
                    {
                        targetLayer[row, col] = 0;
                    }
                    else
                    {
                        Q.Assert(objectIdDict.ContainsKey(tileID),
                            "Assert fail. p={0}, type={1}, tileID={2}",
                            new Position(row, col),
                            layer.Name,
                            tileID);
                        string objectID = objectIdDict[tileID];
                        targetLayer[row, col] = int.Parse(objectID);
                    }
                }
            }
            return conf;
        }
    }
}
