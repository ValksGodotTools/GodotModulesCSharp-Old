# Formatting Guidelines

## C#
### Case Format
- Class, struct, methods and public variables are `PascalCase` format
- Private and protected variables are `_camelCase` format

### A word on static / DI
- All `static` members should go to the very top of the class
- Dependency injection is the "better" way of doing things but if it is a major annoyance, use `static` but just keep track of when the static variables are no longer being used

### Namespaces / usings
- Please make sure all classes have file scoped namespaces
- If a `using` is used across several scripts, consider making it `global` if there are no conflicts

### Readability
- Try to add comments to all the new things you add, not everyone will understand what you did!
- Favor readability over everything else, space out the code, consider using `=>`, and do not use `new()` if it makes the code look cryptic
- Use `var` wherever you can, if something looks too cryptic just add a comment

## VSCode
- Please set `Tab Size` to `4` and `End of Line Sequence` to `CRLF`

## Godot
- All Godot signal methods should go to the very bottom of the class

## GitHub
- Do not commit all your work in one commit, create commits with meaningful messages describing small things you add here and there. Or at least create a commit for each separate file.

## Code Snippets
Please make use of the following snippets

- `nodepath` -> `"[Export] protected readonly NodePath NodePath"`
- `packedscene` -> `"public readonly static PackedScene "`
