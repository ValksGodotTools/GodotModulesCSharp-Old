using Godot;

namespace GodotModules;

public class PopupCreateLobby : WindowDialog
{
    [Export] protected readonly NodePath NodePathName;
    [Export] protected readonly NodePath NodePathDescription;
    [Export] protected readonly NodePath NodePathPort;
    [Export] protected readonly NodePath NodePathMaxPlayers;
    [Export] protected readonly NodePath NodePathPassword;

    private LineEdit _name;
    private TextEdit _description;
    private LineEdit _port;
    private LineEdit _maxPlayers;
    private LineEdit _password;
        
    private PopupManager _popupManager;

    public void PreInit(PopupManager popupManager)
    {
        _popupManager = popupManager;
    }

    public override void _Ready()
    {
        _name = GetNode<LineEdit>(NodePathName);
        _description = GetNode<TextEdit>(NodePathDescription);
        _port = GetNode<LineEdit>(NodePathPort);
        _maxPlayers = GetNode<LineEdit>(NodePathMaxPlayers);
        _password = GetNode<LineEdit>(NodePathPassword);
    }

    private void _on_Name_text_changed(string v)
    {

    }

    private void _on_Description_text_changed()
    {

    }

    private void _on_Password_text_changed(string v)
    {

    }

    private void _on_Port_text_changed(string v)
    {

    }

    private void _on_Max_Players_text_changed(string v)
    {

    }

    private void _on_PopupCreateLobby_popup_hide()
    {
        _popupManager.Next();
        QueueFree();
    }

    private void _on_Create_pressed()
    {
        Hide();
    }
}