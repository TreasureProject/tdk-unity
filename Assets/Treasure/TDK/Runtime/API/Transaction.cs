using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        public object[] abi;
        public string functionName;
        public object[] args;
        public TransactionOverrides txOverrides;
        public string backendWallet;
    }

    [Serializable]
    public class SendRawTransactionBody
    {
        public struct TransactionOverrides
        {
            public string gas;
            public string maxFeePerGas;
            public string maxPriorityFeePerGas;
        }

        public string to;
        public string value;
        public string data;
        public TransactionOverrides txOverrides;
        public string backendWallet;
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
            body.backendWallet ??= TDK.AppConfig.GetBackendWallet();
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
            var contractAddress = TDK.Common.GetContractAddress(contract);
            return await WriteTransaction(
                address: contractAddress,
                functionName: functionName,
                args: args
            );
        }

        public async Task<Transaction> SendRawTransaction(SendRawTransactionBody body)
        {
            body.backendWallet ??= TDK.AppConfig.GetBackendWallet();
            var response = await Post("/transactions/raw", JsonConvert.SerializeObject(body));
            return JsonConvert.DeserializeObject<Transaction>(response);
        }
    }
}
