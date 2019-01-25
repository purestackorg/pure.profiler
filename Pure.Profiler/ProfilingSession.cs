
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Pure.Profiler.Configuration;
using Pure.Profiler.ProfilingFilters;
using Pure.Profiler.Storages;
using Pure.Profiler.Timings;
using System.Collections.Concurrent;
using System.Text;
using System.Collections.Specialized;

namespace Pure.Profiler
{
    /// <summary>
    /// Represents a profiling session.
    /// </summary>
    public sealed class ProfilingSession : MarshalByRefObject
    {
        public static string FailOnErrorMark = "PureProfilingFailOnErrorMark";

        private static Action<Exception, object> LogWriter = null;

        private static IProfilingSessionContainer _profilingSessionContainer;
        private static IProfilingStorage _profilingStorage;
        private readonly IProfiler _profiler;

        #region Properties

        /// <summary>
        /// Gets the <see cref="IProfiler"/> attached to the current profiling session.
        /// </summary>
        public IProfiler Profiler
        {
            get { return _profiler; }
        }

        /// <summary>
        /// Gets the current profiling session.
        /// </summary>
        public static ProfilingSession Current
        {
            get { return _profilingSessionContainer.CurrentSession; }
        }

        /// <summary>
        /// Sets current profiling session as specified session and sets the parent step as specified.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="parentStepId">if parentStepId not specified, use the root step of session as parent step by default.</param>
        public static void SetCurrentProfilingSession(
            ProfilingSession session, Guid? parentStepId = null)
        {
            ProfilingSessionContainer.CurrentSession = null;
            ProfilingSessionContainer.CurrentSessionStepId = null;

            if (session == null || session.Profiler == null) return;

            var timingSession = session.Profiler.GetTimingSession();
            if (timingSession == null
                || timingSession.Timings == null
                || timingSession.Timings.All(t => t.ParentId != timingSession.Id)) return;

            ProfilingSessionContainer.CurrentSession = session;

            if (parentStepId.HasValue && timingSession.Timings.Any(t => t.Id == parentStepId.Value && string.Equals(t.Type, "step")))
            {
                ProfilingSessionContainer.CurrentSessionStepId = parentStepId.Value;
            }
            else // if parentStepId not specified, use the root step of session as parent step by default
            {
                var rootStep = timingSession.Timings.FirstOrDefault(t => t.ParentId == timingSession.Id);
                if (rootStep == null) return;

                ProfilingSessionContainer.CurrentSessionStepId = rootStep.Id;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IProfilingSessionContainer"/>.
        /// </summary>
        public static IProfilingSessionContainer ProfilingSessionContainer
        {
            get { return _profilingSessionContainer; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _profilingSessionContainer = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IProfilingStorage"/>.
        /// </summary>
        public static IProfilingStorage ProfilingStorage
        {
            get { return _profilingStorage; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _profilingStorage = value;
            }
        }
        /// <summary>
        /// config
        /// </summary>
        public static IConfigurationProvider Configuration { get; private set; }
        private static string _ConfigurationString = "";
        private static string _AppConfgString = "";
        public static string NEWLINE  = "<br>";
        private static readonly object syncRoot = new object();
        public static string ConfigurationString
        {
            get
            {
                lock (syncRoot)
                {
                    if (string.IsNullOrEmpty(_ConfigurationString))
                    {
                        StringBuilder sbPureProfiler = new StringBuilder();
                        sbPureProfiler.AppendFormat("<b>Copyright:(C) {0} Benson</b>{1}{1}", DateTime.Now.ToString("yyyy"), NEWLINE);
                        sbPureProfiler.AppendFormat("EnableProfiler:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.EnableProfiler, NEWLINE);
                        sbPureProfiler.AppendFormat("RootBaseUrl:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.RootBaseUrl, NEWLINE);

                        sbPureProfiler.AppendFormat("Provider:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.Provider, NEWLINE);
                        sbPureProfiler.AppendFormat("CircularBufferSize:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.CircularBufferSize, NEWLINE);
                        sbPureProfiler.AppendFormat("DbType:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.DbType, NEWLINE);
                        sbPureProfiler.AppendFormat("EnableUtcTime:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.EnableUtcTime, NEWLINE);
                        sbPureProfiler.AppendFormat("ShowError:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.ShowError, NEWLINE);
                        sbPureProfiler.AppendFormat("EnableProxy:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.EnableProxy, NEWLINE);
                        sbPureProfiler.AppendFormat("ProxyBaseUrl:{0}{1}", ProfilingSession.Configuration.PureProfilerConfiguration.ProxyBaseUrl, NEWLINE);
                        sbPureProfiler.AppendFormat("Storage:{0}{1}{1}", ProfilingSession.Configuration.Storage, NEWLINE);
                        sbPureProfiler.AppendFormat("Filters:{0}", NEWLINE);
                        foreach (var filter in ProfilingFilters)
                        {
                            sbPureProfiler.AppendFormat("Name:{0}    Args:{1}{2}", filter.Name, filter.Args, NEWLINE);
                        }

                        _ConfigurationString = sbPureProfiler.ToString();

                    }
                }
                return _ConfigurationString;
            }
        }
        public static StringBuilder GetPairs(NameValueCollection nvc)
        {
            var result = new StringBuilder();
            if (nvc == null)
                return result;

            for (int i = 0; i < nvc.Count; i++)
            {
                result.AppendFormat("{0}:{1}{2}", nvc.GetKey(i), nvc.Get(i), ProfilingSession.NEWLINE);
            }
            return result;
        }
        public static string AppConfigString
        {
            get
            {
                lock (syncRoot)
                {
                    if (string.IsNullOrEmpty(_AppConfgString))
                    {
                        StringBuilder sbPureProfiler = new StringBuilder();

                        sbPureProfiler.AppendFormat("<b>AppSettings</b>:{0}", NEWLINE); 

                        //sbPureProfiler.AppendFormat(GetPairs(System.Configuration.ConfigurationManager.AppSettings).ToString());
                        sbPureProfiler.AppendFormat("{0}", NEWLINE);

                        sbPureProfiler.AppendFormat("<b>ConnectionStrings</b>:{0}", NEWLINE);
                        //foreach (ConnectionStringSettings item in System.Configuration.ConfigurationManager.ConnectionStrings)
                        //{
                        //    sbPureProfiler.AppendFormat("Name:{0}    ConnectionString:{1}    Provider:{2}{3}", item.Name, item.ConnectionString, item.ProviderName, NEWLINE);

                        //}

                         


                        _AppConfgString = sbPureProfiler.ToString();

                    }
                }
                return _AppConfgString;
            }
        }


        private static bool? _Disabled = null;
        /// <summary>
        /// disable
        /// </summary>
        public static bool Disabled
        {
            get
            {
                if (_Disabled == null)
                {
                    if (Configuration != null)
                    {
                        _Disabled = Configuration.Filters.Any(filter => (filter is DisableProfilingFilter));
                    }
                }
                return _Disabled.HasValue? _Disabled.Value : true;
            }

        }

        /// <summary>
        /// Gets or sets GlobalData.
        /// </summary>
        //private static ConcurrentDictionary<string, string> _GlobalData = new ConcurrentDictionary<string, string>();
        //public static bool ExistGlobalData(string key)
        //{

        //    return _GlobalData.ContainsKey(key);

        //}
        //public static string GetGlobalData(string key)
        //{
        //    string value = "";
        //    _GlobalData.TryGetValue(key, out value);
        //    return value;
        //}
        //public static void SetGlobalData(string key, string globaldata, bool onetime = false)
        //{
        //    if (_GlobalData.ContainsKey(key))
        //    {
        //        if (onetime == true)
        //        {
        //            return;
        //        }
        //        else
        //        {
        //            _GlobalData[key] = globaldata;
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        _GlobalData.TryAdd(key, globaldata);
        //    }

        //}

        /// <summary>
        /// Gets the <see cref="IProfilingFilter"/>s globally registered.
        /// Adds or removes items of this property to control the filtering of profiling sessions.
        /// </summary>
        public static ICollection<IProfilingFilter> ProfilingFilters { get; private set; }

        /// <summary>
        /// Gets or sets a circular buffer for latest profiling sessions.
        /// </summary>
        public static ICircularBuffer<ITimingSession> CircularBuffer { get; set; }

        /// <summary>
        /// Default handler for creating a profiler.
        /// </summary>
        internal static Func<string, IProfilingStorage, TagCollection, IProfiler> CreateProfilerHandler = (name, storage, tags) => new Profiler(name, storage, tags);

        /// <summary>
        /// Default handler for handling exception.
        /// </summary>
        internal static Action<Exception, object> HandleExceptionHandler = HandleException;

        #endregion

        #region Constructors

        static ProfilingSession()
        {

            // by default, use CallContextProfilingSessionContainer
            _profilingSessionContainer = new CallContextProfilingSessionContainer();

            // by default, use JsonProfilingStorage
            _profilingStorage = new NoOperationProfilingStorage();

            // intialize filters
            ProfilingFilters = new ProfilingFilterList(new List<IProfilingFilter>());

            InitializeConfigurationFromConfig();
        }

        /// <summary>
        /// Initializes a <see cref="ProfilingSession"/> from an <see cref="IProfiler"/> instance.
        /// </summary>
        /// <param name="profiler">The attached <see cref="IProfiler"/> instance.</param>
        internal ProfilingSession(IProfiler profiler)
        {
            if (profiler == null)
            {
                throw new ArgumentNullException("profiler");
            }

            _profiler = profiler;
        }

        #endregion

        #region Public Methods

        public static void SetLogWriter(Action<Exception, object> _LogWriter)
        {
            LogWriter = _LogWriter;
        }

        /// <summary>
        /// Starts the profiling.
        /// </summary>
        /// <param name="name">The name of the profiling session.</param>
        /// <param name="tags">The tags of the profiling session.</param>
        public static void Start(string name, params string[] tags)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            // set null the current profiling session if exists
            ProfilingSession.SetCurrentProfilingSession(null);

            if (ProfilingFilters.Count > 0)
            {
                foreach (var filter in ProfilingFilters)
                {
                    if (filter == null) continue;
                    if (filter.ShouldBeExculded(name, tags)) return;
                }
            }

            IProfiler profiler = null;
            try
            {
                profiler = CreateProfilerHandler(name, _profilingStorage, tags == null || !tags.Any() ? null : new TagCollection(tags));
            }
            catch (Exception ex)
            {
                HandleExceptionHandler(ex, typeof(ProfilingSession));
            }

            if (profiler != null)
            {
                // Create the current ProfilingSession
                _profilingSessionContainer.CurrentSession = new ProfilingSession(profiler);
            }
        }

        /// <summary>
        /// Stops the current profiling session.
        /// </summary>
        /// <param name="discardResults">
        /// When true, discards the profiling results of the entire profiling session.
        /// </param>
        public static void Stop(bool discardResults = false)
        {
            var profilingSession = Current;
            if (profilingSession != null)
            {
                try
                {
                    if (CircularBuffer != null)
                    {
                        CircularBuffer.Add(profilingSession.Profiler.GetTimingSession());
                    }

                    profilingSession._profiler.Stop(discardResults);
                }
                catch (Exception ex)
                {
                    HandleExceptionHandler(ex, typeof(ProfilingSession));
                }
            }

            // Clear the current profiling session on stopping
            _profilingSessionContainer.Clear();
        }

        #endregion

        #region Non-Public Methods

        private static void HandleException(Exception ex, object origin)
        {
            if (LogWriter != null)
            {
                LogWriter(ex, origin);

            }
        }

        private static void InitializeConfigurationFromConfig()
        {
            var configSection = ConfigurationHelper.LoadPureProfilerConfigurationSection();//  ConfigurationManager.GetSection("pureprofiler");
            if (configSection != null && !(configSection is PureProfilerConfigurationSection))
            {
                throw new PureProfilerException("Invalid configuration, check the 'pureprofiler' configuration section.");
            }
            if (configSection == null) return;

            var ProfilerConfig = (configSection as PureProfilerConfigurationSection);
            var providerType = Type.GetType(ProfilerConfig.Provider, true);
            var provider = (IConfigurationProvider)Activator.CreateInstance(providerType);
            Configuration = provider;
            ProfilingStorage = provider.Storage;
            if (provider.Filters != null)
            {
                foreach (var filter in provider.Filters)
                {
                    ProfilingFilters.Add(filter);
                }
            }
            CircularBuffer = provider.CircularBuffer;
        }



        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns></returns>
        internal IDisposable StepImpl(string name, string[] tags)
        {
            IProfilingStep step = null;

            try
            {
                step = _profiler.Step(name, tags == null || !tags.Any() ? null : new TagCollection(tags));
            }
            catch (Exception ex)
            {
                HandleExceptionHandler(ex, this);
            }

            return step;
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns></returns>
        internal IDisposable IgnoreImpl()
        {
            IDisposable ignoredStep = null;
            try
            {
                ignoredStep = _profiler.Ignore();
            }
            catch (Exception ex)
            {
                HandleExceptionHandler(ex, this);
            }

            return ignoredStep;
        }

        /// <summary>
        /// Add a tag to current profiling session.
        /// </summary>
        /// <param name="tag"></param>
        internal void AddTagImpl(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;

            if (Profiler == null) return;

            var session = Profiler.GetTimingSession();
            if (session == null) return;

            if (session.Tags == null) session.Tags = new TagCollection();
            session.Tags.Add(tag);
        }

        /// <summary>
        /// Add a custom data field to current profiling session.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void AddFieldImpl(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            if (Profiler == null) return;

            var session = Profiler.GetTimingSession();
            if (session == null) return;

            if (session.Data == null) session.Data = new Dictionary<string, string>();
            session.Data[key] = value;
        }

        #endregion
    }
}
