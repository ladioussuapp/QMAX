using UnityEngine;
using UnityEditor;
using Com4Love.Qmax.Data.Config;
using System.Collections.Generic;

public class AutoPrefab {


    public static Dictionary<string, GameObject> GenerateAudioObject(GameObject parent, UnitConfig uConf)
    {
        Dictionary<string, GameObject> ret = new Dictionary<string, GameObject>();
        if (uConf.AudioCharge != "")
        {
            GameObject audio = GenerateAudioObject("AudioCharge", uConf.AudioCharge);
            audio.transform.parent = parent.transform;
            ret.Add("AudioCharge", audio);
        }
        if (uConf.AudioAttack != "")
        {
            GameObject audio = GenerateAudioObject("AudioAttack", uConf.AudioAttack);
            audio.transform.parent = parent.transform;
            ret.Add("AudioAttack", audio);
        }
        if (uConf.AudioDie != "")
        {
            GameObject audio = GenerateAudioObject("AudioDie", uConf.AudioDie);
            audio.transform.parent = parent.transform;
            ret.Add("AudioDie", audio);
        }
        if (uConf.AudioHit != "")
        {
            GameObject audio = GenerateAudioObject("AudioHit", uConf.AudioHit);
            audio.transform.parent = parent.transform;
            ret.Add("AudioHit", audio);
        }
        if (uConf.AudioUpgrade1 != "")
        {
            GameObject audio = GenerateAudioObject("AudioUpgrade1", uConf.AudioUpgrade1);
            audio.transform.parent = parent.transform;
            ret.Add("AudioUpgrade1", audio);
        }
        if (uConf.AudioUpgrade2 != "")
        {
            GameObject audio = GenerateAudioObject("AudioUpgrade2", uConf.AudioUpgrade2);
            audio.transform.parent = parent.transform;
            ret.Add("AudioUpgrade2", audio);
        }
        if (uConf.AudioUpgrade3 != "")
        {
            GameObject audio = GenerateAudioObject("AudioUpgrade3", uConf.AudioUpgrade3);
            audio.transform.parent = parent.transform;
            ret.Add("AudioUpgrade3", audio);
        }
        return ret;
    }

    public static GameObject GenerateAudioObject(string name, string audioClip)
    {
        GameObject audio = new GameObject();
        audio.name = name;

        audio.AddComponent<AudioSource>();
        AudioSource audioSource = audio.GetComponent<AudioSource>();

        UnityEngine.Audio.AudioMixer am = AssetDatabase.LoadAssetAtPath("Assets/Resources/AudioMixer.mixer", typeof(UnityEngine.Audio.AudioMixer)) as UnityEngine.Audio.AudioMixer;
        UnityEngine.Audio.AudioMixerGroup[] groups = am.FindMatchingGroups("");

        foreach (UnityEngine.Audio.AudioMixerGroup amg in groups)
        {
            if (amg.name == "Sound")
            {
                audioSource.outputAudioMixerGroup = amg;
                audioSource.playOnAwake = false;
                break;
            }
        }

        UnityEngine.AudioClip ac = AssetDatabase.LoadAssetAtPath("Assets/ExternalRes/Audio/" + audioClip, typeof(UnityEngine.AudioClip)) as UnityEngine.AudioClip;
        audioSource.clip = ac;

        return audio;
    }


    public static Object GenerateUnitPrefab(GameObject go, string path)
    {
        Object tempPrefab = PrefabUtility.CreatePrefab(path, go);
        return tempPrefab;
    }
}
