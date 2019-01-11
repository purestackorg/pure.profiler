using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Pure.Profiler.ProfilingFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace Pure.Profiler.Web
{
    public static class PureProfilerMiddlewareExtensions
    {
        public static IApplicationBuilder UsePureProfiler(this IApplicationBuilder builder, bool drillDown = false)
        {
            PureProfilerMiddleware.TryToImportDrillDownResult = drillDown;


            // set WebProfilingSessionContainer as the default profiling session container
            // if the current one is CallContextProfilingSessionContainer
            if (ProfilingSession.ProfilingSessionContainer.GetType() == typeof(CallContextProfilingSessionContainer))
            {
                var host = builder.ApplicationServices.GetRequiredService<IHostingEnvironment>();
                var httpContextAccessor = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
                ProfilingSession.ProfilingSessionContainer = new WebProfilingSessionContainer(httpContextAccessor, host);
            }

            // register pureProfilerModule
            //DynamicModuleUtility.RegisterModule(typeof(PureProfilerModule));
            // register PureProfilerImportModule
            // DynamicModuleUtility.RegisterModule(typeof(PureProfilerImportModule));
            // ignore pureprofiler view-result requests from profiling
            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler/view"));
            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler/export"));
            ProfilingSession.ProfilingFilters.Add(new NameContainsProfilingFilter("/pureprofiler-resources"));


            return builder.UseMiddleware<PureProfilerMiddleware>();
        }
    }
}
