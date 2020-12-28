using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Com4Love.Qmax.Tools
{
    public class AssetBundleManager : IDisposable
    {
        /// <summary>
        /// 操作返回碼
        /// </summary>
        public enum Code : int
        {
            Success = 0,
            //網絡錯誤
            NetworkError = 1,
            //解析json失敗
            ParseJsonError = 2,
        }

        /// <summary>
        /// 加載AssetBundle回調的Delegate
        /// </summary>
        /// <param name="code"></param>
        /// <param name="assetBundleName"></param>
        /// <param name="assetBundle"></param>
        public delegate void LoadCallback(Code code, string assetBundleName, AssetBundle assetBundle);

        /// <summary>
        /// 所有更新完成的事件
        /// </summary>
        public event Action<List<string>, List<Code>> UpdateCompleteEvent;

        /// <summary>
        /// 更新過程事件
        /// </summary>
        /// <param>index，在加載隊列中的序列</param>
        /// <param>Code</param>
        /// <param>assetBundleName</param>
        /// <param>AssetBundle</param>
        public event Action<int, Code, string, AssetBundle, float> UpdateProgressEvent;

        /// <summary>
        /// 資源配置文件的版本
        /// </summary>
        public int AssetConfigVersion = 0;


        private float _totalUpgradeSize;

        public float TotalUpgradeSize { get { return _totalUpgradeSize; } }

        public int NeedUpdateAssetCount { get { return needUpdateAsset.Count; } }

        public int AssetCount { get { return assetDict.Count; } }


        /// <summary>
        /// 需要更新的資源，string是AbInfo.Path
        /// </summary>
        protected List<string> needUpdateAsset;

        /// <summary>
        /// 正在加載未返回的資源，string是AbInfo.Path
        /// </summary>
        protected Dictionary<string, LoadCallback> pendingLoading;

        protected int localReslistVer = 0;

        protected int lastestReslistVer = 0;

        protected Dictionary<string, AssetBundleInfo> localAssetInfos;

        /// <summary>
        /// 保存所有資源的最新版本
        /// asset bundle name -> AssetBundleInfo
        /// </summary>
        protected Dictionary<string, AssetBundleInfo> assetDict;

        private MonoBehaviour monoBeh;

        /// <summary>
        /// 下載AssetBundle的url前綴
        /// </summary>
        private string assetBundleUrl;

        /// <summary>
        /// 隨包打包的AssetBundle目錄
        /// </summary>
        private string packagedAssetBundlePath;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetBundleUrlPrefix"></param>
        /// <param name="monoBeh"></param>
        public AssetBundleManager(string assetBundleUrlPrefix,
                                  RuntimePlatform platform,
                                  MonoBehaviour monoBeh)
        {
            //處理urlPrefix可能最後一個字符為'/'的情况
            assetBundleUrlPrefix =
                assetBundleUrlPrefix[assetBundleUrlPrefix.Length - 1] == '/' ?
                assetBundleUrlPrefix.Substring(0, assetBundleUrlPrefix.Length - 1) :
                assetBundleUrlPrefix;

            string str = Utils.GetAssetBundlesFolder(platform);
			this.assetBundleUrl = Path.Combine(assetBundleUrlPrefix, str);
            packagedAssetBundlePath = Utils.BuildStreamingAssetsReqUrl("assetbundles/" + str);

            this.monoBeh = monoBeh;
            needUpdateAsset = new List<string>();
            pendingLoading = new Dictionary<string, LoadCallback>();
        }


        /// <summary>
        /// 檢查資源版本，看是否需要更新
        /// </summary>
        /// <returns>是否需要更新</returns>
        public void CheckAssetStatus(Action<Code> callback)
        {
            monoBeh.StartCoroutine(LoadReslist(callback));
        }


        /// <summary>
        /// 更新所有未更新的資源
        /// </summary>
        /// <returns>true，需要更新；false，不需要更新</returns>
        public bool UpdateAll()
        {
            if (needUpdateAsset.Count == 0)
            {
                return false;
            }
            monoBeh.StartCoroutine(LoadAllCoroutine());
            return true;
        }


        /// <summary>
        /// 加載一個AssetBundle
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="callback"></param>
        /// <param name="autoDispose"></param>
        public void LoadOneAssetBundle(string assetBundleName,
                                       LoadCallback callback,
                                       bool autoDispose = true)
        {
            Q.Log("LoadOneAssetBundle {0}", assetBundleName);
            assetBundleName = assetBundleName.ToLower();
            Q.Assert(monoBeh != null);
            Q.Assert(!string.IsNullOrEmpty(assetBundleName));

            //如果正在加載，那麼加入監聽隊列，不重新開啟加載
            if (pendingLoading.ContainsKey(assetBundleName))
            {
                pendingLoading[assetBundleName] += callback;
            }
            else
            {
                pendingLoading.Add(assetBundleName, callback);
                monoBeh.StartCoroutine(LoadOneCoroutine(assetBundleName, autoDispose));
            }
        }


        /// <summary>
        /// 加載一個夥伴
        /// </summary>
        /// <param name="unitName"></param>
        /// <param name="callback"></param>
        public void LoadUnit(string unitName, LoadCallback callback)
        {
            string abName = string.Format("unit/{0}", unitName.ToLower());
            LoadOneAssetBundle(abName, callback);
        }

        public void LoadUnitImg(string unitName, LoadCallback callback)
        {
            string abName = string.Format("unitimg/{0}img", unitName.ToLower());
            LoadOneAssetBundle(abName, callback);
        }


        /// <summary>
        /// 加載一段Audio
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="isEffect">是否是音效，否則是背景音樂</param>
        /// <param name="callback"></param>
        public void LoadAudio(string audioName, bool isEffect, LoadCallback callback)
        {
            string abName = string.Format("audio/{0}/{1}", isEffect ? "effect" : "bgm", audioName);
            LoadOneAssetBundle(abName, callback);
        }


        /// <summary>
        /// 加載數據配置
        /// </summary>
        /// <param name="callback"></param>
        public void LoadDataConfig(LoadCallback callback)
        {
            LoadOneAssetBundle("dataconfig", callback, false);
        }


        /// <summary>
        /// 加載關卡配置
        /// </summary>
        /// <param name="level"></param>
        /// <param name="callback"></param>
        public void LoadLevelConfig(LoadCallback callback)
        {
            LoadOneAssetBundle("levelconfig", callback, false);
        }


        public void Dispose()
        {
            UpdateCompleteEvent = null;
            UpdateProgressEvent = null;
        }




        private IEnumerator LoadReslist(Action<Code> callback)
        {
            Q.Log("LoadReslist localResList={0}, lastestReslist={1}",
                packagedAssetBundlePath + "/reslist.json",
                assetBundleUrl + "/reslist.json");

            //用於測速
            //int swID = Q.StartNewStopwatch();

            if (localAssetInfos == null || localAssetInfos.Count == 0)
            {
                yield return monoBeh.StartCoroutine(LoadLocalReslist());
            }

            //獲取最新的reslist
            string remoteReslistUrl = Path.Combine(assetBundleUrl, "reslist.json").Replace("\\", "/");
            WWW www = new WWW(remoteReslistUrl);
            yield return www;

            //Q.LogElapsedSecAndReset(swID, "load remote reslist");

            Code callbackCode = Code.NetworkError;

            if (!string.IsNullOrEmpty(www.error))
            {
                //如果加載不到cdn的reslist，就跟本地保持一致
                Q.Warning("Load remote reslist fail: {0}", www.error);
                assetDict = new Dictionary<string, AssetBundleInfo>(localAssetInfos);
                callbackCode = Code.NetworkError;
            }
            else
            {
                string str = Encoding.UTF8.GetString(www.bytes);
                yield return 0;
                www.Dispose();
                //Q.LogElapsedSecAndReset(swID, "parse text");
                assetDict = ParseReslist(str, true, out lastestReslistVer);
                yield return 0;
                //Q.LogElapsedSecAndReset(swID, "ParseReslist");
                callbackCode = Code.Success;
            }


            Q.Log("LoadReslist localConfVer={0}, lastestConfVer={1}",
                localReslistVer, lastestReslistVer);

            needUpdateAsset = CompareReslist(out _totalUpgradeSize);

            //Q.LogElapsedSecAndReset(swID, "compare");
            if (callback != null)
                callback(callbackCode);
        }


        /// <summary>
        /// 加載解析本地的Reslist
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadLocalReslist()
        {
            localAssetInfos = new Dictionary<string, AssetBundleInfo>();
            //獲取本地的reslist

            string reslistUrl = Path.Combine(packagedAssetBundlePath, "reslist.json").Replace("\\", "/");
			Q.Log ("AssetBundleMgr:LoadLocalReslist() reslistUrl={0}", reslistUrl);

            WWW www = new WWW(reslistUrl);
            yield return www;

            //Q.LogElapsedSecAndReset(swID, "load local reslist");

            if (string.IsNullOrEmpty(www.error))
            {
                localAssetInfos = ParseReslist(www.text, false, out localReslistVer);
                //Q.LogElapsedSecAndReset(swID, "ParseReslist");
                www.Dispose();
                yield return 0;
            }
            else
            {
                //本地log，解析失敗，則認為本地沒有任何AssetBundle
                Q.Log("Load local reslist fail: {0}", www.error);
            }
            assetDict = new Dictionary<string, AssetBundleInfo>(localAssetInfos);
        }


        /// <summary>
        /// 比較本地資源列表和遠程資源列表
        /// </summary>
        /// <param name="totalSize"></param>
        /// <returns></returns>
        private List<string> CompareReslist(out float totalSize)
        {
            List<String> ret = new List<string>();
            totalSize = 0f;
            if (localReslistVer >= lastestReslistVer)
                return ret;

            //考慮以下情況：
            //1. 已隨包打包，且為最新
            //2. 已隨包打包，但不是最新
            //3. 未隨包打包，已緩存
            //4. 未隨包打包，未緩存
            var dict = new Dictionary<string, AssetBundleInfo>(assetDict);
            foreach (var pair in dict)
            {
                //Q.Log("{0}.{1}.{2}.{3}",
                //    pair.Key,
                //    localAbs.ContainsKey(pair.Key),
                //    localAbs[pair.Key].Version == pair.Value.Version,
                //    localAbs[pair.Key].Packaged);

                //隨包的AssetBundle是最新版本
                if (localAssetInfos.ContainsKey(pair.Key) &&
                    localAssetInfos[pair.Key].Version == pair.Value.Version &&
                    localAssetInfos[pair.Key].Packaged)
                {
                    AssetBundleInfo abInfo = assetDict[pair.Key];
                    //已經存在本地的，做一個標記
                    abInfo.Packaged = true;
                    assetDict[pair.Key] = abInfo;
                    continue;
                }

                //本地已經有緩存
                string url = string.Format("{0}/{1}", assetBundleUrl, pair.Key);
                if (Caching.IsVersionCached(url, pair.Value.Version))
                    continue;

                totalSize += pair.Value.Size;

                ret.Add(pair.Value.Path);
            }
            return ret;
        }


        private IEnumerator LoadAllCoroutine()
        {
            List<string> downloadfailList = new List<string>();
            List<Code> failCodeList = new List<Code>();
            int count = 0;
            while (needUpdateAsset.Count > 0)
            {
                AssetBundleInfo abInfo = assetDict[needUpdateAsset[0]];
                needUpdateAsset.RemoveAt(0);
                string url = null;
                if (abInfo.Packaged)
                    url = Path.Combine(packagedAssetBundlePath, abInfo.Path).Replace("\\", "/");
                else
                    url = Path.Combine(assetBundleUrl, abInfo.Path).Replace("\\", "/");

                if (pendingLoading.ContainsKey(abInfo.Path))
                {
                    Q.Assert(false, GetType().Name + ":LoadAllCoroutine Assert 1");
                    continue;
                }
                else
                {
                    pendingLoading.Add(abInfo.Path,
                        delegate(Code code, string s, AssetBundle ab) {/*do nothing*/}
                    );
                }

                WWW www = WWW.LoadFromCacheOrDownload(url, abInfo.Version);
                yield return www;

                count++;
                if (www.error != null)
                {
                    downloadfailList.Add(abInfo.Path);
                    failCodeList.Add(Code.NetworkError);

                    if (UpdateProgressEvent != null)
                        UpdateProgressEvent(count, Code.NetworkError, abInfo.Path, null, 0);

                    pendingLoading[abInfo.Path](Code.NetworkError, abInfo.Path, null);
                    pendingLoading.Remove(abInfo.Path);

                    yield return 1;
                    continue;
                }

                if (UpdateProgressEvent != null)
                    UpdateProgressEvent(count, Code.Success, abInfo.Path, www.assetBundle, abInfo.Size);


                pendingLoading[abInfo.Path](Code.Success, abInfo.Path, www.assetBundle);
                pendingLoading.Remove(abInfo.Path);

                www.assetBundle.Unload(false);
                www.Dispose();

                yield return 1;
            }


            if (UpdateCompleteEvent != null)
                UpdateCompleteEvent(downloadfailList, failCodeList);
        }


        private IEnumerator LoadOneCoroutine(string assetbundlePath,
                                             bool autoDispose)
        {
            if (assetDict == null || assetDict.Count == 0)
            {
                //如果要加載資源，但是發現還未解析好reslit，那麼就直接加載解析本地的reslist
                yield return monoBeh.StartCoroutine(LoadLocalReslist());
            }

            bool isPackaged = false;
            if (assetDict.ContainsKey(assetbundlePath))
            {
                isPackaged = assetDict[assetbundlePath].Packaged;
                Q.Log("LoadOneCoroutine1: p={0}, v={1}, isPackaged={2}",
                    assetbundlePath,
                    assetDict[assetbundlePath].Version,
                    isPackaged);
            }
            else
            {
                Q.Log(LogTag.Error, "LoadOneCoroutine(). assetDict do not contains {0}",
                       assetbundlePath);
            }


            assetbundlePath = assetbundlePath.ToLower();
            if (needUpdateAsset != null && needUpdateAsset.Contains(assetbundlePath))
                needUpdateAsset.Remove(assetbundlePath);

            string url = null;
            if (isPackaged)
                url = Path.Combine(packagedAssetBundlePath, assetbundlePath).Replace("\\", "/");
            else
                url = Path.Combine(assetBundleUrl, assetbundlePath).Replace("\\", "/");
                
			WWW www = WWW.LoadFromCacheOrDownload(url, assetDict[assetbundlePath].Version);
            yield return www;

            Q.Assert(pendingLoading.ContainsKey(assetbundlePath),
                GetType().Name + ":LoadOneCoroutine Assert 1");

            Code c = www.error == null ? Code.Success : Code.NetworkError;
            Q.Assert(c == Code.Success, "asset={0}, msg={1}", assetbundlePath, www.error);
            if (pendingLoading.ContainsKey(assetbundlePath))
            {
                pendingLoading[assetbundlePath](c, assetbundlePath, c == Code.Success ? www.assetBundle : null);
                pendingLoading.Remove(assetbundlePath);
            }

            if (c == Code.Success && autoDispose)
                www.assetBundle.Unload(false);

            www.Dispose();
        }


        /// <summary>
        /// 解析reslist的函數
        /// </summary>
        /// <param name="rawContent"></param>
        /// <param name="isRemoteReslist"></param>
        /// <param name="confVersion"></param>
        /// <returns></returns>
        static public Dictionary<string, AssetBundleInfo> ParseReslist(string rawContent,
                                                                       bool isRemoteReslist,
                                                                       out int confVersion)
        {
            Dictionary<string, AssetBundleInfo> ret = new Dictionary<string, AssetBundleInfo>();
            confVersion = 0;
            try
            {
                JSONInStream jInStream = new JSONInStream(rawContent);
                jInStream.Content("version", out confVersion)
                .List("assets",
                    delegate(int index, JSONInStream stream)
                    {
                        AssetBundleInfo info = new AssetBundleInfo();
                        int iPackged = 0;
                        stream.Start(0)
                            .Content("assetbundlename", out info.Path)
                            .Content("version", out info.Version)
                            .Content("size", out info.Size)
                            .Content("hash", out info.Hash)
                            .ContentOptional("packaged", ref iPackged)
                            .End();

                        if (isRemoteReslist)//只有本地的reslist才會記錄Packged字段
                            info.Packaged = false;
                        else
                            info.Packaged = iPackged == 1;

                        ret.Add(info.Path, info);
                    }
                )
                .End();
            }
            catch (Exception e)
            {
                Q.Log(e.Message);
            }
            return ret;
        }
    }
}
