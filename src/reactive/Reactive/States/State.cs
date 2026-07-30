using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reactive {
    [PublicAPI]
    public class State<T> : StateBase<T>, IMutableState<T> {
        public State(T initialValue) {
            _value = initialValue;
        }

        public T Value {
            get => _value;
            set {
                if (EqualityComparer<T>.Default.Equals(value, _value)) {
                    return;
                }
                
                _value = value;
                NotifyValueChanged(value);
            }
        }
        
        private T _value;

        public static implicit operator T(State<T> value) {
            return value._value;
        }
    }
}