using Godot;
using GodotModules.Netcode.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GodotModules
{
    public class GameManager : Node
    {
        public static string GameName = "Godot Modules";
        public static OptionsData Options { get; set; }
        public static SceneTree GameTree { get; set; }

        private static ConcurrentQueue<GodotCmd> GodotCmds { get; set; }

        public override void _Ready()
        {
            GameTree = GetTree();
            GodotCmds = new();
        }

        public override async void _Process(float delta)
        {
            while (GodotCmds.TryDequeue(out GodotCmd cmd))
            {
                switch (cmd.Opcode)
                {
                    case GodotOpcode.ENetPacket:
                        var packetHandleData = (PacketHandleData)cmd.Data;
                        var packetReader = packetHandleData.Reader;
                        var client = packetHandleData.Client;
                        var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                        //Utils.Log($"[Client]: Received {opcode}");

                        if (!ENetClient.HandlePacket.ContainsKey(opcode))
                        {
                            Logger.LogWarning($"[Client]: Received malformed opcode: {opcode} (Ignoring)");
                            break;
                        }

                        var handlePacket = ENetClient.HandlePacket[opcode];
                        try
                        {
                            handlePacket.Read(packetReader);
                        }
                        catch (System.IO.EndOfStreamException ex)
                        {
                            Logger.LogWarning($"[Client]: Received malformed opcode: {opcode} {ex.Message} (Ignoring)");
                            break;
                        }
                        await handlePacket.Handle(client);

                        packetReader.Dispose();
                        break;

                    case GodotOpcode.LogMessage:
                        var message = (GodotMessage)cmd.Data;
                        var text = message.Text;
                        var color = message.Color;

                        Console.ForegroundColor = color;
                        GD.Print(text);

                        if (!string.IsNullOrWhiteSpace(message.Path))
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            GD.Print($"   at ({message.Path})");
                        }

                        Console.ResetColor();

                        UIDebugger.AddMessage(text);
                        break;

                    case GodotOpcode.LogError:
                        var error = (GodotError)cmd.Data;
                        var exception = error.Exception;
                        var hint = error.Hint;

                        var errorText = $"[Error]: {hint}{exception.Message}\n{exception.StackTrace}";

                        Console.ForegroundColor = ConsoleColor.Red;
                        GD.PrintErr(exception);
                        Console.ResetColor();

                        ErrorNotifier.IncrementErrorCount();
                        UIDebugger.AddMessage(errorText);
                        break;

                    case GodotOpcode.PopupMessage:
                        GameManager.SpawnPopupMessage((string)cmd.Data);
                        break;

                    case GodotOpcode.ChangeScene:
                        await SceneManager.ChangeScene($"{cmd.Data}");
                        break;

                    case GodotOpcode.Disconnect:
                        NetworkManager.DisconnectOpcode = (DisconnectOpcode)cmd.Data;
                        await SceneManager.ChangeScene("GameServers");
                        break;
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_debug"))
            {
                UIDebugger.ToggleVisibility();
            }

            if (Input.IsActionJustPressed("ui_fullscreen"))
                UtilOptions.ToggleFullscreen();
        }

        public override async void _Notification(int what)
        {
            if (what == MainLoop.NotificationWmQuitRequest)
            {
                GameTree.SetAutoAcceptQuit(false);

                await ExitCleanup();
            }
        }

        public static void GodotCmd(GodotOpcode opcode, object data = null) => GodotCmds.Enqueue(new GodotCmd(opcode, data));

        /// <summary>
        /// All cleanup should be done in here
        /// </summary>
        private static async Task ExitCleanup()
        {
            try
            {
                if (NetworkManager.GameServer != null)
                    if (NetworkManager.GameServer.IsRunning)
                    {
                        NetworkManager.GameServer.ENetCmds.Enqueue(new ENetCmd(ENetOpcode.ClientWantsToExitApp));
                        NetworkManager.GameServer.Stop();

                        while (NetworkManager.GameServer.IsRunning) 
                        {
                            Logger.LogDebug("Game server still running");
                            await Task.Delay(100);
                        }
                    }

                if (NetworkManager.GameClient != null)
                    if (NetworkManager.GameClient.IsRunning)
                    {
                        NetworkManager.GameClient.Stop();

                        while (NetworkManager.GameClient.IsRunning) 
                        {
                            Logger.LogDebug("Game client still running");
                            await Task.Delay(100);
                        }
                    }

                UtilOptions.SaveOptions();
                NetworkManager.WebClient.Dispose();
            }
            catch (Exception e)
            {
                Logger.LogErr(e, "Game exit cleanup");
                await Task.Delay(3000);
            }

            GameTree.Quit();
        }

        public static void SpawnPopupMessage(string message)
        {
            var popupMessage = Prefabs.PopupMessage.Instance<UIPopupMessage>();
            GameTree.CurrentScene.AddChild(popupMessage);
            popupMessage.Init(message);
            popupMessage.PopupCentered();
        }

        public static void SpawnPopupError(Exception e)
        {
            ErrorNotifier.IncrementErrorCount();
            var popupError = Prefabs.PopupError.Instance<UIPopupError>();
            GameTree.CurrentScene.AddChild(popupError);
            popupError.Init(e.Message, e.StackTrace);
            popupError.PopupCentered();
        }

        /// <summary>
        /// This should be used instead of GetTree().Quit() has it will handle cleanup and saving options
        /// Note that if the console is closed directly then the cleanup will never happen, this should be avoided.
        /// </summary>
        public static void Exit() => GameTree.Notification(MainLoop.NotificationWmQuitRequest);
    }

    public struct GodotMessage
    {
        public string Text { get; set; }
        public string Path { get; set; }
        public ConsoleColor Color { get; set; }
    }

    public struct GodotError
    {
        public Exception Exception { get; set; }
        public string Hint { get; set; }
    }
}