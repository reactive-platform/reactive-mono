using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public static class ComponentExtensions {
    #region Button
    
    public static T WithAccentColor<T>(
        this T button,
        Color color
    ) where T : ColoredButton {
        button.Colors = new SimpleColorSet {
            NotInteractableColor = color.ColorWithAlpha(0.25f),
            HoveredColor = color.ColorWithAlpha(0.7f),
            Color = color.ColorWithAlpha(0.4f),
        };
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
}