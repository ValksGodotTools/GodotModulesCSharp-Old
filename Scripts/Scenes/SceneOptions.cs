namespace GodotModules
{
	public class SceneOptions : AScene
	{
		private static OptionSection _currentSection = OptionSection.Game;

		[Export] protected readonly NodePath NodePathBtnGame;
		[Export] protected readonly NodePath NodePathBtnVideo;
		[Export] protected readonly NodePath NodePathBtnDisplay;
		[Export] protected readonly NodePath NodePathBtnAudio;
		[Export] protected readonly NodePath NodePathBtnControls;
		[Export] protected readonly NodePath NodePathBtnMultiplayer;

		[Export] protected readonly NodePath NodePathOptionsGame;
		[Export] protected readonly NodePath NodePathOptionsVideo;
		[Export] protected readonly NodePath NodePathOptionsDisplay;
		[Export] protected readonly NodePath NodePathOptionsAudio;
		[Export] protected readonly NodePath NodePathOptionsControls;
		[Export] protected readonly NodePath NodePathOptionsMultiplayer;

		private Button _btnGame;
		private Button _btnVideo;
		private Button _btnDisplay;
		private Button _btnAudio;
		private Button _btnControls;
		private Button _btnMultiplayer;
		private Dictionary<OptionSection, Control> _optionSections;

		private Managers _managers;

		public override void PreInitManagers(Managers managers)
		{
			_managers = managers;

			_optionSections = new Dictionary<OptionSection, Control>
			{
				[OptionSection.Game] = GetNode<UIOptionsGame>(NodePathOptionsGame),
				[OptionSection.Video] = GetNode<UIOptionsVideo>(NodePathOptionsVideo),
				[OptionSection.Display] = GetNode<UIOptionsDisplay>(NodePathOptionsDisplay),
				[OptionSection.Audio] = GetNode<UIOptionsAudio>(NodePathOptionsAudio),
				[OptionSection.Controls] = GetNode<UIOptionsControls>(NodePathOptionsControls),
				[OptionSection.Multiplayer] = GetNode<UIOptionsMultiplayer>(NodePathOptionsMultiplayer)
			};

			var options = managers.Options;

			((UIOptionsGame)_optionSections[OptionSection.Game]).PreInit(options);
			((UIOptionsVideo)_optionSections[OptionSection.Video]).PreInit(options);
			((UIOptionsDisplay)_optionSections[OptionSection.Display]).PreInit(options);
			((UIOptionsAudio)_optionSections[OptionSection.Audio]).PreInit(managers.Music, options);
			((UIOptionsControls)_optionSections[OptionSection.Controls]).PreInit(managers.ManagerHotkey);
			((UIOptionsMultiplayer)_optionSections[OptionSection.Multiplayer]).PreInit(options, managers.Web, managers.Tokens);
		}

		public override void _Ready()
		{
			_btnGame = GetNode<Button>(NodePathBtnGame);
			_btnVideo = GetNode<Button>(NodePathBtnVideo);
			_btnDisplay = GetNode<Button>(NodePathBtnDisplay);
			_btnAudio = GetNode<Button>(NodePathBtnAudio);
			_btnControls = GetNode<Button>(NodePathBtnControls);
			_btnMultiplayer = GetNode<Button>(NodePathBtnMultiplayer);

			switch (_currentSection)
			{
				case OptionSection.Game:
					_btnGame.GrabFocus();
					break;
				case OptionSection.Video:
					_btnVideo.GrabFocus();
					break;
				case OptionSection.Display:
					_btnDisplay.GrabFocus();
					break;
				case OptionSection.Audio:
					_btnAudio.GrabFocus();
					break;
				case OptionSection.Controls:
					_btnControls.GrabFocus();
					break;
				case OptionSection.Multiplayer:
					_btnMultiplayer.GrabFocus();
					break;
			}

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

			Notifications.AddListener(this, Event.OnKeyboardInput, (sender, args) => {
				_managers.ManagerScene.HandleEscape(async () => {
					_managers.Tokens.Cancel("check_connection");
					await _managers.ManagerScene.ChangeScene(GameScene.Menu);
				});
			});
		}

		private void SetFocusNeighborLeft(Control parent, Control target) => GetControlChildren(parent, child => child.FocusNeighbourLeft = target.GetPath());

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
