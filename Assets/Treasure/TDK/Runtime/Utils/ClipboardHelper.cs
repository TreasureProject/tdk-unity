using System.Runtime.InteropServices;
using UnityEngine;

public static class ClipboardHelper
{
#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void CopyToClipboard(string text);
#endif

    public static void Copy(string text)
    {
#if UNITY_WEBGL
        CopyToClipboard(text);
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}