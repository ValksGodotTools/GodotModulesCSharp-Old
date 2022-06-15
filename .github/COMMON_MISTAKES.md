## Thread Safety
The netcode uses a while loop that runs constantly and thus needs to be on a separate thread from the Godot thread. Two methods are used to safely send data between threads, ConcurrentQueue<T> and Interlocked.Read(). ConcurrentQueue<T> allows any kind of data to be enqueued on one thread and dequeued on another. Interlocked.Read() is a nice and easy way to read a bool from any thread. If data is not accessed through one of these methods, then the rules of thread safety are being violated and will vastly increase the chances of the program crashing with no errors. Trying to debug errors that do not appear in the console is a nightmare, please be mindful when sending data between threads.

## Reading data from packets
You may forget to assign the value that `reader.readXXXX();` returns. If this happens then in this case the `Ready` bool will always have the default value `false`.
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
Adding and removing a child within the same frame almost always this leads to errors (sometimes game crashing errors). Generally, the workaround is to wait 1 idle_frame between removing and adding the child. Perhaps there is a better solution to what you are trying to do like re-assigning the parent of the child instead.


## Changing the location of a script
If a script in Godot is attached to a node, do not change the location of the script inside VSCode, instead do it inside Godot. Changing the location of the script in Godot will update the necessary references whereas doing so in VSCode will not.


## Renaming export node paths
Generally, it's a good idea to keep the script as close as you can to the NodePaths so when renaming nodes you won't have to worry about re-assigning the nodepaths.


## `Host.Create(address, byte)`
Host.Create(address, byte) will always fail. Always do Host.Create(address, int) and don't even try casting from byte to int, it will fail. Client will not connect and you won't know why.
