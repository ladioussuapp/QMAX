using Com4Love.Qmax.Net.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Com4Love.Qmax.Net
{
    public struct MsgStruct
    {
        public byte module;
        public byte cmd;
        public IProtocol protocol;
        public double time;
        public bool crypt;
    }

    /// <summary>
    /// Qmax網絡層核心，用於連接服務端、發送消息、解析消息
    /// （考慮此類只保留最基本的發送與接收的功能）
    /// </summary>
    /// 這個類與業務邏輯無關
    public class BaseClient : IDisposable
    {
        /// <summary>
        /// 每次接受回包的長度
        /// </summary>
        public const int BufferSize = 8192;

        public const short CONNECT_FAIL_CODE = -10;

#if DelayTest
        /// <summary>
        /// 测试使用
        /// </summary>
        public static int TestLogID;
        public static byte TestModule;
        public static byte TestCmd;
        public static double TestDelayTime;

#endif

        /// <summary>
        /// 計算哈希值
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public int CalcHashCode(byte[] data)
        {
            int seed = 16777619;
            int hash = -2128831035;
            int j = data.Length;
            for (int i = 0; i < j; ++i)
            {
                sbyte b = (sbyte)data[i];
                hash = (hash ^ b) * seed;
            }

            hash += hash << 7;
            hash ^= hash >> 6;
            hash += hash << 3;
            hash ^= hash >> 8;
            hash += hash << 1;
            return hash;
        }

        /// <summary>
        /// 回包回調委託
        /// </summary>
        /// <param name="module"></param>
        /// <param name="cmd"></param>
        /// <param name="status"></param>
        /// <param name="value"></param>
        public delegate void OnResponse(byte module, byte cmd, short status, object value);

        /// <summary>
        /// 斷開連接事件.
        /// 無論主動調用Close()方法，還是被動斷開，都會調用這個事件。
        /// 注意：這個事件可能並非在主線程裡調用，調用Unity的API注意要在主線程中調用
        /// </summary>
        public event Action DisConnectEvent;

        /// <summary>
        /// 連接是否成功
        /// 注意：這個事件可能並非在主線程裡調用，調用Unity的API注意要在主線程中調用
        /// </summary>
        public event Action<bool> ConnectResultEvnet;

        /// <summary>
        /// 所有出了錯的回包都會另外在這裡被回調。正式發佈時應該去掉此邏輯。 (考慮移到子類實現)
        /// 注意：這個事件可能並非在主線程裡調用，調用Unity的API注意要在主線程中調用
        /// </summary>
        public event OnResponse ErrorResponseEvent;

        /// <summary>
        /// 是否連接中
        /// </summary>
        public virtual bool Connected { get { return tcp != null && tcp.Connected; } }

        /// <summary>
        /// AES加密的key
        /// </summary>
        public byte[] CryptKey;

        /// <summary>
        /// 發送的協議是否使用小端
        /// </summary>
        public bool IsLittleEndian = false;


        protected TcpClient tcp;
        protected byte[] buffer;
        protected int bufferNail;
        protected NetworkStream stream;

        /// <summary>
        /// 使用module、cmd映射到回包Type
        /// </summary>
        protected Dictionary<byte, Dictionary<byte, Type>> responseDict;

        /// <summary>
        /// 回包回調的字典，回調函數會統一在OnUpdate裡調用
        /// </summary>
        protected Dictionary<Module, OnResponse> responseCallbackDict;

        protected struct Response
        {
            public object Value;
            public byte Module;
            public byte Cmd;
            public short Status;
        }

        public BaseClient()
        {
            buffer = new byte[BufferSize];
            bufferNail = 0;
            responseDict = new Dictionary<byte, Dictionary<byte, Type>>();
        }

        public virtual void Dispose()
        {
            DisConnectEvent = null;
            ConnectResultEvnet = null;
        }


        /// <summary>
        /// 註冊回包解析協議
        /// </summary>
        /// <param name="module"></param>
        /// <param name="cmd"></param>
        /// <param name="type"></param>
        public virtual void RegisterResponseType(byte module, byte cmd, Type type)
        {
            //Q.Log(LogTag.Net, "RegisterResponseType, module={0}, cmd={1}", module, cmd);

            if (!responseDict.ContainsKey(module))
                responseDict.Add(module, new Dictionary<byte, Type>());
            responseDict[module].Add(cmd, type);
        }

        /// <summary>
        /// 添加回包監聽
        /// 注意：調用callback時可能並非在主線程裡調用，調用Unity的API注意要在主線程中調用
        /// </summary>
        /// <param name="module">需要監聽的模塊名</param>
        /// <param name="callback"></param>
        public virtual void AddResponseCallback(Module module, OnResponse callback)
        {
            if (responseCallbackDict == null)
                responseCallbackDict = new Dictionary<Module, OnResponse>();

            if (!responseCallbackDict.ContainsKey(module))
                responseCallbackDict.Add(module, new OnResponse(callback));
            else
                responseCallbackDict[module] += callback;
        }


        /// <summary>
        /// 刪除回包監聽
        /// </summary>
        /// <param name="callback"></param>
        public void RemoveResponseCallback(Module module, OnResponse callback)
        {
            if (responseCallbackDict == null)
                responseCallbackDict = new Dictionary<Module, OnResponse>();

            if (responseCallbackDict[module] != null)
                responseCallbackDict[module] -= callback;
        }


        /// <summary>
        /// 開始異步連接。連接結果會調用ConnectResultEvent.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public virtual void Connect(string host, int port)
        {
            Q.Log(LogTag.Net, "BaseClient:Connect: host={0}, port={1}", host, port);
            //連接的回調
            Action<IAsyncResult> OnConnectResult = delegate(IAsyncResult result)
            {
                try
                {
                    tcp.EndConnect(result);
                }
                catch (InvalidOperationException e)
                {
                    //The EndConnect method was previously called for the asynchronous connection.
                    Q.Log(LogTag.Net, "連接遊服失敗. e={0}", e.Message);

                    if (ConnectResultEvnet != null)
                    {
                        ConnectResultEvnet(false);
                    }
                    return;
                }
                catch (SocketException e)
                {
                    //An error occurred when attempting to access the Socket. 
                    //See the Remarks section for more information.
                    Q.Log(LogTag.Net, "連接遊服失敗. e={0}", e.Message);

                    if (ConnectResultEvnet != null)
                    {
                        ConnectResultEvnet(false);
                    }
                    return;
                }

                stream = tcp.GetStream();
                AsyncCallback callback = new AsyncCallback(OnReadComplete);
                stream.BeginRead(buffer, bufferNail, BufferSize - bufferNail, callback, tcp);

                if (ConnectResultEvnet != null)
                {
                    ConnectResultEvnet(true);
                }
            };//OnConnectResult

            Q.Assert(tcp == null || !tcp.Connected);
            tcp = new TcpClient();
            try
            {
                AsyncCallback connectCallback = new AsyncCallback(OnConnectResult);
                tcp.BeginConnect(host, port, connectCallback, tcp);
            }
            catch (Exception e)
            {
                Q.Log(LogTag.Net, "Connect fail at {0}:{1}, {2}", host, port, e);

                if (ConnectResultEvnet != null)
                {
                    ConnectResultEvnet(false);
                }
            }
        }



        /// <summary>
        /// 發送一條消息
        /// </summary>
        /// <param name="moduleID"></param>
        /// <param name="cmd"></param>
        /// <param name="value"></param>
        /// <param name="crypt"></param>
        /// <returns></returns>
        public virtual void SendMsg(byte moduleID, byte cmd, IProtocol value = null, bool crypt = false)
        {
            //Q.Log(LogTag.Net, "BaseClient:SendMsg m={0}, c={1}", moduleID, cmd);
            if (tcp == null || !tcp.Connected)
            {
                Q.Warning("BaseClient:SendMsg Fail. module={0},cmd={1}", moduleID, cmd);
                //發送失敗，模擬一個Response
                ReceiveMsg(moduleID, cmd, null, CONNECT_FAIL_CODE);
                return;
            }
#if DelayTest
            BaseClient.TestLogID = Q.StartNewStopwatch();
            BaseClient.TestModule = moduleID;
            BaseClient.TestCmd = cmd;
            ///-1為更新了請求
            BaseClient.TestDelayTime = -1;
#endif

            //Q.Log(LogTag.Net, "SendMsg: module={0}, cmd={1}", moduleID, cmd);
            MemoryStream mStream = new MemoryStream();
            int packageLen = 0;
            byte[] valueBytes = value != null ? valueBytes = value.Serialize(IsLittleEndian) : new byte[0];
            //先計算hashCode再加密
            int hashCode = CalcHashCode(valueBytes);
            if (crypt)
                valueBytes = Utils.Encrypt(valueBytes, CryptKey);

            //packageLen = (moduleID + cmd + hashCode + crypt+value).Length
            packageLen = 7 + valueBytes.Length;
            //Q.Log (LogTag.Net, "valueBytes.Length={0}", 0);

            int offset = 0;
            //headerFlag
            Serializer.ToBytes(mStream, (int)-1860108940, IsLittleEndian);
            offset += 4;
            //packageLen
            Serializer.ToBytes(mStream, packageLen, IsLittleEndian);
            offset += 4;
            //module id			
            mStream.WriteByte(moduleID);
            offset += 1;
            //command id            
            mStream.WriteByte(cmd);
            offset += 1;
            //hashCode
            Serializer.ToBytes(mStream, hashCode, IsLittleEndian);
            offset += 4;
            //crypt
            if (crypt)
                mStream.WriteByte(1);
            else
                mStream.WriteByte(0);
            offset += 1;
            //Q.Log (LogTag.Net, "hashCode={0}", calcHashCode(valueBytes));
            if (valueBytes.Length > 0)
            {
                //value
                mStream.Write(valueBytes, 0, valueBytes.Length);
                offset += valueBytes.Length;
            }
            Q.Assert(offset == packageLen + 8);

            stream = tcp.GetStream();
            stream.Write(mStream.ToArray(), 0, packageLen + 8);
            mStream.Close();
        }

        private void OnReadComplete(IAsyncResult ar)
        {
            Q.Log(LogTag.Net, "BaseClient:OnReadComplete 0");
            int bytesReaded;
            try
            {
                lock (stream)
                {
                    bytesReaded = stream.EndRead(ar);
                }

                //此時表示已經被斷開
                if (bytesReaded == 0)
                    throw new Exception("讀取到0字節");

                int offset = 0;
                const int headLen = 8;
                bufferNail = bufferNail + bytesReaded;
                while (offset + headLen < bufferNail)
                {
                    int headerFlag = Serializer.ReadInt32(buffer, offset, IsLittleEndian);
                    offset += 4;
                    Q.Assert(headerFlag == -1860108940, "Packager header error.");
                    int packageLen = Serializer.ReadInt32(buffer, offset, IsLittleEndian);
                    offset += 4;

                    if (offset + packageLen <= bufferNail)
                    {
                        //剩下內容足以讀取一個包
                        ReadPackage(offset);
                        offset += packageLen;
                    }
                    else
                    {
                        //Q.Log("====== 剩下內容不足以讀取一個包: headLen{0}, offset={1}, packageLen={2}, bufferNail={3}",
                        //    headLen,
                        //    offset,
                        //    packageLen,
                        //    bufferNail);
                        //剩下內容不足以讀取一個包
                        offset -= headLen;
                        break;
                    }
                }

                if (offset < bufferNail)
                {
                    byte[] newBuffer = new byte[BufferSize];
                    Array.Copy(buffer, offset, newBuffer, 0, bufferNail - offset);
                    buffer = newBuffer;
                    bufferNail = bufferNail - offset;
                }
                else
                {
                    bufferNail = 0;
                }

                //Q.Log(LogTag.Net, "BaseClient:OnReadComplete 1 stream={0}", stream);
                lock (stream)
                {
                    AsyncCallback callback = new AsyncCallback(OnReadComplete);
                    stream.BeginRead(buffer, bufferNail, BufferSize - bufferNail, callback, tcp);
                }
            }
            catch (ObjectDisposedException e)
            {
                //The NetworkStream is closed.
                Q.Log(LogTag.Net, "NetworkStream被主動關閉. errMsg={0}", e.Message);
                //這裡不會調用DisConnectEvent()
                //因為是被主動關閉的，所以會在Close()函數里調用DisConnectEvent()
            }
            catch (IOException e)
            {
                //The underlying Socket is closed.
                //-or-
                //There was a failure while reading from the network.
                //-or-
                //An error occurred when accessing the socket. See the Remarks section for more information.

                Q.Log(LogTag.Net, "NetworkStream被動關閉. errMsg={0}", e.Message);
                Close();
                //每次發消息是發現連接已斷開，回到登錄界面
                Response r = new Response();
                r.Module = (byte)Module.Http;
                r.Cmd = (byte)HttpCmd.DIS_CONNECT;
                r.Status = 0;
                r.Value = null;
                InvokeRespCallback(r);

                if (DisConnectEvent != null)
                    DisConnectEvent();
            }
        }

        private void ReadPackage(int headOffset)
        {
            byte moduleID = buffer[headOffset++];
            byte cmd = buffer[headOffset++];
            short status = Serializer.ReadInt16(buffer, headOffset, IsLittleEndian);
            Q.Log(LogTag.Net, "BaseClient:ReadPackage: [{0}, {1}], status={2}", moduleID, cmd, status);
            headOffset += 2;
            object value = null;

            if (status == 0 &&
               responseDict.ContainsKey(moduleID) &&
               responseDict[moduleID].ContainsKey(cmd))
            {
                Type type = responseDict[moduleID][cmd];
                if (type.GetInterface("IProtocol") != null)
                {
                    value = Activator.CreateInstance(type);
                    IProtocol p = (IProtocol)value;

                    p.Deserialize(buffer, headOffset, IsLittleEndian);
                }
                else if (type == typeof(int))
                    value = Serializer.ReadInt32(buffer, headOffset, IsLittleEndian);
            }
            ReceiveMsg(moduleID, cmd, value, status);
        }

        /// <summary>
        /// 接收到一條消息
        /// </summary>
        /// <param name="moduleId"></param>
        /// <param name="cmd"></param>
        /// <param name="value"></param>
        /// <param name="status"></param>
        protected virtual void ReceiveMsg(byte moduleId, byte cmd, object value, short status)
        {
            Response r = new Response();
            r.Module = moduleId;
            r.Cmd = cmd;
            r.Value = value;
            r.Status = status;
            InvokeRespCallback(r);
        }


        private void InvokeRespCallback(Response r)
        {
            if (r.Status != 0 && ErrorResponseEvent != null)
            {
                ErrorResponseEvent(r.Module, r.Cmd, r.Status, r.Value);
            }

            //Q.Log(LogTag.Net, "Excuting Reponse: Module={0}, Cmd={1}", r.Module, r.Cmd);
            if (responseCallbackDict.ContainsKey((Module)r.Module))
                responseCallbackDict[(Module)r.Module](r.Module, r.Cmd, r.Status, r.Value);

#if DelayTest

            ///輸出最後一次請求協議的耗時//
            if (r.Module == BaseClient.TestModule
                && r.Cmd == BaseClient.TestCmd)
            {
                BaseClient.TestDelayTime = Q.ElapsedSecAndReset(BaseClient.TestLogID);

                if (BaseClient.TestDelayTime > 0.1)
                    UnityEngine.Debug.Log(string.Format("module:{0} cmd:{1} delaytime:{2}", r.Module, r.Cmd, BaseClient.TestDelayTime));
            }
#endif
        }


        /// <summary>
        /// 主動關閉連接
        /// 關閉之後，會觸發讀流失敗，在OnReadComplete()函數里發出DisconnectEvent
        /// </summary>
        public virtual void Close()
        {
            bool flg = false;
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
                flg = true;
            }

            if (tcp != null)
            {
                tcp.Close();
                tcp = null;
                if (DisConnectEvent != null)
                    DisConnectEvent();
                flg = true;
            }
            if (flg)
                Q.Log(LogTag.Net, "BaseClient:Close");
        }
    }//class
}//namespace

