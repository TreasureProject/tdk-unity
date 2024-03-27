# TDK Unity

## What is the Treasure Development Kit (TDK)
TDK Unity is exeprimental and in early development phase. It facilitates easy installation and integration with our internal client-side tools and vendor SDKs. It removes biolerplate integration code and abstracts interaction between game code and vendor SDKs.

#### What's in the box
- ### Treasure Connect
  A branded Treasure Login experience.

  ```
  TDK.Connect.Show();
  ```

- ### Analytics tracking
  Tracking of any client-side analytics events.
  
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
5. Add the `ThirdwebManager` prefab to your scene (located at `Assets/Thirdweb/Core/Prefabs/ThirdwebManager.prefab`).
6. Add either the `TDKConnectCanvasLandscape` or `TDKConnectCanvasPortrait` prefab to your scene (located in `Assets/Treasure/TDK/ConnectPrefabs/`).
