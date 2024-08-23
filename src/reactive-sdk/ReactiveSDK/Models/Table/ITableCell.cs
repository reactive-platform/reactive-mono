using System;

namespace Reactive.Components {
    public interface ITableCell<in TItem> {
        event Action<ITableCell<TItem>, bool>? CellAskedToChangeSelectionEvent;

        void Init(TItem item);
        void OnCellStateChange(bool selected);
    }
}