using Godot;

namespace GodotModules;

public class UIOptionsVideo : Control
{
    private OptionsManager _optionsManager;

    public void PreInit(OptionsManager optionsManager)
    {
        _optionsManager = optionsManager;
    }
}