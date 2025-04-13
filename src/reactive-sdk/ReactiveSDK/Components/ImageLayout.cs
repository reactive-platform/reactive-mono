using JetBrains.Annotations;

namespace Reactive.Components.Basic;

/// <summary>
/// A layout with an image. Leaf functions are not available here.
/// </summary>
[PublicAPI]
public class ImageLayout : ComponentLayout<Image> {
    public Image Image => Component;
}