using Godot;
using System;

public class UIHotkey : Node
{
    [Export] public readonly NodePath NodePathLabel;
    [Export] public readonly NodePath NodePathBtnHotkey;

    private Label _label;
    private UIBtnHotkey _btnHotkey;

    private string _action;

    public override void _Ready()
    {
        _label = GetNode<Label>(NodePathLabel);
        _btnHotkey = GetNode<UIBtnHotkey>(NodePathBtnHotkey);
        _label.Text = _action.Replace("_", " ").ToTitleCase();
        _btnHotkey.Init(_action);
    }

    public void Init(string action)
    {
        _action = action;
    }
}
