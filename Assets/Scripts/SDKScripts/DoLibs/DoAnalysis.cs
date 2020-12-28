using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using DoPlatform;
using System.Collections.Generic;

namespace DoPlatform
{
	public enum EventType : int {
		DoLogin = 0,
		DoRegiste = 1,
		DoPay = 2,
		DoLevel = 3,
		DoCustom = 4,
		DoEventBegain = 5,
		DoEventEnd = 6,
        DoBeforePay = 7,
        DoSetup = 8
	};
	
	public class DoAnalysis 
    {
        public static string kEventParamAnalysisSetupChannel 		= "AnalysisSetupChannel";
        public static string kEventParamAnalysisRegistAccountId 	= "AnalysisRegistAccountId";
        public static string kEventParamAnalysisRegistRoleId 		= "AnalysisRegistRoleId";
        public static string kEventParamAnalysisRegistRoleLevel 	= "AnalysisRegistRoleLevel";
        public static string kEventParamAnalysisLoginAccountId 		= "AnalysisLoginAccountId";

        public static string kEventParamAnalysisLoginRoleId 		= "AnalysisLoginRoleId";
        public static string kEventParamAnalysisLoginRoleLevel 		= "AnalysisLoginRoleLevel";

        public static string kEventParamAnalysisBeforePayAccountId 	= "AnalysisBeforePayAccountId";
        public static string kEventParamAnalysisBeforePayRoleId 	= "AnalysisBeforePayRoleId";
        public static string kEventParamAnalysisBeforePayRoleLevel 	= "AnalysisBeforePayRoleLevel";
        public static string kEventParamAnalysisBeforePayPaymentAmount = "AnalysisBeforePayPaymentAmount";

        public static string kEventParamAnalysisPayAccountId 		= "AnalysisPayAccountId";
        public static string kEventParamAnalysisPayRoleId 			= "AnalysisPayRoleId";
        public static string kEventParamAnalysisPayRoleLevel	 	= "AnalysisPayRoleLevel";
        public static string kEventParamAnalysisCurrency 			= "AnalysisCurrency";
        public static string kEventParamAnalysisPrice 				= "AnalysisPrice";
        public static string kEventParamAnalysisValue 				= "AnalysisValue";
		public static string  kEventParamAnalysisLevel      		=  "AnalysisLevel";

        public static string kEventParamAnalysisLevelUpAccountId 	= "AnalysisLevelUpAccountId";
        public static string kEventParamAnalysisLevelUpRoleId 		= "AnalysisLevelUpRoleId";
        public static string kEventParamAnalysisLevelUpRoleLevel 	= "AnalysisLevelUpRoleLevel";


		//程序运行时创建一个静态只读的辅助对象  
		public static readonly DoAnalysis Instance = new DoAnalysis();  
		
		private DoAnalysis() { }   
#if UNITY_EDITOR

#elif UNITY_ANDROID
		const string mJavaClass = "com.sdk.unitylibs.DosdkUnitylibs";
		static AndroidJavaClass DoAnalysisJava = new AndroidJavaClass(mJavaClass);
		
		static AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
#endif
		/**
		 * 
		 * 
		 */
		public void onEvent(Dictionary<string,string> pEvent, EventType type)
        {
			Debug.LogWarning("----> DoAnalysis onEvent pEvnent =  "+pEvent+"  type = "+type);

#if UNITY_EDITOR
#elif UNITY_ANDROID
			if(type==EventType.DoRegiste){
				DoAnalysisJava.CallStatic("registerEvent");

			}else if(type==EventType.DoLevel){
				if (pEvent.ContainsKey(kEventParamAnalysisLevel))
				{
					string value = pEvent[kEventParamAnalysisLevel];
					DoAnalysisJava.CallStatic("levelChange",int.Parse(value));
				}
			}else if(type==EventType.DoLogin){
				DoAnalysisJava.CallStatic("loginEvent");

			}else{
				Debug.LogWarning("----> DoAnalysis Android don't contain event "+type);
			}
#elif UNITY_IPHONE
			_doAnalysisEventWithType(dictionaryToString(pEvent),(int)type);
#else

#endif
        }


#if UNITY_EDITOR
#elif UNITY_ANDROID
		/***/
		private static AndroidJavaObject dictionaryToJavaHashMap(Dictionary<string, string> dic)
		{
			var hashMap = new AndroidJavaObject("java.util.HashMap");
			var putMethod = AndroidJNIHelper.GetMethodID(hashMap.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
			var arguments = new object[2];
			foreach (var entry in dic)
			{
				using (var key = new AndroidJavaObject("java.lang.String", entry.Key))
				{
					using (var val = new AndroidJavaObject("java.lang.String", entry.Value))
					{
						arguments[0] = key;
						arguments[1] = val;
						AndroidJNI.CallObjectMethod(hashMap.GetRawObject(), putMethod, AndroidJNIHelper.CreateJNIArgArray(arguments));
					}
				} // end using
			} // end foreach
			
			return hashMap;
		}

#endif

#if UNITY_EDITOR

#elif UNITY_IPHONE
		[DllImport("__Internal")]
		static extern void _doAnalysisEventWithType (string eventParm, int type);

		public static string dictionaryToString(Dictionary<string,string> eventValues){
			string attributesString = "";
			foreach(KeyValuePair<string, string> kvp in eventValues)
			{
				attributesString += kvp.Key + "=" + kvp.Value + "\n";
			}
			return attributesString;
		}
#endif
	}
}
