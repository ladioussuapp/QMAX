using UnityEngine;
using System.Collections;
using Com4Love.Qmax;


public class PlayerPrefsTools
{
    /// <summary>
    /// 默認是沒有ID的///
    /// 因為沒有獲取到ID的時候引用ID為空///
    ///設置值的時候有ID，取值也必須有相同ID///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="isHaveID">保存的Key是否需要根據角色來區分</param>
    static public void SetIntValue(OnOff key, int value, bool isHaveID = false)
    {
        SetIntValue(key.ToString(), value, isHaveID);
    }


    static public void SetIntValue(string key, int value, bool isHaveID = false)
    {
        if (isHaveID)
        {
            //string id = GameController.Instance.Model.LoginData.actorId.ToString();
            PlayerPrefs.SetInt(KeyAddID(key), value);
        }
        else
        {
            PlayerPrefs.SetInt(key.ToString(), value);
        }
    }


    static public int GetIntValue(OnOff key, bool isHaveID = false)
    {
        return GetIntValue(key.ToString(), isHaveID);
    }

    static public int GetIntValue(string key, bool isHaveID = false)
    {
        int value = 0;

        if (isHaveID)
        {
            //string id =GameController.Instance.Model.LoginData.actorId.ToString();
            value = PlayerPrefs.GetInt(KeyAddID(key), 0);
        }
        else
        {
            value = PlayerPrefs.GetInt(key.ToString(), 0);
        }

        return value;
    }

    static public void SetBoolValue(string key, bool value, bool isHaveID = false)
    {
        SetIntValue(key, value == true ? 1 : 0, isHaveID);
    }

    static public void SetBoolValue(OnOff key, bool value, bool isHaveID = false)
    {
        SetBoolValue(key.ToString(), value, isHaveID);
    }

    static public bool GetBoolValue(string key, bool isHaveID = false)
    {
        int value = GetIntValue(key, isHaveID);
        return value != 0 ? true : false;
    }

    static public bool GetBoolValue(OnOff key, bool isHaveID = false)
    {
        int value = GetIntValue(key, isHaveID);
        return value != 0 ? true : false;
    }


    static public void SetStringValue(OnOff key, string value, bool isHaveID = false)
    {
        if (isHaveID)
        {
            //string id = GameController.Instance.Model.LoginData.actorId.ToString();
            PlayerPrefs.SetString(KeyAddID(key), value);
        }
        else
        {
            PlayerPrefs.SetString(key.ToString(), value);
        }
    }

    static public string GetStringValue(OnOff key, bool isHaveID = false)
    {
        string value = "";

        if (isHaveID)
        {
            //string id = GameController.Instance.Model.LoginData.actorId.ToString();
            value = PlayerPrefs.GetString(KeyAddID(key), "");
        }
        else
        {
            value = PlayerPrefs.GetString(key.ToString(), "");
        }

        return value;
    }

    static public bool HasKey(OnOff key, bool isHaveID = false)
    {
        return HasKey(key.ToString(), isHaveID);
    }

    static public bool HasKey(string key, bool isHaveID = false)
    {
        if (isHaveID)
        {
            key = KeyAddID(key);
        }

        return PlayerPrefs.HasKey(key);
    }


    static private string KeyAddID(OnOff key)
    {
        return KeyAddID(key.ToString());
    }

    static private string KeyAddID(string key)
    {
        string id = "";

        if (GameController.Instance.Model.LoginData != null)
        {
            id = GameController.Instance.Model.LoginData.actorId.ToString();
        }

        return key + id;
    }
}


