using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;


public class AggregationSDK : EditorWindow
{
    private static string libProj;
    private static string enName;
    private static string textPath = "C:\\QmaxAndroidProj";
    private static string textBundle;
    private static string textAdtVersion = "21";
    private static string textVersionCode = "5";
    private static string textVersionName = "1";
    private static string textBuildSymbol = "UNITY_ANDROID_DOSDK;LOCAL_LOG";

    private static bool isBugly = true;
    private static string textBuglyAppId = "900007773";
    private static bool isXGPush = true;
    private static string textXGPushAccessId = "2100148857";
    private static string textXGPushAccessKey = "A5USIB5Z163M";

    //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "UNITY_ANDROID_DOSDK");

    //游戏工程和渠道相关的路径
    private static string curPath;
    private static string gamePath;
    private static string platformPath;
    private static string sdkPath;

    private static Process build;

    private Boolean copyfiles;
    private static string projectName;



    [MenuItem("Youai SDK/Build Android Related")]
    static void ShowWindow()
    {
        Rect wr = new Rect(Screen.width - 500, 0, 500, 300);
        AggregationSDK window = (AggregationSDK)EditorWindow.GetWindowWithRect(typeof(AggregationSDK), wr, true, "Build Android Project");
        window.Show();
    }

    void OnGUI()
    {
        textBundle = EditorGUILayout.TextField("Bundle Identifier: ", PlayerSettings.applicationIdentifier);
        PlayerSettings.applicationIdentifier = textBundle;
        textPath = EditorGUILayout.TextField("Android Project Path: ", textPath);
        //textAdtVersion = EditorGUILayout.TextField("ADT Version: ", textAdtVersion);
        //textVersionCode = EditorGUILayout.TextField("Version Code: ", textVersionCode);
        //textVersionName = EditorGUILayout.TextField("Version Name: ", textVersionName);
        textBuildSymbol = EditorGUILayout.TextField("ScriptingDefineSymbols: ", textBuildSymbol);
        isDebugBuild = EditorGUILayout.Toggle("isDebugBuild: ", isDebugBuild);

        // Bugly控件
        isBugly = EditorGUILayout.BeginToggleGroup("Bugly", isBugly);
        textBuglyAppId = EditorGUILayout.TextField("    BuglyAppId: ", textBuglyAppId);
        EditorGUILayout.EndToggleGroup();
        // 推送控件
        isXGPush = EditorGUILayout.BeginToggleGroup("XGPush", isXGPush);
        textXGPushAccessId = EditorGUILayout.TextField("    XGPushAccessId: ", textXGPushAccessId);
        textXGPushAccessKey = EditorGUILayout.TextField("    XGPushAccessKey: ", textXGPushAccessKey);
        EditorGUILayout.EndToggleGroup();

        if (GUILayout.Button("Export Android Ant Project", GUILayout.Width(200)))
        {
            PlayerSettings.applicationIdentifier = textBundle;
            //string[] names = Application.dataPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            projectName = "GameAndroid";
            BuildAndroidProject(textPath + "/" + projectName, textPath);
        }
        // 相关说明
        EditorGUILayout.LabelField("相关说明:");
        EditorGUILayout.LabelField("  Bundle Identifier:", "android package name");
        EditorGUILayout.LabelField("  Bugly:", "集成Bugly到工程里");
        EditorGUILayout.LabelField("  XGPush:", "集成XGPush到工程里");
        EditorGUILayout.LabelField("  LOCAL_LOG:", "编译宏，开启日志文件打印（正式发布可去掉）");

        //如果想要增加打包种类，可以在这里添加按钮，并制定渠道的enName和libProj
        //if(GUILayout.Button("Build Android Apk (nd91)"))
        //{
        //	PlayerSettings.bundleIdentifier = textBundle;
        //	BuildApk ("nd91", "91SDK_LibProject");
        //}
    }

    static void BuildApk(string enName, string libProj)
    {

        string[] names = Application.dataPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        int step = 0;
        projectName = names[names.Length - 2];
        curPath = Application.dataPath + "/../../AndroidWorkSpace/";
        gamePath = Application.dataPath + "/../../AndroidWorkSpace/" + projectName;
        platformPath = Application.dataPath + "/../../AndroidWorkSpace/Platforms/" + enName;
        sdkPath = Application.dataPath + "/../../sdklibs/" + libProj;

        try
        {
            step++;	//1
            if (!CleanEnv(libProj))
            {
                return;
            }
            step++;	//2
            if (!ChangeAdtVersion(sdkPath))
            {
                return;
            }
            step++;	//3
            //BuildAndroidProject(gamePath, curPath);
            step++;	//4
            CopyPlatformRes(enName, libProj);
            step++;	//5
            CopySdkAssets(libProj, gamePath);
            step++;	//6
            MergeXML(enName, libProj, gamePath, platformPath, sdkPath);
            step++;	//7
            ConfigureProj(libProj, gamePath, sdkPath);
            step++;	//8
            ConfigurePlugins();
            step++;	//9
            ConfigureSrc(enName, gamePath);
            step++;	//10
            RunBuildRelease(gamePath);

            EditorUtility.DisplayDialog("Information", "Build Succeed " + enName, "OK");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log(ex);
        }
    }

