namespace GodotModules;

public class PopupCreateLobby : WindowDialog
{
    [Export] protected readonly NodePath NodePathName;
    [Export] protected readonly NodePath NodePathDescription;
    [Export] protected readonly NodePath NodePathPort;
    [Export] protected readonly NodePath NodePathMaxPlayers;
    [Export] protected readonly NodePath NodePathPassword;

    private LineEdit _inputName;
    private TextEdit _inputDescription;
    private LineEdit _inputPort;
    private LineEdit _inputMaxPlayers;
    private LineEdit _inputPassword;

    private string _name;
    private string _description;
    private ushort _port;
    private byte _maxPlayers;
    private string _password;

    private Popups _popupManager;
    private Managers _managers;

    public void PreInit(Popups popupManager, Managers managers)
    {
        _popupManager = popupManager;
        _managers = managers;
    }

    public override void _Ready()
    {
        _inputName = GetNode<LineEdit>(NodePathName);
        _inputDescription = GetNode<TextEdit>(NodePathDescription);
        _inputPort = GetNode<LineEdit>(NodePathPort);
        _inputMaxPlayers = GetNode<LineEdit>(NodePathMaxPlayers);
        _inputPassword = GetNode<LineEdit>(NodePathPassword);

        _name = "Another lobby";
        _description = "Some description";
        _port = 25565;
        _maxPlayers = 100;
        _password = "";

        _inputName.Text = _name;
        _inputDescription.Text = _description;
        _inputPort.Text = $"{_port}";
        _inputMaxPlayers.Text = $"{_maxPlayers}";
        _inputPassword.Text = _password;
    }

    private void _on_Name_text_changed(string v) =>
        _name = _inputName.Filter((text) => text.IsMatch("^[A-Za-z ]+$"));

    private void _on_Description_text_changed() =>
        _description = _inputDescription.Filter((text) => text.Length < 250);

    private void _on_Password_text_changed(string v) =>
        _password = _inputPassword.Filter((text) => text.Length < 100);

    private void _on_Port_text_changed(string v) =>
        _port = (ushort)_inputPort.FilterRange(ushort.MaxValue);

    private void _on_Max_Players_text_changed(string v) =>
        _maxPlayers = (byte)_inputMaxPlayers.FilterRange(byte.MaxValue);

    private void _on_PopupCreateLobby_popup_hide()
    {
        _popupManager.Next();
        QueueFree();
    }

    private async void _on_Create_pressed()
    {
        var ctsServer = _managers.Tokens.Create("server_running");
        var ctsClient = _managers.Tokens.Create("client_running");
        _managers.Net.StartServer(_port, _maxPlayers, ctsServer);
        _managers.Net.StartClient("127.0.0.1", _port, ctsClient);
        Hide();

        try
        {
            while (!_managers.Net.Server.HasSomeoneConnected)
            {
                await Task.Delay(1, ctsServer.Token);
            }
        }
        catch (TaskCanceledException)
        {
            return;
        }

        _managers.Net.Client.Send(ClientPacketOpcode.Lobby, new CPacketLobby
        {
            Username = _managers.Options.Data.OnlineUsername,
            LobbyName = _name,
            LobbyDescription = _description
        });
    }
}
