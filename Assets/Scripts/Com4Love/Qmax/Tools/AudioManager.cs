using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace Com4Love.Qmax.Tools
{
    public class AudioManager : MonoBehaviour
    {
        const string MAP_SCENE_MUSIC = "Bgm_map_rolling";
        const string AUDIO_SOURCE_PREFAB = "Prefabs/AudioSourcePrefab";

        //播放bgm 
        AudioSource bgmSource;
        AudioSource oneShortSource;
        Dictionary<string, MusicData> loopMusics;

        private bool MSwitch = true;
        private bool SSwitch = true;
        public bool SoundSwitch
        {
            get
            {
                return SSwitch;
            }
            set
            {
                SSwitch = value;
                UpAudioMixer();
                FMOD_Manager.instance.SetOneShotSwitch(SSwitch);
            }
        }
        public bool MusicSwitch
        {
            get
            {
                return MSwitch;
            }
            set
            {
                MSwitch = value;
                UpAudioMixer();
                FMOD_Manager.instance.SetMusicSwitch(MSwitch);
            }
        }

        public AudioMixer audioMixer;


        private void UpAudioMixer()
        {
            if (MSwitch && SSwitch)
            {
                audioMixer.FindSnapshot("AllOn").TransitionTo(0);
            }
            else if (!MSwitch && !SSwitch)
            {
                audioMixer.FindSnapshot("AllOff").TransitionTo(0);
            }
            else if (MSwitch && !SSwitch)
            {
                audioMixer.FindSnapshot("SoundOff").TransitionTo(0);
            }
            else if (!MSwitch && SSwitch)
            {
                audioMixer.FindSnapshot("MusicOff").TransitionTo(0);
            }
        }

        struct MusicData
        {
            public AudioSource source;
            public bool bgmUnPause;
        }

        /// <summary>
        /// 播放背景音樂   背景音樂隨時有可能暫停。 
        /// </summary>
        /// <param name="clip"></param>
        public void PlayBgm(AudioClip clip)
        {
            if (bgmSource.isPlaying && bgmSource.clip.name == clip.name)
            {
                return;
            }

            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = 1f;
            bgmSource.Play();
        }

        public void CheckMusicCache()
        {
            //FileStream aFile = new FileStream(PathKit.GetOSDataPath("CacheMusic.txt"), FileMode.OpenOrCreate);
            //byte[] bytes = new byte[aFile.Length];
            //aFile.Read(bytes, 0, (int)aFile.Length);
            //aFile.Close();
            //string[] arr = Encoding.UTF8.GetString(bytes).Split('\n');
            //if(arr.Length != 1)
            //{
            //    string music = arr[0];
            //    string sound = arr[1];

            //    MusicSwitch = bool.Parse(music.Split('_')[1]);
            //    SoundSwitch = bool.Parse(sound.Split('_')[1]);
            //}
            //else
            //{
            //     MSwitch = true;
            //     SSwitch = true;
            //}
            //string actorId = GameController.Instance.Model.LoginData.actorId.ToString();

            ////1為開，0為關///
            ///默認開關為開///
            if (PlayerPrefsTools.HasKey(OnOff.CacheMusic))
            {
                MusicSwitch = PlayerPrefsTools.GetIntValue(OnOff.CacheMusic) == 1;
            }
            
            if (PlayerPrefsTools.HasKey(OnOff.CacheSound))
            {
                SoundSwitch = PlayerPrefsTools.GetIntValue(OnOff.CacheSound) == 1;
            }
        }

        public void SaveMusicLocal()
        {
            //StreamWriter sw;
            //FileInfo t = new FileInfo(PathKit.GetOSDataPath("CacheMusic.txt"));
            //sw = t.CreateText();
            //string music = "MusicData_" + MusicSwitch;
            //string sound = "SoundData_" + SoundSwitch;
            //sw.Write(music + "\n" + sound);
            //sw.Close();
            //sw.Dispose();

            //string actorId = GameController.Instance.Model.LoginData.actorId.ToString();

            PlayerPrefsTools.SetIntValue(OnOff.CacheMusic, (MusicSwitch ? 1 : 0));
            PlayerPrefsTools.SetIntValue(OnOff.CacheSound, (SoundSwitch ? 1 : 0));

        }

        /// <summary>
        /// 播放音樂 可以循環，可以疊加
        /// </summary>
        /// <param name="clipName"></param>
        /// <param name="pauseBgm">是否暫停bgm bgm會在此音樂被播放完畢之後繼續播放</param>
        /// <param name="loop">是否循環</param>
        public void PlayMusic(string clipName, bool pauseBgm = false, bool loop = false)
        {
            if (pauseBgm)
            {
                PauseBgm();
            }

            GameController.Instance.QMaxAssetsFactory.CreateEffectAudio(
                clipName, delegate(AudioClip clip)
                {
                    if (clip == null)
                        return;

                    clip.name = clipName;
                    PlayMusic(clip);
                });
        }

        public void PlayMusic(AudioClip clip, bool pauseBgm = false, bool loop = false)
        {
            //每次播放，請求一個緩存的AudioSource
            AudioSource MusicSource = GameController.Instance.PoolManager.PrefabSpawn(AUDIO_SOURCE_PREFAB).GetComponent<AudioSource>();
            MusicSource.clip = clip;
            MusicSource.loop = loop;
            MusicSource.Play();
            MusicData data = new MusicData();
            data.source = MusicSource;
            data.bgmUnPause = pauseBgm;

            if (loop)
            {
                if (!loopMusics.ContainsKey(clip.name))
                {
                    loopMusics.Add(clip.name, data);
                }
            }
            else
            {
                //檢測聲音播放完成。協程太耗，待更改 TODO
                this.StartCoroutine(this.ListForAudioStop(data));
            }
        }

        //停止音樂 當此音樂是循環播放時，需要主動停止此音樂。 TODO
        public void StopMusic(string clipName)
        {
            if (!loopMusics.ContainsKey(clipName))
            {
                return;
            }

            MusicData data = loopMusics[clipName];
            StopMusic(data);
            loopMusics.Remove(clipName);
        }

        void StopMusic(MusicData data)
        {
            data.source.clip = null;

            if (data.bgmUnPause)
            {
                UnPauseBgm();
            }

            GameController.Instance.PoolManager.Despawn(data.source.transform);
        }

        //PlayOneShot效果有問題
        public void PlayAudio(string clipName)
        {
            if (oneShortSource == null)
            {
                //oneShortSource 在start裡面初始化，直接調試場景時，有可能在此腳本的start之前播放聲音
                return;
            }

            GameController.Instance.QMaxAssetsFactory.CreateEffectAudio(
                clipName,
                delegate(AudioClip clip)
                {
                    oneShortSource.PlayOneShot(clip);
                });
        }

        public void PlayAudio(AudioClip clip)
        {
            if (oneShortSource == null)
            {
                return;
            }

            oneShortSource.PlayOneShot(clip);
        }

        private IEnumerator ListForAudioStop(MusicData data)
        {
            // Safer to wait a frame before testing if playing.
            yield return null;

            while (data.source.isPlaying)
                yield return null;

            StopMusic(data);
        }

        /// <summary>
        /// 繼續播放bgm
        /// </summary>
        public void UnPauseBgm()
        {
            if (bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.UnPause();
        }

        /// <summary>
        /// 暫停bgm
        /// </summary>
        public void PauseBgm()
        {
            if (bgmSource == null || !bgmSource.isPlaying)
            {
                return;
            }

            bgmSource.Pause();
        }

        /// <summary>
        /// 停止播放 bgm
        /// </summary>
        public void StopBgm()
        {
            if (bgmSource == null || !bgmSource.isPlaying)
            {
                return;
            }

            if (bgmSource.isPlaying)
            {
                bgmSource.Stop();
            }

            bgmSource.Stop();
            bgmSource.clip = null;
        }

        void Start()
        {
            if (loopMusics == null)
            {
                loopMusics = new Dictionary<string, MusicData>();
                GameController.Instance.PoolManager.PrePrefabSpawn(AUDIO_SOURCE_PREFAB, 5);
                Transform bgmSourceT = GameController.Instance.PoolManager.PrefabSpawn(AUDIO_SOURCE_PREFAB);
                Transform oneShortSourceT = GameController.Instance.PoolManager.PrefabSpawn(AUDIO_SOURCE_PREFAB);

                bgmSource = bgmSourceT.GetComponent<AudioSource>();
                oneShortSource = oneShortSourceT.GetComponent<AudioSource>();
            }
        }


        //依賴了對像池。對像池內部的屬性在Awake的時候初始化。
        //void Start()
        //{
        //    loopMusics = new Dictionary<string, MusicData>();
        //    GameBehaviour.Instance.PoolManager.PrePrefabSpawn(AUDIO_SOURCE_PREFAB, 5);
        //    Transform bgmSourceT = GameBehaviour.Instance.PoolManager.PrefabSpawn(AUDIO_SOURCE_PREFAB);
        //    Transform oneShortSourceT = GameBehaviour.Instance.PoolManager.PrefabSpawn(AUDIO_SOURCE_PREFAB);

        //    bgmSource = bgmSourceT.GetComponent<AudioSource>();
        //    oneShortSource = oneShortSourceT.GetComponent<AudioSource>();
        //}
    }
}
