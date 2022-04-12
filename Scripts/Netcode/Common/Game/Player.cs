using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Game
{
    public class Player
    {
        public string Username { get; set; }
        public string Ip { get; set; }

        public override string ToString() => Username;
    }

    public enum Status
    {
        Online,
        Away,
        Offline
    }
}
