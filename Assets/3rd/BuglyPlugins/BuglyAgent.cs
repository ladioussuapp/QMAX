// ----------------------------------------
//
//  BuglyAgent.cs
//
//  Author:
//       Yeelik, <bugly@tencent.com>
//
//  Copyright (c) 2015 Bugly, Tencent. All rights reserved.
//
// ----------------------------------------
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using System.Runtime.InteropServices;

using System;
using Com4Love.Qmax;

// We dont use the LogType enum in Unity as the numerical order doesnt suit our purposes
/// <summary>
/// Log severity. 
/// { Log, LogDebug, LogInfo, LogWarning, LogAssert, LogError, LogException }
/// </summary>
public enum LogSeverity
{
    Log,
    LogDebug,
    LogInfo,
    LogWarning,
    LogAssert,
    LogError,
    LogException
}

/// <summary>
/// Bugly agent.
/// </summary>
public sealed class BuglyAgent
{

    public delegate void LogCallbackDelegate(string condition, string stackTrace, LogType type);

    /// <summary>
    /// Init sdk with the specified appId. 
    /// <para>This will initialize sdk to report native exception such as obj-c, c/c++, java exceptions, and also enable c# exception handler to report c# exception logs</para>
    /// </summary>
    /// <param name="appId">App identifier.</param>
    public static void InitWithAppId(string appId)
    {
        if (IsInitialized)
        {
            DebugLog(null, "BuglyAgent has already been initialized.");

            return;
        }

        if (string.IsNullOrEmpty(appId))
        {
            return;
        }

        // init the sdk with app id
        InitUnityAgent(appId);

        // Register the LogCallbackHandler by Application.RegisterLogCallback(Application.LogCallback)
        _RegisterExceptionHandler();
    }

    /// <summary>
    /// Only Enable the C# exception handler. 
    /// 
    /// <para>
    /// You can call it when you do not call the 'InitWithAppId(string)', but you must make sure initialized the sdk in elsewhere, 
    /// such as the native code in associated Android or iOS project.
    /// </para>
    /// 
    /// <para>
    /// Default Level is <c>LogError</c>, so the LogError, LogException will auto report.
    /// </para>
    /// 
    /// <para>
    /// You can call the method <code>BuglyAgent.ConfigAutoReportLogLevel(LogSeverity)</code>
    /// to change the level to auto report if you known what are you doing.
    /// </para>
    /// 
    /// </summary>
    public static void EnableExceptionHandler()
    {
        if (IsInitialized)
        {
            DebugLog(null, "BuglyAgent has already been initialized.");
            return;
        }

        PrintLog(LogSeverity.Log, "Only enable the exception handler, please make sure you has initialized the sdk in the native code in associated Android or iOS project.");

        // Register the LogCallbackHandler by Application.RegisterLogCallback(Application.LogCallback)
        _RegisterExceptionHandler();
    }

    /// <summary>
    /// Registers the log callback handler. 
    /// 
    /// If you need register logcallback using Application.RegisterLogCallback(LogCallback),
    /// you can call this method to replace it.
    /// 
    /// <para></para>
    /// </summary>
    /// <param name="handler">Handler.</param>
    public static void RegisterLogCallback(LogCallbackDelegate handler)
    {
        if (handler != null)
        {
            DebugLog(null, "Add log callback handler");

            LogCallbackEventHandler += handler;
        }
    }


    /// <summary>
    /// Reports the exception.
    /// </summary>
    /// <param name="e">E.</param>
    /// <param name="message">Message.</param>
    public static void ReportException(System.Exception e, string message)
    {
        if (!IsInitialized)
        {
            return;
        }

        DebugLog(null, "Report exception: {0}\n------------\n{1}\n------------", message, e);
        //Log to file
        Q.LogFile(String.Format("Exception: {0}\n------------\n{1}\n------------", String.Format(message, e)));
        //
        _HandleException(e, message, false);
    }

