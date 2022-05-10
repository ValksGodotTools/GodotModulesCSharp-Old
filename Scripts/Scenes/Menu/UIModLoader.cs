using Godot;
using System;
using System.Diagnostics;

namespace GodotModules
{
    public class UIModLoader : Control
    {
        [Export] public readonly NodePath NodePathModList;
        [Export] public readonly NodePath NodePathModName;
        [Export] public readonly NodePath NodePathModGameVersions;
        [Export] public readonly NodePath NodePathModDependencies;
        [Export] public readonly NodePath NodePathModDescription;
        [Export] public readonly NodePath NodePathLogger;

        // mod list
        public VBoxContainer ModList { get; set; } // where the ModInfo children are added

        public Dictionary<string, UIModInfo> ModInfoList = new Dictionary<string, UIModInfo>(); // references to the ModInfo children added to the ModList VBoxContainer
        public Dictionary<string, UIModInfo> ModInfoDependencyList = new Dictionary<string, UIModInfo>(); // refernces to the ModInfo children in the dependency list if any

        // mod info
        public Label ModName { get; set; }

        public Label ModGameVersions { get; set; }
        public VBoxContainer ModDependencies { get; set; }
        public Label ModDescription { get; set; }

        // logger
        private RichTextLabel Logger { get; set; }

        public override void _Ready()
        {
            ModList = GetNode<VBoxContainer>(NodePathModList);
            ModName = GetNode<Label>(NodePathModName);
            ModGameVersions = GetNode<Label>(NodePathModGameVersions);
            ModDependencies = GetNode<VBoxContainer>(NodePathModDependencies);
            ModDescription = GetNode<Label>(NodePathModDescription);
            Logger = GetNode<RichTextLabel>(NodePathLogger);

            ModName.Text = "";
            ModGameVersions.Text = "";
            ModDescription.Text = "";
            Logger.Clear();

            GameManager.ModLoader = new ModLoader(this);
            GameManager.ModLoader.Init();
            GameManager.ModLoader.LoadMods();

            DisplayMods();
        }

        public void ClearLog() => Logger.Clear();

        public void Log(string text) => Logger.AddText($"{text}\n");

        public void UpdateModInfo(string name)
        {
            foreach (Node node in ModDependencies.GetChildren())
                node.QueueFree();

            var modInfo = GameManager.ModLoader.ModInfo[name].ModInfo;

            ModName.Text = name;

            var gameVersions = modInfo.GameVersions == null ? "" : string.Join(" ", modInfo.GameVersions);
            ModGameVersions.Text = $"Game Version(s): {gameVersions}";

            var description = modInfo.Description == null ? "" : modInfo.Description;
            ModDescription.Text = description;

            ModInfoDependencyList.Clear();

            modInfo.Dependencies.ForEach(dependency =>
            {
                var instance = CreateModInfoInstance(dependency);
                instance.DisplayedInDependencies = true;

                ModInfoDependencyList[dependency] = instance;

                ModDependencies.AddChild(instance);
            });
        }

        public void DisplayMods()
        {
            GameManager.ModLoader.LoadedMods.ForEach(mod =>
            {
                var modInfo = CreateModInfoInstance(mod.ModInfo.Name);
                ModList.AddChild(modInfo);
                ModInfoList[mod.ModInfo.Name] = modInfo;
            });

            if (GameManager.ModLoader.LoadedMods.Count > 0)
                UpdateModInfo(GameManager.ModLoader.LoadedMods[0].ModInfo.Name);
        }

        private UIModInfo CreateModInfoInstance(string modName)
        {
            var instance = Prefabs.ModInfo.Instance<UIModInfo>();
            instance.UIModLoader = this;
            instance.SetModName(modName);

            if (GameManager.ModLoader.ModInfo[modName].ModInfo.Enabled)
                instance.SetModEnabled(GameManager.ModLoader.ModInfo[modName].ModInfo.Enabled);
            else
                instance.SetModEnabled(false);

            return instance;
        }

        private void _on_Refresh_pressed()
        {
            GameManager.ModLoader.SetModsEnabled();

            foreach (Node node in ModList.GetChildren())
                node.QueueFree();

            GameManager.ModLoader.SortMods();
            DisplayMods();
        }

        private void _on_Load_Mods_pressed()
        {
            foreach (UIModInfo info in ModList.GetChildren())
            {
                var modName = info.ModName;
                var modEnabled = info.Enabled;

                GameManager.ModLoader.ModInfo[modName].ModInfo.Enabled = modEnabled;
            }

            GameManager.ModLoader.SetModsEnabled();
            GameManager.ModLoader.LoadMods();
        }

        private void _on_Open_Mods_Folder_pressed()
        {
            //var fileDialogPopup = Prefabs.PopupFileDialogMods.Instance<UIPopupFileDialogMods>();
            //AddChild(fileDialogPopup);
            //fileDialogPopup.Open();

            try
            {
                using (Process myProcess = new Process())
                {
                    myProcess.StartInfo.FileName = GameManager.ModLoader.PathMods;
                    myProcess.Start();
                }
            }
            catch (Exception e)
            {
                GameManager.Logger.LogErr(e);
            }
        }
    }
}