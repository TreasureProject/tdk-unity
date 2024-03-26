using UnityEngine;

namespace Treasure
{
    [System.Serializable]
    public class LoginSettings
    {
        public bool hasSocialLogin = true;
        public bool hasGoogleLogin = true;
        public bool hasAppleLogin = true;
        public bool hasXLogin = true;
        [Space]
        public bool hasEmailLogin = true;
        public bool hasWalletLogin = true;
    }
}
