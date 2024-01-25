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
        public static Identity identity;

        /// <summary>
        /// Initialize the Identity module
        /// </summary>
        private void InitIdentity()
        {
            identity = new Identity(AppConfig.GameId, AppConfig.TDKApiUrl);

#if TDK_THIRDWEB
            TDKServiceLocator.GetService<TDKThirdwebService>();
#endif
        }
    }

    public class Identity
    {
        private string _gameId;
        private string _tdkApiUrl;
        private string _authToken;
        private bool _isAuthenticated;

        private Wallet _wallet
        {
            get { return TDKServiceLocator.GetService<TDKThirdwebService>().wallet; }
        }

        public Identity(string gameId, string tdkApiUrl)
        {
            _gameId = gameId;
            _tdkApiUrl = tdkApiUrl;
        }

        public string AuthToken
        {
            get { return _authToken; }
        }

        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        public async Task<TDKProject> GetProject()
        {
            var req = new UnityWebRequest
            {
                // url = $"{_tdkApiUrl}/projects/{_gameId}",
                url = $"{_tdkApiUrl}/projects/platform",
                method = "GET",
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Chain-Id", (await _wallet.GetChainId()).ToString());
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[LogIn] {req.error}: {rawResponse}");
            }

            return JsonConvert.DeserializeObject<TDKProject>(rawResponse);
        }

        private async Task<TDKAuthPayload> GetAuthPayload()
        {
            var body = JsonConvert.SerializeObject(new TDKAuthPayloadRequest
            {
                address = await _wallet.GetAddress(),
                chainId = (await _wallet.GetChainId()).ToString(),
            });
            var req = new UnityWebRequest
            {
                url = $"{_tdkApiUrl}/auth/payload",
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
                url = $"{_tdkApiUrl}/auth/login",
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

        public async Task<string> Authenticate(TDKProject project)
        {
            // Create auth token
            var payload = await GetAuthPayload();
            var signature = await GenerateSignature(payload);
            var token = await LogIn(payload, signature);

            // Create session key
            var permissionEndTimestamp = Utils.GetUnixTimeStampNow() + 60 * 60 * 24 * 3; // in 3 days
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
                url = $"{_tdkApiUrl}/harvesters/{id}",
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
                throw new UnityException($"[LogIn] {req.error}: {rawResponse}");
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
                url = $"{_tdkApiUrl}/contracts/{address}",
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

        public async Task<string> HarvesterStakeNft(string nftHandlerAddress, string permitsAddress, BigInteger permitsTokenId)
        {
            return await WriteContract(
                address: nftHandlerAddress,
                functionName: "stakeNft",
                args: new string[] { permitsAddress, permitsTokenId.ToString(), "1" }
            );
        }

        public async Task<string> HarvesterDepositMagic(string harvesterAddress, BigInteger amount)
        {
            return await WriteContract(
                address: harvesterAddress,
                functionName: "deposit",
                args: new string[] { amount.ToString(), "0" }
            );
        }
    }
}