    /// <summary>
    /// Reports the exception.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="message">Message.</param>
    /// <param name="stackTrace">Stack trace.</param>
    public static void ReportException(string name, string message, string stackTrace)
    {
        if (!IsInitialized)
        {
            return;
        }

        DebugLog(null, "Report exception: {0} {1} \n{2}", name, message, stackTrace);
        //Log to file
        Q.LogFile(String.Format("Exception: {0} {1} \n{2}", name, message, stackTrace));

        _HandleException(LogSeverity.LogException, name, message, stackTrace, false);
    }

    /// <summary>
    /// Unregisters the log callback.
    /// </summary>
    /// <param name="handler">Handler.</param>
    public static void UnregisterLogCallback(LogCallbackDelegate handler)
    {
        if (handler != null)
        {
            DebugLog(null, "Remove log callback handler");

            LogCallbackEventHandler -= handler;
        }
    }

    /// <summary>
    /// Sets the user identifier.
    /// </summary>
    /// <param name="userId">User identifier.</param>
    public static void SetUserId(string userId)
    {
        if (!IsInitialized)
        {
            return;
        }
        DebugLog(null, "Set user id: {0}", userId);

        SetUserInfo(userId);
    }

    /// <summary>
    /// Sets the scene.
    /// </summary>
    /// <param name="sceneId">Scene identifier.</param>
    public static void SetScene(int sceneId)
    {
        if (!IsInitialized)
        {
            return;
        }
        DebugLog(null, "Set scene: {0}", sceneId);

        SetCurrentScene(sceneId);
    }

    /// <summary>
    /// Adds the scene data.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <param name="value">Value.</param>
    public static void AddSceneData(string key, string value)
    {
        if (!IsInitialized)
        {
            return;
        }

        DebugLog(null, "Add scene data: [{0}, {1}]", key, value);

        AddKeyAndValueInScene(key, value);
    }

    /// <summary>
    /// Configs the debug mode.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> debug mode.</param>
    public static void ConfigDebugMode(bool enable)
    {
        EnableDebugMode(enable);
    }

    /// <summary>
    /// Configs the auto quit application.
    /// </summary>
    /// <param name="autoQuit">If set to <c>true</c> auto quit.</param>
    public static void ConfigAutoQuitApplication(bool autoQuit)
    {
        _autoQuitApplicationAfterReport = autoQuit;
    }

    /// <summary>
    /// Configs the auto report log level. Default is LogSeverity.LogError.
    /// <example>
    /// LogSeverity { Log, LogDebug, LogInfo, LogWarning, LogAssert, LogError, LogException }
    /// </example>
    /// </summary>
    /// 
    /// <param name="level">Level.</param> 
    public static void ConfigAutoReportLogLevel(LogSeverity level)
    {
        _autoReportLogLevel = level;
    }

    /// <summary>
    /// Configs the default.
    /// </summary>
    /// <param name="channel">Channel.</param>
    /// <param name="version">Version.</param>
    /// <param name="user">User.</param>
    /// <param name="delay">Delay.</param>
    public static void ConfigDefault(string channel, string version, string user, long delay)
    {
        ConfigDefaultBeforeInit(channel, version, user, delay);
    }

    /// <summary>
    /// Logs the debug.
    /// </summary>
    /// <param name="tag">Tag.</param>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void DebugLog(string tag, string format, params object[] args)
    {
        if (string.IsNullOrEmpty(format))
        {
            return;
        }

        Console.WriteLine("[BuglyAgent] <{0}> - {1}", tag, string.Format(format, args));
    }

    /// <summary>
    /// Prints the log.
    /// </summary>
    /// <param name="level">Level.</param>
    /// <param name="format">Format.</param>
    /// <param name="args">Arguments.</param>
    public static void PrintLog(LogSeverity level, string format, params object[] args)
    {
        if (string.IsNullOrEmpty(format))
        {
            return;
        }

        LogToConsole(level, string.Format(format, args));
    }

#if UNITY_EDITOR || UNITY_STANDALONE

