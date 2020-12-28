using Com4Love.Qmax.Data;
using Com4Love.Qmax.Net;
using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace System
{
    public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

}

namespace Com4Love.Qmax
{
    public class Utils
    {
        /// <summary>
        /// 打印程序當前狀態
        /// </summary>
        public static void LogStatus()
        {
            Q.Log("GC.GetTotalMemory={0}MB\nGetMonoUsedSize={1}MB\nGetMonoHeapSize={2}MB",
                System.GC.GetTotalMemory(false) / 1048576f,
                UnityEngine.Profiling.Profiler.GetMonoUsedSize() / 1048576f,
                UnityEngine.Profiling.Profiler.GetMonoHeapSize() / 1048576f);
        }


        /// <summary>
        /// 根據中文為key，獲取對應的語言版本的文字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetText(string value)
        {
#if UNITY_EDITOR
            if (GameController.Instance.Model.LanguageConfigs == null)
            {
                return value;
            }
#endif


            if (GameController.Instance.Model.LanguageConfigs.ContainsKey(value))
            {
                return FormatText(GameController.Instance.Model.LanguageConfigs[value]);
            }
            else
            {
                Q.Warning("語言表缺少項：" + value);
                return value;
            }
        }

        static string FormatText(string val)
        {
            val = FormatChsText(val);
            val = FormatNewLineText(val);
            val = DecodeHtmlStringInXml(val);

            return val;
        }

        static Regex chsRegex = new Regex(@"^#\d+#");

        public static string FormatChsText(string val)
        {
            if (PackageConfig.LANGUAGE == "chs"|| PackageConfig.LANGUAGE=="cht" && val[0] == '#')
            {
                return chsRegex.Replace(val, "");
            }

            return val;
        }

        static Regex newLineRegex = new Regex(@"\\r\\n");

        /// <summary>
        /// 配置表裡面的\r\n換行字符解析
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string FormatNewLineText(string val)
        {
            return newLineRegex.Replace(val, "\r\n");
        }

        /// <summary>
        /// 以中文文本為key，獲取當前語言版本的文字
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetText(string format, params object[] args)
        {
            if (GameController.Instance.Model.LanguageConfigs.ContainsKey(format))
            {
                try
                {
                    return FormatText(String.Format(GameController.Instance.Model.LanguageConfigs[format], args));
                }
                catch (Exception e)
                {
                    Q.Warning("語言獲取格式化錯誤,format:" + format + e.ToString());
                    return "語言獲取格式化錯誤,format:" + format;
                }
            }
            else
            {
                Q.Warning("語言表缺少項：" + format);
                return format;
            }
        }

        /// <summary>
        /// 根據StatusCode返回語句
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static string GetTextByStatusCode(ResponseCode statusCode)
        {
            int iCode = (int)statusCode;
            if (GameController.Instance.Model.LanguageConfigsByStatusCode.ContainsKey(iCode))
            {
                return GameController.Instance.Model.LanguageConfigsByStatusCode[iCode];
            }
            else
            {
                Q.Warning("語言表缺少statusCode：" + iCode);
                return "";
            }
        }


        /// <summary>
        /// 根據id獲取當前語言版本的文字
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetTextByID(int id)
        {
            if (GameController.Instance.Model.LanguageConfigsById.ContainsKey(id))
            {
                return FormatText(GameController.Instance.Model.LanguageConfigsById[id]);
            }
            else
            {
                Q.Warning("語言表缺少id：" + id);
                return id.ToString();
            }
        }

        public static string GetTextByID(int id, params object[] args)
        {
            if (GameController.Instance.Model.LanguageConfigsById.ContainsKey(id))
            {
                try
                {
                    return FormatText(String.Format(GameController.Instance.Model.LanguageConfigsById[id], args));
                }
                catch (Exception e)
                {
                    Q.Warning("語言獲取格式化錯誤,id:" + id + e.ToString());
                    return "語言獲取格式化錯誤,id:" + id;
                }
            }
            else
            {
                Q.Warning("語言表缺少id:" + id);
                return id.ToString();
            }
        }

