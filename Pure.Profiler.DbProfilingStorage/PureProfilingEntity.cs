using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Profiler.DbProfilingStorage
{
    public class PureProfilingEntity
    {
        public string SEQ { get; set; }

        public string MachineName { get; set; }
        public string SessionId { get; set; }

        
        /// <summary>
        /// Gets or sets the type of the timing.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the identity of the timing.
        /// </summary>
        public string Id { get; set; }
       // public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identity of the parent timing.
        /// </summary>
        public string ParentId { get; set; }
       // public Guid? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the timing.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the UTC time of when the timing is started.
        /// </summary>
        public DateTime Started { get; set; }

        /// <summary>
        /// Gets or sets the start milliseconds since the start of the profling session.
        /// </summary>
        public long StartMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the duration milliseconds of the timing.
        /// </summary>
        public long DurationMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the tags of the timing.
        /// </summary>
        //public TagCollection Tags { get; set; }
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the ticks of this timing for sorting.
        /// </summary>
        public long Sort { get; set; }

        /// <summary>
        /// Gets or sets addtional data of the timing.
        /// </summary>
        //public Dictionary<string, string> Data { get; set; }
        public string Data { get; set; }


        public string ExecuteType { get; set; }
        public long ExecuteResult { get; set; }
        public string Parameters { get; set; }
        public string HttpVerb { get; set; }
        public string IsAjax { get; set; }
        public string ClientIp { get; set; }
        public long DbCount { get; set; }
        public long DbDuration { get; set; }
        public string RequestType { get; set; }
        public long ErrorCount { get; set; }

        
    }
}
