using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thirdweb;
using Thirdweb.EWS;
using Thirdweb.Wallets;
using UnityEngine;
using UnityEngine.Events;

namespace Treasure
{
    public class TDKEmbeddedWalletUI : EmbeddedWalletUI
    {
        [Space]
        [Tooltip("Invoked when the user completes OTP process.")]
        public UnityEvent OnEmailOTPVerificationSuccess;
        [Space]
        [SerializeField] private ConfirmLoginModal confirmLoginModal;

        private void Start()
        {
            OnEmailOTPVerificationFailed.AddListener(() =>
            {
                SetOtpCodeIsWrong();
            });
        }

        public override async Task LoginWithOTP()
        {
            if (_email == null)
                throw new UnityException("Email is required for OTP login");

            SubmitButton.onClick.AddListener(() =>
            {
                if (confirmLoginModal.CheckConfirmationCodeIsValid())
                {
                    SubmitButton.GetComponent<LoadingButton>().SetLoading(true);
                    OnSubmitOTP();
                }              
            });

            await OnSendOTP();
            TDKIdentityUIManager.Instance.ShowConfirmLoginModal();
        }

        public override async Task<User> Connect(EmbeddedWallet embeddedWallet, string email, AuthOptions authOptions)
        {
            var config = Resources.Load<ThirdwebConfig>("ThirdwebConfig");
            _customScheme = config != null ? config.customScheme : null;
            if (!string.IsNullOrEmpty(_customScheme))
                _customScheme = _customScheme.EndsWith("://") ? _customScheme : $"{_customScheme}://";
            _embeddedWallet = embeddedWallet;
            _email = email;
            _user = null;
            _exception = null;
            OTPInput.text = "";
            RecoveryInput.text = "";
            RecoveryInput.gameObject.SetActive(false);
            SubmitButton.onClick.RemoveAllListeners();
            RecoveryCodesCopy.onClick.RemoveAllListeners();
            EmbeddedWalletCanvas.SetActive(false);
            RecoveryCodesCanvas.SetActive(false);

            try
            {
                string authProvider = authOptions.authProvider switch
                {
                    AuthProvider.EmailOTP => "EmailOTP",
                    AuthProvider.Google => "Google",
                    AuthProvider.Apple => "Apple",
                    AuthProvider.Facebook => "Facebook",
                    AuthProvider.JWT => "CustomAuth",
                    _ => throw new UnityException($"Unsupported auth provider: {authOptions.authProvider}"),
                };
                return await _embeddedWallet.GetUserAsync(_email, authProvider);
            }
            catch (Exception e)
            {
                ThirdwebDebug.Log($"Could not recreate user automatically, proceeding with auth: {e.Message}");
            }

            try
            {
                switch (authOptions.authProvider)
                {
                    case AuthProvider.EmailOTP:
                        await LoginWithOTP();
                        break;
                    case AuthProvider.Google:
                        await LoginWithOauth("Google");
                        break;
                    case AuthProvider.Apple:
                        await LoginWithOauth("Apple");
                        break;
                    case AuthProvider.Facebook:
                        await LoginWithOauth("Facebook");
                        break;
                    case AuthProvider.JWT:
                        await LoginWithJWT(authOptions.jwtOrPayload, authOptions.encryptionKey);
                        break;
                    case AuthProvider.AuthEndpoint:
                        await LoginWithAuthEndpoint(authOptions.jwtOrPayload, authOptions.encryptionKey);
                        break;
                    default:
                        throw new UnityException($"Unsupported auth provider: {authOptions.authProvider}");
                }
            }
            catch (Exception e)
            {
                _exception = e;
            }

            await new WaitUntil(() => _user != null || _exception != null);

            //(TODO) need to handle when OTP is wrong 
            //EmbeddedWalletCanvas.SetActive(false);
            if (_exception != null)
            {
                SetOtpCodeIsWrong();
                throw _exception;
            }
            return _user;
        }

        public override async void OnSubmitOTP()
        {
            OTPInput.interactable = false;
            RecoveryInput.interactable = false;
            SubmitButton.interactable = false;
            try
            {
                string otp = OTPInput.text;
                var res = await _embeddedWallet.VerifyOtpAsync(_email, otp, string.IsNullOrEmpty(RecoveryInput.text) ? null : RecoveryInput.text);
                if (res.User == null)
                {
                    if (res.CanRetry && OnEmailOTPVerificationFailed.GetPersistentEventCount() > 0)
                    {
                        OnEmailOTPVerificationFailed.Invoke();
                        return;
                    }
                    _exception = new UnityException("User OTP Verification Failed.");
                    return;
                }
                _user = res.User;
                ShowRecoveryCodes(res);
            }
            catch (Exception e)
            {
                _exception = e;
            }
            finally
            {
                OTPInput.interactable = true;
                RecoveryInput.interactable = true;
                SubmitButton.interactable = true;
                OnEmailOTPVerificationSuccess?.Invoke();
            }
        }

        private void SetOtpCodeIsWrong()
        {
            Debug.LogError("Login with OTP failed");
            confirmLoginModal.SetErrorText("OTP code is wrong");
            SubmitButton.GetComponent<LoadingButton>().SetLoading(false);
        }
    }
}
