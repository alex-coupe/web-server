using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServer
{
    public static class Logger
    {
        private static readonly ConcurrentQueue<string> logQueue = new();
        private static string logFilePath = "";
        private static int batchSize;
        private static int timeThreshold;
        private static int maxFileSize;
        private static readonly CancellationTokenSource cancellationTokenSource = new();
        private static Task loggingTask = Task.CompletedTask;
        private static long currentFileSize;
        private static int roll = 1;
        private static DateTime lastLogWrite = DateTime.Now;
        public static void Init(string logFilePath, int batchSize, int timeThreshold, int maxFileSize)
        {
            Logger.logFilePath = logFilePath;
            Logger.batchSize = batchSize;
            Logger.timeThreshold = timeThreshold;
            Logger.maxFileSize = maxFileSize;
            StartLoggingTask();
        }
        public static void Log(string message)
        {
            logQueue.Enqueue(message);
        }
        private static void StartLoggingTask()
        {
            loggingTask = Task.Run(async () =>
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    TimeSpan timeDifference = DateTime.Now - lastLogWrite;
                    if (logQueue.Count >= batchSize || timeDifference.Seconds >= timeThreshold)
                    {
                        await WriteLogsToFile();
                    }
                }
            }, cancellationTokenSource.Token);
        }
        private static async Task WriteLogsToFile()
        {
            try
            {
                string[] logsToWrite = [.. logQueue];
                logQueue.Clear();

                if (File.Exists(logFilePath))
                {
                    var fileInfo = new FileInfo(logFilePath);
                    currentFileSize = fileInfo.Length;
                }

                if (currentFileSize >= maxFileSize)
                {
                    // Roll the log file by appending a timestamp to the file name
                    string rolledLogFilePath = $"{Path.GetDirectoryName(logFilePath)}\\{Path.GetFileNameWithoutExtension(logFilePath)}_{DateTime.Now:yyyyMMddHHmmss}_{roll}{Path.GetExtension(logFilePath)}";
                    File.Move(logFilePath, rolledLogFilePath);
                    roll++;
                    currentFileSize = 0; // Reset current file size after rolling
                }
                await File.AppendAllLinesAsync(logFilePath, logsToWrite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing logs to file: {ex.Message}");
            }
        }
        public static void Dispose()
        {
            cancellationTokenSource.Cancel();
            loggingTask.Wait();
            cancellationTokenSource.Dispose();
        }
    }
}
