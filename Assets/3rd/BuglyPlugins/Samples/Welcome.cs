using UnityEngine;
using System;
using System.Collections;
using System.Threading;

public class Welcome : MonoBehaviour
{   
	private string TextUserId;
	private string TextChannelId;
	private bool LogEnable;
	private string TextTips;
	public GUISkin defaultSkin;
    
	void InitBuglySDK(){
		// enable debug log print
		BuglyAgent.ConfigDebugMode (true);
		// Register log callback with 'BuglyAgent.LogCallbackDelegate' to replace the 'Application.RegisterLogCallback(Application.LogCallback)'
		BuglyAgent.RegisterLogCallback (CallbackDelegate.Instance.OnApplicationLogCallbackHandler);
		
		#if UNITY_IPHONE || UNITY_IOS
		BuglyAgent.InitWithAppId ("900001055");
		#elif UNITY_ANDROID
		BuglyAgent.InitWithAppId ("900001191");
		#endif

		// If you do not need call 'InitWithAppId(string)' to initialize the sdk(may be you has initialized the sdk it associated Android or iOS project),
		// please call this method to enable c# exception handler only.
		BuglyAgent.EnableExceptionHandler ();
	}

	// Use this for initialization
	void Start ()
	{

		System.Console.Write ("Welcome start");
		System.Console.WriteLine ();

		TextUserId = "Input user id";
		TextChannelId = "Input channel name";
		LogEnable = true;

		TextTips = "";

		InitBuglySDK ();
	}
    
	// Update is called once per frame
	void Update ()
	{
		// 按返回键退出应用
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
	}

	void OnDestroy ()
	{
		Debug.LogWarning ("Welcome destroy");
	}
    
	void OnGUI ()
	{
		GUI.skin = defaultSkin;
		float scale = 1.0f;
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			scale = Screen.width / 360;
		}
		//GUI.skin.button.fontSize = Convert.ToInt32(16 * scale);
        
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
        
//      GUILayout.BeginArea (new Rect((Screen.width - 280) / 2,(Screen.height - 320) / 2, 280, 320));
		GUILayout.FlexibleSpace ();

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();

		GUILayout.BeginVertical ();

		// set the user
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("User ID :", GUILayout.MinHeight (28 * scale), GUILayout.Width (80 * scale));
		TextUserId = GUILayout.TextField (TextUserId, GUILayout.MinHeight (28 * scale), GUILayout.MinWidth (160 * scale));
		GUILayout.EndHorizontal ();

		GUILayout.Space (5 * scale);
		// set the channel
		GUILayout.BeginHorizontal ();
		GUILayout.Label ("Channel ID :", GUILayout.MinHeight (28 * scale), GUILayout.Width (80 * scale));
		TextChannelId = GUILayout.TextField (TextChannelId, GUILayout.MinHeight (28 * scale), GUILayout.MinWidth (160 * scale));
		GUILayout.EndHorizontal ();

		GUILayout.Space (20 * scale);
		GUILayout.BeginHorizontal ();
		LogEnable = GUILayout.Toggle (LogEnable, "Log Trigger", GUILayout.MinHeight (28 * scale), GUILayout.Width (80 * scale));
//      GUILayout.Space (10);
		if (GUILayout.Button ("Login", GUILayout.MinHeight (28 * scale), GUILayout.MinWidth (160 * scale))) {
			if (string.IsNullOrEmpty (TextUserId) || "Input user id".Equals (TextUserId)) {
				TextTips = "Please input the user id !";
				return;
			}

			TextTips = "";

			if (string.IsNullOrEmpty (TextChannelId) || "Input channel name".Equals (TextChannelId)) {
				TextChannelId = "channel_bugly_test";
			}

			BuglyAgent.SetUserId (TextUserId);

			Debug.Log ("Login with user");

			Application.LoadLevel (Application.loadedLevel + 1);
		}
        
		GUILayout.EndHorizontal ();

		GUILayout.Label (TextTips, GUILayout.MinHeight (48 * scale));
		GUILayout.EndVertical ();

		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();

		GUILayout.FlexibleSpace ();
//      GUILayout.EndArea ();
        
		GUILayout.EndArea ();
	}
}