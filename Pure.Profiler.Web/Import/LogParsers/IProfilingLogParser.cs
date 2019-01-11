

using System;
using System.Collections.Generic;
using Pure.Profiler.Timings;

namespace Pure.Profiler.Web.Import.LogParsers
{
    /// <summary>
    /// Represents a profiling log parser.
    /// </summary>
    public interface IProfilingLogParser
    {
        /// <summary>
        /// Loads latest top profiling session summaries from log.
        /// </summary>
        /// <param name="top"></param>
        /// <param name="minDuration"></param>
        /// <returns></returns>
        IEnumerable<ITimingSession> LoadLatestSessionSummaries(uint? top = 100, uint? minDuration = 0);

        /// <summary>
        /// Loads a full profiling session from log.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        ITimingSession LoadSession(Guid sessionId);

        /// <summary>
        /// Drill down profiling session from log.
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        ITimingSession DrillDownSession(Guid correlationId);

        /// <summary>
        /// Drill up profiling session from log.
        /// </summary>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        ITimingSession DrillUpSession(Guid correlationId);
    }
}
