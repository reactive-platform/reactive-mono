using System;
using System.Collections.Generic;
using System.Linq;
using HMUI;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber.Components {
    [PublicAPI]
    public static class ModalAnimationExtensions {
        #region JumpAnimation

        public static T WithJumpAnimation<T>(this T wrapper, AnimationDuration? duration = null) where T : IComponentHolder<IModal> {
            var modal = wrapper.Component;
            var group = new Lazy<CanvasGroup>(() => modal.Content.GetOrAddComponent<CanvasGroup>());

            modal.OpenProgress.Attach(x => {
                    EvaluateJumpCurve(x, out var xScale, out var yScale);

                    group.Value.alpha = x;
                    modal.ContentTransform.localScale = new Vector3(xScale, yScale, 1f);
                }
            );

            return wrapper;
        }

        // Made based on the base-game curve
        private static void EvaluateJumpCurve(float t, out float x, out float y) {
            x = t <= 0.3f ?
                Mathf.Lerp(0.85f, 1.065f, t / 0.3f) :
                Mathf.Lerp(1.065f, 1f, (t - 0.3f) / 0.7f);

            y = t <= 0.47f ?
                Mathf.Lerp(0f, 0.95f, t / 0.47f) :
                Mathf.Lerp(0.95f, 1f, (t - 0.47f) / 0.53f);
        }

        #endregion

        #region AlphaAnimation

        /// <summary>
        /// Adds an alpha animation to the specified object.
        /// </summary>
        /// <param name="wrapper">A modal to animate with.</param>
        /// <param name="gameObject">An object accessor. Leave null to attach to the nearest ViewController.</param>
        /// <param name="targetAlpha">A target animation alpha.</param>
        /// <exception cref="InvalidOperationException">Throws when accessor is null and the object is not in a ViewController hierarchy.</exception>
        public static T WithAlphaAnimation<T>(this T wrapper, Lazy<GameObject>? gameObject = null, float targetAlpha = 0.2f) where T : IComponentHolder<IModal> {
            var modal = wrapper.Component;

            if (!gameObject.HasValue) {
                // We cannot access it immediately as modal is most likely
                // not in the hierarchy at this point
                var accessor = () => {
                    var controller = modal.Content.GetComponentInParent<ViewController>();

                    if (controller == null) {
                        throw new InvalidOperationException("The component either must be in a ViewController hierarchy or provide a custom object accessor");
                    }

                    return controller.gameObject;
                };

                gameObject = accessor;
            }

            var group = new Lazy<CanvasGroup>(() => gameObject.Value.Value.GetOrAddComponent<CanvasGroup>());

            wrapper.Component.OpenProgress.Attach(x => group.Value.alpha = x);

            return wrapper;
        }

        #endregion
    }
}