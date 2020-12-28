namespace Com4Love.Qmax.Net
{
    /// <summary>
    /// 模塊名枚舉
    /// </summary>
    public enum Module : byte
    {
        Http = 0,
        User = 1,
        Unit = 2,
        ActorGame = 3,
        Energy = 4,
        Stage = 5,
        GetChance = 6,
        SIGN = 7,
        Activity = 10,
        Achievement = 11,
        Tree = 12,
        Goods = 13,
        Mail = 14,
        Counterpart = 15,
        Purchase = 16,
        Test = 100
    }

    public enum CounterpartCmd : byte
    {
        GET_STAGE_LIST = 1,
        BEGAIN_STAGE,
        SUBMIT_COUNTERPART_FIGHT
    }

    public enum HttpCmd : byte
    {
        LOGIN = 1,
        REGISTER = 2,
        //獲取遊服地址
        GET_ADDRESS = 3,

        DIS_CONNECT = 4,

        //公告 前端增加命令
        GET_NOTICE = 5,

        /// <summary>
        /// 獲取充值信息
        /// </summary>
        RECHARGE_INFO = 6,

        /// <summary>
        ///  推送充值結果 
        ///  推送:{@code ValueResultListResponse}
        /// </summary>
        PUSH_RECHARGE_RESULT = 7,
    }

    public enum UserCmd : byte
    {
        /// <summary>
        /// 心跳包
        /// </summary>   
        HEART_BEAT = 0,

        /// <summary>
        /// 用戶登陸 (第三方驗證)
        /// </summary>
        USER_LOGIN = 1,

        /// <summary>
        /// 獲取角色信息
        /// </summary>
        GET_ACTOR = 2,

        /// <summary>
        /// 創建角色
        /// </summary>
        CREATE_ACTOR = 3,

        /// <summary>
        /// 角色登陸
        /// <see cref="ActorLoginRequest"/>
        /// <see cref="ActorLoginResponse"/>
        /// </summary>
        ACTOR_LOGIN = 4,

        /// <summary>
        /// 推送角色屬性
        /// <see cref="ActorAttributeResponse"/>
        /// <see cref="UserDisabledResponse"/>
        /// </summary>
        PUSH_ACTOR_ATTRIBUTE = 5,

        /// <summary>
        /// 設置新手引導流程步驟
        /// <see cref="SaveGuidesStepRequest"/>
        /// </summary>
        SAVE_GUIDES_STEP = 7,

        /// <summary>
        /// 保存角色行為動作
        /// <see cref="ActorMontionRequest"/>
        /// </summary>
        SAVE_MONTION = 8,

        /// <summary>
        /// 踢人下線
        /// <see cref="KickOffResponse"/>
        /// </summary>
        KICK_OFF = 9,

        /// <summary>
        /// 用戶重連
        /// <see cref="UserReconnectRequest"/>
        /// <see cref="UserLoginResponse"/>
        /// </summary>
        USER_RECONNECTION = 10,

        /// <summary>
        /// 設置新手引導流程步驟
        /// <see cref="SavePushKeyRequest"/>
        /// </summary>
        SAVE_PUSH_KEY = 11,
    }

    public enum EnergyCmd : byte
    {
        /// <summary>
        /// 購買體力
        /// <see cref="BuyEnergyRequest"/>
        /// <see cref="ValueResultListResponse"/>
        /// </summary>
        BUY_ENERGY = 1,

        /// <summary>
        /// 提升體力上限
        /// <see cref="BuyMaxEnergyRequest"/>
        /// <see cref="ValueResultListResponse"/>
        /// </summary>
        INCRE_MAXENERGY = 2,

        /// <summary>
        /// 刷新體力
        /// <see cref=""/>
        /// <see cref="RefreshEnergyResponse"/>
        /// </summary>
        REFRESH_ENERGY = 3
    }

    public enum ActorGameCmd : byte
    {
        /// <summary>
        /// 獲取用戶信息
        /// <see cref="ActorGameResponse"/>
        /// </summary>
        GET_DATA = 1,

        /// <summary>
        /// 推送充值消息 
        /// </summary>
        PUSH_RECHARGE_RESULT = 2,
        /// <summary>
        /// 保存引導步驟
        /// </summary>
        SAVE_GUIDE = 3,

        /// <summary>
        /// 充值後刷新
        /// </summary>
        RECHARGE_REFRESH = 4

    }

    public enum StageCmd : byte
    {
        /// <summary>
        /// 獲取關卡最高通關記錄
        /// 請求:{@code }
        /// 響應:{@code StageInfoResponse}
        /// </summary>
        GET_STAGEINFO = 1,

        /// <summary>
        /// 通關關卡
        /// 請求:{@code ListStageDataRequest}
        /// 響應:{@code ListStageResponse}
        /// </summary>
        //PASS_STAGE = 2,

        /// <summary>
        /// 獲取關卡通關詳細信息
        /// 請求:{@code ListStageDataRequest}
        /// 響應:{@code ListStageResponse}
        /// </summary>
        LIST_STAGEDATA = 3,

        /// <summary>
        /// 獲取單個關卡通關詳細信息
        /// 請求:{@code StageDataRequest}
        /// 響應:{@code Stage}
        /// </summary>
        STAGE_DATA = 4,

        /// <summary>
        /// 開始關卡挑戰(並扣減體力)
        /// 請求:{@code BeginStageRequest}
        /// 響應:{@code ValueResultListResponse}
        /// </summary>
        STAGE_BEGIN = 5,

        /// <summary>
        /// 提交關卡挑戰結果(並扣減體力) (測試用)正式的為6
        /// 請求:{@code SubmitStageFightRequest}
        /// 響應:{@code SubmitStageFightResponse}
        /// </summary>        
        SUBMIT_STAGEFIGHT = 6,

        /// <summary>
        /// 同SUBMIT_STAGEFIGHT，此為測試API
        /// </summary>
        SUBMIT_STAGEFIGHT_TEST = 7,

        /**
       * 鑽石購買步數
       * 請求:{@code Request}
       * 響應:{@code BuyStepResponse}
       */
        GEM_BUY_STEP = 8,

        /**
         * 鑽石解鎖關卡
         * 請求:{@code UnlockStageRequest}
         * 響應:{@code ValueResultListResponse}
         */
        GEM_UNLOCK_STAGE = 9,

        /// <summary>
        /// 鑽石購買時間 请求:{@code Request} 響應:{@code BuyTimeResponse}
        /// </summary>
        GEM_BUY_TIME = 11
    }

    public enum UnitCmd : byte
    {
        /// <summary>
        /// 獲取數據
        /// 請求:{@code }
        /// 響應:{@code UnitListResponse}
        /// </summary>
        GET_DATA = 1,

        /// <summary>
        /// 升级伙伴
        /// 請求:{@code UpgradeUnitRequest}
        /// 響應:{@code ValueResultListResponse}
        /// </summary>
        UPGRAD_UNIT = 2,


        /// <summary>
        /// 購買升級材料
        /// 請求:{@code BuyUpgradeRequest}
        /// 響應:{@code ValueResultListResponse}
        /// </summary>
        BUY_UPGRADE = 3,


        /// <summary>
        /// 一鍵升級夥伴
        /// 請求:{@code FastUpgradeUnitRequest}
        /// 響應:{@code FastUpgradeUnitResponse}
        /// </summary>
        FAST_UPGRAD_UNIT = 4
    }

    public enum GetChanceCmd : byte
    {
        /// <summary>
        /// 抽獎
        /// 請求:{@code }
        /// 響應:{@code }
        /// </summary>
        OPEN_BOX = 1,

        /// <summary>
        /// 購買鑰匙
        /// 請求:{@code }
        /// 響應:{@code ValueResultListResponse}
        /// </summary>
        BUY_KEYS = 2
    }

    public enum ActivityCmd : byte
    {
        /// <summary>
        /// 兌換碼
        /// 請求:{@code ExchangeCodeRequest}
        /// 響應:{@code ValueResultListResponse}
        /// </summary>
        EXCHANGE_CODE = 1
    }


    public enum SignCmd : byte
    {
        /// <summary>
        /// 獲取簽到信息
        /// </summary>
        INFO = 1,

        /// <summary>
        /// 簽到
        /// </summary>
        SIGN = 2
    }

    public enum TreeCmd : byte
    {

        /**
         * 獲取大樹信息
         *  請求:{@code Request} 
         * 	響應:{@code TreeInfoResponse}
         */
        GET_INFO = 1,

        /**
         * 進入活動關卡
         *  請求:{@code Request} 
         * 	響應:{@code TreeStageResponse}
         */
        ENTER = 2,

        /**
         * 提交戰鬥結果
         * 	請求:{@code SubmitTreeFightRequest} 
         * 	響應:{@code SubmitTreeFightResponse}
         */
        SUBMIT_FIGHT
    }


    public enum AchievementCmd : byte
    {
        /**
	     * 獲取成就列表 請求 {@code AchievementListRequest} 響應:{@code AchievementListResponse}
	     */
        GET_ACHIEVEMENT_LIST = 1,

        /**
	     * 領取成就獎勵 請求 {@code DataPacket} 響應:{@code ValueResultListResponse}
	     */
        GET_ACHIEVEMENT_REWARD = 2,

        /**
	     * 刷新每日任務成就 請求 {@code DataPacket} 響應:{@code AchievementListResponse}
	     */
        REFRESH_ACHIEVEMENT_LIST = 3,

        /**
         * 獲取已達成的成就數量
         */
        ROLL_POLLING_ACHIEVE = 4
    }


    public enum GoodsCmd : byte
    {

        /**
        * 獲取背包商品列表
        */
        GET_ALL_GOODS = 1,

        /**
        * 使用商品請求
        */
        USE_GOODS = 2,

        /**
        * 購買物品
        * 請求:{@code BuyGoodsRequest}
        * 響應:{@code BuyGoodsResponse}
        */
        BUY_GOODS = 3
    }

    public enum MailCmd : byte
    {
        /// <summary>
        /// 反饋
        /// 請求FeedBackRequest 響應：無
        /// </summary>
        FEEDBACK = 6
    }


    public enum PurchaseCmd : byte
    {
        /// <summary>
        /// 驗證IAP收據
        /// </summary>
        VALIDATE = 3
    }
}