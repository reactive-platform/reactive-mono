using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

[PublicAPI]
public abstract class AnimatedStateBase<T> : StateBase<T>, IState<T>, IReactiveModule {
    protected AnimatedStateBase(T initialValue, IValueInterpolator<T> valueInterpolator) {
        _animation.Interpolator = valueInterpolator;
        _animation.Set(valueInterpolator.Lerp(initialValue, initialValue, 1f));
    }

    public float Progress => _animation.Progress;

    public T Value => _animation.Value;

    public bool IsFinished => !_animation.IsRunning;

    public AnimationTiming Timing {
        get => _animation.Timing;
        set => _animation.Timing = value;
    }

    public AnimationCurve Curve {
        get => _animation.Curve ?? AnimationCurve.Linear;
        set => _animation.Curve = value;
    }

    public Action<T>? OnFinish { get; set; }
    public Action<T>? OnStart { get; set; }

    protected T Target => _animation.To;

    private AnimatedValue<T> _animation;

    protected void Retarget(T value) {
        var shouldNotify = _animation.IsRunning;

        if (_animation.Retarget(value) && shouldNotify) {
            OnStart?.Invoke(Value);
        }
    }

    protected void SetImmediate(T value) {
        _animation.Set(value);

        NotifyValueInvalidated();
        OnFinish?.Invoke(Value);
    }

    void IReactiveModule.OnUpdate() {
        if (!_animation.IsRunning) {
            return;
        }

        _animation.Advance(Time.deltaTime);
        NotifyValueInvalidated();

        if (!_animation.IsRunning) {
            OnFinish?.Invoke(Value);
        }
    }

    void IReactiveModule.OnBind() { }
    void IReactiveModule.OnUnbind() { }

    public static implicit operator T(AnimatedStateBase<T> state) {
        return state.Value;
    }
}
