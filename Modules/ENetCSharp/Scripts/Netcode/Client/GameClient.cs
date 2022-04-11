using ENet;

namespace Valk.Modules.Netcode.Client
{
    public class GameClient : ENetClient 
    {
        public static string Username = "Unnamed";

        public override void ProcessGodotCommands(GodotCmd cmd)
        {
            switch (cmd.Opcode)
            {
                case GodotOpcode.LoadMainMenu:
                    //UIGameMenu.ClientPressedDisconnect = false;
                    //UIMainMenu.LoadMainMenu();
                    break;
            }
        }

        public override void Connect(Event netEvent)
        {
            GDLog("Client connected to server");
        }

        public override void Timeout(Event netEvent)
        {
            GDLog("Client connection timeout");
        }

        public override void Disconnect(Event netEvent)
        {
            GDLog("Client disconnected from server");
        }
    }
}