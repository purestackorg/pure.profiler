using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;
using Pure.Profiler.Timings;
using Pure.Profiler.Web.Import;
using Pure.Profiler.Configuration;

namespace Pure.Profiler.Web
{
    public class PureProfilerMiddleware
    {
        private readonly RequestDelegate _next;

        public const string XCorrelationId = "X-ET-Correlation-Id";


        private const string ViewUrl = "/pureprofiler/view";
        private const string ViewUrlClear = "/pureprofiler/view/clear";
        private const string ViewUrlInclude = "/pureprofiler/view/include";
       private const string ExportUrl = "/pureprofiler/export";

        private const string Import = "import";
        private const string ExportJson = "json";
        private const string ExportType = "exporttype";
        private const string QueryString = "QUERY_STRING";

        //private const string Import = "import";
        //private const string Export = "?export";
        private const string CorrelationId = "correlationId";
        /// <summary>
        /// The default Html of the view-result index page: ~/pureprofiler/view
        /// </summary>
        public static string ViewResultIndexHeaderHtml = "<h1>Pure Profiler-页面资源加载时序</h1>";

        /// <summary>
        /// The default Html of the view-result page: ~/pureprofiler/view/{uuid}
        /// </summary>
        public static string ViewResultHeaderHtml = "<h1>性能检测报告</h1>";


        /// <summary>
        /// Tries to import drilldown result by remote address of the step
        /// </summary>
        public static bool TryToImportDrillDownResult;

        /// <summary>
        /// The handler to search for child profiling session by correlationId.
        /// </summary>
        public static Func<string, Guid?> DrillDownHandler { get; set; }

        /// <summary>
        /// The handler to search for parent profiling session by correlationId.
        /// </summary>
        public static Func<string, Guid?> DrillUpHandler { get; set; }

        public PureProfilerMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        public async Task Invoke(HttpContext context)
        {
             
            if (context == null)
            {
                await _next.Invoke(context);

                return;
            }
                  
            if (ProfilingSession.Disabled || ConfigurationHelper.LoadPureProfilerConfigurationSection().EnableProfiler == false)
            {
                await _next.Invoke(context);

                return;
            }

            // disable view profiling if CircularBuffer is not enabled
            if (ProfilingSession.CircularBuffer == null)
            {
                await _next.Invoke(context);
                return;
            }

            ClearIfCurrentProfilingSessionStopped();

            var url = UriHelper.GetDisplayUrl(context.Request);
            ProfilingSession.Start(url);

            // set correlationId if exists in header
            var correlationId = GetCorrelationIdFromHeaders(context);
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                ProfilingSession.Current.AddField("correlationId", correlationId);
            }

            // only supports GET method for view results
            //if (context.Request.Method != "GET")
            //{
            //    try
            //    {
            //        await _next.Invoke(context);
            //    }
            //    catch (System.Exception)
            //    {
            //        // stop and save profiling results on error
            //        using (ProfilingSession.Current.Step("Stop on Error")) { }

            //        throw;
            //    }
            //    finally
            //    {
            //        ProfilingSession.Stop();
            //    }
            //    return;
            //}

            var path = context.Request.Path.ToString().TrimEnd('/');

            // generate baseViewPath
            string baseViewPath = null;
            var posStart = path.IndexOf(ViewUrl, StringComparison.OrdinalIgnoreCase);
            //if (posStart < 0)
            //    posStart = path.IndexOf(ViewUrlNano, StringComparison.OrdinalIgnoreCase);
            if (posStart >= 0)
                baseViewPath = path.Substring(0, posStart) + ViewUrl;

            // prepend pathbase if specified
            baseViewPath = context.Request.PathBase + baseViewPath;

           
            if (path.EndsWith("/pureprofiler-resources/icons"))
            {
                context.Response.ContentType = "image/png";
                var iconsStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.icons.png");

                using (var br = new BinaryReader(iconsStream))
                {
                    await context.Response.Body.WriteAsync(br.ReadBytes((int)iconsStream.Length), 0, (int)iconsStream.Length);
                }
                return;
                 
            }
            if (path.EndsWith("/pureprofiler-resources/images/json"))
            {
                context.Response.ContentType = "image/png";
                var iconsStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.json.png");
                using (var br = new BinaryReader(iconsStream))
                {
                    await context.Response.Body.WriteAsync(br.ReadBytes((int)iconsStream.Length), 0, (int)iconsStream.Length);
                }
                return;
            }


            if (path.EndsWith("/pureprofiler-resources/css"))
            {
                context.Response.ContentType = "text/css;charset=utf-8";
                var cssStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.treeview_timeline.css");
                using (var sr = new StreamReader(cssStream))
                {
                    await context.Response.WriteAsync(sr.ReadToEnd());
                }
                return;
            }

