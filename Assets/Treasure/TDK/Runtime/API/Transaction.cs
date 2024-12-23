using System;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Thirdweb;

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
        public string abi;
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
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            if (await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber))
            {
                return await thirdwebService.WriteTransaction(body);
            }
            body.backendWallet ??= TDK.AppConfig.GetBackendWallet();
            var response = await Post("/transactions", JsonConvert.SerializeObject(body));
            var transaction = JsonConvert.DeserializeObject<Transaction>(response);
            return await TDK.Common.WaitForTransaction(transaction.queueId);
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

        public async Task<Transaction> SendRawTransaction(SendRawTransactionBody body)
        {
            var thirdwebService = TDKServiceLocator.GetService<TDKThirdwebService>();
            if (await thirdwebService.IsZkSyncChain(TDK.Connect.ChainIdNumber))
            {
                return await thirdwebService.WriteTransactionRaw(body);
            }
            body.backendWallet ??= TDK.AppConfig.GetBackendWallet();
            var response = await Post("/transactions/raw", JsonConvert.SerializeObject(body));
            var transaction = JsonConvert.DeserializeObject<Transaction>(response);
            return await TDK.Common.WaitForTransaction(transaction.queueId);
        }
    }
}
