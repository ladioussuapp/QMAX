namespace Com4Love.Qmax
{
    //方向
    public enum Direction
    {
        None,
        //U=Up, D=Down, R=Right, L=Left
        R, DR, D, DL, L, UL, U, UR
    }


    /// <summary>
    /// 消除物類型
    /// </summary>
    public enum ElementType : int
    {
        NotElement = 0,
        /// <summary>
        /// 普通消除物
        /// </summary>
        Normal,
        /// <summary>
        /// 炸弹
        /// </summary>
        Bomb,
        /// <summary>
        /// 轉換石
        /// </summary>
        ConvertBlock,
        /// <summary>
        /// 多色石
        /// </summary>
        MultiColor
    }

    /// <summary>
    /// 消除範圍的類型
    /// </summary>
    public enum ElimRangeMode
    {
        Normal = 0,
        Horizontal,
        Vertical,
        Diamond,
        Rect,
    }

    public enum RewardType : byte
    {
        Good = 0,
        Key = 1,
        UpgradeA = 2,
        UpgradeB = 3,

        Gem = 4,

        /// <summary>
        /// 體力上限
        /// </summary>
        MaxEnergy = 5,

        /// <summary>
        /// 體力
        /// </summary>
        Energy = 6,

        Unit = 7,

        /// <summary>
        /// 可溢出體力
        /// </summary>
        ExtraEnergy = 8,

        /// <summary>
        /// 金幣
        /// </summary>
        Coin = 11,
    }


    /// <summary>
    /// 怪物的動作名
    /// </summary>
    public class EnemyAnim
    {
        public const string IDLE = "Idle";
        public const string ATTACK = "Attack";
        public const string HIT = "Hit";
        public const string DIE = "Die";
        public const string WEAK_IDLE = "Weak_Idle";
        public const string WEAK_HIT = "Weak_Hit";
        public const string WEAK_ATTACK = "Weak_Attack";
        public const string WEAK_DIE = "Weak_Die";
        public const string SHOCK = "Shock";
    }

    /// <summary>
    /// 夥伴的動作名
    /// </summary>
    public class GuyAnim
    {
        public const string IDLE = "Idle";
        public const string IDLE2 = "Idle2";
        public const string ATTACK = "Attack";
        public const string CHARGE = "Charge";
        public const string WIN = "Win";
        public const string HIT = "Hit";
        public const string SHOCK_LEFT = "Shock_left";
        public const string SHOCK_RIGHT = "Shock_right";
        public const string SKILL = "Skill";
    }

    /// <summary>
    /// 關卡的遊戲模式
    /// </summary>
    public enum StageMode
    {
        StepLimit = 1, TimeLimit = 2
    }


    /// <summary>
    /// 連接網絡類型
    /// </summary>
    public enum NetType
    {
        Outline, //離線數據
        Extranet, //外網
        Intranet, //內網
        Xiaolu, //小盧
        Beijing,//北京測試地址
        Ligang,
        Localhost,//本地
    }

    public enum TileType
    {
        //Tile=0 表示所有地形物
        Tile,
        Element,
        Cover,
        Obstacle,
        SeperatorH,
        SeperatorV,
        Bottom,
        Collect
    }

    public enum ColorType : int
    {
        None = 0,
        Earth = 1,
        Fire = 2,
        Wood = 3,
        Water = 4,
        Golden = 5,
        All = 6
    }

    public enum UnitAnimation
    {
        Idle,
        Walk,
        Charge,
        Attack,
        Skill,
        Win,
        Hit,
        Idle2,
        Shock_left,
        Shock_right
    }


    public struct Tags
    {
        public static string Plane = "Plane";
        public static string BattleCamera = "BattleCamera";
    }

    public struct Layer
    {
        public static string Battle = "Battle";
        public static string Touch = "Touch";
        public static string TextureRenderer = "TextureRenderer";
        public static string Default = "Default";
        public static string UI = "UI";
    }

    public enum Scenes
    {
        None,
        Bootstrap,
        MapScene,
        LoadingScene,
        BattleScene,
        test_scene,
        LoginScene,
        UpdateAssetsScene,
        Tree,
        Test3MatchStage
        //Test
    }

    public enum PopupID
    {
        None,
        UIPause,
        UIGetChanceUnitWin,
        UISelectHeroNew,
        UISelectHero,
        UIUpgradGoleTipWin,
        UISetting,
        UIPowerShop,
        UIExchangeCode,
        UILanguage,
        UIUpgradUnitInfoWin,
        UIUpgradWin,
        UIAlert,
        UIOut,
        UIAddMove,
        UIShop,
        UIUnlock,
        UIHotUpdate,
        UIWin,
        UILose,
        LightLoading,
        UINoticeWindow,
        UILoginWindow,
        UIReconnect,
        UILoginGive,
        UIAchievement,
        TestGem,
        UICommonDialog,
        UIGoodsWin,
        UIMail,
        UILeanShop,
        UIShopGoodsWindow,
        UIBattleTips,
        UISelectProp,
        UIGetChance,
        UIUpgrad,
        UILinkTips,
        UIUpgradeSkillTip,
        UIGoalTip
    }


    public enum AssetsType
    {
        Prefab, Texture
    }

    /// <summary>
    /// Login:登陸完成
    /// </summary>
    public enum LoginState
    {
        None,
        Register,
        Login,
        ReqGameServerInfo, //請求有遊戲服信息
        ConnectGameServer, //連接遊服
        CreateActor,
        ActorLogin,
        ReqPurchaseInfo, //請求購買信息
        AllDone = ReqPurchaseInfo, //全部過程完成。將這個值設置為何最後一個流程的值相同
    }


    /// <summary>
    /// 戰鬥勝利條件
    /// </summary>
    public enum BattleGoal
    {
        none = -1,  //沒有勝利條件 比如大樹
        Unit = 1, //消除敵人
        Object = 2, //消除物
        Score = 3 //得分數
    }

    /// <summary>
    /// 遊戲模式
    /// </summary>
    public enum BattleMode
    {
        Normal = 1, //普通模式
        TimeLimit = 2, //限時模式
        Tree            //大樹模式 
    }

    public enum SkillType
    {
        Guy = 1, //夥伴技能
        Enemy = 2, //敵人技能
        Shield = 3, //護盾技能
        Active = 4//主動技能
    }

    public struct UIAudioConfig
    {
        public static string MAP_SCENE_BUTTON_CLICK = "SD_ui_press";

        /// <summary>
        /// 進入遊戲
        /// </summary>
        public static string MAP_SCENE_ENTER_BATTLE = "SD_ui_loading_game";
        /// <summary>
        /// 地圖場景返回關閉按鈕 應該是地圖場景裡面的窗口關閉聲音
        /// </summary>
        public static string MAP_SCENE_BACK = "SD_ui_back1";

        /// <summary>
        /// 夥伴界面返回關閉按鈕聲音
        /// </summary>
        public static string BACK_TYPE2 = "SD_ui_back2";

        /// <summary>
        /// 夥伴界面頁面切換按鈕音效
        /// </summary>
        public static string UPGRAD_SCENE_PAGEBUTTON_CLICK = "SD_ui_switchover";

        /// <summary>
        /// 升級音效
        /// </summary>
        public static string UPGRAD_SCENE_UPGRAD = "SD_level_up";

        //Vo_unit_level_up_(顏色)_(階段的等級)_(後綴)
        //顏色：紫色1，紅色2，綠色3，藍色4，黃色5
        //階段的等級：1,2,3 1代表1階-2階，2代表3-4階，3代表5階
        //後綴：a,b,c
        public static string UPGRADE_UNIT_LEVELUP_ROOT = "Vo_level_up{0}_{1}_{2}";

        /// <summary>
        /// 數字循環滾動音效
        /// </summary>
        public static string NUMBER_ROLLING_LOOP = "SD_attack_tree_number_rolling_loop";
    }

    /// <summary>
    /// 圖集
    /// </summary>
    public enum Atlas
    {
        Tile,
        Common,
        UIWin,
        UILose,
        UIGetChance,
        UISelectUnitNew,
        UIExchangeCode,
        UIBattle,
        UIHotUpdate,
        UIGem,
        UIUpgradeBg,
        UIDialog,
        UIWifi,
        UINotice,
        UIMap,
        UILoginGive,
        UIComponent,
        //UIAlert,          //移除UIAlert。如果需要動態獲取彈出框的內容圖片，需要將內容圖片直接放到Resource下
        UIAchievement
    }

    public enum AtlasName
    {
        Tile0,
        Tile1,   // 將Tile拆分成2個圖集
        Tile2
    }

    public enum Language
    {
        English = 0,//英文
        ChineseSimplified = 1,//中文简体
        ChineseTraditional = 2,//中文繁體
    }

    /// <summary>
    /// 成就主類型
    /// </summary>
    public enum AchieveType
    {
        EveryData,  //每天
        Unit,       //夥伴
        Level,      //關卡
        Lottery,    //抽獎
        Stars,      //星星
        Friends,    //好友
        Stuff,      //材料
        Count
    }

    /// <summary>
    /// 成就子類型
    /// </summary>
    public enum AchieveSubType
    {
        Count
    }

    public enum OnOff
    {
        CacheMusic,
        CacheSound,
        Account,
        AccountPass,
        ChangeButtonMark,
        UpgradeButtonMark,
        FirstTimeMeet6203,
        FirstTimeMeet6261,
        FirstTimeBamboo,
        FirstTimeWin,
        FirstTimeLose,
        FirstTimeGitOrange,
        FirstTimeOpenUnitSelectWindow,
        FirstTimeSkill,
        TipAutoUpgradeMark,    //提示全部果實升級夥伴mark
        TipDontShowOnGetChance,
        OpenUILoginGive,  ///每天打開登陸獎勵時間///
        PauseCheckProp,
        PauseCheckUnit,
        StartMovie,
        None
    }

    public enum AchievementType : byte
    {
        /// <summary>
        /// 夥伴
        /// </summary>
        UNITS = 1,
        /// <summary>
        /// 關卡
        /// </summary>
        GATES = 2,
        /// <summary>
        /// 抽獎
        /// </summary>
        LOTTERY = 3,
        /// <summary>
        /// 星星
        /// </summary>
        STAR = 4,
        /// <summary>
        /// 好友
        /// </summary>
        FRIEND = 5,
        /// <summary>
        /// 材料
        /// </summary>
        MATERIAL = 6,
        /// <summary>
        /// 每日
        /// </summary>
        DALIY = 7,
        /// <summary>
        /// 消除
        /// </summary>
        ELIMINATE = 8,
        /// <summary>
        /// 所有
        /// </summary>
        ALL = 99
    }
    public enum PropType
    {
        None,
        /// <summary>
        /// 提升體力上限
        /// </summary>
        EnergyB = 42,
        /// <summary>
        ///被動道具//
        /// </summary>
        AddState = 101,
        ReduceMonsterHP = 102,
        ReduceGoal = 103,
        AddScore = 104,
        AddAward = 105,
        AddHurt = 106,

        /// <summary>
        /// 主動道具//
        /// </summary>
        EliminateOne = 201,
        EliminateColor = 202,
        RearrangeByColor = 203,
        NoCDSkill = 204
    }

    public enum BattleTips
    {
        None,
        Moves,
        Score,
        Goal,
        StarBG,
        Star1,
        Star2,
        Star3

    }
}
