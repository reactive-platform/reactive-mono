using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Represents an object that holds a layout controller. Unlike <see cref="ILayoutDriver"/>, the holder
/// is not required to drive the layout itself, so it can expose a controller of an underlying driver.
/// </summary>
[PublicAPI]
public interface ILayoutControllerHolder {
    ILayoutController? LayoutController { get; set; }
}
