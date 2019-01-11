
//using System;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Web;
//using System.Xml;
//using Pure.Profiler.Timings;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Reflection;

//namespace Pure.Profiler.Web.Handlers
//{
//    /// <summary>
//    /// The HttpModule of pureProfiler supports view-latest-profiling-results
//    /// via ~/pureprofiler/view and ~/pureprofiler/view/{uuid}
//    /// </summary>
//    public class PureProfilerModule : IHttpModule
//    {
//        private const string ViewUrl = "/pureprofiler/view";
//        private const string ViewUrlClear = "/pureprofiler/view/clear";
//        private const string ViewUrlInclude = "/pureprofiler/view/include";
//        //private const string ViewUrlSignal = "/pureprofiler/view/signal";
//        private const string ETCorrelationId = "X-ET-Correlation-Id";

//        /// <summary>
//        /// The default Html of the view-result index page: ~/pureprofiler/view
//        /// </summary>
//        public static string ViewResultIndexHeaderHtml = "<h1>Pure Profiler-页面资源加载时序</h1>";

//        /// <summary>
//        /// The default Html of the view-result page: ~/pureprofiler/view/{uuid}
//        /// </summary>
//        public static string ViewResultHeaderHtml = "<h1>性能检测报告</h1>";

//        /// <summary>
//        /// The handler to search for child profiling session by correlationId.
//        /// </summary>
//        public static Func<string, Guid?> DrillDownHandler { get; set; }

//        /// <summary>
//        /// The handler to search for parent profiling session by correlationId.
//        /// </summary>
//        public static Func<string, Guid?> DrillUpHandler { get; set; }



//        #region IHttpModule Members

//        /// <summary>
//        /// Disposes the current <see cref="PureProfilerModule"/>.
//        /// </summary>
//        public void Dispose()
//        {
//        }

//        private HttpApplication _application;
//        /// <summary>
//        /// Initializes the current <see cref="PureProfilerModule"/>.
//        /// </summary>
//        /// <param name="application">The application.</param>
//        public void Init(HttpApplication application)
//        {
//            application.BeginRequest += ApplicationOnBeginRequest;
//            application.EndRequest += ApplicationOnEndRequest;
//            application.Error += ApplicationOnError;
//            //application.AcquireRequestState +=new EventHandler(context_AcquireRequestState);
//            //application.PostAcquireRequestState += new EventHandler(BeginSessionAccess);
//            application.PostRequestHandlerExecute += (EndSessionAccess);

//            //application.PostReleaseRequestState += new EventHandler(EndRequest); 
//            _application = application;



//        }
//        internal void EndRequest(object sender, EventArgs e)
//        {
//            HttpContext.Current.Response.Write("EndRequest<br>");
//            HttpContext.Current.Response.Write(WebProfilingSessionContainer.GetSessions(HttpContext.Current) + "<br>");
//        }

//        private void BeginSessionAccess(object sender, EventArgs e)
//        {

//            HttpContext.Current.Response.Write("BeginSessionAccess<br>");
//            HttpContext.Current.Response.Write(WebProfilingSessionContainer.GetSessions(HttpContext.Current) + "<br>");
//        }

//        private void EndSessionAccess(object sender, EventArgs e)
//        {

//            var context = HttpContext.Current;
//            if (context == null)
//            {
//                return;
//            }


//            if (ProfilingSession.Disabled)
//            {
//                return;
//            }

//            //HttpContext.Current.Response.Write("EndSessionAccess<br>");
//            //HttpContext.Current.Response.Write(WebProfilingSessionContainer.GetSessions(HttpContext.Current) + "<br>");
//            //HttpContext.Current.Response.Write(WebProfilingSessionContainer.GetRespnoseBody(context) + "<br>");
//            if (context == null || context.Session == null || context.Session.Count == 0)
//            {
//                return;
//            }
//            else
//            {
//                _application.Application["PureProfilerSession"] = WebProfilingSessionContainer.GetSessions(HttpContext.Current);

//            }


//        }
//        private void context_AcquireRequestState(object sender, EventArgs e)
//        {
//            HttpApplication application = (HttpApplication)sender;
//            HttpContext context = application.Context;
//            HttpContext.Current.Response.Write("context_AcquireRequestState<br>");
//            HttpContext.Current.Response.Write(WebProfilingSessionContainer.GetSessions(HttpContext.Current) + "<br>");

//            if (context == null)
//            {
//                return;
//            }


//            if (ProfilingSession.Disabled)
//            {
//                return;
//            }

//            var path = context.Request.Path.TrimEnd('/');



//        }
//        #endregion

//        #region Private Methods

