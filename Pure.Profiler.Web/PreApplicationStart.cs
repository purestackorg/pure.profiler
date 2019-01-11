
//using Pure.Profiler.ProfilingFilters;
//using Pure.Profiler.Web.Handlers;
//using Microsoft.Web.Infrastructure.DynamicModuleHelper;
//using Pure.Profiler.Web.Import.Handlers;

//namespace Pure.Profiler.Web
//{
//    /// <summary>
//    /// The PreApplicationStart helper class for setting HttpContextCallContextProfilingSessionContainer
//    /// as the default profiling session container to make
//    /// ProfilingSession.Current work consistently in web application.
//    /// </summary>
//    public static class PreApplicationStart
//    {
//        /// <summary>
//        /// The init method to be called in app startup.
//        /// </summary>
//        public static void Init()
//        {
//            // set WebProfilingSessionContainer as the default profiling session container
//            // if the current one is CallContextProfilingSessionContainer
//            if (ProfilingSession.ProfilingSessionContainer.GetType() == typeof(CallContextProfilingSessionContainer))
//            {
//                ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer();
//            }

//            // register pureProfilerModule
//            DynamicModuleUtility.RegisterModule(typeof(PureProfilerModule));
//            // register PureProfilerImportModule
//            DynamicModuleUtility.RegisterModule(typeof(PureProfilerImportModule));
//            // ignore pureprofiler view-result requests from profiling
//            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler/view"));
//            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler/export"));
//            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler-resources"));
             
//        }
//    }
//}
