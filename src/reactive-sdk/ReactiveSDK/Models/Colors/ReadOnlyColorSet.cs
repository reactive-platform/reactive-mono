using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public struct ReadOnlyColorSet : IColorSet {
        public Color DisabledColor;
        public Color Color;
        public Color ActiveColor;
        public Color HoveredColor;
        public Color? HoveredActiveColor;

        public event Action? SetUpdatedEvent;
        
        public Color GetColor(GraphicState state) {
            if (state.Hovered) {
                return state.Active ? HoveredActiveColor.GetValueOrDefault(ActiveColor) : HoveredColor;
            }
            if (state.Active) {
                return ActiveColor;
            }
            if (!state.Interactable) {
                return DisabledColor;
            }
            return Color;
        }
    }
}