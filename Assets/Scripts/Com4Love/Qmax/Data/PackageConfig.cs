
using UnityEngine;
namespace Com4Love.Qmax.Data
{
    /// <summary>
    /// 包配置
    /// </summary>
    public class PackageConfig
    {
        /// <summary>
        /// 包版本號
        /// </summary>
        public const string Version = "1.096";

        /// <summary>
        /// 協議版本號
        /// </summary>
        public const string PROTOCOL_VER = "0.0.21";

        /// <summary>
        /// 是否使用測試狀態的CDN
        /// </summary>
        public static bool UseTestCDN = false;

        /// <summary>
        /// 語言版本
        /// </summary>
        //public const string LANGUAGE = "chs";
        //改成繁體版本
        public const string LANGUAGE = "cht";

        /// <summary>
        /// 正式環境CDN目錄
        /// </summary>
        public const string CDN_URL = "http://app1104772395.imgcache.qzoneapp.com/app1104772395";

        /// <summary>
        /// 開發環境CDN目錄
        /// </summary>
        public const string DEV_CDN_URL = "http://app1104772395.imgcache.qzoneapp.com/app1104772395/dev";

        public const string LOGIN_PORT = ":8081/";
        public const string RECHARGE_PORT = ":8080/";

        public const string HTTP_ROOT_EXTRA = "http://123.59.76.69";
        public const string HTTP_ROOT_INTRA = "http://192.168.103.11";
        public const string HTTP_LOCALHOST = "http://127.0.0.1";
        public const string HTTP_ROOT_LU = "http://192.168.103.98";
        public const string HTTP_ROOT_LIGANG = "http://192.168.103.94";

        /// 正式服務器
        public const string HTTP_BEIJING = "http://203.195.243.221";


        /// <summary>
        /// 自搭建帳號系統的登錄接口
        /// </summary>
        // public const string HTTP_LOGIN_URL = "http://user.looddy.cc:18080/platform/login.do";
        public const string HTTP_LOGIN_URL = "http://123.59.76.69:18080/platform/login.do";
        /// <summary>
        /// 自搭建帳號系統的註冊接口
        /// </summary>
        // public const string HTTP_REGISTER_URL = "http://user.looddy.cc:18080/platform/reg.do";
        public const string HTTP_REGISTER_URL = "http://123.59.76.69:18080/platform/reg.do";


        /// <summary>
        /// 讀取哪個平台的AssetBundle
        /// UNITY_EDITOR_OSX
        /// </summary>
#if UNITY_EDITOR_OSX
		public static RuntimePlatform AssetBundlePlatform = RuntimePlatform.IPhonePlayer;
#else
        public static RuntimePlatform AssetBundlePlatform = Application.platform;
        //public static RuntimePlatform AssetBundlePlatform = RuntimePlatform.Android;
#endif

        /// <summary>
        /// 是否從本地加載AssetBundle
        /// </summary>
#if UNITY_EDITOR
        public static bool IsLoadAssetBundleFromLocal = true;
#else
        public static bool IsLoadAssetBundleFromLocal = false;
#endif

        public const string YOUAI_APP_ID = "100004";
        public const string YOUAI_APP_KEY = "fba3d95640dc11e5a56152540016a8ea";

        /// <summary>
        /// 是否使用DOSDK
        /// </summary>
#if DOSDK
        public static bool DOSDK { get { return false; } }
#else
        public static bool DOSDK { get { return false; } }
#endif


        /// <summary>
        /// 使用自建的登錄系統
        /// </summary>
        public static bool BASE_LOGIN { get { return !DOSDK; } }


        /// <summary>
        /// 是否使用Bugly
        /// </summary>
#if BUGLY && UNITY_ANDROID //只有在android下使用bugly
        public static bool BUGLY { get { return true; } }
        public const string BUGLY_APP_ID = "900007773";
#else

        public static bool BUGLY { get { return false; } }

        /// <summary>
        /// iOS的Bugy App ID
        /// </summary>
        public const string BUGLY_APP_ID = "900010253";
#endif


        /// <summary>
        /// 是否自動戰鬥
        /// </summary>
#if AUTO_FIGHT
        public static bool AUTO_FIGHT { get { return true; } }
#else
        public static bool AUTO_FIGHT { get { return false; } }
#endif

        /// <summary>
        /// Log是否要寫本地
        /// </summary>
#if LOCAL_LOG && UNITY_ANDROID //只有Android支持本地log
        public static bool LOCAL_LOG { get { return true; } }
#else
        public static bool LOCAL_LOG { get { return false; } }
#endif
    }
}
