using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace WebServer
{
    public class Middleware
    {
        private static Dictionary<string, (int, DateTime)> requestTracker = [];
        private static int RATE_LIMIT = ConfigManager.Configuration.RateLimit;
        private static int CLEANUP_INTERVAL = ConfigManager.Configuration.CleanupInterval; 
        private static readonly CancellationTokenSource cancellationTokenSource = new();
       
        public static void Init()
        {
            CleanupTask();
        }
        private static void CleanupTask()
        {
            Task.Run(() =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    DateTime currentTime = DateTime.UtcNow;
                   
                    var keysToRemove = new List<string>();
                    foreach (var entry in requestTracker)
                    {
                        TimeSpan timeDiff = currentTime - entry.Value.Item2;
                        if (timeDiff.TotalSeconds > CLEANUP_INTERVAL) 
                        {
                            keysToRemove.Add(entry.Key);
                        }
                    }
                    foreach (var key in keysToRemove)
                    {
                        requestTracker.Remove(key);
                    }
                }
            }, cancellationTokenSource.Token);
        }
    
        public static void HandleRateLimiting(HttpListenerContext context)
        {
            DateTime currentTime = DateTime.UtcNow;
            var clientIp = context.Request.RemoteEndPoint.Address.ToString();
            if (requestTracker.TryGetValue(clientIp, out (int, DateTime) value))
            {
                (int requestCount, DateTime lastRequestTime) = value;
                TimeSpan timeDiff = currentTime - lastRequestTime;
                if (timeDiff.TotalSeconds > 60)
                {
                    requestCount = 0;
                }
                requestCount++;
                requestTracker[clientIp] = (requestCount, currentTime);
                if (requestCount > RATE_LIMIT)
                {
                    context.Response.StatusCode = 429;
                    context.Response.Close();
                } 
            }
            else
            {
                requestTracker.Add(clientIp, (1, currentTime));
            }
        }

        public static void HandleCors(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "OPTIONS" && ConfigManager.Configuration.CORS != null 
                && ConfigManager.Configuration.CORS.AllowOptions)
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", ConfigManager.Configuration.CORS.AllowOrigin);
                context.Response.Headers.Add("Access-Control-Allow-Methods", ConfigManager.Configuration.CORS.AllowMethods);
                context.Response.Headers.Add("Access-Control-Allow-Headers", ConfigManager.Configuration.CORS.AllowHeaders);
                context.Response.Headers.Add("Access-Control-Allow-Credentials", ConfigManager.Configuration.CORS.AllowAuth ? "True" : "False");

                context.Response.StatusCode = 200;
                context.Response.Close();
            }
            else if (context.Request.HttpMethod == "OPTIONS" && ConfigManager.Configuration.CORS != null
                && !ConfigManager.Configuration.CORS.AllowOptions)
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
            }
        }
        
    } 
}
