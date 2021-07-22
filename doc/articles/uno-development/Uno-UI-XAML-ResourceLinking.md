# XAML Resource and Bindings Linking

## Using XAML Resources abd Bindings generation trimming

In order for an application to enable resources trimming, the following needs to be added in the project file:

```xml
<PropertyGroup>
    <UnoXamlResourcesTrimming>true</UnoXamlResourcesTrimming>
</PropertyGroup>
```

## Technical Details

In Uno, XAML is generating C# code in order to speed up the creation of the UI. This allows for compile-time optimizations, such as type conversions or `x:Bind` integration. 

The drawback of this approach is that code may be bundled with an app even if it's not used. In common use cases, the IL Linker is able to remove code not referenced, but in the case of XAML resources, this code is conditionally referenced through string cases only. This makes it impossible for the linker to remove that code through out-of-the-box means.

In order to dermine what is needed by the needed, the Uno Platform tooling runs a sequence of IL Linker passes and substitutions.

In order to prepare the linking pass:
- During the source generation, the tooling generates a `LinkerHints` class, which contains a set of all properties for `DependencyObject` inheriting classes.
- The source generation creates XAML Resources and Bindable Metadata code that conditionally uses those classes behind `LinkerHints` properties. 
- The tooling also embeds an ILLinker substitution file allowing the linker to unconditionally remove the code that conditionally references those properties. For instance, for `LinkerHints.Is_Windows_UI_Xaml_Controls_Border_Available`, any block of `if (LinkerHints.Is_Windows_UI_Xaml_Controls_Border_Available)` will be removed when the `--feature Is_Windows_UI_Xaml_Controls_Border_Available false` parameter is provided to the linker. 

Then the multiple passes of IL Linker are done:
- The first pass runs the IL Linker with all XAML resources and Binding Metadata disabled by setting all `LinkerHints` properties to false. This removes all code directly associated to it. This has the effect of only keeping framework code which is directly referenced from user code.
- The tooling then reads the result of the linker to determine which types in the `LinkerHints` are still available in the assemblies.
- The subsequent passes run the IL Linker with `LinkerHints` enabled only for types detected to be used during the first pass. This will enable types indirectly referenced by XAML Resources (e.g. a ScrollBar inside a ScrollViewer style) to be kept by the linker.
- The tooling then reads again the result of the linker to determine which types in the `LinkerHints` are still available in the assemblies.
- The tooling re-runs this last pass until the available types list stops changing.

The resulting `LinkerHints` types are now passed as features to the final linker pass (the one from the .NET Framework), generating the proper binary, containing only the used types.

As of Uno 3.9, the Uno.UI WebAssembly assembly is 7.5MB, trimmed down to 3.1MB for a `dotnet new unoapp` template.
