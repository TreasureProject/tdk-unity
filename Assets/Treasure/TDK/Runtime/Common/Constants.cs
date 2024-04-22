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
        Legions,
        Treasures,
        CorruptionRemoval,
        ERC1155TokenSetCorruptionHandler,
        HarvesterEmberwing,
        ZeeverseZee,
        ZeeverseItems,
    }

    public static class Constants
    {
        // player prefs
        public const string PPREFS_EPOCH_DIFF = "treasure.epoch_diff";

        // misc values
        public const string SERVER_TIME_ENDPOINT_DEV = "https://darkmatter-dev.treasure.lol/utils/time-unix"; //"https://trove-api.treasure.lol/v1/time";
        public const string SERVER_TIME_ENDPOINT_PROD = "https://darkmatter.treasure.lol/utils/time-unix"; //"https://trove-api.treasure.lol/v1/time";

        // contract accresses
        public static Dictionary<ChainId, Dictionary<Contract, string>> ContractAddresses = new Dictionary<ChainId, Dictionary<Contract, string>> {
            {
                ChainId.Arbitrum, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x539bde0d7dbd336b79148aa742883198bbf60342" },
                    { Contract.Consumables, "0xf3d00a2559d84de7ac093443bcaada5f4ee4165c" },
                    { Contract.Legions, "0xfe8c1ac365ba6780aec5a985d989b327c27670a1" },
                    { Contract.Treasures, "0xebba467ecb6b21239178033189ceae27ca12eadf" },
                    { Contract.CorruptionRemoval, "0x08f3533acdf2b9c400204056f771bdd6f1f1c200" },
                    { Contract.ERC1155TokenSetCorruptionHandler, "0x3c62778d8e01ed17c1048b64edaf121d36c71a4e" },
                    { Contract.HarvesterEmberwing, "" },
                    { Contract.ZeeverseZee, "0x094fa8ae08426ab180e71e60fa253b079e13b9fe" },
                    { Contract.ZeeverseItems, "0x58318bceaa0d249b62fad57d134da7475e551b47" },
                }
            },
            {
                ChainId.ArbitrumSepolia, new Dictionary<Contract, string> {
                    { Contract.Magic, "0x55d0cf68a1afe0932aff6f36c87efa703508191c" },
                    { Contract.Consumables, "0x9d012712d24C90DDEd4574430B9e6065183896BE" },
                    { Contract.Legions, "0xd144e34c3c0a8e605e9d45792380841a2169dd8f" },
                    { Contract.Treasures, "0xfe592736200d7545981397ca7a8e896ac0c166d4" },
                    { Contract.CorruptionRemoval, "0xdd8b0dd8128873049b1d528262724bde600f5be2" },
                    { Contract.ERC1155TokenSetCorruptionHandler, "0x937817e7fe8e3b3543db46f14473d5f110a79ece" },
                    { Contract.HarvesterEmberwing, "0x816c0717cf263e7da4cd33d4979ad15dbb70f122" },
                    { Contract.ZeeverseZee, "0xb1af672c7e0e8880c066ecc24930a12ff2ee8534" },
                    { Contract.ZeeverseItems, "0xfaad5aa3209ab1b25ede22ed4da5521538b649fa" },
                }
            }
        };
    }
}