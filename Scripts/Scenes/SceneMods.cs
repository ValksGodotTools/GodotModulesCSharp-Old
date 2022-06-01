namespace GodotModules
{
    public class SceneMods : AScene
    {
        [Export] protected readonly NodePath NodePathModList;
        [Export] protected readonly NodePath NodePathModName;
        [Export] protected readonly NodePath NodePathModGameVersions;
        [Export] protected readonly NodePath NodePathModDependencies;
        [Export] protected readonly NodePath NodePathModDescription;
        [Export] protected readonly NodePath NodePathModLoaderLogs;

        public Control ModList { get; private set; }
        public Label ModName { get; private set; }
        public Label GameVersions { get; private set; }
        public Control DependencyList { get; private set; }
        public Label Description { get; private set; }

        public Dictionary<string, UIModBtn> ModBtnsLeft { get; private set; }
        public Dictionary<string, UIModBtn> ModBtnsRight { get; private set; }

        private RichTextLabel _modLoaderLogs;
        private Managers _managers;

        public override void PreInitManagers(Managers managers)
        {
            _managers = managers;
        }

        public override void _Ready()
        {
            ModList = GetNode<Control>(NodePathModList);

            ModName = GetNode<Label>(NodePathModName);
            GameVersions = GetNode<Label>(NodePathModGameVersions);
            DependencyList = GetNode<Control>(NodePathModDependencies);
            Description = GetNode<Label>(NodePathModDescription);

            ModBtnsLeft = new Dictionary<string, UIModBtn>();
            ModBtnsRight = new Dictionary<string, UIModBtn>();
            
            _modLoaderLogs = GetNode<RichTextLabel>(NodePathModLoaderLogs);
            _modLoaderLogs.Text = ModLoader.ModLoaderLogs;

            UpdateUI();
            ModLoader.SceneMods = this;

            Notifications.AddListener(this, Event.OnKeyPressed, (sender, args) => {
                _managers.ManagerScene.HandleEscape(async () => {
                    ModLoader.SceneMods = null;
				    await _managers.ManagerScene.ChangeScene(GameScene.Menu);
                });
            });
        }

        public void Log(string text) => _modLoaderLogs.AddText($"{text}\n");

        private void UpdateUI()
        {
            var modBtns = new List<UIModBtn>();

            foreach (Control child in ModList.GetChildren())
                child.QueueFree();

            ModBtnsLeft.Clear();

            foreach (var pair in ModLoader.Mods) 
            {
                var modBtn = Prefabs.UIModBtn.Instance<UIModBtn>();
                ModBtnsLeft.Add(pair.Key, modBtn);
                modBtn.PreInit(this);
                ModList.AddChild(modBtn);
                modBtn.SetModName(pair.Key);
                modBtns.Add(modBtn);
                modBtn.SetEnabled(pair.Value.ModInfo.Enabled);
            }

            if (modBtns.Count > 0)
                modBtns[0].SetInfo();
        }

        private void _on_Refresh_pressed()
        {
            ModLoader.SaveEnabled();
            ModLoader.GetMods();
            UpdateUI();
        }

        private void _on_Load_Mods_pressed()
        {
            ModLoader.ModLoaderLogs = "";
            _modLoaderLogs.Clear();
            ModLoader.LoadMods();
        }

        private void _on_Open_Mods_Folder_pressed()
        {
            OS.ShellOpen(ModLoader.PathModsFolder);
        }
    }
}
