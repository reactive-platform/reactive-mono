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
    StateSubscription AddCallback(StateCallback callback, object arg1, object arg2);
    bool RemoveCallback(in StateSubscription sub);
}