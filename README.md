# Compiling IHLFNModMenu from Source

This guide explains how to compile the **IN HEAT / Clover Mod Menu** on Windows.

## Requirements

1. **IN HEAT** installed through Steam.
2. **MelonLoader 0.7.3 (x64)** installed in the IN HEAT game folder.
3. **.NET 6 SDK (x64)** installed from Microsoft.
4. The mod source files:
   - `MenuMod.cs`
   - `InHeatMenu.csproj`

Visual Studio is optional. You can compile the project entirely through PowerShell.

## 1. Install and initialize MelonLoader

1. Close IN HEAT.
2. In Steam, right-click **IN HEAT** and select **Manage → Browse local files**.
3. Extract the MelonLoader 0.7.3 x64 files into the folder containing `IN HEAT.exe`.
4. Start the game once and wait until the title screen appears.
5. Close the game.

MelonLoader should now have generated this directory:

```text
IN HEAT\MelonLoader\Il2CppAssemblies
```

The project needs these generated assemblies to compile.

## 2. Check the source-project references

Open `InHeatMenu.csproj` in Notepad or another text editor. Its `<HintPath>` entries must point to your actual IN HEAT installation.

For example, if the game is installed here:

```text
E:\SteamLibrary\steamapps\common\IN HEAT
```

the reference paths should begin with:

```text
E:\SteamLibrary\steamapps\common\IN HEAT\MelonLoader
```

If your game is on another drive or Steam library, replace the beginning of every `<HintPath>` with the correct location. Do not change the filenames at the end of those paths.

## 3. Confirm that .NET 6 is installed

Open PowerShell and run:

```powershell
dotnet --version
```

The command should show a version beginning with `6.`. A newer SDK may also be capable of building the `net6.0` project if the required targeting pack is available.

## 4. Compile the mod

Open PowerShell inside the folder containing `MenuMod.cs` and `InHeatMenu.csproj`, then run:

```powershell
dotnet build .\InHeatMenu.csproj -c Release
```

When compilation succeeds, the finished mod will be located here:

```text
bin\Release\net6.0\IHLFNModMenu.dll
```

## 5. Install the compiled DLL

1. Close IN HEAT.
2. Copy `IHLFNModMenu.dll` into:

```text
IN HEAT\Mods
```

3. Remove older copies such as `InHeatMenu.dll` to prevent the mod from loading twice.
4. Start IN HEAT normally through Steam.
5. Press **F1** in-game to show or hide the mod menu.
6. Press **F2** if automated Sammy camera input ever needs to be released immediately.

## Troubleshooting

### Missing reference or metadata-file errors

Check every `<HintPath>` in `InHeatMenu.csproj`. Also ensure that IN HEAT was launched once after installing MelonLoader so `Il2CppAssemblies` was generated.

### `dotnet` is not recognized

Install the x64 .NET 6 SDK, close PowerShell, and open a new PowerShell window.

### The mod does not appear

- Confirm the DLL is directly inside `IN HEAT\Mods`.
- Confirm MelonLoader 0.7.3 starts when the game launches.
- Check `IN HEAT\MelonLoader\Latest.log` for errors.
- Ensure there is only one copy of the mod DLL in the `Mods` folder.

### The game was updated

Launch the updated game once so MelonLoader regenerates its IL2CPP assemblies, then rebuild the mod. A major game update may require source-code changes.