    public static bool isDebugBuild = false;

    #region 清理StreamingAsset平台相关的资源
    static void ClearOtherPlatformAssetBundles()
    {
        //重建Temp目录，用于暂时存放其他平台的AssetBundle文件
        if (!AssetDatabase.IsValidFolder("Assets/Temp"))
        {
            AssetDatabase.DeleteAsset("Assets/Temp");
            AssetDatabase.CreateFolder("Assets", "Temp");
        }

        string srcPath = "Assets/StreamingAssets/assetbundles/Windows";
        string descPath = "Assets/Temp/Windows";
        if (AssetDatabase.IsValidFolder(srcPath))
        {
            string ret = AssetDatabase.MoveAsset(srcPath, descPath);
            if (!string.IsNullOrEmpty(ret))
                UnityEngine.Debug.LogFormat("转移AssetBundles失败:{0}", ret);
        }

        srcPath = "Assets/StreamingAssets/assetbundles/iOS";
        descPath = "Assets/Temp/iOS";
        if (AssetDatabase.IsValidFolder(srcPath))
        {
            string ret = AssetDatabase.MoveAsset(srcPath, descPath);
            if (!string.IsNullOrEmpty(ret))
                UnityEngine.Debug.LogFormat("转移AssetBundles失败:{0}", ret);
        }
    }

    static void RevertOtherPlatformAssetBundles()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Temp"))
            return;

        string descPath = "Assets/StreamingAssets/assetbundles/Windows";
        string srcPath = "Assets/Temp/Windows";
        if (AssetDatabase.IsValidFolder(srcPath))
        {
            string ret = AssetDatabase.MoveAsset(srcPath, descPath);
            if (!string.IsNullOrEmpty(ret))
                UnityEngine.Debug.LogFormat("恢复AssetBundles失败:{0}", ret);
        }

        descPath = "Assets/StreamingAssets/assetbundles/iOS";
        srcPath = "Assets/Temp/iOS";
        if (AssetDatabase.IsValidFolder(srcPath))
        {
            string ret = AssetDatabase.MoveAsset(srcPath, descPath);
            if (!string.IsNullOrEmpty(ret))
                UnityEngine.Debug.LogFormat("恢复AssetBundles失败:{0}", ret);
        }