//        private static void ClearIfCurrentProfilingSessionStopped()
//        {
//            var profilingSession = ProfilingSession.Current;
//            if (profilingSession == null)
//            {
//                return;
//            }

//            if (profilingSession.Profiler.IsStopped)
//            {
//                ProfilingSession.ProfilingSessionContainer.Clear();
//            }
//        }

//        private string GetCorrelationIdFromHeaders(HttpContext context)
//        {
//            if (context.Request.Headers.AllKeys.Contains(ETCorrelationId))
//            {
//                var correlationIds = context.Request.Headers.GetValues(ETCorrelationId);
//                if (correlationIds != null)
//                {
//                    return correlationIds.FirstOrDefault();
//                }
//            }

//            return null;
//        }


//        InnerHttpHelper _httpHelper = null;
//        private void ApplicationOnBeginRequest(object sender, EventArgs eventArgs)
//        {
//            var context = HttpContext.Current;
//            if (context == null)
//            {
//                return;
//            }


//            if (ProfilingSession.Disabled)
//            {
//                return;
//            }

//            bool EnableProxy = ProfilingSession.Configuration.PureProfilerConfiguration.EnableProxy;
//            #region Start web profiling

//            var url = context.Request.Url.ToString();

//            // WCF profiling will be started in wcf profiling behavior
//            // so it is not necessary to start profiling here
//            if (url.Contains(".svc")) return;
//            if (EnableProxy == true && url.Contains("__proxy__=pureprofiler"))
//            {
                
//                return;
//            }

//            ProfilingSession.Start(url);

//            var correlationId = GetCorrelationIdFromHeaders(context);
//            if (!string.IsNullOrWhiteSpace(correlationId))
//            {
//                ProfilingSession.Current.AddField("correlationId", correlationId);
//            }

//            #endregion

//            if (ProfilingSession.CircularBuffer == null)
//            {
//                return;
//            }



//            ClearIfCurrentProfilingSessionStopped();


//            #region Proxy Request
//            if (EnableProxy == true && ProfilingSession.Current != null)
//	        {
//                var proxyStr = context.Request.QueryString["__proxy__"];
//                if (string.IsNullOrEmpty(proxyStr) || proxyStr.IndexOf("pureprofiler") == -1)
//                {
//                    if (_httpHelper == null)
//                    {
//                        string baseUrl = context.Request.Url.Scheme + "://" + context.Request.Url.Host;

//                        string ProxyBaseUrl = ProfilingSession.Configuration.PureProfilerConfiguration.ProxyBaseUrl;
//                        if (!string.IsNullOrEmpty(ProxyBaseUrl))
//                        {
//                            baseUrl = ProxyBaseUrl;
//                        }
//                        _httpHelper = new InnerHttpHelper(baseUrl);
//                    }

//                    string _queryString = context.Request.Url.Query;
//                    string targetUrl = _queryString.Length > 0 ? url + "&__proxy__=pureprofiler" : url + "?__proxy__=pureprofiler";
//                    var paras = WebProfilingSessionContainer.GetRequestParams(context);
//                    //var cookies = WebProfilingSessionContainer.GetRequestCookies(context);
//                    var cookiesstr = WebProfilingSessionContainer.GetRequestCookiesString(context);
//                    _httpHelper.SetCookie(cookiesstr);

//                    //var headers = WebProfilingSessionContainer.GetRequestHeaders(context);
//                    //_httpHelper.SetHeader(headers);

//                    var isGet = context.Request.HttpMethod.ToUpper() == "GET" ? true : false;
//                    string result = "";
//                    if (isGet)
//                    {
//                        result = _httpHelper.Get(paras, targetUrl);
//                    }
//                    else
//                    {
//                        if (context.Request.Files != null && context.Request.Files.Count > 0)
//                        {
//                            result = _httpHelper.PostFile(paras, targetUrl, context.Request.Files); 
//                        }
//                        else
//                        {
//                            result = _httpHelper.PostDic(paras, targetUrl);
//                        }
//                    }
            
//                    ProfilingSession.Current.Profiler.GetTimingSession().Data["ResponseBody2"] = result;
//                    //context.Response.ContentType = context.Request.Headers["Accept"];
//                    //context.Response.Write(result);
//                    //context.Response.End();
//                    return;
//                }
//	        }
           
//            #endregion

//            // only supports GET method for view results
//            //if (context.Request.HttpMethod != "GET")
//            //{
//            //    return;
//            //}

//            var path = context.Request.Path.TrimEnd('/');

