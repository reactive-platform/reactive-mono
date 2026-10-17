using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reactive.Components;

[Flags]
public enum CellUpdateType {
    None = 0,
    Item = 1 << 0,
    Selection = 1 << 1
}

[PublicAPI]
public static class CellUpdateExtensions {
    extension(CellUpdateType type) {
        public bool IsItem => (type & CellUpdateType.Item) != 0;
        public bool IsSelection => (type & CellUpdateType.Selection) != 0;
    }
}

public class CellContext<T> : StateBase<CellContext<T>>, IState<CellContext<T>> {
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

    /// <summary>
    /// Kinds of updates delivered with the last flush, combined as flags. Kept until the next flush.
    /// </summary>
    public CellUpdateType UpdateType { get; private set; }

    private CellUpdateType _pendingUpdateType;
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
    
    private void NotifyValueChanged(CellUpdateType type) {
        _pendingUpdateType |= type;
        NotifyValueInvalidated();
    }

    protected override void OnFlush() {
        UpdateType = _pendingUpdateType;
        _pendingUpdateType = CellUpdateType.None;
    }

    #endregion
}