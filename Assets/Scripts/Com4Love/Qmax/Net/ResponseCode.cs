
namespace Com4Love.Qmax.Net
{
    /// <summary>
    /// 狀態碼
    /// </summary>
    public enum ResponseCode : int
    {
        SUCCESS = 0,

        /// <summary>
        /// HttpStatusCode != 200
        /// </summary>
        HTTP_FAIL = -1,

        /// <summary>
        /// 發送超時
        /// </summary>
        TIME_OUT = -1000,

        /// <summary>
        /// 請求公告失敗
        /// </summary>
        REQ_NOTICE_FAIL = -3,

        /// <summary>
        /// 連接服務器失敗狀態碼
        /// </summary>
        CONNECT_FAIL = -10,

        /// <summary>
        /// 連接遊戲服務器失敗
        /// </summary>
        CONNECT_GAME_SERVER_FAIL = -11,

        /// <summary>
        /// 連接登錄服失敗
        /// </summary>
        CONNECT_LOGIN_SERVER_FAIL = -12,

        /// <summary>
        /// 服務器未登錄
        /// </summary>
        SERVER_UNLOGIN = 5,

        /// <summary>
        /// 服務器停機維護中
        /// </summary>
        SERVER_DOWN = 7,

        /// <summary>
        /// 服務器內部錯誤
        /// </summary>
        SERVER_INNER_ERROR = 15,

        #region 登录/注册返回码
        /// <summary>
        /// 服務器錯誤
        /// </summary>
        SERVER_ERROR = 10000,

        /// <summary>
        /// 用戶名或密碼錯誤
        /// </summary>
        USERNAME_OR_PWD_ERROR = 10001,

        /// <summary>
        /// 註冊失敗，用戶已存在
        /// </summary>
        USERNAME_EXSIT = 10002,

        /// <summary>
        /// 用戶名不能為空
        /// </summary>
        USERNAME_NOT_NULL = 10003,

        /// <summary>
        /// 密碼不能為空
        /// </summary>
        PWD_NOT_NULL = 10004,

        /// <summary>
        /// Token驗證錯誤
        /// </summary>
        TOKEN_VOLIDATE_ERROR = 10005,
        #endregion
    }
}
