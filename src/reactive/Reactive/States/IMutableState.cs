namespace Reactive {
    /// <summary>
    /// Represents a mutable variant of the reactive state.
    /// </summary>
    /// <typeparam name="T">A type of the state.</typeparam>
    public interface IMutableState<T> : IState<T> {
        new T Value { get; set; }
    }
}