//            if (path.EndsWith("/pureprofiler-resources/icons"))
//            {
//                context.Response.ContentType = "image/png";
//                var iconsStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.icons.png");
//                using (var br = new BinaryReader(iconsStream))
//                {
//                    context.Response.BinaryWrite(br.ReadBytes((int)iconsStream.Length));
//                    context.Response.End();
//                }
//                return;
//            }
//            if (path.EndsWith("/pureprofiler-resources/images/json"))
//            {
//                context.Response.ContentType = "image/png";
//                var iconsStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.json.png");
//                using (var br = new BinaryReader(iconsStream))
//                {
//                    context.Response.BinaryWrite(br.ReadBytes((int)iconsStream.Length));
//                    context.Response.End();
//                }
//                return;
//            }

//            if (path.EndsWith("/pureprofiler-resources/css"))
//            {
//                context.Response.ContentType = "text/css";
//                var cssStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.treeview_timeline.css");
//                using (var sr = new StreamReader(cssStream))
//                {
//                    context.Response.Write(sr.ReadToEnd());
//                    context.Response.End();
//                }
//                return;
//            }

//            if (path.EndsWith("/pureprofiler-resources/js"))
//            {
//                context.Response.ContentType = "text/javascript;charset=UTF-8";
//                var cssStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.pureprofiler.js");
//                using (var sr = new StreamReader(cssStream))
//                {
//                    context.Response.Write(sr.ReadToEnd());
//                    context.Response.End();
//                }
//                return;
//            }

//            //clear  ~/pureprofiler/view/clear
//            if (path.EndsWith(ViewUrlClear, StringComparison.OrdinalIgnoreCase))
//            {
//                ProfilingSession.CircularBuffer.Clear();
//                HttpContext.Current.Response.Redirect(ViewUrl);
//                return;
//            }

//            // view index of all latest results: ~/pureprofiler/view
//            if (path.EndsWith(ViewUrl, StringComparison.OrdinalIgnoreCase))
//            {
//                context.Response.ContentType = "text/html";

//                var sb = new StringBuilder();
//                sb.Append("<head>");
//                sb.Append("<title>页面资源加载时序</title>");
//                sb.Append("<link rel=\"stylesheet\" href=\"./pureprofiler-resources/css\" />");
//                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");
//                sb.Append("<style>th {   text-align: left;background: #BD6840;color: #fff;font-size: 14px; } .gray { background-color: #eee; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
//                sb.Append("</head");
//                sb.Append("<body class='pureprofiler-pagebody' style=\"background:#EAF2FF;z-index:99999;\">");
//                sb.Append(ViewResultIndexHeaderHtml);
//                sb.Append("<a target='_self' href='" + ViewUrl + "'>Refresh</a>");
//                sb.Append("&nbsp; <a target='_blank' href='export?json'>Json</a>");
//                sb.Append("&nbsp; <a target='_self' href='view/clear'>Clear</a>");
//                sb.Append("&nbsp; <a target='_self' href=\"#\" onclick=\"return clickGlobal();\">Global</a>");

//                //tab
//                sb.Append("<p>");
//                sb.Append("<div id=\"tabs1box\">");
//                sb.Append("        <div class=\"menu1box\">");
//                sb.Append("            <ul id=\"menu1\">");
//                sb.Append("                <li class=\"hover\" onmouseover=\"setPureProfilerTab(1,0)\"><a href=\"#\">Configuration</a></li>");
//                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 1)\"><a href=\"#\">Session/Cache</a></li>");
//                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 2)\"><a href=\"#\">Environment</a></li>");
//                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 3)\"><a href=\"#\">ServerVariables</a></li>");
//                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 4)\"><a href=\"#\">PureProfiler</a></li>");
//                sb.Append("            </ul>");
//                sb.Append("        </div>");
//                sb.Append("        <div class=\"tab-main1box\">");
//                sb.Append("            <div class=\"tab-main\" id=\"tab-main1\">");
//                sb.Append("                <ul class=\"block\"><li>" + ProfilingSession.AppConfigString + WebProfilingSessionContainer.WebConfigString + "</li></ul>");
//                sb.Append("                <ul><li>" + _application.Application["PureProfilerSession"] + ProfilingSession.NEWLINE + RuntimeCacheGetter.GetCaches(HttpContext.Current) + "</li></ul>");
//                sb.Append("                <ul><li>" + WebProfilingSessionContainer.EnvironmentString + "</li></ul>");
//                sb.Append("                <ul><li>" + WebProfilingSessionContainer.GetServerVariables(HttpContext.Current) + "</li></ul>");
//                sb.Append("                <ul><li>" + ProfilingSession.ConfigurationString + "</li></ul>");
//                sb.Append("            </div>");
//                sb.Append("        </div>");
//                sb.Append("    </div>");


