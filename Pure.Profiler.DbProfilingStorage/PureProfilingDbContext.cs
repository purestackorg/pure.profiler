using Pure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Profiler.DbProfilingStorage
{
    public class DbLogger
    {
        static string logPrifix = "profilings-";
        static string customDirectory = PathHelper.CombineWithBaseDirectory("logs");
        public static void LogStatic(string msg, Exception ex = null, Pure.Data.MessageType type = Pure.Data.MessageType.Debug)
        {
            if (ex != null)
            {
                msg += " -----> Error : "+ex;
            }
   
            FastLogger.WriteLog(customDirectory, logPrifix, msg);
        }
    }

    public class PureProfilingDbContext : Database
    {
        public PureProfilingDbContext()
            : base(  "PureProfilingDbContext.xml", DbLogger.LogStatic, config =>
            {

                //if (config.EnableDebug == true)
                //{
                //    config.DbConnectionInit = (conn) =>
                //    {

                //        if (ProfilingSession.Current == null)
                //        {
                //            return conn;
                //        }
                //        if (conn != null && conn.State != System.Data.ConnectionState.Open)
                //        {
                //            var dbProfiler = new Pure.Profiler.Data.DbProfiler(Pure.Profiler.ProfilingSession.Current.Profiler);

                //            conn = new Pure.Profiler.Data.ProfiledDbConnection(conn, dbProfiler);
                //        }

                //        return conn;
                //    };
                //}

            })
        {

        }
         
    }
}
