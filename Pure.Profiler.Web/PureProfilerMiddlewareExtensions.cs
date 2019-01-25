using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Pure.Profiler.ProfilingFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Pure.Profiler.Web
{
    public static class PureProfilerMiddlewareExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPureProfiler(this IServiceCollection services )
        {
           
            services.AddSession();
           
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            return services;
        }

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

            ServiceLocator.Current = builder.ApplicationServices;
             
            return builder.UseMiddleware<PureProfilerMiddleware>();
        }
    }


    internal class ServiceLocator
    {
        public static IServiceProvider Current { get; set; }

        public static T GetService<T>()
        {
            return Current.GetService<T>();
        }
  

        public static object GetService(Type type)
        {
            return Current.GetService(type);
        }

        public static bool IsRegistered<T>()
        {
            return GetService<T>() != null;
        }

        public static bool IsRegistered(Type type)
        {
            return GetService( type) != null;
        }
    }
}
