# roslyn_codegen

Bunch of shit code to learn source generation, I wouldn't recommend any of it for production

## Very simplified theory:
1. Generator class has to be marked with `Generator` attribute and it has to implement either `ISourceGenerator` or `IIncrementalGenerator`.
2. Get source of truth with `context.SyntaxProvider.CreateSyntaxProvider()`. The `predicate` filters valid syntaxes and `transform` transforms the valid syntax into metadata used for source generation. The `transform` can return anything.
3. Register providers for source generation

```
context.RegisterSourceOutput(provider, static (SourceProductionContext ctx, TransformReturnType t)) => {
    /*
    **  Do some source generation here
    */
    ctx.AddSource(generatedFileName, generatedCode);
};
```

## Setup for Unity:
1. Create new solution from Roslyn > Source Generators template in Rider 
2. The "SourceGenerators~" folder has to have tilde at the end, to hide the folder from Unity
3. Add to "SourceGenerators.csproj" copy step, in order to generate .pdb and .dll outside the Unity hidden folder

```
<Target Name="CustomAfterBuild" AfterTargets="Build">
    <ItemGroup>
        <_FilesToCopy Include="$(OutputPath)**\$(AssemblyName).dll"/>
        <_FilesToCopy Include="$(OutputPath)**\$(AssemblyName).pdb"/>
    </ItemGroup>
    <Copy SourceFiles="@(_FilesToCopy)" DestinationFolder="$(OutputPath)..\..\..\..\..\.."/>
</Target>
```

4. Change the target version of "Microsoft.CodeAnalysis.CSharp" to "4.0.1" that is supported by Unity


## Better resources:
- https://github.com/amis92/csharp-source-generators (c# source generators)
- https://www.youtube.com/watch?v=UiYQR8eQfgU&t=4835s (some of it is useful, setup for Unity at the end 1:42:40)
- https://docs.unity3d.com/2022.1/Documentation/Manual/roslyn-analyzers.html (Unity manual for source generator)
- https://sharplab.io/ (syntax tree visualizer)

## TODO:
- [x] Fast enums
- [x] Simple readable serialize (and slow)
    - [ ] Add async version
- [x] Get and Set attributes
- [ ] Clonable attribute
- [ ] Steal zig comptime 
- [ ] Better serialize
- [ ] Validators
- [ ] Logging attribute
- [ ] Commandline
    - [ ] Generate commands from methods with Command attribute
    - [ ] Commands initialized without reflection
    - [ ] Autocompletion precompiled
- [ ] Dependency injection
