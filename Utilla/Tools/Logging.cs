using BepInEx.Logging;

namespace Utilla.Tools
{
    internal class Logging
    {
        public static void Message(object data) => Log(data, LogLevel.Message);

        public static void Info(object data) => Log(data, LogLevel.Info);

        public static void Warning(object data) => Log(data, LogLevel.Warning);

        public static void Error(object data) => Log(data, LogLevel.Error);

        public static void Fatal(object data) => Log(data, LogLevel.Fatal);

        public static void Log(object data, LogLevel level = LogLevel.Info)
        {
#if DEBUG
            Plugin.Logger?.Log(level, data);
#endif
        }
    }
}
