using System;
using System.Collections.Generic;
using System.Linq;
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

        [Required]
        public Func<CellContext<TItem>, TCell> ConstructCell {
            get;
            init {
                field = value;
                _constructCellSet = true;

                DoInitialUpdate();
            }
        }

        /// <summary>
        /// A collection of added items.
        /// </summary>
        [Required]
        public IReadOnlyList<TItem> Items {
            get;
            set {
                field = value;
                _itemsSet = true;

                if (_initialized) {
                    RefreshCells();
                } else {
                    DoInitialUpdate();
                }
            }
        }

        /// <summary>
        /// A collection of selected items.
        /// </summary>
        public IReadOnlyCollection<TItem> SelectedItems {
            get => _selectedItems;
            set {
                switch (SelectionMode) {
                    case SelectionMode.None when value.Count is not 0:
                        throw new InvalidOperationException("Cannot set a non-empty selected cells list when SelectionMode is None");

                    case SelectionMode.Single when value.Count > 1:
                        throw new InvalidOperationException("Cannot set a selected cells list with len > 1 when SelectionMode is Single");
                }

                _selectedItems.Clear();
                _selectedItems.AddRange(value);

                // Refresh only if called after Items and ConstructCell
                if (_itemsSet && _constructCellSet) {
                    RefreshSelection();

                    OnSelectedItemsChanged?.Invoke(_selectedItems);
                }
            }
        }

        /// <summary>
        /// An enum that determines how many cells you can select.
        /// </summary>
        public SelectionMode SelectionMode {
            get;
            set {
                if (value == field) {
                    return;
                }

                field = value;

                switch (value) {
                    case SelectionMode.None:
                        _selectedItems.Clear();
                        break;

                    case SelectionMode.Single when _selectedItems.Count > 1:
                        var any = _selectedItems.First();

                        _selectedItems.Clear();
                        _selectedItems.Add(any);
                        break;
                }

                if (_itemsSet && _constructCellSet) {
                    RefreshSelection();
                }
            }
        } = SelectionMode.Single;

        public Action<IReadOnlyCollection<TItem>>? OnSelectedItemsChanged { get; set; }

        private bool _constructCellSet;
        private bool _itemsSet;
        private bool _initialized;

        private void DoInitialUpdate() {
            if (!_initialized && _itemsSet && _constructCellSet) {
                RefreshCells();
                _initialized = true;
            }
        }

        #endregion

        #region Selection

        private readonly HashSet<TItem> _selectedItems = new();
        private bool _blockContextUpdates;

        private void RefreshSelection() {
            foreach (var pair in _cells) {
                // We only care about visible cells
                if (!pair.Cell.Enabled) {
                    return;
                }

                pair.Context.Selected = _selectedItems.Contains(pair.Context.Item);
            }
        }

        private void HandleCellContextUpdated(CellContext<TItem> context) {
            if (context.UpdateType is not CellUpdateType.Selection || _blockContextUpdates) {
                return;
            }

            _blockContextUpdates = true;

            if (context.Selected) {
                switch (SelectionMode) {
                    case SelectionMode.None:
                        return;

                    case SelectionMode.Single when Items.Count > 0:
                        _selectedItems.Clear();
                        _selectedItems.Add(context.Item);
                        break;

                    case SelectionMode.Multiple:
                        _selectedItems.Add(context.Item);
                        break;
                }
            } else {
                _selectedItems.Remove(context.Item);
            }

            RefreshSelection();
            OnSelectedItemsChanged?.Invoke(_selectedItems);

            _blockContextUpdates = false;
        }

        #endregion

        #region Cells

        private readonly List<(CellContext<TItem> Context, TCell Cell)> _cells = new();

        private Layout _container = null!;

        private void RefreshCells() {
            _blockContextUpdates = true;

            for (var i = 0; i < Items!.Count; i++) {
                var item = Items[i];

                CellContext<TItem> context;
                TCell cell;

                if (i >= _cells.Count) {
                    context = new CellContext<TItem>();
                    context.ValueChangedEvent += HandleCellContextUpdated;

                    context.Init(item, i, Items.Count);

                    cell = ConstructCell(context);

                    _cells.Add((context, cell));
                    _container.Children.Add(cell);
                } else {
                    (context, cell) = _cells[^1];

                    context.Init(item, i, Items.Count);
                    context.Selected = _selectedItems.Contains(item);

                    cell.Enabled = true;
                }
            }

            // Disable remaining cells if there's any
            for (var i = Items.Count; i < _cells.Count; i++) {
                _cells[i].Item2.Enabled = false;
            }

            _blockContextUpdates = false;
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