//                sb.Append("</p>");

//                sb.Append("<table>");
//                sb.Append("<tr><th class=\"nowrap\">发生时间 (UTC)</th><th class=\"nowrap\">耗时 (ms)</th><th class=\"nowrap\">Http Verb</th><th class=\"nowrap\">Is Ajax</th><th class=\"nowrap\">状态码</th><th>Url地址</th></tr>");
//                var latestResults = ProfilingSession.CircularBuffer.OrderByDescending(r => r.Started);
//                var i = 0;
//                foreach (var result in latestResults)
//                {
//                    sb.Append("<tr");
//                    if ((i++) % 2 == 1)
//                    {
//                        sb.Append(" class=\"gray\"");
//                    }
//                    sb.Append("><td class=\"nowrap\">");
//                    sb.Append(result.Started.ToString("yyyy-MM-ddTHH:mm:ss.FFF"));
//                    sb.Append("</td><td class=\"nowrap\">");
//                    sb.Append(result.DurationMilliseconds);
//                    sb.Append("</td><td class=\"nowrap\">");
//                    string httpverb = "";
//                    if (result.Data != null && result.Data.ContainsKey("Http Verb"))
//                    {
//                        httpverb = result.Data["Http Verb"];
//                    }
//                    sb.Append(httpverb);
//                    sb.Append("</td><td class=\"nowrap\">");
//                    string IsAjaxString = "";
//                    if (result.Data != null && result.Data.ContainsKey("IsAjax"))
//                    {
//                        IsAjaxString = result.Data["IsAjax"];
//                    }
//                    sb.Append(IsAjaxString);
//                    sb.Append("</td><td class=\"nowrap\">");
//                    string statusCode = "";
//                    if (result.Data != null && result.Data.ContainsKey("ResponseStatusCode"))
//                    {
//                        statusCode = result.Data["ResponseStatusCode"];
//                    }
//                    sb.Append(statusCode);
//                    sb.Append("</td><td><a href=\"view/");
//                    sb.Append(result.Id.ToString());
//                    sb.Append("\" target=\"_blank\">");
//                    sb.Append(result.Name.Replace("\r\n", " "));
//                    sb.Append("</a></td></tr>");
//                }
//                sb.Append("</table>");

//                //author
//                //sb.Append("<%--PureProfiler @ 郭建斌--%>");

//                sb.Append("</body>");

//                context.Response.Write(sb.ToString());
//                context.Response.End();
//                return;
//            }


//            // view index of all latest results: ~/pureprofiler/view/include
//            if (path.EndsWith(ViewUrlInclude, StringComparison.OrdinalIgnoreCase))
//            {

//                context.Response.ContentType = "text/javascript;charset=UTF-8";
//                //var curr = ProfilingSession.Current.Profiler.GetTimingSession();
//                var latestResults = ProfilingSession.CircularBuffer.OrderByDescending(r => r.Started);


//                var sb = new StringBuilder();


//                var position = context.Request.QueryString["position"];
//                string positionString = "right: 20px;top: 350px;";
//                if (position == "left")
//                {
//                    positionString = "left: 20px;top: 20px;";
//                }
//                else if (position == "right")
//                {
//                    positionString = "right: 20px;top: 20px;";
//                }
//                else if (position == "bottomleft")
//                {
//                    positionString = "left: 20px;bottom: 20px;";
//                }
//                else if (position == "bottomright")
//                {
//                    positionString = "right: 20px;bottom: 20px;";
//                }
//                else if (position == "middleleft")
//                {
//                    positionString = "left: 20px;top: 350px;";
//                }
//                else if (position == "middleright")
//                {
//                    positionString = "right: 20px;top: 350px;";
//                }

//                var autoshow = context.Request.QueryString["autoshow"] == "1";
//                if (autoshow == false)
//                {
//                    var pureprofiler_autoshow = context.Request.Cookies["pureprofiler_autoshow"] != null ? context.Request.Cookies["pureprofiler_autoshow"].Value == "1" : false;
//                    if (pureprofiler_autoshow)
//                    {
//                        autoshow = pureprofiler_autoshow;
//                    }
//                }

//                string autoshowStrig = autoshow ? "" : ".pureprofiler{display:none;} ";

//                var rooturl = context.Request.QueryString["rooturl"];
//                string roolurlString = !string.IsNullOrEmpty(rooturl) ? rooturl.ToString().TrimEnd('/') : "";

//                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");


