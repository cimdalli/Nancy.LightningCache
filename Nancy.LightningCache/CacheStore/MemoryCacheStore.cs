using System;
using System.Collections.Generic;
using System.Linq;
using Nancy.LightningCache.Projection;

namespace Nancy.LightningCache.CacheStore
{
    public class MemoryCacheStore : ICacheStore
    {
        private static readonly Dictionary<string, SerializableResponse> Cache = new Dictionary<string, SerializableResponse>();
        private readonly object _lock = new object();

        public CachedResponse Get(string key)
        {
            lock (_lock)
            {
                if (Cache.ContainsKey(key))
                {
                    return new CachedResponse(Cache[key]);
                }
            }
            return null;
        }

        public void Set(string key, NancyContext context, DateTime absoluteExpiration)
        {
            lock (_lock)
            {
                if (Cache.ContainsKey(key))
                {
                    Remove(key);
                }

                Cache.Add(key, new SerializableResponse(context.Response, absoluteExpiration));

                Cache.Where(pair => pair.Value.Expiration < DateTime.Now).Select(pair => pair.Key).ToList().ForEach(Remove);
            }
        }

        public void Remove(string key)
        {
            lock (_lock)
            {
                if (Cache.ContainsKey(key))
                {
                    Cache.Remove(key);
                }
            }
        }
    }
}
