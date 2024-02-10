using Nethereum.Siwe.Core;
using Newtonsoft.Json;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;
using UnityEngine.Networking;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Identity Identity;

        /// <summary>
        /// Initialize the Identity module
        /// </summary>
        private void InitIdentity()
        {
            Identity = new Identity();

#if TDK_THIRDWEB
            TDKServiceLocator.GetService<TDKThirdwebService>();
#endif
        }
    }

    public class Identity
    {
        #region private vars
        private string _authToken;
        private bool _isAuthenticated;
        #endregion

        #region accessors / mutators
        private Wallet _wallet
        {
            get { return TDKServiceLocator.GetService<TDKThirdwebService>().wallet; }
        }

        public string AuthToken
        {
            get { return _authToken; }
        }

        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        public async Task<string> GetWalletAddress()
        {
            return await _wallet.GetAddress();
        }
        #endregion

        #region constructors
        public Identity() { }
        #endregion

        #region private methods
        private async Task<TDKAuthPayload> GetAuthPayload()
        {
            var body = JsonConvert.SerializeObject(new TDKAuthPayloadRequest
            {
                address = await _wallet.GetAddress(),
                chainId = (await _wallet.GetChainId()).ToString(),
            });
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/auth/payload",
                method = "POST",
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[GetAuthPayload] {req.error}: {rawResponse}");
            }

            var response = JsonConvert.DeserializeObject<TDKAuthPayloadResponse>(rawResponse);
            return response.payload;
        }

        private async Task<string> GenerateSignature(TDKAuthPayload payload)
        {
            var message = new SiweMessage()
            {
                Uri = payload.uri,
                Statement = payload.statement,
                Address = payload.address,
                Domain = payload.domain,
                ChainId = payload.chain_id,
                Version = payload.version,
                Nonce = payload.nonce,
                IssuedAt = payload.issued_at,
                ExpirationTime = payload.expiration_time,
                NotBefore = payload.invalid_before,
            };
            var finalMessage = SiweMessageStringBuilder.BuildMessage(message);
            return await TDKServiceLocator.GetService<TDKThirdwebService>().Sign(finalMessage);
        }

        private async Task<string> LogIn(TDKAuthPayload payload, string signature)
        {
            var body = JsonConvert.SerializeObject(new TDKAuthLoginRequest()
            {
                payload = new TDKAuthLoginPayload()
                {
                    payload = payload,
                    signature = signature
                },
            });
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/auth/login",
                method = "POST",
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[LogIn] {req.error}: {rawResponse}");
            }

            var response = JsonConvert.DeserializeObject<TDKAuthLoginResponse>(rawResponse);
            return response.token;
        }
        #endregion

        #region public api
        public async Task<TDKProject> GetProject()
        {
            var req = new UnityWebRequest
            {
                // url = $"{TDK.Instance.AppConfig.TDKApiUrl}/projects/{TDK.Instance.AppConfig.GameId}",

                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/projects/platform",
                method = "GET",
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Chain-Id", (await _wallet.GetChainId()).ToString());
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[GetProject] {req.error}: {rawResponse}");
            }

            return JsonConvert.DeserializeObject<TDKProject>(rawResponse);
        }

        public async Task<string> Authenticate(TDKProject project)
        {
            // Create auth token
            var payload = await GetAuthPayload();
            var signature = await GenerateSignature(payload);
            var token = await LogIn(payload, signature);

            // Create session key
            var permissionEndTimestamp = Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * TDK.Instance.AppConfig.SessionLengthDays;
            await _wallet.CreateSessionKey(
                signerAddress: project.backendWallets[0],
                approvedTargets: project.callTargets,
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
            );

            _authToken = token;
            _isAuthenticated = true;

            return token;
        }

        public void LogOut()
        {
            _authToken = null;
            _isAuthenticated = false;
        }

        public async Task<TDKHarvesterResponse> GetHarvester(string id)
        {
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/harvesters/{id}",
                method = "GET",
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Chain-Id", (await _wallet.GetChainId()).ToString());
            req.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[GetHarvester] {req.error}: {rawResponse}");
            }

            return JsonConvert.DeserializeObject<TDKHarvesterResponse>(rawResponse);
        }

        public async Task<string> WriteContract(string address, string functionName, string[] args)
        {
            var body = JsonConvert.SerializeObject(new TDKContractWriteRequest()
            {
                functionName = functionName,
                args = args,
            });
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/contracts/{address}",
                method = "POST",
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Chain-Id", (await _wallet.GetChainId()).ToString());
            req.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[WriteContract] {req.error}: {rawResponse}");
            }

            var response = JsonConvert.DeserializeObject<TDKContractWriteResponse>(rawResponse);
            return response.queueId;
        }

        private async Task<TDKTransactionResponse> GetTransaction(string queueId)
        {
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/transactions/{queueId}",
                method = "GET",
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {_authToken}");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[GetTransaction] {req.error}: {rawResponse}");
            }

            return JsonConvert.DeserializeObject<TDKTransactionResponse>(rawResponse);
        }

        private async Task WaitForTransaction(string queueId)
        {
            var retries = 0;
            TDKTransactionResponse transaction;
            do
            {
                if (retries > 0)
                {
                    await Task.Delay(2_500);
                }

                transaction = await GetTransaction(queueId);
                retries++;
            } while (
                retries < 10 &&
                transaction.status != "errored" &&
                transaction.status != "cancelled" &&
                transaction.status != "mined"
            );

            if (transaction.status == "errored")
            {
                throw new UnityException($"[WaitForTransaction] Transaction {queueId} errored: {transaction.errorMessage}");
            }

            if (transaction.status == "cancelled")
            {
                throw new UnityException($"[WaitForTransaction] Transaction {queueId} cancelled");
            }

            if (transaction.status != "mined")
            {
                throw new UnityException($"[WaitForTransaction] Transaction {queueId} timed out with status: {transaction.status}");
            }
        }

        public async Task ApproveMagic(string operatorAddress, BigInteger amount)
        {
            var queueId = await WriteContract(
                address: "0x55d0cf68a1afe0932aff6f36c87efa703508191c",
                functionName: "approve",
                args: new string[] { operatorAddress, amount.ToString() }
            );
            await WaitForTransaction(queueId);
        }

        public async Task ApproveConsumables(string operatorAddress)
        {
            var queueId = await WriteContract(
                address: "0x9d012712d24C90DDEd4574430B9e6065183896BE",
                functionName: "setApprovalForAll",
                args: new string[] { operatorAddress, "true" }
            );
            await WaitForTransaction(queueId);
        }

        public async Task HarvesterStakeNft(string nftHandlerAddress, string permitsAddress, BigInteger permitsTokenId)
        {
            var queueId = await WriteContract(
                address: nftHandlerAddress,
                functionName: "stakeNft",
                args: new string[] { permitsAddress, permitsTokenId.ToString(), "1" }
            );
            await WaitForTransaction(queueId);
        }

        public async Task HarvesterDepositMagic(string harvesterAddress, BigInteger amount)
        {
            var queueId = await WriteContract(
                address: harvesterAddress,
                functionName: "deposit",
                args: new string[] { amount.ToString(), "0" }
            );
            await WaitForTransaction(queueId);
        }
        #endregion
    }
}
