## Popups
Never make static references to Godot Popup Windows, changing the scene more than once will cause the code to access a old invalid reference and crash the game with `Could not access a disposed object`
