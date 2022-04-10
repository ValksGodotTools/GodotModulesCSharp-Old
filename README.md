# Godot Lua Modding Template
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

## Setting Up ModLoader On Your Project
### Initial setup
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

### Setting up scenes and scripts
This setup will assume your game has 2 scenes `Game.tscn` and `Menu.tscn`
1. Add `ui_left_click` mapped to left mouse click in `Project > Project Settings > Input Map`
2. Add `Modules/ModLoader/Scenes/Prefabs/ModLoader.tscn` to `Menu.tscn`
3. Add the following code to `Menu.tscn`
```cs
public override void _Ready()
{
    ModLoader.Init();
    ModLoader.LoadMods();

    UIModLoader.DisplayMods();
}
```
4. In `Game.tscn`, hooks can be made through the mod loader now
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
5. In `res://Scripts/Lua/*.lua` add all Lua game definitions here
```lua
Player = {} -- if you have a player class

function OnGameInit() end
function OnGameUpdate(delta) end

-- Modders need this so they won't overwrite functions their hooking into
-- Callback
function RegisterCallback(funcname, precallback, postcallback)
    --- fetch the original function
    local originalFunction = _G[funcname]
    --- wrap it
    _G[funcname] = function(self, ...)
        local arg = {...}

        --- call any prehook (this can change arguments but not return values)
        if precallback ~= nil then precallback(self, table.unpack(arg)) end
        --- call the original function save the result
        local result = originalFunction(self, table.unpack(arg))
        --- call any post hook, this can return a new result
        if postcallback ~= nil then postcallback(self, table.unpack(arg)) end
        -- return result
        return result
    end
end
```

### Setting up the mods
Every mod has `info.json` and `script.lua`

info.json
```json
{
    "name": "ModTest",
    "version": "0.0.1",
    "author": "Foo",
    "dependencies": ["Bar"],
    "description": "Example mod",
    "gameVersions": []
}
```

script.lua
```lua
-- example script of what you can do

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

Mod file structure
```
|-Mods
|--Foo
|---info.json
|---script.lua
|--Bar
|---info.json
|---script.lua
```

Mods Directory Location
- Exported Releases: `${GameExecutable}/Mods/...`
- Non-Exported Releases: `res://Mods/...`

Lua Scripts Location
- `res://Scripts/Lua/...`

Note that there is a demo scene you can play around with at `Modules/ModLoader/Scenes/Demo/D_Menu.tscn`

## Contributing
See [CONTRIBUTING.md](https://github.com/valkyrienyanko/GodotLuaModdingTest/blob/main/CONTRIBUTING.md)

## Note
Just found out that this is a thing https://docs.godotengine.org/en/3.4/tutorials/export/exporting_pcks.html although as of writing this it will not work with C# projects because of this issue https://github.com/godotengine/godot/issues/36828. If your project is 100% GDScript then by all means use PCK files instead of this.
