namespace GodotModules
{
    public class UIErrorNotifier : Control
    {
        [Export] protected readonly NodePath NodePathLabel;
        [Export] protected readonly NodePath NodePathTween;

        public int Count { get; set; }

        public override async void _Ready()
        {
            GetNode<Label>(NodePathLabel).Text = "" + Count;

            RectPosition = OS.WindowSize + new Vector2(-RectSize.x, RectSize.y);

            var tween = GetNode<Tween>(NodePathTween);
            tween.InterpolateProperty(this, "rect_position:y", OS.WindowSize.y + RectSize.y, OS.WindowSize.y - RectSize.y, 1.5f, Tween.TransitionType.Circ);
            tween.Start();

            await Task.Delay(5000);
            tween.InterpolateProperty(this, "rect_position:y", OS.WindowSize.y - RectSize.y, OS.WindowSize.y + RectSize.y, 1.5f, Tween.TransitionType.Linear);
            tween.Start();
        }
    }
}
