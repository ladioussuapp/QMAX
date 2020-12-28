using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FMOD_StudioEventEmitter))]
public class FMODEventEmitterInspector : Editor
{
    bool showParameters;


    public override void OnInspectorGUI()
    {
        FMOD_StudioEventEmitter emitter = (FMOD_StudioEventEmitter)target;
        var ev = serializedObject.FindProperty("asset");
        EditorGUI.BeginChangeCheck();
        //emitter.asset = (FMODAsset)EditorGUILayout.ObjectField("Asset:", emitter.asset, typeof(FMODAsset), false);
        EditorGUILayout.PropertyField(ev, new GUIContent("Asset:"));
        if (EditorGUI.EndChangeCheck())
        {
            emitter.asset = (FMODAsset)ev.objectReferenceValue;
            // Note: set path to guid just in case the asset gets deleted
            emitter.path = (emitter.asset != null) ? emitter.asset.id : "";
            UpdateParamsOnEmmitter(serializedObject, emitter.path);
        }

        string id = "";
        string path = "";
        bool is3D = false;
        float minDistance = 0, maxDistance = 0;
        FMOD.Studio.EventDescription desc = null;
        if (emitter.asset != null)
        {
            id = emitter.asset.id;
            path = emitter.asset.path;

            desc = FMODEditorExtension.GetEventDescription(id);
            if (desc != null)
            {
                desc.is3D(out is3D);
                desc.getMinimumDistance(out minDistance);
                desc.getMaximumDistance(out maxDistance);
            }
        }
        EditorGUILayout.LabelField("Path:", path, GUILayout.Height(14));
        EditorGUILayout.LabelField("GUID:", id, GUILayout.Height(14));
        
        GUILayout.Label(is3D ? "3D" : "2D");
        if (is3D)
        {
            GUILayout.Label("Distance: (" + minDistance + " - " + maxDistance + ")");
        }

        bool isDirty = false;
        {
            bool oldIsMusic = emitter.isMusic;
            emitter.isMusic = GUILayout.Toggle(oldIsMusic, "IsMusic");
            isDirty = isDirty || (oldIsMusic != emitter.isMusic);

            bool oldState = emitter.startEventOnAwake;
            emitter.startEventOnAwake = GUILayout.Toggle(oldState, "Start Event on Awake");
            isDirty = isDirty || (oldState != emitter.startEventOnAwake);
        }

        if (isDirty)
            EditorUtility.SetDirty(emitter);

        showParameters = EditorGUILayout.Foldout(showParameters, "Parameters");
        var param = serializedObject.FindProperty("Params");
        if (showParameters && param.arraySize > 0 && desc != null)
        {
            for (int i = 0; i < param.arraySize; i++)
            {
                var parami = param.GetArrayElementAtIndex(i);
                var nameProperty = parami.FindPropertyRelative("name");
                var minProperty = parami.FindPropertyRelative("min");
                var maxProperty = parami.FindPropertyRelative("max");
                var valueProperty = parami.FindPropertyRelative("value");

                int count = 0;
                bool isHas = false;
                desc.getParameterCount(out count);
                for (int j = 0; j < count; j++)
                {
                    FMOD.Studio.PARAMETER_DESCRIPTION parameter;
                    desc.getParameterByIndex(j, out parameter);
                    if (parameter.name == nameProperty.stringValue)
                    {
                        isHas = true;
                        break;
                    }
                }
                if (!isHas)
                {
                    param.DeleteArrayElementAtIndex(i);
                    i--;
                    continue;
                }
                EditorGUILayout.Slider(valueProperty, minProperty.floatValue, maxProperty.floatValue, "[" + nameProperty.stringValue + "]");
            }
        }
        serializedObject.ApplyModifiedProperties();
    }


    public static void UpdateParamsOnEmmitter(SerializedObject serializedObject, string id)
    {
        var param = serializedObject.FindProperty("Params");
        if (param == null)
        {
            return;
        }

        param.ClearArray();

        FMOD.Studio.EventDescription desc = FMODEditorExtension.GetEventDescription(id);
        if (desc == null)
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }

        int count = 0;
        desc.getParameterCount(out count);
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                FMOD.Studio.PARAMETER_DESCRIPTION parameter;
                desc.getParameterByIndex(i, out parameter);

                param.InsertArrayElementAtIndex(0);
                var parami = param.GetArrayElementAtIndex(0);
                parami.FindPropertyRelative("name").stringValue = parameter.name;
                parami.FindPropertyRelative("min").floatValue = parameter.minimum;
                parami.FindPropertyRelative("max").floatValue = parameter.maximum;
                parami.FindPropertyRelative("value").floatValue = 0;
            }
        }
    }

}
