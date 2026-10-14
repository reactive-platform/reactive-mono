using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A universal state base that implement the notification logic.
/// It intentionally doesn't implement IState and IMutableState to let you chose
/// what you need and avoid explicit interface implementations.
/// </summary>
/// <typeparam name="T">A type of the state.</typeparam>
[PublicAPI]
public abstract class StateBase<T> : IState, IInvalidationSource, IScheduledState {
    private StateCallbacksList<T> _list;
    private T _pendingValue = default!;
    private bool _hasPendingValue;
    private bool _scheduled;

    // Helps to avoid evaluating Value when there's nothing to invoke
    protected bool HasCallbacks => _list.HasCallbacks;

    protected StateBase() {
        _list = new(this);
    }

    /// <summary>
    /// Notifies that the state has a new value. States derived from it learn at once,
    /// callbacks get the latest value when <see cref="StateScheduler"/> flushes.
    /// </summary>
    protected void NotifyValueChanged(T value) {
        StateScheduler.BeginNotification();
        _list.InvokeInvalidation();

        if (_list.HasCallbacks) {
            _pendingValue = value;
            _hasPendingValue = true;
            Schedule();
        }

        StateScheduler.EndNotification();
    }

    /// <summary>
    /// Like <see cref="NotifyValueChanged"/>, but callbacks run before it returns, as for an event. For states whose
    /// notification carries something that's gone afterwards (ScrollContext's UpdateType). States derived from it
    /// still run their callbacks at the next flush.
    /// </summary>
    protected void NotifyValueChangedImmediately(T value) {
        StateScheduler.BeginNotification();
        _list.InvokeInvalidation();
        // A pending delivery would only repeat this one
        Cancel();
        _list.Invoke(value);
        StateScheduler.EndNotification();
    }

    /// <summary>
    /// Notifies that the state's value has to be evaluated again, for states that evaluate it when read
    /// (the state must implement <see cref="IState{T}"/>). Callbacks read it when <see cref="StateScheduler"/>
    /// flushes, so it's evaluated once however many times it was invalidated.
    /// </summary>
    protected void NotifyValueInvalidated() {
        StateScheduler.BeginNotification();
        _list.InvokeInvalidation();

        if (_list.HasCallbacks) {
            _pendingValue = default!;
            _hasPendingValue = false;
            Schedule();
        }

        StateScheduler.EndNotification();
    }

    private void Schedule() {
        if (_scheduled) {
            return;
        }

        _scheduled = true;
        StateScheduler.Schedule(this);
    }

    void IScheduledState.Deliver() {
        // Delivered immediately since it was scheduled
        if (!_scheduled) {
            return;
        }

        var value = _pendingValue;
        var hasValue = _hasPendingValue;

        Cancel();

        if (!_list.HasCallbacks) {
            return;
        }

        _list.Invoke(hasValue ? value : ((IState<T>)this).Value);
    }

    void IScheduledState.Cancel() => Cancel();

    private void Cancel() {
        _scheduled = false;
        _hasPendingValue = false;
        // Don't keep the value alive until the next change
        _pendingValue = default!;
    }

    public StateSubscription AddCallback(StateCallback<T> callback, object arg1, object arg2) {
        return _list.Add(callback, arg1, arg2);
    }

    public StateSubscription AddCallback(StateCallback callback, object arg1, object arg2) {
        return _list.Add(callback, arg1, arg2);
    }

    StateSubscription IInvalidationSource.AddInvalidationCallback(StateCallback callback, object arg1, object arg2) {
        return _list.AddInvalidation(callback, arg1, arg2);
    }

    public bool RemoveCallback(in StateSubscription sub) {
        return _list.Remove(sub);
    }
}

/// <summary>
/// A state that can tell its dependents about a change without evaluating its value.
/// </summary>
internal interface IInvalidationSource {
    StateSubscription AddInvalidationCallback(StateCallback callback, object arg1, object arg2);
}
