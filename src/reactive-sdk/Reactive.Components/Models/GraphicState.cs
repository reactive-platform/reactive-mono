using System;
using JetBrains.Annotations;
using static Reactive.Components.GraphicState;

namespace Reactive.Components;

[Flags]
public enum GraphicState {
    None = 0,
    Hovered = 1,
    Active = 2,
    NonInteractable = 4,
}

[PublicAPI]
public static class GraphicStateExtensions {
    extension(in GraphicState state) {
        public bool IsInteractable {
            get => (state & NonInteractable) == 0;
        }

        public bool IsHovered {
            get => (state & Hovered) > 0;
        }

        public bool IsActive {
            get => (state & Active) > 0;
        }

        public GraphicState AddIf(GraphicState add, bool value) {
            return value ? state | add : state;
        }

        public GraphicState Set(GraphicState mask, bool value) {
            return value ? state | mask : state & ~mask;
        }

        public GraphicState And(GraphicState add) {
            return state | add;
        }
    }

    extension(IMutableState<GraphicState> state) {
        public bool IsInteractable {
            get => state.Value.IsInteractable;
            set => state.Value = state.Value.Set(NonInteractable, !value);
        }

        public bool IsHovered {
            get => state.Value.IsHovered;
            set => state.Value = state.Value.Set(Hovered, value);
        }

        public bool IsActive {
            get => state.Value.IsActive;
            set => state.Value = state.Value.Set(Active, value);
        }
    }
}