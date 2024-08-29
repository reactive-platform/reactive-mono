using JetBrains.Annotations;

namespace Reactive.Components;

/// <summary>
/// An abstraction for interactable canvas components.
/// </summary>
[PublicAPI]
public interface IInteractableComponent {
    bool IsPressed { get; }
    bool IsHovered { get; }
    bool Interactable { get; set; }
}