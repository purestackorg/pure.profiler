
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using System.Web; 
using System.Linq;
using System.Diagnostics;
using System.Threading; 
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;

namespace Pure.Profiler.Web
{
    /// <summary>
    /// A IProfilingSessionContainer implementation
    /// which stores current profiling session in both HttpContext.Items and CallContext,
    /// So that ProfilingSession.Current could work consistently in web application.
    /// </summary>
    public class WebProfilingSessionContainer : IProfilingSessionContainer
    {
        private const string CurrentProfilingSessionCacheKey = "pure_profiler::current_profiling_session";
        private const string CurrentProfilingStepIdCacheKey = "pure_profiler::current_profiling_step_id";
        private const string WebProfilingRequestType = "web";
         
        public static Microsoft.AspNetCore.Http.HttpContext CurrentHttpContext => _accessor.HttpContext;
        private static IHttpContextAccessor _accessor;
        private static   IHostingEnvironment _hostingEnvironment;

       
        public WebProfilingSessionContainer(IHttpContextAccessor accessor, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _accessor = accessor;
        }




        private readonly IProfilingSessionContainer _callContextProfilingSessionContainer
             = new CallContextProfilingSessionContainer();

        #region Public Methods

        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        public ProfilingSession CurrentSession
        {
            get
            {
                ProfilingSession profilingSession = null;
                if (CurrentHttpContext != null)
                {
                    // Try to get current profiling session from HttpContext.Items first
                    profilingSession = CurrentHttpContext.Items[CurrentProfilingSessionCacheKey] as ProfilingSession;
                }

                // when ProfilingSession.Start() executes in begin request event handler in a different thread
                // the callcontext might not contain the current profiling session correctly
                // so on reading of the current session from WebProfilingSessionContainer
                // double check to ensure current session stored in callcontext
                if (profilingSession != null && _callContextProfilingSessionContainer.CurrentSession == null)
                {
                    _callContextProfilingSessionContainer.CurrentSession = profilingSession;
                }

                return profilingSession
                    ?? _callContextProfilingSessionContainer.CurrentSession;
            }
            set
            {
                // Cache current profiler session in CallContext
                _callContextProfilingSessionContainer.CurrentSession = value;

                if (CurrentHttpContext != null)
                {
                    if (value != null)
                    {
                        var profiler = value.Profiler;
                        if (profiler != null)
                        {
                            // set the profiler's request type to "web"
                            profiler.GetTimingSession().Data["Http Verb"] = CurrentHttpContext.Request.Method;
                          
                            // set the profiler's IsAjax
                            profiler.GetTimingSession().Data["IsAjax"] = IsAjaxRequest(CurrentHttpContext.Request).ToString();
                            // set the profiler's request type to "web"
                            profiler.GetTimingSession().Data["请求类型"] = WebProfilingRequestType;

                            // set client IP address
                            profiler.GetTimingSession().Data["客户端IP"] = GetUserIp(CurrentHttpContext);// CurrentHttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? CurrentHttpContext.Request.UserHostAddress;
                            //request body
                            profiler.GetTimingSession().Data["RequestBody"] = GetRequestBody(CurrentHttpContext);
                            //profiler.GetTimingSession().Data["UrlReferrer"] = GetUrlReferrer(CurrentHttpContext);
                            
                            
                        }
                    }

                    // Cache current profiler session in HttpContext.Items if HttpContext accessible
                    CurrentHttpContext.Items[CurrentProfilingSessionCacheKey] = value;
                }
            }
        }

