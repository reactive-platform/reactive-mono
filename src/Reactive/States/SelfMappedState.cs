using System;
using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A wrapper class over <see cref="IState"/> that exposes the state itself as the value, notifying whenever the state changes.
/// </summary>
[PublicAPI]
public class SelfMappedState<TState> : StateBase<TState>, IState<TState>, IDisposable where TState : IState {
    public TState Value => _state;

    private readonly TState _state;
    private StateSubscription _subscription;

    public SelfMappedState(TState state) {
        _state = state;
        _subscription = state.AddInvalidationCallback(HandleValueChanged, this, null!);
    }

    private static void HandleValueChanged(ref RefStateSubscription _, object arg1, object arg2) {
        var self = (SelfMappedState<TState>)arg1;

        self.NotifyValueInvalidated();
    }

    public void Dispose() {
        _state.RemoveCallback(_subscription);
    }
}
