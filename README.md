# GodotLuaModdingTest
This project was created to better understand MoonSharp (Lua) for implementing modding into a game.

https://www.moonsharp.org/getting_started.html  

## Todo
- [ ] Do not add mod if lua scripts did not compile and give some kind of feedback in console about this
- [ ] Do not add mod if info.json does not exist
- [ ] Add a game menu and list all mods / add stuff to manage / reload mods
- [ ] Figure out how to lua and how to mod lua and how to lua mod lua mod lua

## Setup
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

## Note to non-windows users
Since I'm on Windows 10 I'm going to be lazy and use `C:/Mods`. If you're not using Windows and you would like to contribute feel free to change the code to write to the directory `LuaModdingTest.csproj` is in. These paths are just for debugging anyways, the main path is next to the exported Godot build exe.
