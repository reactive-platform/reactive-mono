using System;
using System.Collections.Generic;

namespace Reactive.Components;

public enum CellUpdateType {
    None,
    Item,
    Selection
}

public class CellContext<T> : IState<CellContext<T>> {
    public T Item => _item ?? throw new();

    public int Index { get; private set; }
    public int TotalCells { get; private set; }

    public bool Selected {
        get;
        set {
            if (field == value) {
                return;
            }

            field = value;
            NotifyValueChanged(CellUpdateType.Selection);
        }
    }

    public CellUpdateType UpdateType { get; private set; }

    private T? _item;

    // TODO: hide from the user
    public void Init(T item, int index, int totalCells) {
        var updated = false;

        // EqualityComparer handles nulls, so it's completely safe to unwrap
        if (!EqualityComparer<T>.Default.Equals(_item!, item)) {
            _item = item;
            updated = true;
        }

        if (Index != index) {
            Index = index;
            updated = true;
        }

        if (TotalCells != totalCells) {
            TotalCells = totalCells;
            updated = true;
        }

        if (updated) {
            NotifyValueChanged(CellUpdateType.Item);
        }
    }

    #region State

    public CellContext<T> Value => this;

    public event Action<CellContext<T>>? ValueChangedEvent;
    public event Action? StateUpdatedEvent;

    private void NotifyValueChanged(CellUpdateType type) {
        UpdateType = type;

        ValueChangedEvent?.Invoke(this);
        StateUpdatedEvent?.Invoke();

        UpdateType = CellUpdateType.None;
    }

    #endregion
}