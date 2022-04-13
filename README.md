# Godot Modules
A collection of useful modules and scripts that can easily be ported into other C# Godot games.

## Modules
### [ModLoader](https://github.com/valkyrienyanko/GodotModules/blob/main/MOD_LOADER.md)  
![image](https://user-images.githubusercontent.com/6277739/162651881-b8f98aa5-da2a-4499-b4dd-737a64dec4a9.png)  

### [Options](https://github.com/valkyrienyanko/GodotModules/blob/main/OPTIONS.md)  
![image](https://user-images.githubusercontent.com/6277739/163117944-e350b70c-aaaa-426f-8719-3c28648d5747.png)  

### ENet-CSharp (wip)
Realtime client-side checks  
![image](https://user-images.githubusercontent.com/6277739/163118366-42523efa-33ab-4b0e-939f-3fba74618c83.png)  
Server browser  
![image](https://user-images.githubusercontent.com/6277739/163118505-7f47f22e-94a8-44ab-ad56-18bafd44c149.png)  

Todo
- [x] Thread safety, run client on one thread, server on another
- [ ] Server list scene
- [ ] Post created servers to web server / fetch all servers from web server
- [ ] Ping servers
- [ ] Lobby scene
- [ ] Demo netcode for game scene

### Popup Menu (coming soon)
In games usually when you press Esc a popup menu comes up asking if you want to go back to the main menu or edit the options

### Tech Tree (coming soon)
Tech tree where nodes in tree are positioned automatically via script

## Utils
On top of all the modules there are also useful manager / utility scripts in `res://Global/` and `res://Scripts/`

```cs
// Load and play music
MusicManager.Load("menu theme", pathToMusic);
MusicManager.Play("menu theme");
MusicManager.SetVolume(50) // value ranges from 0 to 100
MusicManager.Pause();
MusicManager.Resume();

// FileManager
FileManager.GetProjectPath(); // non-exported = "res://", exported = next to the game exe
FileManager.GetConfig(path);
FileManager.WriteConfig<T>(path);
FileManager.WriteConfig(path, data);

// Utils
var someValue = 24f;
someValue.Remap(0, 100, -40, 80); //remap someValue from range 0-100 to range -40-80

// Encryption (this is not meant to be secure, it's just another annoyance to add to make mischief slightly harder)
var encryptedPassword = EncryptionHelper.Encrypt("epicPa55w0rd");
var password = EncryptionHelper.Decrypt(encryptedPassword);

// Exit and do clean up
GameManager.Exit();
```

## Contributing
See [CONTRIBUTING.md](https://github.com/valkyrienyanko/GodotLuaModdingTest/blob/main/CONTRIBUTING.md)
