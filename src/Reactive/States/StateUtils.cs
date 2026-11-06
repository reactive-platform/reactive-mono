using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reactive.Compiler;
using UnityEngine;

namespace Reactive {
    [PublicAPI]
    public static partial class StateUtils {
        #region Owned

        public static State<T> Remember<T>(T initialValue) {
            return new State<T>(initialValue);
        }
        
        [VariadicGen(8, "T{}", "IState<T{}> state{}", "state{}")]
        public static DerivedState<T1, T> RememberDerived<T1, T>(IState<T1> state1, [NoStateCapture] Func<T1, T> predicate) {
            return new(state1, predicate);
        }

        public static AnimationPlayer<T> RememberAnimationPlayer<T>(IReactiveModuleBinder binder, T target) {
            var player = new AnimationPlayer<T>(target);
            binder.BindModule(player);
            return player;
        }

        public static AnimatedState<T> RememberAnimated<T>(
            IReactiveModuleBinder binder,
            T initialValue,
            IValueInterpolator<T> interpolator,
            AnimationTiming timing,
            AnimationCurve? curve = null,
            Action<T>? onFinish = null
        ) {
            var value = new AnimatedState<T>(initialValue, interpolator) {
                Timing = timing,
                OnFinish = onFinish,
                Curve = curve ?? AnimationCurve.Linear
            };
            binder.BindModule(value);
            return value;
        }

        public static AnimatedState<Color> RememberAnimatedColor(
            IReactiveModuleBinder binder,
            Color initialValue,
            AnimationTiming timing,
            AnimationCurve? curve = null,
            Action<Color>? onFinish = null
        ) {
            return RememberAnimated(
                binder,
                initialValue,
                ColorValueInterpolator.Instance,
                timing,
                curve,
                onFinish
            );
        }

        public static AnimatedState<Vector2> RememberAnimatedVector(
            IReactiveModuleBinder binder,
            Vector2 initialValue,
            AnimationTiming timing,
            AnimationCurve? curve = null,
            Action<Vector2>? onFinish = null
        ) {
            return RememberAnimated(
                binder,
                initialValue,
                Vector2ValueInterpolator.Instance,
                timing,
                curve,
                onFinish
            );
        }

        public static AnimatedState<Vector3> RememberAnimatedVector(
            IReactiveModuleBinder binder,
            Vector3 initialValue,
            AnimationTiming timing,
            AnimationCurve? curve = null,
            Action<Vector3>? onFinish = null
        ) {
            return RememberAnimated(
                binder,
                initialValue,
                Vector3ValueInterpolator.Instance,
                timing,
                curve,
                onFinish
            );
        }

        public static AnimatedState<float> RememberAnimatedFloat(
            IReactiveModuleBinder binder,
            float initialValue,
            AnimationTiming timing,
            AnimationCurve? curve = null,
            Action<float>? onFinish = null
        ) {
            return RememberAnimated(
                binder,
                initialValue,
                SingleValueInterpolator.Instance,
                timing,
                curve,
                onFinish
            );
        }

        #endregion
    }
}