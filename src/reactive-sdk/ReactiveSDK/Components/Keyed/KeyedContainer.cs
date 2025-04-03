using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reactive.Components {
    [PublicAPI]
    public class KeyedContainer<TKey> : DrivingReactiveComponent {
        #region Setup

        public IReactiveComponent? DummyView {
            get => _dummyView;
            set {
                if (_dummyView != null) {
                    Children.Remove(_dummyView);
                }
                _dummyView = value;
                if (_dummyView != null) {
                    Children.Add(_dummyView);
                }
            }
        }

        public IKeyedControlComponent<TKey>? Control {
            get => _control;
            set {
                if (_control != null) {
                    _control.SelectedKeyChangedEvent -= HandleSelectedKeyChanged;
                }
                _control = value;
                if (_control != null) {
                    _control.SelectedKeyChangedEvent += HandleSelectedKeyChanged;
                }
            }
        }

        public IDictionary<TKey, IReactiveComponent> Items => _items;

        private readonly ObservableDictionary<TKey, IReactiveComponent> _items = new();
        private IKeyedControlComponent<TKey>? _control;
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

        #endregion

        #region Callbacks

        private void HandleSelectedKeyChanged(TKey key) {
            Select(key);
        }

        private void HandleItemRemoved(TKey key, IReactiveComponent component) {
            Children.Remove(component);
            component.Enabled = false;
        }

        private void HandleItemAdded(TKey key, IReactiveComponent component) {
            Children.Add(component);
            component.Enabled = false;

            if (_selectedComponent == null) {
                HandleSelectedKeyChanged(key);
            }
        }


        #endregion
    }
}