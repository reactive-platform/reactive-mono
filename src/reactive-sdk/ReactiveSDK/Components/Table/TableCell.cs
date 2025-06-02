using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class TableCell<TItem> : ReactiveComponent, ITableCell<TItem> {
        #region Factory

        private IReactiveComponent? _constructedComponent;

        public delegate IReactiveComponent Constructor(INotifyValueChanged<TItem> item, ObservableValue<bool> selected);

        public TableCell(Constructor constructor) : base(false) {
            _observableSelected.ValueChangedEvent += SelectSelf;
            _constructedComponent = constructor(_observableItem!, _observableSelected);
            ConstructAndInit();
        }

        public TableCell() { }

        protected override GameObject Construct() {
            return _constructedComponent?.Use(null) ?? base.Construct();
        }

        #endregion

        #region TableCell

        event Action<ITableCell<TItem>, bool>? ITableCell<TItem>.CellAskedToChangeSelectionEvent {
            add => CellAskedToChangeSelectionEvent += value;
            remove => CellAskedToChangeSelectionEvent -= value;
        }

        private event Action<ITableCell<TItem>, bool>? CellAskedToChangeSelectionEvent;
        private ObservableValue<TItem?> _observableItem = new(default);
        private ObservableValue<bool> _observableSelected = new(false);
        private bool _canSelect = true;

        void ITableCell<TItem>.Init(TItem item) {
            _observableItem.Value = item;
            OnInit(item);
        }

        void ITableCell<TItem>.OnCellStateChange(bool selected) {
            _canSelect = false;
            if (_observableSelected.Value != selected) {
                _observableSelected.Value = selected;
            }
            _canSelect = true;
            OnCellStateChange(selected);
        }

        #endregion

        #region Abstraction

        public TItem Item => _observableItem!;
        public bool Selected => _observableSelected!;

        public INotifyValueChanged<TItem> ObservableItem => _observableItem!;
        public INotifyValueChanged<TItem> ObservableSelected => _observableItem!;

        protected virtual void OnInit(TItem item) { }

        protected virtual void OnCellStateChange(bool selected) { }

        protected void SelectSelf(bool select) {
            if (_canSelect) {
                CellAskedToChangeSelectionEvent?.Invoke(this, select);
            }
        }

        #endregion
    }
}