using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Com4Love.Qmax.Tools
{

    /// <summary>
    /// 這裡有兩種對像池：SpawnPool和InstancePool
    /// SpawnPool：key是string，value是Prefab，每次可以獲取從Prefab生成的實例。
    /// InstancePool：key是string，value是GameObject實例，每次就是獲取對應的唯一的GameObject
    /// </summary>
    public class GOPoolManager
    {
#if EFFECT_HIDE
        /// <summary>
        /// resource下特效prefab的路徑
        /// </summary>
        public const string RESOURCE_EFFECT_ROOT = "Prefabs/Effects";
        public const string BLOCK_EFFECT_PREFAB = "Prefabs/BlockEffectTestPrefab";
#endif

        protected SpawnPool spawnPool;
        protected Dictionary<string, AudioClip> audioClipPools = new Dictionary<string, AudioClip>();
        protected Transform root;

        /// <summary>
        /// 實例的對像池，一個Key對應一個GameObject
        /// 與SpawnPool做區分：SpawnPool是用Prefab做key，每次獲取從Prefab生成的GameObject
        /// </summary>
        private Transform instancePool;

        private Dictionary<string, Transform> insPoolDict;

        public GOPoolManager(SpawnPool spawnPool_, Transform root_)
        {
            spawnPool = spawnPool_;
            root = root_;
            instancePool = root_.Find("InstancePool");
            Q.Assert(instancePool != null);
            insPoolDict = new Dictionary<string, Transform>();
        }

        /// <summary>
        ///與初始化prefab 使用前初始化，在使用時不會卡頓 直接通過key去獲取
        /// </summary>
        /// <param name="key">Resource下全路徑</param>
        /// <param name="count">初始化的數量</param>
        public void PrePrefabSpawn(string key, int count = 1)
        {
            if (!spawnPool.prefabs.ContainsKey(key))
            {
                Transform prefab;

#if EFFECT_HIDE
                //隱藏特效 當判斷路徑是特效路徑時，統一做處理
                if (key.IndexOf(RESOURCE_EFFECT_ROOT) > -1)
                {
                    prefab = Resources.Load<Transform>(BLOCK_EFFECT_PREFAB);
                    prefab.name = string.Concat(key, BLOCK_EFFECT_PREFAB);
                }
                else
                {
                    prefab = Resources.Load<Transform>(key);
                }
#else
                prefab = Resources.Load<Transform>(key);
#endif

                PrefabPool prefabPool = new PrefabPool(prefab);
                prefabPool.preloadAmount = count;
                spawnPool.CreatePrefabPool(prefabPool, key);
            }
        }


        /// <summary>
        /// loading中預加載使用
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="key">Resource下全路徑</param>
        /// <param name="count"></param>
        public void PrePrefabSpawn(Transform prefab, string key, int count = 1)
        {
            if (!spawnPool.prefabs.ContainsKey(key))
            {
                PrefabPool prefabPool = new PrefabPool(prefab);
                prefabPool.preloadAmount = count;
                spawnPool.CreatePrefabPool(prefabPool, key);
            }
        }

        public bool IsSpawnPoolExist(string key)
        {
            return spawnPool.prefabs.ContainsKey(key);
        }

        //命名跟其它的完全不一樣。。。。
        public bool PushToInstancePool(string assetName, Transform ins)
        {
            if (ins == null)
                return false;

            if (insPoolDict.ContainsKey(assetName))
            {
                //如果已經有這個了，就直接銷毀
                GameObject.Destroy(ins.gameObject);
            }

            //不管 insPoolDict 裡面有沒有此項，都要重新放進去 2.16
            ins.SetParent(instancePool);
            ins.gameObject.SetActive(false);
            insPoolDict.Add(assetName, ins);
            return true;
        }

        /// <summary>
        /// 從單例池中取出來
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public Transform PopFromInstancePool(string key, bool loadFromResource = false)
        {
            Transform tran = null;

            //Q.Log("PopFromInstancePool {0}", assetName);
            if (insPoolDict.ContainsKey(key))
            {
                tran = insPoolDict[key];
                insPoolDict.Remove(key);
                Q.Assert(tran != null, "PopFromInstancePool tran is null");
                tran.gameObject.SetActive(true);            //在編輯器運行時出現過 tran 為 null的BUG。原因可能是因為有其它變量直接引用了在單例對像池中的此物體，然後執行銷毀動作。
            }
            else if (loadFromResource)
            {
                Transform sourceTran;
#if EFFECT_HIDE
                //隱藏特效 當判斷路徑是特效路徑時，統一做處理
                if (key.IndexOf(RESOURCE_EFFECT_ROOT) > -1)
                {
                    sourceTran = Resources.Load<Transform>(BLOCK_EFFECT_PREFAB);
                    sourceTran.name = string.Concat(key, BLOCK_EFFECT_PREFAB);
                }
                else
                {
                    sourceTran = Resources.Load<Transform>(key);
                }
#else
                sourceTran = Resources.Load<Transform>(key);
#endif

                tran = GameObject.Instantiate<Transform>(sourceTran);
            }

            return tran;
        }

        public void RemoveAtInstancePool(string assetName)
        {
            //Q.Log("RemoveAtInstancePool {0}", assetName);
            if (insPoolDict.ContainsKey(assetName))
            {
                GameObject.Destroy(insPoolDict[assetName].gameObject);
                insPoolDict.Remove(assetName);
            }
        }

        public void RemoveAtInstancePool(Transform trans)
        {
            string k = null;
            foreach (var pair in insPoolDict)
            {
                if (pair.Value == trans)
                {
                    k = pair.Key;
                    break;
                }
            }
            if (k != null)
            {
                //Q.Log("RemoveAtInstancePool {0}", k);
                RemoveAtInstancePool(k);
            }
        }

        public bool CheckInstancePool(string assetName) { return insPoolDict.ContainsKey(assetName); }


        /// <summary>
        /// 創建prefab    
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public Transform PrefabSpawn(string key, bool isFromAssetBundle = false)
        {
            PrePrefabSpawn(key, 1);

            return spawnPool.Spawn(key);
        }


        public void GetUnitInstance(UnitConfig conf,
                                    Action<string, Transform> callback)
        {
            if (insPoolDict.ContainsKey(conf.PrefabPath))
            {
                Transform target = PopFromInstancePool(conf.PrefabPath);
                if (!string.IsNullOrEmpty(conf.Skin))
                {
                    target.GetComponent<SkeletonAnimator>().initialSkinName = conf.Skin;
                    target.GetComponent<SkeletonAnimator>().Reset();
                }
                callback(conf.PrefabPath, target);
            }
#if UNITY_EDITOR
            else if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
                //Editor狀態下，從本地加載
                string p = string.Format("Assets/ExternalRes/Unit/{0}/{1}.prefab", conf.ResourceIcon, conf.ResourceIcon);
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(p);
                Q.Assert(prefab != null, "夥伴缺少模型素材id：{0}", p);

                GameObject ins = GameObject.Instantiate<GameObject>(prefab);
                if (!string.IsNullOrEmpty(conf.Skin))
                {
                    ins.GetComponent<SkeletonAnimator>().initialSkinName = conf.Skin;
                    ins.GetComponent<SkeletonAnimator>().Reset();
                }
                ins.transform.localScale = prefab.transform.localScale;
                callback(conf.PrefabPath, ins.transform);
            }
#endif
            else
            {
                //從AssetBundle異步加載
                GameController.Instance.AssetBundleMrg.LoadUnit(
                    conf.ResourceIcon,
                    delegate (AssetBundleManager.Code code, string abName, AssetBundle ab)
                    {
                        if (callback == null)
                            return;

                        if (code != AssetBundleManager.Code.Success)
                        {
                            Q.Warning("加載失敗 ab={0}, code={1}", abName, code);
                            callback(conf.PrefabPath, null);
                        }

                        GameObject prefab = ab.LoadAsset<GameObject>(conf.PrefabPath);
                        GameObject ins = GameObject.Instantiate<GameObject>(prefab);
                        if (!string.IsNullOrEmpty(conf.Skin))
                        {
                            ins.GetComponent<SkeletonAnimator>().initialSkinName = conf.Skin;
                            ins.GetComponent<SkeletonAnimator>().Reset();
                        }
                        ins.transform.localScale = prefab.transform.localScale;
                        //因為從AssetBundle加載，所以音效的output關聯會丟失
                        //所以在這裡需要重新關聯上
                        AudioSource[] arr = ins.transform.GetComponentsInChildren<AudioSource>();
                        AudioMixerGroup[] amg = GameController.Instance.AudioMixer.FindMatchingGroups("Master/Sound");
                        Q.Assert(amg.Length > 0);
                        for (int i = 0, n = arr.Length; i < n; i++)
                        {
                            arr[i].outputAudioMixerGroup = amg[0];
                        }
                        callback(conf.PrefabPath, ins.transform);
                    }
                );
            }
        }

        IEnumerator LoadUnit(UnitConfig conf, Action<string, Transform> callback)
        {
            string url = string.Format("Prefabs/Units/{0}", conf.ResourceIcon);
            ResourceRequest req = Resources.LoadAsync<GameObject>(url);
            while (!req.isDone)
            {
                yield return 0;
            }

            GameObject prefab = req.asset as GameObject;
            if (prefab == null)
            {
                callback(conf.PrefabPath, null);
            }
            else
            {
                Transform trans = GameObject.Instantiate<GameObject>(prefab).transform;
                callback(conf.PrefabPath, trans);
            }
        }

        void LoadUnitSync(UnitConfig conf, Action<string, Transform> callback)
        {
            string url = string.Format("Prefabs/Units/{0}", conf.ResourceIcon);
            GameObject prefab = Resources.Load<GameObject>(url);
            if (prefab == null)
            {
                callback(conf.PrefabPath, null);
            }
            else
            {
                Transform trans = GameObject.Instantiate<GameObject>(prefab).transform;
                callback(conf.PrefabPath, trans);
            }
        }

        public void PreAudioClipSpawn(string key)
        {
            if (!audioClipPools.ContainsKey(key))
            {
                audioClipPools.Add(key, Resources.Load(key) as AudioClip);
            }
        }

        public AudioClip AudioClipSpawn(string key)
        {
            PreAudioClipSpawn(key);

            return audioClipPools[key];
        }


        /// <summary>
        /// 將對象扔回對像池
        /// </summary>
        /// <param name="t"></param>
        /// <param name="time"></param>
        public void Despawn(Transform t, float time = 0)
        {
            if (spawnPool.isDestroyed)
            {
                return;
            }

            spawnPool.Despawn(t, time, root);
        }

        /// <summary>
        /// 將某對​​像池徹底銷毀
        /// </summary>
        /// <param name="t"></param>
        public void Despool(Transform t)
        {
            if (spawnPool.isDestroyed)
            {
                return;
            }

            Transform prefab = spawnPool.GetPrefab(t);
            PrefabPool prefabPool = spawnPool.GetPrefabPool(prefab);
            spawnPool.DesPrefabPool(prefabPool);
        }

        /// <summary>
        /// 將某對​​像池徹底銷毀
        /// </summary>
        /// <param name="key"></param>
        public void Despool(string key)
        {
            if (spawnPool.isDestroyed)
            {
                return;
            }

            if (spawnPool.prefabs.ContainsKey(key))
            {
                Transform prefab = spawnPool.prefabs[key];
                PrefabPool prefabPool = spawnPool.GetPrefabPool(prefab);
                spawnPool.DesPrefabPool(prefabPool);
            }
        }
    }

}
