using System;

namespace Reactive;

internal readonly struct StateCallbackEntry<T> {
    public StateCallbackEntry(StateCallback callback, object arg1, object arg2) {
        Callback = callback;
        _arg1 = arg1;
        _arg2 = arg2;
        _typed = false;
    }

    public StateCallbackEntry(StateCallback<T> callback, object arg1, object arg2) {
        Callback = callback;
        _arg1 = arg1;
        _arg2 = arg2;
        _typed = true;
    }

    public readonly object Callback;

    private readonly bool _typed;
    private readonly object _arg1;
    private readonly object _arg2;

    public void Invoke(ref RefStateSubscription sub, T value) {
        if (_typed) {
            ((StateCallback<T>)Callback).Invoke(ref sub, value, _arg1, _arg2);
        } else {
            ((StateCallback)Callback).Invoke(ref sub, _arg1, _arg2);
        }
    }
}

/// <summary>
/// Such types are usually classes, but as it's meant to be used in
/// every state, making it a class would be a performance bottleneck 
/// as each state would require an additional allocation. You should never clone this struct.
/// </summary>
public struct StateCallbacksList<T> {
    private StateCallbackEntry<T>?[]? _array;
    private int _lastAvailableSlot;
    private readonly IState _state;

    public bool HasCallbacks => _array?.Length > 0;

    public StateCallbacksList(IState state) {
        _state = state;
    }

    public void Invoke(T value) {
        for (var i = 0; i < _array?.Length; i++) {
            if (_array[i] == null) {
                continue;
            }

            var sub = new RefStateSubscription(true);

            _array[i]?.Invoke(ref sub, value);

            if (!sub.GetIsValid()) {
                _array[i] = null;
            }
        }
    }

    public StateSubscription Add(StateCallback callback, object arg1, object arg2) {
        var slot = GetSlot();

        _array![slot] = new(callback, arg1, arg2);
        _lastAvailableSlot = slot + 1;

        return new(slot, callback, _state);
    }

    public StateSubscription Add(StateCallback<T> callback, object arg1, object arg2) {
        var slot = GetSlot();

        _array![slot] = new(callback, arg1, arg2);
        _lastAvailableSlot = slot + 1;

        return new(slot, callback, _state);
    }

    public bool Remove(in StateSubscription sub) {
        if (_array == null) {
            return false;
        }

        var (i, cb, state) = sub.GetData();

        if (!ReferenceEquals(_state, state) || _array.Length <= i) {
            return false;
        }

        var callback = _array[i]?.Callback;

        if (!ReferenceEquals(callback, cb)) {
            return false;
        }

        _array[i] = null;
        // When inserting new callbacks we use forward lookups,
        // so we store the first unoccupied entry
        if (i < _lastAvailableSlot) {
            _lastAvailableSlot = i;
        }

        return true;
    }

    private int GetSlot() {
        if (_array == null) {
            _array = new StateCallbackEntry<T>?[4];
            return 0;
        }

        var len = _array.Length;

        for (var i = _lastAvailableSlot; i < len; i++) {
            if (!_array[i].HasValue) {
                return i;
            }
        }

        Array.Resize(ref _array, _array.Length * 2);
        return len;
    }
}