namespace GodotModules 
{
    public static class Textures 
    {
        public readonly static Texture MiniGodotChan = Load("Items/MiniGodotChan.png");
        
        /*private static AnimatedTexture GetCoin()
        {
            var coinFrames = ResourceLoader.Load<SpriteFrames>("res://Msc/CoinFrames.tres");
            var animatedTexture = new AnimatedTexture();

            foreach (Texture coinFrame in coinFrames.Frames)
            {
                //animatedTexture.SetFrameTexture(0, null);
            }
            
        }*/

        private static Texture Load(string path) => ResourceLoader.Load<Texture>($"res://Scenes/Prefabs/{path}");
    }
}