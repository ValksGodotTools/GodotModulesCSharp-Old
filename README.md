# Godot Modules
## About
A collection of useful modules and scripts for C# Godot games.

## Why make this?
I was thinking to myself, I want to make a bullet hell game, but I am also going to be making more then just one game in the future. I do not want to redo the same things over again. If I want multiplayer, I can just grab it from here. If I want a modloader, I can find it here. That is the motivation behind this project.

## Table of Contents
- [Modules](#modules)
  - [ModLoader](#modloader)
  - [Netcode](#netcode)
  - [Options](#options)
- [Scripts](#scripts)
  - [Music Manager](#music-manager)
  - [File Manager](#file-manager)
  - [Scene Manager](#scene-manager)
  - [Game Manager](#game-manager)
  - [Utils](#utils)
- [Contributing](#contributing)

## Modules
### ModLoader
![image](https://user-images.githubusercontent.com/6277739/162651881-b8f98aa5-da2a-4499-b4dd-737a64dec4a9.png)  
This module was created to better understand MoonSharp (Lua) for implementing modding into a game. The project has evolved into a template to be used in other Godot C# games to add Lua modding support.

Learn Lua: https://www.lua.org/pil/contents.html  
Learn MoonSharp: https://www.moonsharp.org/getting_started.html  

#### Features
- Mods all share one Lua environment
- Mods can register callbacks without overwriting other mod callbacks
- Callbacks support params
- ModLoader can register function hooks anywhere
- Mods can see objects like Player and do stuff like Player:setHealth(x)
- Lua Debugger support
- Safety null checks to see if all properties were filled out in mod info.json
- Individual mods can be enabled or disabled
- Mod name, description, author, version, game versions, dependencies displayed in UI

#### Todo
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
- [x] Toggling mod dependency in mod info page should also toggle the mod dependency in the mod list (left side) (and vice versa)
- [x] Add a ModLoader logger
- [ ] Add button to toggle debug server
- [ ] Add website for displaying user-defined Lua documentation

#### Hooks
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

#### Mod Structure
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

#### Note
Just found out that this is a thing https://docs.godotengine.org/en/3.4/tutorials/export/exporting_pcks.html although as of writing this it will not work with C# projects because of this issue https://github.com/godotengine/godot/issues/36828. If your project is 100% GDScript then by all means use PCK files instead of this.

---

### Netcode
![image](https://user-images.githubusercontent.com/6277739/164528687-8ce3891f-2aa2-4c43-b9d2-404620aefad2.png)
![image](https://user-images.githubusercontent.com/6277739/164519290-fcd96048-3267-4278-bbd9-34bd7c0a86c0.png)
![image](https://user-images.githubusercontent.com/6277739/164519339-a23cc3be-29dd-4df8-ad3b-e975508f5ec8.png)
- [x] Server is shipped with client
- [x] Server browser
- [x] Join, leave lobbies
- [x] Lobbies are broadcasted to master nodejs server
- [x] Lobby chat
- [ ] Player positions
- [ ] Game events like player spawn, player bullet spawn, enemy bullet spawn events
- [ ] Fix all bugs

#### Threads
The client runs on 2 threads; the Godot thread and the ENet thread. Never run Godot code in the ENet thread and likewise never run ENet code in the Godot thread. If you ever need to communicate between the threads, use the proper `ConcurrentQueue`'s in `ENetClient.cs`.

#### Networking
The netcode utilizes [ENet-CSharp](https://github.com/SoftwareGuy/ENet-CSharp/blob/master/DOCUMENTATION.md), a reliable UDP networking library.

Never give the client any authority, the server always has the final say in everything. This should always be thought of when sending new packets.

Packets are sent like this (the way packets are read has a very similar setup)
```cs
namespace GodotModules.Netcode
{
    public class WPacketPlayerData : IWritable
    {
        public uint PlayerId { get; set; }
        public uint PlayerHealth { get; set; }
        public string PlayerName { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(PlayerHealth);
            writer.Write(PlayerName);
        }
    }
}

// Since packets are being enqueued to a ConcurrentQueue they can be called from any thread
GameClient.Send(ClientPacketOpcode.PlayerData, new WPacketPlayerData {
    PlayerId = 0,
    PlayerHealth = 100,
    PlayerName = "Steve"
});
```

Consider size of data types when sending them over the network https://condor.depaul.edu/sjost/nwdp/notes/cs1/CSDatatypes.htm (the smaller the better but keep it practical)

- [x] Post created servers to [NodeJS web server](https://github.com/valkyrienyanko/GodotListServers) / fetch all servers

---

### Tech Tree (coming soon)
Tech tree where nodes in tree are positioned automatically via script

### Options
![image](https://user-images.githubusercontent.com/6277739/163117944-e350b70c-aaaa-426f-8719-3c28648d5747.png)  

### Popup Menu (coming soon)
In games usually when you press Esc a popup menu comes up asking if you want to go back to the main menu or edit the options

## Scripts
### Music Manager
```cs
// Load and play music
MusicManager.Load("menu theme", pathToMusic);
MusicManager.Play("menu theme");
MusicManager.SetVolume(50) // value ranges from 0 to 100
MusicManager.Pause();
MusicManager.Resume();
```

### File Manager
```cs
// FileManager
SystemFileManager.GetProjectPath(); // non-exported = "res://", exported = next to the game exe
SystemFileManager.GetConfig(path);
SystemFileManager.WriteConfig<T>(path);
SystemFileManager.WriteConfig(path, data);
SystemFileManager.GetGameDataPath(); // gets AppData/Local/GameName/ path

// non-exported release returns res:// and exported release returns path next to games exe
GodotFileManager.GetProjectPath(); 

// all main scenes are put into res://Scenes/Scenes directory
var loadedScenes = GodotFileManager.LoadDir("Scenes/Scenes", (dir, fileName) =>
{
    if (!dir.CurrentIsDir())
        LoadScene(fileName.Replace(".tscn", ""));
});

if (loadedScenes)
    ChangeScene("Menu");
```

### Scene Manager
```cs
// All scenes are changed through the scene mananger, this allows for persistent nodes throughout scenes. 
// (for e.g. a debugger console that pops up when pressing F12)

SceneManager.ChangeScene("Menu");
SceneManager.ChangeScene("Game");
```

### Game Manager
```cs
// spawns a popup message in the center of the screen
GameManager.SpawnPopupMessage(string message);

// spawns a error message in the center of the screen
GameManager.SpawnPopupError(Exception e);

// Exit and do proper clean up (perhaps this should be under SceneManager)
GameManager.Exit();
```

![image](https://user-images.githubusercontent.com/6277739/164518782-328291c5-f96d-4ca1-b980-c01180ec6eb2.png)
![image](https://user-images.githubusercontent.com/6277739/164518875-4f769eb1-5c1e-44df-bf20-938b37843677.png)

### Utils
```cs
// Change scene to menu scene when ESC is pressed 
// (perhaps this should be under SceneManager)

Utils.EscapeToScene("Menu", () => {
    // optional code here
});

"hello world".ToTitleCase(); // Hello World
"helloWorld".AddSpaceBeforeEachCapitol(); // hello World

var list = new List<int>{1,2,3};
list.Print(); // 1, 2, 3

var dict = new Dictionary<int, int>{ {1,1}, {2,3} };
dict.Print(); // 1 1, 2 3

var someValue = 24f;
someValue.Remap(0, 100, -40, 80); //remap someValue from range 0-100 to range -40-80

var encryptedPassword = EncryptionHelper.Encrypt("epicPa55w0rd");
var password = EncryptionHelper.Decrypt(encryptedPassword);
```

## Contributing
Contributions / pull requests are very much welcomed. If you want to help contribute, get the project running on your PC and see how everything works. My code isn't the best and it would be nice if others peer reviewed it pointing out things I could do better. 

The following scripts need a peer review / major refactor. They look super messy and unorganized in their current state.
- [UICreateLobby.cs](https://github.com/valkyrienyanko/GodotModules/blob/main/Scripts/Scenes/Game%20Servers/UICreateLobby.cs)
- [Validator.cs](https://github.com/valkyrienyanko/GodotModules/blob/main/Scripts/Utils/Validator.cs)

[Setup Instructions](https://github.com/valkyrienyanko/GodotModules/blob/main/.github/CONTRIBUTING.md)

[Godot Modules Discord](https://discord.gg/866cg8yfxZ) (if you have any questions, contact me (valk#9904) though here)

## Credit
Thank you to [LazerGoat](https://github.com/LazerGoat) for pointing out bugs here and there
