public partial class Abis {
    public const string magicswapRouterV2 = @"
[
  {
    ""inputs"": [
      {
        ""internalType"": ""address"",
        ""name"": ""_factory"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_WETH"",
        ""type"": ""address""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""constructor""
  },
  {
    ""inputs"": [],
    ""name"": ""MagicSwapV2InvalidPath"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""MagicSwapV2WrongAmountADeposited"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""MagicSwapV2WrongAmountBDeposited"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""MagicSwapV2WrongAmountDeposited"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""MagicSwapV2WrongAmounts"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UniswapV2RouterExcessiveInputAmount"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UniswapV2RouterExpired"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UniswapV2RouterInsufficientAAmount"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UniswapV2RouterInsufficientBAmount"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UniswapV2RouterInsufficientOutputAmount"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UniswapV2RouterInvalidPath"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UniswapV2RouterOnlyAcceptETHViaFallbackFromWETHContract"",
    ""type"": ""error""
  },
  {
    ""inputs"": [],
    ""name"": ""UnsupportedNft"",
    ""type"": ""error""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      {
        ""indexed"": true,
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""indexed"": false,
        ""internalType"": ""address"",
        ""name"": ""pair"",
        ""type"": ""address""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""indexed"": false,
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""vault"",
        ""type"": ""tuple""
      }
    ],
    ""name"": ""NFTLiquidityAdded"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      {
        ""indexed"": true,
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""indexed"": false,
        ""internalType"": ""address"",
        ""name"": ""pair"",
        ""type"": ""address""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""indexed"": false,
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""vault"",
        ""type"": ""tuple""
      }
    ],
    ""name"": ""NFTLiquidityRemoved"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      {
        ""indexed"": true,
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""indexed"": false,
        ""internalType"": ""address"",
        ""name"": ""pair"",
        ""type"": ""address""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""indexed"": false,
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""vaultA"",
        ""type"": ""tuple""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""indexed"": false,
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""vaultB"",
        ""type"": ""tuple""
      }
    ],
    ""name"": ""NFTNFTLiquidityAdded"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      {
        ""indexed"": true,
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""indexed"": false,
        ""internalType"": ""address"",
        ""name"": ""pair"",
        ""type"": ""address""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""indexed"": false,
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""vaultA"",
        ""type"": ""tuple""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""indexed"": false,
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""vaultB"",
        ""type"": ""tuple""
      }
    ],
    ""name"": ""NFTNFTLiquidityRemoved"",
    ""type"": ""event""
  },
  {
    ""inputs"": [],
    ""name"": ""BURN_ADDRESS"",
    ""outputs"": [
      {
        ""internalType"": ""address"",
        ""name"": """",
        ""type"": ""address""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""ONE"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": """",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""WETH"",
    ""outputs"": [
      {
        ""internalType"": ""address"",
        ""name"": """",
        ""type"": ""address""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address"",
        ""name"": ""tokenA"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""tokenB"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountADesired"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountBDesired"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountAMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountBMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""addLiquidity"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""liquidity"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address"",
        ""name"": ""token"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountTokenDesired"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountTokenMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETHMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""addLiquidityETH"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountToken"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETH"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""liquidity"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""payable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vault"",
        ""type"": ""tuple""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_tokenB"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountBDesired"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountBMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""addLiquidityNFT"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""lpAmount"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vault"",
        ""type"": ""tuple""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountETHMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""addLiquidityNFTETH"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountToken"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETH"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""lpAmount"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""payable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vaultA"",
        ""type"": ""tuple""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vaultB"",
        ""type"": ""tuple""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountAMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountBMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""addLiquidityNFTNFT"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""lpAmount"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collection"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenId"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amount"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""contract INftVault"",
        ""name"": ""_vault"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      }
    ],
    ""name"": ""depositVault"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountMinted"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""factory"",
    ""outputs"": [
      {
        ""internalType"": ""address"",
        ""name"": """",
        ""type"": ""address""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOut"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""reserveIn"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""reserveOut"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""pair"",
        ""type"": ""address""
      }
    ],
    ""name"": ""getAmountIn"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountIn"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountIn"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""reserveIn"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""reserveOut"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""pair"",
        ""type"": ""address""
      }
    ],
    ""name"": ""getAmountOut"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOut"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOut"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      }
    ],
    ""name"": ""getAmountsIn"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountIn"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      }
    ],
    ""name"": ""getAmountsOut"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""reserveA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""reserveB"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""quote"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""pure"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address"",
        ""name"": ""tokenA"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""tokenB"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""liquidity"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountAMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountBMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""removeLiquidity"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address"",
        ""name"": ""token"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""liquidity"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountTokenMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETHMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""removeLiquidityETH"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountToken"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETH"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address"",
        ""name"": ""token"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""liquidity"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountTokenMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETHMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""bool"",
        ""name"": ""approveMax"",
        ""type"": ""bool""
      },
      {
        ""internalType"": ""uint8"",
        ""name"": ""v"",
        ""type"": ""uint8""
      },
      {
        ""internalType"": ""bytes32"",
        ""name"": ""r"",
        ""type"": ""bytes32""
      },
      {
        ""internalType"": ""bytes32"",
        ""name"": ""s"",
        ""type"": ""bytes32""
      }
    ],
    ""name"": ""removeLiquidityETHWithPermit"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountToken"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETH"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vault"",
        ""type"": ""tuple""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_tokenB"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_lpAmount"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountAMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountBMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""bool"",
        ""name"": ""_swapLeftover"",
        ""type"": ""bool""
      }
    ],
    ""name"": ""removeLiquidityNFT"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vault"",
        ""type"": ""tuple""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_lpAmount"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountTokenMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountETHMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""bool"",
        ""name"": ""_swapLeftover"",
        ""type"": ""bool""
      }
    ],
    ""name"": ""removeLiquidityNFTETH"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountToken"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountETH"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vaultA"",
        ""type"": ""tuple""
      },
      {
        ""components"": [
          {
            ""internalType"": ""contract INftVault"",
            ""name"": ""token"",
            ""type"": ""address""
          },
          {
            ""internalType"": ""address[]"",
            ""name"": ""collection"",
            ""type"": ""address[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""tokenId"",
            ""type"": ""uint256[]""
          },
          {
            ""internalType"": ""uint256[]"",
            ""name"": ""amount"",
            ""type"": ""uint256[]""
          }
        ],
        ""internalType"": ""struct IMagicSwapV2Router.NftVaultLiquidityData"",
        ""name"": ""_vaultB"",
        ""type"": ""tuple""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_lpAmount"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountAMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountBMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""removeLiquidityNFTNFT"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address"",
        ""name"": ""tokenA"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""tokenB"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""liquidity"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountAMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountBMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""bool"",
        ""name"": ""approveMax"",
        ""type"": ""bool""
      },
      {
        ""internalType"": ""uint8"",
        ""name"": ""v"",
        ""type"": ""uint8""
      },
      {
        ""internalType"": ""bytes32"",
        ""name"": ""r"",
        ""type"": ""bytes32""
      },
      {
        ""internalType"": ""bytes32"",
        ""name"": ""s"",
        ""type"": ""bytes32""
      }
    ],
    ""name"": ""removeLiquidityWithPermit"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountA"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountB"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOut"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapETHForExactTokens"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""payable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collection"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenId"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amount"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""_path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapETHForNft"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""payable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOutMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapExactETHForTokens"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""payable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountIn"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOutMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapExactTokensForETH"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountIn"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOutMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapExactTokensForTokens"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collection"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenId"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amount"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountOutMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""_path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapNftForETH"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""payable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collectionIn"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenIdIn"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amountIn"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collectionOut"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenIdOut"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amountOut"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""_path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapNftForNft"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collection"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenId"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amount"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountOutMin"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""_path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapNftForTokens"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOut"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountInMax"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapTokensForExactETH"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountOut"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountInMax"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapTokensForExactTokens"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collection"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenId"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amount"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_amountInMax"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""address[]"",
        ""name"": ""_path"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""_deadline"",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""swapTokensForNft"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""amounts"",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""address[]"",
        ""name"": ""_collection"",
        ""type"": ""address[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_tokenId"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""uint256[]"",
        ""name"": ""_amount"",
        ""type"": ""uint256[]""
      },
      {
        ""internalType"": ""contract INftVault"",
        ""name"": ""_vault"",
        ""type"": ""address""
      },
      {
        ""internalType"": ""address"",
        ""name"": ""_to"",
        ""type"": ""address""
      }
    ],
    ""name"": ""withdrawVault"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""amountBurned"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""stateMutability"": ""payable"",
    ""type"": ""receive""
  }
]
    ";
}