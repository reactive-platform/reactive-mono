using System;
using JetBrains.Annotations;

namespace Reactive {
    [PublicAPI]
    public class State<T> : IMutableState<T> {
        public State(T initialValue) {
            _value = initialValue;
        }

        public T Value {
            get => _value;
            set {
                _value = value;
                ValueChangedEvent?.Invoke(value);
                StateUpdatedEvent?.Invoke();
            }
        }

        public event Action<T>? ValueChangedEvent;
        public event Action? StateUpdatedEvent;

        private T _value;

        public void ClearBindings() {
            ValueChangedEvent = null;
            StateUpdatedEvent = null;
        }

        public static implicit operator T(State<T> value) {
            return value._value;
        }
    }
}