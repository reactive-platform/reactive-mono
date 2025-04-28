using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public static class ModalExtensions {
        #region Events

        public static T WithCloseListener<T>(this T comp, Action callback) where T : IModal {
            comp.ModalClosedEvent += (_, _) => callback();
            return comp;
        }

        public static T WithCloseListener<T>(this T comp, Action<IModal, bool> callback) where T : IModal {
            comp.ModalClosedEvent += callback;
            return comp;
        }

        public static T WithOpenListener<T>(this T comp, Action callback) where T : IModal {
            comp.ModalOpenedEvent += (_, _) => callback();
            return comp;
        }

        public static T WithOpenListener<T>(this T comp, Action<IModal, bool> callback) where T : IModal {
            comp.ModalOpenedEvent += callback;
            return comp;
        }

        #endregion

        #region Scale Animation

        public static T WithScaleAnimation<T>(
            this T modal,
            Optional<AnimationDuration> duration = default,
            AnimationCurve? curve = null
        ) where T : ModalBase {
            WithScaleOpenAnimation(modal, duration, curve);
            WithScaleCloseAnimation(modal, duration, curve);
            return modal;
        }

        public static T WithScaleOpenAnimation<T>(
            this T modal,
            Optional<AnimationDuration> duration = default,
            AnimationCurve? curve = null
        ) where T : ModalBase {
            var scale = ValueUtils.AnimatedFloat(0f, duration.GetValueOrDefault(200.ms()), curve);

            modal.OpenAnimation = AnimationUtils.Animation(
                () => {
                    scale.SetValueImmediate(modal.ContentTransform.localScale.x);
                    scale.Value = 1f;
                },
                [scale]
            );

            modal.Animate(
                scale,
                static (x, y) => {
                    x.ContentTransform.localScale = y * Vector3.one;
                }
            );

            return modal;
        }

        public static T WithScaleCloseAnimation<T>(
            this T modal,
            Optional<AnimationDuration> duration = default,
            AnimationCurve? curve = null
        ) where T : ModalBase {
            var scale = ValueUtils.AnimatedFloat(1f, duration.GetValueOrDefault(200.ms()), curve);

            modal.OpenAnimation = AnimationUtils.Animation(
                () => {
                    scale.SetValueImmediate(modal.ContentTransform.localScale.x);
                    scale.Value = 0f;
                },
                [scale]
            );

            modal.Animate(
                scale,
                static (x, y) => {
                    x.ContentTransform.localScale = y * Vector3.one;
                }
            );

            return modal;
        }

        #endregion
    }
}