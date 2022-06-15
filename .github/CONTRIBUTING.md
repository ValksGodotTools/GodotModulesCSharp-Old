# Contributing
Contributions and pull requests are very much welcomed. If you want to help contribute, get the project running on your PC and see how everything works. My code isn't the best and it would be nice if others peer reviewed it and pointed out things I could do better. 


> ⚠️ **IMPORTANT**: The whole project is being done from scratch on the [dev branch](https://github.com/GodotModules/GodotModulesCSharp/tree/dev).


You can discuss about the project in the [Godot Modules Discord Server](https://discord.gg/866cg8yfxZ). If you have any questions, contact `valk#9904` though the server.


## Setup


### Godot Mono (C#)
1. Install [Godot Mono 64 Bit](https://godotengine.org)
2. Install [.NET SDK from this link](https://dotnet.microsoft.com/en-us/download)
3. Install [.NET Framework 4.7.2](https://duckduckgo.com/?q=.net+framework+4.7.2)
4. Launch Godot through [VSCode](#vscode)
5. In `Godot Editor > Editor Settings > Mono > Builds`: Make sure `Build Tool` is set to `dotnet CLI`

The Godot startup scene should be set to `res://Scenes/Main.tscn`, if it is not then the game server and web server will not start and a lot of other code that needs to be initialized will not be initialized. To fix this go to `Godot > Project Settings > Application > Run > Main Scene` and set it to the main scene.

### VSCode
VSCode is a UI friendly text editor for developers
1. Install [VSCode](https://code.visualstudio.com)
2. Install the following extensions for VSCode
    - [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
    - [C# Tools for Godot](https://marketplace.visualstudio.com/items?itemName=neikeq.godot-csharp-vscode)
    - [godot-tools](https://marketplace.visualstudio.com/items?itemName=geequlim.godot-tools)
    - [Mono Debug](https://marketplace.visualstudio.com/items?itemName=ms-vscode.mono-debug)
    - [MoonSharp Debug](https://marketplace.visualstudio.com/items?itemName=xanathar.moonsharp-debug) (only if debugging lua)
3. Launch Godot through VSCode by hitting `F1` to open up VSCode command and run `godot tools: open workspace with godot editor` or simply click the `Open Godot Editor` button bottom right

### GitHub
1. Fork this repo
2. Clone your fork with `git clone https://github.com/<USERNAME>/GodotModules` (replace `<USERNAME>` with your GitHub username) 
    - *If you get `'git' is not recognized as an internal or external command` then install [Git scm](https://git-scm.com/downloads)*
3. Extract the zip and open the folder in VSCode
4. Go to the source control tab
5. Click the 3 dots icon, click `Checkout to...`, switch to the `dev` branch
    - *If you want to see the netcode working in action then stay on `main`*
6. All the files you make changes to should appear here as well, you can stage the files you want to commit, give the commit a message and then push it to your fork
7. Once you have some commits on your fork, you can go [here](https://github.com/GodotModules/GodotModulesCSharp/pulls) and open up a new pull request and request to merge your work with the main repo

### Common Mistakes
Some common mistakes are listed in [this](https://github.com/valkyrienyanko/GodotModules/blob/main/.github/COMMON_MISTAKES.md) page.


## Exporting
Do not forget to copy enet.dll to exported release folder
