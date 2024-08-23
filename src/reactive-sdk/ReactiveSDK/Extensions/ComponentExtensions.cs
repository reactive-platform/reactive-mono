using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public static class ComponentExtensions {
    #region Button

    public static T WithClickListener<T>(this T button, Action listener) where T : IClickableComponent {
        button.ClickEvent += listener;
        return button;
    }

    public static T WithStateListener<T>(this T button, Action<bool> listener) where T : ButtonBase {
        button.WithListener(static x => x.Active, listener);
        return button;
    }

    public static T WithColors<T>(
        this T comp,
        Optional<Color> color = default,
        Optional<Color> hoveredColor = default,
        Optional<Color> disabledColor = default,
        Optional<Color> activeColor = default,
        Optional<AnimationDuration> duration = default
    ) where T : ButtonBase, IColoredComponent {
        if (color.HasValue) {
            comp.WithColorTransition(color, ComponentState.Default, duration);
        }
        if (hoveredColor.HasValue) {
            comp.WithColorTransition(hoveredColor, ComponentState.Focused, duration);
        }
        if (disabledColor.HasValue) {
            comp.WithColorTransition(disabledColor, ButtonBase.NonInteractableState, duration);
        }
        if (activeColor.HasValue) {
            comp.WithColorTransition(activeColor, ButtonBase.ActiveState, duration);
        }
        return comp;
    }

    public static T WithAccentColor<T>(
        this T button,
        Color color
    ) where T : ButtonBase, IColoredComponent {
        button.WithColors(
            color: color.ColorWithAlpha(0.4f),
            disabledColor: color.ColorWithAlpha(0.25f),
            hoveredColor: color.ColorWithAlpha(0.7f)
        );
        return button;
    }

    #endregion

    #region Other

    [Pure]
    public static T In<T>(this ILayoutItem comp) where T : DrivingReactiveComponent, new() {
        return new T {
            Children = { comp.WithRectExpand() }
        };
    }

    #endregion

    #region Colored

    public static T WithColorTransitions<T>(
        this T comp,
        Optional<Color> color = default,
        Optional<Color> hoveredColor = default,
        Optional<Color> pressedColor = default,
        Optional<AnimationDuration> duration = default
    ) where T : IColoredComponent, IStateAnimationHost {
        if (color.HasValue) {
            comp.WithColorTransition(color, ComponentState.Default, duration);
        }
        if (hoveredColor.HasValue) {
            comp.WithColorTransition(hoveredColor, ComponentState.Hovered, duration);
        }
        if (pressedColor.HasValue) {
            comp.WithColorTransition(pressedColor, ComponentState.Pressed, duration);
        }
        return comp;
    }

    public static T WithColorTransition<T>(
        this T comp,
        Color color,
        ComponentState state,
        Optional<AnimationDuration> duration = default
    ) where T : IColoredComponent, IStateAnimationHost {
        return comp.WithTransition(
            state,
            static x => x.Color,
            color,
            duration.GetValueOrDefault(ComponentConstants.TransitionDuration)
        );
    }

    public static T WithTransition<T>(
        this T host,
        ComponentState state,
        IAnimation transition
    ) where T : IStateAnimationHost {
        host.AddStateTransition(state, transition);
        return host;
    }

    #endregion
}