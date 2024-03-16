using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebServer;
class Program
{
    /// <summary>
    /// TODO:
    /// 1. Error Handling
    /// 2. Multithreading
    /// 3. Routing
    /// 4. Request Validation
    /// 5. Dynamix Content Generation
    /// 7. Security
    /// 8. Caching
    /// 9. Configuration
    /// 10. Rate Limiting
    /// 11. Compression
    /// 12. Content Type Negotiation
    /// 13. CORS
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task Main(string[] args)
    {
        Logger.Init("./logfile.txt",5,new TimeSpan(0,0,5),300);
        Logger.Log("Logger Initialised, booting server");
        // Set up the HTTP listener
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Logger.Log("Server started. Listening for connections...");
        // Set up cancellation token
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
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
                ResponseWriter.Write(context);
            }
            Logger.Log("Server shutting down");
        }
        catch (ArgumentNullException ane)
        {
            Logger.Log($"Request context is null! {ane.Message} - {ane.InnerException}");
        }
    }
}