using Newtonsoft.Json;
using System;
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
        public struct Payload
        {
            public AuthPayload payload;
            public string signature;
        }

        public Payload payload;
    }

    [Serializable]
    public struct LogInResponse
    {
        public string token;
    }

    [Serializable]
    public struct User
    {
        public string id;
        public string smartAccountAddress;
        public string email;
    }

    public partial class API
    {
        public async Task<AuthPayload> GetAuthPayload(string address, string chainId)
        {
            var response = await Post("/auth/payload", JsonConvert.SerializeObject(new GetAuthPayloadBody()
            {
                address = address,
                chainId = chainId,
            }));
            return JsonConvert.DeserializeObject<GetAuthPayloadResponse>(response).payload;
        }

        public async Task<string> LogIn(AuthPayload payload, string signature)
        {
            var response = await Post("/auth/login", JsonConvert.SerializeObject(new LogInBody()
            {
                payload = new LogInBody.Payload()
                {
                    payload = payload,
                    signature = signature,
                }
            }));
            return JsonConvert.DeserializeObject<LogInResponse>(response).token;
        }

        public async Task<User> GetCurrentUser()
        {
            var response = await Get("/users/me");
            return JsonConvert.DeserializeObject<User>(response);
        }
    }
}
