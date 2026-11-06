using System;
using Reactive.Compiler;
using UnityEngine;

namespace Reactive;

public partial class ReactiveComponent {
    #region Lerp

    protected static float Lerp(float from, float to, float value) {
        return Mathf.LerpUnclamped(from, to, value);
    }

    protected static Color Lerp(Color from, Color to, float value) {
        return Color.LerpUnclamped(from, to, value);
    }

    protected static Vector2 Lerp(Vector2 from, Vector2 to, float value) {
        return Vector2.LerpUnclamped(from, to, value);
    }

    protected static Vector3 Lerp(Vector3 from, Vector3 to, float value) {
        return Vector3.LerpUnclamped(from, to, value);
    }

    protected static Quaternion Lerp(Quaternion from, Quaternion to, float value) {
        return Quaternion.LerpUnclamped(from, to, value);
    }

    #endregion

    #region Owned Values

    protected static State<TValue> Remember<TValue>(TValue initialValue) {
        return new State<TValue>(initialValue);
    }
    
    [VariadicGen(8, "T{}", "IState<T{}> state{}", "state{}")]
    protected static DerivedState<T1, T> RememberDerived<T1, T>(IState<T1> state1, [NoStateCapture] Func<T1, T> predicate) {
        return StateUtils.RememberDerived(state1, predicate);
    }

    protected AnimationPlayer<T> RememberAnimationPlayer<T>(T target) {
        return StateUtils.RememberAnimationPlayer(this, target);
    }

    protected AnimatedState<Color> RememberAnimated(
        Color initialValue,
        AnimationTiming timing,
        AnimationCurve? curve = null,
        Action<Color>? onFinish = null
    ) {
        return StateUtils.RememberAnimatedColor(this, initialValue, timing, curve, onFinish);
    }

    protected AnimatedState<Vector2> RememberAnimated(
        Vector2 initialValue,
        AnimationTiming timing,
        AnimationCurve? curve = null,
        Action<Vector2>? onFinish = null
    ) {
        return StateUtils.RememberAnimatedVector(this, initialValue, timing, curve, onFinish);
    }

    protected AnimatedState<Vector3> RememberAnimated(
        Vector3 initialValue,
        AnimationTiming timing,
        AnimationCurve? curve = null,
        Action<Vector3>? onFinish = null
    ) {
        return StateUtils.RememberAnimatedVector(this, initialValue, timing, curve, onFinish);
    }

    protected AnimatedState<float> RememberAnimated(
        float initialValue,
        AnimationTiming timing,
        AnimationCurve? curve = null,
        Action<float>? onFinish = null
    ) {
        return StateUtils.RememberAnimatedFloat(this, initialValue, timing, curve, onFinish);
    }

    protected AnimatedState<TValue> RememberAnimated<TValue>(
        TValue initialValue,
        IValueInterpolator<TValue> interpolator,
        AnimationTiming timing,
        AnimationCurve? curve = null,
        Action<TValue>? onFinish = null
    ) {
        return StateUtils.RememberAnimated(this, initialValue, interpolator, timing, curve, onFinish);
    }

    #endregion

    #region Other

    /// <summary>
    /// Returns a canvas transform.
    /// </summary>
    protected RectTransform CanvasTransform => (RectTransform)Canvas!.transform;

    /// <summary>
    /// Creates a reference that can be shared before the value is bound.
    /// </summary>
    /// <param name="value">An initial value, if any.</param>
    protected static Ref<T> Ref<T>(T? value = null) where T : class {
        return new(value);
    }
    
    #endregion
}