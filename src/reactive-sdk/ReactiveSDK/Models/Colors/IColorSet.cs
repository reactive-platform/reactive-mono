using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public interface IColorSet {
        event Action? SetUpdatedEvent;

        Color GetColor(GraphicState state);
    }
}