using Com4Love.Qmax.Data;
using Com4Love.Qmax.Data.Config;
using Com4Love.Qmax.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Com4Love.Qmax.Ctr
{
    /// <summary>
    /// 專門用於加載數據配置的容器類
    /// </summary>
    public class DataConfigContainer
    {
        static private string[] ConfigFiles = { 
                                             "Assets/ExternalRes/Config/GameSystemConfig.xml",
                                             "Assets/ExternalRes/Config/RandomSeedConfig.xml",
                                             "Assets/ExternalRes/Config/DialogConfig.xml",
                                             "Assets/ExternalRes/Config/GuideConfig.xml",
                                             "Assets/ExternalRes/Config/ObjectConfig.xml",
                                             "Assets/ExternalRes/Config/StageConfig.xml",
                                             "Assets/ExternalRes/Config/SkillConfig.xml",
                                             "Assets/ExternalRes/Config/UnitConfig.xml",
                                             "Assets/ExternalRes/Config/ComboConfig.xml",
                                             "Assets/ExternalRes/Config/TestData.xml",
                                             "Assets/ExternalRes/Config/GetChanceConfig.xml",
                                             "Assets/ExternalRes/Config/LanguageConfig.xml",
                                             "Assets/ExternalRes/Config/StageModelSettingConfig.xml",
                                             "Assets/ExternalRes/Config/LoginRewardConfig.xml",
                                             "Assets/ExternalRes/Config/LoadingConfig.xml",
                                             "Assets/ExternalRes/Config/TimeLimitedHPConfig.xml",
                                             "Assets/ExternalRes/Config/TimeLimitedSummaryConfig.xml",
                                             "Assets/ExternalRes/Config/TimeLimitedLineConfig.xml",
                                             "Assets/ExternalRes/Config/AchieveConfig.xml",
                                             "Assets/ExternalRes/Config/GiftSpringwindowConfig.xml",
                                             "Assets/ExternalRes/Config/GoodsConfig.xml",
                                             "Assets/ExternalRes/Config/ShopConfig.xml",
											 //不同平台加載不同的PaymentConfig文件
#if UNITY_IOS
                                             "Assets/ExternalRes/Config/PaymentSystemIOSConfig.xml",
#else
                                             "Assets/ExternalRes/Config/PaymentSystemConfig.xml",
#endif
                                             "Assets/ExternalRes/Config/PaymentChanConfig.xml",
                                             "Assets/ExternalRes/Config/GuideStepIDConfig.xml",
                                             };

        #region Game Configs
        public GameSystemConfig GameSystemConfig;

        /// <summary>
        /// 地形物配置，id -> ObjectConfig
        /// </summary>
        public Dictionary<int, TileObjectConfig> TileObjectConfigs;


        /// <summary>
        /// 關卡配置
        /// </summary>
        public Dictionary<int, StageConfig> StageConfigs;

        /// <summary>
        /// 技能配置
        /// </summary>
        public Dictionary<int, SkillConfig> SkillConfigs;

        /// <summary>
        /// 夥伴配置
        /// </summary>
        public Dictionary<int, UnitConfig> UnitConfigs;

        /// <summary>
        /// Combo配置
        /// </summary>
        public Dictionary<int, ComboConfig> ComboConfigs;

        /// <summary>
        /// 抽獎配置
        /// </summary>
        public Dictionary<int, GetChanceConfig> GetChanceConfigs;

        /// <summary>
        /// 以中文為key的語言配置表
        /// </summary>
        public Dictionary<string, string> LanguageConfigs;

        /// <summary>
        /// 以Id為key的語言配置表
        /// </summary>
        public Dictionary<int, string> LanguageConfigsById;

        /// <summary>
        /// 以statusCode為key的語言配置表
        /// </summary>
        public Dictionary<int, string> LanguageConfigsByStatusCode;


        /// <summary>
        /// 購買鑽石的配置，從服務器獲取
        /// </summary>
        public Dictionary<string, PaymentSystemConfig> PaymentSystemConfigs;

        public Dictionary<int, PaymentChanConfig> PaymentChanConfigs;

        public List<StageModelSettingConfig> StageModelSettingConfigs;

        public Dictionary<int, LoginRewardConfig> LoginRewardConfigs;
        public Dictionary<int, LoadingConfig> LoadingConfigs;

        /// <summary>
        /// 隨機生成掉落種子配置
        /// </summary>
        public Dictionary<string, RandomSeedConfig> RandomSeedConfigs;

        /// <summary>
        /// 對話表配置
        /// </summary>
        public Dictionary<string, DialogConfig> DialogConfigs;

        /// <summary>
        /// 引導配置 
        /// </summary>
        public Dictionary<int, GuideConfig> GuideConfigs;

        /// <summary>
        /// 引導步數對應教學id
        /// </summary>
        public Dictionary<int, GuideStepIdConfig> GuideStepIDConfigs;

        //TimeLimitedHPConfig的xml分两个字段存放  一个以ID为key。 一个level为key
        /// <summary>
        /// 大樹hp的獎勵     以level為key， 當某level有多條數據的情況下，只存第一條
        /// </summary>
        public Dictionary<int, TimeLimitedHPConfig> TimeLimitedHPLvlConfigs;
        //以id為key
        public Dictionary<int, TimeLimitedHPConfig> TimeLimitedHPIdConfigs;
        /// <summary>
        /// 攻擊力獎勵
        /// </summary>
        public List<TimeLimitedSummaryConfig> TimeLimitedSummaryConfigs;
        /// <summary>
        /// 連線獎勵
        /// </summary>
        public Dictionary<int, TimeLimitedLineConfig> TimeLimitedLineConfigs;

        //public Dictionary<int, TimeLimitedHPConfig> TimeLimitedHPConfigs;

        public TestData TestData;


        /// <summary>
        /// 成就配置
        /// </summary>
        public Dictionary<int, AchieveConfig> AchieveConfigs;

        ///兌換碼配置表///
        public Dictionary<int, ExchangeCodeConfig> ExchangeCodeConfigs;

        /// <summary>
        /// 道具配置
        /// </summary>
        public Dictionary<int, GoodsConfig> GoodsConfigs;

        /// <summary>
        /// 商店配置
        /// </summary>
        public Dictionary<int, ShopConfig> ShopConfigs;
        /// <summary>
        /// 單個物品對應的 商城購買項 shop表裡，如果購買數量是1的就表示是單個物品的價格
        /// </summary>
        public Dictionary<int, ShopConfig> SingleGoodsShopConfig;
        public List<ShopConfig> ShopConfigList;

        /// <summary>
        /// 按照tap來存儲商店配置
        /// </summary>
        public Dictionary<int, List<ShopConfig>> ShopConfigByTap;

        #endregion Config


        /// <summary>
        /// 加載配置數量
        /// </summary>
        private int _loadCfgNum = 0;

        /// <summary>
        /// 最高關卡，打完最高關卡後遊戲通關
        /// </summary>
        public int LastStageId = 0;


        /// <summary>
        /// 加載初始配置，這些配置在獲取AssetBundle信息之前就要拿到
        /// </summary>
        /// <param name="dataConfAB"></param>
        /// <param name="result"></param>
        public void LoadInitDataCfg(AssetBundle dataConfAB, Action<bool> result)
        {
            Func<AssetBundle, string, TextAsset> LoadTextAsset = delegate(AssetBundle ab, string path)
            {
#if UNITY_EDITOR
                return UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
#else
                return ab.LoadAsset<TextAsset>(path);
#endif
            };

            TextAsset textAsset = LoadTextAsset(dataConfAB, "Assets/ExternalRes/Config/LanguageConfig.xml");
            string content = textAsset.text;
            Loom.RunAsync(delegate()
            {
                LanguageConfigs = new Dictionary<string, string>();
                LanguageConfigsById = new Dictionary<int, string>();
                LanguageConfigsByStatusCode = new Dictionary<int, string>();

                XMLInStream stream = new XMLInStream(content);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        LanguageConfig cfg = new LanguageConfig(itemStream);

                        if (!LanguageConfigs.ContainsKey(cfg.CNS))
                            LanguageConfigs.Add(cfg.CNS, cfg.Text);
                        else
                            Q.Warning("LanguageConfig有重複id={0}, cns={1}", cfg.ID, cfg.CNS);

                        LanguageConfigsById.Add(cfg.ID, cfg.Text);

                        //-1是特殊Status
                        if (cfg.Statuscode != -1)
                            LanguageConfigsByStatusCode.Add(cfg.Statuscode, cfg.Text);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
            });

            if (dataConfAB != null)
            {
                dataConfAB.Unload(true);
            }
        }

        /// <summary>
        /// 用Coroutine解析DataConfig
        /// </summary>
        /// <param name="dataConfAB"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator LoadDataConfig(AssetBundle dataConfAB, Action<bool> callback)
        {
            Func<AssetBundle, string, TextAsset> LoadTextAsset = delegate(AssetBundle ab, string path)
            {                
#if UNITY_EDITOR
                return UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
#else
                return ab.LoadAsset<TextAsset>(path);
#endif
            };


            TextAsset textAsset0 = LoadTextAsset(dataConfAB, ConfigFiles[0]);
            string content0 = textAsset0.text;

            //RandomSeedConfig
            TextAsset textAsset1 = LoadTextAsset(dataConfAB, ConfigFiles[1]);
            string content1 = textAsset1.text;

            yield return 0;
            //DialogConfig
            TextAsset textAsset2 = LoadTextAsset(dataConfAB, ConfigFiles[2]);
            string content2 = textAsset2.text;

            yield return 0;
            //GuideConfig
            TextAsset textAsset3 = LoadTextAsset(dataConfAB, ConfigFiles[3]);
            string content3 = textAsset3.text;

            yield return 0;
            TextAsset textAsset4 = LoadTextAsset(dataConfAB, ConfigFiles[4]);
            string content4 = textAsset4.text;

            yield return 0;
            TextAsset textAsset5 = LoadTextAsset(dataConfAB, ConfigFiles[5]);
            string content5 = textAsset5.text;

            yield return 0;
            TextAsset textAsset6 = LoadTextAsset(dataConfAB, ConfigFiles[6]);
            string content6 = textAsset6.text;

            yield return 0;
            TextAsset textAsset7 = LoadTextAsset(dataConfAB, ConfigFiles[7]);
            string content7 = textAsset7.text;

            yield return 0;
            TextAsset textAsset8 = LoadTextAsset(dataConfAB, ConfigFiles[8]);
            string content8 = textAsset8.text;

            yield return 0;
            TextAsset textAsset9 = LoadTextAsset(dataConfAB, ConfigFiles[9]);
            string content9 = textAsset9.text;

            yield return 0;
            TextAsset textAsset10 = LoadTextAsset(dataConfAB, ConfigFiles[10]);
            string content10 = textAsset10.text;

            yield return 0;
            TextAsset textAsset11 = LoadTextAsset(dataConfAB, ConfigFiles[11]);
            string content11 = textAsset11.text;

            yield return 0;
            TextAsset textAsset12 = LoadTextAsset(dataConfAB, ConfigFiles[12]);
            string content12 = textAsset12.text;

            yield return 0;
            TextAsset textAsset13 = LoadTextAsset(dataConfAB, ConfigFiles[13]);
            string content13 = textAsset13.text;

            yield return 0;
            TextAsset textAsset14 = LoadTextAsset(dataConfAB, ConfigFiles[14]);
            string content14 = textAsset14.text;

            yield return 0;
            TextAsset textAsset15 = LoadTextAsset(dataConfAB, ConfigFiles[15]);
            string content15 = textAsset15.text;

            yield return 0;
            TextAsset textAsset16 = LoadTextAsset(dataConfAB, ConfigFiles[16]);
            string content16 = textAsset16.text;

            yield return 0;
            TextAsset textAsset17 = LoadTextAsset(dataConfAB, ConfigFiles[17]);
            string content17 = textAsset17.text;

            yield return 0;
            TextAsset textAsset18 = LoadTextAsset(dataConfAB, ConfigFiles[18]);
            string content18 = textAsset18.text;

            yield return 0;
            TextAsset textAsset19 = LoadTextAsset(dataConfAB, ConfigFiles[19]);
            string content19 = textAsset19.text;

            yield return 0;
            TextAsset textAsset20 = LoadTextAsset(dataConfAB, ConfigFiles[20]);
            string content20 = textAsset20.text;

            yield return 0;
            TextAsset textAsset21 = LoadTextAsset(dataConfAB, ConfigFiles[21]);
            string content21 = textAsset21.text;

            yield return 0;            
            TextAsset textAsset22 = LoadTextAsset(dataConfAB, ConfigFiles[22]);
            string content22 = textAsset22.text;

            yield return 0;
            TextAsset textAsset23 = LoadTextAsset(dataConfAB, ConfigFiles[23]);
            string content23 = textAsset23.text;

            yield return 0;
            TextAsset textAsset24 = LoadTextAsset(dataConfAB, ConfigFiles[24]);
            string content24 = textAsset24.text;

            int[] array1 = new int[ConfigFiles.Length];
            for (int i = 0; i < array1.Length; i++)
            {
                array1[i] = 0;
            }
            Loom.RunAsync(delegate()
            {
                XMLInStream stream = new XMLInStream(content0);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        GameSystemConfig = new GameSystemConfig(itemStream);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[0] = 1;
                _loadCfgNum++;

                DialogConfigs = new Dictionary<string, DialogConfig>();
                stream = new XMLInStream(content2);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        DialogConfig cfg = new DialogConfig(itemStream);
                        string id = string.Format("{0}_{1}", cfg.UID, cfg.ID);
                        DialogConfigs.Add(id, cfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });

                array1[1] = 1;
                _loadCfgNum++;



                GuideConfigs = new Dictionary<int, GuideConfig>();
                stream = new XMLInStream(content3);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        GuideConfig cfg = new GuideConfig(itemStream, DialogConfigs);
                        GuideConfigs.Add(cfg.UID, cfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });

                array1[2] = 1;
                _loadCfgNum++;

                TileObjectConfigs = new Dictionary<int, TileObjectConfig>();
                stream = new XMLInStream(content4);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        TileObjectConfig cfg = new TileObjectConfig(itemStream);
                        TileObjectConfigs.Add(cfg.ID, cfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[3] = 1;
                _loadCfgNum++;
                Q.Assert(TileObjectConfigs.Count > 0);


                RandomSeedConfigs = new Dictionary<string, RandomSeedConfig>();
                stream = new XMLInStream(content1);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        RandomSeedConfig cfg = new RandomSeedConfig(itemStream);
                        string id = string.Format("{0}_{1}", cfg.SeedType, cfg.SeedId);
                        RandomSeedConfigs.Add(id, cfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }

                });

                array1[4] = 1;
                _loadCfgNum++;

                StageConfigs = new Dictionary<int, StageConfig>();
                stream = new XMLInStream(content5);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        StageConfig sfg = new StageConfig(itemStream, RandomSeedConfigs);
                        StageConfigs.Add(sfg.ID, sfg);

                        if (LastStageId < sfg.ID)
                        {
                            LastStageId = sfg.ID;
                        }
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });

                array1[5] = 1;
                _loadCfgNum++;
                Q.Assert(StageConfigs.Count > 0);


                //Q.LogElapsedSecAndReset(confID, "LoadConfigs 9");
                SkillConfigs = new Dictionary<int, SkillConfig>();

                stream = new XMLInStream(content6);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        SkillConfig skfg = new SkillConfig(itemStream);
                        SkillConfigs.Add(skfg.ID, skfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[6] = 1;
                _loadCfgNum++;
                Q.Assert(SkillConfigs.Count > 0);

                UnitConfigs = new Dictionary<int, UnitConfig>();
                stream = new XMLInStream(content7);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        UnitConfig ufg = new UnitConfig(itemStream);
                        UnitConfigs.Add(ufg.ID, ufg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                Q.Assert(UnitConfigs.Count > 0);
                array1[7] = 1;
                _loadCfgNum++;

                ComboConfigs = new Dictionary<int, ComboConfig>();

                stream = new XMLInStream(content8);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        ComboConfig cfg = new ComboConfig(itemStream);
                        ComboConfigs.Add(cfg.Combo, cfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[8] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content9);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    TestData = new TestData(itemStream);
                });
                array1[9] = 1;
                _loadCfgNum++;

                GetChanceConfigs = new Dictionary<int, GetChanceConfig>();
                stream = new XMLInStream(content10);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        GetChanceConfig gfg = new GetChanceConfig(itemStream);
                        GetChanceConfigs.Add(gfg.ID, gfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[10] = 1;
                _loadCfgNum++;

                LanguageConfigs = new Dictionary<string, string>();
                LanguageConfigsById = new Dictionary<int, string>();
                LanguageConfigsByStatusCode = new Dictionary<int, string>();

                stream = new XMLInStream(content11);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        LanguageConfig cfg = new LanguageConfig(itemStream);

                        if (!LanguageConfigs.ContainsKey(cfg.CNS))
                            LanguageConfigs.Add(cfg.CNS, cfg.Text);
                        else
                            Q.Warning("LanguageConfig有重复id={0}, cns={1}", cfg.ID, cfg.CNS);

                        LanguageConfigsById.Add(cfg.ID, cfg.Text);

                        //-1是特殊Status
                        if (cfg.Statuscode != -1)
                            LanguageConfigsByStatusCode.Add(cfg.Statuscode, cfg.Text);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[11] = 1;
                _loadCfgNum++;


                stream = new XMLInStream(content12);
                StageModelSettingConfigs = new List<StageModelSettingConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        StageModelSettingConfig stageMConfig = new StageModelSettingConfig(itemStream);
                        StageModelSettingConfigs.Add(stageMConfig);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[12] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content13);
                LoginRewardConfigs = new Dictionary<int, LoginRewardConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        LoginRewardConfig lRewardConfig = new LoginRewardConfig(itemStream);
                        LoginRewardConfigs.Add(lRewardConfig.Id, lRewardConfig);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[13] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content14);
                LoadingConfigs = new Dictionary<int, LoadingConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        LoadingConfig config = new LoadingConfig(itemStream);
                        LoadingConfigs.Add(config.ID, config);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[14] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content15);
                TimeLimitedHPLvlConfigs = new Dictionary<int, TimeLimitedHPConfig>();
                TimeLimitedHPIdConfigs = new Dictionary<int, TimeLimitedHPConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        TimeLimitedHPConfig hpConfig = new TimeLimitedHPConfig(itemStream);
                        TimeLimitedHPIdConfigs.Add(hpConfig.ID, hpConfig);

                        if (!TimeLimitedHPLvlConfigs.ContainsKey(hpConfig.Level))
                        {
                            TimeLimitedHPLvlConfigs.Add(hpConfig.Level, hpConfig);
                        }
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[15] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content16);
                TimeLimitedSummaryConfigs = new List<TimeLimitedSummaryConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    TimeLimitedSummaryConfig summaryConfig = new TimeLimitedSummaryConfig(itemStream);
                    //確認是否需要手動根據id排序
                    TimeLimitedSummaryConfigs.Add(summaryConfig);
                });
                array1[16] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content17);
                TimeLimitedLineConfigs = new Dictionary<int, TimeLimitedLineConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        TimeLimitedLineConfig lineConfig = new TimeLimitedLineConfig(itemStream);
                        TimeLimitedLineConfigs.Add(lineConfig.ID, lineConfig);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[17] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content18);
                AchieveConfigs = new Dictionary<int, AchieveConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        AchieveConfig config = new AchieveConfig(itemStream);
                        AchieveConfigs.Add(config.AchieveId, config);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[18] = 1;
                _loadCfgNum++;

                stream = new XMLInStream(content19);
                ExchangeCodeConfigs = new Dictionary<int, ExchangeCodeConfig>();
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        ExchangeCodeConfig config = new ExchangeCodeConfig(itemStream);
                        ExchangeCodeConfigs.Add(config.GiftID, config);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[19] = 1;
                _loadCfgNum++;

                GoodsConfigs = new Dictionary<int, GoodsConfig>();
                stream = new XMLInStream(content20);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        GoodsConfig cfg = new GoodsConfig(itemStream);
                        GoodsConfigs.Add(cfg.UID, cfg);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[20] = 1;
                _loadCfgNum++;


                ShopConfigs = new Dictionary<int, ShopConfig>();
                SingleGoodsShopConfig = new Dictionary<int, ShopConfig>();
                ShopConfigList = new List<ShopConfig>();
                ShopConfigByTap = new Dictionary<int, List<ShopConfig>>();


                stream = new XMLInStream(content21);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        ShopConfig cfg = new ShopConfig(itemStream);
                        ShopConfigList.Add(cfg);
                        ShopConfigs.Add(cfg.UID, cfg);

                        if (cfg.ByNum == 1 && !SingleGoodsShopConfig.ContainsKey(cfg.GoodsId))
                        {
                            SingleGoodsShopConfig.Add(cfg.GoodsId, cfg);
                        }

                        if (ShopConfigByTap.ContainsKey(cfg.Tab))
                            ShopConfigByTap[cfg.Tab].Add(cfg);
                        else
                            ShopConfigByTap.Add(cfg.Tab, new List<ShopConfig>());
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[21] = 1;
                _loadCfgNum++;

                ShopConfigList.Sort(ShopItemListSortCompare);

                PaymentSystemConfigs = new Dictionary<string, PaymentSystemConfig>();
                stream = new XMLInStream(content22);
                //Debug.Log("解析PaymentConfig\n " + content22);
                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        PaymentSystemConfig config = new PaymentSystemConfig(itemStream);
                        PaymentSystemConfigs.Add(config.PaymentId, config);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[22] = 1;
                _loadCfgNum++;

                ShopConfigList.Sort(ShopItemListSortCompare);

                PaymentChanConfigs = new Dictionary<int, PaymentChanConfig>();
                stream = new XMLInStream(content23);

                stream.List("item", delegate(XMLInStream itemStream)
                {
                    try
                    {
                        PaymentChanConfig config = new PaymentChanConfig(itemStream);
                        PaymentChanConfigs.Add(config.ID, config);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[23] = 1;
                _loadCfgNum++;

                GuideStepIDConfigs = new Dictionary<int, GuideStepIdConfig>();
                stream = new XMLInStream(content24);

                stream.List("item", delegate (XMLInStream itemStream)
                {
                    try
                    {
                        GuideStepIdConfig config = new GuideStepIdConfig(itemStream);
                        GuideStepIDConfigs.Add(config.GuideStep, config);
                    }
                    catch (Exception e)
                    {
                        Q.Error(e);
                    }
                });
                array1[24] = 1;
                _loadCfgNum++;
            });

            yield return 0;

            int totalCfgNum = ConfigFiles.Length;
            while (true)
            {
                string arrStr = "";
                for (int i = 0; i < array1.Length; i++)
                {
                    arrStr = string.Format("{0}_{1}", arrStr, array1[i].ToString());
                }

                Q.Log("配置表完成標誌位：{0}/{1} {2}", _loadCfgNum, totalCfgNum, arrStr);
                if (_loadCfgNum >= ConfigFiles.Length)
                {
                    break;
                }
                yield return 0;
            }

            if (dataConfAB != null)
            {
                dataConfAB.Unload(true);
            }


            if (callback != null)
                callback(true);
        }


        private int ShopItemListSortCompare(ShopConfig a, ShopConfig b)
        {
            int res = 0;

            if (a.SortId > b.SortId)
            {
                res = 1;
            }
            else if (a.SortId < b.SortId)
            {
                res = -1;
            }

            return res;
        }

    }
}
