using Com4Love.Qmax.Ctr;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Net.Protocols.ActorGame;
using Com4Love.Qmax.Net.Protocols.Base;
using Com4Love.Qmax.Net.Protocols.Stage;
using Com4Love.Qmax.Net.Protocols.User;
using Com4Love.Qmax.Tools;
using Com4Love.Tiled;
using DoPlatform;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com4Love.Qmax.Data
{
    public class QmaxModel : IDisposable
    {

        #region Game Configs
        public GameSystemConfig GameSystemConfig { get { return confContainer.GameSystemConfig; } }

        /// <summary>
        /// 地形物配置，id -> ObjectConfig
        /// </summary>
        public Dictionary<int, TileObjectConfig> TileObjectConfigs { get { return confContainer.TileObjectConfigs; } }


        /// <summary>
        /// 關卡配置
        /// </summary>
        public Dictionary<int, StageConfig> StageConfigs { get { return confContainer.StageConfigs; } }

        /// <summary>
        /// 技能配置
        /// </summary>
        public Dictionary<int, SkillConfig> SkillConfigs { get { return confContainer.SkillConfigs; } }

        /// <summary>
        /// 夥伴配置
        /// </summary>
        public Dictionary<int, UnitConfig> UnitConfigs { get { return confContainer.UnitConfigs; } }

        /// <summary>
        /// Combo配置
        /// </summary>
        public Dictionary<int, ComboConfig> ComboConfigs { get { return confContainer.ComboConfigs; } }

        /// <summary>
        /// 抽獎配置
        /// </summary>
        public Dictionary<int, GetChanceConfig> GetChanceConfigs { get { return confContainer.GetChanceConfigs; } }

        /// <summary>
        /// 以中文為key的語言配置表
        /// </summary>
        public Dictionary<string, string> LanguageConfigs { get { return confContainer.LanguageConfigs; } }

        /// <summary>
        /// 以Id為key的語言配置表
        /// </summary>
        public Dictionary<int, string> LanguageConfigsById { get { return confContainer.LanguageConfigsById; } }

        /// <summary>
        /// 以statusCode為key的語言配置表
        /// </summary>
        public Dictionary<int, string> LanguageConfigsByStatusCode { get { return confContainer.LanguageConfigsByStatusCode; } }


        /// <summary>
        /// 購買鑽石的配置，從服務器獲取
        /// </summary>
        public Dictionary<string, PaymentSystemConfig> PaymentSystemConfigs { get { return confContainer.PaymentSystemConfigs; } }

        public Dictionary<int, PaymentChanConfig> PaymentChanConfigs { get { return confContainer.PaymentChanConfigs; } }

        public List<StageModelSettingConfig> StageModelSettingConfigs { get { return confContainer.StageModelSettingConfigs; } }

        public Dictionary<int, LoginRewardConfig> LoginRewardConfigs { get { return confContainer.LoginRewardConfigs; } }
        public Dictionary<int, LoadingConfig> LoadingConfigs { get { return confContainer.LoadingConfigs; } }

        /// <summary>
        /// 隨機生成掉落種子配置
        /// </summary>
        public Dictionary<string, RandomSeedConfig> RandomSeedConfigs { get { return confContainer.RandomSeedConfigs; } }

        /// <summary>
        /// 對話表配置
        /// </summary>
        public Dictionary<string, DialogConfig> DialogConfigs { get { return confContainer.DialogConfigs; } }

        /// <summary>
        /// 引導配置 
        /// </summary>
        public Dictionary<int, GuideConfig> GuideConfigs { get { return confContainer.GuideConfigs; } }

        ///教學步數對應教學id
        public Dictionary<int, GuideStepIdConfig> GuideStepIDConfigs { get { return confContainer.GuideStepIDConfigs; } }

        //TimeLimitedHPConfig的xml分兩個字段存放 一個以ID為key。一個level為key
        /// <summary>
        ///大樹hp的獎勵 以level為key， 當某level有多條數據的情況下，只存第一條
        /// </summary>
        public Dictionary<int, TimeLimitedHPConfig> TimeLimitedHPLvlConfigs { get { return confContainer.TimeLimitedHPLvlConfigs; } }
        //以id為key
        public Dictionary<int, TimeLimitedHPConfig> TimeLimitedHPIdConfigs { get { return confContainer.TimeLimitedHPIdConfigs; } }
        /// <summary>
        /// 攻擊力獎勵
        /// </summary>
        public List<TimeLimitedSummaryConfig> TimeLimitedSummaryConfigs { get { return confContainer.TimeLimitedSummaryConfigs; } }
        /// <summary>
        /// 連線獎勵
        /// </summary>
        public Dictionary<int, TimeLimitedLineConfig> TimeLimitedLineConfigs { get { return confContainer.TimeLimitedLineConfigs; } }


        public TestData TestData;


        /// <summary>
        /// 成就配置
        /// </summary>
        public Dictionary<int, AchieveConfig> AchieveConfigs { get { return confContainer.AchieveConfigs; } }

        ///兌換碼配置表///
        public Dictionary<int, ExchangeCodeConfig> ExchangeCodeConfigs { get { return confContainer.ExchangeCodeConfigs; } }

        /// <summary>
        /// 道具配置
        /// </summary>
        public Dictionary<int, GoodsConfig> GoodsConfigs { get { return confContainer.GoodsConfigs; } }

        /// <summary>
        /// 商店配置
        /// </summary>
        public Dictionary<int, ShopConfig> ShopConfigs { get { return confContainer.ShopConfigs; } }
        /// <summary>
        /// 單個物品對應的 商城購買項 shop表裡，如果購買數量是1的就表示是單個物品的價格
        /// </summary>
        public Dictionary<int, ShopConfig> SingleGoodsShopConfig { get { return confContainer.SingleGoodsShopConfig; } }
        public List<ShopConfig> ShopConfigList { get { return confContainer.ShopConfigList; } }

        /// <summary>
        /// 按照tap來存儲商店配置
        /// </summary>
        public Dictionary<int, List<ShopConfig>> ShopConfigByTap { get { return confContainer.ShopConfigByTap; } }

        #endregion Config


        private LevelConfigContainer lvConfContainer;

        /// <summary>
        /// 玩家數據
        /// </summary>
        public ActorGameResponse PlayerData;

        /// <summary>
        /// 加載配置數量
        /// </summary>
        private int _loadCfgNum = 0;

        public int LoadCfgNum
        {
            get { return _loadCfgNum; }
        }


        private ActorLoginResponse _loginData;
        /// <summary>
        /// 登錄數據   
        /// </summary>
        public ActorLoginResponse LoginData
        {
            get { return _loginData; }
            set
            {
                //考慮value == null的情況
                _loginData = value;

                if (_loginData != null)
                {
                    long id = _loginData.actorId;
                    int isCanPushValue = PlayerPrefs.GetInt("isCanPush" + id, 1);
                    Q.Log("isCanPush >> " + isCanPushValue);

                    if (isCanPushValue == 1)
                        IsCanMessagePush = true;
                    else
                        IsCanMessagePush = false;
                }
            }
        }

        /// <summary>
        ///HTTP獲取到的socket地址信息 一開始就被qmaxmodel初始化
        /// </summary>
        public SdkLoginData SdkData;


        /// <summary>
        /// 關卡信息的緩存
        /// </summary>
        public Dictionary<int, Stage> Stages = new Dictionary<int, Stage>();
        /// <summary>
        /// 將活動關卡的ID數據單獨存起來
        /// </summary>
        public List<int> ActiveStageIds = new List<int>();

        /// <summary>
        /// 所有關卡的星數總和 所有關卡的的星數會在基本信息中返回，此時計算星數總和。通過一關後有可能需要增加此數值
        /// </summary>
        public int allStageStar = 0;

        /// <summary>
        ///上一場戰鬥是否通過一個新關卡 玩家從地圖場景進入到其它場景後將此值設置成false。表示再回戰斗場景時不需要自動開啟下一關
        /// </summary>
        public bool IsStagePassedInLastFight;

        /// <summary>
        /// 通關前的最高關卡
        /// </summary>
        public int PrePassStageId = 0;

        /// <summary>
        /// 最高關卡，打完最高關卡後遊戲通關
        /// </summary>
        public int LastStageId { get { return confContainer.LastStageId; } }

        /// <summary>
        /// 推送的數據，待改成強類型
        /// </summary>
        public JSONInStream PushData = null;


        /// <summary>
        /// 建議配置和動態數據進行分類
        /// </summary>
        private bool isCanPush;


        /// <summary>
        /// 專門用於加載數據配置
        /// </summary>
        private DataConfigContainer confContainer;

        public bool IsCanMessagePush
        {
            get { return isCanPush; }
            set
            {
                isCanPush = value;
                long id = LoginData.actorId;
                PlayerPrefs.SetInt("isCanPush" + id, isCanPush ? 1 : 0);
                GameController.Instance.SetPushSwitch(isCanPush);
            }
        }

        /// <summary>
        /// 大樹活動的數據 TreeActivityCtr中初始化
        /// </summary>
        public TreeActivityData TreeActivityData = new TreeActivityData();

        public BattleModelModifyAgent BattleModel;


        /// <summary>
        /// 登錄獎勵數據///
        /// </summary>
        public LoginGiveData LoginGiveData = new LoginGiveData();


        private Language _language = Language.ChineseTraditional;
        public Language Language
        {
            get
            {
                return _language;
            }
            set
            {
                if (_language != value)
                {
                    _language = value;
                    if (modelEventSystem.OnLanguageChange != null)
                        modelEventSystem.OnLanguageChange(_language);
                    ///測試---------------------------------
                    Debug.Log("LanguageChange!!");
                }
            }
        }

        protected ModelEventSystem modelEventSystem;

        public QmaxModel(ModelEventSystem modelEventSystem)
        {
            _loadCfgNum = 0;
            this.modelEventSystem = modelEventSystem;
            modelEventSystem.BeforeSceneChangeEvent += OnBeforeSceneChangeEvent;
            //這裡先建立一個BattleModel，因為有可能進來後就是戰鬥場景，不會觸發BeforeSceneChangeEvent
            //BattleModel = new BattleModelModifyAgent(this, modelEventSystem);

            confContainer = new DataConfigContainer();
        }

        public void Dispose()
        {
            if (modelEventSystem != null)
                modelEventSystem.BeforeSceneChangeEvent -= OnBeforeSceneChangeEvent;

            if (BattleModel != null)
                BattleModel = null;
        }


        /// <summary>
        /// 清理數據嗎，回到調用構造函數之後的狀態
        /// </summary>
        public void Clear(ModelEventSystem modelEventSystem)
        {
            PlayerData = null;
            LoginData = null;
            SdkData = null;
            Stages.Clear();
            allStageStar = 0;
            IsStagePassedInLastFight = false;
            PrePassStageId = 0;

            this.modelEventSystem = modelEventSystem;
            BattleModel = new BattleModelModifyAgent(this, modelEventSystem);
        }

        /// <summary>
        /// 從網絡獲取xml存入配置
        /// </summary>
        /// <param name="text"></param>
        public void GenPaymentSystemConfig(string text)
        {
            //此配置有時效性，允許多次刷新
            //PaymentSystemConfigs = new Dictionary<int, PaymentSystemConfig>();
            //XMLInStream stream = new XMLInStream(text);

            //stream.List("item", delegate(XMLInStream itemStream)
            //{
            //    PaymentSystemConfig config = new PaymentSystemConfig(itemStream);

            //    PaymentSystemConfigs.Add(config.PaymentId, config);
            //});
        }


        public void LoadInitCfg(Action<bool> callback)
        {
            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
                confContainer.LoadInitDataCfg(null, callback);
            }
            else
            {
                AssetBundleManager abMrg = GameController.Instance.AssetBundleMrg;

                abMrg.LoadDataConfig(delegate(AssetBundleManager.Code code, string assetBundleName, AssetBundle ab)
                {
                    Q.Assert(code == AssetBundleManager.Code.Success,
                        "QmaxModel:LoadConfigs Assert 2");
                    confContainer.LoadInitDataCfg(ab, callback);
                });
            }
        }

        /// <summary>
        /// 加載各種配置文件
        /// </summary>
        public void LoadConfigs(Action<bool> callback)
        {
            Action<bool> OnLoadConfDone = delegate(bool success)
            {
                if (modelEventSystem.OnModelReady != null)
                    modelEventSystem.OnModelReady();

                if (callback != null)
                    callback(success);
            };


            if (PackageConfig.IsLoadAssetBundleFromLocal)
            {
                GameController.Instance.MonoBeh.StartCoroutine(confContainer.LoadDataConfig(null, OnLoadConfDone));
                lvConfContainer = new LevelConfigContainer(null);
            }
            else
            {
                AssetBundleManager abMrg = GameController.Instance.AssetBundleMrg;
                //先加载LevelConfig，再加载DataConfig
                //由於解析DataConfig需要比較久，所以用Coroutine來處理

                AssetBundleManager.LoadCallback LoadLvConfDone =
                    delegate(AssetBundleManager.Code code, string assetBundleName, AssetBundle ab)
                    {
                        Q.Assert(code == AssetBundleManager.Code.Success,
                            "QmaxModel:LoadConfigs Assert 1");

                        if (code != AssetBundleManager.Code.Success)
                        {
                            OnLoadConfDone(false);
                            return;
                        }

                        GameController.Instance.MonoBeh.StartCoroutine(confContainer.LoadDataConfig(ab, OnLoadConfDone));
                    };

                abMrg.LoadLevelConfig(delegate(AssetBundleManager.Code code, string assetBundleName, AssetBundle ab)
                {
                    Q.Assert(code == AssetBundleManager.Code.Success,
                        "QmaxModel:LoadConfigs Assert 2");

                    lvConfContainer = new LevelConfigContainer(ab);
                    if (code == AssetBundleManager.Code.Success)
                        Q.Assert(ab.GetAllAssetNames().Length > 50);

                    abMrg.LoadDataConfig(LoadLvConfDone);
                });
            }
        }



        public LevelConfig GetBattleLevel(int lv)
        {
            StageConfig stageConf = StageConfigs[lv];
            int levelRandom = UnityEngine.Random.Range(1, stageConf.SettingNum + 1);
            string levelName = string.Format("{0}_{1}", stageConf.GameSetting, levelRandom);
            return GetBattleLevel(levelName);
        }

        public LevelConfig GetBattleLevel(string levelName)
        {
            return lvConfContainer.GetBattleLevel(levelName);
        }


        /// <summary>
        /// 平台是否直購
        /// </summary>
        /// <returns></returns>
        public bool IsPaymentChan()
        {
            bool result = false;
            foreach (PaymentChanConfig item in PaymentChanConfigs.Values)
            {
                if (item.ChannelSdk == SdkData.channalID)
                {
                    if (item.PaymentChan == 2)
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            return result;
        }


        private void OnBeforeSceneChangeEvent(Scenes preScene, Scenes nextScene)
        {
            if (preScene == Scenes.BattleScene && BattleModel != null)
                BattleModel = null;

            if (nextScene == Scenes.BattleScene)
                BattleModel = new BattleModelModifyAgent(this, modelEventSystem);
        }

        public class SdkLoginData
        {
            public GetAddressResponse GetAddressResponse;
            public string token;
            public string channalID;
            public string platformID;
            public string sdkVersion;
            public string UserName;
            public string appKey;
            public string userId;
            public long actorId;
        }
    }
}
