using System.Collections.Generic;

namespace Valk.Modules.Netcode.Server
{
    public abstract class ENetCmd
    {
        public abstract ENetOpcode Opcode { get; set; }

        public abstract void Handle(List<object> value);
    }
}
