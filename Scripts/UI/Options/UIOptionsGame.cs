using Godot;

namespace GodotModules
{
    public class UIOptionsGame : Control
    {
        private OptionsManager _optionsManager;

        public void PreInit(OptionsManager optionsManager)
        {
            _optionsManager = optionsManager;
        }

        public override void _Ready()
        {

        }
    }
}
