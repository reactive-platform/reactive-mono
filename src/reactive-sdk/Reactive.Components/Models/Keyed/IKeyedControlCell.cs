using System;

namespace Reactive.Components {
    public interface IKeyedControlCell<TKey, in TParam> : IKeyedControlCellBase<TKey, TParam> {
        event Action<TKey>? CellAskedToBeSelectedEvent;

        void OnCellStateChange(bool selected);
    }
}