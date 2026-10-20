using Hyperplasia.Core.Diagnostics;
using UnityEngine;

namespace Hyperplasia.Utils
{
    /// <summary>
    /// 把 Core 日志转发到 Unity Console 的适配器，由 GameBootstrap 注入。
    /// </summary>
    public sealed class UnityCoreLogSink
    {
        private const string Prefix = "[Core] ";

        public void Write(CoreLogLevel level, string message)
        {
            switch (level)
            {
                case CoreLogLevel.Warning:
                    Debug.LogWarning(Prefix + message);
                    break;
                case CoreLogLevel.Error:
                    Debug.LogError(Prefix + message);
                    break;
                default:
                    Debug.Log(Prefix + message);
                    break;
            }
        }
    }
}
