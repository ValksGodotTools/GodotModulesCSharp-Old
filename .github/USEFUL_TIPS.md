# Useful Tips

## Logging packet opcodes
If you don't see the opcodes, these lines of code might be commented out. Feel free to un-comment them. As of writing this I noticed there is a [Client] prefix for the client but no [Server] prefix for the server.

### Server packets
https://github.com/GodotModules/GodotModulesCSharp/blob/4e1f6b20c99256a5ac2164b463412f64d87f942d/Scripts/Netcode/Server/GameServer.cs#L101-L103

### Client Packets
https://github.com/GodotModules/GodotModulesCSharp/blob/4e1f6b20c99256a5ac2164b463412f64d87f942d/Scripts/Msc/GodotCommands.cs#L25-L30
