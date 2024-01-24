namespace Treasure
{
    [System.Serializable]
    public class TDKAuthPayload
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

        public TDKAuthPayload()
        {
            type = "evm";
        }
    }

    [System.Serializable]
    public struct TDKAuthPayloadRequest
    {
        public string address;
        public string chainId;
    }

    [System.Serializable]
    public struct TDKAuthPayloadResponse
    {
        public TDKAuthPayload payload;
    }

    [System.Serializable]
    public struct TDKAuthLoginPayload
    {
        public TDKAuthPayload payload;
        public string signature;
    }

    [System.Serializable]
    public struct TDKAuthLoginRequest
    {
        public TDKAuthLoginPayload payload;
    }

    [System.Serializable]
    public struct TDKAuthLoginResponse
    {
        public string token;
    }
}