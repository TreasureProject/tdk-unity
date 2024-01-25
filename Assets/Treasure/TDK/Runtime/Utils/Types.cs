using System;
using System.Collections.Generic;
using System.Numerics;

namespace Treasure
{
    [Serializable]
    public struct TDKProject
    {
        public string slug;
        public string name;
        public List<string> backendWallets;
        public List<string> callTargets;
        public string icon;
        public string cover;
        public string color;
    }

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
    public struct TDKContractWriteRequest
    {
        public string functionName;
        public string[] args;
    }

    [Serializable]
    public struct TDKContractWriteResponse
    {
        public string queueId;
    }

    [Serializable]
    public struct TDKHarvesterResponse
    {
        [Serializable]
        public struct Harvester
        {
            public string nftHandlerAddress;
            public string permitsAddress;
            public BigInteger permitsTokenId;
        }

        [Serializable]
        public struct User
        {
            public BigInteger magicBalance;
            public BigInteger permitsBalance;
            public BigInteger harvesterMagicAllowance;
            public bool harvesterPermitsApproved;
            public BigInteger harvesterDepositCap;
            public BigInteger harvesterDepositAmount;
        }

        public Harvester harvester;
        public User user;
    }
}
