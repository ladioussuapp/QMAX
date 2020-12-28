using UnityEngine;
using UnityEditor;
using Com4Love.Qmax;

[CustomEditor(typeof(BattlePathPoint))]
public class BattlePathPointInspector : Editor
{
    protected BattlePathPoint model;

    public override void OnInspectorGUI()
    {
        //Debug.Log("OnInspectorGUI");

        model = target as BattlePathPoint;

        GameObject cameraGo = GameObject.FindGameObjectWithTag(Tags.BattleCamera.ToString());
        cameraGo.transform.position = model.transform.position;
        cameraGo.transform.forward = model.transform.forward;
        base.OnInspectorGUI();
    }
}