using Godot;
using GodotModules.ModLoader;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System;

namespace Game
{
    public class SceneGame : AScene
    {
        public static SceneGame Instance { get; set; } // TODO: Move stat ic somewhere else
        private static PrevCurQueue<Dictionary<byte, DataEntityTransform>> PlayerTransformQueue = new PrevCurQueue<Dictionary<byte, DataEntityTransform>>(ServerIntervals.PlayerTransforms);
        public static void UpdatePlayerPositions(Dictionary<byte, DataEntityTransform> playerTransforms) // TODO: Move stat ic somewhere else
        {
            PlayerTransformQueue.Add(playerTransforms);
        }

        public ClientPlayer Player { get; set; }

        [Export] public readonly NodePath NodePathLabelPlayerHealth;
        public Label LabelPlayerHealth;

        private static Dictionary<uint, OtherPlayer> Players;
        private static Dictionary<uint, Vector2> NextServerPlayerPositions { get; set; }
        private static Dictionary<uint, Vector2> PrevServerPlayerPositions { get; set; }

        private List<Sprite> Bullets = new List<Sprite>();

        public override void _Ready()
        {
            var bullet = Prefabs.Bullet.Instance<Sprite>();
            Bullets.Add(bullet);
            AddChild(bullet);

            Players = new();
            NextServerPlayerPositions = new();
            PrevServerPlayerPositions = new();
            Instance = this;
            LabelPlayerHealth = GetNode<Label>(NodePathLabelPlayerHealth);
            Player = Prefabs.ClientPlayer.Instance<ClientPlayer>();
            Player.Position = Vector2.Zero;

            AddChild(Player);

            // set game definitions
            ModLoader.Script.Globals["Player", "setHealth"] = (Action<int>)Player.SetHealth;

            ModLoader.Call("OnGameInit");

            if (NetworkManager.GameClient != null)
                if (NetworkManager.GameClient.Running)
                    InitMultiplayerStuff();
        }

        public override async void _Input(InputEvent @event)
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                if (SceneManager.PrevSceneName == "Menu")
                {
                    // Singleplayer
                    await SceneManager.ChangeScene("Menu", false);
                }
                else
                {
                    // Multiplayer
                    if (NetworkManager.GameClient != null)
                        NetworkManager.GameClient.Stop();
                    if (NetworkManager.GameServer != null)
                        await NetworkManager.GameServer.Stop();
                }
            }
        }

        public static void RemovePlayer(byte id)
        {
            var player = Players[id];
            player.QueueFree();
            Players.Remove(id);
        }

        private void InitMultiplayerStuff()
        {
            Players.Add(NetworkManager.GameClient.PeerId, Player);
            Player.SetUsername(GameManager.Options.OnlineUsername);

            bool IsNotClient(uint id) => id != NetworkManager.GameClient.PeerId;

            NetworkManager.GameClient.Players
                .Where(x => IsNotClient(x.Key))
                .ForEach(pair =>
                {
                    var otherPlayer = Prefabs.OtherPlayer.Instance<OtherPlayer>();
                    otherPlayer.Position = Vector2.Zero;
                    Players.Add(pair.Key, otherPlayer);
                    AddChild(otherPlayer);
                    otherPlayer.SetUsername(pair.Value);
                });
        }

        public override void _PhysicsProcess(float delta)
        {
            ModLoader.Call("OnGameUpdate", delta);

            if (SceneManager.PrevSceneName == "Menu") // singleplayer
                return;
            
            foreach (var bullet in Bullets)
                bullet.Position += new Vector2(0, -1f);

            PlayerTransformQueue.UpdateProgress(delta);

            if (PlayerTransformQueue.NotReady)
            {
                PrevServerPlayerPositions = NextServerPlayerPositions;
                return;
            }

            foreach (var pair in PlayerTransformQueue.Current)
            {
                if (!Players.ContainsKey(pair.Key))
                    continue;

                var player = Players[pair.Key];

                var prev = PlayerTransformQueue.Previous[pair.Key];
                var cur = pair.Value;

                player.Position = Utils.Lerp(prev.Position, cur.Position, PlayerTransformQueue.Progress);
                player.PlayerSprite.Rotation = Utils.LerpAngle(player.PlayerSprite.Rotation, cur.Rotation, 0.05f);
            }
        }

        public override void Cleanup()
        {

        }
    }
}