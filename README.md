# Godot Modules
## About
A collection of useful modules and scripts that can easily be ported into other C# Godot games.

The goal of this project is to figure out all the tedious / not-so-obvious stuff in game development to make game development much easier.

I was thinking to myself, I want to make a bullet hell game, but I'm also going to be making more then just one game in the future, so that's why I created this project, so I don't have to redo the same things over and over again. If I want multiplayer, I can just grab it from here. If I want a modloader, I can find it here. That is my motivation behind this project.

## Modules
### [ModLoader](https://github.com/valkyrienyanko/GodotModules/blob/main/.github/MOD_LOADER.md)  
![image](https://user-images.githubusercontent.com/6277739/162651881-b8f98aa5-da2a-4499-b4dd-737a64dec4a9.png)  

### [Options](https://github.com/valkyrienyanko/GodotModules/blob/main/.github/OPTIONS.md)  
![image](https://user-images.githubusercontent.com/6277739/163117944-e350b70c-aaaa-426f-8719-3c28648d5747.png)  

### [Netcode](https://github.com/valkyrienyanko/GodotModules/blob/main/.github/NETCODE.md) (wip)
- [x] Server is shipped with client
- [x] Server browser
- [x] Join, leave lobbies
- [x] Lobbies are broadcasted to master nodejs server
- [x] Lobby chat
- [ ] Player positions
- [ ] Game events like player spawn, player bullet spawn, enemy bullet spawn events
- [ ] Fix all bugs

### Popup Menu (coming soon)
In games usually when you press Esc a popup menu comes up asking if you want to go back to the main menu or edit the options

### Tech Tree (coming soon)
Tech tree where nodes in tree are positioned automatically via script

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
See [CONTRIBUTING.md](https://github.com/valkyrienyanko/GodotModules/blob/main/.github/CONTRIBUTING.md)

Have a look at all the issues marked with -> [`Good First Issue`](https://github.com/valkyrienyanko/GodotModules/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22)

A lot of this is overwhelming at times, just making a minor contribution here and there is a huge motivator for me. Talk to me through Discord valk#9904 if you need more info on something.

Another way to contact me is by joining this [Discord](https://discord.gg/866cg8yfxZ)

## Credit
Thank you to [LazerGoat](https://github.com/LazerGoat) for pointing out bugs here and there
