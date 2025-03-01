using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.Numerics;
using Thirdweb;
using Newtonsoft.Json;
using Cysharp.Threading.Tasks;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Identity Identity { get; private set; }

        /// <summary>
        /// Initialize the Identity module
        /// </summary>
        private void InitIdentity()
        {
            Identity = new Identity();
        }
    }

    public class Identity
    {
        #region private vars
        private string _address;
        private string _authToken;
        #endregion

        #region accessors / mutators
        public string Address
        {
            // Identity service can have its own address set in the case where an existing session was used and no wallet is connected
            // Otherwise, fall back to Connect service address
            get
            {
                if (string.IsNullOrEmpty(_address))
                {
                    return TDK.Connect.Address;
                }

                return _address;
            }
        }

        public string AuthToken
        {
            get { return _authToken; }
        }

        public bool IsAuthenticated
        {
            get { return !string.IsNullOrEmpty(_authToken); }
        }

        public bool IsUsingTreasureLauncher
        {
            get { return TreasureLauncherUtils.GetLauncherAuthCookie() != null; }
        }
        #endregion

        #region constructors
        public Identity() { }
        #endregion

        #region private methods
        private async Task<string> SignLoginPayload(AuthPayload payload)
        {
            var payloadToSign =
                $"{payload.domain} wants you to sign in with your Ethereum account:"
                + $"\n{payload.address}\n\n"
                + $"{(string.IsNullOrEmpty(payload.statement) ? "" : $"{payload.statement}\n")}"
                + $"{(string.IsNullOrEmpty(payload.uri) ? "" : $"\nURI: {payload.uri}")}"
                + $"\nVersion: {payload.version}"
                + $"\nChain ID: {payload.chain_id}"
                + $"\nNonce: {payload.nonce}"
                + $"\nIssued At: {payload.issued_at}"
                + $"{(string.IsNullOrEmpty(payload.expiration_time) ? "" : $"\nExpiration Time: {payload.expiration_time}")}"
                + $"{(string.IsNullOrEmpty(payload.invalid_before) ? "" : $"\nNot Before: {payload.invalid_before}")}";
            return await TDKServiceLocator.GetService<TDKThirdwebService>().ActiveWallet.PersonalSign(payloadToSign);
        }

        public async Task CreateSessionKey(string backendWallet, List<string> callTargets, BigInteger nativeTokenLimitPerTransaction)
        {
            var permissionStartTimestamp = (decimal)Utils.GetUnixTimeStampNow() - 60 * 60;
            var permissionEndTimestamp = (decimal)(Utils.GetUnixTimeStampNow() + TDK.AppConfig.SessionDurationSec);
            await TDKServiceLocator.GetService<TDKThirdwebService>().ActiveWallet.CreateSessionKey(
                signerAddress: backendWallet,
                approvedTargets: callTargets,
                nativeTokenLimitPerTransactionInWei: nativeTokenLimitPerTransaction.ToString(),
                permissionStartTimestamp: permissionStartTimestamp.ToString(),
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: permissionStartTimestamp.ToString(),
                reqValidityEndTimestamp: permissionEndTimestamp.ToString()
            );
        }

        public async Task<List<User.Session>> GetUserSessions()
        {
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            var activeSignersTask = thirdwebService.ActiveWallet.GetAllActiveSigners();
            var allAdminsTask = thirdwebService.ActiveWallet.GetAllAdmins();
            await Task.WhenAll(activeSignersTask, allAdminsTask);
            var activeSigners = activeSignersTask.Result;
            var allAdmins = allAdminsTask.Result;
            return activeSigners.Select(activeSigner => new User.Session
            {
                isAdmin = allAdmins.Contains(activeSigner.Signer),
                signer = activeSigner.Signer,
                approvedTargets = activeSigner.ApprovedTargets.ToArray(),
                nativeTokenLimitPerTransaction = activeSigner.NativeTokenLimitPerTransaction,
                startTimestamp = activeSigner.StartTimestamp,
                endTimestamp = activeSigner.EndTimestamp
            }).ToList();
        }

        public bool ValidateSession(string backendWallet, List<string> callTargets, BigInteger nativeTokenLimitPerTransaction, User.Session session)
        {
            var requestedCallTargets = callTargets.Select(callTarget => callTarget.ToLowerInvariant());
            var signerApprovedTargets = session.approvedTargets.Select(approvedTarget => approvedTarget.ToLowerInvariant());
            var nowDate = Utils.GetUnixTimeStampNow();
            var minEndDate = Utils.GetUnixTimeStampNow() + TDK.AppConfig.SessionMinDurationLeftSec;
            var maxEndDate = Utils.GetUnixTimeStampIn10Years();
            return
                // Expected backend wallet is signer
                session.signer.ToLowerInvariant() == backendWallet.ToLowerInvariant() &&
                // If this signer is an admin, they always have the required permissions
                (session.isAdmin || (
                    // Start date has passed
                    session.startTimestamp < nowDate &&
                    // Expiration date meets minimum time requirements
                    session.endTimestamp >= minEndDate &&
                    // Expiration date is not too far in the future (10 years because Thirdweb uses this for admins)
                    // This check is to prevent sessions from being created with timestamps in milliseconds
                    session.endTimestamp <= maxEndDate &&
                    // All requested targets are approved
                    requestedCallTargets.All(callTarget => signerApprovedTargets.Contains(callTarget)) &&
                    // Native token limit per transaction is approved
                    session.nativeTokenLimitPerTransaction >= nativeTokenLimitPerTransaction
                ));
        }

        private async Task StartLauncherSessionRequest()
        {
            var body = JsonConvert.SerializeObject(new
            {
                backendWallet = TDK.AppConfig.GetBackendWallet(),
                approvedTargets = TDK.AppConfig.GetCallTargets(),
                nativeTokenLimitPerTransaction = TDK.AppConfig.GetNativeTokenLimitPerTransaction(),
                sessionDurationSec = TDK.AppConfig.SessionDurationSec,
                sessionMinDurationLeftSec = TDK.AppConfig.SessionMinDurationLeftSec,
            });

            var launcherServerUrl = TreasureLauncherUtils.GetLauncherServerUrl();
            using UnityWebRequest www = UnityWebRequest.Post($"{launcherServerUrl}/tdk-start-session", body, "application/json");
            await www.SendWebRequest();
            var rawResponse = www.downloadHandler.text;
            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"Error starting session - {www.error}: {rawResponse}");
            }
        }
        #endregion

        #region public api
        public async Task<User?> ValidateUserSession(ChainId chainId, string authToken)
        {
            TDKLogger.Log("Validating existing user session");
            try
            {
                // Fetch user details for provided auth token
                TDKLogger.Log("Fetching user details");
                var user = await TDK.API.GetCurrentUser(new API.RequestOverrides()
                {
                    authToken = authToken,
                    chainId = chainId
                });

                var backendWallet = TDK.AppConfig.GetBackendWallet();
                var callTargets = TDK.AppConfig.GetCallTargets();
                var requiresSession = !string.IsNullOrEmpty(backendWallet) && callTargets.Count > 0;
                if (requiresSession)
                {
                    var nativeTokenLimitPerTransaction = TDK.AppConfig.GetNativeTokenLimitPerTransaction();

                    // Check if any active signers match the call targets
                    var hasActiveSession = user.sessions.Any((signer) =>
                    {
                        return ValidateSession(
                            backendWallet,
                            callTargets,
                            nativeTokenLimitPerTransaction,
                            signer
                        );
                    });

                    if (!hasActiveSession)
                    {
                        TDKLogger.Log("Existing user session does not have required permissions. User must start a new session.");
                        return null;
                    }
                }

                _address = user.address;
                _authToken = authToken;

                TDKLogger.Log("Existing user session is valid");

                return user;
            }
            catch (Exception ex)
            {
                TDKLogger.LogDebug($"ValidateUserSession error: {ex.Message}");
                // Auth token was invalid or expired
                return null;
            }
        }

        public async Task<string> StartUserSession(ChainId sessionChainId = ChainId.Unknown, string sessionAuthToken = null)
        {
            if (IsUsingTreasureLauncher)
            {
                TDKLogger.LogError("Unable to start user session. Use StartUserSessionViaLauncher instead.");
                return await Task.FromResult(string.Empty);
            }
            // Check if user already has a valid session for the specified chain
            var chainId = sessionChainId == ChainId.Unknown ? TDK.Connect.ChainId : sessionChainId;
            var authToken = !string.IsNullOrEmpty(sessionAuthToken) ? sessionAuthToken : _authToken;
            if (!string.IsNullOrEmpty(authToken))
            {
                var user = await ValidateUserSession(chainId, authToken);
                if (user.HasValue)
                {
                    TDKLogger.Log("User already has a valid session");
                    return authToken;
                }
            }

            // The rest of this flow requires a wallet to be connected
            if (!await TDK.Connect.IsWalletConnected())
            {
                TDKLogger.LogError("Unable to start user session. TDK Identity wallet not connected.");
                return await Task.FromResult(string.Empty);
            }

            // Switch chains if not on the correct one already
            if (chainId != TDK.Connect.ChainId)
            {
                TDKLogger.Log($"Switching chain to {chainId}");
                await TDK.Connect.SetChainId(chainId);
            }

            var backendWallet = TDK.AppConfig.GetBackendWallet();
            var callTargets = TDK.AppConfig.GetCallTargets();
            var nativeTokenLimitPerTransaction = TDK.AppConfig.GetNativeTokenLimitPerTransaction();
            var requiresSession = !string.IsNullOrEmpty(backendWallet) && callTargets.Count > 0;
            var didCreateSession = false;

            // If smart wallet isn't deployed yet, create a new session to bundle the two txs
            if (!await TDKServiceLocator.GetService<TDKThirdwebService>().ActiveWallet.IsDeployed() && requiresSession)
            {
                TDKLogger.Log("Deploying smart wallet and creating session key");
                await CreateSessionKey(backendWallet, callTargets, nativeTokenLimitPerTransaction);
                didCreateSession = true;
            }

            // Create auth token
            TDKLogger.Log("Fetching login payload");
            var payload = await TDK.API.GetLoginPayload(Address);

            TDKLogger.Log("Signing login payload");
            var signature = await SignLoginPayload(payload);

            TDKLogger.Log("Logging in and fetching TDK auth token");
            _authToken = await TDK.API.LogIn(payload, signature);

            // Smart wallet was already deployed, so check for existing sessions
            if (!didCreateSession && requiresSession)
            {
                var hasActiveSession = false;
                List<User.Session> sessions = null;
                try
                {
                    sessions = await GetUserSessions();
                }
                catch (Exception e)
                {
                    // GetAllActiveSigners can error if the expirationDate is invalid
                    // In this case, we will ignore the session and override it by creating a new one
                    TDKLogger.LogError($"Error fetching active signers: {e}");
                }

                if (sessions != null && sessions.Count > 0)
                {
                    // Check if any active signers match the call targets
                    hasActiveSession = sessions.Any((signer) =>
                    {
                        return ValidateSession(
                            backendWallet,
                            callTargets,
                            nativeTokenLimitPerTransaction,
                            signer
                        );
                    });
                }

                if (!hasActiveSession)
                {
                    TDKLogger.Log("Creating new session key");
                    await CreateSessionKey(backendWallet, callTargets, nativeTokenLimitPerTransaction);
                }
                else
                {
                    TDKLogger.Log("Using existing session key");
                }
            }

            TDKLogger.Log("User session started successfully");

            return _authToken;
        }

        public async Task EndUserSession()
        {
            if (IsUsingTreasureLauncher)
            {
                TDKLogger.Log("[TDK.Identity:EndUserSession] Using launcher token, skipping.");
                return;
            }
            try
            {
                await TDK.Connect.Disconnect();
            }
            catch (Exception e)
            {
                TDKLogger.LogError($"Error ending user session: {e}");
                return;
            }

            _address = null;
            _authToken = null;
        }

        public async Task StartUserSessionViaLauncher()
        {
            if (!IsUsingTreasureLauncher)
            {
                TDKLogger.LogError("Unable to start user session. Use StartUserSession instead.");
                return;
            }
            TDKLogger.Log("Starting session via launcher token");
            await StartLauncherSessionRequest();

            var user = await ValidateUserSession(TDK.Connect.ChainId, TreasureLauncherUtils.GetLauncherAuthToken());
            if (user.HasValue)
            {
                TDKLogger.Log("User session validated successfully");
            }
            else
            {
                TDKLogger.Log("Unable to validate user session");
            }
        }

        public async Task AttemptConnectionViaLauncherAuth()
        {
            try
            {
                var launcherAuthCookie = TreasureLauncherUtils.GetLauncherAuthCookie();
                var launcherAuthProvider = TreasureLauncherUtils.GetLauncherAuthProvider();
                if (launcherAuthCookie == null || !launcherAuthProvider.HasValue)
                {
                    return;
                }
                TDKLogger.LogDebug($"Connecting via auth cookie (provider: {launcherAuthProvider.Value})");
                var didConnect = await TDK.Connect.ConnectViaCookie(
                    launcherAuthCookie,
                    launcherAuthProvider.Value,
                    email: launcherAuthProvider == AuthProvider.Default ? TreasureLauncherUtils.GetEmailAddressFromAuthCookie() : null
                );
                if (didConnect)
                {
                    TDKLogger.LogDebug($"Connected via auth cookie.");
                }
                else
                {
                    TDKLogger.LogDebug($"Unable to connect via auth cookie, check internet connection.");
                }
            }
            catch (Exception ex)
            {
                TDKLogger.LogException("Error connecting with launcher token", ex);
            }
        }
        #endregion
    }
}
