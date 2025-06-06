using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    /// <summary>
    /// A <see cref="ListView{LItem,LCell}"/> overload with an ability to make cells in-place.
    /// </summary>
    [PublicAPI]
    public class ListView<TItem> : ListView<TItem, ListCell<TItem>> {
        /// <summary>
        /// Defines a cell constructor. Must be specified.
        /// </summary>
        public ListCell<TItem>.Constructor? ConstructCell { get; set; }

        protected override void OnInstantiate() {
            cellsPool.Construct = CreateCell;
        }

        private ListCell<TItem> CreateCell() {
            if (ConstructCell == null) {
                throw new UninitializedComponentException("The ConstructCell property must be specified");
            }

            return new ListCell<TItem>(ConstructCell);
        }
    }

    /// <summary>
    /// A kind of Table which spawns cells directly in the layout flow.
    /// </summary>
    [PublicAPI]
    public class ListView<TItem, TCell> : ReactiveComponent, ILayoutDriver where TCell : IListCell<TItem>, IReactiveComponent, new() {
        #region Layout Driver

        // Avoid using collection expression as it create a new instance of List each time.
        ICollection<ILayoutItem> ILayoutDriver.Children => Array.Empty<ILayoutItem>();

        public ILayoutController? LayoutController {
            get => _container.LayoutController;
            set => _container.LayoutController = value;
        }

        #endregion

        #region ListView

        /// <summary>
        /// A collection of added items.
        /// </summary>
        public IReadOnlyList<TItem> Items {
            get => _items;
            set {
                _items = value;
                Refresh();
            }
        }

        private IReadOnlyList<TItem> _items = new List<TItem>();

        public void Refresh() {
            RefreshCells();
            OnRefresh();
            RefreshedCb?.Invoke(this);
        }

        #endregion

        #region Cells

        internal readonly ReactivePool<TCell> cellsPool = new() { DetachOnDespawn = true };
        private Layout _container = null!;

        private void RefreshCells() {
            cellsPool.DespawnAll();

            foreach (var item in _items) {
                var cell = cellsPool.Spawn(false);
                cell.Init(item);
                cell.Enabled = true;
                
                OnCellConstruct(cell);
                CellConstructedCb?.Invoke(cell);
                
                _container.Children.Add(cell);
            }
        }

        #endregion

        #region Abstraction

        public Action<ListView<TItem, TCell>>? RefreshedCb;
        public Action<TCell>? CellConstructedCb;

        protected virtual void OnRefresh() { }
        protected virtual void OnCellConstruct(TCell cell) { }

        #endregion

        #region Construct

        protected sealed override GameObject Construct() {
            return new Layout()
                .AsFlexGroup(direction: Yoga.FlexDirection.Column)
                .Bind(ref _container)
                .Use();
        }

        #endregion
    }
}