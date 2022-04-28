using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UINotifyError : Control
{
    [Export] public readonly NodePath NodePathErrorCount;
    [Export] public readonly NodePath NodePathAnimationPlayer;

    private Label ErrorCount { get; set; }
    private AnimationPlayer AnimationPlayer { get; set; }

    public int Count { get; set; }

    public override async void _Ready()
    {
        ErrorCount = GetNode<Label>(NodePathErrorCount);
        ErrorCount.Text = $"{Count}";
        AnimationPlayer = GetNode<AnimationPlayer>(NodePathAnimationPlayer);
        AnimationPlayer.Play("Appear");
        await Task.Delay(2000);
        AnimationPlayer.Play("Disappear");
        await Task.Delay(2000);
        QueueFree();
    }
}
