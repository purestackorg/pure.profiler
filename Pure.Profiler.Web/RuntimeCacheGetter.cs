using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
//using System.Web.Caching;

namespace Pure.Profiler.Web
{
    public class RuntimeCacheGetter
    {
        private const string TestCacheKey = "pureprofilerCache";
         private static readonly MethodInfo MethodInfoCacheGet = null;
        private static readonly PropertyInfo ProcessInfoUtcCreated;
        private static readonly PropertyInfo ProcessInfoUtcExpires;
        private static readonly PropertyInfo ProcessInfoSlidingExpiration;

        //static RuntimeCacheGetter()
        //{
        //    if (HttpRuntime.Cache != null)
        //    {

        //        // Need an item in the cache to call the MethodInfoCacheGet.Invoke below.
        //        HttpRuntime.Cache.Add(TestCacheKey, string.Empty, null, DateTime.Now.AddHours(1), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.AboveNormal, null);
        //         MethodInfoCacheGet = HttpRuntime.Cache.GetType().GetMethod("Get", BindingFlags.Instance | BindingFlags.NonPublic);
        //         if (MethodInfoCacheGet != null)
        //         {
        //             var cacheEntry = MethodInfoCacheGet.Invoke(HttpRuntime.Cache, new object[] { TestCacheKey, 1 });
        //             var typeCacheEntity = cacheEntry.GetType();
        //             ProcessInfoUtcCreated = typeCacheEntity.GetProperty("UtcCreated", BindingFlags.NonPublic | BindingFlags.Instance);
        //             ProcessInfoUtcExpires = typeCacheEntity.GetProperty("UtcExpires", BindingFlags.NonPublic | BindingFlags.Instance);
        //             ProcessInfoSlidingExpiration = typeCacheEntity.GetProperty("SlidingExpiration", BindingFlags.NonPublic | BindingFlags.Instance);

        //         }
              
        //        HttpRuntime.Cache.Remove(TestCacheKey);
        //    }

           
        //}

        public static object GetValueSafe(object value)
        {
            if (value != null)
            {
                var type = value.GetType();
                if (!type.IsSerializable)
                {
                    if (type.GetMethod("ToString").DeclaringType == type)
                    {
                        value = value.ToString();
                    }
                    else
                    {
                        value = @"\Non serializable type :(\";
                    }
                }
            }

            return value;
        }
        private static object GetCacheProperty(PropertyInfo property, object cacheEntry)
        {
            try
            {
                return property.GetValue(cacheEntry, null);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string GetCaches(HttpContext context)
        {
            if (context == null)
            {
                return "";
            }
            return "暂不支持";

            //StringBuilder sbSession = new StringBuilder();
            //sbSession.AppendFormat("<b>Runtime Cache</b>:{0}", ProfilingSession.NEWLINE);

            //if (HttpRuntime.Cache == null)
            //{
            //    sbSession.Append("Warning: HttpRuntime.Cache IS NULL !" );
            //    return sbSession.ToString();
            //}

            //var EffectivePercentagePhysicalMemoryLimit = HttpRuntime.Cache.EffectivePercentagePhysicalMemoryLimit;
            //var EffectivePrivateBytesLimit = HttpRuntime.Cache.EffectivePrivateBytesLimit;
            //sbSession.AppendFormat("EffectivePercentagePhysicalMemoryLimit:{0}{1}", EffectivePercentagePhysicalMemoryLimit, ProfilingSession.NEWLINE);
            //sbSession.AppendFormat("EffectivePrivateBytesLimit:{0}{1}", EffectivePrivateBytesLimit, ProfilingSession.NEWLINE);

            //var list = HttpRuntime.Cache.Cast<System.Collections.DictionaryEntry>().ToList();
            //foreach (var item in list)
            //{
            //    try
            //    {

            //        var cacheEntry = MethodInfoCacheGet.Invoke(HttpRuntime.Cache, new object[] { item.Key, 1 });
            //        sbSession.AppendFormat("Key:{0}{5}    Value:{1}{5}    CreatedOn:{2}{5}    ExpiresOn:{3}{5}    SlidingExpiration:{4}{5}", item.Key.ToString(), GetValueSafe(item.Value)
            //            ,GetCacheProperty(ProcessInfoUtcCreated, cacheEntry) as DateTime? , 
            //            GetCacheProperty(ProcessInfoUtcExpires, cacheEntry) as DateTime?,
            //            GetCacheProperty(ProcessInfoSlidingExpiration, cacheEntry) as TimeSpan?
            //            , ProfilingSession.NEWLINE);

            //        sbSession.AppendFormat(ProfilingSession.NEWLINE);


            //    }
            //    catch (Exception)
            //    {
            //        return "";
            //    }
            //}

            //return sbSession.ToString();
        }


    }
}