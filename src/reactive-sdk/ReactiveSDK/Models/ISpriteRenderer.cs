using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// Represents a sprite renderer component.
/// </summary>
[PublicAPI]
public interface ISpriteRenderer {
    Sprite Sprite { get; set; }
    Action<Sprite?>? OnSpriteChanged { get; set; }
}