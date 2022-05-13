using Godot;
using System;

namespace GodotModules
{
    public class UIOptions : Node
    {
        private static OptionSection _currentSection = OptionSection.Game;

        [Export] public readonly NodePath NodePathBtnGame;
        [Export] public readonly NodePath NodePathBtnVideo;
        [Export] public readonly NodePath NodePathBtnDisplay;
        [Export] public readonly NodePath NodePathBtnAudio;
        [Export] public readonly NodePath NodePathBtnControls;
        [Export] public readonly NodePath NodePathBtnMultiplayer;

        [Export] public readonly NodePath NodePathOptionsGame;
        [Export] public readonly NodePath NodePathOptionsVideo;
        [Export] public readonly NodePath NodePathOptionsDisplay;
        [Export] public readonly NodePath NodePathOptionsAudio;
        [Export] public readonly NodePath NodePathOptionsControls;
        [Export] public readonly NodePath NodePathOptionsMultiplayer;

        private Button _btnGame;
        private Button _btnVideo;
        private Button _btnDisplay;
        private Button _btnAudio;
        private Button _btnControls;
        private Button _btnMultiplayer;
        private Dictionary<OptionSection, Control> _optionSections;

        public void PreInit(HotkeyManager hotkeyManager, OptionsManager optionsManager, MusicManager musicManager)
        {
            _optionSections = new();
            _optionSections[OptionSection.Game] = GetNode<UIOptionsGame>(NodePathOptionsGame);
            _optionSections[OptionSection.Video] = GetNode<UIOptionsVideo>(NodePathOptionsVideo);
            _optionSections[OptionSection.Display] = GetNode<UIOptionsDisplay>(NodePathOptionsDisplay);
            _optionSections[OptionSection.Audio] = GetNode<UIOptionsAudio>(NodePathOptionsAudio);
            _optionSections[OptionSection.Controls] = GetNode<UIOptionsControls>(NodePathOptionsControls);
            _optionSections[OptionSection.Multiplayer] = GetNode<UIOptionsMultiplayer>(NodePathOptionsMultiplayer);

            ((UIOptionsDisplay)_optionSections[OptionSection.Display]).PreInit(optionsManager);
            ((UIOptionsAudio)_optionSections[OptionSection.Audio]).PreInit(musicManager, optionsManager);
            ((UIOptionsControls)_optionSections[OptionSection.Controls]).PreInit(hotkeyManager);
        }

        public override void _Ready()
        {
            _btnGame = GetNode<Button>(NodePathBtnGame);
            _btnVideo = GetNode<Button>(NodePathBtnVideo);
            _btnDisplay = GetNode<Button>(NodePathBtnDisplay);
            _btnAudio = GetNode<Button>(NodePathBtnAudio);
            _btnControls = GetNode<Button>(NodePathBtnControls);
            _btnMultiplayer = GetNode<Button>(NodePathBtnMultiplayer);
            _btnGame.GrabFocus();
            ShowSection(_currentSection);

            foreach (var pair in _optionSections)
            {
                var section = pair.Value;

                switch (pair.Key)
                {
                    case OptionSection.Game:
                        SetFocusNeighborLeft(section, _btnGame);
                        break;
                    case OptionSection.Video:
                        SetFocusNeighborLeft(section, _btnVideo);
                        break;
                    case OptionSection.Display:
                        SetFocusNeighborLeft(section, _btnDisplay);
                        break;
                    case OptionSection.Audio:
                        SetFocusNeighborLeft(section, _btnAudio);
                        break;
                    case OptionSection.Controls:
                        SetFocusNeighborLeft(section, _btnControls);
                        break;
                    case OptionSection.Multiplayer:
                        SetFocusNeighborLeft(section, _btnMultiplayer);
                        break;
                }
            }
        }

        private void SetFocusNeighborLeft(Control parent, Control target) => GetControlChildren(parent, (child) => child.FocusNeighbourLeft = target.GetPath());

        private void GetControlChildren(Node node, Action<Control> code)
        {
            foreach (Node child in node.GetChildren())
            {
                if (child is Control control)
                    code(control);
                GetControlChildren(child, code);
            }
        }

        private void _on_Game_focus_entered() => ShowSection(OptionSection.Game);
        private void _on_Video_focus_entered() => ShowSection(OptionSection.Video);
        private void _on_Display_focus_entered() => ShowSection(OptionSection.Display);
        private void _on_Audio_focus_entered() => ShowSection(OptionSection.Audio);
        private void _on_Controls_focus_entered() => ShowSection(OptionSection.Controls);
        private void _on_Multiplayer_focus_entered() => ShowSection(OptionSection.Multiplayer);

        private void ShowSection(OptionSection section)
        {
            void HideAllSections() => _optionSections.ForEach(x => x.Value.Hide());

            HideAllSections();
            _optionSections[section].Visible = true;
            _currentSection = section;
        }

        private enum OptionSection
        {
            Game,
            Video,
            Display,
            Audio,
            Controls,
            Multiplayer
        }
    }
}
