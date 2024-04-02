using System.Collections.Generic;

namespace Treasure
{
    public enum ChainId : int
    {
        Arbitrum = 42161,
        ArbitrumSepolia = 421614
    }

    public enum Contract
    {
        Magic,
        Consumables,
        HarvesterEmerion,
    }

    public static class Constants
    {
        // player prefs
        public const string PPREFS_EPOCH_DIFF = "treasure.epoch_diff";

        // misc values
        public const string SERVER_TIME_ENDPOINT_DEV =  "https://darkmatter-dev.treasure.lol/utils/time-unix"; //"https://trove-api.treasure.lol/v1/time";
        public const string SERVER_TIME_ENDPOINT_PROD =  "https://darkmatter.treasure.lol/utils/time-unix"; //"https://trove-api.treasure.lol/v1/time";
        
        // contract accresses
        public static Dictionary<ChainId, Dictionary<Contract, string>> ContractAddresses = new Dictionary<ChainId, Dictionary<Contract, string>> {
            {
                ChainId.Arbitrum, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x539bde0d7dbd336b79148aa742883198bbf60342" },
                    { Contract.Consumables, "0xf3d00a2559d84de7ac093443bcaada5f4ee4165c" },
                    { Contract.HarvesterEmerion, "0x587dc30014e10b56907237d4880a9bf8b9518150" },
                }
            },
            {
                ChainId.ArbitrumSepolia, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x55d0cf68a1afe0932aff6f36c87efa703508191c" },
                    { Contract.Consumables, "0x9d012712d24C90DDEd4574430B9e6065183896BE" },
                    { Contract.HarvesterEmerion, "0x466d20a94e280bb419031161a6a7508438ad436f" },
                }
            }
        };
    }
}