using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Collections;
using Data;
 
public class BattleEdit : Editor
{
    //棄用
    //[MenuItem("Tools/QMax/Bake")]
    //static void Bake()
    //{
    //    Transform[] selections = Selection.GetTransforms(SelectionMode.Assets);

    //    if (selections.Length == 0 || selections.Length > 1)
    //    {
    //        EditorUtility.DisplayDialog("提示", "選中一個需要烘焙的root", "OK", "");
    //        return;
    //    }

    //    BattleParse.Bake(selections[0]);
    //}

    //棄用
    //[MenuItem("Tools/QMax/LoadBakeInfo")]
    //static void LoadBakeInfo()
    //{
    //    BattleParse.Load();
    //}

    //棄用
    //[MenuItem("Tools/QMax/ExportBattleData")]
    //static void ExposeBattleData()
    //{
    //    //Transform[] selections = Selection.GetTransforms(SelectionMode.Assets);

    //    //if (selections.Length == 0)
    //    //{
    //    //    EditorUtility.DisplayDialog("提示", "選中模型所在的world", "OK", "");
    //    //    return;
    //    //}

    //    //Transform root = selections[0];
    //    //BattleBehaviour world = root.GetComponent<BattleBehaviour>();

    //    //if (!world)
    //    //{
    //    //    EditorUtility.DisplayDialog("提示", "選中的對像沒有world腳本", "OK", "");
    //    //    return;
    //    //}

    //    //foreach (EnemyData modelData in world.EnemyDatas)
    //    //{
    //    //    if (modelData.Enemy == null)
    //    //    {
    //    //        EditorUtility.DisplayDialog("提示","模型丢失！" , "ok");
    //    //    }

    //    //    Object modelPrefab = PrefabUtility.GetPrefabParent(modelData.Enemy.gameObject);

    //    //    if (!modelPrefab)
    //    //    {
    //    //        //沒有此對象的prefab  創建一個？提示？
    //    //        EditorUtility.DisplayDialog("提示", string.Format("{0} 没有对应的prefab", modelData.Enemy.name), "ok");
    //    //        Selection.activeGameObject = modelData.Enemy.gameObject;
    //    //        return;
    //    //    }

    //    //    modelData.prefabName = modelPrefab.name;
    //    //}

    //    //foreach (EyePathData eyePointsData in world.ProgressDatas)
    //    //{
    //    //    if (eyePointsData.startPointIndex >= world.EyePostions.Length)
    //    //    {
    //    //        EditorUtility.DisplayDialog("提示", "progress錯誤！", "ok");
    //    //        return;
    //    //    }
    //    //}

    //    //try
    //    //{
    //    //    BattleParse.Encode(world);
    //    //    EditorUtility.DisplayDialog("提示", "導出完成!", "ok");
    //    //}
    //    //catch (System.Exception error)
    //    //{
    //    //    EditorUtility.DisplayDialog("提示", "導出失敗：" + error.Message, "ok");
    //    //}
    //    //finally
    //    //{
    //    //    Caching.CleanCache();
    //    //}
    //}

    //棄用
    //[MenuItem("Tools/QMax/ImportBattleData")]
    //static void ImportBattleData()
    //{
    //    BattleBehaviour world = (BattleBehaviour)GameObject.FindObjectOfType(typeof(BattleBehaviour));

    //    if (world != null)
    //    {
    //        EditorUtility.DisplayDialog("提示", "World對像已存在", "ok");
    //        return;
    //    }

    //    Transform[] selections = Selection.GetTransforms(SelectionMode.Assets);
    //    GameObject worldGObj;

    //    if (selections.Length > 0)
    //    {
    //        worldGObj = selections[0].gameObject;
    //    }
    //    else
    //    {
    //       worldGObj = new GameObject("World");
    //    }

    //    world = worldGObj.AddComponent<BattleBehaviour>();
    //    Caching.CleanCache();

    //    string fileName = "test_battle1";
    //    world.StartCoroutine(loadBundle(fileName, world));
    //}

    //棄用
    //static IEnumerator loadBundle(string fileName, BattleBehaviour world)
    //{
    //    WWW www = new WWW("file://" + Application.dataPath + "/StreamingAssets/" + fileName + ".assetbundle");
    //    yield return www;

    //    if (!string.IsNullOrEmpty(www.error))
    //    {
    //        EditorUtility.DisplayDialog("提示", fileName + "不存在！", "OK", "");
    //    }

    //    AssetBundle assetBundle = www.assetBundle;

    //    try
    //    {
    //        BattleParse.Decode(assetBundle, world);
    //        EditorUtility.DisplayDialog("提示", "導入成功！", "OK", "");
    //    }
    //    catch (System.Exception error)
    //    {
    //        EditorUtility.DisplayDialog("提示", "解碼失敗：" + error.Message, "OK", "");
    //    }
    //    finally
    //    {
    //        assetBundle.Unload(true);
    //    }