        public static string GetUserIp( Microsoft.AspNetCore.Http.HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }
            if (string.IsNullOrEmpty(ip))
                return "127.0.0.1";
            return ip;


        }

      
        public bool IsAjaxRequest(HttpRequest request)
        {
            if (request == null)
            {
                return false;
            }
            return request.Query["X-Requested-With"] == "XMLHttpRequest" || (request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest");
        }

        private string GetUrlReferrer(HttpContext context)
        {
            if (context == null)
            {
                return "";
            }
            var request = context.Request;
            string UrlReferrer = "";
            Uri uri = null;
            string referer = request.Headers[HeaderNames.Referer]; //["Referer"];
            if (!string.IsNullOrEmpty(referer))
            {
                uri = new Uri(referer);
                UrlReferrer = uri.ToString();
            }
           
            return UrlReferrer;
        }

        static Dictionary<string, object> NvcToDictionary(NameValueCollection nvc, bool handleMultipleValuesPerKey)
        {
            var result = new Dictionary<string, object>();
            foreach (string key in nvc.Keys)
            {
                if (handleMultipleValuesPerKey)
                {
                    string[] values = nvc.GetValues(key);
                    if (values.Length == 1)
                    {
                        result.Add(key, values[0]);
                    }
                    else
                    {
                        result.Add(key, values);
                    }
                }
                else
                {
                    result.Add(key, nvc[key]);
                }
            }

            return result;
        }

        public static Dictionary<string, string> GetRequestParams(HttpContext context)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (context == null)
            {
                return result;
            }
            var request = context.Request;
            Func<IEnumerable<KeyValuePair<string, StringValues>>, NameValueCollection> tryGetCollection = getter =>
            {
                try
                {
                    NameValueCollection collects = new NameValueCollection();
                    if (getter != null)
                    {
                        foreach (var item in getter)
                        {
                            collects.Add(item.Key, item.Value);
                        }
                    }
                    
                    return collects;
                    // return new NameValueCollection(getter(request));
                }
                catch (Exception e)
                {

                    return new NameValueCollection { { "CollectionFetchError", e.Message } };
                }
            };
            var _queryString = tryGetCollection( request.Query);

            NameValueCollection _formString = null;
            if (request.HasFormContentType && request.Form != null) // request.Method == "POST" 
            {
                  _formString = tryGetCollection(request.Form);

            }
            var _query = NvcToDictionary(_queryString, true);
            var _form = NvcToDictionary(_formString, true);
            foreach (var item in _query)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value.ToString());
                }
            }
            foreach (var item in _form)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value.ToString());
                }
            }

            return result;
        }

        public static Dictionary<string, string> GetRequestHeaders(HttpContext context)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (context == null)
            {
                return result;
            }
            var request = context.Request;
            Func<Func<HttpRequest, NameValueCollection>, NameValueCollection> tryGetCollection = getter =>
            {
                try
                {
                    return new NameValueCollection(getter(request));
                }
                catch (Exception e)
                {

                    return new NameValueCollection { { "CollectionFetchError", e.Message } };
                }
            };

            var _requestHeaders = new NameValueCollection(request.Headers.Count);
            foreach (var header in request.Headers.Keys)
            {
                // Cookies are handled above, no need to repeat
                if (string.Compare(header, "Cookie", StringComparison.OrdinalIgnoreCase) == 0)
                    continue;

                if (request.Headers[header] != StringValues.Empty)
                    _requestHeaders[header] = request.Headers[header];
            }


            var _header = NvcToDictionary(_requestHeaders, true); 
            foreach (var item in _header)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value.ToString());
                }
            }
            
            return result;
        }
        public static NameValueCollection GetRequestCookies(HttpContext context)
        {
            if (context == null)
            {
                return null;
            }
            var request = context.Request;
          
            NameValueCollection _cookies = null;

            try
            {
                _cookies = new NameValueCollection(request.Cookies.Count);
                foreach (var cookie in request.Cookies)
                {
                    _cookies.Add(cookie.Key, cookie.Value);

                }
                //for (var i = 0; i < request.Cookies.Count; i++)
                //{
                //    var name = request.Cookies[i].Name;
                //    _cookies.Add(name, request.Cookies[i].Value);
                //}
            }
            catch (Exception e)
            {
                //Trace.WriteLine("Error parsing cookie collection: " + e.Message);
            }
            return _cookies;
           
        }
        public static string GetRequestCookiesString(HttpContext context)
        {
            var _cookies = GetRequestCookies(context);
            if (_cookies == null)
                return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _cookies.Count; i++)
            {
                sb.AppendFormat("{0}={1}{2}", _cookies.GetKey(i), _cookies.Get(i), ";" );
            }
            return sb.ToString(); 
        }

         public string GetRawUrl(HttpContext context)
        {
            var request = context.Request;
                return new StringBuilder()
                .Append(request.PathBase)
                .Append(request.Path.Value.Split('?')[0])
                .Append(request.QueryString).ToString();
            
        }
        public string GetUserHostAddress(HttpContext context)
        {
            var request = context.Request;

            return request.Headers["X-Original-For"];
           
        }
        private string GetRequestBody(HttpContext context)
        {
            if (context == null)
            {
                return "";
            }
            var request = context.Request;
            string AbsUrl = GetRawUrl(context); //request.RawUrl.ToString();// request.Url.ToString();
            //string RawUrl = request.RawUrl.ToString();
            
            string verb = request.Method;
            //string ContentEncoding = request.ContentEncoding.ToString();
            string ContentLength = request.ContentLength.ToString();
            string ContentType = request.ContentType;
            string UrlReferrer = GetUrlReferrer(context);// request.UrlReferrer != null ?request.UrlReferrer.ToString():""; 
            string UserHostAddress = GetUserHostAddress(context);// request.UserHostAddress;
            string UserHostName = request.Headers[HeaderNames.UserAgent];// request.UserHostName; 
            //string ApplicationPath = request.ApplicationPath; 
            //string FilePath = request.FilePath;
            //string IsLocal = request.IsLocal.ToString();
            string Path = request.Path;
            //string PathInfo = request.PathInfo;
            //string PhysicalPath = request.PhysicalPath;
            //string PhysicalApplicationPath = request.PhysicalApplicationPath;
 

            NameValueCollection _propertyVariables = new NameValueCollection();
         //   _propertyVariables.Add("ContentEncoding", ContentEncoding);
            _propertyVariables.Add("ContentLength", ContentLength);
            _propertyVariables.Add("ContentType", ContentType);
            _propertyVariables.Add("UrlReferrer", UrlReferrer);
            _propertyVariables.Add("UserHostAddress", UserHostAddress);
            _propertyVariables.Add("UserHostName", UserHostName);
          //  _propertyVariables.Add("ApplicationPath", ApplicationPath);
          //  _propertyVariables.Add("FilePath", FilePath);
            _propertyVariables.Add("Path", Path);
            //_propertyVariables.Add("PathInfo", PathInfo);
            //_propertyVariables.Add("PhysicalPath", PhysicalPath);
            //_propertyVariables.Add("PhysicalApplicationPath", PhysicalApplicationPath);
            //_propertyVariables.Add("IsLocal", IsLocal);

  
           


            NameValueCollection _queryString = null;
              NameValueCollection _form = null;
              NameValueCollection _cookies = null;
              NameValueCollection _requestHeaders;
         

            //Func<Func<HttpRequest, NameValueCollection>, NameValueCollection> tryGetCollection = getter =>
            //{
            //    try
            //    {
            //        return new NameValueCollection(getter(request));
            //    }
            //    catch (Exception e)
            //    {

            //        return new NameValueCollection { { "CollectionFetchError", e.Message } };
            //    }
            //};

            Func<IEnumerable<KeyValuePair<string, StringValues>>, NameValueCollection> tryGetCollection = getter =>
            {
                try
                {
                    NameValueCollection collects = new NameValueCollection();
                    if (getter != null)
                    {
                        foreach (var item in getter)
                        {
                            collects.Add(item.Key, item.Value);
                        }
                    }

                    return collects;
                    // return new NameValueCollection(getter(request));
                }
                catch (Exception e)
                {

                    return new NameValueCollection { { "CollectionFetchError", e.Message } };
                }
            };

            StringBuilder sbFile = new StringBuilder();

            _queryString = tryGetCollection(request.Query);
            if (request.HasFormContentType &&  request.Form != null) //request.Method =="POST"
            {
                _form = tryGetCollection(request.Form);

                foreach (var file in request.Form.Files)
                {
                    // var file = request.Files[key];
                    if (file != null)
                    {
                        sbFile.AppendFormat("FileName:{0}    ContentLength:{1}    ContentType:{2}{3}", file.FileName, file.Length, file.ContentType, NEWLINE);
                    }
                }
            }

            //_serverVariables = tryGetCollection(r => r.ServerVariables);
            //_queryString = tryGetCollection(r => r.QueryString);
            //_form = tryGetCollection(r => r.Form);

            try
            {
                _cookies = new NameValueCollection(request.Cookies.Count);
                foreach (var cookie in request.Cookies)
                {
                    _cookies.Add(cookie.Key, cookie.Value);

                }
                //for (var i = 0; i < request.Cookies.Count; i++)
                //{
                //    var name = request.Cookies[i].Name;
                //    _cookies.Add(name, request.Cookies[i].Value);
                //}
            }
            catch (Exception e)
            {
                //Trace.WriteLine("Error parsing cookie collection: " + e.Message);
            }

            _requestHeaders = new NameValueCollection(request.Headers.Count);
            foreach (var header in request.Headers.Keys)
            {
                // Cookies are handled above, no need to repeat
                if (string.Compare(header, "Cookie", StringComparison.OrdinalIgnoreCase) == 0)
                    continue;

                if (request.Headers[header] != StringValues.Empty)
                    _requestHeaders[header] = request.Headers[header];
            }

            string propertyVariablesString = GetPairs(_propertyVariables).ToString();

            return string.Format("<span class=\"requestUrl\">{0} {1}</span>{7}<b>" + propertyVariablesString + "</b>{7}<b>RequestHeaders</b>{7} {2}{7}<b>QueryString</b>{7} {3}{7}<b>Form</b>{7} {4}{7}<b>Files</b>{7} {5}{7}<b>Cookies</b>{7} {6}{7}",
                verb, AbsUrl, GetPairs(_requestHeaders).ToString(), GetPairs(_queryString).ToString(), GetPairs(_form).ToString(), sbFile.ToString(), GetPairs(_cookies).ToString(), NEWLINE);
        }


        public void SetResponseContent()
        {
            if (CurrentHttpContext != null && CurrentSession != null)
            {
                //状态码
                CurrentSession.Profiler.GetTimingSession().Data["ResponseStatusCode"] = CurrentHttpContext.Response.StatusCode.ToString();

                //set response
                CurrentSession.Profiler.GetTimingSession().Data["ResponseBody"] = GetRespnoseBody(CurrentHttpContext);


            }
        }
        public void SetResponseBodyContent(string body)
        {
            if (CurrentHttpContext != null && CurrentSession != null)
            {
              
                //set response
                CurrentSession.Profiler.GetTimingSession().Data["ResponseBody2"] = body;
            }
        }

        #region ServerVariables
        public static IDictionary<string, string> ToDictionary( IDictionary<string, StringValues> input)
        {
            var result = new Dictionary<string, string>(input.Count);
            foreach (var key in input.Keys)
            {
                var typedKey = key.ToString();
                result.Add(typedKey, input[typedKey] != StringValues.Empty ? input[typedKey].ToString() :"");
            }

            return result;
        } 
        public static string GetServerVariables(HttpContext context)
        {
            if (context == null)
            {
                return "";
            }
            //NameValueCollection _serverVariables;
            //var request = context.Request;

            //Func<Func<HttpRequest, NameValueCollection>, NameValueCollection> tryGetCollection = getter =>
            //{
            //    try
            //    {
            //        return new NameValueCollection(getter(request));
            //    }
            //    catch (Exception e)
            //    {

            //        return new NameValueCollection { { "CollectionFetchError", e.Message } };
            //    }
            //};

            //_serverVariables = tryGetCollection(r => r.ServerVariables);

            //return ProfilingSession.GetPairs(_serverVariables).ToString();

            var HttpVariables = new Dictionary<string, string>();
            var GeneralVariables = new Dictionary<string, string>();
            var SecurityRelatedVariables = new Dictionary<string, string>();

            foreach (var serverVariable in ToDictionary(context.Request.Headers))
            {
                string lowerCasedKey = serverVariable.Key.ToLower();

                if (lowerCasedKey.StartsWith("http_"))
                {
                    HttpVariables.Add(serverVariable.Key, serverVariable.Value);
                }
                else if (lowerCasedKey.StartsWith("cert_") || lowerCasedKey.StartsWith("https_"))
                {
                    SecurityRelatedVariables.Add(serverVariable.Key, serverVariable.Value);
                }
                else
                {
                    GeneralVariables.Add(serverVariable.Key, serverVariable.Value);
                }
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<b>HttpVariables</b>:{0}", NEWLINE);
            foreach (var item in HttpVariables)
            {
                sb.AppendFormat("{0}:{1}{2}", item.Key, item.Value, ProfilingSession.NEWLINE);

            }
            sb.AppendFormat("{0}", NEWLINE);

            sb.AppendFormat("<b>SecurityRelatedVariables</b>:{0}", NEWLINE);
            foreach (var item in SecurityRelatedVariables)
            {
                sb.AppendFormat("{0}:{1}{2}", item.Key, item.Value, ProfilingSession.NEWLINE);

            }
            sb.AppendFormat("{0}", NEWLINE);

            sb.AppendFormat("<b>GeneralVariables</b>:{0}", NEWLINE);
            foreach (var item in GeneralVariables)
            {
                sb.AppendFormat("{0}:{1}{2}", item.Key, item.Value, ProfilingSession.NEWLINE);

            }
            sb.AppendFormat("{0}", NEWLINE);

            return sb.ToString();
        }

        
       
        #endregion

        #region Session
        public static string GetSessions(HttpContext context)
        {
            StringBuilder sbSession = new StringBuilder();
            sbSession.AppendFormat("<b>Session</b>:{0}", ProfilingSession.NEWLINE);
            if (context == null || context.Session == null || context.Session.Keys.Count() == 0)
            {
                return sbSession.ToString();
            }


            var session = context.Session;
            sbSession.AppendFormat("SessionID:{0}{1}", session.Id, ProfilingSession.NEWLINE);
            //sbSession.AppendFormat("Timeout:{0}{1}", session.Timeout, ProfilingSession.NEWLINE);
            //sbSession.AppendFormat("Mode:{0}{1}", session.Mode, ProfilingSession.NEWLINE);
            //sbSession.AppendFormat("CookieMode:{0}{1}", session.CookieMode, ProfilingSession.NEWLINE);
            sbSession.AppendFormat("IsAvailable:{0}{1}", session.IsAvailable, ProfilingSession.NEWLINE);
            sbSession.AppendFormat("{0}", ProfilingSession.NEWLINE);
            foreach (string key in session.Keys)
            {
                object o = session.GetString(key);//  session[key];
                sbSession.AppendFormat("Key:{0}    Value:{1}    Type:{2}{3}", key, o, o != null ? o.GetType().ToString() : "", ProfilingSession.NEWLINE);
            }
            return sbSession.ToString();
        }

        #endregion

        #region Env
        private static string _EnvironmentString = "";

        public static string EnvironmentString
        {
            get
            {
                lock (syncRoot)
                {
                    if (string.IsNullOrEmpty(_EnvironmentString))
                    {
                        StringBuilder sbPureProfiler = new StringBuilder(); 

                        sbPureProfiler.AppendFormat("<b>WebServer</b>:{0}", NEWLINE);
                        sbPureProfiler.AppendFormat(BuildWebServerDetails(CurrentHttpContext));
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        sbPureProfiler.AppendFormat("<b>Framework</b>:{0}", NEWLINE);
                        sbPureProfiler.AppendFormat(BuildFrameworkDetails(CurrentHttpContext));
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        sbPureProfiler.AppendFormat("<b>Machine</b>:{0}", NEWLINE);
                        sbPureProfiler.AppendFormat(BuildMachineDetails());
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        sbPureProfiler.AppendFormat("<b>TimeZone</b>:{0}", NEWLINE);
                        sbPureProfiler.AppendFormat(BuildTimeZoneDetails());
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);


                        sbPureProfiler.AppendFormat("<b>Process</b>:{0}", NEWLINE);
                        sbPureProfiler.AppendFormat(BuildProcessDetails());
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        sbPureProfiler.AppendFormat("<b>ApplicationAssemblies</b>:{0}", NEWLINE);
                        sbPureProfiler.AppendFormat(BuildProcessDetails());
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        sbPureProfiler.AppendFormat(FindAssemblies());
                        
                        _EnvironmentString = sbPureProfiler.ToString();

                    }
                }
                return _EnvironmentString;
            }
        }
        private static bool? IsFullyTrusted(Assembly assembly)
        {
#if NET35
            return null;      
#else
            return assembly.IsFullyTrusted;
#endif
        }
        private static string GetVersionNumber(Assembly assembly)
        {
            var infoVersion = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                                            .Cast<AssemblyInformationalVersionAttribute>()
                                            .SingleOrDefault();

            return infoVersion != null ? infoVersion.InformationalVersion : null;
        }
        private  static bool IsAssemblyDebugBuild(Assembly assembly)
        {
            foreach (var attribute in assembly.GetCustomAttributes(typeof(DebuggableAttribute), false))
            {
                var debuggableAttribute = attribute as DebuggableAttribute;
                if (debuggableAttribute != null)
                {
                    return debuggableAttribute.IsJITTrackingEnabled;
                }
            }
            return false;
        }
        private readonly static IEnumerable<string> systemNamspaces = new List<string> { "System", "Microsoft" }; 

        private static string FindAssemblies()
        {
            var allAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies().OfType<Assembly>().Concat(AppDomain.CurrentDomain.GetAssemblies()).Distinct().OrderBy(o => o.FullName);

            StringBuilder sbSys = new StringBuilder();
            StringBuilder sbApp = new StringBuilder();
            sbApp.AppendFormat("<b>ApplicationAssemblies</b>:{0}", NEWLINE);
            sbSys.AppendFormat("<b>SystemAssemblies</b>:{0}", NEWLINE);
          
            foreach (var assembly in allAssemblies)
            {
                var assemblyName = assembly.GetName();
                var name = assemblyName.Name;
                var version = assemblyName.Version.ToString();
                var versionInfo = GetVersionNumber(assembly);
                var culture = string.IsNullOrEmpty(assemblyName.CultureInfo.Name) ? "neutral" : assemblyName.CultureInfo.Name;
                var fromGac = assembly.GlobalAssemblyCache;
                var fullTrust = IsFullyTrusted(assembly);
                var buildMode = IsAssemblyDebugBuild(assembly) == true ? "Debug" : "Release";
                string str = string.Format("Name:{0}    Version:{1}    VersionInfo:{2}    Culture:{3}    FromGac:{4}    FullTrust:{5}    BuildMode:{6}",
                    name, version, versionInfo, culture, fromGac, fullTrust, buildMode);
                var isSystem = systemNamspaces.Any(systemNamspace => assembly.FullName.StartsWith(systemNamspace));
                if (isSystem)
                {
                    sbSys.Append(str);
                    sbSys.Append(NEWLINE);
                }
                else
                {
                    sbApp.Append(str);
                    sbApp.Append(NEWLINE);
                }
            }
            return sbApp.ToString() + NEWLINE + sbSys.ToString();
        }

        private static string BuildProcessDetails()
        {
            var process = Process.GetCurrentProcess();

            var processId = process.Id;
            var processName = process.MainModule.ModuleName;
            var startTime = process.StartTime;
            var result = new StringBuilder();
            result.AppendFormat("ProcessId:{0}{1}", processId, NEWLINE);
            result.AppendFormat("WorkerProcess:{0}{1}", processName, NEWLINE);
            result.AppendFormat("StartTime:{0}{1}", startTime, NEWLINE); 

            return result.ToString(); 
        }
        private static string BuildTimeZoneDetails()
        {
            var timeZoneInfo = TimeZoneInfo.Local;

            var name = timeZoneInfo.DaylightName;
            var utcOffset = timeZoneInfo.BaseUtcOffset.Hours;
            var utcOffsetWithDls = timeZoneInfo.BaseUtcOffset.Hours;
            var isDaylightSavingTime = false;
            if (timeZoneInfo.IsDaylightSavingTime(DateTime.Now))
            {
                utcOffsetWithDls++;
                isDaylightSavingTime = true;
            }

            var result = new StringBuilder();
            result.AppendFormat("Name:{0}{1}", name, NEWLINE);
            result.AppendFormat("IsDaylightSavingTime:{0}{1}", isDaylightSavingTime, NEWLINE);
            result.AppendFormat("UtcOffset:{0}{1}", utcOffset, NEWLINE);
            result.AppendFormat("UtcOffsetWithDls:{0}{1}", utcOffsetWithDls, NEWLINE);
            
            return result.ToString();
        }
        private static string BuildWebServerDetails(HttpContext context)
        {
            var serverSoftware = context.Request.Headers["SERVER_SOFTWARE"] != StringValues.Empty ? context.Request.Headers["SERVER_SOFTWARE"].ToString():"" ;// context.Request.ServerVariables["SERVER_SOFTWARE"];
            var processName = Process.GetCurrentProcess().MainModule.ModuleName;

            var serverType = !string.IsNullOrEmpty(serverSoftware) ? serverSoftware : processName.StartsWith("WebDev.WebServer", StringComparison.InvariantCultureIgnoreCase) ? "Visual Studio Web Development Server" : "Unknown";
           // var integratedPipeline = HttpRuntime.UsingIntegratedPipeline;
            var result = new StringBuilder();
            result.AppendFormat("ServerType:{0}{1}", serverType, NEWLINE);
           // result.AppendFormat("IntegratedPipeline:{0}{1}", integratedPipeline, NEWLINE);

            return result.ToString();
        }
        private static string BuildFrameworkDetails(HttpContext context)
        {
            var dotnetFramework = string.Format(".NET {0} ({1} bit)", System.Environment.Version, IntPtr.Size * 8);
            //var debugging = context.IsDebuggingEnabled;
            var serverCulture = Thread.CurrentThread.CurrentCulture.DisplayName;
            //var currentTrustLevel = GetCurrentTrustLevel().ToString();
            var result = new StringBuilder();
            result.AppendFormat("Framework Environment:{0}{1}", dotnetFramework, NEWLINE);
         //   result.AppendFormat("Debugging:{0}{1}", debugging, NEWLINE);
            result.AppendFormat("ServerCulture:{0}{1}", serverCulture, NEWLINE);
           // result.AppendFormat("CurrentTrustLevel:{0}{1}", currentTrustLevel, NEWLINE);

            return result.ToString();
        }
        private static bool? Is64BitOperatingSystem()
        {
#if NET35
            return null;      
#else
            return System.Environment.Is64BitOperatingSystem;
#endif
        }
        private static string BuildMachineDetails()
        {
            var is64BitOperatingSystem = Is64BitOperatingSystem();
            var name = string.Format("{0} ({1} processors)", System.Environment.MachineName, System.Environment.ProcessorCount);
            var operatingSystem = string.Format("{0} ({1} bit)", System.Environment.OSVersion.VersionString, is64BitOperatingSystem == null ? "?" : is64BitOperatingSystem.Value ? "64" : "32");
            var startTime = DateTime.Now.AddMilliseconds(System.Environment.TickCount * -1);
            var result = new StringBuilder();
            result.AppendFormat("Name:{0}{1}", name, NEWLINE);
            result.AppendFormat("OperatingSystem:{0}{1}", operatingSystem, NEWLINE);
            result.AppendFormat("StartTime:{0}{1}", startTime, NEWLINE); 

            return result.ToString(); 
        }
        //private static AspNetHostingPermissionLevel GetCurrentTrustLevel()
        //{
        //    var levels = new[] { AspNetHostingPermissionLevel.Unrestricted, AspNetHostingPermissionLevel.High, AspNetHostingPermissionLevel.Medium, AspNetHostingPermissionLevel.Low, AspNetHostingPermissionLevel.Minimal };
        //    foreach (var trustLevel in levels)
        //    {
        //        try
        //        {
        //            new AspNetHostingPermission(trustLevel).Demand();
        //        }
        //        catch (System.Security.SecurityException)
        //        {
        //            continue;
        //        }

        //        return trustLevel;
        //    }

        //    return AspNetHostingPermissionLevel.None;
        //}
        #endregion

        #region Config
        private static readonly object syncRoot = new object();
        private static string _AppConfgString = "";
        public static string WebConfigString
        {
            get
            {
                lock (syncRoot)
                {
                    if (string.IsNullOrEmpty(_AppConfgString))
                    {
                        StringBuilder sbPureProfiler = new StringBuilder();

                        string WebRootPath = _hostingEnvironment.WebRootPath;
                        string ApplicationName = _hostingEnvironment.ApplicationName;
                        string EnvironmentName = _hostingEnvironment.EnvironmentName;
                        string ContentRootPath = _hostingEnvironment.ContentRootPath;
                        bool IsProduction = _hostingEnvironment.IsProduction();
                        bool IsDevelopment = _hostingEnvironment.IsDevelopment();
                        // bool IsEnvironment = _hostingEnvironment.IsEnvironment();
                        bool IsStaging = _hostingEnvironment.IsStaging();

                        sbPureProfiler.AppendFormat("<b>ApplicationName</b>:{0}", ApplicationName); 
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        sbPureProfiler.AppendFormat("<b>WebRootPath</b>:{0}", WebRootPath);
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);
                        sbPureProfiler.AppendFormat("<b>ContentRootPath</b>:{0}", ContentRootPath);
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);
                        sbPureProfiler.AppendFormat("<b>EnvironmentName</b>:{0}", EnvironmentName);
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);
                        sbPureProfiler.AppendFormat("<b>IsProduction</b>:{0}", IsProduction);
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);
                        sbPureProfiler.AppendFormat("<b>IsDevelopment</b>:{0}", IsDevelopment);
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);
                        sbPureProfiler.AppendFormat("<b>IsStaging</b>:{0}", IsStaging);
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                

                        //sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        //sbPureProfiler.AppendFormat("<b>Authentication</b>:{0}", NEWLINE);
                        //sbPureProfiler.AppendFormat(ProcessAuthenticationSection(ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection));
                        //sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        //sbPureProfiler.AppendFormat("<b>CustomErrors</b>:{0}", NEWLINE);
                        //sbPureProfiler.AppendFormat(ProcessCustomErrors(ConfigurationManager.GetSection("system.web/customErrors") as CustomErrorsSection));
                        //sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        //sbPureProfiler.AppendFormat("<b>RoleManager</b>:{0}", NEWLINE);
                        //sbPureProfiler.AppendFormat(ProcessRoleManager(ConfigurationManager.GetSection("system.web/roleManager") as RoleManagerSection));
                        //sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        //sbPureProfiler.AppendFormat("<b>HttpModules</b>:{0}", NEWLINE);
                        //sbPureProfiler.AppendFormat( ProcessHttpModules(ConfigurationManager.GetSection("system.web/httpModules") as HttpModulesSection));
                        //sbPureProfiler.AppendFormat("{0}", NEWLINE);


                        //sbPureProfiler.AppendFormat("<b>HttpHandlers</b>:{0}", NEWLINE);
                        //sbPureProfiler.AppendFormat(ProcessHttpHandler(ConfigurationManager.GetSection("system.web/httpHandlers") as HttpHandlersSection));
                        //sbPureProfiler.AppendFormat("{0}", NEWLINE);
                        
                        _AppConfgString = sbPureProfiler.ToString();

                    }
                }
                return _AppConfgString;
            }
        }

        //private static string ProcessRoleManager(RoleManagerSection roleManagerSection)
        //{
        //    if (roleManagerSection == null)
        //    {
        //        return null;
        //    }

        //    var result = new StringBuilder();
        //    result.AppendFormat("Domain:{0}{1}", roleManagerSection.Domain, NEWLINE);
        //    result.AppendFormat("DefaultProvider:{0}{1}", roleManagerSection.DefaultProvider, NEWLINE);
        //    result.AppendFormat("Enabled:{0}{1}", roleManagerSection.Enabled, NEWLINE);
        //    result.AppendFormat("CookieName:{0}{1}", roleManagerSection.CookieName, NEWLINE);
        //    result.AppendFormat("CookiePath:{0}{1}", roleManagerSection.CookiePath, NEWLINE);
        //    result.AppendFormat("CacheRolesInCookie:{0}{1}", roleManagerSection.CacheRolesInCookie, NEWLINE);
        //    result.AppendFormat("CookieProtection:{0}{1}", roleManagerSection.CookieProtection, NEWLINE);
        //    result.AppendFormat("CookieRequireSSL:{0}{1}", roleManagerSection.CookieRequireSSL, NEWLINE);
        //    result.AppendFormat("CookieSlidingExpiration:{0}{1}", roleManagerSection.CookieSlidingExpiration, NEWLINE);
        //    result.AppendFormat("CookieTimeout:{0}{1}", roleManagerSection.CookieTimeout, NEWLINE);
        //    result.AppendFormat("CreatePersistentCookie:{0}{1}", roleManagerSection.CreatePersistentCookie, NEWLINE);
        //    result.AppendFormat("MaxCachedResults:{0}{1}", roleManagerSection.MaxCachedResults, NEWLINE);

 

        //    var providerSection = roleManagerSection.Providers;
        //    if (providerSection != null)
        //    {
        //        result.AppendFormat("Providers:{0}", NEWLINE);

        //        foreach (ProviderSettings provider in providerSection)
        //        {
        //            result.AppendFormat("Name:{0}    Type:{1}    Parameters:{2}{3}", provider.Name, provider.Type, ProfilingSession.GetPairs(provider.Parameters), NEWLINE);
                     
        //        }
                 
        //    }

        //    return result.ToString();
        //}

        //private static string ProcessCustomErrors(CustomErrorsSection customErrorsSection)
        //{
        //    if (customErrorsSection == null)
        //    {
        //        return null;
        //    }
        //    var result = new StringBuilder();
        //    result.AppendFormat("Mode:{0}{1}", customErrorsSection.Mode.ToString(), NEWLINE);
        //    result.AppendFormat("DefaultRedirect:{0}{1}", customErrorsSection.DefaultRedirect, NEWLINE);
        //    result.AppendFormat("RedirectMode:{0}{1}", customErrorsSection.RedirectMode.ToString(), NEWLINE);
             

        //    var errorsSection = customErrorsSection.Errors;
        //    if (errorsSection != null)
        //    {
        //        result.AppendFormat("ErrorsSection:{0}", NEWLINE);

        //        foreach (CustomError error in errorsSection)
        //        {
        //            result.AppendFormat("Redirect:{0}    StatusCode{1}{2}", error.Redirect, error.StatusCode, NEWLINE);
                     
                     
        //        }
                 
        //    }

        //    return result.ToString();
        //}


        //private static string  ProcessAuthenticationSection(AuthenticationSection authenticationSection)
        //{
        //    if (authenticationSection == null)
        //    {
        //        return null;
        //    }

        //    var formsSection = authenticationSection.Forms;
        //    var result = new StringBuilder();
        //    result.AppendFormat("Mode:{0}{1}", authenticationSection.Mode.ToString(), NEWLINE);

        //    if (formsSection != null)
        //    {
        //        result.AppendFormat("Name:{0}    Path:{1}    Timeout:{2}    Cookieless:{3}    DefaultUrl:{4}    Domain:{5}    EnableCrossAppRedirects:{6}    Protection:{7}    RequireSSL:{8}    SlidingExpiration:{9}    TicketCompatibilityMode:{10}", formsSection.Name, formsSection.Path, formsSection.Timeout, formsSection.Cookieless,
        //            formsSection.DefaultUrl, formsSection.Domain, formsSection.EnableCrossAppRedirects, formsSection.Protection, formsSection.RequireSSL,
        //            formsSection.SlidingExpiration, formsSection.TicketCompatibilityMode);

        //        result.AppendFormat("{0}",  NEWLINE);
        //        result.AppendFormat("Credentials:{0}", NEWLINE);
         
        //        var credentialsSection = formsSection.Credentials;
        //        if (credentialsSection != null)
        //        {
        //            result.AppendFormat("PasswordFormat:{0}{1}", credentialsSection.PasswordFormat.ToString(), NEWLINE);
                     
        //        }
        //    }

        //    return result.ToString();
        //}
        //private static string ProcessHttpModules(HttpModulesSection httpModulesSection)
        //{
        //    if (httpModulesSection == null)
        //    {
        //        return "";
        //    }

        //    var result = new StringBuilder();
        //    foreach (HttpModuleAction httpModule in httpModulesSection.Modules)
        //    {
        //        result.AppendFormat("{0}:{1}{2}", httpModule.Name, httpModule.Type, NEWLINE);
                 
        //    }

        //    return result.ToString();
        //}

        //private static string ProcessHttpHandler(HttpHandlersSection httpHandlersSection)
        //{
        //    if (httpHandlersSection == null)
        //    {
        //        return null;
        //    }
        //    var result = new StringBuilder();

    
        //    foreach (HttpHandlerAction httpModule in httpHandlersSection.Handlers)
        //    {
        //        result.AppendFormat("Path:{0}    Type:{1}    Verb:{2}    Validate:{3}{4}", httpModule.Path, httpModule.Type, httpModule.Verb, httpModule.Validate, NEWLINE);
        //    }

        //    return result.ToString();
        //}
        #endregion
      

        public static string GetRespnoseBody(HttpContext context)
        {
             
            var response = context.Response;

           // string HeaderEncoding =   response.HeaderEncoding.ToString();
         //   string ContentEncoding = response.ContentEncoding.ToString();
            string StatusCode = response.StatusCode.ToString();
            string ContentType = response.ContentType;
            //string Charset = response.Charset;

            NameValueCollection _propertyVariables = new NameValueCollection();
           // _propertyVariables.Add("ContentEncoding", ContentEncoding);
            _propertyVariables.Add("StatusCode", StatusCode);
            _propertyVariables.Add("ContentType", ContentType);
         //   _propertyVariables.Add("Charset", Charset);

            string ouputText = "";// Pure.Profiler.Web.Proxy.HttpContextProxy.GetResponseText(response);
            
            //using (var sr = new StreamReader(response.OutputStream, System.Text.Encoding.Default))
            //{
            //    ouputText = sr.ReadToEnd();
            //    sr.Close();
            //}

            
             
            NameValueCollection _cookies = null;
            NameValueCollection _requestHeaders;

            
            //try
            //{
            //    _cookies = new NameValueCollection(response.Cookies.Count);
            //    for (var i = 0; i < response.Cookies.Count; i++)
            //    {
            //        var name = response.Cookies[i].Name;
            //        _cookies.Add(name, response.Cookies[i].Value);
            //    }
            //}
            //catch (Exception e)
            //{
            //    //Trace.WriteLine("Error parsing cookie collection: " + e.Message);
            //}

            _requestHeaders = new NameValueCollection(response.Headers.Count);
            foreach (var header in response.Headers.Keys)
            {
                // Cookies are handled above, no need to repeat
                if (string.Compare(header, "Cookie", StringComparison.OrdinalIgnoreCase) == 0)
                    continue;

                if (response.Headers[header] != StringValues.Empty)
                    _requestHeaders[header] = response.Headers[header];
            }


            return string.Format("<b>{0}</b>{5}<b>RequestHeaders</b>{5} {1}{5}<b>Cookies</b>{5} {2}{5}{5}{3}{5} ",
                ProfilingSession.GetPairs(_propertyVariables).ToString(), ProfilingSession.GetPairs(_requestHeaders).ToString(), ProfilingSession.GetPairs(_cookies).ToString(), ouputText, "", NEWLINE);
        }

        static string NEWLINE = "<br>";

        private StringBuilder GetPairs(NameValueCollection nvc)
        {
            var result = new StringBuilder();
            if (nvc == null)
                return result;

            for (int i = 0; i < nvc.Count; i++)
            {
                result.AppendFormat("{0}:{1}{2}", nvc.GetKey(i), nvc.Get(i), NEWLINE);
            }
            return result;
        }

        /// <summary>
        /// Gets or sets the current profiling step id.
        /// </summary>
        public Guid? CurrentSessionStepId
        {
            get
            {
                Guid? stepId = null;
                if (CurrentHttpContext != null)
                {
                    // Try to get current step id from HttpContext.Items first
                    stepId = CurrentHttpContext.Items[CurrentProfilingStepIdCacheKey] as Guid?;
                }

                // when ProfilingSession.Start() executes in begin request event handler in a different thread
                // the callcontext might not contain the current profiling session step id correctly
                // so on reading of the step id from WebProfilingSessionContainer
                // double check to ensure step id stored in callcontext
                if (stepId != null && _callContextProfilingSessionContainer.CurrentSessionStepId == null)
                {
                    _callContextProfilingSessionContainer.CurrentSessionStepId = stepId;
                }

                return stepId ?? _callContextProfilingSessionContainer.CurrentSessionStepId;
            }
            set
            {
                // Cache current step if in CallContext
                _callContextProfilingSessionContainer.CurrentSessionStepId = value;

                if (CurrentHttpContext != null)
                {
                    // Cache current step id in HttpContext.Items if HttpContext accessible
                    CurrentHttpContext.Items[CurrentProfilingStepIdCacheKey] = value;
                }
            }
        }

        /// <summary>
        /// Clears the current profiling session &amp; step id.
        /// </summary>
        public void Clear()
        {
            // clear callcontext container
            _callContextProfilingSessionContainer.Clear();

            // clear current session
            CurrentSession = null;

            // clear step id
            CurrentSessionStepId = null;
        }

        #endregion

        #region IProfilingSessionContainer Members

        ProfilingSession IProfilingSessionContainer.CurrentSession
        {
            get { return CurrentSession; }
            set { CurrentSession = value; }
        }

        Guid? IProfilingSessionContainer.CurrentSessionStepId
        {
            get { return CurrentSessionStepId; }
            set { CurrentSessionStepId = value; }
        }

        void IProfilingSessionContainer.Clear()
        {
            Clear();
        }

        #endregion
    }
}
