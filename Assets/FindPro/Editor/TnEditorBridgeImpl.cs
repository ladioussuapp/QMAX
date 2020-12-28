using UnityEngine;
using System.Collections;
using UnityEditor;
using Tiinoo.Editor;

[InitializeOnLoad]
public class TnEditorBridgeImpl
{
	static TnEditorBridgeImpl()
	{
		TnEditorBridge.IsUnityPro = IsUnityPro;
		TnEditorBridge.IsUnityProSkin = IsUnityProSkin;
		TnEditorBridge.GetDisplayName = GetDisplayName;
	}

	public static bool IsUnityPro()
	{
		#if UNITY_PRO_LICENSE
		return true;
		#else
		return false;
		#endif
	}

	public static bool IsUnityProSkin()
	{
		return EditorGUIUtility.isProSkin;
	}

	public static string GetDisplayName(SerializedProperty p)
	{
		#if UNITY_PRO_LICENSE
		return p.displayName;
		#else
		return p.name;
		#endif
	}
}
