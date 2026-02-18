using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive {
    /// <summary>
    /// An animated reactive state.
    /// </summary>
    /// <typeparam name="T">A type of the state.</typeparam>
    [PublicAPI]
    public class AnimatedState<T> : IState<T>, IReactiveModule {
        public AnimatedState(T initialValue, IValueInterpolator<T> valueInterpolator) {
            _startValue = initialValue;
            _targetValue = initialValue;
            _valueInterpolator = valueInterpolator;
            _set = true;
            _progress = 1f;
            _elapsedTime = 0f;
        }

        public T TargetValue {
            get => _targetValue;
            set {
                if (_valueInterpolator.Equals(_targetValue, value)) {
                    return;
                }

                _startValue = Value;
                _targetValue = value;

                _elapsedTime = 0f;
                _progress = 0f;

                var shouldNotify = !_set;
                _set = false;

                if (shouldNotify) {
                    OnStart?.Invoke(Value);
                }
            }
        }

        public float Progress {
            get => _progress;
            private set {
                _progress = value;
                ValueChangedEvent?.Invoke(Value);
            }
        }
        
        public T Value => _valueInterpolator.Lerp(_startValue, _targetValue, _progress);
        public bool IsFinished => _set;

        public AnimationDuration Duration { get; set; }
        public AnimationCurve Curve { get; set; } = AnimationCurve.Linear;

        public Action<T>? OnFinish { get; set; }
        public Action<T>? OnStart { get; set; }

        public event Action<T>? ValueChangedEvent;

        private readonly IValueInterpolator<T> _valueInterpolator;
        private T _targetValue;
        private T _startValue;

        private float _progress;
        private float _elapsedTime;
        private bool _set;

        public void SetValueImmediate(T value, bool silent = false) {
            _set = true;
            _startValue = value;
            _targetValue = value;
            _targetValue = _valueInterpolator.Lerp(_startValue, _targetValue, 1f);

            if (!silent) {
                Progress = 1f;
                FinishAnimation();
            } else {
                _progress = 1f;
            }
        }

        /// <summary>
        /// Evaluates the animation on the next frame. Use this to provide default values to subscribers.
        /// If called when the animation is running, nothing will happen.
        /// </summary>
        public void EvaluateNextFrame() {
            _set = false;
        }

        void IReactiveModule.OnUpdate() {
            if (_set) {
                return;
            }

            if (Duration.Unit is DurationUnit.Seconds) {
                _elapsedTime += Time.deltaTime;
                _progress = Mathf.Clamp01(_elapsedTime / Duration);
            } else {
                _progress = Mathf.Lerp(Progress, 1f, Time.deltaTime * Duration);
            }

            Progress = Curve.Evaluate(Progress);
            // Finishing if needed
            if (Mathf.Approximately(1f, Progress)) {
                FinishAnimation();
            }
        }

        void IReactiveModule.OnBind() { }
        void IReactiveModule.OnUnbind() { }

        private void FinishAnimation() {
            _set = true;
            _elapsedTime = 0f;

            OnFinish?.Invoke(Value);
        }

        public static implicit operator T(AnimatedState<T> state) {
            return state.Value;
        }
    }
}