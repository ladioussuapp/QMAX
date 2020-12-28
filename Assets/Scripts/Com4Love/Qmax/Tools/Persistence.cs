//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnityEngine;

///// <summary>
///// 持久化工具類
///// </summary>
//public class Persistence {

//    static protected Persistence _instance;

//    static public Persistence Instance
//    {
//        get
//        {
//            if (_instance == null)
//                _instance = new Persistence();

//            return _instance;
//        }
//    }

//    private Dictionary<string, string> m_perst_dict;
//    private Dictionary<string, string> m_temp_dict;

//    private Persistence()
//    {
//        m_perst_dict = new Dictionary<string, string>();
//        m_temp_dict = new Dictionary<string, string>();
//        LoadData();
//    }

//    /// <summary>
//    /// 通過key獲取數據
//    /// </summary>
//    /// <param name="key">指定Key</param>
//    /// <returns></returns>
//    public string GetValue(string key)
//    {
//        if (m_temp_dict.ContainsKey(key))
//        {
//            return m_temp_dict[key];
//        }
//        if (m_perst_dict.ContainsKey(key))
//        {
//            return m_perst_dict[key];
//        }
//        return null;
//    }

//    /// <summary>
//    /// 設置key-value數據
//    /// </summary>
//    /// <param name="key">指定Key</param>
//    /// <param name="value">指定Value</param>
//    /// <param name="perst">是否需要持久化</param>
//    public void SetValue(string key, string value, bool perst = true)
//    {
//        if (perst)
//        {
//            m_perst_dict[key] = value;
//            PersistenceData();
//        }
//        else
//        {
//            m_temp_dict[key] = value;
//        }
//    }

//    private void LoadData()
//    {
//        string path = PathKit.GetOSDataPath("data.db");
//        FileInfo file = new FileInfo(path);
//        bool exists = file.Exists;

//        FileStream aFile = new FileStream(path, FileMode.OpenOrCreate);
//        if (!exists)
//        {
//            aFile.Close();
//            return;
//        }
//        // clear perst dictionary
//        m_perst_dict.Clear();

//        byte[] bytes = new byte[aFile.Length];
//        aFile.Read(bytes, 0, (int)aFile.Length);
//        aFile.Close();
//        int offset = 0;
//        // count
//        int count = Bytes2Int(bytes, offset); offset += 4;
//        for (int i = 0; i < count; i++)
//        {
//            // read key
//            int keyLen = Bytes2Int(bytes, offset); offset += 4;
//            string key_base64 = Encoding.UTF8.GetString(bytes, offset, keyLen); offset += keyLen;
//            // read value
//            int valueLen = Bytes2Int(bytes, offset); offset += 4;
//            string value_base64 = Encoding.UTF8.GetString(bytes, offset, valueLen); offset += valueLen;
//            // set key & value
//            string key = Encoding.UTF8.GetString(System.Convert.FromBase64String(key_base64));
//            string value = Encoding.UTF8.GetString(System.Convert.FromBase64String(value_base64));
//            m_perst_dict[key] = value;
//        }
//    }


//    private void PersistenceData()
//    {
//        FileInfo t = new FileInfo(PathKit.GetOSDataPath("data.db"));
//        FileStream stream = t.Open(FileMode.Create);
//        // count
//        byte[] array = Int2Bytes(m_perst_dict.Count);
//        stream.Write(array, 0, array.Length);

//        foreach (var item in m_perst_dict)
//        {
//            array = Encoding.GetEncoding("UTF-8").GetBytes(item.Key);
//            string key_base64 = System.Convert.ToBase64String(array, 0, array.Length);
//            array = Encoding.GetEncoding("UTF-8").GetBytes(key_base64);
//            byte[] keyLen = Int2Bytes(array.Length);
//            // write key
//            stream.Write(keyLen, 0, keyLen.Length);
//            stream.Write(array, 0, array.Length);

//            array = Encoding.GetEncoding("UTF-8").GetBytes(item.Value);
//            string value_base64 = System.Convert.ToBase64String(array, 0, array.Length);
//            array = Encoding.GetEncoding("UTF-8").GetBytes(value_base64);
//            byte[] valueLen = Int2Bytes(array.Length);
//            // write value
//            stream.Write(valueLen, 0, valueLen.Length);
//            stream.Write(array, 0, array.Length);
//        }
//        stream.Close();
//        stream.Dispose();
//    }


//    static byte[] Int2Bytes(int i)
//    {
//        byte[] ret = new byte[4];
//        ret[3] = (byte)((i & 0xff000000) >> 24);
//        ret[2] = (byte)((i & 0x00ff0000) >> 16);
//        ret[1] = (byte)((i & 0x0000ff00) >> 8);
//        ret[0] = (byte)( i & 0x000000ff);
//        return ret;
//    }

//    static int Bytes2Int(byte[] bytes, int offset)
//    {
//        int ret = ((bytes[offset + 3] & 0x000000ff) << 24) |
//                  ((bytes[offset + 2] & 0x000000ff) << 16) |
//                  ((bytes[offset + 1] & 0x000000ff) << 8) |
//                  ((bytes[offset + 0] & 0x000000ff) << 0);
//        return ret;
//    }

//}
