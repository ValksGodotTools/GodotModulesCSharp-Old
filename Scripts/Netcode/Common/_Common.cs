using System;

namespace GodotModules.Netcode
{
    // Intervals
    public struct ClientIntervals
    {
        public static int PlayerDirection = 20;
        public static int PlayerPosition = 150;
        public static int PlayerRotation = 150;
    }

    public struct ServerIntervals
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

    public struct ThreadCmd<TOpcode>
    {
        public TOpcode Opcode { get; set; }
        public object Data { get; set; }

        public ThreadCmd(TOpcode opcode, object data = null)
        {
            Opcode = opcode;
            Data = data;
        }
    }

    public enum SimulationOpcode 
    {

    }

    public enum LoggerOpcode
    {
        LogMessage,
        LogError
    }

    public enum GodotOpcode
    {
        ENetPacket,
        ChangeScene,
        PopupMessage,
        Disconnect
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

    public class LobbyListing
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