        /// <summary>
        /// 這是一種配置表專用的格式
        /// stringID的格式有可能為 ID,參數1，參數2
        /// </summary>
        /// <returns></returns>
        public static string GetTextByStringID(string stringId)
        {
            string[] idAndParams = stringId.Split(',');
            int id = 0;

            if (idAndParams.Length > 1)
            {
                int.TryParse(idAndParams[0], out id);
                Q.Assert(id != 0, "StringId格式錯誤：" + stringId);
                string[] params_ = new string[idAndParams.Length - 1];
                Array.Copy(idAndParams, 1, params_, 0, params_.Length);

                return GetTextByID(id, params_);
            }
            else
            {
                int.TryParse(stringId, out id);
                Q.Assert(id != 0, "StringId格式錯誤：" + stringId);

                return GetTextByID(id);
            }
        }

        /// <summary>
        /// 延遲執行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delaySeconds"></param>
        /// <returns></returns>
        public static IEnumerator DelayToInvokeDo(Action action, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            action();
        }


        /// <summary>
        /// 延遲一幀調用GameObject.SetActive(false)
        /// </summary>
        /// <remarks>
        /// Unity 5.0.0版本中的一個bug，詳見
        /// http://forum.unity3d.com/threads/statemachinebehaviour-onstateexit-crashes-unity.314737/
        /// http://cikusa.lofter.com/post/1cba884a_33f2d07
        /// </remarks>
        /// <param name="obj"></param>
        public static IEnumerator DelayDeactive(GameObject obj)
        {
            yield return null;
            obj.SetActive(false);
        }

        /// <summary>
        /// 延遲一幀調用func
        /// </summary>
        /// <remarks>
        /// Unity 5.0.0版本中的一個bug，詳見
        /// http://forum.unity3d.com/threads/statemachinebehaviour-onstateexit-crashes-unity.314737/
        /// http://cikusa.lofter.com/post/1cba884a_33f2d07
        /// </remarks>
        /// <param name="obj"></param>
        public static IEnumerator DelayNextFrameCall(Action func)
        {
            yield return null;
            func();
        }


        /// <summary>
        /// 把DateTime轉化為Unix時間戳（以秒為單位）
        /// </summary>
        /// <param name="universeTime"></param>
        /// <returns></returns>
        static public double DateTimeToUnixTime(DateTime universeTime)
        {
            DateTime startTime = new DateTime(1970, 1, 1);
            return (universeTime - startTime).TotalSeconds;
        }

        /// <summary>
        /// 把Unix時間戳（以秒為單位）轉為DateTime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        static public DateTime UnixTimeToDateTime(double time)
        {
            DateTime startTime = new DateTime(1970, 1, 1);
            return startTime.AddSeconds(time);
        }

        /// <summary>
        /// Unix時間戳轉化為本時區時間
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        static public DateTime UnixTimeToLocalTime(double time)
        {
            DateTime d = new DateTime(1970, 1, 1).AddSeconds(time);
            return TimeZone.CurrentTimeZone.ToLocalTime(d);
        }


        /// <summary>
        /// 本時區時間轉化為Unix時間戳
        /// </summary>
        /// <param name="localTime"></param>
        /// <returns></returns>
        static public double LocalTimeToUnixTime(DateTime localTime)
        {
            return DateTimeToUnixTime(TimeZone.CurrentTimeZone.ToUniversalTime(localTime));
        }



