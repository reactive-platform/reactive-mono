using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class Dummy : DrivingReactiveComponent {
        protected override void Construct(RectTransform rect) { }
    }
}