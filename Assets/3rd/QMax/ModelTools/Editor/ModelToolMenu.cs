using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class ModelSearchMenu : Editor
{
    [MenuItem("Tools/QMax/根據材質分組子物體")]
    static void GroupModelsByMatialsMenu()
    {
        Transform[] selections = Selection.GetTransforms(SelectionMode.Assets);

        if (selections.Length == 0 || selections.Length > 1)
        {
            EditorUtility.DisplayDialog("提示", "在場景中選擇一個查找的root節點", "OK", "");
            return;
        }

        GroupModelsByMatials(selections[0]);
    }

    static void GroupModelsByMatials(Transform root)
    {
        Dictionary<Material, List<Transform>> transfomDict = new Dictionary<Material, List<Transform>>();

        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            Material[] m = renderer.sharedMaterials;
 
            if (m == null || m.Length > 1)
            {
                continue;
            }

            if (!transfomDict.ContainsKey(m[0]))
            {
                List<Transform> group = new List<Transform>();
                transfomDict.Add(m[0], group);
            }

            transfomDict[m[0]].Add(renderer.transform);
        }

        foreach (KeyValuePair<Material, List<Transform>> item in transfomDict)
        {
            GameObject go = new GameObject();

            if (item.Key != null && item.Key.name != "")
            {
                go.name = item.Key.name;
            }
            
            go.transform.SetParent(root);

            for (int j = 0; j < item.Value.Count; j++)
            {
                Transform t = item.Value[j];
                t.SetParent(go.transform);
            }
        }
    }
}
