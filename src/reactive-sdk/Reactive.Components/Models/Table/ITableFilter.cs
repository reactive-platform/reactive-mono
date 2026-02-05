using System;

namespace Reactive.Components {
    /// <summary>
    /// A table filter.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface ITableFilter<in T> {
        event Action? FilterUpdatedEvent;
        
        bool Matches(T value);
    }
}