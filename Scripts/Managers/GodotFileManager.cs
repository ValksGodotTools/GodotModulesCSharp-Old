using Godot;

namespace GodotModules
{
    public class GodotFileManager
    {
        public string GetProjectPath() => OS.HasFeature("standalone")
            ? System.IO.Directory.GetParent(OS.GetExecutablePath()).FullName
            : ProjectSettings.GlobalizePath("res://");

        public bool LoadDir(string path, System.Action<Directory, string> action)
        {
            var dir = new Directory();

            if (dir.Open($"res://{path}") != Error.Ok)
            {
                Logger.LogWarning($"Failed to open {path}");
                return false;
            }

            dir.ListDirBegin(true);
            var fileName = dir.GetNext();
            
            while (fileName != "")
            {
                action(dir, fileName);
                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
            return true;
        }
    }
}