using JetBrains.Annotations;

namespace Reactive;

public delegate void StateCallback(ref RefStateSubscription sub, object arg1, object arg2);
public delegate void StateCallback<in T>(ref RefStateSubscription sub, T value, object arg1, object arg2);

/// <summary>
/// Represents a reactive state.
/// </summary>
/// <typeparam name="T">A type of the state.</typeparam>
[PublicAPI]
public interface IState<out T> : IState {
    T Value { get; }

    StateSubscription AddCallback(StateCallback<T> callback, object arg1, object arg2);
}

/// <summary>
/// Represents a non-generic reactive state.
/// </summary>
[PublicAPI]
public interface IState {
    /// <summary>
    /// Whether the state changed since its callbacks last ran. A dirty state is scheduled
    /// to be flushed by <see cref="ReactiveScheduler.State"/>.
    /// </summary>
    bool IsDirty { get; }

    /// <summary>
    /// Runs the callbacks if the state is dirty.
    /// </summary>
    void Flush();

    StateSubscription AddCallback(StateCallback callback, object arg1, object arg2);

    /// <summary>
    /// Adds a callback invoked at once when the state changes, before it's flushed: how derived states
    /// learn that a dependency changed without evaluating anything.
    /// </summary>
    StateSubscription AddInvalidationCallback(StateCallback callback, object arg1, object arg2);

    bool RemoveCallback(in StateSubscription sub);
}