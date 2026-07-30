using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reactive.Compiler;
using UnityEngine;

namespace Reactive.Components.Basic {
    public enum SelectionMode {
        None,
        Single,
        Multiple
    }

    [PublicAPI]
    public partial class Table<TItem, TCell> : ReactiveComponent where TCell : class, IReactiveComponent {
        #region Props

        [Required, RawState]
        public ScrollContext ScrollContext {
            get => _scrollContext!;
            set {
                _scrollContextSubscription?.RemoveCallback();

                _scrollArea.ScrollContext = value;
                _scrollContext = value;
                _scrollContextSubscription = _scrollContext.AddCallback(HandleScrollContextUpdated);

                DoInitialUpdate();
            }
        }

        [Required]
        public Func<CellContext<TItem>, TCell> ConstructCell {
            get => _constructCell!;
            set {
                _constructCell = value;
                DoInitialUpdate();
            }
        }

        /// <summary>
        /// A collection of added items.
        /// </summary>
        [Required]
        public IReadOnlyList<TItem> Items {
            get => _items!;
            set {
                // TODO: handle selection
                _items = value;
                DoInitialUpdate();
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

                _selectedItems.AddRange(value);
                RefreshSelected();
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

                RefreshSelected();
            }
        } = SelectionMode.Single;

        public ScrollOrientation ScrollOrientation {
            get => _scrollArea.ScrollOrientation;
            set {
                if (_scrollArea.ScrollOrientation == value) {
                    return;
                }

                _scrollArea.ScrollOrientation = value;

                if (_scrollContext != null) {
                    RefreshCells();
                }
            }
        }

        public Action<IReadOnlyCollection<TItem>>? OnSelectedItemsChanged { get; set; }

        private Func<CellContext<TItem>, TCell>? _constructCell;
        private IReadOnlyList<TItem>? _items;
        private ScrollContext? _scrollContext;
        private StateSubscription? _scrollContextSubscription;
        private bool _needsInitialUpdate = true;

        private void DoInitialUpdate() {
            // Required properties can be initialized in any order, so we call 
            // this method in each setter to perform initialization no matter the order
            if (_needsInitialUpdate && _constructCell != null && _scrollContext != null && _items != null) {
                RefreshCells();
                _needsInitialUpdate = false;
            }
        }

        #endregion

        #region Selection

        private readonly HashSet<TItem> _selectedItems = new();

        private void RefreshSelected() {
            foreach (var pair in _cells) {
                // We only care about visible cells
                if (!pair.Cell.Enabled) {
                    continue;
                }

                pair.Context.Selected = SelectedItems.Contains(pair.Context.Item);
            }

            OnSelectedItemsChanged?.Invoke(SelectedItems);
        }

        #endregion

        #region Dimensions

        private bool _needToRefreshCellSize = true;

        private void PlaceCell(Transform transform, int index) {
            if (ScrollOrientation is ScrollOrientation.Vertical) {
                transform.localPosition = new(0f, -(_cellStartPos + (index + 1) * CellSize));
            } else {
                transform.localPosition = new(-(_cellStartPos + index * CellSize), 0f);
            }
        }

        private void AlignCell(RectTransform transform) {
            if (ScrollOrientation is ScrollOrientation.Vertical) {
                transform.anchorMin = new(0f, 1f);
                transform.anchorMax = new(1f, 1f);
                transform.sizeDelta = new(0f, transform.sizeDelta.y);
                transform.pivot = new(1f, 0f);
            } else {
                transform.anchorMin = new(0f, 0f);
                transform.anchorMax = new(0f, 1f);
                transform.sizeDelta = new(transform.sizeDelta.x, 0f);
                transform.pivot = new(1f, 1f);
            }
        }

        private void RefreshCellSizeIfNeeded() {
            if (!_needToRefreshCellSize || _items is not { Count: > 0 }) {
                return;
            }

            if (_cells.Count == 0) {
                SpawnCell(Items[0], 0);
            }

            var cell = _cells[0].Cell;

            AlignCell(cell.ContentTransform);
            cell.RecalculateLayoutImmediate();

            _cellSize = cell.ContentTransform.rect.size;
            _scrollArea.LineSize = CellSize;

            _needToRefreshCellSize = false;
        }

        #endregion

        #region Cells

        private record struct CellPair(CellContext<TItem> Context, TCell Cell, ScrollOrientation Orientation);

