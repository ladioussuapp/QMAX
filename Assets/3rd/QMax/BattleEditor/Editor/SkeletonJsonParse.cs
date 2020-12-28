using System.Collections;
using UnityEditor;
using Spine;
using System.IO;
using System.Collections.Generic;
using System;
using Data;
using System.Runtime.Serialization.Formatters.Binary;

//測試spine 的json配置解析的代碼。  未使用。
public class SkeletonJsonParser : Editor
{


    //[MenuItem("Tools/QMax/SkeletonJsonParse")]
    static void Parse()
    {

    }


    //[MenuItem("Tools/QMax/SkeletonJsonCheck")]
    static void Check()
    {
        //string path = "Assets/Spine/Unit/Ape1/Ape1_SkeletonData.asset";
        string path2 = "Assets/Spine/Unit/Ape1/BattleSysData.asset";

        //SkeletonDataAsset assets = (SkeletonDataAsset)AssetDatabase.LoadAssetAtPath(path, typeof(SkeletonDataAsset));


        BattleSysData assetsTest = (BattleSysData)AssetDatabase.LoadAssetAtPath(path2, typeof(BattleSysData));

        if (assetsTest == null)
        {
            assetsTest = BattleSysData.CreateInstance<BattleSysData>();
            assetsTest.TestScriptableDict = new ScriptableDictionary();
            assetsTest.TestScriptableDict.Add("aa", 888);
            AssetDatabase.CreateAsset(assetsTest, path2);
            AssetDatabase.SaveAssets();
        }


        //UnityEngine.Debug.Log(assets);
    }

    protected static IEnumerator CheckDict(Dictionary<string, object> dict)
    {
        foreach (object val in dict.Values)
        {
            yield return 0;

            //ScriptableDictionary sDict = new ScriptableDictionary();

            if (val is string)
            {
                UnityEngine.Debug.Log("string:" + val);
            }
            else if (val is Array)
            {
                UnityEngine.Debug.Log("Array");
            }
            else if (val is List<object>)
            {
                UnityEngine.Debug.Log("List");
                CheckList(val as List<object>);
            }
            else if (val is Dictionary<string, object>)
            {
                UnityEngine.Debug.Log("Dictionary");
                CheckDict(val as Dictionary<string, object>);
            }
            else
            {
                UnityEngine.Debug.Log("other:" + val);
            }
        } 
 
    }

    protected static void CheckList(List<object> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            object val = list[i];

            if (val is string)
            {
                UnityEngine.Debug.Log("string");
            }
            else if (val is Dictionary<string,object>)
            {
                UnityEngine.Debug.Log("Dictionary");
                CheckDict(val as Dictionary<string, object>);
            }
            else
            {
                UnityEngine.Debug.Log("other:" + val);
            }
        }
    }

    static void TestCheck()
    {

    }
}
