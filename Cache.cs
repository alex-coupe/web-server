using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public static class Cache
    {
        public static Dictionary<string, CacheEntry> CacheEntries = [];
        private static int maxCacheSize; 
        private static TimeSpan cacheExpiration;

        public static void Init(int maxCacheSize, int cacheExpirationMinutes)
        {
            Cache.maxCacheSize = maxCacheSize;
            cacheExpiration = TimeSpan.FromMinutes(cacheExpirationMinutes);
        }

        public static bool IsCacheEntryExpired(CacheEntry entry)
        {
            return (DateTime.Now - entry.LastAccessTime) > cacheExpiration;
        }

        public static void CacheFile(string filename, byte[] content)
        {
            if (CacheEntries.Count >= maxCacheSize)
            {
                EvictLRUEntry();
            }
            CacheEntries[filename] = new CacheEntry(content, DateTime.Now);
        }

        private static void EvictLRUEntry()
        {
            string lruFilename = "";
            DateTime minLastAccessTime = DateTime.MaxValue;
            foreach (var entry in CacheEntries)
            {
                if (entry.Value.LastAccessTime < minLastAccessTime)
                {
                    lruFilename = entry.Key;
                    minLastAccessTime = entry.Value.LastAccessTime;
                }
            }
            CacheEntries.Remove(lruFilename);
            Logger.Log("Evicted least recently used entry from cache: " + lruFilename);
        }
    }
}
