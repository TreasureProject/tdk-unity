//------------------------------------------------------------------------------
// This code was generated by a tool.
//
//   Tool : MetaMask Unity SDK ABI Code Generator
//   Input filename:  ERC20PresetMinterPauser.sol
//   Output filename: ERC20PresetMinterPauserBacking.cs
//
// Changes to this file may cause incorrect behavior and will be lost when
// the code is regenerated.
// <auto-generated />
//------------------------------------------------------------------------------

#if UNITY_EDITOR || !ENABLE_MONO
using System;
using System.Numerics;
using System.Threading.Tasks;
using evm.net;
using evm.net.Models;

namespace MetaMask.Contracts
{
	public class ERC20PresetMinterPauserBacking : Contract, ERC20PresetMinterPauser
	{
		public ERC20PresetMinterPauserBacking(IProvider provider, EvmAddress address, Type interfaceType) : base(provider, address, interfaceType)
		{
		}
		public Task<ERC20PresetMinterPauser> DeployNew(String name, String symbol)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<ERC20PresetMinterPauser>) InvokeMethod(method, new object[] { name, symbol });
		}
		
		[EvmMethodInfo(Name = "DEFAULT_ADMIN_ROLE", View = true)]
		public Task<HexString> DEFAULT_ADMIN_ROLE()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<HexString>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "MINTER_ROLE", View = true)]
		public Task<HexString> MINTER_ROLE()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<HexString>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "PAUSER_ROLE", View = true)]
		public Task<HexString> PAUSER_ROLE()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<HexString>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "allowance", View = true)]
		public Task<BigInteger> Allowance(EvmAddress owner, EvmAddress spender)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<BigInteger>) InvokeMethod(method, new object[] { owner, spender });
		}
		
		[EvmMethodInfo(Name = "approve", View = false)]
		public Task<Transaction> Approve(EvmAddress spender, BigInteger amount)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { spender, amount });
		}
		
		[EvmMethodInfo(Name = "balanceOf", View = true)]
		public Task<BigInteger> BalanceOf(EvmAddress account)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<BigInteger>) InvokeMethod(method, new object[] { account });
		}
		
		[EvmMethodInfo(Name = "burn", View = false)]
		public Task<Transaction> Burn(BigInteger amount)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { amount });
		}
		
		[EvmMethodInfo(Name = "burnFrom", View = false)]
		public Task<Transaction> BurnFrom(EvmAddress account, BigInteger amount)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { account, amount });
		}
		
		[EvmMethodInfo(Name = "decimals", View = true)]
		[return: EvmParameterInfo(Type = "uint8")]
		public Task<UInt16> Decimals()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<UInt16>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "decreaseAllowance", View = false)]
		public Task<Transaction> DecreaseAllowance(EvmAddress spender, BigInteger subtractedValue)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { spender, subtractedValue });
		}
		
		[EvmMethodInfo(Name = "getRoleAdmin", View = true)]
		public Task<HexString> GetRoleAdmin(HexString role)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<HexString>) InvokeMethod(method, new object[] { role });
		}
		
		[EvmMethodInfo(Name = "getRoleMember", View = true)]
		public Task<EvmAddress> GetRoleMember(HexString role, BigInteger index)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<EvmAddress>) InvokeMethod(method, new object[] { role, index });
		}
		
		[EvmMethodInfo(Name = "getRoleMemberCount", View = true)]
		public Task<BigInteger> GetRoleMemberCount(HexString role)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<BigInteger>) InvokeMethod(method, new object[] { role });
		}
		
		[EvmMethodInfo(Name = "grantRole", View = false)]
		public Task<Transaction> GrantRole(HexString role, EvmAddress account)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { role, account });
		}
		
		[EvmMethodInfo(Name = "hasRole", View = true)]
		public Task<Boolean> HasRole(HexString role, EvmAddress account)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Boolean>) InvokeMethod(method, new object[] { role, account });
		}
		
		[EvmMethodInfo(Name = "increaseAllowance", View = false)]
		public Task<Transaction> IncreaseAllowance(EvmAddress spender, BigInteger addedValue)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { spender, addedValue });
		}
		
		[EvmMethodInfo(Name = "mint", View = false)]
		public Task<Transaction> Mint(EvmAddress to, BigInteger amount)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { to, amount });
		}
		
		[EvmMethodInfo(Name = "name", View = true)]
		public Task<String> Name()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<String>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "pause", View = false)]
		public Task<Transaction> Pause()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "paused", View = true)]
		public Task<Boolean> Paused()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Boolean>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "renounceRole", View = false)]
		public Task<Transaction> RenounceRole(HexString role, EvmAddress account)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { role, account });
		}
		
		[EvmMethodInfo(Name = "revokeRole", View = false)]
		public Task<Transaction> RevokeRole(HexString role, EvmAddress account)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { role, account });
		}
		
		[EvmMethodInfo(Name = "supportsInterface", View = true)]
		public Task<Boolean> SupportsInterface([EvmParameterInfo(Type = "bytes4", Name = "interfaceId")] Byte[] interfaceId)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Boolean>) InvokeMethod(method, new object[] { interfaceId });
		}
		
		[EvmMethodInfo(Name = "symbol", View = true)]
		public Task<String> Symbol()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<String>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "totalSupply", View = true)]
		public Task<BigInteger> TotalSupply()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<BigInteger>) InvokeMethod(method, new object[] {  });
		}
		
		[EvmMethodInfo(Name = "transfer", View = false)]
		public Task<Transaction> Transfer(EvmAddress to, BigInteger amount)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { to, amount });
		}
		
		[EvmMethodInfo(Name = "transferFrom", View = false)]
		public Task<Transaction> TransferFrom(EvmAddress from, EvmAddress to, BigInteger amount)
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] { from, to, amount });
		}
		
		[EvmMethodInfo(Name = "unpause", View = false)]
		public Task<Transaction> Unpause()
		{
			var method = System.Reflection.MethodBase.GetCurrentMethod();
			return (Task<Transaction>) InvokeMethod(method, new object[] {  });
		}
		
	}
}
#endif