    #region Interface(Empty) in Editor
    private static void InitUnityAgent(string appId)
    {
    }

    private static void ConfigDefaultBeforeInit(string channel, string version, string user, long delay)
    {
    }

    private static void EnableDebugMode(bool enable)
    {
    }

    private static void SetUserInfo(string userInfo)
    {
    }

    private static void ReportException(int type, string name, string message, string stackTrace, bool quitProgram)
    {
    }

    private static void SetCurrentScene(int sceneId)
    {
    }

    private static void AddKeyAndValueInScene(string key, string value)
    {
    }

    private static void ReportAttachmentWithException(string log)
    {
    }

    private static void ReportExtrasWithException(string key, string value)
    {
    }

    private static void LogToConsole(LogSeverity level, string message)
    {
    }
    #endregion

#elif UNITY_ANDROID
//	#if UNITY_ANDROID

    #region Interface for Android
	private static readonly string CLASS_UNITYAGENT = "com.tencent.bugly.unity.UnityAgent";

	private static AndroidJavaObject _unityAgent;

	public static AndroidJavaObject UnityAgent {
	
		get {
			if (_unityAgent == null) {
				using (AndroidJavaClass clazz = new AndroidJavaClass(CLASS_UNITYAGENT)) {
					_unityAgent = clazz.CallStatic<AndroidJavaObject> ("getInstance");
				}
			}
			return _unityAgent;
		}

	}

	private static string _configChannel;
	private static string _configVersion;
	private static string _configUser;
	private static long _configDelayTime;

	private static void ConfigDefaultBeforeInit(string channel, string version, string user, long delay){
		_configChannel = channel;
		_configVersion = version;
		_configUser = user;
		_configDelayTime = delay;
	}

	private static void InitUnityAgent (string appId)
	{
		if (IsInitialized) {
			return;
		}

		try {
			UnityAgent.Call("initWithConfiguration", appId, _configChannel, _configVersion, _configUser, _configDelayTime);

			_isInitialized = true;
		} catch {

		}
	}

	private static void EnableDebugMode(bool enable){
		_debugMode = enable;

		try {
			UnityAgent.Call ("setLogEnable", enable);
		} catch {

		}
	}

	private static void SetUserInfo(string userInfo){

		try {
			UnityAgent.Call ("setUserId", userInfo);
		} catch {
		}
	}

	private static void ReportException (int type, string name, string reason, string stackTrace, bool quitProgram)
	{
		try {
			UnityAgent.Call ("traceException", name, reason, stackTrace, quitProgram);
		} catch {

		}
	}
	
	private static void SetCurrentScene(int sceneId) {
		try {
			UnityAgent.Call ("setScene", sceneId);
		} catch {
			
		}
	}

	private static void AddKeyAndValueInScene(string key, string value){
		try {
			UnityAgent.Call ("addSceneValue", key, value);
		} catch {
			
		}
	}

	private static void ReportAttachmentWithException(string log){
		// no impl
	}

	private static void ReportExtrasWithException(string key, string value){
		// no impl
	}

	private static void LogToConsole(LogSeverity level, string message){
	
		if (!_debugMode && LogSeverity.Log != level) {
			if (level < LogSeverity.LogInfo) {
				return;
			}
		}

		try {
			UnityAgent.Call ("printLog", string.Format ("[BuglyAgent] <{0}> - {1}", level.ToString (), message));
		} catch {
			
		}
	}

    #endregion

#elif UNITY_IPHONE || UNITY_IOS

    #region Interface for iOS
	private static void ConfigDefaultBeforeInit(string channel, string version, string user, long delay){

		try {
			__setChannel (channel);
			__setBundleVersion (version);
			__setUserId (user);
		} catch {

		}
	}
	
	private static void EnableDebugMode(bool enable){
		_debugMode = enable;

		__enableLog (enable);
	}

