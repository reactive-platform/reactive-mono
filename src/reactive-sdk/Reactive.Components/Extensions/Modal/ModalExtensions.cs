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

        public static T WithBeforeOpenListener<T>(this T comp, Action callback) where T : ISharedModal {
            comp.BeforeModalOpenedEvent += _ => callback();
            return comp;
        }

        public static T WithBeforeOpenListener<T>(this T comp, Action<IModal> callback) where T : ISharedModal {
            comp.BeforeModalOpenedEvent += callback;
            return comp;
        }

        #endregion

        #region Scale Animation

        public static T WithScaleAnimation<T>(
            this T modal,
            Optional<AnimationDuration> duration = default,
            AnimationCurve? curve = null
        ) where T : ModalBase {
            modal.OpenProgress.AttachLerp(
                x => modal.ContentTransform.localScale = x * Vector3.one,
                curve ?? AnimationCurve.Linear
            );

            return modal;
        }

        #endregion
    }
}