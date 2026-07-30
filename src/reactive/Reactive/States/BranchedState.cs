using System;
using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A wrapper class over <see cref="IState"/> that enables
/// an ability to set a condition over state updates.
/// </summary>
[PublicAPI]
public class BranchedState<T> : StateBase<T>, IState<T>, IDisposable {
    /// <summary>
    /// Represents a state value. Keep in mind that it returns the last value
    /// that met branching conditions, so it's not guaranteed that the value is equal to the original one.
    /// However, this value will always be equal to the original one before
    /// the first update, no matter it matches the condition or not.
    /// </summary>
    public T Value { get; private set; }

    private readonly IState<T> _state;
    private readonly Func<T, bool> _predicate;
    private readonly StateSubscription _subscription;

    public BranchedState(IState<T> state, Func<T, bool> predicate) {
        _state = state;
        _predicate = predicate;

        Value = state.Value;
        _subscription = state.AddCallback(HandleValueChanged, this, null!);
    }

    private static void HandleValueChanged(ref RefStateSubscription sub, T value, object arg1, object arg2) {
        var self = (BranchedState<T>)arg1;

        if (self._predicate(value)) {
            self.Value = value;
            self.NotifyValueChanged(value);
        }
    }

    public void Dispose() {
        _state.RemoveCallback(_subscription);
    }
}