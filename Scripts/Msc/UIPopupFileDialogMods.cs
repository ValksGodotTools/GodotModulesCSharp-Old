using Godot;

namespace GodotModules
{
    public class UIPopupFileDialogMods : Control
    {
        [Export] public readonly NodePath NodePathFileManager;

        private FileDialog FileManager;

        public override void _Ready()
        {
            FileManager = GetNode<FileDialog>(NodePathFileManager);
        }

        public void Open()
        {
            FileManager.CurrentDir = ModLoader.ModLoader.PathMods;
            FileManager.PopupCentered();
        }

        private void _on_FileDialog_popup_hide()
        {
            QueueFree();
        }
    }
}