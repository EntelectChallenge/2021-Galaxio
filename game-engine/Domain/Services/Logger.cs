using System;
using System.Collections.Generic;

namespace Domain.Services
{
    public static class Logger
    {
        private static readonly List<string> logLevel = GetLogLevels();

        private static List<string> GetLogLevels()
        {
            var envarLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
            if (string.IsNullOrWhiteSpace(envarLevel))
            {
                return new List<string>
                {
                    "DATA",
                    "DEBUG",
                    "INFO",
                    "WARNING",
                    "ERROR"
                };
            }

            return envarLevel.ToLowerInvariant() switch
            {
                "data" => new List<string>
                {
                    "DATA",
                    "DEBUG",
                    "INFO",
                    "WARNING",
                    "ERROR"
                },
                "debug" => new List<string>
                {
                    "DATA",
                    "DEBUG",
                    "INFO",
                    "WARNING",
                    "ERROR"
                },
                "info" => new List<string>
                {
                    "INFO",
                    "WARNING",
                    "ERROR"
                },
                "warning" => new List<string>
                {
                    "WARNING",
                    "ERROR"
                },
                "error" => new List<string>
                {
                    "ERROR"
                },
                _ => new List<string>
                {
                    "DATA",
                    "DEBUG",
                    "INFO",
                    "WARNING",
                    "ERROR"
                }
            };
        }

        private static void Log(
            ConsoleColor color,
            string level,
            string tag,
            object data)
        {
            if (!logLevel.Contains(level))
            {
                return;
            }

            Console.ForegroundColor = color;
            Console.WriteLine($"[{level}] [{tag}]: {data}");
            Console.ResetColor();
        }

        // TODO make this better so it can be injected and use the class name of the file it is Injected to?
        public static void LogData(object data) => LogDebug("DATA", data);
        public static void LogDebug(string tag, object data) => Log(ConsoleColor.Blue, "DEBUG", tag, data);
        public static void LogInfo(string tag, object data) => Log(ConsoleColor.Green, "INFO", tag, data);
        public static void LogError(string tag, object data) => Log(ConsoleColor.Red, "ERROR", tag, data);
        public static void LogWarning(string tag, object data) => Log(ConsoleColor.Yellow, "WARNING", tag, data);

        public static void LogDebug(Guid botId, string tag, object data) => LogDebug($"{GetStartId(botId)}: " + tag, data);

        private static string GetStartId(Guid id) => id.ToString().Substring(0, 4);
    }
}