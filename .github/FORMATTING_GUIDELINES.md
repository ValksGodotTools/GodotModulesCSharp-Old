### Case Format
- Class, struct, methods and public variables are `PascalCase` format
- Private and protected variables are `_camelCase` format
- Variables that do not change are `UPPER_CASE` format

### Namespaces / usings
- Please make sure all classes have file scoped namespaces
- If a `using` is used across several scripts, consider making it `global` if there are no conflicts

### Readability
- Try to add comments to all the new things you add, not everyone will understand what you did!
- Favor readability over everything else, space out the code, consider using `=>`, and do not use `new()` if it makes the code look cryptic
- Use `var` wherever you can, if something looks too cryptic just add a comment
- Try not to squash every piece of logic into one class, if code is related to a cat put it in a cat class, don't just stuff it all in animals class

### VSCode
- Please set `Tab Size` to `4` and `End of Line Sequence` to `CRLF`

### The Order of Things
- All `static` members should go to the very top of the class  
- `void Preinit(GameManager gameManager)` should go just above `override void _Ready()`
- `_Ready()` `_Process()` functions should be at the top, all user-defined functions go below
- All private methods should go below public methods
- All Godot signal methods should go to the very bottom of the class

### GitHub
- Try not to commit all your work in one commit, create commits with meaningful messages describing small things you add here and there. Or at least create a commit for each separate file.

### Code Snippets
Please make use of the following snippets

- `nodepath` -> `"[Export] protected readonly NodePath NodePath"`
- `packedscene` -> `"public readonly static PackedScene "`
