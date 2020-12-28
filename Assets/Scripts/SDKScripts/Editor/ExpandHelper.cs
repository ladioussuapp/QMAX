using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;


public class ExpandHelper
{

    private static string namespaceURI = "http://schemas.android.com/apk/res/android";

    private static string activityClass = "com/loves/qmax/QMaxActivity.java";

    public static bool Expand(string projectPath, string bundleName, bool bugly, bool xgpush, string buglyAppId, string xgAccessId, string xgAccessKey)
    {
        string activityClassPath = projectPath + "/src/" + activityClass;
        if (bugly)
        {
            Helper.CopyDirectory(Application.dataPath + "/AndroidExpand/Bugly/libs/", projectPath + "/libs");
            Helper.CopyDirectory(Application.dataPath + "/AndroidExpand/Bugly/src/", projectPath + "/src", false);
            if (File.Exists(activityClassPath))
            {
                InsertJavaCode(activityClassPath, ExpandConf.buglyImportMark, ExpandConf.buglyImportCode);
                string str = String.Format(ExpandConf.buglyInitCrashReportCode[0], buglyAppId);
                InsertJavaCode(activityClassPath, ExpandConf.buglyInitCrashReportMark, str); 
            }
        }
        if (xgpush)
        {
            Helper.CopyDirectory(Application.dataPath + "/AndroidExpand/XGPush/libs/", projectPath + "/libs");
            Helper.CopyDirectory(Application.dataPath + "/AndroidExpand/XGPush/res/", projectPath + "/res");
            Helper.CopyDirectory(Application.dataPath + "/AndroidExpand/XGPush/src/", projectPath + "/src", false);
            if (File.Exists(activityClassPath))
            {
                InsertJavaCode(activityClassPath, ExpandConf.xgPushImportMark, ExpandConf.xgPushImportCode);
                InsertJavaCode(activityClassPath, ExpandConf.xgPushRegisterPushMark, ExpandConf.xgPushRegisterPushCode);
            }
        }
        return XmlText(projectPath, bundleName, bugly, xgpush, xgAccessId, xgAccessKey);
    }

    public static bool XmlText(string projectPath, string bundleName, bool bugly, bool xgpush, string accessId, string accessKey)
    {
        string xmlFile = projectPath + "/AndroidManifest.xml";
        if (!File.Exists(xmlFile))
        {
            return false;
        }
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlFile);
        
        XmlNode manifest = doc.DocumentElement;
        // package name
        if (!String.IsNullOrEmpty(bundleName))
        {
            ((XmlElement)manifest).SetAttribute("package", bundleName);
        }

        // applicatioin
        XmlNode applicatioin = manifest.SelectSingleNode("application");
        if (applicatioin != null)
        {
            if (xgpush)
            {
                XmlNode refChild = applicatioin.SelectSingleNode("meta-data");
                // 推送通知和消息是不同的，详情查阅XG Push文档
                AddXGPushReceiver(doc, applicatioin, refChild); // 接收通知的Receiver
                AddXGPushReceiver2(doc, applicatioin, refChild); // 接收消息的Receiver
                AddXGPushActivity(doc, applicatioin, refChild);
                AddXGPushService(doc, applicatioin, refChild, bundleName);
                AddXGPushMetaData(doc, applicatioin, refChild, accessId, accessKey);
            }

            // activity main class
            XmlNode activity = applicatioin.SelectSingleNode("activity");
            if (activity != null)
            {
                if (bugly || xgpush)
                {
                    ((XmlElement)activity).SetAttribute("android:name", "com.loves.qmax.QMaxActivity");
                }
            }
        }

        // permissions
        Dictionary<string, string> dict = new Dictionary<string, string>();
        XmlNodeList permissions = manifest.SelectNodes("uses-permission");
        foreach(XmlNode node in permissions)
        {
            string pre = ((XmlElement)node).GetAttribute("android:name");
            if (pre != null && !String.IsNullOrEmpty(pre)) {
                dict.Add(pre, pre);
            }
        }

        // bugly uses-permission
        if (bugly)
        {
            AddBuglyPermission(doc, manifest, dict);
        }

        // xg push uses-permission
        if (xgpush)
        {
            AddXGPushPermission(doc, manifest, dict);
        }

