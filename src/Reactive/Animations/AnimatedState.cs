using JetBrains.Annotations;

namespace Reactive {
    /// <summary>
    /// An animated reactive state.
    /// </summary>
    /// <typeparam name="T">A type of the state.</typeparam>
    [PublicAPI]
    public class AnimatedState<T> : AnimatedStateBase<T> {
        public AnimatedState(T initialValue, IValueInterpolator<T> valueInterpolator) : base(initialValue, valueInterpolator) { }

        public T TargetValue {
            get => Target;
            set => Retarget(value);
        }

        public void SetValueImmediate(T value) {
            SetImmediate(value);
        }
    }
}
