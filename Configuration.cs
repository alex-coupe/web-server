using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WebServer
{
    public class Configuration
    {
        [JsonPropertyName("listen")]
        public string[] Listen { get; set; } = [];
        [JsonPropertyName("documentRoot")]
        public string DocumentRoot { get; set; } = default!;
        [JsonPropertyName("keepAlive")]
        public bool KeepAlive { get; set; }
        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }
        [JsonPropertyName("maxConnections")]
        public int MaxConnections { get; set; }
        [JsonPropertyName("logBatchSize")]
        public int LogBatchSize { get; set; }
        [JsonPropertyName("logTimeThreshold")]
        public int LogTimeThreshold { get; set; }
        [JsonPropertyName("logMaxFileSize")]
        public int LogMaxFileSize { get; set; }
        [JsonPropertyName("cors")]
        public CORSConfig? CORS { get; set; }

        [JsonPropertyName("rateLimit")]
        public int RateLimit { get; set; }

        [JsonPropertyName("cleanupInterval")]
        public int CleanupInterval { get; set; }
    }

    public class CORSConfig
    {
        [JsonPropertyName("allowOptions")]
        public bool AllowOptions { get; set; }
        [JsonPropertyName("allowOrigin")]
        public string AllowOrigin { get; set; } = default!;
        [JsonPropertyName("allowMethods")]
        public string AllowMethods { get; set; } = default!;
        [JsonPropertyName("allowHeaders")]
        public string AllowHeaders { get; set; } = default!;
        [JsonPropertyName("allowAuth")]
        public bool AllowAuth { get; set; }
    }
}
