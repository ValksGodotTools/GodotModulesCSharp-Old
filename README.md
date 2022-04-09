# GodotLuaModdingTest
## Note
Just found out that this is a thing https://docs.godotengine.org/en/3.4/tutorials/export/exporting_pcks.html although as of writing this it will not work with C# projects because of this issue https://github.com/godotengine/godot/issues/36828. If your project is 100% GDScript then by all means use PCK files instead of this.

## About
This project was created to better understand MoonSharp (Lua) for implementing modding into a game. The project has evolved into a template to be used in other Godot C# games to add Lua modding support.

Learn Lua: https://www.lua.org/pil/contents.html  
Learn MoonSharp: https://www.moonsharp.org/getting_started.html  

## Features
- Mods all share one Lua environment
- Mods can register callbacks without overwriting other mod callbacks
- Callbacks support params
- ModLoader can register function hooks anywhere
- Mods can see objects like Player and do stuff like Player:setHealth(x)
- Lua Debugger support
- Safety null checks to see if all properties were filled out in mod info.json
- Individual mods can be enabled or disabled
- Mod name, description, author, version, game versions, dependencies displayed in UI

## Todo
- [x] Figure out how to do something like `player:setHealth(x)` from Lua
- [x] Figure out how to add something like Godots `_process(float delta)` in Lua so modders can call a function every frame if so desired
- [x] Do not add mod if lua scripts did not compile and give some kind of feedback in console about this
- [x] Do not add mod if info.json does not exist
- [x] Lua debugger
- [x] Allow mods to interact with each other without overwriting one another
- [x] Callbacks with params
- [x] Figure out mod load order
- [x] Figure out mod dependencies
- [x] Display mods in menu
- [x] Add button to load mods
- [x] Add buttons to disable/enable individual mods
- [x] Give better feedback about mod dependencies
- [x] Rearrange project folder structure so it can easily be ported to another game
- [ ] Toggling mod dependency in mod info page should also toggle the mod dependency in the mod list (left side)
- [ ] Add website for displaying user-defined Lua documentation
- [ ] Add button to toggle debug server

![image](https://user-images.githubusercontent.com/6277739/162085875-b69e42d2-c7fe-46a3-a1fa-f96d0386336b.png)


## Setup
### Godot Mono (C#)
1. Install [Godot Mono 64 Bit](https://godotengine.org)
2. Install [.NET SDK from this link](https://dotnet.microsoft.com/en-us/download)
3. Install [.NET Framework 4.7.2](https://duckduckgo.com/?q=.net+framework+4.7.2)
4. Launch Godot through [VSCode](#vscode)
5. In Godot Editor > Editor Settings > Mono > Builds > Make sure `Build Tool` is set to `dotnet CLI`

If the startup scene is the main menu, the [game server](https://github.com/Raccoons-Rise-Up/server/blob/main/.github/CONTRIBUTING.md#setup) and [web server](https://github.com/Raccoons-Rise-Up/website/blob/main/.github/CONTRIBUTING.md) will need to be running to get past the login screen to the main game scene, otherwise you can change the startup scene to the main game scene by going to `Godot > Project Settings > Application > Run > Main Scene`.

### VSCode
VSCode is a UI friendly text editor for developers
1. Install [VSCode](https://code.visualstudio.com)
2. Install the following extensions for VSCode
    - [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
    - [C# Tools for Godot](https://marketplace.visualstudio.com/items?itemName=neikeq.godot-csharp-vscode)
    - [godot-tools](https://marketplace.visualstudio.com/items?itemName=geequlim.godot-tools)
    - [Mono Debug](https://marketplace.visualstudio.com/items?itemName=ms-vscode.mono-debug)
    - [MoonSharp Debug](https://marketplace.visualstudio.com/items?itemName=xanathar.moonsharp-debug)
3. Launch Godot through VSCode by hitting `F1` to open up VSCode command and run `godot tools: open workspace with godot editor` or simply click the `Open Godot Editor` button bottom right

## Setting Up ModLoader On Your Project
This setup will assume your game has 2 scenes `Game.tscn` and `Menu.tscn`
1. Copy the `Modules/` directory to `res://` of your project
2. Godot > Project > Tools > C# > Generate Solution
3. Add the following to `.csproj`
```xml
<ItemGroup>
  <PackageReference Include="MoonSharp" Version="2.0.0" />
  <PackageReference Include="MoonSharp.Debugger.VsCode" Version="2.0.0" />
  <PackageReference Include="Newtonsoft.Json" Version="13.0.1" /> <!--This is used because net472 does not have System.Text.Json-->
</ItemGroup>
```
4. Add `Modules/ModLoader/Scenes/Prefabs/ModLoader.tscn` to `Menu.tscn`
5. Add the following code to `Menu.tscn`
```cs
public override void _Ready()
{
    ModLoader.Init();
    ModLoader.LoadMods();

    UIModLoader.DisplayMods();
}
```
6. In `Game.tscn`, hooks can be made through the mod loader now
```cs
public override void _Ready()
{
    // if we had a Player class defined with SetHealth function, this is how you would link that function with Lua
    ModLoader.Script.Globals["Player", "setHealth"] = (Action<int>)D_Master.Player.SetHealth;

    ModLoader.Call("OnGameInit");
}

public override void _Process(float delta)
{
    ModLoader.Call("OnGameUpdate", delta);
}
```
7. The following can be done from Lua now
```lua
RegisterCallback('OnGameInit', nil, function()
    print('Game start')
end)

local x = 0

RegisterCallback('OnGameUpdate', nil, function(delta)
    print('Delta', delta)
    Player:setHealth(x)
    x = x + 1
end)
```

Mods Directory Location
- Exported Releases: `${GameExecutable}/Mods/...`
- Non-Exported Releases: `res://Mods/...`

Lua Scripts Location
- `res://Scripts/Lua/...`

Note that there is a demo scene you can play around with at `Modules/ModLoader/Scenes/Demo/D_Menu.tscn`

### Debugging
1. Launch the VSCode configuration `Play in Editor` (if configuration is set to this already then just press `F5`)
2. While the debugger is running in the editor and if you want to debug Lua scripts as well launch the VSCode configuration `MoonSharp Attach`
3. Place a debug point anywhere in Lua or C# script to start debugging
