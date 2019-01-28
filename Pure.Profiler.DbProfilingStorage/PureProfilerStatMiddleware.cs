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
using System.Data;
using Pure.Profiler.Web;
using Pure.Profiler.ProfilingFilters;
using Microsoft.AspNetCore.Builder;
using Pure.Profiler.DbProfilingStorage;

namespace Pure.Profiler.Web
{

    public static class PureProfilerMiddlewareExtensions
    { 
        public static IApplicationBuilder UsePureProfilerStat(this IApplicationBuilder builder, bool drillDown = false)
        {
            PureProfilerMiddleware.TryToImportDrillDownResult = drillDown;
             
            // set WebProfilingSessionContainer as the default profiling session container
            // if the current one is CallContextProfilingSessionContainer
            //if (ProfilingSession.ProfilingSessionContainer.GetType() == typeof(CallContextProfilingSessionContainer))
            //{
            //    var host = builder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            //    var httpContextAccessor = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            //    ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer(httpContextAccessor, host);
            //}

            // register pureProfilerModule
            //DynamicModuleUtility.RegisterModule(typeof(PureProfilerModule));
            // register PureProfilerImportModule
            // DynamicModuleUtility.RegisterModule(typeof(PureProfilerImportModule));
            // ignore pureprofiler view-result requests from profiling
            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler/stat-web"));
            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler/stat-db"));
            //ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler/export"));
            //ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler-resources"));

            return builder.UseMiddleware<PureProfilerStatMiddleware>();
        }
    }

    public class PureProfilerStatMiddleware
    {
        private readonly RequestDelegate _next;


        private const string Auth_CookieName = ConfigurationHelper.AuthCookieName;
        private const string LoginUrl = ConfigurationHelper.LoginUrl;//"/pureprofiler/login";

        //private const string ViewUrl = "/pureprofiler/view";
        private const string ViewStatWebUrl = "/pureprofiler/stat-web";
        private const string ViewStatDbUrl = "/pureprofiler/stat-db";
        //private const string ViewUrlClear = "/pureprofiler/view/clear";
        //private const string ViewUrlInclude = "/pureprofiler/view/include";
        //private const string ExportUrl = "/pureprofiler/export";

        //private const string Import = "import";
        //private const string ExportJson = "json";
        //private const string ExportType = "exporttype"; 

        //private const string Import = "import";
        //private const string Export = "?export";
        //private const string CorrelationId = "correlationId";
        /// <summary>
        /// The default Html of the view-result index page: ~/pureprofiler/view
        /// </summary>
        public static string ViewResultIndexHeaderHtml = "<h1>Pure Profiler-Web URI Stat监控</h1>";
        public static string DbViewResultIndexHeaderHtml = "<h1>Pure Profiler-SQL Stat监控</h1>";
        
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

        public PureProfilerStatMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        private void SetAuthSession(HttpContext context, string sessionStr)
        {
            context.Session.SetString(Auth_CookieName, sessionStr);
        }

