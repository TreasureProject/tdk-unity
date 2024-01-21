using System;
using UnityEngine;

namespace Thirdweb
{
    public static class ThirdwebDebug
    {
        public static bool IsEnabled => ThirdwebManager.Instance != null && ThirdwebManager.Instance.showDebugLogs;

        public static void Log(object message)
        {
            if (IsEnabled)
                Debug.Log(message);
        }

        public static void LogWarning(object message)
        {
            if (IsEnabled)
                Debug.LogWarning(message);
        }

        public static void LogError(object message)
        {
            if (IsEnabled)
                Debug.LogError(message);
        }

        public static void LogException(Exception exception)
        {
            if (IsEnabled)
                Debug.LogException(exception);
        }
    }
}
