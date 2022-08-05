namespace GodotModules;

public class UIOptionsVideo : Control
{
    private Options _optionsManager;

    public void PreInit(Options optionsManager)
    {
        _optionsManager = optionsManager;
    }
}
