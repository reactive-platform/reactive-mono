using JetBrains.Annotations;

namespace Reactive.Components;

[PublicAPI]
public interface IListCell<in TItem> {
    void Init(TItem item);
}