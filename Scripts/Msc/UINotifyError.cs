using Godot;
using System;
using System.Collections.Generic;

public class UINotifyError : Control
{
    [Export] public readonly NodePath NodePathErrorCount;
    [Export] public readonly NodePath NodePathAnimationPlayer;

    private Label ErrorCount { get; set; }
    private AnimationPlayer AnimationPlayer { get; set; }

    public override void _Ready()
    {
        ErrorCount = GetNode<Label>(NodePathErrorCount);
        AnimationPlayer = GetNode<AnimationPlayer>(NodePathAnimationPlayer);
        AnimationPlayer.Play("Move");
    }
}
