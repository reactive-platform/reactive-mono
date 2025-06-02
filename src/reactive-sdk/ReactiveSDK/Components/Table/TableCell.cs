using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class TableCell<TItem> : ReactiveComponent, ITableCell<TItem> {
        public delegate IReactiveComponent Constructor(INotifyValueChanged<TItem> item, ObservableValue<bool> selected);

        #region Factory

        public TableCell(Constructor constructor) : this() {
            _constructor = constructor;
        }

        public TableCell() : base(false) { }

        private Constructor? _constructor;

        protected override GameObject Construct() {
            if (_constructor == null) {
                return base.Construct();
            }

            var component = _constructor(_observableItem!, _observableSelected!);
            return component.Use(null);
        }

        #endregion

        #region TableCell

        event Action<ITableCell<TItem>, bool>? ITableCell<TItem>.CellAskedToChangeSelectionEvent {
            add => CellAskedToChangeSelectionEvent += value;
            remove => CellAskedToChangeSelectionEvent -= value;
        }

        private event Action<ITableCell<TItem>, bool>? CellAskedToChangeSelectionEvent;
        private ObservableValue<TItem>? _observableItem;
        private ObservableValue<bool> _observableSelected = new(false);
        private bool _canSelect = true;

        void ITableCell<TItem>.Init(TItem item) {
            if (!IsInitialized) {
                _observableItem = new(item);
                ConstructAndInit();
            } else {
                _observableItem!.Value = item;
            }
            
            OnInit(item);
        }

        void ITableCell<TItem>.OnCellStateChange(bool selected) {
            _canSelect = false;

            if (_observableSelected.Value != selected) {
                _observableSelected.Value = selected;
            }
            OnCellStateChange(selected);

            _canSelect = true;
        }

        #endregion

        #region Abstraction

        public TItem Item => _observableItem!;
        public bool Selected => _observableSelected;

        public INotifyValueChanged<TItem> ObservableItem => _observableItem!;
        public INotifyValueChanged<bool> ObservableSelected => _observableSelected;

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