//                sb.Append("<style>" + autoshowStrig + ".pureprofiler{position:fixed;z-index: 99900;bottom:0;width:100%;height:300px;}  .pureprofiler-pin{height:32px;line-height:30px;padding:5px;position: fixed;" + positionString + "z-index: 99999; background: #008c5e;color:#fff;border-radius: 10px;box-shadow: 0 5px 10px rgba(0,0,0,.28);} .pureprofiler-refresh{color:#fff;background:#f00; padding: 5px;border-radius:10px;}  th { width: 200px; text-align: left; } .gray { background-color: #eee; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");


//                sb.Append("<div class=\"pureprofiler-pin\" id=\"pureprofiler-pin\">");
//                sb.Append("<span class=\"pureprofiler-refresh\"  id=\"pureprofiler-refresh\"> 刷新 </span>");
//                sb.Append("<span class=\"pureprofiler-show\"  id=\"pureprofiler-show\">耗时：");

//                sb.Append(CalcTime(latestResults) + " ms");
//                sb.Append("</span>");
//                sb.Append("</div>");


//                sb.Append("<div class=\"pureprofiler\" id=\"pureprofiler\"><iframe src=" + ViewUrl + " name=\"iframepureprofiler\" id=\"iframepureprofiler\"  style=\"background:#EAF2FF;z-index:99999;\" width=\"100%\" height=\"300px\" marginwidth=\"0\" marginheight=\"0\" frameborder=\"0\" scrolling=\"auto\"  ></iframe>");
//                sb.Append("</div>");



//                //js


//                sb.Append("<script>function getStyle(obj, attr) {if (obj.currentStyle) {return obj.currentStyle[attr];} else {return getComputedStyle(obj, false)[attr];}}</script>");


//                sb.Append("<script> var divProfilerShow = document.getElementById(\"pureprofiler-show\");divProfilerShow.onclick = function () { var divProfile =document.getElementById(\"pureprofiler\"); if(getStyle(divProfile, \"display\") ==\"none\"){ divProfile.style.display=\"block\";setCookie(\"pureprofiler_autoshow\",\"1\",30);}else{divProfile.style.display=\"none\";setCookie(\"pureprofiler_autoshow\",\"0\",30);}}</script>");
//                sb.Append("<script>var divProfilerRefresh = document.getElementById(\"pureprofiler-refresh\");divProfilerRefresh.onclick = function () { document.getElementById(\"iframepureprofiler\").src = \"" + ViewUrl + "\";}</script>");



//                string html = "document.write('" + sb.ToString() + "')";

//                context.Response.Write(html);
//                context.Response.End();
//                return;
//            }

//            // view specific result by uuid: ~/pureprofiler/view/{uuid}
//            if (path.IndexOf(ViewUrl, StringComparison.OrdinalIgnoreCase) >= 0)
//            {
//                context.Response.ContentType = "text/html";

//                var sb = new StringBuilder();
//                sb.Append("<head>");
//                sb.Append("<meta charset=\"utf-8\" />");
//                sb.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
//                sb.Append("<title>性能检测报告</title>");
//                sb.Append("<link rel=\"stylesheet\" href=\"./pureprofiler-resources/css\" />");
//                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");
//                sb.Append("</head");
//                sb.Append("<body>");
//                sb.Append("<h1>性能检测报告</h1>");

//                var uuid = path.Split('/').Last();
//                var result = ProfilingSession.CircularBuffer.FirstOrDefault(
//                        r => r.Id.ToString().ToLowerInvariant() == uuid.ToLowerInvariant());
//                if (result != null)
//                {
//                    sb.Append("<div class=\"css-treeview\">");

//                    // print summary
//                    sb.Append("<ul>");
//                    sb.Append("<li class=\"summary\">");
//                    PrintDrillUpLink(sb, result);
//                    sb.Append(result.Name.Replace("\r\n", " "));
//                    sb.Append("<p class='exportbar'>");
//                    sb.Append("<a target='_blank' href='../export?exporttype=json&id=" + uuid + "'>Json</a>");
//                    sb.Append("</p>");

//                    sb.Append("</li>");
//                    sb.Append("<li class=\"summary\">");
//                    if (result.Data != null)
//                    {
//                        foreach (var keyValue in result.Data)
//                        {
//                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;
//                            if ((keyValue.Key) == "RequestBody") continue;
//                            if ((keyValue.Key) == "ResponseBody") continue;
//                            if ((keyValue.Key) == "ResponseBody2") continue;
                            
//                            sb.Append("<b>");
//                            sb.Append(keyValue.Key);
//                            sb.Append(": </b>");
//                            var encodedValue = HttpUtility.HtmlEncode(keyValue.Value);
//                            if (keyValue.Key.EndsWith("Count") || keyValue.Key.EndsWith("Duration"))
//                            {
//                                sb.Append("<span class=\"");
//                                sb.Append(keyValue.Key);
//                                sb.Append("\">");
//                                sb.Append(encodedValue);
//                                sb.Append("</span>");
//                            }
//                            else
//                            {
//                                sb.Append(encodedValue);
//                            }
//                            sb.Append(" &nbsp; ");
//                        }


