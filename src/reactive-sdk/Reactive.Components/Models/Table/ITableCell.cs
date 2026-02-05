using System;
using JetBrains.Annotations;

namespace Reactive.Components {
    [PublicAPI]
    public interface ITableCell<in TItem> : IListCell<TItem> {
        event Action<ITableCell<TItem>, bool>? CellAskedToChangeSelectionEvent;

        void OnCellStateChange(bool selected);
    }
}