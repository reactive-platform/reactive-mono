using System;

namespace Reactive;

internal readonly struct StateCallbackEntry<T> {
    public StateCallbackEntry(StateCallback callback, object arg1, object arg2, bool invalidation) {
        Callback = callback;
        Invalidation = invalidation;
        _arg1 = arg1;
        _arg2 = arg2;
        _typed = false;
    }

    public StateCallbackEntry(StateCallback<T> callback, object arg1, object arg2) {
        Callback = callback;
        Invalidation = false;
        _arg1 = arg1;
        _arg2 = arg2;
        _typed = true;
    }

    public readonly object Callback;
    public readonly bool Invalidation;

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
    private int _count;
    private int _invalidationCount;
    private readonly IState _state;

    /// <summary>
    /// Whether there are callbacks that take the value (invalidation callbacks don't).
    /// </summary>
    public bool HasCallbacks => _count > _invalidationCount;

    public StateCallbacksList(IState state) {
        _state = state;
    }

    public void Invoke(T value) {
        if (HasCallbacks) {
            Invoke(value, false);
        }
    }

    public void InvokeInvalidation() {
        if (_invalidationCount > 0) {
            Invoke(default!, true);
        }
    }

    private void Invoke(T value, bool invalidation) {
        for (var i = 0; i < _array?.Length; i++) {
            if (_array[i] is not { } entry || entry.Invalidation != invalidation) {
                continue;
            }

            var sub = new RefStateSubscription(true);

            entry.Invoke(ref sub, value);

            if (!sub.GetIsValid()) {
                Free(i);
            }
        }
    }

    public StateSubscription Add(StateCallback callback, object arg1, object arg2) {
        return Add(new(callback, arg1, arg2, false), callback);
    }

    public StateSubscription Add(StateCallback<T> callback, object arg1, object arg2) {
        return Add(new(callback, arg1, arg2), callback);
    }

    /// <summary>
    /// Adds a callback invoked at once when the state changes, without a value.
    /// </summary>
    public StateSubscription AddInvalidation(StateCallback callback, object arg1, object arg2) {
        _invalidationCount++;
        return Add(new(callback, arg1, arg2, true), callback);
    }

    private StateSubscription Add(StateCallbackEntry<T> entry, object callback) {
        var slot = GetSlot();

        _array![slot] = entry;
        _lastAvailableSlot = slot + 1;
        _count++;

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

        Free(i);

        return true;
    }

    private void Free(int i) {
        if (_array![i]!.Value.Invalidation) {
            _invalidationCount--;
        }

        _array[i] = null;
        _count--;
        // When inserting new callbacks we use forward lookups,
        // so we store the first unoccupied entry
        if (i < _lastAvailableSlot) {
            _lastAvailableSlot = i;
        }
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