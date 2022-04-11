using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Game
{
    public class Channel
    {
        public string Name { get; set; }
        public List<uint> Users { get; set; }
    }

    public enum SpecialChannel
    {
        Global,
        Game
    }
}
