using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// A wrapper class over <see cref="IState"/> that updates when one of the dependencies does.
/// </summary>
[PublicAPI]
public class DerivedState<T, TDeps> : IState<T>, IDisposable where TDeps : ITuple {
    public T Value => _predicate(_dependencies);

    public event Action<T>? ValueChangedEvent;

    private readonly TDeps _dependencies;
    private readonly Func<TDeps, T> _predicate;

    public DerivedState(Func<TDeps, T> predicate, TDeps dependencies) {
        _dependencies = dependencies;
        _predicate = predicate;

        // C# does not support variadic generics (or templates) as C++ does,
        // hence to avoid writing custom generators or lots of boilerplate,
        // we use this workaround as a "temporary" solution
        
        for (var i = 0; i < dependencies.Length; i++) {
            // Non-state objects are simply ignored
            if (dependencies[i] is IState<object> state) {
                state.ValueChangedEvent += HandleValueChanged;
            } else {
                Debug.LogWarning("You've passed a dependency which is not a state, consider removing it and passing directly");
            }
        }
    }

    private void HandleValueChanged(object value) {
        ValueChangedEvent?.Invoke(_predicate(_dependencies));
    }

    public void Dispose() {
        for (var i = 0; i < _dependencies.Length; i++) {
            if (_dependencies[i] is IState<object> state) {
                state.ValueChangedEvent -= HandleValueChanged;
            }
        }
    }
}