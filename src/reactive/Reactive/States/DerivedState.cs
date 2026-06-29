using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// A wrapper class over <see cref="IState"/> that updates when one of the dependencies does.
/// </summary>
[PublicAPI]
public class DerivedState<T, TDeps> : StateBase<T>, IState<T>, IDisposable where TDeps : ITuple {
    public T Value {
        get {
            _value ??= _predicate(_dependencies);
            return _value;
        }
    }

    private readonly TDeps _dependencies;
    private readonly Func<TDeps, T> _predicate;
    private readonly StateSubscription[] _subscriptions;
    private readonly int _occupiedSubsLen;
    private T? _value;

    public DerivedState(Func<TDeps, T> predicate, TDeps dependencies) {
        _dependencies = dependencies;
        _predicate = predicate;
        _subscriptions = new StateSubscription[dependencies.Length];

        // C# does not support variadic generics (or templates) as C++ does,
        // hence to avoid writing custom generators or lots of boilerplate,
        // we use this workaround as a "temporary" solution

        for (var i = 0; i < dependencies.Length; i++) {
            // Non-state objects are simply ignored
            if (dependencies[i] is IState state) {
                _subscriptions[_occupiedSubsLen] = state.AddCallback(HandleStateUpdated, this, null!);
                _occupiedSubsLen++;
            } else {
                Debug.LogWarning("You've passed a dependency which is not a state, consider removing it and passing directly");
            }
        }
    }

    private static void HandleStateUpdated(ref RefStateSubscription sub, object arg1, object arg2) {
        var self = (DerivedState<T, TDeps>)arg1;

        self._value = default;

        if (self.HasCallbacks) {
            self.NotifyValueChanged(self.Value);
        }
    }

    public void Dispose() {
        for (var i = 0; i < _occupiedSubsLen; i++) {
            _subscriptions[i].RemoveCallback();
        }
    }
}