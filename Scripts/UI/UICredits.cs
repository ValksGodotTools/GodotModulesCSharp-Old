using Godot;
using System;

public class UICredits : Node
{
    [Export] public readonly NodePath NodePathTween;
    [Export] public readonly NodePath NodePathCreditsContent;
    private Tween Tween { get; set; }
    private Control CreditsContent { get; set; }

    public override void _Ready()
    {
        CreditsContent = GetNode<Control>(NodePathCreditsContent);
        Tween = GetNode<Tween>(NodePathTween);

        var duration = (CreditsContent.RectSize.y / OS.WindowSize.y) * 50f;

        Tween.InterpolateProperty(CreditsContent, "rect_position:y", OS.WindowSize.y, -CreditsContent.RectSize.y, duration);
        Tween.Start();
    }

    private async void _on_Tween_tween_completed(object obj, NodePath key) => await GM.ChangeScene("Menu");
}
