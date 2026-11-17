using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive {
    /// <summary>
    /// An animated reactive state.
    /// </summary>
    /// <typeparam name="T">A type of the state.</typeparam>
    [PublicAPI]
    public class AnimatedState<T> : StateBase<T>, IState<T>, IReactiveModule {
        public AnimatedState(T initialValue, IValueInterpolator<T> valueInterpolator) {
            _animation.Interpolator = valueInterpolator;
            _animation.Set(valueInterpolator.Lerp(initialValue, initialValue, 1f));
        }

        public T TargetValue {
            get => _animation.To;
            set {
                var shouldNotify = _animation.IsRunning;

                if (_animation.Retarget(value) && shouldNotify) {
                    OnStart?.Invoke(Value);
                }
            }
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

        private AnimatedValue<T> _animation;

        public void SetValueImmediate(T value) {
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

        public static implicit operator T(AnimatedState<T> state) {
            return state.Value;
        }
    }
}