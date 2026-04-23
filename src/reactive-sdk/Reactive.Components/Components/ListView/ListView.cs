using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components.Basic;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.Components {
    /// <summary>
    /// A component that spawns physical cells directly in the layout flow.
    /// </summary>
    [PublicAPI]
    public partial class ListView<TItem, TCell> : ReactiveComponent, ILayoutDriver where TCell : class, IReactiveComponent {
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
                RefreshCells();
            }
        }

        [Required]
        public Func<IState<TItem>, TCell> ConstructCell { get; set; }

        private IReadOnlyList<TItem> _items = Array.Empty<TItem>();

        protected virtual void OnRefreshInternal() { }
        protected virtual void OnCellInitInternal(TCell cell) { }

        #endregion

        #region Cells

        private readonly List<(State<TItem>, TCell)> _cells = new();
        private Layout _container = null!;

        private void RefreshCells() {
            for (var i = 0; i < _items.Count; i++) {
                var item = _items[i];

                State<TItem> state;
                TCell cell;

                if (i >= _cells.Count) {
                    state = new State<TItem>(item);
                    cell = ConstructCell(state);

                    _cells.Add((state, cell));
                    _container.Children.Add(cell);
                } else {
                    (state, cell) = _cells[^1];

                    state.Value = item;
                    cell.Enabled = true;
                }

                OnCellInitInternal(cell);
            }

            // Disable remaining cells if there's any
            for (var i = _items.Count; i < _cells.Count; i++) {
                _cells[i].Item2.Enabled = false;
            }

            OnRefreshInternal();
        }

        #endregion

        #region Construct

        protected sealed override GameObject Construct() {
            return new Layout()
                .AsFlexGroup(direction: FlexDirection.Column)
                .Bind(ref _container)
                .Use();
        }

        #endregion
    }
}