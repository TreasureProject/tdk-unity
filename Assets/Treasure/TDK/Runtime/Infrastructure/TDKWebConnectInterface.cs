using System;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Events;

namespace Treasure
{
    public class TDKWebConnectInterface : MonoBehaviour
    {
        public enum BrowserConnectionState
        {
            Disconnected, Connected, NewConnection
        }
        
        public static Action BrowserWalletConnectedAction;
        private static TDKWebConnectInterface _instance = null;
        private BrowserConnectionState _connectionState = BrowserConnectionState.Disconnected;

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
            if (IsActive())
            {
                WebGLNotifyReady();
            }
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

        public static BrowserConnectionState GetBrowserConnectionState()
        {
            return _instance._connectionState;
        }

        public static bool BrowserHasActiveWalletConnection()
        {
            return _instance._connectionState == BrowserConnectionState.Connected;
        }

#region Unity -> Browser
        public static void OpenConnectModal()
        {
            _instance._connectionState = BrowserConnectionState.NewConnection;
            TDKConnectUIManager.Instance.ShowTransitionModal(
                "",
                "Proceed with login in the popup",
                buttonText: "Cancel",
                buttonAction: () => {
                    TDK.Connect.HideConnectModal();
                }
            );
            if (TDK.AppConfig.ConnectModalMode == TDKConfig.ConnectUIModalMode.WebGLExternalForWalletLoginOnly)
            {
                WebGLOpenWalletConnectModal();
            }
            else
            {
                WebGLOpenConnectModal();
            }
        }

        public static void LogOut()
        {
            WebGLLogOut();
        }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void WebGLNotifyReady();

        [DllImport("__Internal")]
        private static extern void WebGLOpenConnectModal();
        
        [DllImport("__Internal")]
        private static extern void WebGLOpenWalletConnectModal();

        [DllImport("__Internal")]
        private static extern void WebGLLogOut();
        
        [DllImport("__Internal")]
        private static extern void WebGLRequestReconnect();
#else
        private static void WebGLNotifyReady() {}

        private static void WebGLOpenConnectModal() {}
        
        private static void WebGLOpenWalletConnectModal() {}

        private static void WebGLLogOut() {}
        
        private static void WebGLRequestReconnect() {}
#endif

#endregion

#region Browser -> Unity
        public void OnWalletConnected()
        {
            if (_connectionState == BrowserConnectionState.Disconnected)
            {
                // browser has notified that active wallet is connected before any user interaction
                _connectionState = BrowserConnectionState.Connected;
            }
            BrowserWalletConnectedAction?.Invoke();
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
