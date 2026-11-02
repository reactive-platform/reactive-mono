using System;
using JetBrains.Annotations;
using Reactive.Compiler;

namespace Reactive;

/// <summary>
/// A state computed from other states. The value is cached until one of them changes.
/// </summary>
[PublicAPI]
public abstract class DerivedStateBase<T> : StateBase<T>, IState<T>, IDisposable {
    public T Value {
        get {
            if (!_hasValue) {
                field = Compute();
                _hasValue = true;
            }

            return field;
        }
    }

    private bool _hasValue;

    protected abstract T Compute();
    public abstract void Dispose();

    protected StateSubscription Subscribe(IState state) {
        return state.AddInvalidationCallback(HandleStateUpdated, this, null!);
    }

    private static void HandleStateUpdated(ref RefStateSubscription sub, object arg1, object arg2) {
        var self = (DerivedStateBase<T>)arg1;

        self._hasValue = false;
        self.NotifyValueInvalidated();
    }
}

/// <summary>
/// A state computed from other states, recalculated when one of them changes.
/// The predicate receives their values in the same order.
/// </summary>
[PublicAPI]
[VariadicGen(
    8,
    "T{}",
    "IState<T{}> state{}",
    "IState<T{}> _state{}",
    "StateSubscription _subscription{}",
    "_state{} = state{}",
    "_subscription{} = Subscribe(state{})",
    "_state{}.Value",
    "_subscription{}.RemoveCallback()"
)]
public sealed class DerivedState<T1, T> : DerivedStateBase<T> {
    private readonly Func<T1, T> _predicate;
    private readonly IState<T1> _state1;
    private readonly StateSubscription _subscription1;

    public DerivedState(IState<T1> state1, [NoStateCapture] Func<T1, T> predicate) {
        _predicate = predicate;
        _state1 = state1;
        _subscription1 = Subscribe(state1);
    }

    protected override T Compute() {
        return _predicate(_state1.Value);
    }

    public override void Dispose() {
        _subscription1.RemoveCallback();
    }
}