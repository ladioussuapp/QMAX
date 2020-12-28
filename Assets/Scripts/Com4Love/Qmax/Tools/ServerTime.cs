using System;
using UnityEngine;

namespace Com4Love.Qmax.Tools
{

    /// <summary>
    /// 
    /// </summary>
    public class ServerTime
    {
        public static DateTime CreateDateTime(double unixTime)
        {
            return Utils.UnixTimeToDateTime(unixTime);
        }

        /// <summary>
        /// 最近一次更新服務器時間時，對應的Time.realtimeSinceStartup
        /// </summary>
        private float lastUpdateServTimeSinceStartup = 0f;

        /// <summary>
        /// 最近一次更新到的服務器時間
        /// </summary>
        private double lastServTime = 0;

        /// <summary>
        /// 最後一次調用TimeTick的時間
        /// </summary>
        double lastInvokeTimeTick = 0;

        /// <summary>
        /// 
        /// </summary>
        private double _unixTime = 0;
        /// <summary>
        /// 由服務器傳下的服務器時間，會隨著本地時間的增加而增加
        /// Unix時間戳，以秒為單位，UniversalTime
        /// </summary>
        public double UnixTime
        {
            get { return _unixTime; }
            set
            {
                lastServTime = value;
                lastUpdateServTimeSinceStartup = Time.realtimeSinceStartup;
                _unixTime = value;
                universalTime = Utils.UnixTimeToDateTime(_unixTime);
                if (lastInvokeTimeTick == 0)
                {
                    //第一次賦值,兩個時間保持一致
                    lastInvokeTimeTick = _unixTime;
                }
            }
        }

        /// <summary>
        /// 本地時間
        /// </summary>
        public DateTime LocalTime { get { return TimeZone.CurrentTimeZone.ToLocalTime(universalTime); } }

        public DateTime UniversalTime { get { return universalTime; } }



        DateTime universalTime;
        event Action<double> OnTimeTick;

        public ServerTime()
        {

        }

        /// <summary>
        /// 添加時間事件
        /// </summary>
        /// <param name="tickHandler"></param>
        public void AddTimeTick(Action<double> tickHandler)
        {
            RemoveTimeTick(tickHandler);    //防止重複添加
            OnTimeTick += tickHandler;
        }

        /// <summary>
        /// 刪除時間事件
        /// </summary>
        /// <param name="tickHandler"></param>
        public void RemoveTimeTick(Action<double> tickHandler)
        {
            OnTimeTick -= tickHandler;
        }


        public void OnUpdate()
        {
            if (_unixTime == 0)
            {
                //未收到服務器時間 沒有初始化
                return;
            }

            //距離上次服務器時間更新的時間
            float delta = Time.realtimeSinceStartup - lastUpdateServTimeSinceStartup;
            //計算得出最新的服務器時間
            _unixTime = lastServTime + delta;
            //上次調用OnTimeTick距離現在的時間
            double duration = _unixTime - lastInvokeTimeTick;
            
            if (duration < 1)
                return;

            universalTime = Utils.UnixTimeToDateTime(_unixTime);
            lastInvokeTimeTick = _unixTime;

            if (OnTimeTick != null)
            {
                OnTimeTick(duration);
            }
        }

    }
}
