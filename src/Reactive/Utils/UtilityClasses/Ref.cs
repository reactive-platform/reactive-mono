using JetBrains.Annotations;

namespace Reactive {
    /// <summary>
    /// A shareable reference to a value that may be assigned later.
    /// </summary>
    /// <typeparam name="T">A type of the referenced value.</typeparam>
    [PublicAPI]
    public sealed class Ref<T> where T : class {
        public Ref() { }

        public Ref(T? value) {
            Value = value;
        }

        /// <summary>
        /// The referenced value. Null until assigned.
        /// </summary>
        public T? Value { get; set; }

        public bool HasValue => Value != null;

        public static implicit operator Ref<T>(T value) {
            return new(value);
        }
    }
}
