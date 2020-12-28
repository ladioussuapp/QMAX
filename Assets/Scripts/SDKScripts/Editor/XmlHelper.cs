using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;                   // 用来编辑XML文件
using System.Diagnostics;

public class XmlHelper
{
	#region 打开AndroidManifest(createAndroidManifest)
	public static bool createAndroidManifest(string orginManifestXml, string forManifestXml, string distFile,string package, string pVersionCode, string pVersionName)
	{
		const string packageTag = "package";
		const string versionCodeTag = "android:versionCode";
		const string versionNameTag = "android:versionName";
		
		//检测 文件是否存在
		XmlDocument orginXML = new XmlDocument();
		XmlDocument forXML = new XmlDocument();
		
		if (!File.Exists(orginManifestXml) || !File.Exists(forManifestXml))
			return false;
		
		orginXML.Load(orginManifestXml);
		forXML.Load(forManifestXml);
		//初始化xml
		XmlNode originalManifest = orginXML.DocumentElement;
		XmlNode originalApplicatioin = originalManifest.SelectSingleNode("application");
		
		XmlNode forManifest = forXML.DocumentElement;
		//替换manifest标签的参数   优化，把所有的标签参数进行遍历 不要写死
		string versionCode = pVersionCode;
		string versionName = pVersionName;
		
		
		if (!String.IsNullOrEmpty(package))
		{
			((XmlElement)originalManifest).SetAttribute(packageTag, package);
		}
		if (!String.IsNullOrEmpty(versionCode))
		{
			((XmlElement)originalManifest).SetAttribute(versionCodeTag, versionCode);
		}
		if (!String.IsNullOrEmpty(versionName))
		{
			((XmlElement)originalManifest).SetAttribute(versionNameTag, versionName);
		}
		
		//添加manifest标签子元素
		bool manifestCfg = ((XmlElement)forManifest).SelectSingleNode("manifestCfg") != null;
		if (manifestCfg)
		{
			XmlNodeList forManifestChildNodes = ((XmlElement)forManifest).SelectSingleNode("manifestCfg").ChildNodes;
			appendOrUpdateXmlNodeWithAttr(orginXML, originalManifest, forManifestChildNodes, "android:name");
		}
		//添加 application 标签子元素
		bool applicationCfg = ((XmlElement)forManifest).SelectSingleNode("applicationCfg") != null;
		if (applicationCfg)
		{
			XmlNodeList forApplicationChildNodes = ((XmlElement)forManifest).SelectSingleNode("applicationCfg").ChildNodes;
			appendOrUpdateXmlNodeWithAttr(orginXML, originalApplicatioin, forApplicationChildNodes, "android:name");
		}
		orginXML.Save(distFile);
		return true;
		
		
	}
	#endregion

	#region 更新节点的属性(appendOrUpdateXmlNodeWithAttr)
	private static void appendOrUpdateXmlNodeWithAttr(XmlDocument xmlDoc, XmlNode node, XmlNodeList childrens, string attr)
	{
		foreach (XmlNode child in childrens)
		{
			foreach (XmlNode toSearch in node.ChildNodes)
			{
				XmlElement temp = null;
				if (toSearch is XmlElement && child is XmlElement)
				{
					temp = (XmlElement)toSearch;
					//找到了相同名字的
					if (temp.GetAttribute(attr).Equals(((XmlElement)child).GetAttribute(attr))&&temp.Name.Equals(((XmlElement)child).Name))
					{
						//node.RemoveChild(toSearch);
						continue;
					}
				}
				
			}
			
			node.AppendChild(xmlDoc.ImportNode(child, true));
			
		}
	}
	#endregion

	#region 创建字符串(createAndroidString)
	public static bool createAndroidString(string orginStringXml, string forStringXml, string distFile)
	{
		
		//检测 文件是否存在
		XmlDocument orginXML = new XmlDocument();
		XmlDocument forXML = new XmlDocument();
		
		if (!File.Exists(orginStringXml) || !File.Exists(forStringXml))
			return false;
		
		orginXML.Load(orginStringXml);
		forXML.Load(forStringXml);
		//初始化xml
		XmlNode originalXml = orginXML.DocumentElement;
		XmlNode forXml = forXML.DocumentElement;
		
		//添加string标签子元素
		XmlNodeList forXmlChildNodes = ((XmlElement)forXml).ChildNodes;
		appendOrUpdateXmlNodeWithAttr(orginXML, originalXml, forXmlChildNodes, "name");
		orginXML.Save(distFile);
		
		return true;
	}
	#endregion
}
