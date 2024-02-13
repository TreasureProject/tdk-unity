using System.Collections.Generic;

namespace Treasure
{
    public enum Contract
    {
        Magic,
        Consumables,
        HarvesterEmerion,
    }

    public class Constants
    {
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