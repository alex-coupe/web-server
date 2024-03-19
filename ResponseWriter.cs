using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public static class ResponseWriter
    {
        public static byte[] CompressResponse(byte[] responseBytes, HttpListenerContext ctx)
        {
            ctx.Response.AddHeader("Content-Encoding", "gzip");
            using MemoryStream outputStream = new();
            using (GZipStream gzipStream = new(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(responseBytes, 0, responseBytes.Length);
            }
            return outputStream.ToArray();
        }
        
        public static void Write(HttpListenerContext ctx)
        {
            if (ctx != null)
            {
                var filepath = GetFilePath(ctx);
                byte[] responseBytes;
                if (Cache.CacheEntries.TryGetValue(filepath, out CacheEntry? value) && !Cache.IsCacheEntryExpired(value))
                {
                    Logger.Log("File found in cache: " + filepath);

                    Cache.CacheEntries[filepath].LastAccessTime = DateTime.Now;
                    responseBytes = Cache.CacheEntries[filepath].Content;
                    ctx.Response.AddHeader("Content-Encoding", "gzip");
                    Middleware.SetSecurityHeaders(ctx);
                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                    ctx.Response.ContentType = DetermineContentType(filepath);
                    ctx.Response.ContentLength64 = responseBytes.Length;
                    ctx.Response.OutputStream.Write(responseBytes);
                }
                else
                {

                    if (File.Exists(filepath))
                    {
                        Logger.Log($"Serving requested resource - {filepath}");
                        responseBytes = File.ReadAllBytes(filepath);
                        var compressedBytes = CompressResponse(responseBytes, ctx);
                        Cache.CacheFile(filepath, compressedBytes);
                        Middleware.SetSecurityHeaders(ctx);
                        ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                        ctx.Response.ContentType = DetermineContentType(filepath);
                        ctx.Response.ContentLength64 = compressedBytes.Length;
                        ctx.Response.OutputStream.Write(compressedBytes);
                    }
                    else
                    {
                        Logger.Log($"Resource not found - {filepath}");
                        var notFound = Path.Combine(Directory.GetCurrentDirectory(), "serverpages/notfound.html");
                        responseBytes = File.ReadAllBytes(notFound);
                        var compressedBytes = CompressResponse(responseBytes, ctx);
                        ctx.Response.AddHeader("Content-Encoding", "gzip");
                        Middleware.SetSecurityHeaders(ctx);
                        ctx.Response.ContentType = "text/html";
                        ctx.Response.ContentLength64 = compressedBytes.Length;
                        ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        ctx.Response.OutputStream.Write(compressedBytes);
                    }
                }
                ctx.Response.Close();
            }
            else
                throw new ArgumentNullException(nameof(ctx));
        }

        private static string GetFilePath(HttpListenerContext context)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), ConfigManager.Configuration.DocumentRoot + context.Request?.Url?.LocalPath);
            var normalizedPath = Path.GetFullPath(filePath);
            if (normalizedPath.EndsWith($"{ConfigManager.Configuration.DocumentRoot}\\"))
            {
                normalizedPath = Path.Combine(Directory.GetCurrentDirectory(), ConfigManager.Configuration.DocumentRoot + "/index.html");
            }
            return normalizedPath;
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
