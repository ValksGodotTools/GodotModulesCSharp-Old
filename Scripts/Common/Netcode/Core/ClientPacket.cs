using ENet;

namespace GodotModules.Netcode
{
    public class ClientPacket : GamePacket
    {
        public ClientPacket(byte opcode, PacketFlags flags, Packet writable = null)
        {
            using (var writer = new PacketWriter())
            {
                writer.Write(opcode);
                if (writable != null)
                    writable.Write(writer);

                Data = writer.Stream.ToArray();
                Size = writer.Stream.Length;
            }

            PacketFlags = flags;
            Opcode = opcode;
        }
    }
}