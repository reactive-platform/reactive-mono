using JetBrains.Annotations;

namespace Reactive.Components;

/// <summary>
/// An abstraction for interactable canvas components.
/// </summary>
[PublicAPI]
public interface IInteractableComponent {
    bool Interactable { get; set; }
}