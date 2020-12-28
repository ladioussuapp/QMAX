using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using DoPlatform;


namespace DoPlatform
{
	public enum SharePlatform : int
	{
		FACEBOOK = 1,
		
		LINE = 2,

		WECHAT = 3
		
	};

	public class DoShare 
    {
		//程序运行时创建一个静态只读的辅助对象  
		public static readonly DoShare Instance = new DoShare();  
		
		private DoShare() { }   
		
	
		/**
		 * 
		 * 
		 */
		public void share(SharePlatform platform,string appID, string title, string description,string picFilePath)
        {
#if UNITY_EDITOR
			Debug.Log("share");
#elif UNITY_ANDROID
			Debug.Log("shareUnity "+platform+appID+ title+ description+picFilePath);

			DoShareJava.CallStatic("shareUnity",(int)platform, appID, title, description,picFilePath,activity);
#elif UNITY_IPHONE
			iosDoShare((int)platform, appID, title, description,  picFilePath);
#else

#endif
        }
#if UNITY_EDITOR

#elif UNITY_ANDROID
		const string mJavaClass = "com.sdk.share.DoShare";

		static AndroidJavaClass DoShareJava = new AndroidJavaClass(mJavaClass);
		
		static AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");

#elif UNITY_IPHONE

        [DllImport("__Internal")]
		private static extern void iosDoShare(int shareType, string appID, string title, string description, string imagePath);

#endif
    }
}
