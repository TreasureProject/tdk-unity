using UnityEngine;
using Thirdweb;
using Thirdweb.Pay;
using System.Numerics;
using Newtonsoft.Json;
using System;
using System.Linq;

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
        private string _intentId;
        private static bool _performingPostOnrampSwap;

        #endregion

        #region constructors

        public Pay() { }

        #endregion

        #region public api

        public async void GetFiatQuote(
            string currencySymbol,
            ChainId chainId,
            string decimalAmount,
            bool testMode
        )
        {
            Debug.Log("GetQuote");

            string connectedAddress = TDK.Identity.Address;

            _quote = null;

            var fiatQuoteParams = new BuyWithFiatQuoteParams(
                fromCurrencySymbol: currencySymbol,
                toAddress: connectedAddress,
                toChainId: ((int)chainId).ToString(),
                toTokenAddress: Utils.NativeTokenAddress,
                toAmount: decimalAmount,
                isTestMode: testMode
            );
            TDKLogger.Log(
                $"Quote Params: {JsonConvert.SerializeObject(fiatQuoteParams, Formatting.Indented)}"
            );

            _quote = await TDKServiceLocator
                .GetService<TDKThirdwebService>()
                .Pay.GetBuyWithFiatQuote(fiatQuoteParams);
            TDKLogger.Log($"Quote: {JsonConvert.SerializeObject(_quote, Formatting.Indented)}");
        }

        public void Buy()
        {
            if (_quote == null)
            {
                TDKLogger.Log("Get a quote first.");
                return;
            }

            try
            {
                _intentId = TDKServiceLocator
                    .GetService<TDKThirdwebService>()
                    .Pay.BuyWithFiat(_quote);
                TDKLogger.Log($"Intent ID: {_intentId}");
            }
            catch (System.Exception e)
            {
                TDKLogger.Log($"Error: {e.Message}");
            }
        }

        public async void GetStatus()
        {
            if (string.IsNullOrEmpty(_intentId))
            {
                TDKLogger.Log("Intent ID is empty. Please buy first.");
                return;
            }

            var status = await TDKServiceLocator
                .GetService<TDKThirdwebService>()
                .Pay.GetBuyWithFiatStatus(_intentId);

            if (!Enum.TryParse<OnRampStatus>(status.Status, out var onRampStatus))
            {
                onRampStatus = OnRampStatus.NONE;
            }

            /*
                NONE,
                PENDING_PAYMENT,
                PAYMENT_FAILED,
                PENDING_ON_RAMP_TRANSFER,
                ON_RAMP_TRANSFER_IN_PROGRESS,
                ON_RAMP_TRANSFER_COMPLETED,
                ON_RAMP_TRANSFER_FAILED,
                CRYPTO_SWAP_REQUIRED,
                CRYPTO_SWAP_COMPLETED,
                CRYPTO_SWAP_FALLBACK,
                CRYPTO_SWAP_IN_PROGRESS,
                CRYPTO_SWAP_FAILED,
            */

            switch (onRampStatus)
            {
                case OnRampStatus.PAYMENT_FAILED:
                case OnRampStatus.ON_RAMP_TRANSFER_FAILED:
                case OnRampStatus.CRYPTO_SWAP_FAILED:
                    TDKLogger.Log(
                        $"Failed! Reason: {status.FailureMessage} | Intent ID: {_intentId}"
                    );
                    break;
                case OnRampStatus.CRYPTO_SWAP_FALLBACK:
                    TDKLogger.Log(
                        $"Fallback! Two step process failed and user received fallback funds on the destination chain."
                    );
                    break;
                case OnRampStatus.CRYPTO_SWAP_REQUIRED:
                    if (_performingPostOnrampSwap)
                    {
                        TDKLogger.Log(
                            $"Status: {JsonConvert.SerializeObject(status, Formatting.Indented)}"
                        );
                        break;
                    }
                    else
                    {
                        TDKLogger.Log(
                            "OnRamp transfer completed. You may now use this intent id to trigger a BuyWithCrypto transaction and get to your destination token: "
                                + _intentId
                        );
                        _performingPostOnrampSwap = true;
                    }

                    // This is only necessary when you can't get to the destination token directly from the onramp

                    var swapQuoteParams = new BuyWithCryptoQuoteParams(
                        fromAddress: status.ToAddress,
                        fromChainId: status.Quote.OnRampToken.ChainId,
                        fromTokenAddress: status.Quote.OnRampToken.TokenAddress,
                        toTokenAddress: status.Quote.ToToken.TokenAddress,
                        toAmount: status.Quote.EstimatedToTokenAmount,
                        intentId: _intentId
                    );

                    var quote = await TDKServiceLocator
                        .GetService<TDKThirdwebService>()
                        .Pay.GetBuyWithCryptoQuote(swapQuoteParams);

                    TDKLogger.Log(
                        $"Quote: {JsonConvert.SerializeObject(quote, Formatting.Indented)}"
                    );

                    var txHash = await TDKServiceLocator
                        .GetService<TDKThirdwebService>()
                        .Pay.BuyWithCrypto(quote);

                    TDKLogger.Log($"Post-Onramp Swap Transaction hash: {txHash}");

                    // Don't need to update status, original intent id is still valid
                    break;
                default:
                    TDKLogger.Log(
                        $"Status: {JsonConvert.SerializeObject(status, Formatting.Indented)}"
                    );
                    break;
            }
        }

        public async void GetHistory()
        {
            string connectedAddress = TDK.Identity.Address;
            var history = await TDKServiceLocator
                .GetService<TDKThirdwebService>()
                .Pay.GetBuyHistory(connectedAddress, 0, 10);
            TDKLogger.Log(
                $"Full History: {JsonConvert.SerializeObject(history, Formatting.Indented)}"
            );
        }

        [ContextMenu("Get Supported Currencies")]
        public async void GetSupportedCurrencies()
        {
            var currencies = await TDKServiceLocator
                .GetService<TDKThirdwebService>()
                .Pay.GetBuyWithFiatCurrencies();
            TDKLogger.Log(
                $"Supported Currencies: {JsonConvert.SerializeObject(currencies, Formatting.Indented)}"
            );
        }

        #endregion
    }
}
