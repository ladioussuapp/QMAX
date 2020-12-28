using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using System;
using UnityEngine;

namespace Com4Love.Qmax.Tools
{
    /// <summary>
    /// 各種配置對應的資源可以直接在此取
    /// </summary>
    public class QMaxAssetsFactory
    {

        const string AUDIO_EFFECT_PATH = "Assets/ExternalRes/Audio/Effect/";
        const string AUDIO_BGM_PATH = "Assets/ExternalRes/Audio/Bgm/";

        public Transform CreatePrefab(string path)
        {
#if EFFECT_HIDE
            //當存在 EFFECT_HIDE 宏時，當需要創建的是特效目錄，則用空白的prefab代替
            if (path.IndexOf(GOPoolManager.RESOURCE_EFFECT_ROOT) > -1)
            {
                path = GOPoolManager.BLOCK_EFFECT_PREFAB;
            }
#endif

            GameObject prefab = (GameObject)Resources.Load(path);

            if (prefab == null)
            {
                return null;
            }

            GameObject go = GameObject.Instantiate(prefab);

            return go.transform;
        }

        public Sprite CreateSprite(string path, Vector2 pivot)
        {
            Texture2D tex;
            tex = (Texture2D)Resources.Load<Texture>(path);

            if (tex == null)
            {
                return null;
            }

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot);
        }

        /// <summary>
        /// 夥伴靜態圖片 路徑 Resources/Textures/UIUnit/
        /// </summary>
        public void CreateUnitSprite(UnitConfig uConfig, Vector2 pivot, Action<Sprite> callback)
        {
            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
#if UNITY_EDITOR
                string p = string.Format("Assets/ExternalRes/UnitImg1/{0}.png", uConfig.ResourceIcon);
                Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                Q.Assert(tex != null, "夥伴靜態圖缺少  unitId:{0} url:{1} ", uConfig.ID, p);


                if (callback != null)
                {
                    if (tex != null)
                        callback(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot));
                    else
                        callback(null);
                }
#endif
            }
            else
            {
                AssetBundleManager.LoadCallback OnLoaded =
                                delegate(AssetBundleManager.Code code, string abName, AssetBundle ab)
                                {
                                    Q.Assert(code == AssetBundleManager.Code.Success);
                                    string assetName = string.Format("Assets/ExternalRes/UnitImg1/{0}.png", uConfig.ResourceIcon);
                                    Texture2D tex = ab.LoadAsset<Texture2D>(assetName);
                                    Q.Assert(tex != null, "夥伴靜態圖缺少  unitId:{0} url:{1} ", uConfig.ID, assetName);
                                    if (callback != null)
                                    {
                                        if (tex != null)
                                            callback(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot));
                                        else
                                            callback(null);
                                    }
                                };
                GameController.Instance.AssetBundleMrg.LoadUnitImg(uConfig.ResourceIcon, OnLoaded);
            }
        }

        public void CreteSelectUnitSprite(UnitConfig uConfig, Vector2 pivot, Action<Sprite> callback)
        {
            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
#if UNITY_EDITOR
                string p = string.Format("Assets/ExternalRes/UnitImg1/{0}.png", uConfig.ResourceIcon);
                Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                Q.Assert(tex != null, "Unitid: " + uConfig.ID + " 素材缺少");
                if (callback != null)
                {
                    if (tex != null)
                        callback(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot));
                    else
                        callback(null);
                }
