using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A universal state base that implement the notification logic.
/// It intentionally doesn't implement IState and IMutableState to let you chose
/// what you need and avoid explicit interface implementations.
/// </summary>
/// <typeparam name="T">A type of the state.</typeparam>
[PublicAPI]
public abstract class StateBase<T> : IState {
    private StateCallbacksList<T> _list;

    // Helps to avoid evaluating Value when there's nothing to invoke
    protected bool HasCallbacks => _list.HasCallbacks;

    protected StateBase() {
        _list = new(this);
    }

    protected void NotifyValueChanged(T value) {
        _list.Invoke(value);
    }

    public StateSubscription AddCallback(StateCallback<T> callback, object arg1, object arg2) {
        return _list.Add(callback, arg1, arg2);
    }

    public StateSubscription AddCallback(StateCallback callback, object arg1, object arg2) {
        return _list.Add(callback, arg1, arg2);
    }

    public bool RemoveCallback(in StateSubscription sub) {
        return _list.Remove(sub);
    }
}