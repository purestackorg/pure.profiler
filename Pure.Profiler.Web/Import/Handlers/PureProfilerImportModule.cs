
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Web;
//using Pure.Profiler.Timings;

//namespace Pure.Profiler.Web.Import.Handlers
//{
//    /// <summary>
//    /// A module supports import and export profiling sessions.
//    /// </summary>
//    public sealed class PureProfilerImportModule : IHttpModule
//    {
//        private const string ViewUrl = "/pureprofiler/export"; 
//        private const string Import = "import";
//        private const string ExportJson = "json";
//        private const string ExportType = "exporttype";
//        private const string QueryString = "QUERY_STRING";
//        private const string CorrelationId = "Id";

//        /// <summary>
//        /// Tries to import drilldown result by remote address of the step
//        /// </summary>
//        public static bool TryToImportDrillDownResult;

//        #region Public Methods

//        /// <summary>
//        /// 
//        /// </summary>
//        public void Dispose()
//        {
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="application"></param>
//        public void Init(HttpApplication application)
//        {
//            application.BeginRequest += ApplicationOnBeginRequest;
//        }

//        #endregion

//        #region Private Methods


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

//            var path = context.Request.Path.TrimEnd('/');

//            //if (path.EndsWith(ViewUrl, StringComparison.OrdinalIgnoreCase)
//            //    || path.EndsWith(ViewUrlCore, StringComparison.OrdinalIgnoreCase))
//            if (path.EndsWith(ViewUrl, StringComparison.OrdinalIgnoreCase))
//            {
//                var import = context.Request.QueryString[Import];
//                if (Uri.IsWellFormedUriString(import, UriKind.Absolute))
//                {
//                    ImportSessionsFromUrl(import);
//                    return;
//                }

//                if (context.Request.ServerVariables[QueryString] == (ExportJson))
//                {
//                    context.Response.ContentType = "application/json";

//                    context.Response.Write(ImportSerializer.SerializeSessions(ProfilingSession.CircularBuffer));
//                    context.Response.End();
//                    return;
//                }

//                var exportCorrelationId = context.Request.QueryString[CorrelationId];
//                if (!string.IsNullOrEmpty(exportCorrelationId))
//                {
//                    var guid = new Guid(exportCorrelationId);
//                    //var result = ProfilingSession.CircularBuffer.FirstOrDefault(
//                    //        r => r.Data != null && r.Data.ContainsKey(CorrelationId) && r.Data[CorrelationId] == exportCorrelationId);
   
//                    var result = ProfilingSession.CircularBuffer.FirstOrDefault(r => r.Id == guid);
//                    if (result != null)
//                    {
//                        if ( context.Request.QueryString[ExportType] == (ExportJson))
//                         {
//                             context.Response.ContentType = "application/json";

//                             context.Response.Write(ImportSerializer.SerializeSessions(new[] { result }));
//                             context.Response.End();
//                             return;
//                         }
                        
//                    }
//                }
//            }
//            else if (TryToImportDrillDownResult && path.IndexOf(ViewUrl, StringComparison.OrdinalIgnoreCase) >= 0)
//            {
//                var uuid = path.Split('/').Last();
//                var result = ProfilingSession.CircularBuffer.FirstOrDefault(
//                        r => r.Id.ToString().ToLowerInvariant() == uuid.ToLowerInvariant());
//                if (result != null)
//                {
//                    foreach (var timing in result.Timings)
//                    {
//                        if (timing.Data == null || !timing.Data.ContainsKey(CorrelationId)) continue;
//                        Guid correlationId;
//                        if (!Guid.TryParse(timing.Data[CorrelationId], out correlationId)
//                            || ProfilingSession.CircularBuffer.Any(r => r.Id == correlationId)) continue;

//                        string remoteAddress;
//                        if (!timing.Data.TryGetValue("remoteAddress", out remoteAddress))
//                            remoteAddress = timing.Name;

//                        if (!Uri.IsWellFormedUriString(remoteAddress, UriKind.Absolute)) continue;

//                        if (!remoteAddress.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;

//                        var pos = remoteAddress.IndexOf("?");
//                        if (pos > 0) remoteAddress = remoteAddress.Substring(0, pos);
//                        if (remoteAddress.Split('/').Last().Contains(".")) remoteAddress = remoteAddress.Substring(0, remoteAddress.LastIndexOf("/"));

//                        try
//                        {
//                            ImportSessionsFromUrl(remoteAddress + "/pureprofiler/view?" + CorrelationId + "=" + correlationId.ToString("N"));
//                        }
//                        catch(Exception ex)
//                        {
//                            System.Diagnostics.Debug.Write(ex.Message);

//                            //ignore exceptions
//                        }
//                    }
//                }
//            }
//        }

//        private void ImportSessionsFromUrl(string importUrl)
//        {
//            IEnumerable<ITimingSession> sessions = null;

//            var request = WebRequest.Create(importUrl);
//            request.Timeout = 30000;
//            using (var response = request.GetResponse() as HttpWebResponse)
//            using (var stream = new StreamReader(response.GetResponseStream()))
//            {
//                if (response.StatusCode == HttpStatusCode.OK)
//                {
//                    var content = stream.ReadToEnd();
//                    sessions = ImportSerializer.DeserializeSessions(content);
//                }
//            }

//            if (sessions == null)
//            {
//                return;
//            }

//            if (ProfilingSession.CircularBuffer == null)
//            {
//                return;
//            }

//            var existingIds = ProfilingSession.CircularBuffer.Select(session => session.Id).ToList();
//            foreach (var session in sessions)
//            {
//                if (!existingIds.Contains(session.Id))
//                {
//                    ProfilingSession.CircularBuffer.Add(session);
//                }
//            }
//        }

//        #endregion
//    }
//}
