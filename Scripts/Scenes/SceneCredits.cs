

namespace GodotModules
{
    public class SceneCredits : AScene
    {
        [Export] protected readonly NodePath NodePathTween;
        [Export] protected readonly NodePath NodePathCreditsContent;
        private Tween Tween { get; set; }
        private RichTextLabel CreditsContent { get; set; }
        private SceneManager _sceneManager;
        private string _creditsText;

        public override void PreInitManagers(Managers managers) => 
            _sceneManager = managers.ManagerScene;

        public override void _Ready()
        {
            _creditsText = new GodotFileManager().ReadFile("credits.txt");
            _creditsText = _creditsText.Replace("CC0", " licensed under [url=\"https://creativecommons.org/publicdomain/zero/1.0/\"]CC0 1.0 Universal (CC0 1.0)[/url]");

            CreditsContent = GetNode<RichTextLabel>(NodePathCreditsContent);
            CreditsContent.BbcodeText = _creditsText;
            Tween = GetNode<Tween>(NodePathTween);

            var duration = (CreditsContent.RectSize.y / OS.WindowSize.y) * 30f;

            Tween.InterpolateProperty(CreditsContent, "rect_position:y", OS.WindowSize.y, -CreditsContent.RectSize.y, duration);
            Tween.Start();
        }

        private async void _on_Tween_tween_completed(object obj, NodePath key) => 
            await _sceneManager.ChangeScene(GameScene.Menu);

        private void _on_Label_meta_clicked(string test)
        {
            OS.ShellOpen(test);
        }
    }
}

