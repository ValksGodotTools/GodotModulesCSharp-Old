## Setup
1. Add the following to `.csproj` (if there is no `.csproj` generate one `Godot > Project > Tools > C# > Generate Solution`)
<ItemGroup>
  <PackageReference Include="Newtonsoft.Json" Version="13.0.1" /> <!--This is used because net472 does not have System.Text.Json-->
</ItemGroup>
2. Add `Global/Scenes/Global.tscn` to `Project > Project Settings > AutoLoad`
3. Add `Modules/Options/Scenes/Prefabs/Options.tscn` where ever you want

Note that `GameManager.Exit()` should be used instead of `GetTree().Quit()` to save the options properly on exit.
