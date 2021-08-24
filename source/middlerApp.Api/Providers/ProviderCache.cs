using System;
using System.Collections.Concurrent;

namespace middlerApp.Api.Providers
{
    public class ProviderCache
    {

        private ConcurrentDictionary<string, IProviderCacheBucket> buckets = new ConcurrentDictionary<string, IProviderCacheBucket>();


        public ProviderCacheBucket<T> GetBucket<T>(string name)
        {
            return (ProviderCacheBucket<T>)buckets.GetOrAdd(name, new ProviderCacheBucket<T>());
        }
    }

    public interface IProviderCacheBucket
    {
        
    }

    public class ProviderCacheBucket<T>: IProviderCacheBucket
    {
        private ConcurrentDictionary<string, T> bucket = new ConcurrentDictionary<string, T>();

        public T GetOrAdd(string key, Func<string, T> factory)
        {
            return bucket.GetOrAdd(key, factory);
        }
    }
}
