using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Reactive.Compiler;

[EditorBrowsable(EditorBrowsableState.Never)]
public readonly ref struct StateBinder<T, TState> where TState : IState<T> {
    private readonly TState? _state;
    private readonly bool _lazy;

    internal StateBinder(TState state, bool lazy) {
        _state = state;
        _lazy = lazy;
    }

    public void AddCallback<TComp>(TComp comp, Action<TComp, T> callback) where TComp : ILifetimeProvider {
        EnsureInitialized();
        
        _state!.AddCallback(comp, callback, _lazy);
    }

    public void AddCallbackUnity<TComp>(TComp comp, Action<TComp, T> callback) where TComp : UnityEngine.Object {
        EnsureInitialized();
        
        _state!.AddCallbackUnity(comp, callback, _lazy);
    }

    private void EnsureInitialized() {
        if (_state == null) {
            throw new InvalidOperationException("StateBinder wasn't initialized");
        }
    }

    public static implicit operator StateBinder<T, TState>(TState state) {
        return new(state, false);
    }
}

[PublicAPI]
public static class StateBinderExtensions {
    extension<T>(IState<T> binder) {
        public StateBinder<T, IState<T>> In() {
            return new(binder, false);
        }

        public StateBinder<T, IState<T>> InLazy() {
            return new(binder, true);
        }
    }
}