using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
class Program
{
    static async Task Main(string[] args)
    {
        // Set up the HTTP listener
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Server started. Listening for connections...");
        // Set up cancellation token
        var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;
        // Handle incoming requests asynchronously
        await HandleRequestsAsync(listener, cancellationToken);
        // Clean up
        listener.Close();
        Console.WriteLine("Server stopped.");
    }
    static async Task HandleRequestsAsync(HttpListener listener, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for an incoming request
                var context = await listener.GetContextAsync();
                // Process the request
                await ProcessRequestAsync(context);
            }
        }
        catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
        {
            // Ignore HttpListenerException when cancellation is requested
        }
    }
    static async Task ProcessRequestAsync(HttpListenerContext context)
    {
        // Create a response
        string responseString = "Hello, World!";
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
        // Send the response
        context.Response.ContentType = "text/plain";
        context.Response.ContentLength64 = responseBytes.Length;
        await context.Response.OutputStream.WriteAsync(responseBytes);
        context.Response.Close();
    }
}