        /// <summary>
        /// 獲取MD5字符串
        /// </summary>
        /// <returns>The md5 hash.</returns>
        /// <param name="md5">Md5.</param>
        /// <param name="input">Input.</param>
        static public string GetMd5Hash(string input, string md5AlgName = null)
        {
            MD5 md5 = md5AlgName == null ? MD5.Create() : MD5.Create(md5AlgName);
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        /// <summary>
        /// 驗證MD5
        /// </summary>
        /// <returns><c>true</c>, if md5 hash was verifyed, <c>false</c> otherwise.</returns>
        /// <param name="md5Hash">Md5 hash.</param>
        /// <param name="input">Input.</param>
        /// <param name="hash">Hash.</param>
        static public bool VerifyMd5Hash(string input, string hash)
        {
            // Hash the input. 
            string hashOfInput = GetMd5Hash(input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static public byte[] Encrypt(byte[] toEncrypt, byte[] key)
        {
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = key;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            return cTransform.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
        }


        static public byte[] Decrypt(byte[] toDecrypt, byte[] key)
        {
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = key;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateDecryptor();
            return cTransform.TransformFinalBlock(toDecrypt, 0, toDecrypt.Length);
        }


        /// <summary>
        /// 在RectTransform中做localPosition和anchoredPosition3D的轉換
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="localPosition"></param>
        /// <returns></returns>
        static public Vector3 LocalToAnchoredPosition(RectTransform rect, Vector3 localPosition)
        {
            return rect.anchoredPosition3D - rect.localPosition + localPosition;
        }

        /// <summary>
        /// 在RectTransform中做anchoredPosition3D和localPosition的轉換
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="anchoredPosition3D"></param>
        /// <returns></returns>
        static public Vector3 AnchoredToLocalPosition(RectTransform rect, Vector3 anchoredPosition3D)
        {
            return rect.localPosition - rect.anchoredPosition3D + anchoredPosition3D;
        }



        static public void SetCurrentPositoinToAnchor(RectTransform rect)
        {
            RectTransform parent = rect.transform.parent as RectTransform;
            //Q.Log(LogTag.Test, "SetCurrentPositoinToAnchor:{0} {1}", rect.anchoredPosition, parent.rect);
            Vector2 targetAnchor = new Vector2(
                rect.anchoredPosition.x / parent.rect.width + rect.anchorMax.x,
                rect.anchoredPosition.y / parent.rect.height + rect.anchorMax.y);
            rect.anchorMax = targetAnchor;
            rect.anchorMin = targetAnchor;
            rect.anchoredPosition = new Vector2();
        }


        /// <summary>
        /// 重設Animator的所有參數為默認值
        /// </summary>
        /// <param name="anim"></param>
        static public void ResetAnimatorParams(Animator anim)
        {
            if (anim == null || anim.runtimeAnimatorController == null)
                return;

            for (int i = 0, n = anim.parameters.GetLength(0); i < n; i++)
            {
                AnimatorControllerParameter p = anim.parameters[i];
                switch (p.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        anim.SetBool(p.name, p.defaultBool);
                        break;
                    case AnimatorControllerParameterType.Float:
                        anim.SetFloat(p.name, p.defaultFloat);
                        break;
                    case AnimatorControllerParameterType.Int:
                        anim.SetInteger(p.name, p.defaultInt);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        anim.ResetTrigger(p.name);
                        break;
                }
            }
        }

        const string LT = "&lt;";
        const string GT = "&gt;";
        /// <summary>
        /// xml配置裡的顏色配置不能有< >等。用 &lt; 與 &gt;轉義
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        static public string DecodeHtmlStringInXml(string val)
        {
            return val.Replace(LT, "<").Replace(GT, ">");
        }


        /// <summary>
        /// 構建出基於StreamingAssetsPath的url
        /// </summary>
        /// <returns></returns>
        static public string BuildStreamingAssetsReqUrl(string subPath)
        {
#if UNITY_EDITOR
            return string.Format("file:///{0}/{1}", Application.streamingAssetsPath, subPath);

#elif UNITY_ANDROID
            return Application.streamingAssetsPath + "/" + subPath;
#elif UNITY_IOS
            return string.Format("file://{0}/{1}", Application.streamingAssetsPath, subPath);
#else
            return string.Format("file://{0}/{1}", Application.streamingAssetsPath, subPath);
#endif
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        static public string GetAssetBundlesFolder(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.Android:
                    return "Android";
                default:
                    return platform.ToString();
            }
        }

    }
}

