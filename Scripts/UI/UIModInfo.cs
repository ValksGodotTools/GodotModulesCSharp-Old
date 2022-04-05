using Godot;
using System;

public class UIModInfo : Control
{
    [Export] public readonly NodePath NodePathLabelModName;

    public Label LabelModName { get; set; }

    public void SetModName(string text)
    {
        LabelModName = GetNode<Label>(NodePathLabelModName);
        LabelModName.Text = text;
    }
}
