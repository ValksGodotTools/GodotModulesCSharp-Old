# Formatting Guidelines

## Generic

- Favor readability over everything else, space out the code, consider using `=>`, and do not use `new()` if it makes the code look cryptic
- Dependency injection is the "better" way of doing things but if it is a major annoyance, use `static` but just keep track of when the static variables are no longer being used
- Please set `Tab Size` to `4` and `End of Line Sequence` to `CRLF`
- Please make sure all classes are wrapped with a namespace
- If a `using` is used across several scripts, consider making it `global` if there are no conflicts
- Class and method and public variables are `PascalCase` format
- Use `var` wherever you can, if something looks too cryptic just add a comment
- Private and protected variables are `_camelCase` format *(this is a habbit I've gotten into, not everyone may like doing it this way, please talk to me if you want to do it another way)*

## Code Snippets

Please make use of the following snippets

- `nodepath` -> `"[Export] protected readonly NodePath NodePath"`
- `packedscene` -> `"public readonly static PackedScene "`
