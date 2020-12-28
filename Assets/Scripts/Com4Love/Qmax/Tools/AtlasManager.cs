using Com4Love.TexturePacker;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Tools
{
    /// <summary>
    /// 對於SpriteSheet類型的貼圖做加載控制
    /// </summary>
    public class AtlasManager
    {
        private Dictionary<Atlas, Dictionary<string, Sprite>> dict;

        /// <summary>
        /// 引用計數
        /// </summary>
        private Dictionary<Atlas, int> refCountDict;

        /// <summary>
        /// Atlas -> TPSheet
        /// </summary>
        //private Dictionary<Atlas, TPSheet> tpSheetDict;

        /// <summary>
        /// 存放所有Atlas圖集的目錄路徑，以'/'结尾
        /// </summary>
        private string atlasDirPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="atlasDirPath">存放所有Atlas圖集的目錄路徑，相對於Resources的相對路徑</param>
        public AtlasManager(string atlasDirPath)
        {
            this.atlasDirPath =
                atlasDirPath[atlasDirPath.Length - 1] == '/' ?
                atlasDirPath :
                atlasDirPath + "/";
            dict = new Dictionary<Atlas, Dictionary<string, Sprite>>();
            refCountDict = new Dictionary<Atlas, int>();
        }

        /// <summary>
        /// 加載圖集
        /// </summary>
        /// <param name="atlas"></param>
        public void LoadAtlas(Atlas atlas)
        {
            if (dict.ContainsKey(atlas))
            {
                refCountDict[atlas]++;
                return;
            }

            Dictionary<string, Sprite> subdict = new Dictionary<string, Sprite>();
            dict.Add(atlas, subdict);
            refCountDict.Add(atlas, 1);
            Sprite[] sprites = Resources.LoadAll<Sprite>(atlasDirPath + atlas);
            
            //Q.Log("Sprites {0}, number = {1}", atlasDir + atlas, sprites.Length);
            for (int i = 0, n = sprites.Length; i < n; i++)
            {
                //Q.Log("{0}, {1}", i, sprites[i].name);
                if (subdict.ContainsKey(sprites[i].name))
                {
                    //Q.Log("------- {0}",  sprites[i].name);
                    continue;
                }
                subdict.Add(sprites[i].name, sprites[i]);
            }
        }

        /// <summary>
        /// 加載圖集
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="atlasName"></param>
        public void LoadAtlas(Atlas atlas, AtlasName atlasName)
        {
            // 注意：允許多次Load同一個Atlas，例如Title圖集拆分成2張圖集Title1.tpsheet & Title2.tpsheet，
            // 多次調用加載，將2張圖集同加載到一個Atlas中，但此時引用計數應該為1（將來卸載圖集時一次卸載），
            // 因此不能單純引用計數 +1然後return。

            Dictionary<string, Sprite> subdict = null;
            // 是否已經加載過
            if (dict.ContainsKey(atlas))
            {
                // 加載過則拿出來
                subdict = dict[atlas];
            }
            else
            {
                // 沒有加載過則新建
                subdict = new Dictionary<string, Sprite>();
                dict.Add(atlas, subdict);
                refCountDict.Add(atlas, 1);
            }

            Sprite[] sprites = Resources.LoadAll<Sprite>(atlasDirPath + atlasName);
            for (int i = 0, n = sprites.Length; i < n; i++)
            {
                if (subdict.ContainsKey(sprites[i].name))
                {
                    continue;
                }
                subdict.Add(sprites[i].name, sprites[i]);
            }
        }

        /// <summary>
        /// 添加一張已經加載的Atlas圖集
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="t2d"></param>
        /// <param name="tpssheetPath"></param>
        public void AddAtlas(Atlas atlas, Texture2D t2d = null)
        {
            if (dict.ContainsKey(atlas))
            {
                refCountDict[atlas]++;
                return;
            }

            LoadAtlas(atlas);
        }


        /// <summary>
        /// 卸載圖集
        /// </summary>
        /// <param name="atlas"></param>
        /// <returns>是否卸載成功</returns>
        public bool UnloadAtlas(Atlas atlas)
        {
            if (!dict.ContainsKey(atlas))
                return false;

            if (--refCountDict[atlas] > 0)
                return false;

            //Dictionary<string, Sprite> atlasDict = dict[atlas];
            //把手動卸載材質的邏輯去掉，只移除引用。由unity自身的管理器去卸載
            //foreach (KeyValuePair<string, Sprite> pair in atlasDict)
            //{
            //    Resources.UnloadAsset(pair.Value.texture);
            //}
            dict.Remove(atlas);
            refCountDict.Remove(atlas);
            return true;
        }

        /// <summary>
        /// 是否已經加載這張圖集
        /// </summary>
        /// <param name="atlas"></param>
        /// <returns></returns>
        public bool ContainsAtlas(Atlas atlas)
        {
            return dict.ContainsKey(atlas);
        }


        /// <summary>
        /// 從某張圖集中獲取一個Sprite
        /// </summary>
        /// <param name="atlas"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite GetSprite(Atlas atlas, string spriteName)
        {
            if (!dict.ContainsKey(atlas) || !dict[atlas].ContainsKey(spriteName))
                return null;

            return dict[atlas][spriteName];
        }


        public void LogStatus()
        {
            if(dict != null)
            {
                Q.Log("AtlasManager count={0}", dict.Count);
                foreach (var pair in dict)
                {
                    Q.Log("tpsheet={0}, ref={1}", pair.Key.ToString(), this.refCountDict[pair.Key]);
                }
            }
        }

        public Sprite GetSpriteInUIComponent(RewardType type)
        {
            if ( !dict.ContainsKey(Atlas.UIComponent))
            {
                LoadAtlas(Atlas.UIComponent);
            }

            string texName = "";
            switch (type)
            {
                case RewardType.Key:
                    texName = "icon8";
                    //itemName = "剪刀";
                    break;
                case RewardType.UpgradeA:
                    texName = "UpgradeA_b";
                    //itemName = "橘子";
                    break;
                case RewardType.UpgradeB:
                    texName = "UpgradeB_b";
                    //itemName = "桃子";
                    break;
                case RewardType.Gem:
                    texName = "Icon2";
                    //itemName = "鑽石";
                    break;
                case RewardType.MaxEnergy:
                    texName = "EnergyCache";
                    //itemName = "體力上限";
                    break;
                case RewardType.Energy:
                    texName = "Icon1";
                    //itemName = "體力";
                    break;
                case RewardType.Coin:
                    texName = "coin_big";
                    //itemName = "金幣";
                    break;
            }

            if (dict[Atlas.UIComponent].ContainsKey(texName))
                return dict[Atlas.UIComponent][texName];

            return null;
        }
    }
}