            if (path.EndsWith("/pureprofiler-resources/js"))
            {
                context.Response.ContentType = "text/javascript;charset=utf-8";
                var cssStream = GetType().Assembly.GetManifestResourceStream("Pure.Profiler.Web.Handlers.pureprofiler.js");
                using (var sr = new StreamReader(cssStream))
                {
                    await context.Response.WriteAsync(sr.ReadToEnd());
                }
                return;
            }

            //clear  ~/pureprofiler/view/clear
            if (path.EndsWith(ViewUrlClear, StringComparison.OrdinalIgnoreCase))
            {
                ProfilingSession.CircularBuffer.Clear();
                context.Response.Redirect(ViewUrl);
                return;
            }

           


            // view index of all latest results: ~/coreprofiler/view
            if (path.EndsWith(ViewUrl, StringComparison.OrdinalIgnoreCase)
                 )
            {
                // try to handle import/export first
                //var import = context.Request.Query[Import];
                //if (Uri.IsWellFormedUriString(import, UriKind.Absolute))
                //{
                //    await ImportSessionsFromUrl(import);
                //    return;
                //}

                //if (context.Request.QueryString.ToString() == Export)
                //{
                //    context.Response.ContentType = "application/json;charset=utf-8";
                //    await context.Response.WriteAsync(ImportSerializer.SerializeSessions(ProfilingSession.CircularBuffer));
                //    return;
                //}

                //var exportCorrelationId = context.Request.Query[CorrelationId];
                //if (!string.IsNullOrEmpty(exportCorrelationId))
                //{
                //    context.Response.ContentType = "application/json;charset=utf-8";
                //    var result = ProfilingSession.CircularBuffer.FirstOrDefault(
                //            r => r.Data != null && r.Data.ContainsKey(CorrelationId) && r.Data[CorrelationId] == exportCorrelationId);
                //    if (result != null)
                //    {
                //        await context.Response.WriteAsync(ImportSerializer.SerializeSessions(new[] { result }));
                //        return;
                //    }
                //}

                //-----------------------展示列表项目详情--------------------------


                // render result list view

                context.Response.ContentType = "text/html;charset=utf-8";

                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<title>页面资源加载时序</title>");
                sb.Append("<link rel=\"stylesheet\" href=\"./pureprofiler-resources/css\" />");
                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");
                sb.Append("<style>th {   text-align: left;background: #BD6840;color: #fff;font-size: 14px; } .gray { background-color: #eee; } .pure-profiler-error-row {  color: #f00 ; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
                sb.Append("</head");
                sb.Append("<body class='pureprofiler-pagebody' style=\"background:#EAF2FF;z-index:99999;\">");
                sb.Append(ViewResultIndexHeaderHtml);
                sb.Append("<a target='_self' href='" + ViewUrl + "'>Refresh</a>");
                sb.Append("&nbsp; <a target='_blank' href='export?exporttype=json'>Json</a>");
                sb.Append("&nbsp; <a target='_self' href='view/clear'>Clear</a>");
                sb.Append("&nbsp; <a target='_blank' href='stat-web'>WebStat</a>");
                sb.Append("&nbsp; <a target='_blank' href='stat-db'>DbStat</a>");
                sb.Append("&nbsp; <a target='_self' href=\"#\" onclick=\"return clickGlobal();\">Global</a>");

                //tab
                sb.Append("<p>");
                sb.Append("<div id=\"tabs1box\">");
                sb.Append("        <div class=\"menu1box\">");
                sb.Append("            <ul id=\"menu1\">");
                sb.Append("                <li class=\"hover\" onmouseover=\"setPureProfilerTab(1,0)\"><a href=\"#\">Configuration</a></li>");
                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 1)\"><a href=\"#\">Session/Cache</a></li>");
                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 2)\"><a href=\"#\">Environment</a></li>");
                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 3)\"><a href=\"#\">ServerVariables</a></li>");
                sb.Append("                <li onmouseover=\"setPureProfilerTab(1, 4)\"><a href=\"#\">PureProfiler</a></li>");
                sb.Append("            </ul>");
                sb.Append("        </div>");
                sb.Append("        <div class=\"tab-main1box\">");
                sb.Append("            <div class=\"tab-main\" id=\"tab-main1\">");
                sb.Append("                <ul class=\"block\"><li>" + ProfilingSession.AppConfigString + WebProfilingSessionContainer.WebConfigString + "</li></ul>");
                sb.Append("                <ul><li>" + WebProfilingSessionContainer.GetSessions(context) + ProfilingSession.NEWLINE + RuntimeCacheGetter.GetCaches(context) + "</li></ul>");
                sb.Append("                <ul><li>" + WebProfilingSessionContainer.EnvironmentString + "</li></ul>");
                sb.Append("                <ul><li>" + WebProfilingSessionContainer.GetServerVariables(context) + "</li></ul>");
                sb.Append("                <ul><li>" + ProfilingSession.ConfigurationString + "</li></ul>");
                sb.Append("            </div>");
                sb.Append("        </div>");
                sb.Append("    </div>");


                sb.Append("</p>");

                sb.Append("<table>");
                sb.Append("<tr><th class=\"nowrap\">发生时间</th><th class=\"nowrap\">耗时 (ms)</th><th class=\"nowrap\">Http Verb</th><th class=\"nowrap\">Is Ajax</th><th class=\"nowrap\">状态码</th><th>Url地址</th></tr>");
                var latestResults = ProfilingSession.CircularBuffer.OrderByDescending(r => r.Started);
                var i = 0;
                foreach (var result in latestResults)
                {
                    string statusCode = "";
                    string errorClass = "";
                    if (result.Data != null && result.Data.ContainsKey("ResponseStatusCode"))
                    {
                        statusCode = result.Data["ResponseStatusCode"];
                    }
                    if (statusCode != "" && statusCode != "200")
                    {
                        errorClass = "pure-profiler-error-row ";
                    }

                    sb.Append("<tr");
                    if ((i++) % 2 == 1)
                    {
                        sb.Append(" class=\"gray "+ errorClass + "\"");
                    }
                    else
                    {
                        sb.Append(" class=\"" + errorClass + "\"");
                    }
                    sb.Append("><td class=\"nowrap\">");
                    sb.Append(result.Started.ToString("yyyy-MM-dd HH:mm:ss.FFF"));
                    sb.Append("</td><td class=\"nowrap\">");
                    sb.Append(result.DurationMilliseconds);
                    sb.Append("</td><td class=\"nowrap\">");
                    string httpverb = "";
                    if (result.Data != null && result.Data.ContainsKey("Http Verb"))
                    {
                        httpverb = result.Data["Http Verb"];
                    }
                    sb.Append(httpverb);
                    sb.Append("</td><td class=\"nowrap\">");
                    string IsAjaxString = "";
                    if (result.Data != null && result.Data.ContainsKey("IsAjax"))
                    {
                        IsAjaxString = result.Data["IsAjax"];
                    }
                    sb.Append(IsAjaxString);
                    sb.Append("</td><td class=\"nowrap\">");
                    
                    sb.Append(statusCode);
                    sb.Append("</td><td><a href=\"view/");
                    sb.Append(result.Id.ToString());
                    sb.Append("\" target=\"_blank\">");
                    sb.Append(result.Name.Replace("\r\n", " "));
                    sb.Append("</a></td></tr>");
                }
                sb.Append("</table>");

                //author
                //sb.Append("<%--PureProfiler @ 郭建斌--%>");

                sb.Append("</body>");


                await context.Response.WriteAsync(sb.ToString());


                //context.Response.ContentType = "text/html;charset=utf-8";

                //var sb = new StringBuilder();
                //sb.Append("<head>");
                //sb.Append("<title>PureProfiler Latest Profiling Results</title>");
                //sb.Append("<style>th { width: 200px; text-align: left; } .gray { background-color: #eee; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
                //sb.Append("</head");
                //sb.Append("<body>");
                //sb.Append(ViewResultIndexHeaderHtml);

                //var tagFilter = context.Request.Query["tag"];
                //if (!string.IsNullOrWhiteSpace(tagFilter))
                //{
                //    sb.Append("<div><strong>Filtered by tag:</strong> ");
                //    sb.Append(tagFilter);
                //    sb.Append("<br/><br /></div>");
                //}

                //sb.Append("<table>");
                //sb.Append("<tr><th class=\"nowrap\">Time</th><th class=\"nowrap\">Duration (ms)</th><th>Url</th></tr>");
                //var latestResults = ProfilingSession.CircularBuffer.OrderByDescending(r => r.Started);
                //var i = 0;
                //foreach (var result in latestResults)
                //{
                //    if (!string.IsNullOrWhiteSpace(tagFilter) &&
                //        (result.Tags == null || !result.Tags.Contains<string>(tagFilter, StringComparer.OrdinalIgnoreCase)))
                //    {
                //        continue;
                //    }

                //    sb.Append("<tr");
                //    if ((i++) % 2 == 1)
                //    {
                //        sb.Append(" class=\"gray\"");
                //    }
                //    sb.Append("><td class=\"nowrap\">");
                //    sb.Append(result.Started.ToString("yyyy-MM-ddTHH:mm:ss.FFF"));
                //    sb.Append("</td><td class=\"nowrap\">");
                //    sb.Append(result.DurationMilliseconds);
                //    sb.Append("</td><td><a href=\"");
                //    sb.Append(baseViewPath);
                //    sb.Append("/");
                //    sb.Append(result.Id.ToString());
                //    sb.Append("\" target=\"_blank\">");
                //    sb.Append(result.Name.Replace("\r\n", " "));
                //    sb.Append("</a></td></tr>");
                //}
                //sb.Append("</table>");

                //sb.Append("</body>");

                //await context.Response.WriteAsync(sb.ToString());
                return;
            }


            ///内嵌的列表项目集合

            // view index of all latest results: ~/pureprofiler/view/include
            if (path.EndsWith(ViewUrlInclude, StringComparison.OrdinalIgnoreCase))
            {

                context.Response.ContentType = "text/javascript;charset=utf-8";
                //var curr = ProfilingSession.Current.Profiler.GetTimingSession();
                var latestResults = ProfilingSession.CircularBuffer.OrderByDescending(r => r.Started);


                var sb = new StringBuilder();


                var position = context.Request.Query["position"];
                string positionString = "right: 20px;top: 350px;";
                if (position == "left")
                {
                    positionString = "left: 20px;top: 20px;";
                }
                else if (position == "right")
                {
                    positionString = "right: 20px;top: 20px;";
                }
                else if (position == "bottomleft")
                {
                    positionString = "left: 20px;bottom: 20px;";
                }
                else if (position == "bottomright")
                {
                    positionString = "right: 20px;bottom: 20px;";
                }
                else if (position == "middleleft")
                {
                    positionString = "left: 20px;top: 350px;";
                }
                else if (position == "middleright")
                {
                    positionString = "right: 20px;top: 350px;";
                }

                var autoshow = context.Request.Query["autoshow"] == "1";
                if (autoshow == false)
                {
                    var pureprofiler_autoshow = context.Request.Cookies["pureprofiler_autoshow"] != null ? context.Request.Cookies["pureprofiler_autoshow"]  == "1" : false;
                    if (pureprofiler_autoshow)
                    {
                        autoshow = pureprofiler_autoshow;
                    }
                }

                string autoshowStrig = autoshow ? "" : ".pureprofiler{display:none;} ";

                var rooturl = context.Request.Query["rooturl"];
                string roolurlString = !string.IsNullOrEmpty(rooturl) ? rooturl.ToString().TrimEnd('/') : "";

                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");


                sb.Append("<style>" + autoshowStrig + ".pureprofiler{position:fixed;z-index: 99900;bottom:0;width:100%;height:300px;}  .pureprofiler-pin{height:32px;line-height:30px;padding:5px;position: fixed;" + positionString + "z-index: 99999; background: #008c5e;color:#fff;border-radius: 10px;box-shadow: 0 5px 10px rgba(0,0,0,.28);} .pureprofiler-refresh{color:#fff;background:#f00; padding: 5px;border-radius:10px;}  th { width: 200px; text-align: left; } .gray { background-color: #eee; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");


                sb.Append("<div class=\"pureprofiler-pin\" id=\"pureprofiler-pin\">");
                sb.Append("<span class=\"pureprofiler-refresh\"  id=\"pureprofiler-refresh\"> 刷新 </span>");
                sb.Append("<span class=\"pureprofiler-show\"  id=\"pureprofiler-show\">耗时：");

                sb.Append(CalcTime(latestResults) + " ms");
                sb.Append("</span>");
                sb.Append("</div>");


                sb.Append("<div class=\"pureprofiler\" id=\"pureprofiler\"><iframe src=" + ViewUrl + " name=\"iframepureprofiler\" id=\"iframepureprofiler\"  style=\"background:#EAF2FF;z-index:99999;\" width=\"100%\" height=\"300px\" marginwidth=\"0\" marginheight=\"0\" frameborder=\"0\" scrolling=\"auto\"  ></iframe>");
                sb.Append("</div>");



                //js


                sb.Append("<script>function getStyle(obj, attr) {if (obj.currentStyle) {return obj.currentStyle[attr];} else {return getComputedStyle(obj, false)[attr];}}</script>");


                sb.Append("<script> var divProfilerShow = document.getElementById(\"pureprofiler-show\");divProfilerShow.onclick = function () { var divProfile =document.getElementById(\"pureprofiler\"); if(getStyle(divProfile, \"display\") ==\"none\"){ divProfile.style.display=\"block\";setCookie(\"pureprofiler_autoshow\",\"1\",30);}else{divProfile.style.display=\"none\";setCookie(\"pureprofiler_autoshow\",\"0\",30);}}</script>");
                sb.Append("<script>var divProfilerRefresh = document.getElementById(\"pureprofiler-refresh\");divProfilerRefresh.onclick = function () { document.getElementById(\"iframepureprofiler\").src = \"" + ViewUrl + "\";}</script>");



                string html = "document.write('" + sb.ToString() + "')";


                await context.Response.WriteAsync(html);
               
                return;
            }



            // view specific result by uuid: ~/pureprofiler/view/{uuid}
            if (path.IndexOf(ViewUrl, StringComparison.OrdinalIgnoreCase) >= 0 )
            {
                context.Response.ContentType = "text/html;charset=utf-8";

                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<meta charset=\"utf-8\" />");
                sb.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
                sb.Append("<title>性能检测报告</title>");
                sb.Append("<link rel=\"stylesheet\" href=\"./pureprofiler-resources/css\" />");
                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");
                sb.Append("</head");
                sb.Append("<body>");
                sb.Append("<h1>性能检测报告</h1>");

                var uuid = path.Split('/').Last();
                var result = ProfilingSession.CircularBuffer.FirstOrDefault(
                        r => r.Id.ToString().ToLowerInvariant() == uuid.ToLowerInvariant());
                if (result != null)
                {
                    if (TryToImportDrillDownResult)
                    {
                        // try to import drill down results
                        foreach (var timing in result.Timings)
                        {
                            if (timing.Data == null || !timing.Data.ContainsKey(CorrelationId)) continue;
                            Guid parentResultId;
                            if (!Guid.TryParse(timing.Data[CorrelationId], out parentResultId)
                                || ProfilingSession.CircularBuffer.Any(r => r.Id == parentResultId)) continue;

                            string remoteAddress;
                            if (!timing.Data.TryGetValue("remoteAddress", out remoteAddress))
                                remoteAddress = timing.Name;

                            if (!Uri.IsWellFormedUriString(remoteAddress, UriKind.Absolute)) continue;

                            if (!remoteAddress.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;

                            var pos = remoteAddress.IndexOf("?");
                            if (pos > 0) remoteAddress = remoteAddress.Substring(0, pos);
                            if (remoteAddress.Split('/').Last().Contains(".")) remoteAddress = remoteAddress.Substring(0, remoteAddress.LastIndexOf("/"));

                            try
                            {
                                await ImportSessionsFromUrl(remoteAddress + "/pureprofiler/view?" + CorrelationId + "=" + parentResultId.ToString("N"));
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.Write(ex.Message);

                                //ignore exceptions
                            }
                        }
                    }
                    sb.Append("<div class=\"css-treeview\">");

                    // print summary
                    sb.Append("<ul>");
                    sb.Append("<li class=\"summary\">");
                    PrintDrillUpLink(sb, result, baseViewPath);
                    sb.Append(result.Name.Replace("\r\n", " "));
                    sb.Append("<p class='exportbar'>");
                    sb.Append("<a target='_blank' href='../export?exporttype=json&id=" + uuid + "'>Json</a>");
                    sb.Append("</p>");

                    sb.Append("</li>");
                    sb.Append("<li class=\"summary\">");
                    if (result.Data != null)
                    {
                        foreach (var keyValue in result.Data)
                        {
                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;
                            if ((keyValue.Key) == "RequestBody") continue;
                            if ((keyValue.Key) == "ResponseBody") continue;
                            if ((keyValue.Key) == "ResponseBody2") continue;

                            sb.Append("<b>");
                            sb.Append(keyValue.Key);
                            sb.Append(": </b>");
                            var encodedValue = WebUtility.HtmlEncode(keyValue.Value);
                            if (keyValue.Key.EndsWith("Count") || keyValue.Key.EndsWith("Duration"))
                            {
                                sb.Append("<span class=\"");
                                sb.Append(keyValue.Key);
                                sb.Append("\">");
                                sb.Append(encodedValue);
                                sb.Append("</span>");
                            }
                            else
                            {
                                sb.Append(encodedValue);
                            }
                            sb.Append(" &nbsp; ");
                        }


                    }
                    sb.Append("<b>所在机器: </b>");
                    sb.Append(result.MachineName);
                    sb.Append(" &nbsp; ");
                    if (result.Tags != null && result.Tags.Any())
                    {
                        sb.Append("<b>标签: </b>");
                        sb.Append(string.Join(", ", result.Tags.Select(t => string.Format("<a href=\"{2}?tag={0}\">{1}</a>", HttpUtility.UrlEncode(t), t, baseViewPath))));
                        sb.Append(" &nbsp; ");
                    }
                    sb.Append("</li>");
                    sb.Append("</ul>");

                    if (result.Data != null)
                    {
                        //Web请求内容
                        if (result.Data.ContainsKey("RequestBody"))
                        {
                            sb.Append(
                            "<br><a title=\"RequestBody\" onclick='clickRequestBody()'>RequestBody</a>");
                            var encodedValue = (result.Data["RequestBody"]);
                            sb.Append("<p class=\"pureprofiler-RequestBody\" id=\"pureprofiler-RequestBody\">");
                            sb.Append(encodedValue);
                            sb.Append("</p> ");
                        }

                        //Web响应内容
                        if (result.Data.ContainsKey("ResponseBody"))
                        {
                            sb.Append(
                            "<br><a title=\"ResponseBody\" onclick='clickResponseBody()'>ResponseBody</a>");
                            var encodedValue = result.Data["ResponseBody"];//
                            if (result.Data.ContainsKey("ResponseBody2"))
                            {
                                encodedValue += "<b>Body</b><br>" + HttpUtility.HtmlEncode(result.Data["ResponseBody2"]);
                            }

                            sb.Append("<p class=\"pureprofiler-ResponseBody\" id=\"pureprofiler-ResponseBody\">");
                            sb.Append(encodedValue);
                            sb.Append("</p> ");

                        }
                    }

                    var totalLength = result.DurationMilliseconds;
                    if (totalLength == 0)
                    {
                        totalLength = 1;
                    }
                    var factor = 300.0 / totalLength;

                    // print ruler
                    sb.Append("<ul>");
                    sb.Append("<li class=\"ruler\"><span style=\"width:300px\">0</span><span style=\"width:80px\">");
                    sb.Append(totalLength);
                    sb.Append(
                        " (ms)</span><span style=\"width:20px\">&nbsp;</span><span style=\"width:60px\">开始</span><span style=\"width:60px\">耗时(ms)</span><span style=\"width:20px\">&nbsp;</span><span>执行时序</span></li>");
                    sb.Append("</ul>");

                    // print timings
                    sb.Append("<ul class=\"timing\">");
                    PrintTimings(result, result.Id, sb, factor, baseViewPath);
                    sb.Append("");
                    sb.Append("</ul>");
                    sb.Append("</div>");

                    // print timing data popups
                    foreach (var timing in result.Timings)
                    {
                        if (timing.Data == null || !timing.Data.Any()) continue;

                        sb.Append("<aside id=\"data_");
                        sb.Append(timing.Id.ToString());
                        sb.Append("\" style=\"display:none\" class=\"modal\">");
                        sb.Append("<div>");
                        sb.Append("<h4><code>");
                        sb.Append(timing.Name.Replace("\r\n", " "));
                        sb.Append("</code></h4>");
                        sb.Append("<textarea readonly=\"readonly\">");
                        foreach (var keyValue in timing.Data)
                        {
                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;

                            sb.Append(keyValue.Key);
                            sb.Append(":\r\n");
                            var value = keyValue.Value.Trim();
                            if (value.StartsWith("<"))
                            {
                                // asuume it is XML
                                // try to format XML with indent
                                var doc = new XmlDocument();
                                try
                                {
                                    doc.LoadXml(value);
                                    var ms = new MemoryStream();
                                    var xwSettings = new XmlWriterSettings
                                    {
                                        Encoding = new UTF8Encoding(false),
                                        Indent = true,
                                        IndentChars = "\t"
                                    };
                                    using (var writer = XmlWriter.Create(ms, xwSettings))
                                    {
                                        doc.Save(writer);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        using (var sr = new StreamReader(ms))
                                        {
                                            value = sr.ReadToEnd();
                                        }
                                    }

                                }
                                catch
                                {
                                    //squash exception
                                }
                            }
                            sb.Append(value);
                            sb.Append("\r\n\r\n");
                        }
                        if (timing.Tags != null && timing.Tags.Any())
                        {
                            sb.Append("tags:\r\n");
                            sb.Append(timing.Tags);
                            sb.Append("\r\n");
                        }
                        sb.Append("</textarea>");
                        sb.Append(
                            "<a href=\"#close\" title=\"Close\" onclick=\"this.parentNode.parentNode.style.display='none'\">关闭</a>");
                        sb.Append("</div>");
                        sb.Append("</aside>");
                    }
                }
                else
                {
                    sb.Append("你所访问的结果报告不存在。");
                }
                sb.Append("</body>");

                await context.Response.WriteAsync(sb.ToString());

                return;
            }


            //导出数据
            if (path.IndexOf(ExportUrl, StringComparison.OrdinalIgnoreCase) >= 0  )
            {
                var import = context.Request.Query[Import];
                if (Uri.IsWellFormedUriString(import, UriKind.Absolute))
                {
                    await ImportSessionsFromUrl(import);
                    return;
                }

                if (context.Request.Query[ExportType] == (ExportJson)  )
                {
                    context.Response.ContentType = "application/json;charset=utf-8";
                    await context.Response.WriteAsync(ImportSerializer.SerializeSessions(ProfilingSession.CircularBuffer));
                    return;
                }

                var exportCorrelationId = context.Request.Query[CorrelationId];
                if (!string.IsNullOrEmpty(exportCorrelationId))
                {
                    context.Response.ContentType = "application/json;charset=utf-8";
                    var result = ProfilingSession.CircularBuffer.FirstOrDefault(
                            r => r.Data != null && r.Data.ContainsKey(CorrelationId) && r.Data[CorrelationId] == exportCorrelationId);
                    if (result != null)
                    {
                        if (context.Request.Query[ExportType] == (ExportJson))
                        {
                            context.Response.ContentType = "application/json;charset=utf-8";
                            await context.Response.WriteAsync(ImportSerializer.SerializeSessions(new[] { result }));
                            return;
                        }

                     
                    }
                }

            }
            else if (TryToImportDrillDownResult && path.IndexOf(ExportUrl, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var uuid = path.Split('/').Last();
                var result = ProfilingSession.CircularBuffer.FirstOrDefault(
                        r => r.Id.ToString().ToLowerInvariant() == uuid.ToLowerInvariant());
                if (result != null)
                {

                    foreach (var timing in result.Timings)
                    {
                        if (timing.Data == null || !timing.Data.ContainsKey(CorrelationId)) continue;
                        Guid correlationId2;
                        if (!Guid.TryParse(timing.Data[CorrelationId], out correlationId2)
                            || ProfilingSession.CircularBuffer.Any(r => r.Id == correlationId2)) continue;

                        string remoteAddress;
                        if (!timing.Data.TryGetValue("remoteAddress", out remoteAddress))
                            remoteAddress = timing.Name;

                        if (!Uri.IsWellFormedUriString(remoteAddress, UriKind.Absolute)) continue;

                        if (!remoteAddress.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;

                        var pos = remoteAddress.IndexOf("?");
                        if (pos > 0) remoteAddress = remoteAddress.Substring(0, pos);
                        if (remoteAddress.Split('/').Last().Contains(".")) remoteAddress = remoteAddress.Substring(0, remoteAddress.LastIndexOf("/"));

                        try
                        {
                            await ImportSessionsFromUrl(remoteAddress + "/pureprofiler/view?" + CorrelationId + "=" + correlationId.ToString());
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.Write(ex.Message);

                            //ignore exceptions
                        }

                        return;
                    }
                }
            }



            try
            {
                await _next.Invoke(context);
            }
            catch (System.Exception ex)
            {
                // stop and save profiling results on error
                //using (ProfilingSession.Current.Step("Stop on Error")) { }

                bool showError = ProfilingSession.Configuration.PureProfilerConfiguration.ShowError;
                if (showError)
                {
                    string newLine = "-----" + System.Environment.NewLine;// "</br>";

                    // stop and save profiling results on error
                    string err = "";
                 
                        Exception objErr = ex ;
                        err = "Error at:      " + context.Request.GetDisplayUrl()  + newLine +
                        "Error Message:      " + objErr.Message.ToString() + newLine +
                        "Error Source:      " + objErr.Source.ToString() + newLine +
                        "Stack Trace:      " + objErr.StackTrace.ToString() + newLine;

                        if (objErr.InnerException != null)
                        {
                            err += "InnerException:      " + newLine +
                        "InnerError Message:      " + objErr.Message.ToString() + newLine +
                        "InnerError Source:      " + objErr.Source.ToString() + newLine +
                        "InnerStack Trace:      " + objErr.StackTrace.ToString() + newLine;
                        }

                   
                    using (ProfilingSession.Current.Step(() => (err), ProfilingSession.FailOnErrorMark))
                    {

                    }
                }
                else
                {
                    using (ProfilingSession.Current.Step("Stop on Error", ProfilingSession.FailOnErrorMark))
                    {



                    }
                }



                throw;
            }
            finally
            {
                if (ProfilingSession.ProfilingSessionContainer is WebProfilingSessionContainer)
                {

                    WebProfilingSessionContainer container = ProfilingSession.ProfilingSessionContainer as WebProfilingSessionContainer;
                    if (container != null)
                    {
                        container.SetResponseContent();
                    }

                }

                ProfilingSession.Stop();
            }
        }

        #region Private Methods

        private void PrintTimings(ITimingSession session, Guid parentId, StringBuilder sb, double factor, string baseViewPath)
        {
            var timings = session.Timings.Where(s => s.ParentId == parentId);
            foreach (var timing in timings)
            {
                PrintTiming(session, timing, sb, factor, baseViewPath);
            }
        }

        private void PrintTiming(ITimingSession session, ITiming timing, StringBuilder sb, double factor, string baseViewPath)
        {
            sb.Append("<li><span class=\"timing\" style=\"padding-left: ");
            var start = Math.Floor(timing.StartMilliseconds * factor);
            if (start > 300)
            {
                start = 300;
            }
            sb.Append(start);
            sb.Append("px\"><span class=\"bar ");
            sb.Append(timing.Type);
            sb.Append("\" title=\"");
            sb.Append(WebUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
            sb.Append("\" style=\"width: ");
            var width = (int)Math.Round(timing.DurationMilliseconds * factor);
            if (width > 300)
            {
                width = 300;
            }
            else if (width == 0)
            {
                width = 1;
            }
            sb.Append(width);
            sb.Append("px\"></span><span class=\"start\">+");
            sb.Append(timing.StartMilliseconds);
            sb.Append("</span><span class=\"duration\">");
            sb.Append(timing.DurationMilliseconds);
            sb.Append("</span></span>");
            var hasChildTimings = session.Timings.Any(s => s.ParentId == timing.Id);
            if (hasChildTimings)
            {
                sb.Append("<input type=\"checkbox\" id=\"t_");
                sb.Append(timing.Id.ToString());
                sb.Append("\" checked=\"checked\" /><label for=\"t_");
                sb.Append(timing.Id.ToString());
                sb.Append("\">");
                PrintDataLink(sb, timing);
                PrintDrillDownLink(sb, timing, baseViewPath);
                sb.Append(WebUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
                sb.Append("</label>");
                sb.Append("<ul>");
                PrintTimings(session, timing.Id, sb, factor, baseViewPath);
                sb.Append("</ul>");
            }
            else
            {
                sb.Append("<span class=\"leaf\">");
                PrintDataLink(sb, timing);
                PrintDrillDownLink(sb, timing, baseViewPath);
                sb.Append(WebUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
                sb.Append("</span>");
            }
            sb.Append("</li>");
        }

        private void PrintDataLink(StringBuilder sb, ITiming timing)
        {
            if (timing.Data == null || !timing.Data.Any()) return;

            sb.Append("[<a href=\"#data_");
            sb.Append(timing.Id.ToString());
            sb.Append("\" onclick=\"document.getElementById('data_");
            sb.Append(timing.Id.ToString());
            sb.Append("').style.display='block';\" class=\"openModal\">data</a>] ");
        }

        private void PrintDrillDownLink(StringBuilder sb, ITiming timing, string baseViewPath)
        {
            if (timing.Data == null || !timing.Data.ContainsKey("correlationId")) return;

            var correlationId = timing.Data["correlationId"];

            Guid? drillDownSessionId = null;
            if (DrillDownHandler == null)
            {
                var drillDownSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Data != null && s.Data.ContainsKey("correlationId") && s.Data["correlationId"] == correlationId);
                if (drillDownSession != null) drillDownSessionId = drillDownSession.Id;
            }
            else
            {
                drillDownSessionId = DrillDownHandler(correlationId);
            }

            if (!drillDownSessionId.HasValue) return;

            sb.Append("[<a href=\"");
            sb.Append(baseViewPath);
            sb.Append("/");
            sb.Append(drillDownSessionId);
            sb.Append("\">drill down</a>] ");
        }

        private void PrintDrillUpLink(StringBuilder sb, ITimingSession session, string baseViewPath)
        {
            if (session.Data == null || !session.Data.ContainsKey("correlationId")) return;

            var correlationId = session.Data["correlationId"];

            Guid? drillUpSessionId = null;
            if (DrillUpHandler == null)
            {
                var drillUpSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Timings != null && s.Timings.Any(t => t.Data != null && t.Data.ContainsKey("correlationId") && t.Data["correlationId"] == correlationId));
                if (drillUpSession != null) drillUpSessionId = drillUpSession.Id;
            }
            else
            {
                drillUpSessionId = DrillUpHandler(correlationId);
            }

            if (!drillUpSessionId.HasValue) return;

            sb.Append("[<a href=\"");
            sb.Append(baseViewPath);
            sb.Append("/");
            sb.Append(drillUpSessionId);
            sb.Append("\">drill up</a>] ");
        }

        private static void ClearIfCurrentProfilingSessionStopped()
        {
            var profilingSession = ProfilingSession.Current;
            if (profilingSession == null)
            {
                return;
            }

            if (profilingSession.Profiler.IsStopped)
            {
                ProfilingSession.ProfilingSessionContainer.Clear();
            }
        }

        private string GetCorrelationIdFromHeaders(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains(XCorrelationId))
            {
                var correlationIds = context.Request.Headers.GetCommaSeparatedValues(XCorrelationId);
                if (correlationIds != null)
                {
                    return correlationIds.FirstOrDefault();
                }
            }

            return null;
        }

        private async Task ImportSessionsFromUrl(string importUrl)
        {
            IEnumerable<ITimingSession> sessions = null;

            using (var httpClient = new HttpClient())
            {
                
                var response = await httpClient.GetAsync(importUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    sessions = ImportSerializer.DeserializeSessions(content);
                }
            }

            if (sessions == null)
            {
                return;
            }

            if (ProfilingSession.CircularBuffer == null)
            {
                return;
            }

            var existingIds = ProfilingSession.CircularBuffer.Select(session => session.Id).ToList();
            foreach (var session in sessions)
            {
                if (!existingIds.Contains(session.Id))
                {
                    ProfilingSession.CircularBuffer.Add(session);
                }
            }
        }

        #endregion



        private string CalcTime(IOrderedEnumerable<ITimingSession> latestResults)
        {
            var total = latestResults.FirstOrDefault().DurationMilliseconds;
            //var total = latestResults.Sum(p => p.DurationMilliseconds);
            return total.ToString();
            
        }

    }
}
