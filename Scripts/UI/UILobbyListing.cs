namespace GodotModules;

public class UILobbyListing : Control
{
    [Export] protected readonly NodePath NodePathName;
    [Export] protected readonly NodePath NodePathPlayers;
    [Export] protected readonly NodePath NodePathPing;
    [Export] protected readonly NodePath NodePathDescription;

    private Label _labelName;
    private Label _labelPlayers;
    private Label _labelPing;
    private Label _labelDescription;

    public string LobbyName { get; private set; }
    public int CurPlayers { get; private set; }
    public int MaxPlayers { get; private set; }
    public int Ping { get; private set; }
    public string Description { get; private set; }

    public override void _Ready()
    {
        _labelName = GetNode<Label>(NodePathName);
        _labelPlayers = GetNode<Label>(NodePathPlayers);
        _labelPing = GetNode<Label>(NodePathPing);
        _labelDescription = GetNode<Label>(NodePathDescription);
    }

    public void SetLobbyName(string v)
    {
        LobbyName = v;
        _labelName.Text = v;
    }

    public void SetPlayers(int cur, int max)
    {
        CurPlayers = cur;
        MaxPlayers = max;
        _labelPlayers.Text = $"{cur} / {max}";
    }

    public void SetPing(int v)
    {
        Ping = v;
        _labelPing.Text = $"{v}";
    }

    public void SetDescription(string v)
    {
        Description = v;
        _labelDescription.Text = v;
    }

    public void SetDark(bool v) => SelfModulate = v ? new Color("8f8f8f") : new Color("ffffff");

    private void _on_PanelContainer_gui_input(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn)
            if (btn.Doubleclick)
                Logger.Log(@event.AsText());
    }
}
