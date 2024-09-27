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
        TreasureRuby = 978657
    }

    public enum Contract
    {
        // Tokens
        Magic,
        Vee,
        // Treasure Misc
        ManagedAccountFactory,
        MagicswapV2Router,
        // Zeeverse
        ZeeverseZee,
        ZeeverseItems,
        ZeeverseVeeClaimer,
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
                    { Contract.Vee, "0x0caadd427a6feb5b5fc1137eb05aa7ddd9c08ce9" },
                    { Contract.ManagedAccountFactory, "0x463effb51873c7720c810ac7fb2e145ec2f8cc60" },
                    { Contract.MagicswapV2Router, "0xf7c8f888720d5af7c54dfc04afe876673d7f5f43" },
                    { Contract.ZeeverseZee, "0x094fa8ae08426ab180e71e60fa253b079e13b9fe" },
                    { Contract.ZeeverseItems, "0x58318bceaa0d249b62fad57d134da7475e551b47" },
                    { Contract.ZeeverseVeeClaimer, "0x1cebdde81a9e4cd377bc7da5000797407cf9a58a" },
                }
            },
            {
                ChainId.ArbitrumSepolia, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x55d0cf68a1afe0932aff6f36c87efa703508191c" },
                    { Contract.Vee, "0x23be0504127475387a459fe4b01e54f1e336ffae" },
                    { Contract.ManagedAccountFactory, "0x463effb51873c7720c810ac7fb2e145ec2f8cc60" },
                    { Contract.MagicswapV2Router, "0xa8654a8097b78daf740c1e2ada8a6bf3cd60da50" },
                    { Contract.ZeeverseZee, "0xb1af672c7e0e8880c066ecc24930a12ff2ee8534" },
                    { Contract.ZeeverseItems, "0xfaad5aa3209ab1b25ede22ed4da5521538b649fa" },
                    { Contract.ZeeverseVeeClaimer, "0xf7abce65b1e683b7a42113f69ef76ee35cabbddc" }
                }
            },
            {
                ChainId.Mainnet, new Dictionary<Contract, string> {}
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
        };

        public static Dictionary<string, ChainId> NameToChainId = new Dictionary<string, ChainId>()
        {
            { "unknown", ChainId.Unknown },
            { "ethereum", ChainId.Mainnet },
            { "sepolia", ChainId.Sepolia },
            { "arbitrum", ChainId.Arbitrum },
            { "arbitrum-sepolia", ChainId.ArbitrumSepolia },
            { "treasure-ruby", ChainId.TreasureRuby },
        };
    }
}