        doc.Save(xmlFile);
        return true;
    }

    private static void AddXGPushReceiver(XmlDocument doc, XmlNode applicatioin, XmlNode refChild)
    {
        XmlNode comm = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm.Value = " XGPush start ";
        applicatioin.InsertBefore(comm, refChild);
        XmlNode comm2 = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm2.Value = " 信鸽receiver广播接收 ";
        applicatioin.InsertBefore(comm2, refChild);

        XmlElement recv = (XmlElement)doc.CreateNode(XmlNodeType.Element, "receiver", null);
        recv.SetAttribute("android:name", namespaceURI, "com.tencent.android.tpush.XGPushReceiver");
        recv.SetAttribute("android:process", namespaceURI, ":xg_service_v2");

        XmlElement intent_filter = (XmlElement)doc.CreateNode(XmlNodeType.Element, "intent-filter", null);
        intent_filter.SetAttribute("android:priority", namespaceURI, "0x7fffffff");
        recv.AppendChild(intent_filter);

        string[] actions = {
                               "com.tencent.android.tpush.action.SDK" ,
                               "com.tencent.android.tpush.action.INTERNAL_PUSH_MESSAGE",
                               "android.intent.action.USER_PRESENT",
                               "android.net.conn.CONNECTIVITY_CHANGE",
                               "android.bluetooth.adapter.action.STATE_CHANGED",
                               "android.intent.action.ACTION_POWER_CONNECTED",
                               "android.intent.action.ACTION_POWER_DISCONNECTED"
                           };
        foreach(string actstr in actions)
        {
            XmlElement act = (XmlElement)doc.CreateNode(XmlNodeType.Element, "action", null);
            act.SetAttribute("android:name", namespaceURI, actstr);
            intent_filter.AppendChild(act);
        }

        intent_filter = (XmlElement)doc.CreateNode(XmlNodeType.Element, "intent-filter", null);
        intent_filter.SetAttribute("android:priority", namespaceURI, "0x7fffffff");
        recv.AppendChild(intent_filter);
        string[] actions2 = {
                               "android.intent.action.MEDIA_UNMOUNTED" ,
                               "android.intent.action.MEDIA_REMOVED",
                               "android.intent.action.MEDIA_CHECKING",
                               "android.intent.action.MEDIA_EJECT"
                           };
        foreach (string actstr in actions2)
        {
            XmlElement act = (XmlElement)doc.CreateNode(XmlNodeType.Element, "action", null);
            act.SetAttribute("android:name", namespaceURI, actstr);
            intent_filter.AppendChild(act);
        }
        XmlElement df = (XmlElement)doc.CreateNode(XmlNodeType.Element, "data", null);
        df.SetAttribute("android:scheme", namespaceURI, "file");
        intent_filter.AppendChild(df);

        applicatioin.InsertBefore(recv, refChild);
    }

    private static void AddXGPushReceiver2(XmlDocument doc, XmlNode applicatioin, XmlNode refChild)
    {
        XmlNode comm = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm.Value = " 接收消息的receiver ";
        applicatioin.InsertBefore(comm, refChild);

        XmlElement recv = (XmlElement)doc.CreateNode(XmlNodeType.Element, "receiver", null);
        recv.SetAttribute("android:name", namespaceURI, "com.loves.qmax.QMaxPushReceiver");
        recv.SetAttribute("android:process", namespaceURI, ":xg_service_v2");

        XmlElement intent_filter = (XmlElement)doc.CreateNode(XmlNodeType.Element, "intent-filter", null);
        recv.AppendChild(intent_filter);
        string[] actions = {
                               "com.tencent.android.tpush.action.PUSH_MESSAGE" ,
                               "com.tencent.android.tpush.action.FEEDBACK"
                           };
        foreach (string actstr in actions)
        {
            XmlElement act = (XmlElement)doc.CreateNode(XmlNodeType.Element, "action", null);
            act.SetAttribute("android:name", namespaceURI, actstr);
            intent_filter.AppendChild(act);
        }
        applicatioin.InsertBefore(recv, refChild);
    }

    private static void AddXGPushActivity(XmlDocument doc, XmlNode applicatioin, XmlNode refChild)
    {
        XmlNode comm = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm.Value = " 展示通知的activity ";
        applicatioin.InsertBefore(comm, refChild);

        XmlElement acty = (XmlElement)doc.CreateNode(XmlNodeType.Element, "activity", null);
        acty.SetAttribute("android:name", namespaceURI, "com.tencent.android.tpush.XGPushActivity");
        acty.SetAttribute("android:theme", namespaceURI, "@android:style/Theme.Translucent");
        acty.SetAttribute("android:exported", namespaceURI, "true");

        XmlElement intent_filter = (XmlElement)doc.CreateNode(XmlNodeType.Element, "intent-filter", null);
        acty.AppendChild(intent_filter);

        XmlElement act = (XmlElement)doc.CreateNode(XmlNodeType.Element, "action", null);
        act.SetAttribute("android:name", namespaceURI, "");
        intent_filter.AppendChild(act);

        applicatioin.InsertBefore(acty, refChild);
    }

    private static void AddXGPushService(XmlDocument doc, XmlNode applicatioin, XmlNode refChild, string packageName)
    {
        XmlNode comm1 = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm1.Value = " 信鸽service ";
        applicatioin.InsertBefore(comm1, refChild);

        XmlElement service1 = (XmlElement)doc.CreateNode(XmlNodeType.Element, "service", null);
        service1.SetAttribute("android:name", namespaceURI, "com.tencent.android.tpush.service.XGPushService");
        service1.SetAttribute("android:exported", namespaceURI, "true");
        service1.SetAttribute("android:persistent", namespaceURI, "true");
        service1.SetAttribute("android:process", namespaceURI, ":xg_service_v2");
        applicatioin.InsertBefore(service1, refChild);

        XmlNode comm2 = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm2.Value = " 通知service，此选项有助于提高抵达率 ";
        applicatioin.InsertBefore(comm2, refChild);

        XmlElement service2 = (XmlElement)doc.CreateNode(XmlNodeType.Element, "service", null);
        service2.SetAttribute("android:name", namespaceURI, "com.tencent.android.tpush.rpc.XGRemoteService");
        service2.SetAttribute("android:exported", namespaceURI, "true");

        XmlElement intent_filter = (XmlElement)doc.CreateNode(XmlNodeType.Element, "intent-filter", null);
        service2.AppendChild(intent_filter);

        XmlElement act = (XmlElement)doc.CreateNode(XmlNodeType.Element, "action", null);
        act.SetAttribute("android:name", namespaceURI, packageName + ".PUSH_ACTION");
        intent_filter.AppendChild(act);

        applicatioin.InsertBefore(service2, refChild);
    }

    private static void AddXGPushMetaData(XmlDocument doc, XmlNode applicatioin, XmlNode refChild, string accessId, string accessKey)
    {
        XmlNode comm1 = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm1.Value = " 信鸽ACCESS_ID和ACCESS_KEY ";
        applicatioin.InsertBefore(comm1, refChild);

        XmlElement md1 = (XmlElement)doc.CreateNode(XmlNodeType.Element, "meta-data", null);
        md1.SetAttribute("android:name", namespaceURI, "XG_V2_ACCESS_ID");
        md1.SetAttribute("android:value", namespaceURI, accessId);
        applicatioin.InsertBefore(md1, refChild);

        XmlElement md2 = (XmlElement)doc.CreateNode(XmlNodeType.Element, "meta-data", null);
        md2.SetAttribute("android:name", namespaceURI, "XG_V2_ACCESS_KEY");
        md2.SetAttribute("android:value", namespaceURI, accessKey);
        applicatioin.InsertBefore(md2, refChild);

        XmlNode comm2 = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm2.Value = " XGPush end ";
        applicatioin.InsertBefore(comm2, refChild);
    }

    private static void AddBuglyPermission(XmlDocument doc, XmlNode manifest, Dictionary<string, string> dict)
    {
        XmlNode comm = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm.Value = " bugly uses-permission ";
        manifest.AppendChild(comm);

        foreach (string per in ExpandConf.buglyPermissions) {
            AddPermission(doc, manifest, dict, per);
        }
    }

    private static void AddXGPushPermission(XmlDocument doc, XmlNode manifest, Dictionary<string, string> dict)
    {
        XmlNode comm = doc.CreateNode(XmlNodeType.Comment, "", null);
        comm.Value = " XG Push uses-permission ";
        manifest.AppendChild(comm);

        foreach (string per in ExpandConf.xgPushPermissions)
        {
            AddPermission(doc, manifest, dict, per);
        }
    }

    private static void AddPermission(XmlDocument doc, XmlNode manifest, Dictionary<string, string> dict, string per)
    {
        if (!dict.ContainsKey(per))
        {
            XmlElement attb = (XmlElement)doc.CreateNode(XmlNodeType.Element, "uses-permission", null);
            attb.SetAttribute("android:name", namespaceURI, per);
            manifest.AppendChild(attb);
            dict.Add(per, per);
        }
    }

    private static void InsertJavaCode(string file, string mark, params string[] insert)
    {
        if (File.Exists(file))
        {
            StringBuilder strBuilder = new StringBuilder();

            StreamReader sr = new StreamReader(file);
            string str = null;
            while ((str = sr.ReadLine()) != null)
            {
                if (str.Contains(mark))
                {
                    foreach (string line in insert)
                    {
                        strBuilder.AppendLine(line);
                    }
                }
                else
                {
                    strBuilder.AppendLine(str);
                }
            }
            sr.Close();

            StreamWriter sw = new StreamWriter(file);
            sw.Write(strBuilder.ToString());
            sw.Close();
        }
    }

}
