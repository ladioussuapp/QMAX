using UnityEngine;
using Data;
using System.Collections.Generic;

//测试烘焙的时候遗留下的代码  废弃不用
public class BattleParse
{
    public delegate Transform PrefabSpawnDelegate(string path);
    public static PrefabSpawnDelegate prefabSpawnHandler;

    public static Transform DefaultSpawnHandler(string prefabName)
    {
        GameObject gObj = (GameObject)GameObject.Instantiate(LoadPrefab(prefabName));
        return gObj.transform;
    }

    static BattleParse()
    {
        prefabSpawnHandler = DefaultSpawnHandler;
    }

#if UNITY_EDITOR

    public static void Bake(Transform parent)
    {
//        if (LightmapSettings.lightmaps == null || LightmapSettings.lightmaps.Length == 0)
//        {
//            Debug.Log("无烘焙信息");
//            return;
//        }

//        BattleSysData sysData = ScriptableObject.CreateInstance<BattleSysData>();
//        Transform child;
//        MeshRenderer meshRenderer;
//        RendererLightmapData rendererData;
//        sysData.battleId = "1";
 
//        //lightmapData.lightmap = LightmapSettings.lightmaps;
//        sysData.rendererDatas = new Dictionary<string, RendererLightmapData>();
 
//        for (int i = 0; i < parent.childCount; i++)
//        {
//            child = parent.GetChild(i);

//            meshRenderer = child.GetComponent<MeshRenderer>();

//            if (meshRenderer != null && meshRenderer.lightmapIndex != 65535)
//            {
//                rendererData = new RendererLightmapData();
//                rendererData.lightmapIndex = meshRenderer.lightmapIndex;
//                rendererData.offset = meshRenderer.lightmapScaleOffset;
//                sysData.TestRD = rendererData;

//                if (sysData.rendererDatas.ContainsKey(meshRenderer.name))
//                {
//                    //已经存在此数据
//                    continue;
//                }
//                else
//                {
//                    sysData.rendererDatas.Add(meshRenderer.name, rendererData);
//                }
//            }
//        }

//        string fileName = "test_lightmap";
//        string path = "Assets/tmp_" + fileName + ".asset";
//        AssetDatabase.CreateAsset(sysData, path);
//        BattleSysData o = (BattleSysData)AssetDatabase.LoadAssetAtPath(path, typeof(BattleSysData));

//        //, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android
//        //过时?
//#pragma warning disable 0618
//        bool res = BuildPipeline.BuildAssetBundle(o, null, Application.dataPath + "/StreamingAssets/" + fileName + ".assetbundle", BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.Android);
//#pragma warning restore 0618
//        //AssetDatabase.DeleteAsset(path);
//        AssetDatabase.Refresh();

//        EditorUtility.DisplayDialog(res ? "保存成功" : "保存失败", path, "OK", "");
    }

    public static void Load(Transform root = null)
    {
        //string fileName = "test_lightmap";
        //AssetBundle assetBundle = AssetBundle.CreateFromFile(Application.dataPath + "/StreamingAssets/" + fileName + ".assetbundle");

        //string path = "Assets/tmp_" + fileName + ".asset";
        //BattleSysData sysData = (BattleSysData)AssetDatabase.LoadAssetAtPath(path, typeof(BattleSysData));

        //try
        //{
        //    //LightmapSettings.lightmaps = sysData.LightmapData.lightmap;

        //    Debug.Log(sysData);
        //}
        //catch (System.Exception error)
        //{
        //    EditorUtility.DisplayDialog("提示", "导入失败：" + error.Message, "OK", "");
        //}
        //finally
        //{
        //    if (assetBundle)
        //    {
        //        assetBundle.Unload(true);
        //    }
        //}
    }

    /// <summary>
    /// 只用在editor中
    /// </summary>
    /// <param name="world"></param>
    public static void Encode(BattleBehaviour world)
    {
        //EyePointsSys[] eyePoints = new EyePointsSys[world.EyePostions.Length];
        //BattleProgressSysData[] eyePointDatas = new BattleProgressSysData[world.ProgressDatas.Length];
        //BattleModelSysData[] modelSysDatas = new BattleModelSysData[world.EnemyDatas.Length];

        //int i;
        //BattleProgressSysData progressSysData;
        //EyePathData progressData;
        //EyePathData progressNextData;

        //for (i = 0; i < world.EyePostions.Length; i++)
        //{
        //    Transform t = world.EyePostions[i];
        //    EyePointsSys eyePoint = new EyePointsSys();
        //    eyePoint.position = t.position;
        //    eyePoint.direction = t.forward;
        //    eyePoints[i] = eyePoint;
        //}

        //for (i = 0; i < world.ProgressDatas.Length; i++)
        //{
        //    progressSysData = new BattleProgressSysData();
        //    progressData = world.ProgressDatas[i];
        //    progressSysData.speed = progressData.speed;
        //    progressSysData.startPointIndex = progressData.startPointIndex;

        //    if (i == world.ProgressDatas.Length - 1)
        //    {
        //        progressSysData.endPointIndex = eyePoints.Length - 1;
        //    }
        //    else
        //    {
        //        progressNextData = world.ProgressDatas[i + 1];
        //        progressSysData.endPointIndex = progressNextData.startPointIndex;
        //    }

        //    eyePointDatas[i] = progressSysData;
        //}

        //for (i = 0; i < world.EnemyDatas.Length; i++)
        //{
        //    EnemyData modelData = world.EnemyDatas[i];
        //    BattleModelSysData modelSysData = new BattleModelSysData();

        //    modelSysData.position = modelData.Enemy.position;
        //    modelSysData.direction = modelData.Enemy.forward;
        //    modelSysData.scale = modelData.Enemy.localScale;
        //    modelSysData.type = (int)modelData.type;
        //    modelSysData.prefabName = modelData.prefabName;
        //    modelSysData.name = modelData.Enemy.name;
        //    modelSysDatas[i] = modelSysData;
        //}

        //BattleSysData sysData = ScriptableObject.CreateInstance<BattleSysData>();
        //sysData.eyePointDatas = eyePointDatas;
        //sysData.eyePoints = eyePoints;
        //sysData.battleId = world.BattleId;
        //sysData.defaultSpeed = world.DefaultSpeed;
        //sysData.modelDatas = modelSysDatas;

        //string fileName = sysData.battleId;
        //string path = "Assets/tmp_" + fileName + ".asset";
        //AssetDatabase.CreateAsset(sysData, path);
        //Object o = AssetDatabase.LoadAssetAtPath(path, typeof(BattleSysData));

        ////过时?
        //BuildPipeline.BuildAssetBundle(o, null, Application.dataPath + "/StreamingAssets/" + fileName + ".assetbundle");
        //AssetDatabase.DeleteAsset(path);
        //AssetDatabase.Refresh();
    }

#endif


