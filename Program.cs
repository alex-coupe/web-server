using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebServer;
class Program
{
    /// <summary>
    /// TODO:
    /// 1. Routing
    /// 2. Request Validation
    /// 3. Dynamix Content Generation
    /// 4. Security
    /// 5. Caching
    /// 7. Content Type Negotiation
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    
   
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
                try
                {
                    if (context.Response.StatusCode == 200)
                    {
                        ResponseWriter.Write(context);
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
}