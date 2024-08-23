using System;

namespace Reactive.Components {
    public interface IKeyedControlComponentCell<TKey, in TParam> : IKeyedControlComponentCellBase<TKey, TParam> {
        event Action<TKey>? CellAskedToBeSelectedEvent;

        void OnCellStateChange(bool selected);
    }
}