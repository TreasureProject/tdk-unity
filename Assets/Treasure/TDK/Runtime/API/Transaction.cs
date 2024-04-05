using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Treasure
{
    [Serializable]
    public class Transaction
    {
        public string queueId;
        public string status;
        public string transactionHash;
        public string errorMessage;
    }

    [Serializable]
    internal class WriteTransactionBody
    {
        public string address;
        public string functionName;
        public object[] args;
    }

    public partial class API
    {
        public async Task<Transaction> GetTransactionByQueueId(string queueId)
        {
            var response = await Get($"/transactions/{queueId}");
            return JsonConvert.DeserializeObject<Transaction>(response);
        }

        public async Task<Transaction> WriteTransaction(string address, string functionName, object[] args)
        {
            var response = await Post("/transactions", JsonConvert.SerializeObject(new WriteTransactionBody()
            {
                address = address,
                functionName = functionName,
                args = args,
            }));
            return JsonConvert.DeserializeObject<Transaction>(response);
        }

        public async Task<Transaction> WriteTransaction(Contract contract, string functionName, object[] args)
        {
            var chainId = await TDK.Identity.GetChainId();
            return await WriteTransaction(
                address: Constants.ContractAddresses[chainId][contract],
                functionName: functionName,
                args: args
            );
        }
    }
}
