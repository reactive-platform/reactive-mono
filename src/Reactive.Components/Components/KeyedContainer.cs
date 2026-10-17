using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reactive.Compiler;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public partial class KeyedContainer<TKey> : ReactiveComponent, ILayoutDriver {
        #region Driver Adapter

        ICollection<ILayoutItem> ILayoutDriver.Children => _layout.Children;

        ILayoutController? ILayoutDriver.LayoutController {
            get => _layout.LayoutController;
            set => _layout.LayoutController = value;
        }

        #endregion

        #region Setup

        public IReactiveComponent? DummyView {
            get;
            set {
                if (field != null) {
                    _layout.Children.Remove(field);
                }

                field = value;

                if (field != null) {
                    _layout.Children.Add(field);
                    field.Enabled = false;
                }

                RefreshSelectedView();
            }
        }

        /// <summary>
        /// Sets a collection of key-view pairs. Fallbacks to DummyView if an already selected key is missing in the updated collection.
        /// </summary>
        [Required]
        public IReadOnlyDictionary<TKey, IReactiveComponent> Views {
            get => _views!;
            set {
                if (_views != null) {
                    // Items that aren't presented in the new collection are considered removed
                    foreach (var (key, comp) in _views) {
                        if (value.ContainsKey(key)) {
                            continue;
                        }
                        
                        _layout.Children.Remove(comp);
                        comp.Enabled = false;
                    }
                }

                _views = value;

                foreach (var comp in value.Values) {
                    _layout.Children.Add(comp);
                    comp.Enabled = false;
                }

                // Re-selects the view for the current key or falls back to the DummyView if the key is missing
                _selectedView = null;
                RefreshSelectedView();
            }
        }

        /// <summary>
        /// Specifies the key that determines the current view.
        /// </summary>
        [Required]
        public TKey? Key {
            get;
            set {
                if (value != null && _views != null && !_views.ContainsKey(value)) {
                    throw new KeyNotFoundException();
                }

                field = value;
                _keyEverSet = true;

                RefreshSelectedView();

                OnKeyChanged?.Invoke(value);
            }
        }
        
        public Action<TKey?>? OnKeyChanged { get; set; }

        private IReadOnlyDictionary<TKey, IReactiveComponent>? _views;
        private IReactiveComponent? _selectedView;
        private bool _keyEverSet;
        
        private Layout _layout = null!;

        private void RefreshSelectedView() {
            if (!_keyEverSet) {
                return;
            }

            if (Key == null || _views == null || !_views.TryGetValue(Key, out var view)) {
                view = DummyView;
            }

            if (_selectedView != view) {
                _selectedView?.Enabled = false;
            }

            _selectedView = view;
            _selectedView?.Enabled = true;
        }

        protected override GameObject Construct() {
            return new Layout().Bind(ref _layout).Use();
        }

        #endregion
    }
}