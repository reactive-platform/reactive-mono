using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components.Basic {

    /// <summary>
    /// A <see cref="ListView{LItem,LCell}"/> overload with an ability to make cells in-place.
    /// </summary>
    [PublicAPI]
    public class ListView<LItem> : ListView<LItem, ListViewCell<LItem>> {
        /// <summary>
        /// Defines a cell constructor. Must be specified.
        /// </summary>
        public ListViewCell<LItem>.Constructor? ConstructCell { get; set; }

        protected override void OnInstantiate() {
            cellsPool.Construct = CreateCell;
        }

        private ListViewCell<LItem> CreateCell() {
            if (ConstructCell == null) {
                throw new UninitializedComponentException("The ConstructCell property must be specified");
            }
            
            return new ListViewCell<LItem>(ConstructCell);
        }
    }

    [PublicAPI]
    public class ListView<LItem, LCell> : ReactiveComponent, IListView<LItem> where LCell : IListViewCell<LItem>, IReactiveComponent, new() {

        #region Table

        /// <summary>
        /// A collection of added items.
        /// </summary>
        public IReadOnlyList<LItem> Items {
            get {
                lock (_itemsLocker) {
                    return _items;
                }
            }
            set {
                lock (_itemsLocker) {
                    _items = value;
                    Refresh();
                }
            }
        }

        private readonly object _itemsLocker = new();
        private IReadOnlyList<LItem> _items = new List<LItem>();

        public void Refresh() {
            RefreshCells();
            OnRefresh();
        }

        #endregion

        #region Cells

        private Layout _container = null!;
        internal readonly ReactivePool<LCell> cellsPool = new() { DetachOnDespawn = true };

        private void RefreshCells() {
            cellsPool.DespawnAll();

            for (int i = 0; i < _items.Count; i++) {
                //spawning and initializing
                var item = _items[i];
                var cell = cellsPool.Spawn();
                cell.Init(item);
                OnCellConstruct(cell);
                _container.Children.Add(cell);
            }
        }

        #endregion

        #region Abstraction
        protected virtual void OnRefresh() { }
        protected virtual void OnCellConstruct(LCell cell) { }

        #endregion

        #region Construct

        protected sealed override GameObject Construct() {
            //constructing
            var content = new Layout {
            }
            .AsFlexGroup(direction: Yoga.FlexDirection.Column, constrainVertical: false)
            .Bind(ref _container);

            return content.Use();
        }

        #endregion
    }
}