using Godot;

namespace GodotModules;

public class UIOptionsDisplay : Control
{
    [Export] protected readonly NodePath NodePathFullscreen;
    [Export] protected readonly NodePath NodePathVSync;
    [Export] protected readonly NodePath NodePathWindowSizeWidth;
    [Export] protected readonly NodePath NotePathWindowSizeHeight;
    private LineEdit _windowSizeWidth;
    private LineEdit _windowSizeHeight;
    private Vector2 _windowSize;

    private OptionsManager _optionsManager;

    public void PreInit(OptionsManager optionsManager)
    {
        _optionsManager = optionsManager;
    }

    public override void _Ready()
    {
        var options = _optionsManager.Options;

        var fullscreen = GetNode<OptionButton>(NodePathFullscreen);
        fullscreen.AddItem("Windowed");
        fullscreen.AddItem("Borderless");
        fullscreen.AddItem("Exclusive Fullscreen");
        fullscreen.Selected = (int)options.FullscreenMode;

        _windowSizeWidth = GetNode<LineEdit>(NodePathWindowSizeWidth);
        _windowSizeHeight = GetNode<LineEdit>(NotePathWindowSizeHeight);

        var windowWidth = options.WindowSize.x;
        var windowHeight = options.WindowSize.y;

        GetNode<CheckBox>(NodePathVSync).Pressed = options.VSync;
        _windowSizeWidth.Text = "" + windowWidth;
        _windowSizeHeight.Text = "" + windowHeight;
        _windowSize = new Vector2(windowWidth, windowHeight);
    }

    private void _on_VSync_toggled(bool v)
    {
        _optionsManager.SetVSync(v);
    }

    private void _on_Fullscreen_item_selected(int v)
    {
        _optionsManager.SetFullscreenMode((FullscreenMode)v);
    }

    private void _on_Window_Size_Width_text_changed(string text) 
    {
        _windowSize.x = _windowSizeWidth.FilterRange((int)OS.GetScreenSize().x);
    }

    private void _on_Window_Size_Height_text_changed(string text)
    {
        _windowSize.y = _windowSizeHeight.FilterRange((int)OS.GetScreenSize().y);
    }

    private void _on_Set_Window_Size_pressed()
    {
        if (_optionsManager.Options.FullscreenMode != FullscreenMode.Windowed)
            _optionsManager.SetFullscreenMode(FullscreenMode.Windowed);
            
        OS.WindowSize = _windowSize;
        _optionsManager.CenterWindow();
    }
}