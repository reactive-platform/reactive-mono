using System;
using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A persistent handle that's required to remove a bound callback.
/// </summary>
[PublicAPI]
public readonly struct StateSubscription {
    private readonly int _index;
    private readonly object _callback;
    private readonly IState _state;
    private readonly bool _isValid;

    public StateSubscription(int index, object callback, IState state) {
        _index = index;
        _callback = callback;
        _state = state;
        _isValid = true;
    }

    public (int index, object callback, IState state) GetData() {
        EnsureInitialized();

        return (_index, _callback, _state);
    }

    public void RemoveCallback() {
        _state.RemoveCallback(this);
    }

    private void EnsureInitialized() {
        if (!_isValid) {
            throw new InvalidOperationException("Called GetData on an invalidated subscription");
        }
    }
}

/// <summary>
/// A temporary handle that exposes an api to remove the callback.
/// </summary>
[PublicAPI]
public ref struct RefStateSubscription {
    // Ref fields aren't supported by Mono, so we rely on passing the entire struct by ref instead
    private bool _isValid;

    public RefStateSubscription(bool isValid) {
        _isValid = isValid;
    }

    public void RemoveCallback() {
        _isValid = false;
    }

    public bool GetIsValid() {
        return _isValid;
    }
}