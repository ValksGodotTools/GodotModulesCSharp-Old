using System;

namespace GodotModules.Netcode
{
    // Intervals
    public static class ClientIntervals 
    {
        public static int PlayerDirection = 20;
        public static int PlayerPosition = 150;
        public static int PlayerRotation = 150;
    }

    public static class ServerIntervals 
    {
        public static int PlayerTransforms = 150;
    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public class GodotCmd
    {
        public GodotCmd(GodotOpcode opcode)
        {
            Opcode = opcode;
        }

        public GodotCmd(GodotOpcode opcode, object data)
        {
            Opcode = opcode;
            Data = data;
        }

        public GodotOpcode Opcode { get; set; }
        public object Data { get; set; }
    }

    public enum GodotOpcode
    {
        ENetPacket,
        LogMessage,
        LogError,
        ChangeScene,
        PopupMessage,
        Disconnect
    }

    public class ENetCmd
    {
        public ENetCmd(ENetOpcode opcode)
        {
            Opcode = opcode;
        }

        public ENetCmd(ENetOpcode opcode, object data)
        {
            Opcode = opcode;
            Data = data;
        }

        public ENetOpcode Opcode { get; set; }
        public object Data { get; set; }
    }

    public enum ENetOpcode
    {
        StartGame,
        StopServer,
        RestartServer,
        ClientWantsToExitApp,
        ClientWantsToDisconnect
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

    public struct LobbyListing
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Ip { get; set; }
        public ushort Port { get; set; }
        public int Ping { get; set; }
        public string LobbyHost { get; set; }
        public byte LobbyHostId { get; set; }
        public int MaxPlayerCount { get; set; }
        public int Players { get; set; }
        public bool Public { get; set; }
    }
}