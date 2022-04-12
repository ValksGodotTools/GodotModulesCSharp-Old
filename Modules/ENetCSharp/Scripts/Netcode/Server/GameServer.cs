using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Netcode;
using ENet;
using Godot;
using Common.Game;

namespace Valk.Modules.Netcode.Server 
{
    // all game specific logic will be put in here
    public class GameServer : ENetServer
    {
        protected override void Connect(Event netEvent)
        {
            
        }

        protected override void Disconnect(Event netEvent)
        {
            
        }

        protected override void Timeout(Event netEvent)
        {
            
        }
    }
}