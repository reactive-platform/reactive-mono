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
    /// Represents a state value. Evaluated on each call.
    /// </summary>
    public TNew Value {
        get {
            _value ??= _predicate(_state.Value);
            return _value;
        }
    }

    private readonly IState<T> _state;
    private readonly Func<T, TNew> _predicate;
    private StateSubscription _subscription;
    private TNew? _value;

    public MappedState(IState<T> state, Func<T, TNew> predicate) {
        _state = state;
        _predicate = predicate;
        _subscription = state.AddCallback(HandleValueChanged, this, null!);
    }

    private static void HandleValueChanged(ref RefStateSubscription _, T value, object arg1, object arg2) {
        var self = (MappedState<T, TNew>)arg1;
        
        self._value = default;

        if (self.HasCallbacks) {
            self.NotifyValueChanged(self.Value);
        }
    }

    public void Dispose() {
        _state.RemoveCallback(_subscription);
    }
}