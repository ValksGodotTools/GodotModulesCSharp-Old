# GodotLuaModdingTest
This project was created to better understand MoonSharp (Lua) for implementing modding into a game.

https://www.moonsharp.org/getting_started.html  
https://www.tutorialspoint.com/lua/lua_basic_syntax.htm  

## Todo
- [ ] Do not add mod if lua scripts did not compile and give some kind of feedback in console about this
- [ ] Do not add mod if info.json does not exist
- [ ] Add a game menu and list all mods / add stuff to manage / reload mods
- [ ] Figure out how to use Lua debugger
- [ ] Figure out how to do something like `player:setHealth(x)` from Lua
- [ ] Figure out how to add something like Godots `_process(float delta)` in Lua so modders can call a function every frame if so desired

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
3. Launch Godot through VSCode by hitting `F1` to open up VSCode command and run `godot tools: open workspace with godot editor` (to debug the game launch it through vscode by pressing `F5`)

### Lua
The mods directory is in `C:/Mods` for when the game is run through the editor (non-exported release)

Inside the mods directory create a new folder for your mod, e.g. `ModTest` and inside that folder create `info.json` and `script.lua`.

Something like below.

```json
{
    "name": "ModTest",
    "version": "0.0.1",
    "author": "valkyrienyanko"
}
```

```lua
-- defines a factorial function
function fact (n)
	if (n == 0) then
		return 1
	else
		return n*fact(n - 1)
	end
end

return fact(5)
```

Then run the game and watch the result of the factorio function get printed to console.

### Note to non-windows users
Since I'm on Windows 10 I'm going to be lazy and use `C:/Mods`. If you're not using Windows and you would like to contribute feel free to change the code to write to the directory `LuaModdingTest.csproj` is in. These paths are just for debugging anyways, the main path is next to the exported Godot build exe.
