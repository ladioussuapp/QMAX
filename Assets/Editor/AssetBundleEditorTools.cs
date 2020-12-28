using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Tools;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleEditorTools : AssetPostprocessor
{
    public const string ASSET_BUNDLE_SRC_DIR = "Assets/ExternalRes";

    /// <summary>
    /// 導出所有Android的Asset Bundle資源
    /// </summary>
    [MenuItem("Qmax/AssetBunlde/Export AssetBundles（導出當前平台的AssetBundle資源）")]
    static void BuildAssetBundles()
    {
        BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
        opt = opt | BuildAssetBundleOptions.DeterministicAssetBundle;
        //opt = opt | BuildAssetBundleOptions.AppendHashToAssetBundleName;

        BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        if (target != BuildTarget.Android &&
            target != BuildTarget.iOS &&
            target != BuildTarget.StandaloneWindows64 &&
            target != BuildTarget.StandaloneWindows)
        {
            Debug.LogWarningFormat("不支持導出{0}平台的AssetBundle", target);
            return;
        }

        BuildAllAssetBundle(opt, target, "Assets/StreamingAssets/assetbundles/" + GetAssetBundlesPlatformFolder(target));
    }


    [MenuItem("Qmax/AssetBunlde/Clear Cache （清理AssetBundle緩存）")]
    static void CleanCache()
    {
        Caching.CleanCache();
    }

    [MenuItem("Qmax/AssetBunlde/Clear Reslist（刪除Reslist中無用的資源）")]
    //static void CleanReslist()
    //{
    //    //TODO
    //}


    [MenuItem("Qmax/AssetBunlde/Mark Local AssetBundle（標記AssetBundle隨包資源）")]
    static public void MarkLocalAssetBundle(BuildTarget target)
    {
        Debug.Log("執行操作：Mark Local AssetBundle");
        string reslistPath = string.Format(
            "{0}/assetbundles/{1}/reslist.json",
            Application.streamingAssetsPath,
            GetAssetBundlesPlatformFolder(target)
        );

        if (!File.Exists(reslistPath))
        {
            Debug.LogWarningFormat("沒有在StreamPath/assetbundles/{0}下找到reslist.json", GetAssetBundlesPlatformFolder(target));
            return;
        }

        string rawInfo = File.ReadAllText(reslistPath);
        if (string.IsNullOrEmpty(rawInfo))
        {

            Debug.LogWarning("reslist.json是空文件");
            return;
        }

        int confVer = 0;
        Dictionary<string, AssetBundleInfo> infoDict = AssetBundleManager.ParseReslist(rawInfo, false, out confVer);
        Dictionary<string, AssetBundleInfo> newDict = new Dictionary<string, AssetBundleInfo>();
        foreach (var pair in infoDict)
        {
            string path = string.Format("{0}/assetbundles/{1}/{2}",
                Application.streamingAssetsPath,
                GetAssetBundlesPlatformFolder(target),
                pair.Key);
            AssetBundleInfo newInfo = pair.Value;
            newInfo.Packaged = File.Exists(path);
            newDict.Add(newInfo.Path, newInfo);
        }

        FileInfo fInfo = new FileInfo(reslistPath);
        SaveReslist(fInfo, newDict, confVer);
        Debug.Log("操作完成");
    }


    static private void BuildAllAssetBundle(BuildAssetBundleOptions opt, BuildTarget biuldTarget, string outputDir)
    {
        int reslistVer = 0;
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        FileInfo reslistFileInfo = null;
        Dictionary<string, AssetBundleInfo> reslistData = LoadResConf(outputDir, out reslistVer, out reslistFileInfo);
        //Unit资源
        builds.AddRange(BuildUnitAssetBundles(opt, biuldTarget, reslistData, outputDir));
        //数据配置
        builds.Add(BuildConfAB(opt, biuldTarget, outputDir));
        //关卡配置
        builds.Add(BuildLevelConfAB(opt, biuldTarget, outputDir));
        //Audio
        builds.AddRange(BuildAudioAB(opt, biuldTarget, outputDir));

        if (builds.Count == 0)
        {
            Debug.LogFormat("沒有資源需要導出AssetBundle");
            return;
        }

        AssetBundleManifest manifest =
            BuildPipeline.BuildAssetBundles(outputDir, builds.ToArray(), opt, biuldTarget);

        if (manifest == null)
        {
            Debug.LogWarning("Manifest == null");
            return;
        }

        string[] assetBundles = manifest.GetAllAssetBundles();
        bool hasChange = false;
        Dictionary<string, AssetBundleInfo> newReslistData = new Dictionary<string, AssetBundleInfo>();
        for (int i = 0, n = assetBundles.Length; i < n; i++)
        {
            string assetbundlename = assetBundles[i];
            string hash = manifest.GetAssetBundleHash(assetbundlename).ToString();
            int version = 0;
            if (!reslistData.ContainsKey(assetbundlename))
            {
                version = 1;
            }
            else if ((string)reslistData[assetbundlename].Hash != hash)
            {
                version = (int)reslistData[assetbundlename].Version + 1;
            }
            else
            {
                version = (int)reslistData[assetbundlename].Version;
                //Debug.LogWarningFormat("{0} hash相同", assetbundlename);
            }

            AssetBundleInfo info = new AssetBundleInfo();
            info.Path = assetbundlename;
            info.Version = version;


            FileInfo fInfo = new FileInfo(outputDir + "/" + assetbundlename);
            info.Size = (int)(fInfo.Length / 1024);
            info.Hash = hash;
            info.Packaged = false;
            newReslistData.Add(assetbundlename, info);
            hasChange = true;
        }

        Debug.LogFormat("AssetBundle總量{0}", newReslistData.Count);

        if (hasChange)
            SaveReslist(reslistFileInfo, newReslistData, reslistVer + 1);
        else
            SaveReslist(reslistFileInfo, newReslistData, reslistVer);

        Debug.Log("處理完成");
    }


    /// <summary>
    /// 檢測目錄"Assets/StreamingAssets/assetbundles/"的改動
    /// 如果有改動，則重新調用MarkLocalAssetBundle命令
    /// </summary>
    /// <param name="importedAssets"></param>
    /// <param name="deletedAssets"></param>
    /// <param name="movedAssets"></param>
    /// <param name="movedFromAssetPaths"></param>
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        bool flg = false;
        for (int i = 0, n = importedAssets.Length; !flg && i < n; i++)
        {
            if (importedAssets[i].IndexOf("Assets/StreamingAssets/assetbundles/") >= 0)
                flg = true;
        }

        for (int i = 0, n = deletedAssets.Length; !flg && i < n; i++)
        {
            if (deletedAssets[i].IndexOf("Assets/StreamingAssets/assetbundles/") >= 0)
                flg = true;
        }

        for (int i = 0, n = movedAssets.Length; !flg && i < n; i++)
        {
            if (movedAssets[i].IndexOf("Assets/StreamingAssets/assetbundles/") >= 0)
                flg = true;
        }

        if (!flg)
            return;

        Debug.Log("'Assets/StreamingAssets/assetbundles/'目錄有改動，重新調用MarkLocalAssetBundle命令");
        //需要重新生成
        MarkLocalAssetBundle(BuildTarget.iOS);
        MarkLocalAssetBundle(BuildTarget.StandaloneWindows64);
        MarkLocalAssetBundle(BuildTarget.Android);
    }


    static private List<AssetBundleBuild> BuildUnitAssetBundles(BuildAssetBundleOptions opt,
                                              BuildTarget biuldTarget,
                                              Dictionary<string, AssetBundleInfo> assetInfoDict,
                                              string outputPath)
    {
        Debug.Log("處理資源: Unit");

        //讀取UnitConfig配置文件，只有這個文件配置的Unit才會記錄在reslist.json裡

        string text = File.ReadAllText(string.Format("{0}/Config/UnitConfig.xml", ASSET_BUNDLE_SRC_DIR));
        XMLInStream stream = new XMLInStream(text);

        Dictionary<string, UnitConfig> UnitConfigs = new Dictionary<string, UnitConfig>();
        stream.List("item", delegate(XMLInStream itemStream)
        {
            UnitConfig ufg = new UnitConfig(itemStream);

            //注意這裡用的是ResourceIcon字段
            if (!UnitConfigs.ContainsKey(ufg.ResourceIcon))
                UnitConfigs.Add(ufg.ResourceIcon, ufg);
        });

        List<AssetBundleBuild> ret = new List<AssetBundleBuild>();

        //生成prefab的AssetBundle
        foreach (var pair in UnitConfigs)
        {
            DirectoryInfo dInfo = new DirectoryInfo(string.Format("{0}/Unit/{1}", ASSET_BUNDLE_SRC_DIR, pair.Key));

            //這裡注意轉成小寫
            string assetbundlename = "unit/" + pair.Key.ToLower();
            if (!dInfo.Exists)
            {
                // 如果UnitConfig配置裡沒有的字段，需要從reslist.json裡清理掉
                if (assetInfoDict.ContainsKey(assetbundlename))
                    assetInfoDict.Remove(assetbundlename);
                continue;
            }

            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = assetbundlename;
            string[] assetNames = { string.Format("{0}/Unit/{1}/{2}.prefab", ASSET_BUNDLE_SRC_DIR, pair.Key, pair.Key) };
            abb.assetNames = assetNames;
            ret.Add(abb);
        }

        //生成UnitImg的AssetBundle
        foreach (var pair in UnitConfigs)
        {
            //夥伴圖1
            string p1 = string.Format("{0}/UnitImg1/{1}.png", ASSET_BUNDLE_SRC_DIR, pair.Key);
            //夥伴圖2
            string p2 = string.Format("{0}/UnitImg2/{1}.png", ASSET_BUNDLE_SRC_DIR, pair.Key);
            FileInfo img1Info = new FileInfo(p1);
            FileInfo img2Info = new FileInfo(p2);
            string assetbundlename = string.Format("unitimg/{0}img", pair.Key.ToLower());
            if (!img1Info.Exists && !img2Info.Exists)
                continue;

            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = assetbundlename;
            string[] assetNames = { p1, p2 };
            abb.assetNames = assetNames;
            ret.Add(abb);
        }

        return ret;
    }


    /// <summary>
    /// 生成關卡配置的AssetBundle
    /// </summary>
    /// <param name="opt"></param>
    /// <param name="biuldTarget"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    static private AssetBundleBuild BuildLevelConfAB(BuildAssetBundleOptions opt,
        BuildTarget biuldTarget,
        string outputPath)
    {
        const string assetbundlename = "levelconfig";

        Debug.LogFormat("處理資源: {0}", assetbundlename);
        AssetBundleBuild ret = new AssetBundleBuild();
        ret.assetBundleName = assetbundlename;

        List<string> assetNames = new List<string>();
        DirectoryInfo dInfo = new DirectoryInfo(ASSET_BUNDLE_SRC_DIR + "/Config/Levels");
        FileInfo[] fInfoArray = dInfo.GetFiles("*.json");
        for (int i = 0, n = fInfoArray.Length; i < n; i++)
        {
            FileInfo f = fInfoArray[i];
            assetNames.Add(ASSET_BUNDLE_SRC_DIR + "/Config/Levels/" + f.Name);
        }
        ret.assetNames = assetNames.ToArray();
        return ret;
    }



    /// <summary>
    /// 生成Audio的AssetBundle
    /// Audio資源是一個文件一個AssetBundle
    /// </summary>
    /// <param name="opt"></param>
    /// <param name="biuldTarget"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    static private List<AssetBundleBuild> BuildAudioAB(BuildAssetBundleOptions opt,
        BuildTarget biuldTarget,
        string outputPath)
    {
        const string assetbundlename = "audio";

        Debug.LogFormat("處理資源: {0}", assetbundlename);


        List<AssetBundleBuild> ret = new List<AssetBundleBuild>();
        //保存每一個AssetBundle文件的位置，與buildArray中的元素對應
        List<string> pathArray = new List<string>();

        DirectoryInfo dir = new DirectoryInfo(ASSET_BUNDLE_SRC_DIR + "/Audio/Effect");
        List<FileInfo> fileList = new List<FileInfo>(dir.GetFiles("*.ogg"));
        fileList.AddRange(new List<FileInfo>(dir.GetFiles("*.mp3")));
        for (int i = 0, n = fileList.Count; i < n; i++)
        {
            FileInfo f = fileList[i];
            string[] assetNames = { ASSET_BUNDLE_SRC_DIR + "/Audio/Effect/" + f.Name };
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = "audio/effect/" + Path.GetFileNameWithoutExtension(f.Name);
            abb.assetNames = assetNames;
            ret.Add(abb);
            pathArray.Add(outputPath + "/" + abb.assetBundleName);
        }

        dir = new DirectoryInfo(ASSET_BUNDLE_SRC_DIR + "/Audio/Bgm");
        fileList = new List<FileInfo>(dir.GetFiles("*.ogg"));
        fileList.AddRange(new List<FileInfo>(dir.GetFiles("*.mp3")));
        for (int i = 0, n = fileList.Count; i < n; i++)
        {
            FileInfo f = fileList[i];
            string[] assetNames = { ASSET_BUNDLE_SRC_DIR + "/Audio/Bgm/" + f.Name };
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = "audio/bgm/" + Path.GetFileNameWithoutExtension(f.Name);
            abb.assetNames = assetNames;
            ret.Add(abb);
            pathArray.Add(outputPath + "/" + abb.assetBundleName);
        }

        return ret;
    }


    /// <summary>
    /// 生成策劃配置的AssetBundle
    /// </summary>
    /// <param name="opt"></param>
    /// <param name="biuldTarget"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    static private AssetBundleBuild BuildConfAB(BuildAssetBundleOptions opt,
        BuildTarget biuldTarget,
        string outputPath)
    {
        const string assetbundlename = "dataconfig";

        Debug.LogFormat("處理資源: {0}", assetbundlename);
        AssetBundleBuild ret = new AssetBundleBuild();
        ret.assetBundleName = assetbundlename;

        List<string> assetNames = new List<string>();
        DirectoryInfo dInfo = new DirectoryInfo(ASSET_BUNDLE_SRC_DIR + "/Config");
        FileInfo[] fInfoArray = dInfo.GetFiles("*.xml");
        for (int i = 0, n = fInfoArray.Length; i < n; i++)
        {
            FileInfo f = fInfoArray[i];
            assetNames.Add(ASSET_BUNDLE_SRC_DIR + "/Config/" + f.Name);
        }
        ret.assetNames = assetNames.ToArray();
        return ret;
    }


    /// <summary>
    /// 解析reslist.json文件，返回其內容
    /// </summary>
    /// <param name="outputPath"></param>
    /// <param name="reslistJsonVer">配置文件版本號</param>
    /// <returns></returns>
    static private Dictionary<string, AssetBundleInfo> LoadResConf(string outputPath,
                                                             out int reslistJsonVer,
                                                             out FileInfo fileInfo)
    {
        //資源信息
        Dictionary<string, AssetBundleInfo> ret = new Dictionary<string, AssetBundleInfo>();

        //讀取現有的資源配置文件 reslist
        //先獲取當前已經打包的AssetBundle資源的信息
        //然後再這些信息的基礎上做升級
        fileInfo = new FileInfo(outputPath + "/reslist.json");
        reslistJsonVer = 0;
        if (fileInfo.Exists)
        {
            JSONInStream jInStream = new JSONInStream(File.ReadAllText(fileInfo.FullName));
            jInStream.Content("version", out reslistJsonVer)
                .List("assets",
                    delegate(int index, JSONInStream jsonInStream)
                    {
                        string assetbundlename = null;
                        int version = 0;
                        string hash = null;
                        int size = 0;
                        jsonInStream.Start(0)
                            .Content("assetbundlename", out assetbundlename)
                            .Content("version", out version)
                            .Content("size", out size)
                            .Content("hash", out hash)
                            .End();
                        //Debug.LogFormat("------- {0}, {1}", index, assetbundlename);
                        AssetBundleInfo info;
                        info.Packaged = false;
                        info.Path = assetbundlename;
                        info.Size = size;
                        info.Version = version;
                        info.Hash = hash;
                        ret.Add(assetbundlename, info);
                    }
                )
               .End();
        }
        else
        {
            reslistJsonVer = 1;
            //沒有reslist.json文件，創建文件
            if (!fileInfo.Directory.Exists)
                Directory.CreateDirectory(fileInfo.Directory.FullName);
            fileInfo.Create().Close();
        }

        return ret;
    }


    /// <summary>
    /// 保存reslist.json的內容
    /// </summary>
    /// <param name="fileInfo"></param>
    /// <param name="data"></param>
    /// <param name="reslistVersion"></param>
    static private void SaveReslist(FileInfo fileInfo,
        Dictionary<string, AssetBundleInfo> data,
        int reslistVersion)
    {
        //生成新的json文件
        JSONOutStream jOutStream = new JSONOutStream();
        jOutStream.Content("version", reslistVersion)
            .List("assets");
        foreach (var pair in data)
        {
            AssetBundleInfo info = pair.Value;
            jOutStream.Start(0)
                .Content("assetbundlename", info.Path)
                .Content("version", info.Version)
                .Content("hash", info.Hash)
                .Content("size", info.Size)
                .Content("packaged", info.Packaged ? 1 : 0)
                .End();
        }

        jOutStream.End();
        //寫入reslist.json
        File.WriteAllText(fileInfo.FullName, jOutStream.Serialize());
    }



    static private string GetAssetBundlesPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "iOS";
            case BuildTarget.WebGL:
                return "WebGL";
            case BuildTarget.WebPlayer:
                return "WebPlayer";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            default:
                return null;
        }
    }
}