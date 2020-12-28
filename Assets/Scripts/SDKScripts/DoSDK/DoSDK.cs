using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace DoPlatform
{

    public enum Callback : int
    {
        INIT = 1,

        LOGIN = 2,

        LOGOUT = 3,

        EXIT = 4,

        PAY = 5,

        SWITCH_ACCOUNT = 6
    };

    public enum ResultStatus
    {

        FINISHED = 200,

        FINISHED_SWITCH_EXISTS_LOGIN = 201,

        FINISHED_SWITCH_LOGIN = 202,

        ERROR_INIT_FAILURE = 301,

        ERROR_PAY_ORDER_NOT_NULL = 302,

        ERROR_PAY_AMOUNT_NOT_NULL = 303,

        ERROR_PAY_PRODUCT_ID_NOT_NULL = 304,

        ERROR_PAY_PRODUCT_NAME_NOT_NULL = 305,

        ERROR_PAY_PAYDES_NOT_NULL = 306,

        ERROR_PAY_FAILURE = 307,

        ERROR_LOGIN_FAILURE = 400,

        ERROR_LOGOUT_FAILURE = 500,

        ERROR_SWITCH_ACCOUNT_FAILURE = 600
    };

    public class DoSDK
    {
        private DoSDK() { }
        public static readonly DoSDK Instance = new DoSDK();

        public delegate void DoSDKCallback(Callback callback, ResultStatus resultStatus);
        public void setAppkeys(string appKey, string appId)
        {
#if UNITY_EDITOR || UNITY_STANDALONE

#elif UNITY_ANDROID
            DoSDKJava.CallStatic("setAppKeys", appKey, appId, activity);
#elif UNITY_IPHONE
			iosSetAppKey(appKey,appId);
#endif
        }
        public void setInitCallback(DoSDKCallback callback)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("setInitCallback");
#elif UNITY_ANDROID
            Debug.Log("setInitCallback");
			var javaCallback = new CallbackListener(callback);
			DoSDKJava.CallStatic("setInitListener", javaCallback);
#elif UNITY_IPHONE
			initCallback = callback;
			iosSetInitListener(iOSInitCallback);
#endif
        }
        public void setDebug(bool isDebug)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("setDebug");
#elif UNITY_ANDROID
			Debug.Log("setInitCallback");
			DoSDKJava.CallStatic("setDebug", isDebug);
#elif UNITY_IPHONE
			iosSetDebug(isDebug);
#endif
        }

        public void setExitCallback(DoSDKCallback callback)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("setExitCallback");
#elif UNITY_ANDROID
            Debug.Log("setExitCallback");

			var javaCallback = new CallbackListener(callback);
			DoSDKJava.CallStatic("setExitListener", javaCallback);
#elif UNITY_IPHONE
			exitCallback = callback;
			iosSetExitListener(iOSExitCallback);
#endif
        }
        public void setLoginCallback(DoSDKCallback callback)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("setLoginCallback");
#elif UNITY_ANDROID 
            Debug.Log("setLoginCallback");

			var javaCallback = new CallbackListener(callback);
			DoSDKJava.CallStatic("setLoginListener", javaCallback);
#elif UNITY_IPHONE
			loginCallback = callback;
			iosSetLoginListener(iOSLoginCallback);
#endif
        }
        public void setLogoutCallback(DoSDKCallback callback)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("setLogoutCallback");

#elif UNITY_ANDROID     
            Debug.Log("setLogoutCallback");
       
			var javaCallback = new CallbackListener(callback);
			DoSDKJava.CallStatic("setLogoutListener", javaCallback);
#elif UNITY_IPHONE
			logoutCallback = callback;
			iosSetLogoutListener(iOSLogoutCallback);
#endif
        }
        public void setPayCallback(DoSDKCallback callback)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("setPayCallback");

#elif UNITY_ANDROID   
            Debug.Log("setPayCallback");
         
			var javaCallback = new CallbackListener(callback);
			DoSDKJava.CallStatic("setPayListener",javaCallback);
#elif UNITY_IPHONE
			payCallback = callback;
			iosSetPayListener(iOSPayCallback);
#endif
        }
        public void setSwitchAccountListener(DoSDKCallback callback)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("setSwitchAccountListener");

