using ENet;

namespace Valk.Modules.Netcode.Client
{
    public class GameClient : ENetClient 
    {
        public static string Username = "Unnamed";

        protected override void ProcessGodotCommands(GodotCmd cmd)
        {
            switch (cmd.Opcode)
            {
                case GodotOpcode.LoadMainMenu:
                    //UIGameMenu.ClientPressedDisconnect = false;
                    //UIMainMenu.LoadMainMenu();
                    break;
            }
        }

        protected override void Connect(Event netEvent)
        {
            GDLog("Client connected to server");
        }

        protected override void Timeout(Event netEvent)
        {
            GDLog("Client connection timeout");
        }

        protected override void Disconnect(Event netEvent)
        {
            GDLog("Client disconnected from server");
        }
    }
}