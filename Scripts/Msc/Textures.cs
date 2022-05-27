namespace GodotModules 
{
    public static class Textures 
    {
        public readonly static Texture MiniGodotChan = LoadSprite("Items/MiniGodotChan.png");
        public readonly static SpriteFrames Coin = LoadSpriteFrames("Items/Animated/Coin.tres");

        private static SpriteFrames LoadSpriteFrames(string path) => ResourceLoader.Load<SpriteFrames>($"res://Sprites/{path}");
        private static Texture LoadSprite(string path) => ResourceLoader.Load<Texture>($"res://Sprites/{path}");
    }
}