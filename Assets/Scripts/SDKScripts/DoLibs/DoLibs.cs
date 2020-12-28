using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using DoPlatform;


namespace DoPlatform
{
    public class DoLibs 
    {
		private DoLibs(){}
		public static readonly DoLibs Instance = new DoLibs();
        const string mJavaClass = "com.com4loves.android.util.YouaiComm";
        public string getNetType()
        {
#if UNITY_EDITOR
			return "getDoNetType";
#elif UNITY_ANDROID
            return callStringJavaMethod("getNetType");
#elif UNITY_IPHONE
            return _getNetType();
#else
            return "getDoNetType";
#endif
        }
        public string getMacAddress()
        {
#if UNITY_EDITOR
			return "getDoMacAddress";
#elif UNITY_ANDROID
            return callStringJavaMethod("getMacAddress");
#elif UNITY_IPHONE
            return _getMacAddress();
#else
            return "getDoMacAddress";
#endif
        }

        public string getLang()
        {
#if UNITY_EDITOR

			return "getLang";

#elif UNITY_ANDROID
            return callStringJavaMethod("getLang");
#elif UNITY_IPHONE
            return _getCurrentLanguage();
#else
            return "getLang";
#endif
        }
        public string getSystemType()
        {
#if UNITY_EDITOR
			return "getDoSystemType";
#elif UNITY_ANDROID
            return callStringJavaMethod("getSystemType");
#elif UNITY_IPHONE
            return "ios";
#else
            return "getDoSystemType";
#endif
        }
        public string getIMEi()
        {
#if UNITY_EDITOR
			return "getIMEi";

#elif UNITY_ANDROID
            return callStringJavaMethod("getIMEi");
#elif UNITY_IPHONE
            return _getIMEI();
#else
            return "getIMEi";
#endif
        }
        public string getPhoneVersion()
        {
#if UNITY_EDITOR
			return "getDoPhoneVersion";
#elif UNITY_ANDROID
            return callStringJavaMethod("getVersion");
#elif UNITY_IPHONE
            return _getVersion();
#else
            return "getDoPhoneVersion";
#endif
        }
        public string getModel()
        {
#if UNITY_EDITOR
			return "getDoModel";
#elif UNITY_ANDROID
            return callStringJavaMethod("getModel");
#elif UNITY_IPHONE
            return _getModel();
#else
            return "getDoModel";
#endif
        }
        public string getBundleID()
        {
#if UNITY_EDITOR
			return "getDoBundleID";
#elif UNITY_ANDROID
            return callStringJavaMethod("getBundleId");
#elif UNITY_IPHONE
            return _getBundleId();
#endif
        }

        public void addNotification(string pTitle, string pMsg, int pHour,int pMinute)
        {
#if UNITY_EDITOR
			
			Debug.Log("-------- pTitle" + pTitle + pHour.ToString()+":"+pMinute);
#elif UNITY_ANDROID
            AndroidJavaClass youaiComm = new AndroidJavaClass(mJavaClass);
            AndroidJavaObject javaInstance = youaiComm.CallStatic<AndroidJavaObject>("getInstance");
            youaiComm.Dispose();
            AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
            player.Dispose();
			javaInstance.Call("addNotification", context, pTitle, pMsg, pHour,pMinute);
            context.Dispose();
            javaInstance.Dispose();
#elif UNITY_IPHONE
			_addNotificationTiming(pTitle, pMsg, pHour,pMinute);
#endif
        }
        public void cleanNotification()
        {
#if UNITY_EDITOR
			
#elif UNITY_ANDROID           
			callVoidJavaMethod("cleanNotification");
#elif UNITY_IPHONE
            _cleanNotification();
#endif
        }


        public string callStringJavaMethod(string methodName)
        {
#if UNITY_EDITOR
			Debug.Log("callStringJavaMethod("+methodName+")");
			return "callStringJavaMethod("+methodName+")";
#elif UNITY_ANDROID
            AndroidJavaClass youaiComm = new AndroidJavaClass(mJavaClass);
            AndroidJavaObject javaInstance = youaiComm.CallStatic<AndroidJavaObject>("getInstance");
            youaiComm.Dispose();
            AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
            player.Dispose();
            string ret = javaInstance.Call<string>(methodName, context);
            context.Dispose();
            javaInstance.Dispose();
            return ret;
#else

            return "";
#endif

        }

        public void callVoidJavaMethod(string methodName)
		{
#if UNITY_EDITOR


#elif UNITY_ANDROID
            AndroidJavaClass youaiComm = new AndroidJavaClass(mJavaClass);
            AndroidJavaObject javaInstance = youaiComm.CallStatic<AndroidJavaObject>("getInstance");
            youaiComm.Dispose();
            AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = player.GetStatic<AndroidJavaObject>("currentActivity");
            player.Dispose();
            javaInstance.Call(methodName, context);
            context.Dispose();
            javaInstance.Dispose();
#endif
        }

        public void gotoRecommendApp(string appString)
        {
#if UNITY_EDITOR

#elif UNITY_IPHONE       
            _gotoRecommendApp(appString);
            #endif
        }

#if UNITY_EDITOR

 #elif UNITY_IPHONE

        [DllImport("__Internal")]
        public static extern string _getNetType();

        [DllImport("__Internal")]
        public static extern string _getModel();

        [DllImport("__Internal")]
        public static extern string _getVersion();

        [DllImport("__Internal")]
        public static extern string _cleanNotification();
        
        [DllImport("__Internal")]
        public static extern string _getMacAddress();

        [DllImport("__Internal")]
        public static extern string _getIMEI();     //get idfa

        [DllImport("__Internal")]
        public static extern string _getCurrentLanguage(); //get language

        [DllImport("__Internal")]
        public static extern void _addNotificationDelay(string title,string message,int time);

        [DllImport("__Internal")]
        public static extern void _addNotificationTiming(string title,string message,int hour,int minute);         //add push  time为毫秒

        [DllImport("__Internal")]
        public static extern void _gotoRecommendApp(string appUrl);

         [DllImport("__Internal")]
        public static extern string _getBundleId();
        
#endif
    }
}