//                    }
//                    sb.Append("<b>所在机器: </b>");
//                    sb.Append(result.MachineName);
//                    sb.Append(" &nbsp; ");
//                    if (result.Tags != null && result.Tags.Any())
//                    {
//                        sb.Append("<b>标签: </b>");
//                        sb.Append(string.Join(", ", result.Tags));
//                        sb.Append(" &nbsp; ");
//                    }
//                    sb.Append("</li>");
//                    sb.Append("</ul>");

//                    if (result.Data != null)
//                    {
//                        //Web请求内容
//                        if (result.Data.ContainsKey("RequestBody"))
//                        {
//                            sb.Append(
//                            "<br><a title=\"RequestBody\" onclick='clickRequestBody()'>RequestBody</a>");
//                            var encodedValue = (result.Data["RequestBody"]);
//                            sb.Append("<p class=\"pureprofiler-RequestBody\" id=\"pureprofiler-RequestBody\">");
//                            sb.Append(encodedValue);
//                            sb.Append("</p> "); 
//                        }

//                        //Web响应内容
//                        if (result.Data.ContainsKey("ResponseBody"))
//                        {
//                            sb.Append(
//                            "<br><a title=\"ResponseBody\" onclick='clickResponseBody()'>ResponseBody</a>");
//                            var encodedValue = result.Data["ResponseBody"];//
//                            if (result.Data.ContainsKey("ResponseBody2"))
//                            {
//                                encodedValue +="<b>Body</b><br>"+ HttpUtility.HtmlEncode(result.Data["ResponseBody2"]);
//                            }

//                            sb.Append("<p class=\"pureprofiler-ResponseBody\" id=\"pureprofiler-ResponseBody\">");
//                            sb.Append(encodedValue);
//                            sb.Append("</p> ");

//                        }
//                    }

//                    var totalLength = result.DurationMilliseconds;
//                    if (totalLength == 0)
//                    {
//                        totalLength = 1;
//                    }
//                    var factor = 300.0 / totalLength;

//                    // print ruler
//                    sb.Append("<ul>");
//                    sb.Append("<li class=\"ruler\"><span style=\"width:300px\">0</span><span style=\"width:80px\">");
//                    sb.Append(totalLength);
//                    sb.Append(
//                        " (ms)</span><span style=\"width:20px\">&nbsp;</span><span style=\"width:60px\">开始</span><span style=\"width:60px\">耗时(ms)</span><span style=\"width:20px\">&nbsp;</span><span>执行时序</span></li>");
//                    sb.Append("</ul>");

//                    // print timings
//                    sb.Append("<ul class=\"timing\">");
//                    PrintTimings(result, result.Id, sb, factor);
//                    sb.Append("");
//                    sb.Append("</ul>");
//                    sb.Append("</div>");

//                    // print timing data popups
//                    foreach (var timing in result.Timings)
//                    {
//                        if (timing.Data == null || !timing.Data.Any()) continue;

//                        sb.Append("<aside id=\"data_");
//                        sb.Append(timing.Id.ToString());
//                        sb.Append("\" style=\"display:none\" class=\"modal\">");
//                        sb.Append("<div>");
//                        sb.Append("<h4><code>");
//                        sb.Append(timing.Name.Replace("\r\n", " "));
//                        sb.Append("</code></h4>");
//                        sb.Append("<textarea readonly=\"readonly\">");
//                        foreach (var keyValue in timing.Data)
//                        {
//                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;

//                            sb.Append(keyValue.Key);
//                            sb.Append(":\r\n");
//                            var value = keyValue.Value.Trim();
//                            if (value.StartsWith("<"))
//                            {
//                                // asuume it is XML
//                                // try to format XML with indent
//                                var doc = new XmlDocument();
//                                try
//                                {
//                                    doc.LoadXml(value);
//                                    var ms = new MemoryStream();
//                                    var writer = new XmlTextWriter(ms, null) { Formatting = Formatting.Indented };
//                                    doc.Save(writer);
//                                    ms.Seek(0, SeekOrigin.Begin);
//                                    using (var sr = new StreamReader(ms))
//                                    {
//                                        value = sr.ReadToEnd();
//                                    }
//                                }
//                                catch
//                                {
//                                    //squash exception
//                                }
//                            }
//                            sb.Append(value);
//                            sb.Append("\r\n\r\n");
//                        }
//                        if (timing.Tags != null && timing.Tags.Any())
//                        {
//                            sb.Append("tags:\r\n");
//                            sb.Append(timing.Tags);
//                            sb.Append("\r\n");
//                        }
//                        sb.Append("</textarea>");
//                        sb.Append(
//                            "<a href=\"#close\" title=\"Close\" onclick=\"this.parentNode.parentNode.style.display='none'\">关闭</a>");
//                        sb.Append("</div>");
//                        sb.Append("</aside>");
//                    }
//                }
//                else
//                {
//                    sb.Append("你所访问的结果报告不存在。");
//                }
//                sb.Append("</body>");

