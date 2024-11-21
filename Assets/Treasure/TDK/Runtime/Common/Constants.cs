using System.Collections.Generic;

namespace Treasure
{
    public enum ChainId : int
    {
        Unknown = -1,
        Mainnet = 1,
        Sepolia = 11155111,
        Arbitrum = 42161,
        ArbitrumSepolia = 421614,
        TreasureRuby = 978657,
        TreasureTopaz = 978658
    }

    public enum Contract
    {
        Magic,
        ManagedAccountFactory,
        MagicswapV2Router,
    }

    public static class Constants
    {
        // player prefs
        public const string PPREFS_EPOCH_DIFF = "treasure.epoch_diff";

        // misc values
        public const string SERVER_TIME_ENDPOINT_DEV = "https://darkmatter.spellcaster.lol/utils/time-unix"; //"https://trove-api.treasure.lol/v1/time";
        public const string SERVER_TIME_ENDPOINT_PROD = "https://darkmatter.treasure.lol/utils/time-unix"; //"https://trove-api.treasure.lol/v1/time";

        // contract accresses
        public static Dictionary<ChainId, Dictionary<Contract, string>> ContractAddresses = new Dictionary<ChainId, Dictionary<Contract, string>> {
            {
                ChainId.Arbitrum, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x539bde0d7dbd336b79148aa742883198bbf60342" },
                    { Contract.ManagedAccountFactory, "0x463effb51873c7720c810ac7fb2e145ec2f8cc60" },
                    { Contract.MagicswapV2Router, "0xf7c8f888720d5af7c54dfc04afe876673d7f5f43" },
                }
            },
            {
                ChainId.ArbitrumSepolia, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x55d0cf68a1afe0932aff6f36c87efa703508191c" },
                    { Contract.ManagedAccountFactory, "0x463effb51873c7720c810ac7fb2e145ec2f8cc60" },
                    { Contract.MagicswapV2Router, "0xa8654a8097b78daf740c1e2ada8a6bf3cd60da50" },
                }
            },
            {
                ChainId.Mainnet, new Dictionary<Contract, string> {
                    { Contract.ManagedAccountFactory, "0x463effb51873c7720c810ac7fb2e145ec2f8cc60" },
                }
            },
            {
                ChainId.Sepolia, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x013cb2854daad8203c6686682f5d876e5d3de4a2" },
                    { Contract.ManagedAccountFactory, "0x463effb51873c7720c810ac7fb2e145ec2f8cc60" },
                }
            },
            {
                ChainId.TreasureRuby, new Dictionary<Contract, string> {
                    { Contract.ManagedAccountFactory, "0x463effb51873c7720c810ac7fb2e145ec2f8cc60" },
                }
            },
            {
                ChainId.TreasureTopaz, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x095ded714d42cbd5fb2e84a0ffbfb140e38dc9e1" },
                    { Contract.ManagedAccountFactory, null }, // not present in topaz
                    { Contract.MagicswapV2Router, "0xad781ed13b5966e7c620b896b6340abb4dd2ca86" },
                }
            }
        };

        public static Dictionary<ChainId, string> ChainIdToName = new Dictionary<ChainId, string>()
        {
            { ChainId.Unknown, "unknown" },
            { ChainId.Mainnet, "ethereum" },
            { ChainId.Sepolia, "sepolia" },
            { ChainId.Arbitrum, "arbitrum" },
            { ChainId.ArbitrumSepolia, "arbitrum-sepolia" },
            { ChainId.TreasureRuby, "treasure-ruby" },
            { ChainId.TreasureTopaz, "treasure-topaz" }
        };

        public static Dictionary<string, ChainId> NameToChainId = new Dictionary<string, ChainId>()
        {
            { "unknown", ChainId.Unknown },
            { "ethereum", ChainId.Mainnet },
            { "sepolia", ChainId.Sepolia },
            { "arbitrum", ChainId.Arbitrum },
            { "arbitrum-sepolia", ChainId.ArbitrumSepolia },
            { "treasure-ruby", ChainId.TreasureRuby },
            { "treasure-topaz", ChainId.TreasureTopaz }
        };
    }
}
