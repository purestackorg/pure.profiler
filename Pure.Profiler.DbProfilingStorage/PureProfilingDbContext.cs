using Pure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Profiler.DbProfilingStorage
{
    public class DbLogger
    {
        static string logPrifix = "profilings";
        public static void LogStatic(string msg, Exception ex = null, Pure.Data.MessageType type = Pure.Data.MessageType.Debug)
        {
            if (ex != null)
            {
                msg += " -----> Error : "+ex;
            }
            FastLogger.WriteText(logPrifix, msg);
        }
    }

    public class PureProfilingDbContext : Database
    {
        public PureProfilingDbContext()
            : base(  "PureProfilingDbContext.xml", DbLogger.LogStatic, null)
        {

        }
         
    }
}
