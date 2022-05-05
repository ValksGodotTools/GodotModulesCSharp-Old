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

// Change scene to menu scene when ESC is pressed 
SceneManager.EscapeToScene("Menu", () => {
    // optional code here
});
```

### Game Manager
```cs
// spawns a popup message in the center of the screen
GameManager.SpawnPopupMessage(string message);

// spawns a error message in the center of the screen
GameManager.SpawnPopupError(Exception e);

// Exit and do proper clean up
GameManager.Exit();
```

![image](https://user-images.githubusercontent.com/6277739/164518782-328291c5-f96d-4ca1-b980-c01180ec6eb2.png)
![image](https://user-images.githubusercontent.com/6277739/164518875-4f769eb1-5c1e-44df-bf20-938b37843677.png)

### Utils
```cs
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
