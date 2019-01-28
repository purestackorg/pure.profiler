
using System.Collections.Generic;
using System.Configuration;
using Pure.Profiler.Storages;

namespace Pure.Profiler.Configuration
{
    /// <summary>
    /// The configuration section for profiler.
    /// </summary>
    public sealed class PureProfilerConfigurationSection  
    {
        //private const string ProviderPropertyName = "provider";
        //private static readonly string PropProvider="";// ConfigurationProperty PropProvider = new ConfigurationProperty(ProviderPropertyName, typeof(string), typeof(ConfigurationSectionConfigurationProvider).AssemblyQualifiedName);
        //private const string ProfilingFilterElementCollectionName = "filters";
        //private static readonly ConfigurationProperty PropFilters = new ConfigurationProperty(ProfilingFilterElementCollectionName, typeof(ProfilingFilterElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        //private const string ProfilingStoragePropertyName = "storage";
        //private static readonly ConfigurationProperty PropStorage = new ConfigurationProperty(ProfilingStoragePropertyName, typeof(string), typeof(NoOperationProfilingStorage).AssemblyQualifiedName);
        //private const string CircularBufferSizePropertyName = "circularBufferSize";
        //private static readonly ConfigurationProperty PropCircularBufferSize = new ConfigurationProperty(CircularBufferSizePropertyName, typeof(int), 100);
        //private static readonly ConfigurationPropertyCollection Props = new ConfigurationPropertyCollection();

        ////private const string ProfilingDbTypePropertyName = "dbType";
        ////private static readonly ConfigurationProperty PropDbType = new ConfigurationProperty(ProfilingDbTypePropertyName, typeof(string), "sqlserver");

        //private const string ProfilingShowErrorPropertyName = "showError";
        //private static readonly ConfigurationProperty PropShowError = new ConfigurationProperty(ProfilingShowErrorPropertyName, typeof(bool), true);

        //private const string ProfilingEnableProxyPropertyName = "enableProxy";
        //private static readonly ConfigurationProperty PropEnableProxy = new ConfigurationProperty(ProfilingEnableProxyPropertyName, typeof(bool), false);

        //private const string ProfilingProxyBaseUrlPropertyName = "proxyBaseUrl";
        //private static readonly ConfigurationProperty PropProxyBaseUrl = new ConfigurationProperty(ProfilingProxyBaseUrlPropertyName, typeof(string), "");


        private string dbType = "sqlserver";
         /// <summary>
         /// 数据库类型
         /// </summary> 
        public string DbType
        {
            get { return dbType.ToLower(); }
            set { dbType = value; }
        }

        private bool showError = true;
        /// <summary>
        /// 是否显示异常
        /// </summary> 
        public bool ShowError
        {
            get { return showError; }
            set { showError = value; }
        }

        private bool enableProxy = false;
        /// <summary>
        /// 是否启用Proxy代理 获取web response内容
        /// </summary>
        public bool EnableProxy
        {
            get { return (bool)enableProxy; }
            set { enableProxy = value; }
        }

        private string proxyBaseUrl = "";
        /// <summary>
        /// Proxy代理地址：如:http://localhost:41764
        /// </summary>
        public string ProxyBaseUrl
        {
            get { return (string)proxyBaseUrl; }
            set { proxyBaseUrl = value; }
        }

        private string provider =   typeof(ConfigurationSectionConfigurationProvider).AssemblyQualifiedName;

        /// <summary>
        /// Profiling configuration provider type.
        /// </summary>
        public string Provider
        {
            get { return (string)provider; }
            set { provider = value; }
        }

        /// <summary>
        /// Profiling filters.
        /// </summary>
        public List<ProfilingFilterElement> Filters
        {
            get;set;
        }

        private string storage = typeof(NoOperationProfilingStorage).AssemblyQualifiedName;
        /// <summary>
        /// Profiling storage type.
        /// </summary> 
        public string Storage
        {
            get { return storage; }
            set { storage = value; }
        }

        private int circularBufferSize = 100;
        /// <summary>
        /// Latest profiling sessions circular buffer size.
        /// </summary>

        public int CircularBufferSize
        {
            get { return circularBufferSize; }
            set { circularBufferSize = value; }
        }

        /// <summary>
        /// Gets configuration properties.
        /// </summary>
        protected   Dictionary<string, object> Properties
        {
            get;set;
        }

        private bool _EnableUtcTime = false;

        public bool EnableUtcTime
        {
            get { return (bool)_EnableUtcTime; }
            set { _EnableUtcTime = value; }
        }
        private bool _EnableProfiler = true;

        public bool EnableProfiler
        {
            get { return (bool)_EnableProfiler; }
            set { _EnableProfiler = value; }
        }


        private string _RootBaseUrl = "";

        public string RootBaseUrl
        {
            get { return (string)_RootBaseUrl; }
            set { _RootBaseUrl = value; }
        }




        private bool _EnableAuth = true;

        public bool EnableAuth
        {
            get { return (bool)_EnableAuth; }
            set { _EnableAuth = value; }
        }
        private string _AuthAccount = "";

        public string AuthAccount
        {
            get { return (string)_AuthAccount; }
            set { _AuthAccount = value; }
        }

        private string _EncryptAuthAccount = "";

        public string EncryptAuthAccount
        {
            get {
                if (_EncryptAuthAccount == "")
                {
                    _EncryptAuthAccount= PureProfilerEncryptHelper.EncryptByAES(AuthAccount, PureProfilerEncryptHelper.AES_KEY);
                }
                return _EncryptAuthAccount;

            }

        }
        public string Encrypt(string key)
        { 

                return PureProfilerEncryptHelper.EncryptByAES(key, PureProfilerEncryptHelper.AES_KEY);
             

        }
        private string _AuthPassword = "";

        public string AuthPassword
        {
            get { return (string)_AuthPassword; }
            set { _AuthPassword = value; }
        }
    }
}
