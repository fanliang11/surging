using Surging.Core.CPlatform;
using Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Surging.Core.ProxyGenerator.Interceptors.Implementation
{
   public static class ServiceDescriptorExtensions
    {
        public static ServiceDescriptor CacheTime(this ServiceDescriptor descriptor,int time, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = time.ToString();
            else
                metadata.Item1 += $"|{time}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor Mode(this ServiceDescriptor descriptor, CacheTargetType cacheTargetType, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            var targetType= Convert.ToInt32(cacheTargetType).ToString();
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = targetType;
            else
                metadata.Item1 += $"|{targetType}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor  Method(this ServiceDescriptor descriptor, CachingMethod cachingMethod, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            var iCachingMethod = Convert.ToInt32(cachingMethod).ToString();
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = iCachingMethod;
            else
                metadata.Item1 += $"|{iCachingMethod}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor CacheSectionType(this ServiceDescriptor descriptor, string cacheSectionType, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = cacheSectionType;
            else
                metadata.Item1 += $"|{cacheSectionType}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor Force(this ServiceDescriptor descriptor, bool Force, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            var iForce = Convert.ToInt32(Force).ToString();
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = iForce;
            else
                metadata.Item1 += $"|{iForce}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor Key(this ServiceDescriptor descriptor, string key, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = key;
            else
                metadata.Item1 += $"|{key}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor Intercept(this ServiceDescriptor descriptor, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor L2Key(this ServiceDescriptor descriptor, string L2Key, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = L2Key;
            else
                metadata.Item1 += $"|{L2Key}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor EnableL2Cache(this ServiceDescriptor descriptor, bool enableL2Cache,string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            var iEnableL2Cache = Convert.ToInt32(enableL2Cache).ToString();
            if (string.IsNullOrEmpty(metadata.Item1))
                metadata.Item1 = iEnableL2Cache;
            else
                metadata.Item1 += $"|{iEnableL2Cache}";
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }

        public static ServiceDescriptor CorrespondingKeys(this ServiceDescriptor descriptor, string[] CorrespondingKeys, string metadataId)
        {
            var metadata = GetInterceptMetadata(descriptor, metadataId);
            if (CorrespondingKeys != null)
            {
                var correspondingKey = string.Join(",", CorrespondingKeys);
                if (string.IsNullOrEmpty(metadata.Item1))
                    metadata.Item1  = correspondingKey;
                else
                    metadata.Item1 += $"|{correspondingKey}";
            }
            else
            {
                metadata.Item1 += "|";
            }
            metadata.Item2[metadataId] = metadata.Item1;
            descriptor.Metadatas["Intercept"] = metadata.Item2;
            return descriptor;
        }
         

        public static ServiceCacheIntercept GetCacheIntercept(this ServiceDescriptor descriptor,string metadataId)
        {
          var metadata=  descriptor.GetMetadata<JObject>("Intercept",new JObject());
           if( metadata.ContainsKey(metadataId))
            {
                return new ServiceCacheIntercept(metadata[metadataId].ToString().Split("|"));
            }
            return default;
        }

        public static string GetIntercept(this ServiceDescriptor descriptor, string metadataId)
        {
            var metadata = descriptor.GetMetadata<JObject>("Intercept", new JObject());
            if (metadata.ContainsKey(metadataId))
            {
                return metadata[metadataId].ToString();
            }
            return null;
        }

        public static bool ExistIntercept(this ServiceDescriptor descriptor)
        {
            var metadata = descriptor.GetMetadata<JObject>("Intercept", null);
         
            return metadata!=null;
        }

        private static (string,Dictionary<string, object>) GetInterceptMetadata( ServiceDescriptor descriptor, string metadataId)
        {
            var result = "";
            var metadata = descriptor.GetMetadata<Dictionary<string, object>>("Intercept", new Dictionary<string, object>());
            if (metadata.ContainsKey(metadataId))
            {
                result= metadata[metadataId].ToString();
            }
            else
            {
                metadata.Add(metadataId, result);
            }
            return (result, metadata);
        }

    }
}
