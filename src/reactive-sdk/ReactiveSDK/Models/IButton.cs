using JetBrains.Annotations;

namespace Reactive.Components;

/// <summary>
/// A button abstraction.
/// </summary>
[PublicAPI]
public interface IButton {
    void Click();
}