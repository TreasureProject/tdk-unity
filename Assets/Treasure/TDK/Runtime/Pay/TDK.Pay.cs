using UnityEngine;
using Thirdweb;
using Thirdweb.Pay;
using System.Numerics;
using Newtonsoft.Json;

namespace Treasure
{
    public partial class TDK : MonoBehaviour
    {
        public static Pay Pay { get; private set; }

        /// <summary>
        /// Initialize the Pay module
        /// </summary>
        private void InitPay()
        {
            Pay = new Pay();
        }
    }

    public class Pay
    {
        #region private vars
        private BuyWithFiatQuoteResult _quote;
        #endregion

        #region constructors
        public Pay() { }
        #endregion

        #region public api
        public async void GetFiatQuote(string currencySymbol, ChainId chainId, BigInteger amount, bool testMode)
        {
            Debug.Log("GetQuote");

            string connectedAddress = TDK.Identity.Address;

            _quote = null;

            var fiatQuoteParams = new BuyWithFiatQuoteParams(
                fromCurrencySymbol: currencySymbol,
                toAddress: connectedAddress,
                toChainId: ((int)chainId).ToString(),
                toTokenAddress: Utils.NativeTokenAddress,
                toAmount: amount.ToString(),
                isTestMode: testMode
            );
            Debug.Log($"Quote Params: {JsonConvert.SerializeObject(fiatQuoteParams, Formatting.Indented)}");

            _quote = await TDKServiceLocator.GetService<TDKThirdwebService>().SDK.Pay.GetBuyWithFiatQuote(fiatQuoteParams);
            ThirdwebDebug.Log($"Quote: {JsonConvert.SerializeObject(_quote, Formatting.Indented)}");
        }
        #endregion
    }
}
