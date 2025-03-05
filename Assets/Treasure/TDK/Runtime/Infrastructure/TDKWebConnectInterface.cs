using System;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Events;

namespace Treasure
{
    public class TDKWebConnectInterface : MonoBehaviour
    {
        public static Action<bool> BrowserWalletConnectedAction;
        private static TDKWebConnectInterface _instance = null;
        private bool _isReusingConnection = true;

        // create a GameObject to grab stuff from the queue
        public static void Initialize() {
            if (_instance == null) {
                _instance = FindObjectOfType(typeof(TDKWebConnectInterface)) as TDKWebConnectInterface;
                
                if (_instance == null)
                {
                    // create a new instance
                    _instance = new GameObject("TDKWebConnectInterface", new Type[] {
                        typeof(TDKWebConnectInterface),
                    }).GetComponent<TDKWebConnectInterface>();

                    DontDestroyOnLoad(_instance.gameObject);
                }
            }
            WebGLNotifyReady();
        }

        public static bool IsActive() 
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public static void AttemptReconnect()
        {
            WebGLRequestReconnect();
        }

#region Unity -> Browser
        public static void OpenConnectModal()
        {
            _instance._isReusingConnection = false;
            TDKConnectUIManager.Instance.ShowTransitionModal(
                "",
                "Proceed with login in the popup",
                buttonText: "Cancel",
                buttonAction: () => {
                    TDK.Connect.HideConnectModal();
                }
            );
            WebGLOpenConnectModal();
        }

        public static void LogOut()
        {
            WebGLLogOut();
        }

        [DllImport("__Internal")]
        private static extern void WebGLNotifyReady();

        [DllImport("__Internal")]
        private static extern void WebGLOpenConnectModal();

        [DllImport("__Internal")]
        private static extern void WebGLLogOut();
        
        [DllImport("__Internal")]
        private static extern void WebGLRequestReconnect();
#endregion

#region Browser -> Unity
        public void OnWalletConnected()
        {
            BrowserWalletConnectedAction.Invoke(_isReusingConnection);
        }

        // passing params together since SendMessage only accepts 1 arg
        public void OnConnectViaCookie(string authMethodAndCookie)
        {
            var splitResult = authMethodAndCookie.Split("@");
            var authMethod = TreasureLauncherUtils.ParseAuthProviderString(splitResult[0]);
            var authCookie = splitResult[1];
            _ = TDK.Connect.ConnectViaCookie(authCookie, authMethod.Value);
        }

        public void OnConnectViaCookieError(string error)
        {
            TDKConnectUIManager.Instance.GetTransitionModal().SetInfoLabels(
                "An error occurred while trying to connect",
                error
            );
        }
#endregion
    }
}
