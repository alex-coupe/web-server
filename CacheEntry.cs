using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public class CacheEntry(byte[] content, DateTime lastAccessTime)
    {
        public byte[] Content { get; } = content;
        public DateTime LastAccessTime { get; set; } = lastAccessTime;
    }
}
