using Com4Love.Qmax.Data;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    //場景跳轉控制應該只是view層的一個行為類，不應該是遊戲的控制類。
    public class SceneCtr
    {

        protected Scenes _targetScene;

        /// <summary>
        /// 從什麼場景跳轉過來 默認為none，表示沒有跳轉
        /// </summary>
        protected Scenes _preScene = Scenes.None;

        /// <summary>
        ///想要進入的下個場景///
        /// </summary>
        private Scenes _wantToScene = Scenes.None;

        protected Dictionary<string, object> _sceneData;       //暫用 關卡索引
        protected PreloadData[] _preloadDatas;
        protected bool isLoading;
        protected GameObject preloadPrefab;

        private bool _cloudFlag;

        public bool CloudFlag
        {
            get { return _cloudFlag; }
            set { _cloudFlag = value;  }
        }

        public Scenes TargetScene
        {
            get { return _targetScene; }
        }

        public Scenes PreScene
        {
            get { return _preScene; }
        }

        public Scenes WantToScene
        {
            get { return _wantToScene; }
            set { _wantToScene = value; }
        }

        public Dictionary<string, object> SceneData
        {
            get
            {
                return _sceneData;
            }
        }

        public PreloadData[] PreloadDatas
        {
            get
            {
                return _preloadDatas;
            }
        }


        public void Clear()
        {
            _preloadDatas = null;
            if (_sceneData != null)
                _sceneData.Clear();
        }


        /// <summary>
        /// 統一調用這個接口加載Scene
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneData"></param>
        /// <param name="async">如果是同步加載，則不出現Loading界面；異步則需要出現loading界面</param>
        public void LoadLevel(Scenes scene, Dictionary<string, object> sceneData = null, bool async = true, bool cloudFlag = false)
        {
 
            Q.Assert(!isLoading, "正在跳轉場景中");
            Q.Assert(scene != Scenes.LoadingScene, "跳轉到loading場景是幾個意思?");

            //加個判斷
            if (isLoading)
            {
                return;
            }
            // 切場景的時候清除掉FMOD播放
            FMOD_Manager.instance.ClearMusic();
            FMOD_Manager.instance.ClearOneShot();

            _cloudFlag = cloudFlag;
            _preScene = _targetScene;
            _targetScene = scene;
            _sceneData = sceneData;
            ///加載了場景之後把想要進入的場景清空///
            _wantToScene = Scenes.None;
            isLoading = true;
            switch (_preScene)
            {
                case Scenes.MapScene:
                    //從地圖場景離開後就不再屬於上一次戰鬥解鎖了關卡狀態
                    GameController.Instance.Model.IsStagePassedInLastFight = false;
                    break;
                default:
                    break;
            }
            ModelEventSystem evtSys = GameController.Instance.ModelEventSystem;

            if (evtSys.BeforeSceneChangeEvent != null)
                evtSys.BeforeSceneChangeEvent(_preScene, _targetScene);

            Q.Log("SceneChange:{0}->{1} start", _preScene, _targetScene);

            if (async)
            {
                if (_cloudFlag)
                {
                    GameController.Instance.ViewEventSystem.JumpSceneShowCloudEvent(delegate()
                    {
                        //Application.LoadLevel(Scenes.LoadingScene.ToString());   //加載loading場景
                        //異步加載loading場景
                        Application.LoadLevelAsync(Scenes.LoadingScene.ToString());
                    });
                }
                else
                {
                    //Application.LoadLevel(Scenes.LoadingScene.ToString());   //加載loading場景
                    //異步加載loading場景
                    Application.LoadLevelAsync(Scenes.LoadingScene.ToString());
                }
                
            }
            else
            {
                Application.LoadLevel(_targetScene.ToString());
                isLoading = false;
            }

            if (evtSys.AfterSceneChangeEvent != null)
                evtSys.AfterSceneChangeEvent(_preScene, _targetScene);

            SendSceneMessage();
            System.GC.Collect();
            //GameController.Instance.AtlasManager.LogStatus();

            Q.Log("SceneChange_loadingSceneComplete:{0}->{1}", _preScene, _targetScene);
        }

        public void LoadComplete(Scenes lvl)
        {
            Q.Log("SceneChange_TargetSceneComplete:{0}->{1}", _preScene, _targetScene);
            isLoading = Application.loadedLevelName == lvl.ToString();
        }

        void SendSceneMessage()
        {
            string mess = "";
            Scenes[] useScenes = 
            {
                Scenes.MapScene,
                Scenes.BattleScene,
            };

            foreach (var se in useScenes)
            {
                if (se.ToString().Equals(_targetScene.ToString()))
                {
                    mess = se.ToString();
                    break;
                }
            }

            if (string.IsNullOrEmpty(mess))
                return;

            if(_sceneData != null && _sceneData.ContainsKey(Scenes.Tree.ToString()))
            {
                mess += Scenes.Tree.ToString();
            }
            SendHttpMessageCtr.Instance.SendUIMessage(mess);
        }
    }

}
