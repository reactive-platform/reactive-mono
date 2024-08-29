using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    /// <summary>
    /// A set designed specifically for the reactive pattern.
    /// </summary>
    [PublicAPI]
    public class ReactiveColorSet : IColorSet {
        public Color Color {
            get => _color;
            set {
                _color = value;
                SetUpdatedEvent?.Invoke();
            }
        }
        
        public Action<GraphicState>? OnGraphicChanged { get; set; }

        public event Action? SetUpdatedEvent;
        
        private Color _color;

        public Color GetColor(GraphicState state) {
            OnGraphicChanged?.Invoke(state);
            return _color;
        }
    }
}