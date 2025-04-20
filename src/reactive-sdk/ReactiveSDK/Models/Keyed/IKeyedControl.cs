using System;
using System.Collections.Generic;

namespace Reactive.Components {
    /// <summary>
    /// Abstraction for UI elements that offer to select one item from a list
    /// </summary>
    /// <typeparam name="TKey">An item key</typeparam>
    public interface IKeyedControl<TKey> {
        TKey SelectedKey { get; }

        event Action<TKey>? SelectedKeyChangedEvent; 
        
        void Select(TKey key);
    }
    
    /// <summary>
    /// Abstraction for UI elements that offer to select one item from a list
    /// </summary>
    /// <typeparam name="TKey">An item key</typeparam>
    /// <typeparam name="TParam">A param to be passed with key to provide additional info</typeparam>
    public interface IKeyedControl<TKey, TParam> : IKeyedControl<TKey> {
        IDictionary<TKey, TParam> Items { get; }
    }
}