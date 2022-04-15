## Thread Safety
THREAD SAFETY IS NO JOKE. Really really weird things will happen if you don't follow thread safety (as seen [here](https://github.com/valkyrienyanko/GodotModules/issues/13)) :(

## Popups
Never make static references to Godot Popup Windows, changing the scene more than once will cause the code to access a old invalid reference and crash the game with `Could not access a disposed object`
