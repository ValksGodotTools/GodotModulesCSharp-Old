using ENet;

namespace GodotModules.Netcode
{
    public class ClientPacket : GamePacket
    {
        public ClientPacket(byte opcode, PacketFlags flags, APacket writable = null)
        {
            using (var writer = new PacketWriter())
            {
                writer.Write(opcode);
                if (writable != null) writable.Write(writer);

                var stream = writer.GetStream();
                Data = stream.ToArray();
                Size = stream.Length;
            }

            PacketFlags = flags;
            Opcode = opcode;
        }
    }
}