#endif
            }
            else
            {

                AssetBundleManager.LoadCallback OnLoaded =
                    delegate(AssetBundleManager.Code code, string abName, AssetBundle ab)
                    {
                        Q.Assert(code == AssetBundleManager.Code.Success);
                        string assetName = string.Format("Assets/ExternalRes/UnitImg1/{0}.png", uConfig.ResourceIcon);
                        Texture2D tex = ab.LoadAsset<Texture2D>(assetName);
                        Q.Assert(tex != null, "Unitid: " + uConfig.ID + " 素材缺少");
                        if (callback != null)
                        {
                            if (tex != null)
                                callback(Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot));
                            else
                                callback(null);
                        }
                    };
                GameController.Instance.AssetBundleMrg.LoadUnitImg(uConfig.ResourceIcon, OnLoaded);
            }
        }


        public Sprite CreteDialogUnitSprite(UnitConfig uConfig, Vector2 pivot)
        {
            Texture2D tex;

            tex = (Texture2D)Resources.Load<Texture>("Textures/UIUnitDialog/" + uConfig.DialogIcon);

            if (tex == null)
            {
                Q.Log("Unitid: " + uConfig.ID + " 素材缺少");
                return null;
            }

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot);
        }

        /// <summary>
        /// 創建prefab
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public Transform CreateUiPrefab(PopupID popupID)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/Ui/" + popupID);
            GameObject go = GameObject.Instantiate(prefab);

            return go.transform;
        }

        /// <summary>
        /// 選人背景素材缺少
        /// </summary>
        /// <param name="sConfig"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        public Sprite CreateUiSelectUnitBg(StageConfig sConfig, Vector2 pivot)
        {
            Texture2D tex;

            tex = (Texture2D)Resources.Load<Texture>("Textures/UISelectUnitBg/" + sConfig.Set);

            if (tex == null)
            {
                Q.Log("sConfig id: " + sConfig.ID + " 選人背景素材缺少");
                return null;
            }

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot);
        }



        /// <summary>
        /// 夥伴升級時會說一段話。    並且是隨機從N段話中取一條
        /// </summary>
        /// <param name="color"></param>
        /// <param name="advanceLvl">階段名  1-5 是1階  5-10是2階</param>
        /// <returns></returns>
        public void CreateRandomUnitUpgradeAudio(ColorType color, int advanceLvl, Action<AudioClip> callback)
        {
            Q.Assert(advanceLvl <= 3, "夥伴升級音效，階數最大是3階");

            string[] randoms = { "a", "b", "c" };
            int randomIndex = UnityEngine.Random.Range(0, randoms.Length);
            string path = string.Format(UIAudioConfig.UPGRADE_UNIT_LEVELUP_ROOT, (int)color, advanceLvl, randoms[randomIndex]);

            CreateEffectAudio(path, callback);
        }

        /// <summary>
        /// 音效
        /// </summary>
        /// <param name="clipName"></param>
        /// <returns></returns>
        public void CreateEffectAudio(string clipName, Action<AudioClip> callback)
        {
            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {                
#if UNITY_EDITOR
                string p = string.Format("Assets/ExternalRes/Audio/Effect/{0}.ogg", clipName);
                AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                Q.Assert(clip != null, "音樂缺少：" + p);
                if (callback != null)
                    callback(clip);
#endif
            }
            else
            {
                AssetBundleManager.LoadCallback OnLoaded =
                        delegate(AssetBundleManager.Code code, string abName, AssetBundle ab)
                        {
                            string assetPath = string.Format("{0}{1}.ogg", AUDIO_EFFECT_PATH, clipName);
                            AudioClip clip = ab.LoadAsset<AudioClip>(assetPath);

                            Q.Assert(clip != null, "音效缺少：" + assetPath);

                            if (callback != null)
                                callback(clip);
                        };
                GameController.Instance.AssetBundleMrg.LoadAudio(clipName, true, OnLoaded);
            }
        }

        /// <summary>
        /// 戰斗場景的音樂
        /// </summary>
        /// <param name="stage"></param>
        /// <returns></returns>
        public void CreateBattleBgmAudio(StageConfig stage, Action<AudioClip> callback)
        {
            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
#if UNITY_EDITOR
                string p = string.Format("Assets/ExternalRes/Audio/Bgm/{0}.mp3", stage.Gamemusic);
                AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                Q.Assert(clip != null, "音樂缺少：" + p);
                if (callback != null)
                    callback(clip);
#endif
            }
            else
            {
                AssetBundleManager.LoadCallback OnLoaded =
                                delegate(AssetBundleManager.Code code, string abName, AssetBundle ab)
                                {
                                    string assetPath = string.Format("{0}{1}.mp3", AUDIO_BGM_PATH, stage.Gamemusic);
                                    AudioClip clip = ab.LoadAsset<AudioClip>(assetPath);

                                    Q.Assert(clip != null, "音樂缺少：" + assetPath);

                                    if (callback != null)
                                        callback(clip);
                                };
                GameController.Instance.AssetBundleMrg.LoadAudio(stage.Gamemusic, false, OnLoaded);
            }
        }

        /// <summary>
        /// 戰斗場景的音樂
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void CreateBattleBgmAudio(string name, Action<AudioClip> callback)
        {
            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
#if UNITY_EDITOR
                string p = string.Format("Assets/ExternalRes/Audio/Bgm/{0}.mp3", name);
                AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                Q.Assert(clip != null, "音樂缺少：" + p);
                if (callback != null)
                    callback(clip);
#endif
            }
            else
            {
                AssetBundleManager.LoadCallback OnLoaded =
                                delegate (AssetBundleManager.Code code, string abName, AssetBundle ab)
                                {
                                    string assetPath = string.Format("{0}{1}.mp3", AUDIO_BGM_PATH, name);
                                    AudioClip clip = ab.LoadAsset<AudioClip>(assetPath);

                                    Q.Assert(clip != null, "音樂缺少：" + assetPath);

                                    if (callback != null)
                                        callback(clip);
                                };
                GameController.Instance.AssetBundleMrg.LoadAudio(name, false, OnLoaded);
            }
        }

        //戰場物體的prefab
        public Transform CreateBattlePrefab(StageConfig sConfig)
        {
            GameObject prefab = (GameObject)Resources.Load("Prefabs/Map/Battle/" + sConfig.Map);
            Q.Assert(prefab != null, sConfig.ID + "戰場地圖缺失  " + sConfig.Map);
            GameObject go = GameObject.Instantiate(prefab);

            return go.transform;
        }

        /// <summary>
        /// 獲取目標圖片
        /// </summary>
        /// <param name="goal"></param>
        /// <returns></returns>
        public Sprite CreateGoalSprite(StageConfig.Goal goal)
        {
            Sprite sprite;
            string iconPath = "ENEMY";

            if (goal.Type != BattleGoal.Unit)
            {
                //怪物
                Q.Assert(GameController.Instance.Model.TileObjectConfigs.ContainsKey(goal.RelativeID), "Object表中无" + goal.RelativeID + " 项");
                TileObjectConfig oCfg = GameController.Instance.Model.TileObjectConfigs[goal.RelativeID];
                iconPath = oCfg.ResourceIcon;
            }

            AtlasManager atlasMgr = GameController.Instance.AtlasManager;
            sprite = atlasMgr.GetSprite(Atlas.Tile, iconPath);

            return sprite;
        }

        public Sprite CreateLoadingTipSprite(LoadingConfig loadingConfig)
        {

            Texture2D tex;

            tex = (Texture2D)Resources.Load<Texture>("Textures/UILoading/" + loadingConfig.Icon);

            if (tex == null)
            {
                return null;
            }

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }

        /// <summary>
        /// 網絡重連 裡面會有動態加載的內容prefab
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Transform CreateNetTipPrefab(string path)
        {
            Transform t = CreatePrefab("Prefabs/Ui/UINetWork/" + path);
            Q.Assert(t != null, "QMaxAssetsFactory::CreateNetTipPrefab({0})", path);

            return t;
        }

        public const string LVLBUTTON_STATE_UNLOCK_PREFAB = "Prefabs/Map/LvlButtonUnLock";
        public const string LVLBUTTON_STATE_LOCKED_PREFAB = "Prefabs/Map/LvlButtonLocked";
        public const string LVLBUTTON_STATE_SELECTED__PREFAB = "Prefabs/Map/LvlButtonSelected";
        public const string LVLBUTTON_STATE_GEMLOCKED_PREFAB = "Prefabs/Map/LvlButtonGemLocked";
        public const string ACTIVE_LVLBUTTON_STATE_UNLOCK = "Prefabs/Map/ActiveLvlButton";
        public const string ACTIVE_LVLBUTTON_STATE_LOCKED = "Prefabs/Map/ActiveLvlButtonLocked";

        public Transform CreateMapLvlButton(MapThing.ThingState state , bool isGemLocked = false)
        {
            Transform resT = null;

            if (isGemLocked)
            {
                resT = CreatePrefab(LVLBUTTON_STATE_GEMLOCKED_PREFAB);
                return resT;
            }

            switch (state)
            {
                case MapThing.ThingState.STATE_UNLOCK:
                    resT = CreatePrefab(LVLBUTTON_STATE_UNLOCK_PREFAB);
                    break;
                case MapThing.ThingState.STATE_LOCKED:
                    resT = CreatePrefab(LVLBUTTON_STATE_LOCKED_PREFAB);
                    break;
                case MapThing.ThingState.STATE_SELECTED:
                    resT = CreatePrefab(LVLBUTTON_STATE_SELECTED__PREFAB);
                    break;
                default:
                    break;
            }

            return resT;
        }

        public Transform CreateActiveMapLvlButton(MapThing.ThingState state)
        {
            Transform resT = null;

            if (state == MapThing.ThingState.STATE_LOCKED)
            {
                resT = CreatePrefab(ACTIVE_LVLBUTTON_STATE_LOCKED);
            }
            else
            {
                resT = CreatePrefab(ACTIVE_LVLBUTTON_STATE_UNLOCK);
            }
 
            return resT;
        }
    }
}