#elif UNITY_ANDROID   
            Debug.Log("setSwitchAccountListener");

			var javaCallback = new CallbackListener(callback);

			DoSDKJava.CallStatic("setSwitchAccountListener",javaCallback);
#elif UNITY_IPHONE
			iosSetSwitchUserAccountListener(iOSSwitchCallback);
#endif
        }
        public void initDoSDK()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("initDoSDK");

#elif UNITY_ANDROID
            Debug.Log("initDoSDK");

            DoSDKJava.CallStatic("initDoSDK");
#elif UNITY_IPHONE
			iosInitDoSDK();
#endif
        }

        public void login()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("login");

#elif UNITY_ANDROID 
            Debug.Log("login");

            DoSDKJava.CallStatic("login");
#elif UNITY_IPHONE
			iosLogin();
#endif
        }

        public void logout()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("logout");
#elif UNITY_ANDROID         
            Debug.Log("logout");			
   
            DoSDKJava.CallStatic("logout");
#elif UNITY_IPHONE
			iosLogout();
#endif
        }

        public void switchAccount()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("switchAccount");
#elif UNITY_ANDROID           
            Debug.Log("switchAccount");						

            DoSDKJava.CallStatic("switchAccount");
#elif UNITY_IPHONE
			iosSwitchUserAccount();
#endif
        }

        public void pay(string orderId, string prouductId, string productName, string amount, string paydes)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("调用充值接口pay");
#elif UNITY_ANDROID            
            Debug.Log("调用充值接口pay");

            DoSDKJava.CallStatic("pay", orderId, prouductId, productName, amount, paydes);
#elif UNITY_IPHONE
			iosPay(orderId,prouductId,productName,amount,paydes);
#endif
        }

        public void exit()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("exit");

#elif UNITY_ANDROID
            Debug.Log("exit");

            DoSDKJava.CallStatic("exit");
#elif UNITY_IPHONE
			iosExit();
#endif
        }

        public bool isLogin()
        {
            return null != token();
        }

        public string token()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || IS_INNER_PACKAGE
            return "pc_token___pc_token";
#elif UNITY_ANDROID          

            return DoSDKJava.CallStatic<string>("token");
            
          
#elif UNITY_IPHONE
			return iosToken();
#endif
        }



        public void extendData(string jsonStr)
        {
#if UNITY_EDITOR || UNITY_STANDALONE

#elif UNITY_ANDROID         
			DoSDKJava.CallStatic("extendData", jsonStr);
#elif UNITY_IPHONE
			iosExtendData(jsonStr);
#endif
        }

        public void showFloatBtn(bool isShow)
        {
#if UNITY_EDITOR || UNITY_STANDALONE

#elif UNITY_ANDROID         
			DoSDKJava.CallStatic("showFloatBtn", isShow);
#elif UNITY_IPHONE
			iosShowFloatBtn(isShow);
#endif
        }

        public void enterPlatform()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            Debug.Log("进入平台");
#elif UNITY_ANDROID        
            Debug.Log("enterPlatform");            
			DoSDKJava.CallStatic("enterPlatform");
#elif UNITY_IPHONE
			iosEnterPlatform();
#endif
        }

        public string version()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return "pc_version";
#elif UNITY_ANDROID         
			return DoSDKJava.CallStatic<string>("version");
#elif UNITY_IPHONE
			return iosVersion();
#endif
        }

        public string channel()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return "pc_channel";
#elif UNITY_ANDROID      
			return DoSDKJava.CallStatic<string>("channel");
#elif UNITY_IPHONE
			return iosChannel();
#endif
        }

        public string ad()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return "pc_ad";
#elif UNITY_ANDROID           
			return DoSDKJava.CallStatic<string>("ad");
#elif UNITY_IPHONE
			return iosAd();
#endif
        }

        public string ext()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return "{}";
#elif UNITY_ANDROID            
			return DoSDKJava.CallStatic<string>("ext");
#elif UNITY_IPHONE 
			return iosExt();
#endif
        }