    //    if (world.Camera == null)
    //    {
    //        world.Camera = Camera.main;
    //    }
    //}

    //弃用
    //[MenuItem("Tools/QMax/CreateBattle")]
    //static void CreateBattle()
    //{
    //    BattleBehaviour world = (BattleBehaviour)GameObject.FindObjectOfType(typeof(BattleBehaviour));

    //    if (world != null)
    //    {
    //        EditorUtility.DisplayDialog("提示", "World對像已存在", "ok");
    //        return;
    //    }

    //    Transform[] selections = Selection.GetTransforms(SelectionMode.Assets);
    //    GameObject worldGObj;

    //    if (selections.Length > 0)
    //    {
    //        worldGObj = selections[0].gameObject;
    //    }
    //    else
    //    {
    //        worldGObj = new GameObject("World");
    //    }

    //    world = worldGObj.AddComponent<BattleBehaviour>();
    //    world.BattleId = "NewBattle";
    //    BattleParse.CreateNew(world);
    //    Caching.CleanCache();
    //}

    const string PREFAB_PATH = "Assets/Resources/Prefabs/Map/Battle/";

    //合併了網格之後將選中的battlebehaviour自動保存起來
    //執行此
    [MenuItem("Tools/QMax/保存戰斗場景到目錄下")]
    static void CreateBattleWithCombineMesh()
    {
        if(EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("提示", "不能在運行的時候執行", "OK", "");
            return;
        }
        
        Transform[] selections = Selection.GetTransforms(SelectionMode.Assets);

        if (selections.Length == 0 || selections.Length > 1)
        {
            EditorUtility.DisplayDialog("提示", "選中一個Battle物體", "OK", "");
            return;
        }

        if (selections[0].GetComponent<BattleBehaviour>() == null)
        {
            EditorUtility.DisplayDialog("提示", "未找到BattleBehaviour腳本", "OK", "");
            return; 
        }

        Transform battleT = selections[0];
        //創建copy的gameobject
        GameObject battleCombineGo = GameObject.Instantiate<GameObject>(battleT.gameObject);
        string prefabName = battleT.name;
        string suffix = "_Raw";
        int nameFormatIndex = prefabName.LastIndexOf(suffix);

        if (nameFormatIndex == prefabName.Length - suffix.Length)
        {
            //帶了後綴，刪掉後綴就是現在的名稱
            prefabName = prefabName.Substring(0, nameFormatIndex);
        }

        battleCombineGo.name = prefabName;
        //battle才能被看見
        int layer = battleT.gameObject.layer;

        //所有怪點刪除sprite renderer
        EnemyPoint[] enemyPoints = battleCombineGo.GetComponentsInChildren<EnemyPoint>();

        for (int i = 0; i < enemyPoints.Length; i++)
        {
            SpriteRenderer[] renderers = enemyPoints[i].GetComponentsInChildren<SpriteRenderer>();

            for (int j = 0; j < renderers.Length; j++)
            {
                DestroyImmediate(renderers[i]);
            }
        }

        //有SimpleMeshCombine腳本的物體為mesh根目錄。   有可能會有多個目錄
        SimpleMeshCombine[] meshCombines = battleCombineGo.GetComponentsInChildren<SimpleMeshCombine>();
 
        for (int i = 0; i < meshCombines.Length; i++)
        {
            SimpleMeshCombine tmpMeshCombine = meshCombines[i];

            MeshRenderer[] meshRenderers = tmpMeshCombine.GetComponentsInChildren<MeshRenderer>();

            //合併模型之後其它的renderer會被關閉，複製出來後，將所有對應目錄下關閉了的renderer全部刪掉
            for (int j = 0; j < meshRenderers.Length; j++)
            {
                MeshRenderer renderer = meshRenderers[j];

                if (!renderer.enabled)
                {
                    //如果此renderer被關閉表示合併前的小模型，刪除
                    Object.DestroyImmediate(renderer.gameObject);
                }
                else
                {
                    renderer.gameObject.layer = layer;
                }
            }

            if (tmpMeshCombine != null)
            {
                //合併腳本也直接刪掉
                Object.DestroyImmediate(tmpMeshCombine);
            }
        }
 
        GameObject battleCombineGoPrefab = PrefabUtility.CreatePrefab(PREFAB_PATH + battleCombineGo.name + ".prefab", battleCombineGo);
        Object.DestroyImmediate(battleCombineGo);
        PrefabUtility.InstantiatePrefab(battleCombineGoPrefab);


        //EditorUtility.DisplayDialog("提示", "創建成功。 請重新賦值合併後的mesh", "ok");

        if (battleT)
        {
            battleT.gameObject.SetActive(false);
        }
    }
}
 