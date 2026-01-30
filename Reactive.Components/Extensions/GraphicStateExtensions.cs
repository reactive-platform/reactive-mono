using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public static class GraphicStateExtensions {
    public static KeyValuePair<GraphicState, Color> WithColor(this GraphicState state, Color color) {
        return new KeyValuePair<GraphicState, Color>(state, color);
    }

    public static GraphicState AddIf(this GraphicState state, GraphicState add, bool value) {
        if (value) {
            state |= add;
        }
        return state;
    }

    public static GraphicState And(this GraphicState state, GraphicState add) {
        return state | add;
    }

    public static bool IsInteractable(this GraphicState state) {
        return (state & GraphicState.NonInteractable) == 0;
    }

    public static bool IsHovered(this GraphicState state) {
        return (state & GraphicState.Hovered) > 0;
    }

    public static bool IsActive(this GraphicState state) {
        return (state & GraphicState.Active) > 0;
    }

    public static bool IsPressed(this GraphicState state) {
        return (state & GraphicState.Pressed) > 0;
    }
}