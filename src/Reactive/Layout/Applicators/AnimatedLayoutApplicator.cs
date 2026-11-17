using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// Animates the values of an item from its current dimensions to the calculated ones.
/// </summary>
/// <example>
/// <code>
/// new AnimatedLayoutApplicator {
///     All = 10.fact,
///     PosY = new(300.ms, AnimationCurve.EaseOutBack)
/// }
///
/// new AnimatedLayoutApplicator {
///     [LayoutAxis.Size] = 10.fact,
///     [LayoutAxis.Pos] = 200.ms
/// }
/// </code>
/// </example>
[PublicAPI]
public sealed class AnimatedLayoutApplicator : LayoutApplicator {
    #region Public API

    /// <summary>
    /// The animation of the left offset. Applied instantly if not set.
    /// </summary>
    public LayoutAnimation? PosX {
        get;
        set => SetAnimationValues(ref _x, field = value);
    }

    /// <summary>
    /// The animation of the top offset. Applied instantly if not set.
    /// </summary>
    public LayoutAnimation? PosY {
        get;
        set => SetAnimationValues(ref _y, field = value);
    }

    /// <summary>
    /// The animation of the width. Applied instantly if not set.
    /// </summary>
    public LayoutAnimation? SizeX {
        get;
        set => SetAnimationValues(ref _width, field = value);
    }

    /// <summary>
    /// The animation of the height. Applied instantly if not set.
    /// </summary>
    public LayoutAnimation? SizeY {
        get;
        set => SetAnimationValues(ref _height, field = value);
    }

    /// <summary>
    /// Sets <see cref="PosX"/> and <see cref="PosY"/>.
    /// </summary>
    public LayoutAnimation? Pos {
        set => PosX = PosY = value;
    }

    /// <summary>
    /// Sets <see cref="SizeX"/> and <see cref="SizeY"/>.
    /// </summary>
    public LayoutAnimation? Size {
        set => SizeX = SizeY = value;
    }

    /// <summary>
    /// Sets <see cref="PosX"/> and <see cref="SizeX"/>.
    /// </summary>
    public LayoutAnimation? X {
        set => PosX = SizeX = value;
    }

    /// <summary>
    /// Sets <see cref="PosY"/> and <see cref="SizeY"/>.
    /// </summary>
    public LayoutAnimation? Y {
        set => PosY = SizeY = value;
    }

    /// <summary>
    /// Sets all values.
    /// </summary>
    public LayoutAnimation? All {
        set => Pos = Size = value;
    }

    /// <summary>
    /// The animation of the masked values. Setting it sets every masked value, getting it returns the first one
    /// in the <see cref="PosX"/>, <see cref="PosY"/>, <see cref="SizeX"/>, <see cref="SizeY"/> order.
    /// </summary>
    /// <param name="axes">The values to get or set.</param>
    public LayoutAnimation? this[LayoutAxis axes] {
        get {
            if ((axes & LayoutAxis.PosX) != 0) return PosX;
            if ((axes & LayoutAxis.PosY) != 0) return PosY;
            if ((axes & LayoutAxis.SizeX) != 0) return SizeX;
            if ((axes & LayoutAxis.SizeY) != 0) return SizeY;

            return null;
        }
        set {
            if ((axes & LayoutAxis.PosX) != 0) PosX = value;
            if ((axes & LayoutAxis.PosY) != 0) PosY = value;
            if ((axes & LayoutAxis.SizeX) != 0) SizeX = value;
            if ((axes & LayoutAxis.SizeY) != 0) SizeY = value;
        }
    }

    #endregion

    #region Apply

    private AnimatedValue<float> _x = new() { Interpolator = SingleValueInterpolator.Instance };
    private AnimatedValue<float> _y = new() { Interpolator = SingleValueInterpolator.Instance };
    private AnimatedValue<float> _width = new() { Interpolator = SingleValueInterpolator.Instance };
    private AnimatedValue<float> _height = new() { Interpolator = SingleValueInterpolator.Instance };
    private LayoutDimensions _target;
    private int _advancedFrame = -1;

    public override bool Apply(LayoutDimensions current, LayoutDimensions target, out LayoutDimensions dimensions) {
        if (_target != target) {
            _target = target;

            Retarget(ref _y, PosY != null, current.TopOffset, target.TopOffset);
            Retarget(ref _x, PosX != null, current.LeftOffset, target.LeftOffset);
            Retarget(ref _width, SizeX != null, current.Width, target.Width);
            Retarget(ref _height, SizeY != null, current.Height, target.Height);
        }

        // A new target can come in a frame that was advanced already
        var frame = Time.frameCount;
        var deltaTime = frame == _advancedFrame ? 0f : Time.deltaTime;
        _advancedFrame = frame;

        _x.Advance(deltaTime);
        _y.Advance(deltaTime);
        _width.Advance(deltaTime);
        _height.Advance(deltaTime);

        dimensions = new(_y.Value, _x.Value, _width.Value, _height.Value);

        return _x.IsRunning || _y.IsRunning || _width.IsRunning || _height.IsRunning;
    }

    private static void Retarget(ref AnimatedValue<float> value, bool animated, float current, float target) {
        if (!animated) {
            value.Set(target);
            return;
        }

        if (!value.IsRunning) {
            value.Set(current);
        }

        value.Retarget(target);
    }

    private static void SetAnimationValues(ref AnimatedValue<float> value, LayoutAnimation? animation) {
        value.Timing = animation?.Timing ?? default;
        value.Curve = animation?.Curve;
    }

    #endregion
}