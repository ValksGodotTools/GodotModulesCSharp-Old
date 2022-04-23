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

    public async void Init(int count)
    {
        ErrorCount = GetNode<Label>(NodePathErrorCount);
        ErrorCount.Text = $"{count}";
        AnimationPlayer = GetNode<AnimationPlayer>(NodePathAnimationPlayer);
        AnimationPlayer.Play("Appear");
        await Task.Delay(2000);
        AnimationPlayer.Play("Disappear");
        await Task.Delay(2000);
        QueueFree();
    }
}
