using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class CustomColorSet : IColorSet {
        public Func<GraphicState, Color>? OnGraphicChanged { get; set; }

        public event Action? SetUpdatedEvent;

        public Color GetColor(GraphicState state) {
            return OnGraphicChanged?.Invoke(state) ?? Color.clear;
        }

        public void NotifyColorUpdated() {
            SetUpdatedEvent?.Invoke();
        }
    }
}