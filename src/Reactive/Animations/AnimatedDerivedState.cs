using System;
using JetBrains.Annotations;

namespace Reactive;

[PublicAPI]
public sealed class AnimatedDerivedState<T> : AnimatedStateBase<T>, IDisposable {
    public AnimatedDerivedState(IState<T> source, IValueInterpolator<T> valueInterpolator) : base(source.Value, valueInterpolator) {
        _subscription = source.AddCallback(HandleSourceUpdated, this, null!);
    }

    public T TargetValue => Target;

    private readonly StateSubscription _subscription;

    private static void HandleSourceUpdated(ref RefStateSubscription sub, T value, object arg1, object arg2) {
        ((AnimatedDerivedState<T>)arg1).Retarget(value);
    }

    public void Dispose() {
        _subscription.RemoveCallback();
    }
}
