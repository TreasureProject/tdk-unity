using System;

namespace Treasure
{
    [Serializable]
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

    [Serializable]
    public struct TDKAuthPayloadRequest
    {
        public string address;
        public string chainId;
    }

    [Serializable]
    public struct TDKAuthPayloadResponse
    {
        public TDKAuthPayload payload;
    }

    [Serializable]
    public struct TDKAuthLoginPayload
    {
        public TDKAuthPayload payload;
        public string signature;
    }

    [Serializable]
    public struct TDKAuthLoginRequest
    {
        public TDKAuthLoginPayload payload;
    }

    [Serializable]
    public struct TDKAuthLoginResponse
    {
        public string token;
    }

    [Serializable]
    public struct TDKHarvesterResponse
    {
        [Serializable]
        public struct Harvester
        {
            public string permitsAddress;
            public string permitsTokenId;
        }

        [Serializable]
        public struct User
        {
            public string magicBalance;
            public string permitsBalance;
            public string harvesterDepositCap;
            public string harvesterDepositAmount;
        }

        public Harvester harvester;
        public User user;
    }
}
