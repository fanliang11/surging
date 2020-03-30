using Surging.Core.CPlatform;
using Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
   public static class ServiceDescriptorExtensions
    {
        public static ServiceDescriptor CacheTime(this ServiceDescriptor descriptor,int time)
        {
            if(string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
            descriptor.Metadatas["CacheIntercept"] = time;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{time}";

            return descriptor;
        }

        public static ServiceDescriptor Mode(this ServiceDescriptor descriptor, CacheTargetType cacheTargetType)
        {
            var targetType= Convert.ToInt32(cacheTargetType).ToString();
            if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                descriptor.Metadatas["CacheIntercept"] = targetType;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{targetType}";

            return descriptor;
        }

        public static ServiceDescriptor  Method(this ServiceDescriptor descriptor, CachingMethod cachingMethod)
        {
            var iCachingMethod = Convert.ToInt32(cachingMethod).ToString();
            if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                descriptor.Metadatas["CacheIntercept"] = iCachingMethod;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{iCachingMethod}";

            return descriptor;
        }

        public static ServiceDescriptor CacheSectionType(this ServiceDescriptor descriptor, string cacheSectionType)
        {
            if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                descriptor.Metadatas["CacheIntercept"] = cacheSectionType;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{cacheSectionType}";

            return descriptor;
        }

        public static ServiceDescriptor Force(this ServiceDescriptor descriptor, bool Force)
        {
            var iForce = Convert.ToInt32(Force).ToString();
            if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                descriptor.Metadatas["CacheIntercept"] = iForce;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{iForce}";

            return descriptor;
        }

        public static ServiceDescriptor Key(this ServiceDescriptor descriptor, string key)
        {
            if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                descriptor.Metadatas["CacheIntercept"] = key;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{key}";

            return descriptor;
        }

        public static ServiceDescriptor L2Key(this ServiceDescriptor descriptor, string L2Key)
        {
            if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                descriptor.Metadatas["CacheIntercept"] = L2Key;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{L2Key}";

            return descriptor;
        }

        public static ServiceDescriptor EnableL2Cache(this ServiceDescriptor descriptor, bool enableL2Cache)
        {
            var iEnableL2Cache = Convert.ToInt32(enableL2Cache).ToString();
            if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                descriptor.Metadatas["CacheIntercept"] = iEnableL2Cache;
            else
                descriptor.Metadatas["CacheIntercept"] += $"|{iEnableL2Cache}";

            return descriptor;
        }

        public static ServiceDescriptor CorrespondingKeys(this ServiceDescriptor descriptor, string[] CorrespondingKeys)
        {
            if (CorrespondingKeys != null)
            {
                var correspondingKey = string.Join(",", CorrespondingKeys);
                if (string.IsNullOrEmpty(descriptor.GetMetadata<string>("CacheIntercept")))
                    descriptor.Metadatas["CacheIntercept"] = correspondingKey;
                else
                    descriptor.Metadatas["CacheIntercept"] += $"|{correspondingKey}";
            }
            else
            {
                descriptor.Metadatas["CacheIntercept"] += "|";
            }

            return descriptor;
        }

        public static ServiceCacheIntercept GetCacheIntercept(this ServiceDescriptor descriptor)
        {
            return  new ServiceCacheIntercept( descriptor.GetMetadata("CacheIntercept", "").Split("|"));
        }
    }
}
