using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class TableCell<TItem> : ListCell<TItem>, ITableCell<TItem> {
        public new delegate IReactiveComponent Constructor(IState<TItem> item, State<bool> selected);

        #region Factory

        public TableCell(Constructor constructor) {
            this.constructor = x => constructor(x, _observableSelected);
        }

        public TableCell() { }

        #endregion

        #region Cell

        event Action<ITableCell<TItem>, bool>? ITableCell<TItem>.CellAskedToChangeSelectionEvent {
            add => CellAskedToChangeSelectionEvent += value;
            remove => CellAskedToChangeSelectionEvent -= value;
        }

        private event Action<ITableCell<TItem>, bool>? CellAskedToChangeSelectionEvent;
        private State<bool> _observableSelected = new(false);
        private bool _canSelect = true;

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

        public bool Selected => _observableSelected;
        public IState<bool> ObservableSelected => _observableSelected;

        protected virtual void OnCellStateChange(bool selected) { }

        protected void SelectSelf(bool select) {
            if (_canSelect) {
                CellAskedToChangeSelectionEvent?.Invoke(this, select);
            }
        }

        #endregion
    }
}