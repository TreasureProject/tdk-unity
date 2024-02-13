# TDK Unity

## What is the Treasure Development Kit (TDK)
TDK Unity is exeprimental and in early development phase. It facilitates easy installation and integration with our internal client-side tools and vendor SDKs. It removes biolerplate integration code and abstracts interaction between game code and vendor SDKs.

#### What's in the box
- Analytics tracking 
  ```
  TDK.Analytics.TrackCustomEvent("custom_event", new Dictionary<string, object>
  {
      {"custom_event_key", "hello world"}
  });
  ```

## Installation
1. Import `tdk-unity_vX.Y.Z_Core.unitypackage` into your scene.
2. Import additional vendor modules into your scene:
	- `tdk-unity_vX.Y.Z_Helika.unitypackage`
	- `tdk-unity_vX.Y.Z_Thirdweb.unitypackage`
3. Add scripting defines for each vendor modules:
	- `TDK_HELIKA`
	- `TDK_THIRDWEB`
4. Select `Treasure` -> `TDK` -> `Config` from the menu and supply your provided SDK config json. Hit `Configure TDK` when done.

## Thirdweb Setup

1. Configure `ThirdwebManager`:
	1. Add `ThirdwebManager` to your scene.
	2. Configure `ThirdwebManager`
     - Set Active Chain.
     - Add `arbitrum-sepolia` to supported chain list.
     - Provide Client Id.
     - Provide `Factory Address` under `Smart Wallet Options`
     - Enable gasless
2. Configure `Prefab_ConnectWallet`:
	1. Add `Prefab_ConnectWallet` to your Scene.
	2. Modify enabled connections.
	3. Enable 'Use Smart Wallets'.
