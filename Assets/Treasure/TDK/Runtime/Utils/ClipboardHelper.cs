using System.Runtime.InteropServices;
using UnityEngine;

public static class ClipboardHelper
{
    [DllImport("__Internal")]
    private static extern void CopyToClipboard(string text);

    public static void Copy(string text)
    {
#if UNITY_WEBGL
        CopyToClipboard(text);
#else
        GUIUtility.systemCopyBuffer = text;
#endif
    }
}