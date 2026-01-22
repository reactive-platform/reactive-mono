# About

This is a monorepo with all reactive-related libraries and sdks.

# Compiling

To build any project from this repo you'll have to create a `Directory.Build.props.user` file 
and declare either `UnityAssembliesDir` or `BeatSaberDir` there.

> **⚠️** Beat Saber SDK cannot be built without specifying `BeatSaberDir`.

Here is a template for the `Directory.Build.props.user` file:
```xml
<Project>
    <PropertyGroup>
        <!-- You should specify just one! -->
        <BeatSaberDir>path/to/beat/saber/</BeatSaberDir>
        <UnityAssembliesDir>path/to/unity/</UnityAssembliesDir>
    </PropertyGroup>
</Project>
```

After that you should be able to run any of the workflows from the `targets/` directory. 
Projects can be built manually as well, but using pre-defined targets simplifies the process
by producing ready-to-ship artifacts packed the proper way.