using System;

namespace Hyperplasia.Core.Diagnostics
{
    public enum CoreLogLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Core 层日志出口。
    /// Core 只负责发日志，实际写到哪里由 Unity 层注入 Sink 决定。
    /// </summary>
    public static class CoreLog
    {
        public static bool Enabled = true;

        /// <summary>由组装层注入的日志出口；为空时日志被丢弃。</summary>
        public static Action<CoreLogLevel, string> Sink;

        public static void Info(string message)
        {
            Write(CoreLogLevel.Info, message);
        }

        public static void Warning(string message)
        {
            Write(CoreLogLevel.Warning, message);
        }

        public static void Error(string message)
        {
            Write(CoreLogLevel.Error, message);
        }

        private static void Write(CoreLogLevel level, string message)
        {
            if (!Enabled) return;

            Action<CoreLogLevel, string> sink = Sink;
            if (sink == null) return;

            sink(level, message);
        }
    }
}
