using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Work run by <see cref="ReactiveScheduler.PostLayout"/> after the layout has settled.
/// </summary>
[PublicAPI]
public interface IPostLayoutHandler {
    void OnPostLayout();
}
