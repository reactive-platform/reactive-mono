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
                    
                    if (_keyEverSet && Key == null) {
                        _selectedView = field;
                        _selectedView.Enabled = true;
                    }
                }
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

                    // If a view was selected but isn't presented in the new collection, we fall back to the DummyView
                    if (_keyEverSet && (Key == null || !value.ContainsKey(Key))) {
                        _selectedView = DummyView;
                        _selectedView?.Enabled = true;
                    }
                }

                _views = value;

                foreach (var comp in value.Values) {
                    _layout.Children.Add(comp);
                    comp.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Specifies the key that determines the current view.
        /// </summary>
        [Required]
        public TKey? Key {
            get;
            set {
                field = value;
                _keyEverSet = true;

                _selectedView?.Enabled = false;

                if (value == null) {
                    _selectedView = DummyView;
                } else if (_views?.TryGetValue(value, out var view) ?? false) {
                    _selectedView = view;
                } else if (_views != null) {
                    throw new KeyNotFoundException();
                }

                _selectedView?.Enabled = true;

                OnKeyChanged?.Invoke(value);
            }
        }
        
        public Action<TKey?>? OnKeyChanged { get; set; }

        private IReadOnlyDictionary<TKey, IReactiveComponent>? _views;
        private IReactiveComponent? _selectedView;
        private bool _keyEverSet;
        
        private Layout _layout = null!;

        protected override GameObject Construct() {
            return new Layout().Bind(ref _layout).Use();
        }

        #endregion
    }
}