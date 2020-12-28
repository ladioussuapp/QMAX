using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.XCodeEditor;
#endif
using System.IO;

public static class XCodePostProcess
{

#if UNITY_EDITOR
	[PostProcessBuild(999)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
		if (target != BuildTarget.iOS) {
			Debug.LogWarning("Target is not iPhone. XCodePostProcess will not run");
			return;
		}
		
		//得到xcode工程的路径
        string path = Path.GetFullPath (pathToBuiltProject);

		// Create a new project object from build target
		XCProject project = new XCProject( pathToBuiltProject );

		// Find and run through all projmods files to patch the project.
		// Please pay attention that ALL projmods files in your project folder will be excuted!
		string[] files = Directory.GetFiles( Application.dataPath, "*.projmods", SearchOption.AllDirectories );
		foreach( string file in files ) {
			UnityEngine.Debug.Log("ProjMod File: "+file);
			project.ApplyMod( file );
		}
		
		// 编辑plist 文件
        //EditorPlist(path);

        //编辑代码文件
        EditorCode(path);

		//TODO implement generic settings as a module option
		project.overwriteBuildSetting("CODE_SIGN_IDENTITY[sdk=iphoneos*]", "iPhone Distribution", "Release");
		
		// Finally save the xcode project
		project.Save();

	}
	
	private static void EditorCode(string filePath)
    {
		//读取UnityAppController.mm文件
        //XClass UnityAppController = new XClass(filePath + "/Classes/UnityAppController.mm");

        //在指定代码后面增加一行代码
        //UnityAppController.WriteBelow("#include \"PluginBase/AppDelegateListener.h\"","#include \"Bugly/CrashReporter.h\"");

        //在指定代码中替换一行
        //UnityAppController.Replace("return YES;","return [ShareSDK handleOpenURL:url sourceApplication:sourceApplication annotation:annotation wxDelegate:nil];");

        //在指定代码后面增加一行
        //UnityAppController.WriteBelow("::printf(\"-> applicationDidFinishLaunching()\\n\");","[[CrashReporter sharedInstance] enableLog:YES];\n [[CrashReporter sharedInstance] installWithAppId:@\"900010253\"];\n [self performSelector:@selector(crash) withObject:nil afterDelay:3.0];");


    }
	
#endif

	public static void Log(string message)
	{
		UnityEngine.Debug.Log("PostProcess: "+message);
	}
}