        private float CellSize => ScrollOrientation is ScrollOrientation.Vertical ? _cellSize.y : _cellSize.x;

        private readonly List<CellPair> _cells = new();
        private Vector2 _cellSize;
        private float _cellStartPos;

        private CellPair SpawnCell(TItem item, int index) {
            // Spawn a new cell if there's no cells left in the pool
            var context = new CellContext<TItem>();
            // Init before constructing
            context.Init(item, index, Items.Count);
            context.ValueChangedEvent += HandleCellContextUpdated;

            var cell = ConstructCell(context);
            cell.Use(_scrollContent);

            AlignCell(cell.ContentTransform);

            var pair = new CellPair(context, cell, ScrollOrientation);
            _cells.Add(pair);

            return pair;
        }

        private void RefreshCells() {
            RefreshCellSizeIfNeeded();

            var scrollPos = ScrollContext.ActualScrollPos;

            var startIdx = Mathf.FloorToInt((scrollPos + 0.01f) / CellSize);
            var endIdx = Mathf.CeilToInt((scrollPos + ScrollContext.ViewSize - 0.01f) / CellSize);
            var cellsCount = Mathf.Min(endIdx - startIdx, Items.Count);

            _scrollContent.sizeDelta = new(0f, Items.Count * CellSize);
            _cellStartPos = startIdx * CellSize;

            var spawnedCellsCount = _cells.Count;
            var delta = spawnedCellsCount - cellsCount;

            for (var i = 0; i < cellsCount; i++) {
                var itemIdx = i + startIdx;
                var item = Items[itemIdx];

                CellPair pair;
                if (i > spawnedCellsCount - 1) {
                    // Spawn a new cell if there's no more cells in the pool
                    pair = SpawnCell(item, itemIdx);
                } else {
                    pair = _cells[i];

                    pair.Context.Init(item, itemIdx, Items.Count);
                    pair.Cell.Enabled = true;

                    if (pair.Orientation != ScrollOrientation) {
                        // Align cell if it has different orientation
                        AlignCell(pair.Cell.ContentTransform);

                        pair.Orientation = ScrollOrientation;
                    }
                }

                PlaceCell(pair.Cell.ContentTransform, i);
            }

            // Remaining cells are just disabled
            for (var i = 0; i < delta; i++) {
                _cells[spawnedCellsCount + i - 1].Cell.Enabled = false;
            }
        }

        private void RefreshCellsIfNeeded() {
            var scrollPos = ScrollContext.ActualScrollPos;

            if (scrollPos >= _cellStartPos + CellSize || scrollPos < _cellStartPos) {
                RefreshCells();
            }
        }

        #endregion

        #region Construct

        private RectTransform _scrollContent = null!;
        private ScrollArea _scrollArea = null!;

        protected sealed override GameObject Construct() {
            // ScrollContext is required, but we proxy it via another
            // required property, so it's okay to disable
#pragma warning disable RV102
            return new ScrollArea {
#pragma warning restore RV102
                    FinalizeScroll = FinalizeScroll,
                    ScrollContent = new ReactiveComponent {
                        Name = "Content"
                    }.Bind(ref _scrollContent)
                }
                .Bind(ref _scrollArea)
                .Use();
        }

        #endregion

        #region Callbacks

        private void HandleCellContextUpdated(CellContext<TItem> context) {
            if (context.UpdateType is not CellUpdateType.Selection) {
                return;
            }

            switch (SelectionMode) {
                case SelectionMode.None:
                    _selectedItems.Clear();
                    break;

                case SelectionMode.Single:
                    if (context.Selected) {
                        _selectedItems.Clear();
                        _selectedItems.Add(context.Item);
                    } else {
                        _selectedItems.Remove(context.Item);
                    }
                    break;

                case SelectionMode.Multiple:
                    if (context.Selected) {
                        _selectedItems.Add(context.Item);
                    } else {
                        _selectedItems.Remove(context.Item);
                    }
                    break;
            }

            RefreshSelected();
        }

        private void HandleScrollContextUpdated(ScrollContext context) {
            if (context.UpdateType is ScrollUpdateType.Scroll or ScrollUpdateType.Measurements) {
                RefreshCellsIfNeeded();
            }
        }

        private float FinalizeScroll(ScrollContext context) {
            // Adapting position so there's no semi-visible cells
            return MathUtils.RoundStepped(context.ScrollPos, CellSize);
        }

        #endregion
    }
}