        private string GetAuthSession(HttpContext context)
        {
            return context.Session.GetString(Auth_CookieName);
        }
        private bool Authorize(HttpContext context, string sessionStr = "")
        {
            var PureProfilerConfigurationSection = ConfigurationHelper.LoadPureProfilerConfigurationSection();
            if (PureProfilerConfigurationSection.EnableAuth == true)
            {
                string cookieValue = "";
                if (!string.IsNullOrEmpty(sessionStr))
                {
                    cookieValue = sessionStr;
                }
                else
                {
                    var cookie = GetAuthSession(context);
                    cookieValue = cookie != null ? cookie.ToString() : "";
                }

                if (string.IsNullOrEmpty(cookieValue))
                {
                    context.Response.Redirect(LoginUrl, true);
                    return false;
                }
                else
                {
                    string enAuth = PureProfilerConfigurationSection.EncryptAuthAccount;
                    if (cookieValue == enAuth)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            else
            {
                return true;
            }


        }

        private void SetNoCacheHeader(HttpContext context)
        {
            context.Response.Headers.Add("Expires", "0");
            context.Response.Headers.Add("Pragma", "no-cache");
            context.Response.Headers.Add("Cache-control", "no-cache");
            context.Response.Headers.Add("Cache", "no-cache");
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
            //if (ProfilingSession.CircularBuffer == null)
            //{
            //    await _next.Invoke(context);
            //    return;
            //}

            //ClearIfCurrentProfilingSessionStopped();

            var url = UriHelper.GetDisplayUrl(context.Request);
            //ProfilingSession.Start(url);

            // set correlationId if exists in header
            //var correlationId = GetCorrelationIdFromHeaders(context);
            //if (!string.IsNullOrWhiteSpace(correlationId))
            //{
            //    ProfilingSession.Current.AddField("correlationId", correlationId);
            //}

           

            var path = context.Request.Path.ToString().TrimEnd('/');

            //// generate baseViewPath
            //string baseViewPath = null;
            //var posStart = path.IndexOf(ViewUrl, StringComparison.OrdinalIgnoreCase);
            ////if (posStart < 0)
            ////    posStart = path.IndexOf(ViewUrlNano, StringComparison.OrdinalIgnoreCase);
            //if (posStart >= 0)
            //    baseViewPath = path.Substring(0, posStart) + ViewUrl;

            //// prepend pathbase if specified
            //baseViewPath = context.Request.PathBase + baseViewPath;

           

            ////clear  ~/pureprofiler/view/clear
            //if (path.EndsWith(ViewUrlClear, StringComparison.OrdinalIgnoreCase))
            //{
            //    ProfilingSession.CircularBuffer.Clear();
            //    context.Response.Redirect(ViewUrl);
            //    return;
            //}

           


            // view index of all latest results: ~/coreprofiler/view
            if (path.EndsWith(ViewStatWebUrl, StringComparison.OrdinalIgnoreCase)
                 )
            {
                SetNoCacheHeader(context);
                if (Authorize(context) == false)
                {
                    return;
                }
                // render result list view

                context.Response.ContentType = "text/html;charset=utf-8";

                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<title>Web URI监控</title>");
                sb.Append("<link rel=\"stylesheet\" href=\"./pureprofiler-resources/css\" />");
                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");
                sb.Append("<style>table a{text-decoration: none !important;} th {   text-align: left;background: #BD6840;color: #fff;font-size: 14px; } .gray { background-color: #eee; } .pure-profiler-error-row {  color: #f00 ; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
                sb.Append("</head");
                sb.Append("<body class='pureprofiler-pagebody' style=\"background:#EAF2FF;z-index:99999;\">");
                sb.Append(ViewResultIndexHeaderHtml);
                sb.Append("<a target='_self' href='" + ViewStatWebUrl + "'>Refresh</a>");
                //sb.Append("&nbsp; <a target='_blank' href='export?exporttype=json'>Json</a>");
                //sb.Append("&nbsp; <a target='_self' href='view/clear'>Clear</a>");
                //sb.Append("&nbsp; <a target='_self' href=\"#\" onclick=\"return clickGlobal();\">Global</a>");

               

                sb.Append("<table>");
                sb.Append("<tr><th class=\"nowrap\">序号</th>" +
                    "<th class=\"nowrap\">HttpVerb</th>" +
                    "<th class=\"nowrap\">URI</th>" +
                    "<th class=\"nowrap\">请求次数</th>" +
                    "<th class=\"nowrap\">请求时间</th>" +
                    "<th class=\"nowrap\">平均请求时间</th>" +
                    "<th class=\"nowrap\">最大请求时间</th>" +
                    "<th class=\"nowrap\">Db执行数</th>" +
                    "<th class=\"nowrap\">Db执行时间</th>" +
                    "<th class=\"nowrap\">错误数</th>" +
                    "<th class=\"nowrap\">区间分布</th></tr>");

                var DtStatWebData = StatSqlCollections.GetStatWeb();
                if (DtStatWebData != null)
                {
                    int i = 0;
                    int index = 1; string sqlStr = "", sqlStrTmp = "";
                    //foreach (DataRow myRow in DtStatWebData.Rows)
                    foreach (var myRow in DtStatWebData)
                    {
                        sb.Append("<tr");
                        if ((i++) % 2 == 1)
                        {
                            sb.Append(" class=\"gray\"");
                        }
                        else
                        {
                            sb.Append(" class=\"\"");
                        }
                        sb.Append("><td class=\"nowrap\" style='text-align:center'>");
                        sb.Append(index.ToString());
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "HttpVerb"));
                        sb.Append(myRow.HttpVerb);
                        sb.Append("</td><td class=\"\" style='word-wrap:break-word;word-break:break-all;max-width:600px;table-layout:fixed;padding-right: 20px;' >");

 
                        //sqlStr = StatSqlCollections.GetDataRowValue(myRow, "URI");
                        sqlStr = myRow.URI;
                        //if (sqlStr != null)
                        //{
                        //    sqlStr = sqlStr.ToLower();
                        //}
                        //if (sqlStr != null && sqlStr.Length > 58)
                        //{
                        //    sqlStrTmp = sqlStr.Substring(0, 58) + "... ";
                        //}
                        //else
                        //{
                        sqlStrTmp = sqlStr;
                        //}

                        sb.Append("<a title='" + sqlStr + "' href='javascript:void(0)'>" + sqlStrTmp + "</a>");

                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "URI"));
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "RequestCount"));
                        sb.Append(myRow.RequestCount);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "SumDurationMilliseconds"));
                        sb.Append(myRow.SumDurationMilliseconds);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "AvgDurationMilliseconds"));
                        sb.Append(myRow.AvgDurationMilliseconds);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "MaxDurationMilliseconds"));
                        sb.Append(myRow.MaxDurationMilliseconds);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "SumDbCount"));
                        sb.Append(myRow.SumDbCount);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "SumDbDurationMilliseconds"));
                        sb.Append(myRow.SumDbDurationMilliseconds);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "ErrorCount"));
                        sb.Append(myRow.ErrorCount);

                        sb.Append("</td><td class=\"nowrap\">");
                        string qujian = "[" + myRow.DurationMilliseconds1
                         + "," + myRow.DurationMilliseconds2
                         + "," + myRow.DurationMilliseconds3
                         + "," + myRow.DurationMilliseconds4
                         + "," + myRow.DurationMilliseconds5
                         + "," + myRow.DurationMilliseconds6
                         + "," + myRow.DurationMilliseconds7
                         + "]";

                        //string qujian = "[" + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds1")
                        //    +"," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds2")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds3")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds4")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds5")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds6")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds7")
                        //    + "]";
                        sb.Append(qujian);

                        sb.Append("</td></tr>");
                        
                        index++;
                    }
                }



                sb.Append("</table>");

                //author
                //sb.Append("<%--PureProfiler @ 郭建斌--%>");

                sb.Append("</body>");


                await context.Response.WriteAsync(sb.ToString());

 
                return;
            }
            else if (path.EndsWith(ViewStatDbUrl, StringComparison.OrdinalIgnoreCase)
                )
            {
                SetNoCacheHeader(context);
                if (Authorize(context) == false)
                {
                    return;
                }
                // render result list view

                context.Response.ContentType = "text/html;charset=utf-8";

                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<title>SQL Stat监控</title>");
                sb.Append("<link rel=\"stylesheet\" href=\"./pureprofiler-resources/css\" />");
                sb.Append("<script type=\"text/javascript\" src=\"./pureprofiler-resources/js\"></script>");
                sb.Append("<style>table a{text-decoration: none !important;} th {   text-align: left;background: #BD6840;color: #fff;font-size: 14px; } .gray { background-color: #eee; } .pure-profiler-error-row {  color: #f00 ; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
                sb.Append("</head");
                sb.Append("<body class='pureprofiler-pagebody' style=\"background:#EAF2FF;z-index:99999;\">");
                sb.Append(DbViewResultIndexHeaderHtml);
                sb.Append("<a target='_self' href='" + ViewStatDbUrl + "'>Refresh</a>");
                //sb.Append("&nbsp; <a target='_blank' href='export?exporttype=json'>Json</a>");
                //sb.Append("&nbsp; <a target='_self' href='view/clear'>Clear</a>");
                //sb.Append("&nbsp; <a target='_self' href=\"#\" onclick=\"return clickGlobal();\">Global</a>");



                sb.Append("<table>");
                sb.Append("<tr><th class=\"nowrap\">序号</th>" +
                    "<th class=\"nowrap\">SQL</th>" +
                    "<th class=\"nowrap\">类型</th>" +
                    "<th class=\"nowrap\">执行数</th>" +
                    "<th class=\"nowrap\">执行时间</th>" +
                    "<th class=\"nowrap\">平均执行时间</th>" +
                    "<th class=\"nowrap\">最慢</th>" +
                    "<th class=\"nowrap\">影响行数</th>" +
                    "<th class=\"nowrap\">错误数</th>" +
                    "<th class=\"nowrap\">区间分布</th></tr>");

                var DtStatWebData = StatSqlCollections.GetStatDb();
                if (DtStatWebData != null)
                {
                    
                    int i = 0;
                    int index = 1;
                    string sqlStr = "", sqlStrTmp = "" ;
                    //foreach (DataRow myRow in DtStatWebData.Rows)
                    foreach (var myRow in DtStatWebData)
                        {
                        sb.Append("<tr");
                        if ((i++) % 2 == 1)
                        {
                            sb.Append(" class=\"gray\"");
                        }
                        else
                        {
                            sb.Append(" class=\"\"");
                        }
                        sb.Append("><td class=\"nowrap\" style='text-align:center'>");
                        sb.Append(index.ToString());
                        sb.Append("</td><td class=\"\" style='word-wrap:break-word;word-break:break-all;max-width:600px;table-layout:fixed;padding-right: 20px;' >");

                        //sqlStr = StatSqlCollections.GetDataRowValue(myRow, "SQLStr");
                        sqlStr = myRow.SQLStr;

                        if (sqlStr != null)
                        {
                            sqlStr = sqlStr.ToLower();
                        }
                        if (sqlStr != null && sqlStr.Length > 58)
                        {
                            sqlStrTmp = sqlStr.Substring(0, 58) + "... ";
                        }
                        else
                        {
                            sqlStrTmp = sqlStr;
                        }

                        sb.Append( "<a title='"+ sqlStr + "' href='javascript:void(0)'>"+ sqlStrTmp + "</a>");

                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "SQLStr"));
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "ExecuteType"));
                        sb.Append(myRow.ExecuteType);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "RequestCount"));
                        sb.Append(myRow.RequestCount);

                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "SumDurationMilliseconds"));
                        sb.Append(myRow.SumDurationMilliseconds);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "AvgDurationMilliseconds"));
                        sb.Append(myRow.AvgDurationMilliseconds);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "MaxDurationMilliseconds"));
                        sb.Append(myRow.MaxDurationMilliseconds);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "SumDExecuteResult"));
                        sb.Append(myRow.SumDExecuteResult);
                        sb.Append("</td><td class=\"nowrap\">");
                        //sb.Append(StatSqlCollections.GetDataRowValue(myRow, "ErrorCount"));
                        sb.Append(myRow.ErrorCount);

                        sb.Append("</td><td class=\"nowrap\">");
                        //string qujian = "[" + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds1")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds2")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds3")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds4")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds5")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds6")
                        //    + "," + StatSqlCollections.GetDataRowValue(myRow, "DurationMilliseconds7")
                        //    + "]";
                        string qujian = "[" + myRow.DurationMilliseconds1
                           + "," + myRow.DurationMilliseconds2
                           + "," + myRow.DurationMilliseconds3
                           + "," + myRow.DurationMilliseconds4
                           + "," + myRow.DurationMilliseconds5
                           + "," + myRow.DurationMilliseconds6
                           + "," + myRow.DurationMilliseconds7
                           + "]";
                        sb.Append(qujian);

                        sb.Append("</td></tr>");
                        
                        index++;
                    }
                }



                sb.Append("</table>");

                //author
                //sb.Append("<%--PureProfiler @ 郭建斌--%>");

                sb.Append("</body>");


                await context.Response.WriteAsync(sb.ToString());


                return;
            }


            try
            {
                await _next.Invoke(context);
            }
            catch (System.Exception ex)
            { 
                throw ex;
            }
            //finally
            //{
            //    if (ProfilingSession.ProfilingSessionContainer is WebProfilingSessionContainer)
            //    {

            //        WebProfilingSessionContainer container = ProfilingSession.ProfilingSessionContainer as WebProfilingSessionContainer;
            //        if (container != null)
            //        {
            //            container.SetResponseContent();
            //        }

            //    }

            //    ProfilingSession.Stop();
            //}


        }
          
    }
}
