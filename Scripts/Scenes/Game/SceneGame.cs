using Godot;
using GodotModules.ModLoader;
using GodotModules.Netcode;
using GodotModules.Netcode.Client;
using System;

namespace Game
{
    public class SceneGame : Node
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

        private Dictionary<uint, OtherPlayer> Players;
        private Dictionary<uint, Vector2> NextServerPlayerPositions { get; set; }
        private Dictionary<uint, Vector2> PrevServerPlayerPositions { get; set; }

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
                NetworkManager.GameClient?.Stop();
                await NetworkManager.GameServer?.Stop();
            }
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
            foreach (var bullet in Bullets)
                bullet.Position += new Vector2(0, -1f);

            ModLoader.Call("OnGameUpdate", delta);

            PlayerTransformQueue.UpdateProgress(delta);

            if (PlayerTransformQueue.NotReady)
            {
                PrevServerPlayerPositions = NextServerPlayerPositions;
                return;
            }

            PlayerTransformQueue.Current.ForEach(pair =>
            {
                var player = Players[pair.Key];

                var prev = PlayerTransformQueue.Previous[pair.Key];
                var cur = pair.Value;

                player.Position = Utils.Lerp(prev.Position, cur.Position, PlayerTransformQueue.Progress);
                player.PlayerSprite.Rotation = Utils.LerpAngle(player.PlayerSprite.Rotation, cur.Rotation, 0.05f);
            });
        }
    }
}