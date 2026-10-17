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

    public bool IsDirty { get; private set; }

    // Helps to avoid evaluating Value when there's nothing to invoke
    protected bool HasCallbacks => _list.HasCallbacks;

    protected StateBase() {
        _list = new(this);
    }

    /// <summary>
    /// Notifies that the state has a new value. States derived from it are invalidated at once,
    /// callbacks get the value when the state is flushed.
    /// </summary>
    protected void NotifyValueInvalidated() {
        _list.InvokeInvalidation();

        if (_list.HasCallbacks && !IsDirty) {
            IsDirty = true;
            ReactiveScheduler.State.Schedule(this);
        }
    }

    /// <summary>
    /// Runs the callbacks with the state's value if the state is dirty. The state must implement <see cref="IState{T}"/>.
    /// </summary>
    public void Flush() {
        if (!IsDirty) {
            return;
        }

        IsDirty = false;

        if (_list.HasCallbacks) {
            _list.Invoke(((IState<T>)this).Value);
        }
    }

    public StateSubscription AddCallback(StateCallback<T> callback, object arg1, object arg2) {
        return _list.Add(callback, arg1, arg2);
    }

    public StateSubscription AddCallback(StateCallback callback, object arg1, object arg2) {
        return _list.Add(callback, arg1, arg2);
    }

    public StateSubscription AddInvalidationCallback(StateCallback callback, object arg1, object arg2) {
        return _list.AddInvalidation(callback, arg1, arg2);
    }

    public bool RemoveCallback(in StateSubscription sub) {
        return _list.Remove(sub);
    }
}