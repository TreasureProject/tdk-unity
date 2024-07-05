using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;

namespace Treasure
{
    public static class TDKLogger
    {
        public static bool verboseLogging = true; //TODO move to config driven?

        public static Action<String> ExternalLogCallback;

        public static void Log(string message, bool logOnMainThread=true)
        {
            if (TDK.Instance.AppConfig.LoggerLevel > TDKConfig.LoggerLevelValue.INFO) {
                return;
            }
            #if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
            TDKMainThreadDispatcher.Instance.Enqueue(() => ExternalLogCallback?.Invoke(message));
            #else
            if(logOnMainThread)
            {
                TDKMainThreadDispatcher.Instance.Enqueue(LogCoroutine(message));
            }
            else
            {
                LogInternal(message);
            }
            #endif
        }

        private static IEnumerator LogCoroutine(string message)
        {
            LogInternal(message);
            yield return null;
        }

        private static void LogInternal(string message)
        {
            if (TDK.Instance.AppConfig.LoggerLevel > TDKConfig.LoggerLevelValue.DEBUG) {
                return;
            }
            if(!string.IsNullOrEmpty(message))
            {
                try {
                    #if UNITY_EDITOR
                    StackFrame sf = new StackFrame(1);
                    var method = sf.GetMethod();
                    UnityEngine.Debug.Log(string.Format("[{0}] {1}:{2}", Time.realtimeSinceStartup, method.DeclaringType, message));
                    #else
                    UnityEngine.Debug.Log(string.Format("[{0}] {1}", Time.realtimeSinceStartup, message));
                    #endif
                    TDKMainThreadDispatcher.Instance.Enqueue(() => ExternalLogCallback?.Invoke(message));
                }
                catch
                {
                    // no-op
                }
            }
        }

        public static void LogWarning(string message, bool logOnMainThread=true)
        {
            if (TDK.Instance.AppConfig.LoggerLevel > TDKConfig.LoggerLevelValue.WARNING) {
                return;
            }
            #if UNITY_EDITOR || TP_UA
            UnityEngine.Debug.LogWarning(message);
            TDKMainThreadDispatcher.Instance.Enqueue(() => ExternalLogCallback?.Invoke(message));
            #else
            if(logOnMainThread)
            {
                TDKMainThreadDispatcher.Instance.Enqueue(LogWarningCoroutine(message));
            }
            else
            {
                LogWarningInternal(message);
            }
            #endif
        }

        private static IEnumerator LogWarningCoroutine(string message)
        {
            LogWarningInternal(message);
            yield return null;
        }

        private static void LogWarningInternal(string message)
        {
            if (TDK.Instance.AppConfig.LoggerLevel > TDKConfig.LoggerLevelValue.DEBUG) {
                return;
            }
            if(!string.IsNullOrEmpty(message))
            {
                try {
                    #if UNITY_EDITOR
                    StackFrame sf = new StackFrame(1);
                    var method = sf.GetMethod();
                    UnityEngine.Debug.LogWarning(string.Format("[{0}] {1}:{2}", Time.realtimeSinceStartup, method.DeclaringType, message));
                    #else
                    UnityEngine.Debug.Log(string.Format("[{0}] {1}", Time.realtimeSinceStartup, message));
                    #endif
                }
                catch
                {
                    // no-op
                }
            }
        }

        public static void LogError(string message, bool logOnMainThread=true)
        {
            if (TDK.Instance.AppConfig.LoggerLevel > TDKConfig.LoggerLevelValue.ERROR) {
                return;
            }
            #if UNITY_EDITOR || TP_UA
            UnityEngine.Debug.LogError(message);
            TDKMainThreadDispatcher.Instance.Enqueue(() => ExternalLogCallback?.Invoke(message));
            #else
            if(logOnMainThread)
            {
                TDKMainThreadDispatcher.Instance.Enqueue(LogErrorCoroutine(message));
            }
            else
            {
                LogErrorInternal(message);
            }
            #endif
        }

        private static IEnumerator LogErrorCoroutine(string message)
        {
            LogErrorInternal(message);
            yield return null;
        }

        private static void LogErrorInternal(string message)
        {
            if (TDK.Instance.AppConfig.LoggerLevel > TDKConfig.LoggerLevelValue.ERROR) {
                return;
            }
            if(!string.IsNullOrEmpty(message))
            {
                try {
                    #if UNITY_EDITOR
                    StackFrame sf = new StackFrame(1);
                    var method = sf.GetMethod();
                    UnityEngine.Debug.LogError(string.Format("[{0}] {1}:{2}", Time.realtimeSinceStartup, method.DeclaringType, message));
                    #else
                    UnityEngine.Debug.Log(string.Format("[{0}] {1}", Time.realtimeSinceStartup, message));
                    #endif
                }
                catch
                {
                    // no-op
                }
            }
        }
    }
}
