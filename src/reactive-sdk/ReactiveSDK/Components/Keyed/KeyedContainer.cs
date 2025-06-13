using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class KeyedContainer<TKey> : ReactiveComponent, ILayoutDriver {
        #region Driver Adapter

        ICollection<ILayoutItem> ILayoutDriver.Children => _layout.Children;

        ILayoutController? ILayoutDriver.LayoutController {
            get => _layout.LayoutController;
            set => _layout.LayoutController = value;
        }

        #endregion

        #region Setup

        public IReactiveComponent? DummyView {
            get => _dummyView;
            set {
                if (_dummyView != null) {
                    _layout.Children.Remove(_dummyView);
                }
                _dummyView = value;
                if (_dummyView != null) {
                    _layout.Children.Add(_dummyView);
                }
            }
        }

        public IKeyedControl<TKey>? Control {
            get => _control;
            set {
                if (_control != null) {
                    _control.WhenKeySelected -= HandleSelectedKeyChanged;
                }
                _control = value;
                if (_control != null) {
                    _control.WhenKeySelected += HandleSelectedKeyChanged;
                }
            }
        }

        public IDictionary<TKey, IReactiveComponent> Items => _items;

        private readonly ObservableDictionary<TKey, IReactiveComponent> _items = new();
        private Layout _layout = null!;
        private IKeyedControl<TKey>? _control;
        private IReactiveComponent? _selectedComponent;
        private IReactiveComponent? _dummyView;

        public bool Select(TKey? key) {
            if (_selectedComponent != null) {
                _selectedComponent.Enabled = false;
            }
            var validKey = false;
            if (key != null && _items.TryGetValue(key, out var value)) {
                _selectedComponent = value;
                _selectedComponent.Enabled = true;
                validKey = true;
            }
            if (_dummyView != null) {
                _dummyView.Enabled = !validKey;
            }
            return validKey;
        }

        protected override void OnInitialize() {
            _items.ItemAddedEvent += HandleItemAdded;
            _items.ItemRemovedEvent += HandleItemRemoved;
        }

        protected override GameObject Construct() {
            return new Layout().Bind(ref _layout).Use();
        }

        #endregion

        #region Callbacks

        private void HandleSelectedKeyChanged(TKey key) {
            Select(key);
        }

        private void HandleItemRemoved(TKey key, IReactiveComponent component) {
            _layout.Children.Remove(component);
            component.Enabled = false;
        }

        private void HandleItemAdded(TKey key, IReactiveComponent component) {
            _layout.Children.Add(component);
            component.Enabled = false;

            if (_selectedComponent == null) {
                HandleSelectedKeyChanged(key);
            }
        }

        #endregion
    }
}