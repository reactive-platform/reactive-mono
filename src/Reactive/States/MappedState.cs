using System;
using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A wrapper class over <see cref="IState"/> that enables
/// an ability to map a state value.
/// </summary>
[PublicAPI]
public class MappedState<T, TNew> : StateBase<TNew>, IState<TNew>, IDisposable {
    /// <summary>
    /// Represents a state value. Evaluated when read after the original state changed.
    /// </summary>
    public TNew Value {
        get {
            if (!_hasValue) {
                field = _predicate(_state.Value);
                _hasValue = true;
            }

            return field;
        }
    }

    private readonly IState<T> _state;
    private readonly Func<T, TNew> _predicate;
    private StateSubscription _subscription;
    private bool _hasValue;

    public MappedState(IState<T> state, Func<T, TNew> predicate) {
        _state = state;
        _predicate = predicate;
        _subscription = state.AddInvalidationCallback(HandleValueChanged, this, null!);
    }

    private static void HandleValueChanged(ref RefStateSubscription _, object arg1, object arg2) {
        var self = (MappedState<T, TNew>)arg1;

        self._hasValue = false;
        self.NotifyValueChanged();
    }

    public void Dispose() {
        _state.RemoveCallback(_subscription);
    }
}