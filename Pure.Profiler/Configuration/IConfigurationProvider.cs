
using System.Collections.Generic;
using Pure.Profiler.ProfilingFilters;
using Pure.Profiler.Storages;
using Pure.Profiler.Timings;

namespace Pure.Profiler.Configuration
{
    /// <summary>
    /// Reprensent a configuration provider.
    /// </summary>
    public interface IConfigurationProvider
    {
        PureProfilerConfigurationSection PureProfilerConfiguration { get; }
        /// <summary>
        /// Gets the profiling storage.
        /// </summary>
        IProfilingStorage Storage { get; }

        /// <summary>
        /// Gets the profiling filters.
        /// </summary>
        IEnumerable<IProfilingFilter> Filters { get; }

        int CircularBufferSize { get; }
        /// <summary>
        /// Gets the profiling circular buffer.
        /// </summary>
        ICircularBuffer<ITimingSession> CircularBuffer { get; }
    }
}