    public static void Decode(AssetBundle assetBundle, BattleBehaviour world)
    {
        //BattleSysData sysData = assetBundle.mainAsset as BattleSysData;

        //GameObject tmpGObj;
        //EyePointsSys eyePointsSys;
        //BattleProgressSysData progressSysData;
        //EyePathData ProgressData;
        //BattleModelSysData modelSysData;
        //EnemyData modelData;

        //world.BattleId = sysData.battleId;
        //world.EyePostions = new Transform[sysData.eyePoints.Length];
        //world.ProgressDatas = new EyePathData[sysData.eyePointDatas.Length];
        //world.EnemyDatas = new EnemyData[sysData.modelDatas.Length];
        //world.DefaultSpeed = sysData.defaultSpeed;

        //int i;

        //for (i = 0; i < sysData.eyePoints.Length; i++)
        //{
        //    eyePointsSys = sysData.eyePoints[i];

        //    //重复引用的transform会多次创建  TO DO
        //    tmpGObj = new GameObject("point" + i);
        //    tmpGObj.transform.parent = world.transform;
        //    tmpGObj.transform.position = eyePointsSys.position;
        //    tmpGObj.transform.forward = eyePointsSys.direction;
        //    tmpGObj.AddComponent<DrawPoint>();
        //    world.EyePostions[i] = tmpGObj.transform;
        //}

        //for (i = 0; i < sysData.eyePointDatas.Length; i++)
        //{
        //    progressSysData = sysData.eyePointDatas[i];
        //    ProgressData = new EyePathData();
        //    ProgressData.startPointIndex = progressSysData.startPointIndex;
        //    ProgressData.endPointIndex = progressSysData.endPointIndex;
        //    ProgressData.speed = progressSysData.speed;
        //    world.ProgressDatas[i] = ProgressData;
        //}

        ////没有进度数据，则把所有的地图点当做一个进度
        //if (world.ProgressDatas.Length == 0)
        //{
        //    world.ProgressDatas = new EyePathData[1];
        //    ProgressData = new EyePathData();
        //    ProgressData.startPointIndex = 0;
        //    ProgressData.speed = sysData.defaultSpeed;
        //    world.ProgressDatas[0] = ProgressData;
        //}

        //for (i = 0; i < sysData.modelDatas.Length; i++)
        //{
        //    modelSysData = sysData.modelDatas[i];
        //    //保持prefab的链接

        //    tmpGObj = CreatePrefabGObj(modelSysData.prefabName);
        //    tmpGObj.name = modelSysData.name;
        //    tmpGObj.transform.localScale = modelSysData.scale;
        //    tmpGObj.transform.position = modelSysData.position;
        //    tmpGObj.transform.forward = modelSysData.direction;
        //    tmpGObj.transform.parent = world.transform;
        //    modelData = new EnemyData();
        //    modelData.Enemy = tmpGObj.transform;
        //    modelData.type = (BattleModelType)modelSysData.type;
        //    world.EnemyDatas[i] = modelData;
        //}
    }

    protected static GameObject CreatePrefabGObj(string prefabName)
    {
        GameObject gObj;

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            gObj = prefabSpawnHandler(prefabName).gameObject;
        }
        else
        {
            //保持prefab的链接
            gObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(LoadPrefab(prefabName));
        }

#else
         gObj = prefabSpawnHandler(prefabName).gameObject;
#endif
        return gObj;
    }

    //prefab直接存放在Resources中?
    protected static Object LoadPrefab(string prefabName)
    {
        Object assets;

        assets = Resources.Load("Prefabs/" + prefabName, typeof(GameObject));

        return assets;
    }

    public static void CreateNew(BattleBehaviour world)
    {
        GameObject point0 = new GameObject("point0");
        point0.AddComponent<DrawPoint>();
        point0.transform.parent = world.transform;
    }

    public static void TestParse()
    {

        Debug.Log("isPlaying:" + Application.isPlaying);

#if UNITY_EDITOR
        Debug.Log("UNITY_EDITOR");
#endif

#if UNITY_STANDALONE
            Debug.Log("UNITY_STANDALONE");
#endif

#if UNITY_IPHONE
            Debug.Log("UNITY_IPHONE");
#endif

#if UNITY_ANDROID
        Debug.Log("UNITY_ANDROID");
#endif

#if UNITY_EDITOR_WIN
        Debug.Log("UNITY_EDITOR_WIN");
#endif

#if UNITY_STANDALONE_WIN
            Debug.Log("UNITY_STANDALONE_WIN");
#endif

#if UNITY_EDITOR_OSX
            Debug.Log("UNITY_EDITOR_OSX");
#endif

    }
}

