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
            AnimationCurve? curve = default
        ) where T : ModalComponentBase {
            WithScaleOpenAnimation(modal, duration, curve);
            WithScaleCloseAnimation(modal, duration, curve);
            return modal;
        }
        
        public static T WithScaleOpenAnimation<T>(
            this T modal,
            Optional<AnimationDuration> duration = default,
            AnimationCurve? curve = default
        ) where T : ModalComponentBase {
            modal.OpenAnimator = ValueUtils.Animate<ModalComponentBase>(
                modal,
                static (x, y) => x.ContentTransform.localScale = y * Vector3.one,
                duration.GetValueOrDefault(0.2f),
                curve
            );
            return modal;
        }

        public static T WithScaleCloseAnimation<T>(
            this T modal,
            Optional<AnimationDuration> duration = default,
            AnimationCurve? curve = default
        ) where T : ModalComponentBase {
            modal.CloseAnimator = ValueUtils.Animate<ModalComponentBase>(
                modal,
                static (x, y) => x.ContentTransform.localScale = (1 - y) * Vector3.one,
                duration.GetValueOrDefault(0.2f),
                curve
            );
            return modal;
        }

        #endregion
    }
}