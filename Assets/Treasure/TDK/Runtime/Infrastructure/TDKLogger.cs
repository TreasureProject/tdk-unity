using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;

namespace Treasure
{
    public static class TDKLogger
    {
        public static Action<string> ExternalLogCallback;

        public static bool quitting = false;

        public static void LogDebug(string message)
        {
            LogByLevel(TDKConfig.LoggerLevelValue.DEBUG, message);
        }

        public static void LogInfo(string message)
        {
            LogByLevel(TDKConfig.LoggerLevelValue.INFO, message);
        }

        public static void LogWarning(string message)
        {
            LogByLevel(TDKConfig.LoggerLevelValue.WARNING, message);
        }

        public static void LogError(string message)
        {
            LogByLevel(TDKConfig.LoggerLevelValue.ERROR, message);
        }

        // Alias to LogInfo
        public static void Log(string message)
        {
            LogInfo(message);
        }

        private static void LogByLevel(TDKConfig.LoggerLevelValue loggerLevelValue, string message) {
            if (quitting) {
                UnityEngine.Debug.Log($"[{loggerLevelValue}] {message}");
                return;
            }
            if (TDK.Instance.AppConfig.LoggerLevel > loggerLevelValue) {
                return;
            }
            TDKMainThreadDispatcher.Instance.Enqueue(() => ExternalLogCallback?.Invoke(message));
            #if UNITY_EDITOR
            switch (loggerLevelValue)
            {
                case TDKConfig.LoggerLevelValue.ERROR:
                    UnityEngine.Debug.LogError(message);
                    break;
                case TDKConfig.LoggerLevelValue.WARNING:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                default:
                    UnityEngine.Debug.Log(message);
                    break;
            }
            #else
            TDKMainThreadDispatcher.Instance.Enqueue(() => LogMessageInternal(message));
            #endif
        }

        private static void LogMessageInternal(string message)
        {
            try {
                UnityEngine.Debug.Log(string.Format("[{0}] {1}", Time.realtimeSinceStartup, message));
            } catch {
                // no-op
            }
        }
    }
}
