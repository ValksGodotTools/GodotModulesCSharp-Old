using Godot;

namespace GodotModules
{
    public static class GodotFileManager
    {
        public static string GetProjectPath()
        {
            string pathExeDir;

            if (OS.HasFeature("standalone")) // check if game is exported
                // set to exported release dir
                pathExeDir = $"{System.IO.Directory.GetParent(OS.GetExecutablePath()).FullName}";
            else
                // set to project dir
                pathExeDir = ProjectSettings.GlobalizePath("res://");

            return pathExeDir;
        }

        public static bool LoadDir(string path, System.Action<Directory, string> action)
        {
            var dir = new Directory();
            var error = dir.Open($"res://{path}");
            if (error != Error.Ok)
            {
                GM.Logger.LogWarning($"Failed to open {path}");
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