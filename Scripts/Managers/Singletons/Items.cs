namespace GodotModules;

public static class Items
{
    public readonly static Dictionary<string, Texture> Sprites = new Dictionary<string, Texture>();
    public readonly static Dictionary<string, SpriteFrames> AnimatedSprites = new Dictionary<string, SpriteFrames>();

    public static void Init()
    {
        var gfm = new GodotFileManager();
        gfm.LoadDir("Sprites/Items", (dir, name) =>
        {
            if (dir.CurrentIsDir() || name.Contains(".import"))
                return;

            LoadSprite(name);
        });

        gfm.LoadDir("Sprites/Items/Animated", (dir, name) =>
        {
            if (dir.CurrentIsDir() || name.Contains(".import"))
                return;

            LoadSpriteFrames(name);
        });
    }

    private static void LoadSpriteFrames(string name) =>
        AnimatedSprites.Add(name.Replace(".tres", ""), ResourceLoader.Load<SpriteFrames>($"res://Sprites/Items/Animated/{name}"));
    private static void LoadSprite(string name) =>
        Sprites.Add(name.Replace(".png", ""), ResourceLoader.Load<Texture>($"res://Sprites/Items/{name}"));
}
