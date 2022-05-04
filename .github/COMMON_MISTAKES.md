## Thread Safety
THREAD SAFETY IS NO JOKE. Really really weird things will happen if you don't follow thread safety (as seen [here](https://github.com/valkyrienyanko/GodotModules/issues/13)) :(

Note that tasks are surrounded with try catch so you will see error in console unlike with issue above.

## Reading data from packets
I make this mistake all the time, the app will continue to run like normal except you will always yet the default value of whatever type you are reading. In this case it is a bool, it would always be false.
```cs
public bool Ready { get; set; }

public override void Write(PacketWriter writer)
{
    writer.Write(Ready);
}

public override void Read(PacketReader reader)
{
    // reader.ReadBool(); This is bad, do not do this!!!
    Ready = reader.ReadBool(); // Do this!!
}
```

Also forgetting to extend from `PacketServerPeerId` if for example telling everyone else in the server about changes to a peer.
And forgetting to write `base.Write(writer);` and `base.Read(reader);`

## Avoid static in netcode
Static properties / fields should never be used unless you want something to be persistent even if the server / client gets destroyed but this is rarely the case. 

For e.g. you could make Players static and it would retain its data even after server / client gets destroyed. Next time the server / client start up they will add a duplicate player key.

Edit: Just avoid static as much as you can. This goes for everywhere.

## Popups
Never make static references to Godot Popup Windows, changing the scene more than once will cause the code to access a old invalid reference and crash the game with `Could not access a disposed object`

## Removing and adding a child in the same frame
Adding and removing a child within the same frame almost always this leads to many errors (sometimes game crashing errors), and generally the work-a-around is to wait 1 idle_frame between removing and adding the child

## Changing the location of a script
Do not change the location of a script inside vscode, instead do it inside Godot. Changing the location of the script in Godot will update the necessary references unlike in Vscode.

## Renaming export node paths
Generally a good idea to keep the script as close as you can to the NodePaths so when renaming nodes you won't have to worry about re-assigning the nodepaths.

## Host.Create(address, byte)
Host.Create(address, byte) will always fail. Always do Host.Create(address, int) and don't even try casting from byte to int, it will fail. Client will not connect and you won't know why.
