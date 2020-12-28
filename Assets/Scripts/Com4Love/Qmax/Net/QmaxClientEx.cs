using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.Timers;

namespace Com4Love.Qmax.Net
{
    public class QmaxClientEx : QmaxClient
    {
        /// <summary>
        /// 心跳间隔
        /// </summary>
        public const double HEART_BEAT_INTERVAL = 30000;


        /// <summary>
        /// 目前：
        /// 当某种需要Lock的消息*第一次*发出时，都会调用ResponseLockEvent(true)
        /// 当某种需要Lock的消息*所有*回包都收到后，都会调用ResponseLockEvent(false)
        /// 
        /// TODO 改成：
        /// 当*第一条*需要Lock的消息发出时，会调用ResponseLockEvent(true);
        /// 当*所有*需要Lock的消息收到回包时，会调用ResponseLockEvent(false);
        /// </summary>
        public event Action<bool> ResponseLockEvent;

        /// <summary>
        /// 是否开始发心跳包
        /// </summary>
        public virtual bool HeartBeatEnabled
        {
            get { return heartBeatTimer != null && heartBeatTimer.Enabled; }

            set
            {
                if (heartBeatTimer != null)
                    heartBeatTimer.Enabled = value;
            }
        }

        protected Timer heartBeatTimer;

        /// <summary>
        /// 重连ID
        /// </summary>
        public string reConnectId;

        /// <summary>
        /// 发出但未收到回包的协议队列
        /// </summary>
        private Queue<MsgStruct> sendNotReceQue;

        private bool isReconnect = false;


        public QmaxClientEx()
            : base()
        {

            sendNotReceQue = new Queue<MsgStruct>();
            heartBeatTimer = new Timer(HEART_BEAT_INTERVAL);
            heartBeatTimer.Elapsed += OnHeartBeatTimer;
            heartBeatTimer.Enabled = false;
        }

        public override void Close()
        {
            if (heartBeatTimer != null)
                heartBeatTimer.Stop();
            base.Close();
        }

        public override void Dispose()
        {
            heartBeatTimer.Stop();
            ResponseLockEvent = null;
            base.Dispose();
        }


        public override void SendMsg(byte moduleID, byte cmd, IProtocol value, bool crypt = false)
        {
            ///保存必须等回包的消息
            int lockKey = moduleID * 1000 + cmd;
            if (responseLock.ContainsKey(lockKey))
            {
                MsgStruct item = new MsgStruct();
                item.module = moduleID;
                item.cmd = cmd;
                item.protocol = value;
                item.time = Utils.LocalTimeToUnixTime(DateTime.Now);
                item.crypt = crypt;
                sendNotReceQue.Enqueue(item);

                //需要锁定，直到对应的回包
                //如果计数器小于0，说明曾经在没有请求的情况下被推送过一条同样的消息
                if (responseLock[lockKey] == 0)
                {
                    //UnityEngine.Debug.Log("消息锁定 key:" + lockKey);
                    //发送加锁事件
                    if (ResponseLockEvent != null)
                    {
                        Q.Log("RealSendMsg() [{0}, {1}] lock=true", moduleID, cmd);
                        ResponseLockEvent(true);
                    }
                }
                responseLock[lockKey]++;
            }

            base.SendMsg(moduleID, cmd, value, crypt);
        }


        protected override void ReceiveMsg(byte moduleId, byte cmd, object value, short status)
        {
            ///回到消息后移除保存的必须回包的消息
            ///modify by yangbangqing   2015-10-23
            #region   重连发送没回包的消息

            int lockKey = moduleId * 1000 + cmd;
            if (responseLock.ContainsKey(lockKey))
            {
                sendNotReceQue.Dequeue();
            }

            if (moduleId == (byte)Module.User && cmd == (byte)UserCmd.USER_RECONNECTION)
            {
                isReconnect = true;
            }

            if (moduleId == (byte)Module.User && cmd == (byte)UserCmd.ACTOR_LOGIN && isReconnect)
            {
                isReconnect = false;
                ///如果是重新连接的，把没回到回包的消息再发一遍
                while (sendNotReceQue.Count > 0)
                {
                    MsgStruct msgItem = sendNotReceQue.Dequeue();
                    double now = Utils.LocalTimeToUnixTime(DateTime.Now);
                    if (now - msgItem.time < 600)
                    {
                        SendMsg(msgItem.module, msgItem.cmd, msgItem.protocol, msgItem.crypt);
                    }
                }
            }
            #endregion

            base.ReceiveMsg(moduleId, cmd, value, status);

            //有可能注册只有回包的消息
            if (responseLock.ContainsKey(lockKey))
            {
                responseLock[lockKey]--;

                //Q.Assert(responseLock[lockKey] >= 0, "解锁回包次数大于请求次数  moduleId:" + moduleId + " cmd:" + cmd + " 请求次数：" + responseLock[lockKey]);

                //解锁的包
                if (responseLock[lockKey] == 0)
                {
                    //UnityEngine.Debug.Log("消息解锁 key:" + lockKey);
                    //发送解锁事件
                    if (ResponseLockEvent != null)
                    {
                        Q.Log("ReceiveMsg() [{0}, {1}] lock=false", moduleId, cmd);
                        ResponseLockEvent(false);
                    }
                }
            }
        }



        private void OnHeartBeatTimer(object source, ElapsedEventArgs args)
        {
            if (Connected)
                HeartBeat();
        }
    }
}
