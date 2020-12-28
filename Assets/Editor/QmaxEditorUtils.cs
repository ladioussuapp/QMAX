using UnityEngine;
using System.Collections;
using UnityEditor;
using Com4Love.Qmax;
using System;

public class QmaxEditorUtils : Editor
{


    [MenuItem("Qmax/清理所有PlayerPrefs")]
    static public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.SetInt("testtest", 1);
        Array a = Enum.GetValues(typeof(OnOff));

        foreach(var item in a)
        {
            //Debug.Log(item);
            PlayerPrefs.DeleteKey(item as string);
        }
        PlayerPrefs.DeleteKey("testtest");
        PlayerPrefs.DeleteKey(OnOff.Account.ToString());
        Debug.Log(PlayerPrefs.HasKey(OnOff.Account.ToString()));
        Debug.Log(PlayerPrefs.HasKey("testtest"));
    }
}