	private static void InitUnityAgent (string appId)
	{
		__init (appId);
	}
	
	private static void SetUserInfo(string userInfo){
		__setUserId (userInfo);
	}
	
	private static void ReportException (int type, string name, string reason, string stackTrace, bool quitProgram)
	{
		__setCrashAutoThrow (quitProgram);
		
		__reportException (name, reason, stackTrace);
	}

	private static void SetCurrentScene(int sceneId) {
		// no impl
	}

	private static void AddKeyAndValueInScene(string key, string value){
		// no impl
	}

	private static void ReportAttachmentWithException(string log){
		// no impl
	}

	private static void ReportExtrasWithException(string key, string value){
		// mo impl
	}

	private static void LogToConsole(LogSeverity level, string message){
		if (!_debugMode && LogSeverity.Log != level) {
			if (level < LogSeverity.LogInfo) {
				return;
			}
		}

		UnityEngine.Debug.LogWarning (string.Format("[BuglyAgent] <{0}> - {1}", level.ToString(), message));
	}

	// --- dllimport start ---
	[DllImport("__Internal")]
	private static extern void __enableLog (bool enable);
	
	[DllImport("__Internal")]
	private static extern bool __init (string appId);
	
	[DllImport("__Internal")]
	private static extern void __setUserId (string userid);
	
	[DllImport("__Internal")]
	private static extern void __setChannel (string channel);
	
	[DllImport("__Internal")]
	private static extern void __setBundleVersion (string version);
	
	[DllImport("__Internal")]
	private static extern void __setUserData (string key, string value);
	
	[DllImport("__Internal")]
	private static extern void __reportException (string errorClass, string errorMessage, string stackTrace);
	
	[DllImport("__Internal")]
	private static extern void __setDeviceId (string deviceId);
	
	[DllImport("__Internal")]
	private static extern void __setBundleId (string bundleId);
	
	[DllImport("__Internal")]
	private static extern void __setCrashAutoThrow (bool autoThrow);
	// dllimport end
    #endregion

#endif

    #region Privated Fields and Methods
    private static event LogCallbackDelegate LogCallbackEventHandler;

    private static bool _isInitialized = false;
    private static LogSeverity _autoReportLogLevel = LogSeverity.LogError;

#pragma warning disable 414
    private static bool _debugMode = false;
    private static bool _autoQuitApplicationAfterReport = false;
    private static readonly int EXCEPTION_TYPE_UNCAUGHT = 1;
    private static readonly int EXCEPTION_TYPE_CAUGHT = 2;
    private static readonly string _pluginVersion = "1.2.5";

    public static string PluginVersion
    {
        get { return _pluginVersion; }
    }

    public static bool IsInitialized
    {
        get { return _isInitialized; }
    }

    public static bool AutoQuitApplicationAfterReport
    {
        get { return _autoQuitApplicationAfterReport; }
    }

    private static void _RegisterExceptionHandler()
    {
        try
        {
            // hold only one instance 
            AppDomain.CurrentDomain.UnhandledException += _OnUncaughtExceptionHandler;
            //Application.RegisterLogCallback (_OnLogCallbackHandler);
            Application.logMessageReceived += _OnLogCallbackHandler;

            _isInitialized = true;

            PrintLog(LogSeverity.Log, "Register the log callback");
        }
        catch
        {

        }
    }

    private static void _UnregisterExceptionHandler()
    {
        try
        {
            System.AppDomain.CurrentDomain.UnhandledException -= _OnUncaughtExceptionHandler;
            //Application.RegisterLogCallback (null);
            Application.logMessageReceived -= _OnLogCallbackHandler;

            PrintLog(LogSeverity.Log, "Unregister the log callback");
        }
        catch
        {

        }
    }

