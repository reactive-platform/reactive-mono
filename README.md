# About

This is a monorepo with all reactive-related libraries and sdks.

# Compiling

To build a project from this repo you'll need to create a `Directory.Build.props.user` file 
and declare either `UnityAssembliesDir` or `BeatSaberDir` there.

> **⚠️** Beat Saber SDK cannot be built without specifying `BeatSaberDir`.

Here is a template for the `Directory.Build.props.user` file:
```xml
<Project>
    <PropertyGroup>
        <!-- You can leave just BeatSaberDir, UnityAssembliesDir will be derived from it in this case -->
        <BeatSaberDir>path/to/beat/saber/</BeatSaberDir>
        <UnityAssembliesDir>path/to/unity/</UnityAssembliesDir>
    </PropertyGroup>
</Project>
```

After that you'll be able to run any of configurations defined in the solution. 
Each configuration compiles a predefined set of libraries (e.g. Core compiles just Reactive and Reactive.Components).