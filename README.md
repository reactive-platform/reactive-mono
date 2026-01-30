# About

This is a monorepo with all reactive-related libraries and sdks.

## Reactive
Reactive UI is unity library that is made specifically for modding, with performance, convenience and simplicity in mind. It's based on the meta's Yoga layout engine that allows building powerful layouts similar to those you can achieve when using css.
Each `ReactiveComponent` returns a `GameObject` instance, so you can easily wrap existing components and flawlessly integrate with another UI libraries. Read more on: https://reactiveui.beatleader.com/.

## Reactive.Components
A set of platform-agnostic components for Reactive UI library.

## Reactive.BeatSaber
A set of BeatSaber-specific components for Reactive UI library. 
Note that even though some components depend on BeatSaber code, most of them are remade from the scratch.

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

# Roadmap
This roadmap covers not just this repo, but rather the whole reactive sdk set. Here it is:
- [x] Release the first version;
- [X] Refactor names for simplicity, so `ObservableValue<T>` becomes `State<T>`;
- [ ] Rework modal system (Already WIP);
- [ ] Remove components like `ColoredButton` and `ButtonBase` in exchange for `Clickable` and similar wrappers;
- After C# 14 release:
- [X] Replace `Animate` method with compiler-generated property extensions, so binding states will be as easy as writing `sText = _state`;
- [X] Add more state tools like `Map(x => x.Property)`;
- [ ] Replace current NotifyPropertyChanged pattern with compiler-generated type-safe events;
- [ ] Add API for layout animations `.AsFlexItem(onApply: x => myAnimatedSize.Value = x.Size)`.

If you feel like you have an interesting idea, let us know by opening an issue!