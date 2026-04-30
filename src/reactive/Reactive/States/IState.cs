using System;
using JetBrains.Annotations;

namespace Reactive {
    /// <summary>
    /// Represents a reactive state.
    /// </summary>
    /// <typeparam name="T">A type of the state.</typeparam>
    [PublicAPI]
    public interface IState<out T> : IState {
        T Value { get; }

        /// <summary>
        /// Fired whenever the state is updated.
        /// </summary>
        event Action<T>? ValueChangedEvent;
    }
    
    /// <summary>
    /// Represents a non-generic state.
    /// </summary>
    [PublicAPI]
    public interface IState {
        /// <summary>
        /// Fired whenever the state is updated.
        /// </summary>
        event Action? StateUpdatedEvent;
    }
}