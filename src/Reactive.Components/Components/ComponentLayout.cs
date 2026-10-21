using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// Wraps a component making it a layout driver, leaf layout is getting omitted at this place.
/// This component is unsafe, so must not be used on components with children as it can cause undefined layout behaviour.
/// </summary>
/// <typeparam name="T">A component to wrap.</typeparam>
[PublicAPI]
public abstract class ComponentLayout<T> : Layout, IComponentHolder<T> where T : IReactiveComponent, new() {
    protected T Component => _component;
    
    T IComponentHolder<T>.Component => _component;

    private T _component = default!;

    protected sealed override GameObject Construct() {
        // The layout gets promoted once it's added to the host,
        // so it replaces the wrapped component as the layout component
        return new T().Bind(ref _component).Use(null);
    }
}