    private static void _OnLogCallbackHandler(string condition, string stackTrace, LogType type)
    {
        if (LogCallbackEventHandler != null)
        {
            LogCallbackEventHandler(condition, stackTrace, type);
        }

        if (!IsInitialized)
        {
            return;
        }

        if (!string.IsNullOrEmpty(condition) && condition.Contains("[BuglyAgent] <Log>"))
        {
            return;
        }

        if (_uncaughtAutoReportOnce)
        {
            return;
        }

        // convert the log level
        LogSeverity logLevel = LogSeverity.Log;
        switch (type)
        {
            case LogType.Exception:
                logLevel = LogSeverity.LogException;
                break;
            case LogType.Error:
                logLevel = LogSeverity.LogError;
                break;
            case LogType.Assert:
                logLevel = LogSeverity.LogAssert;
                break;
            case LogType.Warning:
                logLevel = LogSeverity.LogWarning;
                break;
            case LogType.Log:
                logLevel = LogSeverity.LogDebug;
                break;
            default:
                break;
        }

        if (LogSeverity.Log == logLevel)
        {
            return;
        }

        PrintLog(LogSeverity.Log, "[{0}] {1}\n{2}", type.ToString(), condition, stackTrace);

        _HandleException(logLevel, null, condition, stackTrace, true);
    }

    private static void _OnUncaughtExceptionHandler(object sender, System.UnhandledExceptionEventArgs args)
    {
        if (args == null || args.ExceptionObject == null)
        {
            return;
        }

        try
        {
            if (args.ExceptionObject.GetType() != typeof(System.Exception))
            {
                return;
            }
        }
        catch
        {
            if (UnityEngine.Debug.isDebugBuild == true)
            {
                UnityEngine.Debug.Log("BuglyAgent: Failed to report uncaught exception");
            }

            return;
        }

        if (!IsInitialized)
        {
            return;
        }

        if (_uncaughtAutoReportOnce)
        {
            return;
        }

        _HandleException((System.Exception)args.ExceptionObject, null, true);
    }

    private static void _HandleException(System.Exception e, string message, bool uncaught)
    {
        if (e == null)
        {
            return;
        }

        if (!IsInitialized)
        {
            return;
        }

        string name = e.GetType().Name;
        string reason = e.Message;

        if (!string.IsNullOrEmpty(message))
        {
            reason = string.Format("{0}{1}***{2}", reason, Environment.NewLine, message);
        }

        StringBuilder stackTraceBuilder = new StringBuilder("");

        StackTrace stackTrace = new StackTrace(e, true);
        int count = stackTrace.FrameCount;
        for (int i = 0; i < count; i++)
        {
            StackFrame frame = stackTrace.GetFrame(i);

            stackTraceBuilder.AppendFormat("{0}.{1}", frame.GetMethod().DeclaringType.Name, frame.GetMethod().Name);

            ParameterInfo[] parameters = frame.GetMethod().GetParameters();
            if (parameters == null || parameters.Length == 0)
            {
                stackTraceBuilder.Append(" () ");
            }
            else
            {
                stackTraceBuilder.Append(" (");

                int pcount = parameters.Length;

                ParameterInfo param = null;
                for (int p = 0; p < pcount; p++)
                {
                    param = parameters[p];
                    stackTraceBuilder.AppendFormat("{0} {1}", param.ParameterType.Name, param.Name);

                    if (p != pcount - 1)
                    {
                        stackTraceBuilder.Append(", ");
                    }
                }
                param = null;

                stackTraceBuilder.Append(") ");
            }

            string fileName = frame.GetFileName();
            if (!string.IsNullOrEmpty(fileName) && !fileName.ToLower().Equals("unknown"))
            {
                fileName = fileName.Replace("\\", "/");

                int loc = fileName.ToLower().IndexOf("/assets/");
                if (loc < 0)
                {
                    loc = fileName.ToLower().IndexOf("assets/");
                }

                if (loc > 0)
                {
                    fileName = fileName.Substring(loc);
                }

                stackTraceBuilder.AppendFormat("(at {0}:{1})", fileName, frame.GetFileLineNumber());
            }
            stackTraceBuilder.AppendLine();
        }

        // report
        _reportException(uncaught, name, reason, stackTraceBuilder.ToString());
    }

