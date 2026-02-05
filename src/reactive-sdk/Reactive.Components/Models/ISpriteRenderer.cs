using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// Represents a sprite renderer component.
/// </summary>
[PublicAPI]
public interface ISpriteRenderer {
    Sprite Sprite { get; set; }
}