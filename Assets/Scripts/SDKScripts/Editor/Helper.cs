using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;

public class Helper
{
	/// <summary>
	/// Deletes the folder.
	/// </summary>
	/// <param name="directoryPath">Directory path.</param>
	public static void DeleteFolder(string directoryPath)
	{
		try
		{
			foreach (string d in Directory.GetFileSystemEntries(directoryPath))
			{
				if (File.Exists(d))
				{
					FileInfo fi = new FileInfo(d);
					if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
						fi.Attributes = FileAttributes.Normal;
					File.Delete(d);     //删除文件   
				}
				else
					DeleteFolder(d);    //删除文件夹
			}
			
			Directory.Delete(directoryPath);    //删除空文件夹
		}
		catch
		{
			
		}
	}

	/// <summary>
	/// Copies the directory.
	/// </summary>
	/// <param name="sourcePath">Source path.</param>
	/// <param name="destinationPath">Destination path.</param>
    /// <param name="overWrite">overWrite.</param>
    public static void CopyDirectory(string sourcePath, string destinationPath, bool overWrite = true)
	{
		
		DirectoryInfo info = new DirectoryInfo(sourcePath);
		Directory.CreateDirectory(destinationPath);
		foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
		{
			string destName = Path.Combine(destinationPath, fsi.Name);
			
			if (fsi is System.IO.FileInfo)
			{     //如果是文件，复制文件
                if (fsi.FullName.Contains(".meta"))
                {
                    continue;
                }
                if (!overWrite && File.Exists(destName))
                {
                    continue;
                }
                File.Copy(fsi.FullName, destName, true);
			}
			else                                    //如果是文件夹，新建文件夹，递归
			{
				if (fsi.FullName.Contains(".svn"))
				{
					continue;
				}
				Directory.CreateDirectory(destName);
				//   Log.i("正在拷贝文件夹 --> " + destName);
                CopyDirectory(fsi.FullName, destName, overWrite);
			}
		}
		
	}

	#region 修改R文件
	public static void ChangeR(string modifyPackage, string originalPackage, string gamePath)
	{
		string srcFolder = gamePath + "/src";
		string originString = originalPackage + ".*.R;";
		string apkToReplace = modifyPackage + ".R;";
		string fileType = "*.java";

		if(Directory.Exists (srcFolder))
		{
			string[] files = Directory.GetFiles(srcFolder, fileType, SearchOption.AllDirectories);
			for(int i = 0; i < files.Length; i++)
			{
				string temp = System.IO.File.ReadAllText (files[i]);
				string outString = Regex.Replace(temp, originString, apkToReplace);
				System.IO.File.WriteAllText (files[i], outString);
			}
		}
	}
	#endregion
}
