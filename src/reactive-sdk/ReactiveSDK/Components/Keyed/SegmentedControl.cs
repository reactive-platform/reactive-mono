using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.Components {
    /// <typeparam name="TKey">An item key</typeparam>
    /// <typeparam name="TParam">A param to be passed with key to provide additional info</typeparam>
    /// <typeparam name="TCell">A cell component</typeparam>
    [PublicAPI]
    public class SegmentedControl<TKey, TParam, TCell> : ReactiveComponent, ILayoutDriver, IKeyedControl<TKey, TParam>
        where TCell : IReactiveComponent, ILayoutItem, IKeyedControlCell<TKey, TParam>, new() {
        #region Driver Adapter

        ICollection<ILayoutItem> ILayoutDriver.Children => _layout.Children;

        ILayoutController? ILayoutDriver.LayoutController {
            get => _layout.LayoutController;
            set => _layout.LayoutController = value;
        }

        #endregion

        #region SegmentedControl

        public IDictionary<TKey, TParam> Items => _items;

        public TKey SelectedKey {
            get => _selectedKey ?? throw new InvalidOperationException("Key cannot be acquired when Items is empty");
            private set {
                if (value!.Equals(_selectedKey)) {
                    return;
                }

                _selectedKey = value;
                SelectedKeyChangedEvent?.Invoke(value);
                NotifyPropertyChanged();
            }
        }

        public event Action<TKey>? SelectedKeyChangedEvent;

        private readonly ReactivePool<TKey, TCell> _cells = new();
        private readonly ObservableDictionary<TKey, TParam> _items = new();

        private Layout _layout = null!;
        private TCell? _selectedCell;
        private TKey? _selectedKey;

        private void SpawnCell(TKey key) {
            var cell = _cells.Spawn(key);

            cell.AsFlexItem(flexGrow: 1f);
            cell.Init(key, Items[key]);
            cell.CellAskedToBeSelectedEvent += HandleCellAskedToBeSelected;

            _layout.Children.Add(cell);

            CellConstructCallback?.Invoke(cell);
            OnCellConstruct(cell);

            if (_selectedCell == null) {
                Select(Items.Keys.First());
            }
        }

        private void DespawnCell(TKey key) {
            if (_selectedKey?.Equals(key) ?? false) {
                _selectedCell!.OnCellStateChange(false);
            }

            var cell = _cells.SpawnedComponents[key];
            cell.CellAskedToBeSelectedEvent -= HandleCellAskedToBeSelected;

            _layout.Children.Remove(cell);
            _cells.Despawn(cell);
        }

        public void Select(TKey key) {
            _selectedCell?.OnCellStateChange(false);
            _selectedCell = _cells.SpawnedComponents[key];
            _selectedCell.OnCellStateChange(true);
            SelectedKey = key;
        }

        #endregion

        #region Setup

        protected override void OnInitialize() {
            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
        }

        protected override GameObject Construct() {
            return new Layout().AsFlexGroup().Bind(ref _layout).Use();
        }

        #endregion

        #region Abstraction

        public Action<TCell>? CellConstructCallback { get; set; }

        protected virtual void OnCellConstruct(TCell cell) { }

        #endregion

        #region Callbacks

        private void HandleCellAskedToBeSelected(TKey key) {
            Select(key);
        }

        private void HandleItemAdded(TKey key, TParam param) {
            SpawnCell(key);
            NotifyPropertyChanged(nameof(Items));
        }

        private void HandleItemRemoved(TKey key, TParam param) {
            DespawnCell(key);
            NotifyPropertyChanged(nameof(Items));
        }

        #endregion
    }
}