using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMOD;

public class FMOD_Manager {

    private static FMOD_Manager sInstance;

    private Dictionary<string, EventInstance> music;

    private Dictionary<string, EventInstance> shot;

    private bool isMusicOpen = true;
    private bool isShotOpen = true;

    public static FMOD_Manager instance
    {
        get
        {
            if (sInstance == null)
            {
                sInstance = new FMOD_Manager();
            }
            return sInstance;
        }
    }

    private FMOD_Manager()
    {
        music = new Dictionary<string, EventInstance>();
        shot = new Dictionary<string, EventInstance>();
    }

    /// <summary>
    /// 播放音樂
    /// </summary>
    /// <param name="asset"></param>
    public void PlayMusic(FMODAsset asset)
    {
        EventInstance evt = FMOD_StudioSystem.instance.GetEvent(asset.id);
        PlayMusic(evt);
    }

    /// <summary>
    /// 播放音樂
    /// </summary>
    /// <param name="path"></param>
    public void PlayMusic(string path)
    {
        EventInstance evt = FMOD_StudioSystem.instance.GetEvent(path);
        PlayMusic(evt);
    }


    /// <summary>
    /// 播放音樂
    /// </summary>
    /// <param name="evt"></param>
    public void PlayMusic(EventInstance evt)
    {
        if (evt == null)
            return;

        string name = evt.getRaw().ToString();
        if (name == null || name.Length == 0)
            return;

        if (music.ContainsKey(name))
            music.Remove(name);

        music.Add(name, evt);

        if (isMusicOpen)
            evt.start();
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="asset"></param>
    public void PlayOneShot(FMODAsset asset)
    {
        EventInstance evt = FMOD_StudioSystem.instance.GetEvent(asset.id);
        PlayOneShot(evt);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="path"></param>
    public void PlayOneShot(string path)
    {
        EventInstance evt = FMOD_StudioSystem.instance.GetEvent(path);
        PlayOneShot(evt);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="evt"></param>
    public void PlayOneShot(EventInstance evt)
    {
        if (!isShotOpen)
            return;

        if (evt == null)
            return;

        string name = evt.getRaw().ToString();
        if (name == null || name.Length == 0)
            return;

        if (shot.ContainsKey(name))
            shot.Remove(name);

        shot.Add(name, evt);
        evt.start();
    }

    /// <summary>
    /// 設置音樂開關
    /// </summary>
    /// <param name="open"></param>
    public void SetMusicSwitch(bool open)
    {
        foreach (KeyValuePair<string, EventInstance> kv in music)
        {
            EventInstance evt = kv.Value;

            float volume = 0;
            evt.getVolume(out volume);

            if (open && volume > 0)
                evt.start();
            else
                evt.setVolume(open ? 1.0f : 0.0f);
        }
        isMusicOpen = open;
    }

    /// <summary>
    /// 設置音效開關
    /// </summary>
    /// <param name="open"></param>
    public void SetOneShotSwitch(bool open)
    {
        if (!open)
        {
            // 停掉所有的音效
            ClearOneShot();
        }
        isShotOpen = open;
    }

    public void ClearMusic()
    {
        foreach (KeyValuePair<string, EventInstance> kv in music)
        {
            EventInstance evt = kv.Value;
            evt.stop(STOP_MODE.IMMEDIATE);
        }
        music.Clear();
    }

    public void ClearOneShot()
    {
        foreach (KeyValuePair<string, EventInstance> kv in shot)
        {
            EventInstance evt = kv.Value;
            evt.stop(STOP_MODE.IMMEDIATE);
        }
        shot.Clear();
    }

}
