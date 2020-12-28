using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using Com4Love.Qmax;


//路徑工具類
public class PathKit
{

    /** 後綴常量字符 */
    public const string SUFFIX = ".txt";
    const string PREFIX = "file://";
    const string FORMAT = ".unity3d";
    public static string RESROOT = Application.persistentDataPath + "/";

    public static string GetStreamingAssetsPath(string p_filename)
    {
        string _strPath = "";
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            _strPath = "file://" + Application.streamingAssetsPath + "/" + p_filename + ".unity3d";
        else if (Application.platform == RuntimePlatform.Android)
            _strPath = Application.streamingAssetsPath + "/" + p_filename + ".unity3d";
        else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.IPhonePlayer)
            _strPath = "file://" + Application.streamingAssetsPath + "/" + p_filename + ".unity3d";

        return _strPath;
    }



    public static string GetOSDataPath(string p_filename)
    {
        string path;
        path = "";

        if (Application.platform == RuntimePlatform.OSXEditor)
            path = Application.persistentDataPath + p_filename;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
            path = RESROOT + p_filename;


        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            path = Application.dataPath + "/cache/" + p_filename;


        if (Application.platform == RuntimePlatform.Android)
            path = RESROOT + p_filename;

        return path;
    }

    public static string GetURLPath(string p_filename, bool needPreFix, bool needFormat)
    {
        string path;
        path = "";

        if (Application.platform == RuntimePlatform.OSXEditor)
            path = Application.persistentDataPath + "/" + p_filename;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
            path = RESROOT + p_filename;


        if (Application.platform == RuntimePlatform.WindowsEditor)
            path = Application.dataPath + "/cache/" + p_filename;

        if (Application.platform == RuntimePlatform.WindowsPlayer)
            path = Application.dataPath + "/cache/" + p_filename;

        if (Application.platform == RuntimePlatform.Android)
            path = RESROOT + p_filename;

        if (needPreFix) path = PREFIX + path;
        if (needFormat) path = path + FORMAT;

        return path;
    }

    public static string getFileName(string path)
    {

        string[] _list = path.Split(new char[] { '/' });

        if (_list.Length > 0) return _list[_list.Length - 1];
        else
            return "";

    }

    public static string getFileDir(string path)
    {
        path = path.Replace("\\", "/");
        path = path.Substring(0, path.LastIndexOf("/"));
        return path;
    }

    public static void CreateDirIfNotExists(string path)
    {
        string dir = getFileDir(path);
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }
    }

    /// <summary>
    /// 讀文件裡的utf字符串。路徑基於 PathKit.GetOSDataPath 的值
    /// </summary>
    /// <param name="fileName">文本文件名</param>
    /// <returns></returns>
    public static string ReadUTFString(string fileName)
    {
        try
        {
            FileStream aFile = new FileStream(PathKit.GetOSDataPath(fileName), FileMode.OpenOrCreate);
            byte[] bytes = new byte[aFile.Length];
            aFile.Read(bytes, 0, (int)aFile.Length);
            aFile.Close();
            return Encoding.UTF8.GetString(bytes); 
        }
        catch (System.Exception)
        {
            Q.Assert(false, "PathKit.ReadUTFString(" + fileName + ") error!");
            return "";
        }
    }

    /// <summary>
    /// 寫utf字符串到文件裡
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="val"></param>
    public static void WriteUTFString(string fileName , string val)
    {
        try
        {
            StreamWriter sw;
            FileInfo t = new FileInfo(PathKit.GetOSDataPath(fileName));
            sw = t.CreateText();
            sw.Write(val);
            sw.Close();
            sw.Dispose();
        }
        catch (System.Exception)
        {
            Q.Assert(false, "PathKit.WriteUTFString(" + fileName + "," + val + ") error!");
        }
    }
}
