## Thread Safety
**THREAD SAFETY IS NO JOKE**. Really weird things will happen if you don't follow thread safety (as seen [here](https://github.com/valkyrienyanko/GodotModules/issues/13)) :(

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

Also, forgetting to extend from `PacketServerPeerId` (if, for example, telling everyone else in the server about changes to a peer) is a common misake. Forgetting to write `base.Write(writer)` and `base.Read(reader)` in this case can also lead to problems.


## Removing and adding a child in the same frame
Adding and removing a child within the same frame almost always this leads to errors (sometimes game crashing errors). Generally, the workaround is to wait 1 idle_frame between removing and adding the child.


## Changing the location of a script
Do not change the location of a script inside VSCode, instead do it inside Godot. Changing the location of the script in Godot will update the necessary references whereas doing so in VSCode will not.


## Renaming export node paths
Generally, it's a good idea to keep the script as close as you can to the NodePaths so when renaming nodes you won't have to worry about re-assigning the nodepaths.


## `Host.Create(address, byte)`
Host.Create(address, byte) will always fail. Always do Host.Create(address, int) and don't even try casting from byte to int, it will fail. Client will not connect and you won't know why.
