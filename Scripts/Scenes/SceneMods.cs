using Godot;

namespace GodotModules
{
    public class SceneMods : AScene
    {
        [Export] public readonly NodePath NodePathModList;
        [Export] public readonly NodePath NodePathModName;
        [Export] public readonly NodePath NodePathModGameVersions;
        [Export] public readonly NodePath NodePathModDependencies;
        [Export] public readonly NodePath NodePathModDescription;
        [Export] public readonly NodePath NodePathModLoaderLogs;

        private Control _modList;
        private Label _modName;
        private Label _modGameVersions;
        private Control _modDependencies;
        private Label _modDescription;
        private RichTextLabel _modLoaderLogs;

        private Managers _managers;

        public override void PreInitManagers(Managers managers)
        {
            _managers = managers;
        }

        public override void _Ready()
        {
            _modList = GetNode<Control>(NodePathModList);
            _modName = GetNode<Label>(NodePathModName);
            _modGameVersions = GetNode<Label>(NodePathModGameVersions);
            _modDependencies = GetNode<Control>(NodePathModDependencies);
            _modDescription = GetNode<Label>(NodePathModDescription);
            _modLoaderLogs = GetNode<RichTextLabel>(NodePathModLoaderLogs);
        }

        private void _on_Refresh_pressed()
        {

        }

        private void _on_Load_Mods_pressed()
        {

        }

        private void _on_Open_Mods_Folder_pressed()
        {
            //OS.ShellOpen(appDataModsFolder);
        }
    }
}
