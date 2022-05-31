namespace GodotModules
{
    public class UIOptionsGame : Control
    {
        [Export] protected readonly NodePath NodePathColorPlayer;
        [Export] protected readonly NodePath NodePathColorEnemy;
        [Export] protected readonly NodePath NodePathColorChatText;

        private Options _optionsManager;
        private OptionColors _optionColors;

        public void PreInit(Options optionsManager)
        {
            _optionsManager = optionsManager;
            _optionColors = _optionsManager.Data.Colors;
        }

        public override void _Ready()
        {
            GetNode<ColorPickerButton>(NodePathColorPlayer).Color = new Color(_optionColors.Player);
            GetNode<ColorPickerButton>(NodePathColorEnemy).Color = new Color(_optionColors.Enemy);
            GetNode<ColorPickerButton>(NodePathColorChatText).Color = new Color(_optionColors.ChatText);
        }

        private void _on_Color_Player_color_changed(Color color) 
        {
            _optionColors.Player = color.ToHtml();
            _optionsManager.Data.Colors = _optionColors;
        }

        private void _on_Color_Enemy_color_changed(Color color)
        {
            _optionColors.Enemy = color.ToHtml();
            _optionsManager.Data.Colors = _optionColors;
        }

        private void _on_Color_Chat_Text_color_changed(Color color)
        {
            _optionColors.ChatText = color.ToHtml();
            _optionsManager.Data.Colors = _optionColors;
        }
    }
}
