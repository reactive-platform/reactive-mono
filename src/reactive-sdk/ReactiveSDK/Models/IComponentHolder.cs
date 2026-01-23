using JetBrains.Annotations;

namespace Reactive.Components;

/// <summary>
/// Represents an object that holds (wraps) an instance of another object.
/// Usually used to write scalable extensions.
/// </summary>
/// <typeparam name="T">An instance of another object.</typeparam>
[PublicAPI]
public interface IComponentHolder<out T> {
    T Component { get; }
}