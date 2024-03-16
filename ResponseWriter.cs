using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public static class ResponseWriter
    {
        public static void Write(HttpListenerContext ctx)
        {
            if (ctx != null)
            {
                var filepath = GetFilePath(ctx);

                if (File.Exists(filepath))
                {
                    Logger.Log($"Serving requested resource - {filepath}");
                    byte[] responseBytes = File.ReadAllBytes(filepath);
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    ctx.Response.ContentType = DetermineContentType(filepath);
                    ctx.Response.ContentLength64 = responseBytes.Length;
                    ctx.Response.OutputStream.Write(responseBytes);
                }
                else
                {
                    Logger.Log($"Resource not found - {filepath}");
                    var notFound = Path.Combine(Directory.GetCurrentDirectory(), "serverpages/notfound.html");
                    byte[] responseBytes = File.ReadAllBytes(notFound);
                    ctx.Response.ContentType = "text/html";
                    ctx.Response.ContentLength64 = responseBytes.Length;
                    ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    ctx.Response.OutputStream.Write(responseBytes);
                }
                ctx.Response.ContentEncoding = Encoding.UTF8;
                ctx.Response.Close();
            }
            else
                throw new ArgumentNullException(nameof(ctx));
        }

        private static string GetFilePath(HttpListenerContext context)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigManager.Configuration.DocumentRoot + context.Request?.Url?.LocalPath);
            if (filePath.EndsWith(ConfigManager.Configuration.DocumentRoot+"/"))
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigManager.Configuration.DocumentRoot + "/index.html");
            }
            return filePath;
        }

        private static string DetermineContentType(string filepath)
        {
            var filetype = Path.GetExtension(filepath).ToLower();
            return filetype switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                _ => "application/octet-stream",
            };
        }
    }
}