    private static void _reportException(bool uncaught, string name, string reason, string stackTrace)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (string.IsNullOrEmpty(stackTrace))
        {
            stackTrace = StackTraceUtility.ExtractStackTrace();
        }

        if (string.IsNullOrEmpty(stackTrace))
        {
            stackTrace = "Empty";
        }
        else
        {

            try
            {
                string[] frames = stackTrace.Split('\n');

                if (frames != null && frames.Length > 0)
                {

                    StringBuilder trimFrameBuilder = new StringBuilder();

                    string frame = null;
                    int count = frames.Length;
                    for (int i = 0; i < count; i++)
                    {
                        frame = frames[i];

                        if (string.IsNullOrEmpty(frame) || string.IsNullOrEmpty(frame.Trim()))
                        {
                            continue;
                        }

                        frame = frame.Trim();

                        // System.Collections.Generic
                        if (frame.StartsWith("System.Collections.Generic.") || frame.StartsWith("ShimEnumerator"))
                        {
                            continue;
                        }
                        if (frame.Contains("Bugly"))
                        {
                            continue;
                        }
                        if (frame.Contains("..ctor"))
                        {
                            continue;
                        }

                        frame = frame.Replace(":", ".");

                        int start = frame.ToLower().IndexOf("(at");
                        int end = frame.ToLower().IndexOf("/assets/");

                        if (start > 0 && end > 0)
                        {
                            trimFrameBuilder.AppendFormat("{0}(at {1}", frame.Substring(0, start), frame.Substring(end));
                        }
                        else
                        {
                            trimFrameBuilder.Append(frame);
                        }

                        trimFrameBuilder.AppendLine();
                    }

                    stackTrace = trimFrameBuilder.ToString();
                }
            }
            catch
            {

            }

        }

        PrintLog(LogSeverity.Log, "\n*********\n{0} {1}\n{2}\n*********", name, reason, stackTrace);

        _uncaughtAutoReportOnce = uncaught && _autoQuitApplicationAfterReport;

        ReportException(uncaught ? EXCEPTION_TYPE_UNCAUGHT : EXCEPTION_TYPE_CAUGHT, name, reason, stackTrace, uncaught && _autoQuitApplicationAfterReport);
    }

    private static void _HandleException(LogSeverity logLevel, string name, string message, string stackTrace, bool uncaught)
    {
        if (!IsInitialized)
        {
            PrintLog(LogSeverity.Log, "It has not been initialized.");
            return;
        }

        if (logLevel == LogSeverity.Log)
        {
            return;
        }

        if ((uncaught && logLevel < _autoReportLogLevel))
        {
            PrintLog(LogSeverity.Log, "Not report exception for level {0}", logLevel.ToString());
            return;
        }

        string type = null;
        string reason = null;

        if (!string.IsNullOrEmpty(message))
        {
            try
            {
                if ((LogSeverity.LogException == logLevel) && message.Contains("Exception"))
                {
                    Match match = new Regex(@"^(?<errorType>\S+):\s*(?<errorMessage>.*)").Match(message);

                    if (match.Success)
                    {
                        type = match.Groups["errorType"].Value;
                        reason = match.Groups["errorMessage"].Value.Trim();
                    }
                }
            }
            catch
            {

            }

            if (string.IsNullOrEmpty(reason))
            {
                reason = message;
            }
        }

        if (string.IsNullOrEmpty(name))
        {
            if (string.IsNullOrEmpty(type))
            {
                type = string.Format("Unity{0}", logLevel.ToString());
            }
        }
        else
        {
            type = name;
        }

        _reportException(uncaught, type, reason, stackTrace);
    }

    private static bool _uncaughtAutoReportOnce = false;

    #endregion

}