        AssetDatabase.DeleteAsset("Assets/Temp");
    }
    #endregion
    #region 清理环境
    static bool CleanEnv(string libProj)
    {
        string tempGamePath = curPath + "GameAndroid";

        if (Directory.Exists(tempGamePath))
        {

            Helper.DeleteFolder(tempGamePath);
        }
        Helper.CopyDirectory(gamePath, tempGamePath);
        gamePath = tempGamePath;
        if (!Directory.Exists(sdkPath))
        {
            EditorUtility.DisplayDialog("Error", sdkPath + "找不到SDK依赖库，请从 https://10.0.2.100/svn/develop/Android_IOS_WP_SDK/Android/AndroidProj/sdklibs 检出，和client同级存放！", "OK");
            return false;
        }
        return true;
    }
    #endregion
    #region 修改ADT版本
    static bool ChangeAdtVersion(string sdkPath)
    {
        try
        {
            string[] files = Directory.GetFiles(sdkPath + "/");
            foreach (string file in files)
            {
                string sfx = file.Substring(file.LastIndexOf(".") + 1);
                if (".properties".Contains(sfx))
                {
                    FileStream fs = new FileStream(file, FileMode.Open);
                    byte[] bytes = new byte[fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    string fileString = System.Text.Encoding.UTF8.GetString(bytes);
                    //如果哪天adt版本在位数上发生变化，请修改正则
                    string resultString = Regex.Replace(fileString, @"target=android-\d\d", "target=android-" + textAdtVersion, RegexOptions.IgnoreCase);
                    fs.Close();
                    StreamWriter sw = File.CreateText(file);
                    sw.Write(resultString);
                    sw.Close();
                }
            }

        }
        catch
        {
            EditorUtility.DisplayDialog("Error", "Failed to modify  *.properties! in sdkProject", "OK");
            return false;
        }
        return true;
    }
    #endregion
    #region 将游戏工程放到指定位置中
    static void BuildAndroidProject(string path, string curPath)
    {
        try
        {
            ClearOtherPlatformAssetBundles();

            string clientPath = textPath + "/" + PlayerSettings.productName;
            if (Directory.Exists(path))
                Helper.DeleteFolder(path);
            if (Directory.Exists(clientPath))
                Helper.DeleteFolder(clientPath);

            BuildOptions buildOption = BuildOptions.AcceptExternalModificationsToPlayer;

            if (isDebugBuild)
            {
                buildOption |= BuildOptions.Development;
                buildOption |= BuildOptions.AllowDebugging;
                buildOption |= BuildOptions.ConnectWithProfiler;
            }

            if (isXGPush)
                textBuildSymbol += ";QMAX_XGPUSH";

            // 使用QMaxActivity
            textBuildSymbol += ";QMAX_ACTIVITY";

            string tmpSetting = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, textBuildSymbol);
            BuildPipeline.BuildPlayer(GetBuildScenes(), curPath, BuildTarget.Android, buildOption);

            Helper.CopyDirectory(clientPath, path);
            Helper.DeleteFolder(clientPath);
            Helper.CopyDirectory(Application.dataPath + "/Scripts/SDKScripts/Editor/AntRequired/", path);
            Helper.CopyDirectory(Application.dataPath + "/Scripts/SDKScripts/Editor/EclipseRequired/", path);
            //DoSDK demo的
            //Helper.CopyDirectory(Application.dataPath + "/Scripts/SDKScripts/Editor/JavaSrc/", path + "/src");
            Helper.CopyDirectory(Application.dataPath + "/Scripts/SDKScripts/Editor/extraCmd/", path + "/extraCmd");

            ExpandHelper.Expand(path, textBundle, isBugly, isXGPush, textBuglyAppId, textXGPushAccessId, textXGPushAccessKey);
            //	EditorUtility.DisplayDialog ("Information", "Build End!", "OK");

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, tmpSetting);

            RevertOtherPlatformAssetBundles();
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Information", e.Message, "OK");
        }
    }
    #endregion
    #region 拷贝渠道差异文件
    static void CopyPlatformRes(string enName, string gamePath)
    {
        if (Directory.Exists(curPath + "/Platforms/" + enName + "/ForRes"))
            Helper.CopyDirectory(curPath + "/Platforms/" + enName + "/ForRes", gamePath + "/res");
        if (Directory.Exists(curPath + "/Platforms/" + enName + "/ForAssets"))
            Helper.CopyDirectory(curPath + "/Platforms/" + enName + "/ForAssets", gamePath + "/assets");
        if (Directory.Exists(curPath + "/Platforms/" + enName + "/ForRaw"))
            Helper.CopyDirectory(curPath + "/Platforms/" + enName + "/ForRaw", gamePath + "/res/raw");
    }
    #endregion
    #region 拷贝依赖库中的文件
    static void CopySdkAssets(string libProj, string gamePath)
    {
        if (Directory.Exists(Application.dataPath + "/../../sdklibs/assets"))
            Helper.CopyDirectory(Application.dataPath + "/../../sdklibs/assets" + libProj, gamePath + "/assets");
        if (Directory.Exists(Application.dataPath + "/../../sdklibs/icon"))
            Helper.CopyDirectory(Application.dataPath + "/../../sdklibs/icon" + libProj, gamePath + "/icon");
        if (Directory.Exists(Application.dataPath + "/../../sdklibs/libs"))
            Helper.CopyDirectory(Application.dataPath + "/../../sdklibs/libs" + libProj, gamePath + "/libs");
    }
    #endregion
    #region 合并XML文件
    static void MergeXML(string enName, string libProj, string gamePath, string platformPath, string sdkPath)
    {
        //不清楚modifyPackge和BundleIdentifier会有什么关系
        string modifyPackage = PlayerSettings.applicationIdentifier + "." + enName;
        string forManifestXml = sdkPath + "/ForManifest.xml";
        string distFile = gamePath + "/AndroidManifest.xml";
        XmlHelper.createAndroidManifest(distFile, forManifestXml, distFile, modifyPackage, textVersionCode, textVersionName);

        //合并渠道和游戏的Manifest
        forManifestXml = platformPath + "/ForManifest.xml";
        XmlHelper.createAndroidManifest(distFile, forManifestXml, distFile, modifyPackage, textVersionCode, textVersionName);
        //合并渠道和游戏的String
        string originStringXml = gamePath + "/res/values/strings.xml";
        string forStringXml = platformPath + "/ForStrings.xml";
        distFile = gamePath + "/res/values/strings.xml";
        XmlHelper.createAndroidString(originStringXml, forStringXml, distFile);
        //继续合并SDK的String
        forManifestXml = platformPath + "/ForStrings.xml";
        XmlHelper.createAndroidString(distFile, forStringXml, distFile);
    }
    #endregion
    #region 修改adt.properties
    static void ConfigureProj(string libProj, string gamePath, string sdkPath)
    {
        StreamWriter sw = File.CreateText(gamePath + "/ADT.properties");
        sw.WriteLine("target=android-" + textAdtVersion);
        sw.WriteLine("android.library.reference.1=${prj.dir}../../sdklibs/" + libProj);
        sw.Close();
    }
    #endregion
    #region 配置插件
    static void ConfigurePlugins()
    {

    }
    #endregion
    #region 整理游戏工程
    static void ConfigureSrc(string enName, string gamePath)
    {
        string modifyPackage = PlayerSettings.applicationIdentifier + "." + enName;
        string originalPackage = PlayerSettings.applicationIdentifier;
        Helper.ChangeR(modifyPackage, originalPackage, gamePath);
    }
    #endregion
    #region 开始打包
    static void RunBuildRelease(string path)
    {

        build = new Process();
        build.StartInfo.FileName = path + "/build.bat";

        build.Start();

    }
    #endregion

    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }

        return names.ToArray();
    }

}
