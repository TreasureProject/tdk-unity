using Newtonsoft.Json;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Treasure
{

    public partial class TDK : MonoBehaviour
    {
        public static Harvester Harvester;

        private void InitHarvester()
        {
            Harvester = new Harvester();
        }
    }

    public class Harvester
    {
        public Harvester() {}

        public async Task<TDKHarvesterResponse> GetHarvester(string id)
        {
            var req = new UnityWebRequest
            {
                url = $"{TDK.Instance.AppConfig.TDKApiUrl}/harvesters/{id}",
                method = "GET",
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("X-Chain-Id", TDK.Identity.GetChainId().ToString());
            req.SetRequestHeader("Authorization", $"Bearer {TDK.Identity.AuthToken}");
            await req.SendWebRequest();

            var rawResponse = req.downloadHandler.text;
            if (req.result != UnityWebRequest.Result.Success)
            {
                throw new UnityException($"[GetHarvester] {req.error}: {rawResponse}");
            }

            return JsonConvert.DeserializeObject<TDKHarvesterResponse>(rawResponse);
        }

        public async Task ApproveMagic(string operatorAddress, BigInteger amount)
        {
            var queueId = await TDK.Identity.WriteContract(
                address: "0x55d0cf68a1afe0932aff6f36c87efa703508191c",
                functionName: "approve",
                args: new string[] { operatorAddress, amount.ToString() }
            );
            await TDK.Identity.WaitForTransaction(queueId);
        }

        public async Task ApproveConsumables(string operatorAddress)
        {
            var queueId = await TDK.Identity.WriteContract(
                address: "0x9d012712d24C90DDEd4574430B9e6065183896BE",
                functionName: "setApprovalForAll",
                args: new string[] { operatorAddress, "true" }
            );
            await TDK.Identity.WaitForTransaction(queueId);
        }

        public async Task HarvesterStakeNft(string nftHandlerAddress, string permitsAddress, BigInteger permitsTokenId)
        {
            var queueId = await TDK.Identity.WriteContract(
                address: nftHandlerAddress,
                functionName: "stakeNft",
                args: new string[] { permitsAddress, permitsTokenId.ToString(), "1" }
            );
            await TDK.Identity.WaitForTransaction(queueId);
        }

        public async Task HarvesterDepositMagic(string harvesterAddress, BigInteger amount)
        {
            var queueId = await TDK.Identity.WriteContract(
                address: harvesterAddress,
                functionName: "deposit",
                args: new string[] { amount.ToString(), "0" }
            );
            await TDK.Identity.WaitForTransaction(queueId);
        }
    }
}
