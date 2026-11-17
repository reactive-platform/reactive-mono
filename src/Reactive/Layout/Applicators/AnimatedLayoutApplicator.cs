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
        get => _x.Animation;
        set => _x.Animation = value;
    }

    /// <summary>
    /// The animation of the top offset. Applied instantly if not set.
    /// </summary>
    public LayoutAnimation? PosY {
        get => _y.Animation;
        set => _y.Animation = value;
    }

    /// <summary>
    /// The animation of the width. Applied instantly if not set.
    /// </summary>
    public LayoutAnimation? SizeX {
        get => _width.Animation;
        set => _width.Animation = value;
    }

    /// <summary>
    /// The animation of the height. Applied instantly if not set.
    /// </summary>
    public LayoutAnimation? SizeY {
        get => _height.Animation;
        set => _height.Animation = value;
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

    private Anim _x;
    private Anim _y;
    private Anim _width;
    private Anim _height;
    private int _advancedFrame = -1;

    public override bool Apply(LayoutDimensions current, LayoutDimensions target, out LayoutDimensions dimensions) {
        // A new target can come in a frame that was advanced already, then only the changed values restart
        var frame = Time.frameCount;
        var deltaTime = frame == _advancedFrame ? 0f : Time.deltaTime;
        _advancedFrame = frame;

        dimensions = new(
            _y.Evaluate(current.TopOffset, target.TopOffset, deltaTime),
            _x.Evaluate(current.LeftOffset, target.LeftOffset, deltaTime),
            _width.Evaluate(current.Width, target.Width, deltaTime),
            _height.Evaluate(current.Height, target.Height, deltaTime)
        );

        return _x.Running || _y.Running || _width.Running || _height.Running;
    }

    // TODO: move logic from AnimatedState into a shared struct and put it here
    private struct Anim {
        public LayoutAnimation? Animation;
        public bool Running;

        private float _value;
        private float _from;
        private float _to;
        private float _elapsedTime;
        private float _progress;

        public float Evaluate(float current, float target, float deltaTime) {
            if (Animation is not { } animation) {
                return Snap(target);
            }

            // The same target keeps the animation going, unless the item was moved outside the applicator
            if (Running && Mathf.Approximately(_to, target) && Mathf.Approximately(_value, current)) {
                return Advance(animation, deltaTime);
            }

            if (Mathf.Approximately(current, target)) {
                return Snap(target);
            }

            // A new target starts a new animation from where the item is
            _from = current;
            _to = target;
            _elapsedTime = 0f;
            _progress = 0f;
            Running = true;

            return _value = current;
        }

        private float Advance(LayoutAnimation animation, float deltaTime) {
            _elapsedTime += deltaTime;
            _progress = animation.Timing.Advance(_progress, _elapsedTime, deltaTime);

            // The clock decides when it's finished, the value may pass the target on the way
            if (_progress >= 1f) {
                return Snap(_to);
            }

            // Unclamped, so curves can overshoot
            return _value = Mathf.LerpUnclamped(_from, _to, animation.Curve?.Evaluate(_progress) ?? _progress);
        }

        private float Snap(float value) {
            _to = value;
            Running = false;

            return _value = value;
        }
    }

    #endregion
}
