## Scripts
### Notifications
![image](https://user-images.githubusercontent.com/6277739/173916833-1e9caa62-62d5-4239-843e-7b28dfba5788.png)
![image](https://user-images.githubusercontent.com/6277739/173916937-bc433fc0-c1e0-44c5-8a88-0cd3896a3f06.png)

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
// string extensions
"helloWorld".AddSpaceBeforeEachCapitol(); // hello World

"player_move_up".Replace("_", " ").ToTitleCase().SmallWordsToUpper(2, (word) => {
    var words = new string[] {"Up", "In"};
    return !words.Contains(word);
}); // Player Move UP

// collection extensions
var list = new List<int>{1,2,3};
list.Print(); // 1, 2, 3

var dict = new Dictionary<int, int>{ {1,1}, {2,3} };
dict.Print(); // 1 1, 2 3

// utils
var someValue = 24f;
someValue.Remap(0, 100, -40, 80); //remap someValue from range 0-100 to range -40-80

Vector2 newPos = Utils.Lerp(playerPos, targetPos, 0.1f);

Vector2 randomDir = Utils.RandomDir();
enemy.Position = randomDir * 10;

myList.ForEach(x => x.DoSomething()); // functional programming with ForEach extension (do not abuse as hard to debug)

// timers
var gTimer = new GTimer(this, nameof(Method)); // wrapper for Godot timer
var sTimer = new STimer(action); // wrapper for System timer
sTimer.Dispose();

// filters for LineEdit nodes with realtime feedback
inputName.Filter((text) => text.IsMatch("^[A-Za-z ]+$"));
inputPort.FilterRange(ushort.MaxValue);

// quick and dirty encryption
var encryptedPassword = EncryptionHelper.Encrypt("epicPa55w0rd");
var password = EncryptionHelper.Decrypt(encryptedPassword);
```