//                context.Response.Write(sb.ToString());
//                context.Response.End();
//                return;
//            }

//        #endregion
//        }




//        private void ApplicationOnEndRequest(object sender, EventArgs eventArgs)
//        {

//            if (ProfilingSession.ProfilingSessionContainer is WebProfilingSessionContainer)
//            {

//                WebProfilingSessionContainer container = ProfilingSession.ProfilingSessionContainer as WebProfilingSessionContainer;
//                if (container != null)
//                {
//                    container.SetResponseContent();
//                }

//            }
//            ProfilingSession.Stop();


//        }

//        private void PrintTimings(ITimingSession session, Guid parentId, StringBuilder sb, double factor)
//        {
//            var timings = session.Timings.Where(s => s.ParentId == parentId);
//            foreach (var timing in timings)
//            {
//                PrintTiming(session, timing, sb, factor);
//            }
//        }

//        private string CalcTime(IOrderedEnumerable<ITimingSession> latestResults)
//        {
//            var total = latestResults.FirstOrDefault().DurationMilliseconds;
//            //var total = latestResults.Sum(p => p.DurationMilliseconds);
//            return total.ToString();
//            //var lastDuration= _application.Application["PureProfilerLastDuration"];
//            //if (lastDuration == null)
//            //{
//            //    _application.Application["PureProfilerLastDuration"] = total;
//            //    return _application.Application["PureProfilerLastDuration"].ToString();    
//            //}
//            //else
//            //{
//            //    string urlRef = null;
//            //    long curTotal = 0;
//            //    foreach (var item in latestResults)
//            //    {
//            //        if (urlRef == null)
//            //        {
//            //            urlRef = item.Data["UrlReferrer"].ToString();
//            //            curTotal += item.DurationMilliseconds;
//            //            continue;

//            //        }
//            //        else
//            //        {
//            //            if (urlRef == item.Data["UrlReferrer"].ToString())
//            //            {
//            //                curTotal += item.DurationMilliseconds;
//            //                continue;
//            //            }
//            //            else
//            //            {
//            //                break;
//            //            }
//            //        }

//            //    }
//            //    _application.Application["PureProfilerLastDuration"] = curTotal;

//            //    return curTotal.ToString();
//            //}
//        }

//        private void PrintTiming(ITimingSession session, ITiming timing, StringBuilder sb, double factor)
//        {
//            sb.Append("<li><span class=\"timing\" style=\"padding-left: ");
//            var start = Math.Floor(timing.StartMilliseconds * factor);
//            if (start > 300)
//            {
//                start = 300;
//            }
//            sb.Append(start);
//            sb.Append("px\"><span class=\"bar ");
//            sb.Append(timing.Type);
//            sb.Append("\" title=\"");
//            sb.Append(HttpUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
//            sb.Append("\" style=\"width: ");
//            var width = (int)Math.Round(timing.DurationMilliseconds * factor);
//            if (width > 300)
//            {
//                width = 300;
//            }
//            else if (width == 0)
//            {
//                width = 1;
//            }
//            sb.Append(width);
//            sb.Append("px\"></span><span class=\"start\">+");
//            sb.Append(timing.StartMilliseconds);
//            sb.Append("</span><span class=\"duration\">");
//            sb.Append(timing.DurationMilliseconds);
//            sb.Append("</span></span>");
//            var hasChildTimings = session.Timings.Any(s => s.ParentId == timing.Id);
//            if (hasChildTimings)
//            {
//                sb.Append("<input type=\"checkbox\" id=\"t_");
//                sb.Append(timing.Id.ToString());
//                sb.Append("\" checked=\"checked\" /><label for=\"t_");
//                sb.Append(timing.Id.ToString());
//                sb.Append("\">");
//                PrintDataLink(sb, timing);
//                PrintDrillDownLink(sb, timing);
//                sb.Append(HttpUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
//                sb.Append("</label>");
//                sb.Append("<ul>");
//                PrintTimings(session, timing.Id, sb, factor);
//                sb.Append("</ul>");
//            }
//            else
//            {
//                sb.Append("<span class=\"leaf\">");
//                PrintDataLink(sb, timing);
//                PrintDrillDownLink(sb, timing);
//                sb.Append(HttpUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
//                sb.Append("</span>");
//            }
//            sb.Append("</li>");
//        }

