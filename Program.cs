using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebServer;
class Program
{
    /// TODO:
    /// 1. Routing & Dynamic Content Generation
    /// 2. Caching
     
    static async Task Main(string[] args)
    {
        ConfigManager.Init();
        Logger.Init("./logfile.txt",ConfigManager.Configuration.LogBatchSize,ConfigManager.Configuration.LogTimeThreshold, ConfigManager.Configuration.LogMaxFileSize);
        Logger.Log("Logger Initialised, booting server");
        // Set up the HTTP listener
        var listener = new HttpListener();
        listener.TimeoutManager.RequestQueue = TimeSpan.FromSeconds(ConfigManager.Configuration.Timeout);
        listener.Prefixes.Add(ConfigManager.Configuration.Listen[0]);
        listener.Start();
        Logger.Log("Server started. Listening for connections...");
        // Set up cancellation token
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        Middleware.Init();
        Cache.Init(ConfigManager.Configuration.MaxCacheEntries, ConfigManager.Configuration.CacheExpiration);
        // Handle incoming requests asynchronously
        await HandleRequestsAsync(listener, cancellationToken);
        // Clean up
        listener.Close();
        Logger.Log("Server stopped.");
    }
    static async Task HandleRequestsAsync(HttpListener listener, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for an incoming request
                var context = await listener.GetContextAsync();
                Logger.Log($"{context.Request.RemoteEndPoint.Address}:{context.Request.RemoteEndPoint.Port} - - [{DateTime.Now}]\"{context.Request.HttpMethod} {context.Request.Url} {context.Request.UserAgent}\"");
                Middleware.HandleRateLimiting(context);
                Middleware.HandleCors(context);
                               
                string selectedContentType = Middleware.GetBestContentMatch(Middleware.GetRequestedContentTypes(context), GetAcceptedType());
                try
                {
                    if (!string.IsNullOrEmpty(selectedContentType) && context.Response.StatusCode == 200)
                    {
                        ResponseWriter.Write(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 406;
                        context.Response.Close();
                    }
                } 
                catch(Exception ex)
                {
                    Logger.Log($"Error processing response {ex.Message} - {ex.StackTrace}");
                }
            }
            Logger.Log("Server shutting down");
        }
        catch (ArgumentNullException ane)
        {
            Logger.Log($"Request context is null! {ane.Message} - {ane.InnerException}");
        }
    }

    private static string[] GetAcceptedType()
    {
        return ["*/*","text/html","text/css","text/javascript","application/javascript","application/json","text/plain"];
    }
}