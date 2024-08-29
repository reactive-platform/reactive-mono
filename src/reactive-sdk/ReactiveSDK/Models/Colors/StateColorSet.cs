using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    /// <summary>
    /// A standard color set.
    /// </summary>
    [PublicAPI]
    public class StateColorSet : IColorSet {
        public Color ActiveColor {
            get => _activeColor;
            set {
                _activeColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public Color HoveredColor {
            get => _hoveredColor;
            set {
                _hoveredColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public Color DisabledColor {
            get => _disabledColor;
            set {
                _disabledColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public Color Color {
            get => _color;
            set {
                _color = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        public Color? HoveredActiveColor {
            get => _hoveredActiveColor;
            set {
                _hoveredActiveColor = value;
                SetUpdatedEvent?.Invoke();
            }
        }

        private Color _disabledColor;
        private Color _activeColor;
        private Color _hoveredColor;
        private Color _color;
        private Color? _hoveredActiveColor;

        public event Action? SetUpdatedEvent;

        public Color GetColor(GraphicState state) {
            if (state.Hovered) {
                return state.Active ? _hoveredActiveColor.GetValueOrDefault(ActiveColor) : _hoveredColor;
            }
            if (state.Active) {
                return _activeColor;
            }
            if (!state.Interactable) {
                return _disabledColor;
            }
            return _color;
        }
    }
}