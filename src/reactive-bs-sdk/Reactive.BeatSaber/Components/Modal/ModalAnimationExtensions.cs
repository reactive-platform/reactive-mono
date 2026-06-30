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
    }
}