## Thread Safety
THREAD SAFETY IS NO JOKE. Really really weird things will happen if you don't follow thread safety (as seen [here](https://github.com/valkyrienyanko/GodotModules/issues/13)) :(

## Popups
Never make static references to Godot Popup Windows, changing the scene more than once will cause the code to access a old invalid reference and crash the game with `Could not access a disposed object`

## Removing and adding a child in the same frame
Adding and removing a child within the same frame almost always this leads to many errors (sometimes game crashing errors), and generally the work-a-around is to wait 1 idle_frame between removing and adding the child

## Changing the location of a script
Do not change the location of a script inside vscode, instead do it inside Godot. Changing the location of the script in Godot will update the necessary references unlike in Vscode.

## Renaming export node paths
Generally a good idea to keep the script as close as you can to the NodePaths so when renaming nodes you won't have to worry about re-assigning the nodepaths.