//        private void PrintDataLink(StringBuilder sb, ITiming timing)
//        {
//            if (timing.Data == null || !timing.Data.Any()) return;

//            sb.Append("[<a href=\"#data_");
//            sb.Append(timing.Id.ToString());
//            sb.Append("\" onclick=\"document.getElementById('data_");
//            sb.Append(timing.Id.ToString());
//            sb.Append("').style.display='block';\" class=\"openModal\">data</a>] ");
//        }

//        private void PrintDrillDownLink(StringBuilder sb, ITiming timing)
//        {
//            if (timing.Data == null || !timing.Data.ContainsKey("correlationId")) return;

//            var correlationId = timing.Data["correlationId"];

//            Guid? drillDownSessionId = null;
//            if (DrillDownHandler == null)
//            {
//                //var allTimes = new List<ITiming>();
//                //foreach (var item in ProfilingSession.CircularBuffer)
//                //{
//                //    allTimes.Add(item);
//                //    if (item.Timings != null)
//                //    {
//                //        allTimes.AddRange(item.Timings);
//                //    }
//                //}
//                //var drillDownSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Timings != null && s.Timings.Any(t => t.Data != null && t.Data.ContainsKey("correlationId") && t.Data["correlationId"] == correlationId));
//                //var drillDownSession = allTimes.FirstOrDefault(s => s.Data != null && s.Data.ContainsKey("correlationId") && s.Data["correlationId"] == correlationId);
//                var drillDownSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Data != null && s.Data.ContainsKey("correlationId") && s.Data["correlationId"] == correlationId);
//                if (drillDownSession != null) drillDownSessionId = drillDownSession.Id;
//            }
//            else
//            {
//                drillDownSessionId = DrillDownHandler(correlationId);
//            }

//            if (!drillDownSessionId.HasValue) return;

//            sb.Append("[<a href=\"./");
//            sb.Append(drillDownSessionId);
//            sb.Append("\">drill down</a>] ");
//        }

//        private void PrintDrillUpLink(StringBuilder sb, ITimingSession session)
//        {
//            if (session.Data == null || !session.Data.ContainsKey("correlationId")) return;

//            var correlationId = session.Data["correlationId"];

//            Guid? drillUpSessionId = null;
//            if (DrillUpHandler == null)
//            {
//                var drillUpSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Timings != null && s.Timings.Any(t => t.Data != null && t.Data.ContainsKey("correlationId") && t.Data["correlationId"] == correlationId));
//                if (drillUpSession != null) drillUpSessionId = drillUpSession.Id;
//            }
//            else
//            {
//                drillUpSessionId = DrillUpHandler(correlationId);
//            }

//            if (!drillUpSessionId.HasValue) return;

//            sb.Append("[<a href=\"./");
//            sb.Append(drillUpSessionId);
//            sb.Append("\">drill up</a>] ");
//        }

//        private void ApplicationOnError(object sender, EventArgs eventArgs)
//        {
//            bool showError = ProfilingSession.Configuration.PureProfilerConfiguration.ShowError;
//            if (showError)
//            {
//                string newLine = "-----" + System.Environment.NewLine;// "</br>";

//                // stop and save profiling results on error
//                string err = "";
//                var context = HttpContext.Current;
//                if (context != null)
//                {
//                    Exception objErr = context.Server.GetLastError().GetBaseException();
//                    err = "Error at:      " + context.Request.Url.ToString() + newLine +
//                    "Error Message:      " + objErr.Message.ToString() + newLine +
//                    "Error Source:      " + objErr.Source.ToString() + newLine +
//                    "Stack Trace:      " + objErr.StackTrace.ToString() + newLine;

//                    if (objErr.InnerException != null)
//                    {
//                        err += "InnerException:      " + newLine +
//                    "InnerError Message:      " + objErr.Message.ToString() + newLine +
//                    "InnerError Source:      " + objErr.Source.ToString() + newLine +
//                    "InnerStack Trace:      " + objErr.StackTrace.ToString() + newLine;
//                    }

//                }

//                using (ProfilingSession.Current.Step(() => (err)))
//                {

//                }
//            }
//            else
//            {
//                using (ProfilingSession.Current.Step("Stop on Error"))
//                {



//                }
//            }



//            ProfilingSession.Stop();
//        }
//    }
//}
