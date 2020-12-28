using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using Spine;
using System.Collections.Generic;
using Com4Love.Qmax.Data.Config;

public class SpineEditorEx : AssetPostprocessor
{
    [MenuItem("Qmax/Build All Units")]
    static void BuildAllUnits()
    {
        DirectoryInfo dInfo = new DirectoryInfo("Assets/ExternalRes/Unit");
        if (!dInfo.Exists)
        {
            Debug.Log("不存在目录");
            return;
        }
        DirectoryInfo assetDire = new DirectoryInfo(".");

        //读取配置文件
        Dictionary<string, UnitConfig> UnitConfigs = new Dictionary<string, UnitConfig>();
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath("Assets/ExternalRes/Config/UnitConfig.xml", typeof(TextAsset)) as TextAsset;
        XMLInStream stream = new XMLInStream(textAsset.text);
        stream.List("item", delegate(XMLInStream itemStream)
        {
            UnitConfig ufg = new UnitConfig(itemStream);
            if (!UnitConfigs.ContainsKey(ufg.ResourceIcon))
                UnitConfigs.Add(ufg.ResourceIcon, ufg);
        });

        DirectoryInfo[] unitsDires = dInfo.GetDirectories();
        for (int i = 0, n = unitsDires.Length; i < n; i++)
        {
            dInfo = unitsDires[i];
            FileInfo[] fileInfoArr = dInfo.GetFiles("*_SkeletonData.asset");
            for (int j = 0, m = fileInfoArr.Length; j < m; j++)
            {
                FileInfo fInfo = fileInfoArr[j];
                //Debug.Log(fInfo.FullName);
                //Debug.Log(fInfo.Name);
                string relativePath = fInfo.FullName.Replace(assetDire.FullName, "");
                relativePath = relativePath.Remove(0, 1);
                //Debug.Log("relativePath=" + relativePath);
                Object o = AssetDatabase.LoadAssetAtPath(relativePath, typeof(Object));

                if (o == null)
                    Debug.LogWarning("null");
                string guid = AssetDatabase.AssetPathToGUID(relativePath);
                string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

                if (!UnitConfigs.ContainsKey(dInfo.Name))
                {
                    Debug.LogFormat("{0}，没有相关配置，忽略", dInfo.Name);
                    continue;
                }

                Debug.LogFormat("正在处理 name={0}", dInfo.Name);
                UnitConfig ufg = UnitConfigs[dInfo.Name];

                try
                {
                    if (ufg.isEnemy)
                        InstantiateSkeletonEnemy((SkeletonDataAsset)o, skinName);
                    else
                        InstantiateSkeletonUnit((SkeletonDataAsset)o, skinName);
                }
                catch (System.Exception e)
                {
                    Debug.LogFormat("{0} 处理失败，重新处理。msg={1}", dInfo.Name, e.Message);
                    j--;
                }
                SceneView.RepaintAll();
            }
        }
    }

