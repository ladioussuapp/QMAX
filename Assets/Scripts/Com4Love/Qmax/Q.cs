using Com4Love.Qmax.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Com4Love.Qmax
{
    public class LogTime : IComparable
    {
        public long TimeStamp;
        public FileInfo LogFile;

        public int CompareTo(object obj)
        {
            int result;
            try
            {
                LogTime info = obj as LogTime;
                if (this.TimeStamp > info.TimeStamp)
                    result = -1;
                else
                    result = 1;
                return result;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

    }

    public class Q
    {
        /// <summary>
        /// LogTag白名單
        /// </summary>
        static public List<LogTag> LogTagWhiteList;

        /// <summary>
        /// LogTag黑名單
        /// </summary>
        static public List<LogTag> LogTagBlackList;

        private static string LOG_FILE_NAME;
        private static byte[] LOG_LOCK = new byte[0];

        static Q()
        {
            if (PackageConfig.LOCAL_LOG)
            {

#if !(UNITY_EDITOR) && QMAX_ACTIVITY
                UnityEngine.AndroidJavaClass jcls = new UnityEngine.AndroidJavaClass("com.loves.qmax.QMaxActivity");
                string qmaxRoot = jcls.CallStatic<string>("getQMaxFilesPath");
                UnityEngine.Debug.LogFormat("QMaxFilesPath={0}", qmaxRoot);
#else
                string qmaxRoot = "/sdcard/qmax";
#endif
                // logger file name
                LOG_FILE_NAME = qmaxRoot + "/" + DateTime.Now.ToString("yyyyMMdd.hhmmss") + ".txt";

                try
                {
                    DirectoryInfo dInfo = new DirectoryInfo(qmaxRoot);
                    if (!dInfo.Exists)
                    {
                        Directory.CreateDirectory(qmaxRoot);
                    }
                    else
                    {
                        FileInfo[] fileInfoArr = dInfo.GetFiles("*.txt");
                        // Log文件數量超過20
                        if (fileInfoArr != null && fileInfoArr.Length > 20)
                        {
                            // 排序列表
                            List<LogTime> sortList = new List<LogTime>();

                            for (int i = 0; i < fileInfoArr.Length; i++)
                            {
                                FileInfo fInfo = fileInfoArr[i];
                                string filaName = fInfo.Name;
                                filaName = filaName.Replace(".txt", "");

                                if (filaName.IndexOf(".") > 0)
                                {
                                    string[] sl = filaName.Split(new Char[] { '.' });
                                    string timeStr = String.Format("{0:####/##/##} {1:##:##:##}", Int32.Parse(sl[0]), Int32.Parse(sl[1]));
                                    DateTime dt = Convert.ToDateTime(timeStr);
                                    TimeSpan span = dt - new DateTime(1970, 1, 1);

                                    LogTime lt = new LogTime();
                                    lt.TimeStamp = (long)span.TotalMilliseconds;
                                    lt.LogFile = fInfo;
                                    sortList.Add(lt);
                                }
                            }
                            // 按時間排序
                            sortList.Sort();
                            // 刪除超出部分
                            while (sortList.Count > 20)
                            {
                                LogTime lt = sortList[sortList.Count - 1];
                                sortList.Remove(lt);
                                lt.LogFile.Delete();
                                UnityEngine.Debug.Log("delete log file:" + lt.LogFile.FullName);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Q.Warning("Q 寫文件出錯. errMsg={0}", e.Message);
                }
            }
        }


        /// <summary>
        /// 斷言
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="msgFormat"></param>
        /// <param name="args"></param>
        static public bool Assert(bool condition, string msgFormat = "", params object[] args)
        {
            if (condition)
                return true;

            //不是在編輯器狀態，需要上報Bugly
#if UNITY_EDITOR
            Log(LogTag.Assert, msgFormat, args);
#else
            //沒有接入Bugly時，這裡會報錯
            try
            {
                BuglyAgent.PrintLog(LogSeverity.LogAssert, "[{0}]{1}", "Assert", string.Format(msgFormat, args));
            }
            catch (Exception)
            {
                Log(LogTag.Assert, msgFormat, args);
            }
#endif

            LogFile(string.Format("[{0}]{1}", "Assert", string.Format(msgFormat, args)));
            return condition;
        }

        static public void Log(string format, params object[] args)
        {
            Log(LogTag.Normal, String.Format(format, args));
        }

        static public void Log(string msg)
        {
            Log(LogTag.Normal, msg);
        }

        static public void Log(object msg)
        {
            Log(LogTag.Normal, msg.ToString());
        }


        /// <summary>
        /// 帶Tag的輸出
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        static public void Log(LogTag tag, string format, params object[] args)
        {
            //先檢查白名單，再檢查黑名單
            bool flg = true;
            if (LogTagWhiteList != null)
                flg = LogTagWhiteList.Contains(tag);
            else if (LogTagBlackList != null)
                flg = !LogTagBlackList.Contains(tag);

            if (flg)
            {
                // BuglyAgent.DebugLog(tag.ToString(), format, args);
                //調用Bugly接口，自動就會打印到控制台
                if (tag == LogTag.Assert)
                    UnityEngine.Debug.LogWarningFormat("[{0}]{1}", tag, String.Format(format, args));
                else if (tag == LogTag.Error)
                    UnityEngine.Debug.LogErrorFormat("[{0}]{1}", tag, String.Format(format, args));
                else
                    UnityEngine.Debug.LogFormat("[{0}]{1}", tag, String.Format(format, args));

                LogFile("Log:" + String.Format("[{0}]{1}", tag, String.Format(format, args)));
            }
        }


        static public void Log(LogTag tag, string msg)
        {
            //先檢查白名單，再檢查黑名單
            bool flg = true;
            if (LogTagWhiteList != null)
                flg = LogTagWhiteList.Contains(tag);
            else if (LogTagBlackList != null)
                flg = !LogTagBlackList.Contains(tag);


            if (flg)
            {
                // BuglyAgent.DebugLog(tag.ToString(), msg);
                //調用Bugly接口，自動就會打印到控制台
                if (tag == LogTag.Assert)
                    UnityEngine.Debug.LogWarningFormat("[{0}]{1}", tag, msg);
                else if (tag == LogTag.Error)
                    UnityEngine.Debug.LogErrorFormat("[{0}]{1}", tag, msg);
                else
                    UnityEngine.Debug.LogFormat("[{0}]{1}", tag, msg);

                LogFile(String.Format("[{0}]{1}", tag, msg));
            }
        }



        static public void Error(Exception e)
        {
            string msg = string.Format("[Error]:\n{0}", e.ToString());
            UnityEngine.Debug.LogWarningFormat(msg);
            LogFile("Error:" + msg);
        }

        static public void Warning(string source)
        {
            UnityEngine.Debug.LogWarning(source);
            LogFile("Warning:" + source);
        }

        static public void Warning(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarning(String.Format(format, args));
            LogFile("Warning:" + String.Format(format, args));
        }


        public static void LogFile(string str)
        {
            if (PackageConfig.LOCAL_LOG)
            {
                lock (LOG_LOCK)
                {
                    try
                    {
                        StreamWriter stream = new StreamWriter(LOG_FILE_NAME, true, Encoding.GetEncoding("UTF-8"));
                        if (stream != null)
                        {
                            string info = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + ": " + str;
                            stream.WriteLine(info);
                            stream.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError(ex.Message + "\n" + ex.StackTrace);
                    }
                }
            }
        }

        static public void Dump(object value, int nesting = 3)
        {
            //TODO

            if (nesting == 1)
            {
                UnityEngine.Debug.Log(value.ToString());
                return;
            }

            if (value is IList)
            {
                IList l = value as IList;
                for (int i = 0, n = l.Count; i < n; i++)
                {
                    Dump(value, nesting - 1);
                }
            }
            else
            {
                UnityEngine.Debug.Log(value.ToString());
            }
        }


        static private Dictionary<int, Stopwatch> StopwatchDict;

        static public int StartNewStopwatch()
        {
            if (StopwatchDict == null)
                StopwatchDict = new Dictionary<int, Stopwatch>();
            ///unity的方法不可以在非主線程調用//
            //int id = UnityEngine.Random.Range(1, Int32.MaxValue);
            System.Random rnm = new System.Random();
            int id = rnm.Next();
            while (StopwatchDict.ContainsKey(id))
            {
                //id = UnityEngine.Random.Range(1, Int32.MaxValue);
                id = rnm.Next();
            }
            Stopwatch sw = new Stopwatch();
            StopwatchDict.Add(id, sw);
            sw.Start();
            return id;
        }
        static public void ResetStopwatch(int id)
        {
            if (StopwatchDict == null || !StopwatchDict.ContainsKey(id))
                return;
            StopwatchDict[id].Reset();
        }


        static public void LogElapsedSecAndReset(int id, string msg)
        {
            Q.Log(LogTag.TestPerf, msg + " {0}s", Q.ElapsedSecAndReset(id));
        }

        static public void LogElapsedSeconds(int id, string msg)
        {
            Q.Log(LogTag.TestPerf, msg + " {0}s", Q.ElapsedSeconds(id));
        }


        static public double ElapsedSecAndReset(int id)
        {
            if (StopwatchDict == null || !StopwatchDict.ContainsKey(id))
                return 0;

            double ret = StopwatchDict[id].ElapsedMilliseconds * 0.001;
            StopwatchDict[id].Reset();
            StopwatchDict[id].Start();
            return ret;
        }

        static public double ElapsedSeconds(int id)
        {
            if (StopwatchDict == null || !StopwatchDict.ContainsKey(id))
                return 0;

            return StopwatchDict[id].ElapsedMilliseconds * 0.001;
        }

        static public void ClearStopwatch(int id)
        {
            if (StopwatchDict == null || !StopwatchDict.ContainsKey(id))
                return;

            StopwatchDict.Remove(id);
        }

        public Q() { }
    }
}