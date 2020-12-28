using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace GameXP.Framewrok
{
    public class HttpUtils
    {

        private static readonly string UserAgent = "GameXP HttpUtils 1.0";

        private static readonly string ContentType = "application/x-www-form-urlencoded";

        private static int DefaultTimeout = 10 * 1000;

        public static void Get(string url, Action<HttpWebResponse> callback, Action<Exception> err)
        {
            Thread thread = new Thread(delegate ()
            {
                try
                {
                    HttpWebResponse resp = CreateHttpRequest(url, "GET", DefaultTimeout);
                    if (callback != null)
                    {
                        callback(resp);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(e);
                    if (err != null)
                        err(e);
                }
            });
            thread.Start();
        }

        public static void Get(string url, int timeout, Action<HttpWebResponse> callback, Action<Exception> err)
        {
            Thread thread = new Thread(delegate ()
            {
                try
                {
                    HttpWebResponse resp = CreateHttpRequest(url, "GET", timeout);
                    if (callback != null)
                    {
                        callback(resp);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(e);
                    if (err != null)
                        err(e);
                }
            });
            thread.Start();
        }

        public static void Get(string url, object parameters, Action<HttpWebResponse> callback, Action<Exception> err)
        {
            Thread thread = new Thread(delegate ()
            {
                try
                {
                    UriBuilder b = new UriBuilder(url);
                    /*
                     * Append Paramters to the URL
                     */

                    if (parameters != null)
                    {
                        if (!string.IsNullOrEmpty(b.Query))
                        {
                            b.Query = b.Query.Substring(1) + "&" + SerializeQueryString(parameters);
                        }
                        else
                        {
                            b.Query = SerializeQueryString(parameters);
                        }
                    }

                    HttpWebResponse resp = CreateHttpRequest(b.Uri.ToString(), "GET", DefaultTimeout);
                    if (callback != null)
                    {
                        callback(resp);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(e);
                    if (err != null)
                        err(e);
                }
            });
            thread.Start();
        }

        public static void Get(string url, int timeout, object parameters, Action<HttpWebResponse> callback, Action<Exception> err)
        {
            Thread thread = new Thread(delegate ()
            {
                try
                {
                    UriBuilder b = new UriBuilder(url);
                    /*
                     * Append Paramters to the URL
                     */

                    if (parameters != null)
                    {
                        if (!string.IsNullOrEmpty(b.Query))
                        {
                            b.Query = b.Query.Substring(1) + "&" + SerializeQueryString(parameters);
                        }
                        else
                        {
                            b.Query = SerializeQueryString(parameters);
                        }
                    }

                    HttpWebResponse resp = CreateHttpRequest(b.Uri.ToString(), "GET", timeout);
                    if (callback != null)
                    {
                        callback(resp);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(e);
                    if (err != null)
                        err(e);
                }
            });
            thread.Start();
        }

        public static void Post(string url, object parameters, Action<HttpWebResponse> callback, Action<Exception> err)
        {
            Thread thread = new Thread(delegate ()
            {
                try
                {
                    HttpWebResponse resp = CreateHttpRequest(url, "POST", DefaultTimeout, parameters);
                    if (callback != null)
                    {
                        callback(resp);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(e);
                    if (err != null)
                        err(e);
                }
            });
            thread.Start();
        }

        public static void Post(string url, int timeout, object parameters, Action<HttpWebResponse> callback, Action<Exception> err)
        {
            Thread thread = new Thread(delegate ()
            {
                try
                {
                    HttpWebResponse resp = CreateHttpRequest(url, "POST", timeout, parameters);
                    if (callback != null)
                    {
                        callback(resp);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning(e);
                    if (err != null)
                        err(e);
                }
            });
            thread.Start();
        }

        /// <summary>
        /// 創建Http請求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="timeout"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HttpWebResponse CreateHttpRequest(string url, string method, int timeout, object parameters = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url is null or empty.");
            }
            if (method == "POST" && parameters == null)
            {
                throw new ArgumentNullException("post method parameters is null.");
            }

            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = method;
            request.UserAgent = UserAgent;
            request.ContentType = ContentType;
            request.Timeout = timeout;

            // post data
            if (method == "POST")
            {
                string postbody = HttpUtils.SerializeQueryString(parameters);
                // Convert the string into a byte array. 
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postbody);
                // Write to the request stream.
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                }
            }

            HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
            return resp;
        }

        /// <summary>
        /// 讀取Response為string
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static string ReadHttpResponse(HttpWebResponse resp)
        {
            string results = "";
            try
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    results = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            return results;
        }


        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //總是接受  
            return true;
        }


        private static string SerializeQueryString(object Parameters)
        {
            string querystring = "";
            int i = 0;
            try
            {
                foreach (var property in Parameters.GetType().GetProperties())
                {
                    querystring += property.Name + "=" + System.Uri.EscapeDataString(property.GetValue(Parameters, null).ToString());
                    if (++i < Parameters.GetType().GetProperties().Length)
                    {
                        querystring += "&";
                    }
                }
            }
            catch (NullReferenceException e)
            {
                throw new ArgumentNullException("Paramters cannot be a null object", e);
            }
            return querystring;
        }
    }
}