    [MenuItem("Assets/Spine/Instantiate (Enemy.json)")]
    static void InstantiateSkeletonEnemy()
    {
        Object[] arr = Selection.objects;
        foreach (Object o in arr)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
            string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

            InstantiateSkeletonEnemy((SkeletonDataAsset)o, skinName);
            SceneView.RepaintAll();
        }
    }

    [MenuItem("Assets/Spine/Instantiate (Enemy.json)", true)]
    static bool ValidateInstantiateSkeletonEnemy()
    {
        Object[] arr = Selection.objects;
        if (arr.Length == 0)
            return false;

        if (!File.Exists("Assets/ExternalRes/Unit/Enemy.json"))
            return false;

        foreach (Object o in arr)
        {
            if (o.GetType() != typeof(SkeletonDataAsset))
                return false;
        }
        return true;
    }

    public static Object InstantiateSkeletonEnemy(SkeletonDataAsset skeletonDataAsset, string skinName)
    {
        return InstantiateSkeletonEnemy(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
    }

    public static Object InstantiateSkeletonEnemy(SkeletonDataAsset skeletonDataAsset, Skin skin = null)
    {
        if (!File.Exists("Assets/ExternalRes/Unit/Enemy.json"))
            return null;

        JSONNode jsonConfig = null;
        using (FileStream fs = new FileStream("Assets/ExternalRes/Unit/Enemy.json", FileMode.Open))
        {
            string sr = new StreamReader(fs).ReadToEnd();
            JSONParser parser = new JSONParser();
            jsonConfig = parser.Parse(new FlashCompatibleTextReader(sr));

        }

        //读取配置文件
        Dictionary<string, UnitConfig> UnitConfigs = new Dictionary<string, UnitConfig>();

        TextAsset textAsset = AssetDatabase.LoadAssetAtPath("Assets/ExternalRes/Config/UnitConfig.xml", typeof(TextAsset)) as TextAsset;
        XMLInStream stream = new XMLInStream(textAsset.text);
        stream.List("item", delegate(XMLInStream itemStream)
        {
            UnitConfig ufg = new UnitConfig(itemStream);
            if (!UnitConfigs.ContainsKey(ufg.ResourceIcon))
                UnitConfigs.Add(ufg.ResourceIcon, ufg);
        });

        string path = AssetDatabase.GetAssetPath(skeletonDataAsset);
        string fpath = path.Replace("_SkeletonData", "_Controller").Replace(".asset", ".controller");
        string mpath = path.Replace("_SkeletonData", "_Controller").Replace(".asset", ".controller.meta");
        if (File.Exists(fpath))
        {
            File.Delete(fpath);
            File.Delete(mpath);
            Debug.Log("删除旧的Controller:" + fpath);
        }
        fpath = path.Replace("_SkeletonData", "").Replace(".asset", ".prefab");
        mpath = path.Replace("_SkeletonData", "").Replace(".asset", ".prefab.meta");
        if (File.Exists(fpath))
        {
            File.Delete(fpath);
            File.Delete(mpath);
            Debug.Log("删除旧的Prefab:" + fpath);
        }

        skeletonDataAsset.controller = null;

        // 创建状态机
        SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset, jsonConfig);
        // 创建Animator Object
        GameObject go = GenerateAnimatorObject(skeletonDataAsset, skin);
        Transform tf = go.GetComponent<Transform>();
        tf.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        // 伙伴UnitSoundBehaviour
        UnitSoundBehaviour usb = go.AddComponent<UnitSoundBehaviour>();
        // 创建Audio Object
        string name = path.Replace("Assets/ExternalRes/Unit/", "").Replace("_SkeletonData.asset", "");
        name = name.Substring(name.IndexOf("/") + 1);
        if (UnitConfigs.ContainsKey(name)) {
            usb.SetSoundSource(AutoPrefab.GenerateAudioObject(go, UnitConfigs[name]));
        }
        // 创建Prefab
        string dataPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
        string prefabPath = dataPath.Replace("_SkeletonData", "").Replace(".asset", ".prefab");
        Object prefab = AutoPrefab.GenerateUnitPrefab(go, prefabPath);
        // 销毁Animator Object
        Object.DestroyImmediate(go);
        //设置asset bundle name
        AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(prefab)).assetBundleName = "units/" + name;
        return prefab;
    }

    [MenuItem("Assets/Spine/Instantiate (Unit.json)")]
    static void InstantiateSkeletonUnit()
    {
        Object[] arr = Selection.objects;
        foreach (Object o in arr)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
            string skinName = EditorPrefs.GetString(guid + "_lastSkin", "");

            InstantiateSkeletonUnit((SkeletonDataAsset)o, skinName);
            SceneView.RepaintAll();
        }
    }

    [MenuItem("Assets/Spine/Instantiate (Unit.json)", true)]
    static bool ValidateInstantiateSkeletonUnit()
    {
        Object[] arr = Selection.objects;
        if (arr.Length == 0)
            return false;

        if (!File.Exists("Assets/ExternalRes/Unit/Unit.json"))
            return false;

        foreach (Object o in arr)
        {
            if (o.GetType() != typeof(SkeletonDataAsset))
                return false;
        }
        return true;
    }

    public static Object InstantiateSkeletonUnit(SkeletonDataAsset skeletonDataAsset, string skinName)
    {
        return InstantiateSkeletonUnit(skeletonDataAsset, skeletonDataAsset.GetSkeletonData(true).FindSkin(skinName));
    }

    public static Object InstantiateSkeletonUnit(SkeletonDataAsset skeletonDataAsset, Skin skin = null)
    {
        if (!File.Exists("Assets/ExternalRes/Unit/Unit.json"))
            return null;

        JSONNode jsonConfig = null;
        using (FileStream fs = new FileStream("Assets/ExternalRes/Unit/Unit.json", FileMode.Open))
        {
            string sr = new StreamReader(fs).ReadToEnd();
            JSONParser parser = new JSONParser();
            jsonConfig = parser.Parse(new FlashCompatibleTextReader(sr));
        }

        //读取配置文件
        Dictionary<string, UnitConfig> UnitConfigs = new Dictionary<string, UnitConfig>();
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath("Assets/ExternalRes/Config/UnitConfig.xml", typeof(TextAsset)) as TextAsset;
        XMLInStream stream = new XMLInStream(textAsset.text);
        stream.List("item", delegate(XMLInStream itemStream)
        {
            UnitConfig ufg = new UnitConfig(itemStream);
            if (!UnitConfigs.ContainsKey(ufg.ResourceIcon))
                UnitConfigs.Add(ufg.ResourceIcon, ufg);
        });

        string path = AssetDatabase.GetAssetPath(skeletonDataAsset);
        string fpath = path.Replace("_SkeletonData", "_Controller").Replace(".asset", ".controller");
        string mpath = path.Replace("_SkeletonData", "_Controller").Replace(".asset", ".controller.meta");
        if (File.Exists(fpath)) {
            File.Delete(fpath);
            File.Delete(mpath);
            Debug.Log("删除旧的Controller:" + fpath);
        }
        fpath = path.Replace("_SkeletonData", "").Replace(".asset", ".prefab");
        mpath = path.Replace("_SkeletonData", "").Replace(".asset", ".prefab.meta");
        if (File.Exists(fpath))
        {
            File.Delete(fpath);
            File.Delete(mpath);
            Debug.Log("删除旧的Prefab:" + fpath);
        }

        skeletonDataAsset.controller = null;
        // 创建状态机
        SkeletonBaker.GenerateMecanimAnimationClips(skeletonDataAsset, jsonConfig);
        // 创建Animator Object
        GameObject go = GenerateAnimatorObject(skeletonDataAsset, skin);
        // 伙伴RectTransform
        go.AddComponent<RectTransform>();
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0);
        // 伙伴UnitSoundBehaviour
        UnitSoundBehaviour usb = go.AddComponent<UnitSoundBehaviour>();

        //伙伴UnitIdleChangeBehaviour
        go.AddComponent<UnitIdleChangeBehaviour>();
        // 创建Audio Object
        string name = path.Replace("Assets/ExternalRes/Unit/", "").Replace("_SkeletonData.asset", "");
        name = name.Substring(name.IndexOf("/") + 1);
        if (UnitConfigs.ContainsKey(name))
        {
            usb.SetSoundSource(AutoPrefab.GenerateAudioObject(go, UnitConfigs[name]));
        }
        // 创建Prefab
        string dataPath = AssetDatabase.GetAssetPath(skeletonDataAsset);
        string prefabPath = dataPath.Replace("_SkeletonData", "").Replace(".asset", ".prefab");
        Object prefab = AutoPrefab.GenerateUnitPrefab(go, prefabPath);
        // 销毁Animator Object
        Object.DestroyImmediate(go);
        // 设置asset bundle name
        AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(prefab)).assetBundleName = "units/" + name;
        return prefab;
    }

    private static GameObject GenerateAnimatorObject(SkeletonDataAsset skeletonDataAsset, Skin skin = null)
    {
        GameObject go = new GameObject(skeletonDataAsset.name.Replace("_SkeletonData", ""), typeof(MeshFilter), typeof(MeshRenderer), typeof(Animator), typeof(SkeletonAnimator));

        go.GetComponent<Animator>().runtimeAnimatorController = skeletonDataAsset.controller;

        SkeletonAnimator anim = go.GetComponent<SkeletonAnimator>();
        anim.skeletonDataAsset = skeletonDataAsset;

        bool requiresNormals = false;

        foreach (AtlasAsset atlasAsset in anim.skeletonDataAsset.atlasAssets)
        {
            foreach (Material m in atlasAsset.materials)
            {
                if (m.shader.name.Contains("Lit"))
                {
                    requiresNormals = true;
                    break;
                }
            }
        }

        anim.calculateNormals = requiresNormals;

        SkeletonData data = skeletonDataAsset.GetSkeletonData(true);

        if (data == null)
        {
            for (int i = 0; i < skeletonDataAsset.atlasAssets.Length; i++)
            {
                string reloadAtlasPath = AssetDatabase.GetAssetPath(skeletonDataAsset.atlasAssets[i]);
                skeletonDataAsset.atlasAssets[i] = (AtlasAsset)AssetDatabase.LoadAssetAtPath(reloadAtlasPath, typeof(AtlasAsset));
            }

            data = skeletonDataAsset.GetSkeletonData(true);
        }

        if (skin == null)
            skin = data.DefaultSkin;

        if (skin == null)
            skin = data.Skins.Items[0];

        anim.Reset();

        anim.skeleton.SetSkin(skin);
        anim.initialSkinName = skin.Name;

        anim.skeleton.Update(1);
        anim.skeleton.UpdateWorldTransform();
        anim.LateUpdate();

        go.AddComponent<ResendAnimEventBehaviour>();
        return go;
    }

    [MenuItem("Qmax/Copy To Resources")]
    static void CopyToRes()
    {
        DirectoryInfo fromDire = new DirectoryInfo("Assets/ExternalRes/Unit");
        if (!fromDire.Exists)
        {
            return;
        }
        DirectoryInfo toDire = new DirectoryInfo("Assets/Resources/Prefabs/Units");
        if (!toDire.Exists)
        {
            toDire.Create();
            return;
        }

        DirectoryInfo[] unitsDires = fromDire.GetDirectories();
        for (int i = 0, n = unitsDires.Length; i < n; i++)
        {
            DirectoryInfo dInfo = unitsDires[i];
            FileInfo[] fileInfoArr = dInfo.GetFiles("*.prefab");
            for (int j = 0, m = fileInfoArr.Length; j < m; j++)
            {
                FileInfo fileInfo = fileInfoArr[j];
                fileInfo.CopyTo("Assets/Resources/Prefabs/Units/" + fileInfo.Name, true);
                Debug.Log("copy " + fileInfo.Name + " to Assets/Resources/Prefabs/Units/" + fileInfo.Name);
            }
        }
    }
}
