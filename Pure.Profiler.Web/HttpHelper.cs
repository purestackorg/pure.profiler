using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace Pure.Profiler.Web
{
    public class InnerHttpHelper
    {
        private HttpClient _httpClient;
        private string _baseIPAddress;
        private CookieContainer cookieContainer ;
        Uri baseUri =null;
        /// <param name="ipaddress">请求的基础IP，例如：http://192.168.0.33:8080/ </param>
        public InnerHttpHelper(string ipaddress = "")
        {
            cookieContainer = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                AllowAutoRedirect = true,
                UseCookies = true
            };
            this._baseIPAddress = ipaddress;
            baseUri = new Uri(_baseIPAddress);
            _httpClient = new HttpClient(httpClientHandler) { BaseAddress = baseUri };
        }

        /// <summary>
        /// 创建带用户信息的请求客户端
        /// </summary>
        /// <param name="userName">用户账号</param>
        /// <param name="pwd">用户密码，当WebApi端不要求密码验证时，可传空串</param>
        /// <param name="uriString">The URI string.</param>
        public InnerHttpHelper(string userName, string pwd = "", string uriString = "")
            : this(uriString)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                _httpClient.DefaultRequestHeaders.Authorization = CreateBasicCredentials(userName, pwd);
            }
        }
        public void SetAuthorization(string userName, string pwd = "")
        {
            _httpClient.DefaultRequestHeaders.Authorization = CreateBasicCredentials(userName, pwd);


        }

        public void SetCookie(string cookies)
        {
            //_httpClient.DefaultRequestHeaders.Add("Host", "www.oschina.net");
            //_httpClient.DefaultRequestHeaders.Add("Method", "Post");
            //_httpClient.DefaultRequestHeaders.Add("KeepAlive", "false");   // HTTP KeepAlive设为false，防止HTTP连接保持
            //_httpClient.DefaultRequestHeaders.Add("UserAgent","Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
            if (_httpClient.DefaultRequestHeaders.Contains("Cookie"))
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");

            }
            _httpClient.DefaultRequestHeaders.Add("Cookie", cookies);

            foreach (var item in cookies.Split(';'))
            {
                cookieContainer.SetCookies(baseUri, item);

            }
           

            var cookiesHead = cookieContainer.GetCookies(baseUri);
            
        }

        public void SetHeader(Dictionary<string,string> headers)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            foreach (var item in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);


            }
           
        }

        

        /// <summary>
        /// Get请求数据
        /// <para>最终以url参数的方式提交</para>
        /// </summary>
        /// <param name="parameters">参数字典,可为空</param>
        /// <param name="requestUri">例如/api/Files/UploadFile</param>
        /// <returns></returns>
        public string Get(Dictionary<string, string> parameters, string requestUri)
        {
            if (parameters != null)
            {
                var strParam = string.Join("&", parameters.Select(o => o.Key + "=" + o.Value));
                requestUri = string.Concat(ConcatURL(requestUri), '?', strParam);
            }
            else
            {
                requestUri = ConcatURL(requestUri);
            }

            var result = _httpClient.GetStringAsync(requestUri);
            return result.Result;
        }

       
        /// <summary>
        /// Post Dic数据
        /// <para>最终以formurlencode的方式放置在http体中</para>
        /// </summary>
        /// <returns>System.String.</returns>
        public string PostDic(Dictionary<string, string> temp, string requestUri)
        {
            HttpContent httpContent = new FormUrlEncodedContent(temp);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return Post(requestUri, httpContent);
        }

        private byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
        //public string PostFile(Dictionary<string, string> paras, string requestUri, HttpFileCollection Files)
        //{

        //    using (var multipartFormDataContent = new MultipartFormDataContent())
        //    {

        //        foreach (var keyValuePair in paras)
        //        {
        //            multipartFormDataContent.Add(new StringContent(keyValuePair.Value),
        //                String.Format("\"{0}\"", keyValuePair.Key));
        //        }

        //        foreach (var key in  Files.AllKeys)
        //        {
        //            var file =  Files[key];
        //            if (file != null)
        //            {
        //                HttpContent content = new ByteArrayContent(StreamToBytes(file.InputStream));
        //                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        //                multipartFormDataContent.Add(content,
        //           '"' + System.IO.Path.GetFileNameWithoutExtension( file.FileName) + '"',
        //           '"' + file.FileName + '"'); 
        //            }
        //        }
                 
                
        //        var result = _httpClient.PostAsync(requestUri, multipartFormDataContent).Result;
                 
        //        return result.Content.ReadAsStringAsync().Result;
        //    }
        //}
 

        public string PostByte(byte[] bytes, string requestUrl)
        {
            HttpContent content = new ByteArrayContent(bytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return Post(requestUrl, content);
        }

        private string Post(string requestUrl, HttpContent content)
        {
            var result = _httpClient.PostAsync(ConcatURL(requestUrl), content);
            return result.Result.Content.ReadAsStringAsync().Result;
        }

        /// <summary>
        /// 把请求的URL相对路径组合成绝对路径
        /// </summary>
        private string ConcatURL(string requestUrl)
        {
            return new Uri(_httpClient.BaseAddress, requestUrl).OriginalString;
        }

        private AuthenticationHeaderValue CreateBasicCredentials(string userName, string password)
        {
            string toEncode = userName + ":" + password;
            // The current HTTP specification says characters here are ISO-8859-1.
            // However, the draft specification for the next version of HTTP indicates this encoding is infrequently
            // used in practice and defines behavior only for ASCII.
            Encoding encoding = Encoding.GetEncoding("utf-8");
            byte[] toBase64 = encoding.GetBytes(toEncode);
            string parameter = Convert.ToBase64String(toBase64);

            return new AuthenticationHeaderValue("Basic", parameter);
        }

    }
}