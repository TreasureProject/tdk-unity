using UnityEngine;

namespace Treasure
{
    [System.Serializable]
    public class LoginSettings
    {
        public bool hasGoogleLogin = true;
        public bool hasAppleLogin = true;
        public bool hasFacebookLogin = true;
        [Space]
        public bool hasEmailLogin = true;
        public bool hasWalletLogin = true;

        public bool HasSocialLogin() => hasGoogleLogin || hasAppleLogin || hasFacebookLogin;
    }
}
