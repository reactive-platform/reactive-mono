using System.Collections.Generic;

namespace Reactive.Components {
    /// <summary>
    /// Abstraction for list views
    /// </summary>
    public interface IListView {
        void Refresh();
    }

    /// <summary>
    /// Abstraction for list views with value
    /// </summary>
    /// <typeparam name="LItem">Data type</typeparam>
    public interface IListView<LItem> : IListView {
        IReadOnlyList<LItem> Items { get; set; }
    }
}