using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Numerics;
using Thirdweb;

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
                    _address = TDK.Connect.Address;
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
            return await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.Sign(payloadToSign);
        }

        private async Task CreateSessionKey(string backendWallet, List<string> callTargets, BigInteger nativeTokenLimitPerTransaction)
        {
            var permissionStartTimestamp = (decimal)Utils.GetUnixTimeStampNow() - 60 * 60;
            var permissionEndTimestamp = (decimal)(Utils.GetUnixTimeStampNow() + TDK.Instance.AppConfig.SessionDurationSec);
            await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.CreateSessionKey(
                signerAddress: backendWallet,
                approvedTargets: callTargets,
                nativeTokenLimitPerTransactionInWei: nativeTokenLimitPerTransaction.ToString(),
                permissionStartTimestamp: permissionStartTimestamp.ToString(),
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: permissionStartTimestamp.ToString(),
                reqValidityEndTimestamp: permissionEndTimestamp.ToString()
            );
        }

        private async Task<List<User.Signer>> GetActiveSigners()
        {
            var activeSigners = await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.GetAllActiveSigners();
            return activeSigners.Select(activeSigner => new User.Signer
            {
                isAdmin = activeSigner.isAdmin ?? false,
                signer = activeSigner.signer,
                approvedTargets = activeSigner.permissions.approvedCallTargets.ToArray(),
                nativeTokenLimitPerTransaction = activeSigner.permissions.nativeTokenLimitPerTransaction,
                startTimestamp = activeSigner.permissions.startDate,
                endTimestamp = activeSigner.permissions.expirationDate
            }).ToList();
        }

        private bool ValidateActiveSigner(string backendWallet, List<string> callTargets, BigInteger nativeTokenLimitPerTransaction, User.Signer signer)
        {
            var requestedCallTargets = callTargets.Select(callTarget => callTarget.ToLowerInvariant());
            var signerApprovedTargets = signer.approvedTargets.Select(approvedTarget => approvedTarget.ToLowerInvariant());
            var nowDate = Utils.GetUnixTimeStampNow();
            var minEndDate = Utils.GetUnixTimeStampNow() + TDK.Instance.AppConfig.SessionMinDurationLeftSec;
            var maxEndDate = Utils.GetUnixTimeStampIn10Years();
            return
                // Expected backend wallet is signer
                signer.signer.ToLowerInvariant() == backendWallet.ToLowerInvariant() &&
                // If this signer is an admin, they always have the required permissions
                (signer.isAdmin ||
                    // Start date has passed
                    (long.Parse(signer.startTimestamp) < nowDate &&
                    // Expiration date meets minimum time requirements
                    long.Parse(signer.endTimestamp) >= minEndDate &&
                    // Expiration date is not too far in the future (10 years because Thirdweb uses this for admins)
                    // This check is to prevent sessions from being created with timestamps in milliseconds
                    long.Parse(signer.endTimestamp) <= maxEndDate &&
                    // All requested targets are approved
                    requestedCallTargets.All(callTarget => signerApprovedTargets.Contains(callTarget)) &&
                    // Native token limit per transaction is approved
                    BigInteger.Parse(signer.nativeTokenLimitPerTransaction) >= nativeTokenLimitPerTransaction)
                );
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

                var backendWallet = await TDK.Instance.AppConfig.GetBackendWallet();
                var callTargets = await TDK.Instance.AppConfig.GetCallTargets();
                var requiresSession = !string.IsNullOrEmpty(backendWallet) && callTargets.Count > 0;
                if (requiresSession)
                {
                    var nativeTokenLimitPerTransaction = await TDK.Instance.AppConfig.GetNativeTokenLimitPerTransaction();

                    // Check if any active signers match the call targets
                    var hasActiveSession = user.allActiveSigners.Any((signer) =>
                    {
                        return ValidateActiveSigner(
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

                _address = user.smartAccountAddress;
                _authToken = authToken;

                TDKLogger.Log("Existing user session is valid");

                return user;
            }
            catch
            {
                // Auth token was invalid or expired
                return null;
            }
        }

        public async Task<string> StartUserSession(ChainId sessionChainId = ChainId.Unknown, string sessionAuthToken = null)
        {
            // Check if user already has a valid session for the specified chain
            var currentChainId = await TDK.Connect.GetChainId();
            var chainId = sessionChainId == ChainId.Unknown ? currentChainId : sessionChainId;
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
            if (chainId != currentChainId)
            {
                TDKLogger.Log($"Switching chain to {chainId}");
                await TDK.Connect.SetChainId(chainId);
            }

            var backendWallet = await TDK.Instance.AppConfig.GetBackendWallet();
            var callTargets = await TDK.Instance.AppConfig.GetCallTargets();
            var nativeTokenLimitPerTransaction = await TDK.Instance.AppConfig.GetNativeTokenLimitPerTransaction();
            var requiresSession = !string.IsNullOrEmpty(backendWallet) && callTargets.Count > 0;
            var didCreateSession = false;

            // If smart wallet isn't deployed yet, create a new session to bundle the two txs
            if (!await TDKServiceLocator.GetService<TDKThirdwebService>().Wallet.IsDeployed() && requiresSession)
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
                List<User.Signer> activeSigners = null;
                try
                {
                    activeSigners = await GetActiveSigners();
                }
                catch (Exception e)
                {
                    // GetAllActiveSigners can error if the expirationDate is invalid
                    // In this case, we will ignore the session and override it by creating a new one
                    TDKLogger.LogError($"Error fetching active signers: {e}");
                }

                if (activeSigners != null && activeSigners.Count > 0)
                {
                    // Check if any active signers match the call targets
                    hasActiveSession = activeSigners.Any((signer) =>
                    {
                        return ValidateActiveSigner(
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
            try
            {
                await TDK.Connect.Disconnect(true);
            }
            catch (Exception e)
            {
                TDKLogger.LogError($"Error ending user session: {e}");
                return;
            }

            _address = null;
            _authToken = null;
        }
        #endregion
    }
}
