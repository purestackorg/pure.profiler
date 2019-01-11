
using System;
using System.Collections.Concurrent;
using System.Collections.Generic; 
using System.Threading;
using System.Linq;

namespace Pure.Profiler
{



    /// <summary>
    /// The default AsyncLocal based <see cref="IProfilingSessionContainer"/> implementation.
    /// </summary>
    public class CallContextProfilingSessionContainer : IProfilingSessionContainer
    {
        private static readonly AsyncLocal<Guid?> _profilingStepId = new AsyncLocal<Guid?>();
        private static readonly AsyncLocal<ProfilingSession> _profilingSession = new AsyncLocal<ProfilingSession>();

        #region Public Members

        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        public ProfilingSession CurrentSession
        {
            get
            {
                return _profilingSession.Value;
            }
            set
            {
                _profilingSession.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the current profiling step id.
        /// </summary>
        public Guid? CurrentSessionStepId
        {
            get { return _profilingStepId.Value; }
            set { _profilingStepId.Value = value; }
        }

        /// <summary>
        /// Clears the current profiling session &amp; step id.
        /// </summary>
        public void Clear()
        {
            _profilingSession.Value = null;
            _profilingStepId.Value = null;
        }

        #endregion

        #region ICurrentProfilingSessionContainer Members

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

    ///// <summary>
    ///// The default CallContext based <see cref="IProfilingSessionContainer"/> implementation.
    ///// </summary>
    //public class CallContextProfilingSessionContainer : IProfilingSessionContainer
    //{
    //    private static readonly ConcurrentDictionary<Guid, WeakReference> ProfilingSessionStore
    //        = new ConcurrentDictionary<Guid, WeakReference>();
    //    private const string CurrentProfilingSessionIdCacheKey = "pure_profiler::current_profiling_session_id";
    //    private const string CurrentProfilingStepIdCacheKey = "pure_profiler::current_profiling_step_id";
    //    private static readonly Timer CleanUpProfilingSessionStoreTimer
    //        = new Timer(CleanUpProfilingSessionStoreTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

    //    #region Public Members

    //    /// <summary>
    //    /// Sets the periodically clean-up profiling session store period in milliseconds.
    //    /// </summary>
    //    /// <param name="milliseconds"></param>
    //    public static void SetCleanUpProfilingSessionStorePeriod(int milliseconds)
    //    {
    //        if (milliseconds <= 0)
    //        {
    //            throw new ArgumentException("milliseconds should > 0");
    //        }

    //        CleanUpProfilingSessionStoreTimer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(milliseconds));

    //    }

    //    /// <summary>
    //    /// Gets or sets the current ProfilingSession.
    //    /// </summary>
    //    public ProfilingSession CurrentSession
    //    {
    //        get
    //        {
    //            var obj = CallContext.GetData(CurrentProfilingSessionIdCacheKey);
    //            if (obj == null)
    //            {
    //                return null;
    //            }

    //            var sessionId = (Guid?)obj;
    //            WeakReference wrapper;
    //            if (!ProfilingSessionStore.TryGetValue(sessionId.Value, out wrapper) || wrapper == null || !wrapper.IsAlive)
    //            {
    //                return null;
    //            }

    //            return wrapper.Target as ProfilingSession;
    //        }
    //        set
    //        {
    //            if (value == null)
    //            {
    //                CallContext.LogicalSetData(CurrentProfilingSessionIdCacheKey, null);
    //                return;
    //            }

    //            ProfilingSessionStore.TryAdd(value.Profiler.Id, new WeakReference(value));
    //            CallContext.LogicalSetData(CurrentProfilingSessionIdCacheKey, (Guid?)value.Profiler.Id);
    //        }
    //    }

    //    /// <summary>
    //    /// Gets or sets the current profiling step id.
    //    /// </summary>
    //    public Guid? CurrentSessionStepId
    //    {
    //        get { return CallContext.LogicalGetData(CurrentProfilingStepIdCacheKey) as Guid?; }
    //        set { CallContext.LogicalSetData(CurrentProfilingStepIdCacheKey, value); }
    //    }

    //    /// <summary>
    //    /// Clears the current profiling session &amp; step id.
    //    /// </summary>
    //    public void Clear()
    //    {
    //        // clear current session
    //        var obj = CallContext.GetData(CurrentProfilingSessionIdCacheKey);
    //        if (obj != null)
    //        {
    //            var sessionId = (Guid?)obj;
    //            WeakReference temp;
    //            ProfilingSessionStore.TryRemove(sessionId.Value, out temp);
    //        }
    //        CurrentSession = null;

    //        // clear step id
    //        CurrentSessionStepId = null;
    //    }

    //    #endregion

    //    #region ICurrentProfilingSessionContainer Members

    //    ProfilingSession IProfilingSessionContainer.CurrentSession
    //    {
    //        get { return CurrentSession; }
    //        set { CurrentSession = value; }
    //    }

    //    Guid? IProfilingSessionContainer.CurrentSessionStepId
    //    {
    //        get { return CurrentSessionStepId; }
    //        set { CurrentSessionStepId = value; }
    //    }

    //    void IProfilingSessionContainer.Clear()
    //    {
    //        Clear();
    //    }

    //    #endregion

    //    #region Private Methods

    //    private static void CleanUpProfilingSessionStoreTimerCallback(object state)
    //    {
    //        WeakReference wrapper;

    //        // search for keys to remove
    //        var keysToRemove = new List<Guid>();
    //        foreach (var key in ProfilingSessionStore.Select(item => item.Key).ToList())
    //        {
    //            if (ProfilingSessionStore.TryGetValue(key, out wrapper) && !wrapper.IsAlive)
    //            {
    //                keysToRemove.Add(key);
    //            }
    //        }

    //        // remove
    //        foreach (var key in keysToRemove)
    //        {
    //            ProfilingSessionStore.TryRemove(key, out wrapper);
    //        }
    //    }

    //    #endregion
    //}
}
