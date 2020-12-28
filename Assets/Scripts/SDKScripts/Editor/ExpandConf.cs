using UnityEngine;
using System.Collections;

public class ExpandConf {

    // bugly所需权限
    public static string[] buglyPermissions = {
                                                   "android.permission.READ_PHONE_STATE",
                                                   "android.permission.INTERNET",
                                                   "android.permission.ACCESS_NETWORK_STATE",
                                                   "android.permission.ACCESS_WIFI_STATE",
                                                   "android.permission.READ_LOGS"
                                               };

    // XG Push所需权限
    public static string[] xgPushPermissions = {
                                                    "android.permission.INTERNET",
                                                    "android.permission.READ_PHONE_STATE",
                                                    "android.permission.ACCESS_WIFI_STATE",
                                                    "android.permission.ACCESS_NETWORK_STATE",
                                                    "android.permission.RECEIVE_BOOT_COMPLETED",
                                                    "android.permission.RESTART_PACKAGES",
                                                    "android.permission.BROADCAST_STICKY",
                                                    "android.permission.WRITE_SETTINGS",
                                                    "android.permission.RECEIVE_USER_PRESENT",
                                                    "android.permission.WRITE_EXTERNAL_STORAGE",
                                                    "android.permission.WAKE_LOCK",
                                                    "android.permission.KILL_BACKGROUND_PROCESSES",
                                                    "android.permission.GET_TASKS",
                                                    "android.permission.READ_LOGS",
                                                    "android.permission.VIBRATE",
                                                    "android.permission.BLUETOOTH",
                                                    "android.permission.BATTERY_STATS"
                                                };

    // bugly java code
    public static string buglyImportMark = "bugly code import";
    public static string[] buglyImportCode = {
                                                 "import com.tencent.bugly.crashreport.CrashReport;" 
                                             };

    public static string buglyInitCrashReportMark = "bugly code initCrashReport";
    public static string[] buglyInitCrashReportCode = {
                                                          "		CrashReport.initCrashReport(this, \"{0}\", true);"
                                                      };


    // XG Push java code
    public static string xgPushImportMark = "XGPush code import";
    public static string[] xgPushImportCode = {
                                                  "import android.util.Log;",
                                                  "import com.tencent.android.tpush.XGIOperateCallback;",
                                                  "import com.tencent.android.tpush.XGPushManager;"
                                              };

    public static string xgPushRegisterPushMark = "XGPush code registerPush";
    public static string[] xgPushRegisterPushCode = {
                                                        "	public static void registerPush(long actorId) {",
                                                        "		if (_gameContext == null) {",
                                                        "			return;",
                                                        "		}",
                                                        "		final String account = Long.toString(actorId);",
                                                        "		_gameContext.runOnUiThread(new Runnable() {",
                                                        "",
                                                        "			@Override",
                                                        "			public void run() {",
                                                        "				XGPushManager.registerPush(_gameContext.getApplicationContext(), account, new XGIOperateCallback() {",
                                                        "					@Override",
                                                        "					public void onSuccess(Object data, int flag) {",
                                                        "						Log.i(\"XGPushManager\", \"register push sucess. token:\" + data.toString() + \" | account:\" + account);",
                                                        "					}",
                                                        "",
                                                        "					@Override",
                                                        "					public void onFail(Object data, int errCode, String msg) {",
                                                        "						Log.w(\"XGPushManager\", \"register push fail. token:\" + data.toString() + \" | account:\" + account);",
                                                        "					}",
                                                        "				});",
                                                        "			}",
                                                        "		});",
                                                        "	}"
                                                    };
}