#if UNITY_EDITOR || UNITY_STANDALONE
#elif UNITY_ANDROID


        delegate void Action();
        static void Run(Action action)
        {
            activity.Call("runOnUiThread", new AndroidJavaRunnable(action));
        }

        class CallbackListener : AndroidJavaProxy
        {
            public CallbackListener(DoSDKCallback Delegate)
                : base("com.sdk.unitylibs.UnityCallbackBridge")
            {
                this.dosdkcallback = Delegate;
            }

            DoSDKCallback dosdkcallback = null;
            public void callback(int callback, int result)
            {
                dosdkcallback((Callback)callback, (ResultStatus)result);
            }
        }
        static AndroidJavaClass DoSDKJava = new AndroidJavaClass("com.sdk.unitylibs.DosdkUnitylibs");

        static AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
#endif

#if UNITY_IPHONE

        static DoSDKCallback initCallback = null;
		static DoSDKCallback loginCallback = null;
		static DoSDKCallback exitCallback = null;
		static DoSDKCallback switchUserCallback = null;
		static DoSDKCallback logoutCallback = null;
		static DoSDKCallback payCallback = null;


		[AOT.MonoPInvokeCallback(typeof(DoSDKCallback))]
		static void iOSInitCallback( Callback callback, ResultStatus result)
		{
			if (initCallback != null)
				initCallback((Callback)callback, (ResultStatus)result);
		}

		[AOT.MonoPInvokeCallback(typeof(DoSDKCallback))]
		static void iOSLoginCallback(Callback callback, ResultStatus result)
		{
			if (loginCallback != null)
				loginCallback((Callback)callback, (ResultStatus)result);
		}

		[AOT.MonoPInvokeCallback(typeof(DoSDKCallback))]
		static void iOSExitCallback( Callback callback, ResultStatus result)
		{
			if (exitCallback != null)
				exitCallback((Callback)callback, (ResultStatus)result);
		}

		[AOT.MonoPInvokeCallback(typeof(DoSDKCallback))]
		static void iOSSwitchCallback(Callback callback, ResultStatus result)
		{
			if (switchUserCallback != null)
				switchUserCallback((Callback)callback, (ResultStatus)result);
		}

		[AOT.MonoPInvokeCallback(typeof(DoSDKCallback))]
		static void iOSLogoutCallback( Callback callback, ResultStatus result)
		{
			if (logoutCallback != null)
				logoutCallback((Callback)callback, (ResultStatus)result);
		}

		[AOT.MonoPInvokeCallback(typeof(DoSDKCallback))]
		static void iOSPayCallback(Callback callback, ResultStatus result)
		{
			if (payCallback != null)
				payCallback((Callback)callback, (ResultStatus)result);
		}

        [DllImport("__Internal")]
        static extern void iosSetAppKey(string appKey,string appId);
        
		[DllImport("__Internal")]
		static extern void iosInitDoSDK();
		
		[DllImport("__Internal")]
		static extern void iosLogin();
		
		[DllImport("__Internal")]
		static extern void iosLogout();
		
		[DllImport("__Internal")]
		static extern void iosSwitchUserAccount();
		
		[DllImport("__Internal")]
		static extern void iosExit();
		
		[DllImport("__Internal")]
		static extern void iosPay(string orderId, string prouductId, string productName, string amount, string paydes);

		[DllImport("__Internal")]
		static extern void iosSetInitListener(DoSDKCallback callback);
		
		[DllImport("__Internal")]
		static extern void iosSetLoginListener(DoSDKCallback callback);
		
		[DllImport("__Internal")]
		static extern void iosSetLogoutListener(DoSDKCallback callback);
		
		[DllImport("__Internal")]
		static extern void iosSetSwitchUserAccountListener(DoSDKCallback callback);
		
		[DllImport("__Internal")]
		static extern void iosSetExitListener(DoSDKCallback callback);
		
		[DllImport("__Internal")]
		static extern void iosSetPayListener(DoSDKCallback callback);
		


        [DllImport("__Internal")]
		static extern void iosEnterPlatform();
        
        [DllImport("__Internal")]
        static extern string iosToken();
                
        [DllImport("__Internal")]
        static extern void iosShowFloatBtn(bool isShow);
                
        [DllImport("__Internal")]
        static extern string iosChannel();
                
        [DllImport("__Internal")]
        static extern string iosAd();
                
        [DllImport("__Internal")]
        static extern string iosExt();
                
        [DllImport("__Internal")]
        static extern void iosExtendData(string jsonStr);
		
		[DllImport("__Internal")]
		static extern string iosVersion();
		
		[DllImport("__Internal")]
		static extern string iosSetDebug(bool isDebug);


#endif
    }

}
