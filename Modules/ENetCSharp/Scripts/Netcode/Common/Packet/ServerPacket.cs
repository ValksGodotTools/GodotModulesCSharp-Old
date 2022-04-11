using ENet;

namespace Common.Netcode
{
    public class ServerPacket : GamePacket
    {
        public Peer[] Peers { get; private set; }

        public ServerPacket(byte opcode, IWritable writable = null, params Peer[] peers)
        {
            using (var writer = new PacketWriter()) 
            {
                writer.Write(opcode);
                if (writable != null) writable.Write(writer);

                var stream = writer.GetStream();
                Data = stream.ToArray();
                Size = stream.Length;
            }

            Opcode = opcode;
            Peers = peers;
        }
    }
}