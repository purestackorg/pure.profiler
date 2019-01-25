
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Pure.Profiler.ProfilingFilters;
using Pure.Profiler.Storages;
using Pure.Profiler.Timings;

namespace Pure.Profiler.Configuration
{

    public static class ConfigurationHelper
    {


        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())//(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("pureprofiler.json", true)
               .Build();
        }
        public const string LoginUrl = "/pureprofiler/login";

        public const string AuthCookieName = "PureProfilerAuthSessionId";

        private static PureProfilerConfigurationSection configsection = null;
        private static object olock = new object();
        public static PureProfilerConfigurationSection LoadPureProfilerConfigurationSection()
        {
            if (configsection == null)
            {
                lock (olock)
                {
                    configsection = new PureProfilerConfigurationSection();

                    var config = ConfigurationHelper.GetConfiguration();
                    if (config == null) return configsection;
                    //var logProviderName = config["logProvider"];
                    //if (!string.IsNullOrEmpty(logProviderName))
                    //{
                    //    var logProviderType = Type.GetType(logProviderName, true);
                    //    if (logProviderType != null)
                    //    {
                    //        ILoggerProvider logProvider;
                    //        if (logProviderType.GetConstructor(new Type[0]) == null)
                    //        {
                    //            logProvider = Activator.CreateInstance(logProviderType, new object[] { null }) as ILoggerProvider;
                    //        }
                    //        else
                    //        {
                    //            logProvider = Activator.CreateInstance(logProviderType) as ILoggerProvider;
                    //        }

                    //        if (logProvider == null)
                    //        {
                    //            throw new InvalidOperationException("Invalid log provider: " + logProviderName);
                    //        }

                    //        LoggerFactory.AddProvider(logProvider);
                    //    }
                    //}

                    var providerName = config["provider"];
                    if (!string.IsNullOrEmpty(providerName))
                    {
                        configsection.Provider = providerName;

                    }

                    var dbType = config["dbType"];
                    if (!string.IsNullOrEmpty(dbType))
                    {
                        configsection.DbType = dbType;

                    }
                    var showError = config["showError"];
                    if (!string.IsNullOrEmpty(showError))
                    {
                        configsection.ShowError = Convert.ToBoolean(showError);

                    }
                    var enableProxy = config["enableProxy"];
                    if (!string.IsNullOrEmpty(enableProxy))
                    {
                        configsection.EnableProxy = Convert.ToBoolean(enableProxy);

                    }
                    var proxyBaseUrl = config["proxyBaseUrl"];
                    if (!string.IsNullOrEmpty(proxyBaseUrl))
                    {
                        configsection.ProxyBaseUrl = proxyBaseUrl;

                    }


                    var storage = config["storage"];
                    if (!string.IsNullOrEmpty(storage))
                    {
                        configsection.Storage = storage;

                    }

                    var circularBufferSizeStr = config["circularBufferSize"];

                    if (!string.IsNullOrEmpty(circularBufferSizeStr))
                    {
                        var circularBufferSize = int.Parse(circularBufferSizeStr);
                        configsection.CircularBufferSize = circularBufferSize;


                    }

                    var EnableUtcTime = config["enableUtcTime"];
                    if (!string.IsNullOrEmpty(EnableUtcTime))
                    {
                        configsection.EnableUtcTime = Convert.ToBoolean(EnableUtcTime);  

                    }

                    var EnableProfiler = config["enableProfiler"];
                    if (!string.IsNullOrEmpty(EnableProfiler))
                    {
                        configsection.EnableProfiler = Convert.ToBoolean(EnableProfiler);

                    }

                    var RootBaseUrl = config["rootBaseUrl"];
                    if (!string.IsNullOrEmpty(RootBaseUrl))
                    {
                        configsection.RootBaseUrl = RootBaseUrl;

                    }


                    var EnableAuth = config["enableAuth"];
                    if (!string.IsNullOrEmpty(EnableAuth))
                    {
                        configsection.EnableAuth = Convert.ToBoolean(EnableAuth);

                    }


                    var AuthAccount = config["authAccount"];
                    if (!string.IsNullOrEmpty(AuthAccount))
                    {
                        configsection.AuthAccount = AuthAccount;

                    }


                    var AuthPassword = config["authPassword"];
                    if (!string.IsNullOrEmpty(AuthPassword))
                    {
                        configsection.AuthPassword = AuthPassword;

                    }

                    // load filters
                    var filtersSection = config.GetSection("filters");
                    if (filtersSection != null)
                    {
                        List<ProfilingFilterElement> Filters = new List<ProfilingFilterElement>();
                        var filters = filtersSection.GetChildren();

                        foreach (var filter in filters)
                        {
                            ProfilingFilterElement filterEle = new ProfilingFilterElement();
                            filterEle.Key = filter["key"];
                            filterEle.Type = filter["type"];
                            filterEle.Value = filter["value"];
                            //if (filter["properties"] != stringv)
                            //{
                            //    filterEle.Properties = filter["properties"];

                            //}
                            Filters.Add(filterEle);
                        }

                        configsection.Filters = Filters;
                    }
                }
                
            }
           

            return configsection;

        }
    }

    internal sealed class ConfigurationSectionConfigurationProvider : IConfigurationProvider
    {


        #region Constructors

        public ConfigurationSectionConfigurationProvider()
        {
            var ProfilerConfig = ConfigurationHelper.LoadPureProfilerConfigurationSection();

            PureProfilerConfiguration = ProfilerConfig;
            // load filters
            var filters = ProfilerConfig.Filters;
            if (filters != null)
            {
                var filterList = new List<IProfilingFilter>();

                foreach (ProfilingFilterElement filter in filters)
                {
                    if (string.IsNullOrWhiteSpace(filter.Type) ||
                        string.Equals(filter.Type, "contain", StringComparison.OrdinalIgnoreCase))
                    {
                        filterList.Add(new NameContainsProfilingFilter(filter.Value));
                    }
                    else if (string.Equals(filter.Type, "regex", StringComparison.OrdinalIgnoreCase))
                    {
                        filterList.Add(new RegexProfilingFilter(new Regex(filter.Value, RegexOptions.Compiled | RegexOptions.IgnoreCase)));
                    }
                    else if (string.Equals(filter.Type, "disable", StringComparison.OrdinalIgnoreCase))
                    {
                        if (filter.Value == "1")
                        {
                            filterList.Add(new DisableProfilingFilter());
                        }
                    }
                    else
                    {
                        var filterType = Type.GetType(filter.Type, true);
                        if (!typeof(IProfilingFilter).IsAssignableFrom(filterType))
                        {
                            throw new PureProfilerException("Invalid type name: " + filter.Type);
                        }

                        try
                        {
                            filterList.Add((IProfilingFilter)Activator.CreateInstance(filterType, new object[] { filter.Value }));
                        }
                        catch (Exception ex)
                        {
                            throw new PureProfilerException("Invalid type name: " + filter.Type, ex);
                        }
                    }
                }

                Filters = filterList;
            }

            // set ProfilingStorage
            if (!string.IsNullOrEmpty(ProfilerConfig.Storage))
            {
                var type = Type.GetType(ProfilerConfig.Storage, true);
                Storage = Activator.CreateInstance(type) as IProfilingStorage;
            }

            // set CircularBuffer
            if (ProfilerConfig.CircularBufferSize > 0)
            {
                CircularBuffer = new CircularBuffer<ITimingSession>(ProfilerConfig.CircularBufferSize);
            }

         
        }

        #endregion

        #region IConfigurationProvider Members

        public PureProfilerConfigurationSection PureProfilerConfiguration { get; private set; }
        public IProfilingStorage Storage { get; private set; }
        public int CircularBufferSize { get; private set; }

        public IEnumerable<IProfilingFilter> Filters { get; private set; }

        public ICircularBuffer<ITimingSession> CircularBuffer { get; private set; }

        #endregion
    }
}
