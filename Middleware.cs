using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public class Middleware
    {
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
