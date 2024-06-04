using System;
using System.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;

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
    public class WriteTransactionBody
    {
        public struct TransactionOverrides
        {
            public string value;
            public string gas;
            public string maxFeePerGas;
            public string maxPriorityFeePerGas;
        }

        public string address;
        public string functionName;
        public object[] args;
        public TransactionOverrides txOverrides;
    }

    public partial class API
    {
        public async Task<Transaction> GetTransactionByQueueId(string queueId)
        {
            var response = await Get($"/transactions/{queueId}");
            return JsonConvert.DeserializeObject<Transaction>(response);
        }

        public async Task<Transaction> WriteTransaction(WriteTransactionBody body)
        {
            var response = await Post("/transactions", JsonConvert.SerializeObject(body));
            return JsonConvert.DeserializeObject<Transaction>(response);
        }

        public async Task<Transaction> WriteTransaction(string address, string functionName, object[] args)
        {
            return await WriteTransaction(new WriteTransactionBody()
            {
                address = address,
                functionName = functionName,
                args = args,
            });
        }

        public async Task<Transaction> WriteTransaction(Contract contract, string functionName, object[] args)
        {
            var contractAddress = await TDK.Common.GetContractAddress(contract);
            return await WriteTransaction(
                address: contractAddress,
                functionName: functionName,
                args: args
            );
        }
    }
}
