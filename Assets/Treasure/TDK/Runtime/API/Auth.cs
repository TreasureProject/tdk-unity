using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Treasure
{
    [Serializable]
    public class AuthPayload
    {
        public string type;
        public string domain;
        public string address;
        public string statement;
        public string uri;
        public string version;
        public string chain_id;
        public string nonce;
        public string issued_at;
        public string expiration_time;
        public string invalid_before;

        public AuthPayload()
        {
            type = "evm";
        }
    }

    [Serializable]
    internal class GetAuthPayloadBody
    {
        public string address;
        public string chainId;
    }

    [Serializable]
    public struct GetAuthPayloadResponse
    {
        public AuthPayload payload;
    }

    [Serializable]
    public struct LogInBody
    {
        public AuthPayload payload;
        public string signature;
    }

    [Serializable]
    public struct LogInResponse
    {
        public string token;
    }

    [Serializable]
    public struct User
    {
        public struct Signer
        {
            public bool isAdmin;
            public string signer;
            public string[] approvedTargets;
            public string nativeTokenLimitPerTransaction;
            public string startTimestamp;
            public string endTimestamp;
        }

        public string id;
        public string smartAccountAddress;
        public string email;
        public List<Signer> allActiveSigners;
    }

    public partial class API
    {
        [Obsolete("GetAuthPayload is deprecated. Use GetLoginPayload.")]
        public async Task<AuthPayload> GetAuthPayload(string address, string chainId)
        {
            var response = await Post("/auth/payload", JsonConvert.SerializeObject(new GetAuthPayloadBody()
            {
                address = address,
                chainId = chainId,
            }));
            return JsonConvert.DeserializeObject<GetAuthPayloadResponse>(response).payload;
        }

        public async Task<AuthPayload> GetLoginPayload(string address)
        {
            var response = await Get($"/login/payload?address={address}");
            return JsonConvert.DeserializeObject<AuthPayload>(response);
        }

        public async Task<string> LogIn(AuthPayload payload, string signature)
        {
            var response = await Post("/login", JsonConvert.SerializeObject(new LogInBody()
            {
                payload = payload,
                signature = signature,
            }));
            return JsonConvert.DeserializeObject<LogInResponse>(response).token;
        }

        public async Task<User> GetCurrentUser(RequestOverrides? overrides = null)
        {
            var response = await Get("/users/me", overrides);
            return JsonConvert.DeserializeObject<User>(response);
        }
    }
}
