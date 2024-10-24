# TDK Unity

The Treasure Development Kit streamlines client-side integration with the Treasure stack.

## Usage

Please refer to our [official documentation portal](https://docs.treasure.lol/tdk/unity/getting-started) for comprehensive installation, setup and integration details.

## Development

Download [Unity Hub and Unity](https://unity.com/download), set to editor version `2022.3.30f1`.

From the menu bar, select Treasure > TDK > Config and enter the initial configuration JSON provided by the Treasure team. To make manual edits after initial import, select Treasure > Edit Config.

## Deployment

1. Increment the build version in [TDKVersion.cs](./Assets/Treasure/TDK/Runtime/TDKVersion.cs) according to [Semantic Versioning notation](https://semver.org/)
2. Confirm that all third-party library versions have been updated in [versions.txt](./versions.txt).
3. Run the "Create Unity Package" workflow in GitHub Actions to create a new release with the package attached.
4. Update the release notes and publish.
